using ShoppingBasket.Models;

namespace ShoppingBasket.CQRS.Queries
{
    public class GetBasketQuery : IQuery<Basket>
    {
        public string BasketId { get; set; }
    }
}