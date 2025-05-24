using System.Linq;
using System.Threading.Tasks;
using ShoppingBasket.CQRS.Commands;
using ShoppingBasket.CQRS.Queries;
using ShoppingBasket.Models;
using ShoppingBasket.Repository;

namespace ShoppingBasket.Services
{
    public class BasketService
    {
        private readonly BasketCommandHandler _commandHandler;
        private readonly BasketQueryHandler _queryHandler;

        public BasketService(IBasketRepository basketRepository, IProductService productService)
        {
            _commandHandler = new BasketCommandHandler(basketRepository, productService);
            _queryHandler = new BasketQueryHandler(basketRepository);
        }

        public Task CreateBasketAsync(string userId)
        {
            return _commandHandler.HandleAsync(new CreateBasketCommand { UserId = userId });
        }

        public Task AddProductToBasketAsync(string basketId, string productId, int quantity)
        {
            return _commandHandler.HandleAsync(new AddProductToBasketCommand
            {
                BasketId = basketId,
                ProductId = productId,
                Quantity = quantity
            });
        }

        public Task RemoveProductFromBasketAsync(string basketId, string productId)
        {
            return _commandHandler.HandleAsync(new RemoveProductFromBasketCommand
            {
                BasketId = basketId,
                ProductId = productId
            });
        }

        public Task FinalizeBasketAsync(string basketId)
        {
            return _commandHandler.HandleAsync(new FinalizeBasketCommand { BasketId = basketId });
        }

        public Task<Basket> GetBasketAsync(string basketId)
        {
            return _queryHandler.HandleAsync(new GetBasketQuery { BasketId = basketId });
        }

        public Task<Basket> GetUserActiveBasketAsync(string userId)
        {
            return _queryHandler.HandleAsync(new GetUserActiveBasketQuery { UserId = userId });
        }

        public decimal CalculateBasketTotal(Basket basket)
        {
            return basket?.Items?.Sum(item => item.Price * item.Quantity) ?? 0;
        }
    }
}