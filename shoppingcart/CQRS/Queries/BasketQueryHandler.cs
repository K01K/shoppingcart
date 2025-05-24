using System.Threading.Tasks;
using ShoppingBasket.Models;
using ShoppingBasket.Repository;

namespace ShoppingBasket.CQRS.Queries
{
    public class BasketQueryHandler :
        IQueryHandler<GetBasketQuery, Basket>,
        IQueryHandler<GetUserActiveBasketQuery, Basket>
    {
        private readonly IBasketRepository _basketRepository;

        public BasketQueryHandler(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }

        public Task<Basket> HandleAsync(GetBasketQuery query)
        {
            return _basketRepository.GetBasketAsync(query.BasketId);
        }

        public Task<Basket> HandleAsync(GetUserActiveBasketQuery query)
        {
            return _basketRepository.GetUserActiveBasketAsync(query.UserId);
        }
    }
}