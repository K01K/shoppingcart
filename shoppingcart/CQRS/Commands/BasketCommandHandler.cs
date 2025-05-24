using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ShoppingBasket.Models;
using ShoppingBasket.Repository;
using ShoppingBasket.Services;
using ShoppingBasket.CQRS.Commands;

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
        private readonly Dictionary<string, System.Timers.Timer> _basketTimers = new Dictionary<string, System.Timers.Timer>();
        private const int RESERVATION_MINUTES = 15;

        public BasketCommandHandler(IBasketRepository basketRepository, IProductService productService)
        {
            _basketRepository = basketRepository;
            _productService = productService;
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

            var reservedUntil = DateTime.UtcNow.AddMinutes(RESERVATION_MINUTES);
            var reservationSuccessful = await _productService.ReserveProductAsync(
                command.ProductId, command.BasketId, reservedUntil);

            if (!reservationSuccessful)
            {
                throw new InvalidOperationException("Nie można zarezerwować produktu");
            }

            var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == command.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += command.Quantity;
                existingItem.ReservedUntil = reservedUntil;
            }
            else
            {
                basket.Items.Add(new BasketItem
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = command.Quantity,
                    ReservedUntil = reservedUntil
                });
            }

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
                await _productService.ReleaseProductReservationAsync(command.ProductId, command.BasketId);
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
                foreach (var item in basket.Items)
                {
                    await _productService.ReleaseProductReservationAsync(item.ProductId, basketId);
                }
                await _basketRepository.DeleteBasketAsync(basketId);
                RemoveBasketExpiryTimer(basketId);
            }
        }
    }
}