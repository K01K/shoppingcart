using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ShoppingBasket.Models;
using ShoppingBasket.Repository;
using ShoppingBasket.Services;
using ShoppingBasket.CQRS.Commands;
using System.Collections.Generic;

namespace ShoppingBasket.CQRS.Commands
{
    public class BasketCommandHandler :
        ICommandHandler<CreateBasketCommand>,
        ICommandHandler<AddProductToBasketCommand>,
        ICommandHandler<RemoveProductFromBasketCommand>,
        ICommandHandler<FinalizeBasketCommand>
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IProductService _productService;
        private readonly IProductLockRepository _productLockRepository;
        private readonly Dictionary<string, System.Timers.Timer> _basketTimers = new Dictionary<string, System.Timers.Timer>();
        private const int RESERVATION_MINUTES = 15;

        public BasketCommandHandler(
            IBasketRepository basketRepository,
            IProductService productService,
            IProductLockRepository productLockRepository)
        {
            _basketRepository = basketRepository;
            _productService = productService;
            _productLockRepository = productLockRepository;
        }

        public async Task HandleAsync(CreateBasketCommand command)
        {
            var existingBasket = await _basketRepository.GetUserActiveBasketAsync(command.UserId);
            if (existingBasket != null)
            {
                return;
            }

            var basket = new Basket
            {
                BasketId = Guid.NewGuid().ToString(),
                UserId = command.UserId,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                IsFinalized = false
            };

            await _basketRepository.SaveBasketAsync(basket);
            SetupBasketExpiryTimer(basket.BasketId);
        }

        public async Task HandleAsync(AddProductToBasketCommand command)
        {
            var basket = await _basketRepository.GetBasketAsync(command.BasketId);
            if (basket == null || basket.IsFinalized)
            {
                throw new InvalidOperationException("Koszyk nie istnieje lub został już sfinalizowany");
            }

            var product = await _productService.GetProductAsync(command.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Produkt nie istnieje");
            }

            // Sprawdź czy produkt już jest w koszyku
            var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == command.ProductId);
            if (existingItem != null)
            {
                throw new InvalidOperationException("Produkt już znajduje się w koszyku");
            }

            var reservedUntil = DateTime.UtcNow.AddMinutes(RESERVATION_MINUTES);

            // AUTOMATYCZNE BLOKOWANIE PRODUKTU podczas dodawania do koszyka
            var lockSuccessful = await _productLockRepository.TryLockProductAsync(
                command.ProductId, command.BasketId, basket.UserId, reservedUntil);

            if (!lockSuccessful)
            {
                throw new InvalidOperationException("Produkt jest obecnie zarezerwowany przez innego użytkownika");
            }

            // Dodaj produkt do koszyka
            basket.Items.Add(new BasketItem
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                ReservedUntil = reservedUntil
            });

            basket.LastModified = DateTime.UtcNow;
            await _basketRepository.SaveBasketAsync(basket);
            SetupBasketExpiryTimer(basket.BasketId);
        }

        public async Task HandleAsync(RemoveProductFromBasketCommand command)
        {
            var basket = await _basketRepository.GetBasketAsync(command.BasketId);
            if (basket == null || basket.IsFinalized)
            {
                throw new InvalidOperationException("Koszyk nie istnieje lub został już sfinalizowany");
            }

            var item = basket.Items.FirstOrDefault(i => i.ProductId == command.ProductId);
            if (item != null)
            {
                basket.Items.Remove(item);
                basket.LastModified = DateTime.UtcNow;
                await _basketRepository.SaveBasketAsync(basket);

                // Zwolnij blokadę produktu
                await _productLockRepository.ReleaseLockAsync(command.ProductId, command.BasketId);

                SetupBasketExpiryTimer(basket.BasketId);
            }
        }

        public async Task HandleAsync(FinalizeBasketCommand command)
        {
            var basket = await _basketRepository.GetBasketAsync(command.BasketId);
            if (basket == null)
            {
                throw new InvalidOperationException("Koszyk nie istnieje");
            }

            if (basket.IsFinalized)
            {
                throw new InvalidOperationException("Koszyk został już sfinalizowany");
            }

            if (!basket.Items.Any())
            {
                throw new InvalidOperationException("Nie można sfinalizować pustego koszyka");
            }

            basket.IsFinalized = true;
            basket.LastModified = DateTime.UtcNow;
            await _basketRepository.SaveBasketAsync(basket);

            // Zwolnij wszystkie blokady dla tego koszyka
            await _productLockRepository.ReleaseAllLocksForBasketAsync(basket.BasketId);

            RemoveBasketExpiryTimer(basket.BasketId);
        }

        private void SetupBasketExpiryTimer(string basketId)
        {
            RemoveBasketExpiryTimer(basketId);

            var timer = new System.Timers.Timer(RESERVATION_MINUTES * 60 * 1000);
            timer.Elapsed += async (sender, e) => await HandleBasketExpiry(basketId);
            timer.AutoReset = false;
            timer.Start();

            lock (_basketTimers)
            {
                _basketTimers[basketId] = timer;
            }
        }

        private void RemoveBasketExpiryTimer(string basketId)
        {
            lock (_basketTimers)
            {
                if (_basketTimers.TryGetValue(basketId, out var timer))
                {
                    timer.Stop();
                    timer.Dispose();
                    _basketTimers.Remove(basketId);
                }
            }
        }

        private async Task HandleBasketExpiry(string basketId)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket != null && !basket.IsFinalized)
            {
                // Zwolnij wszystkie blokady produktów w koszyku
                await _productLockRepository.ReleaseAllLocksForBasketAsync(basketId);

                // Usuń koszyk
                await _basketRepository.DeleteBasketAsync(basketId);
                RemoveBasketExpiryTimer(basketId);
            }
        }
    }
}