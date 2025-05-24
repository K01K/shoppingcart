using ShoppingBasket.Models;

namespace ShoppingBasket.CQRS.Queries
{
    public class GetUserActiveBasketQuery : IQuery<Basket>
    {
        public string UserId { get; set; }
    }
}