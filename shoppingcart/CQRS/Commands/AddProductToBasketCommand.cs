using ShoppingBasket.CQRS.Commands;

namespace ShoppingBasket.CQRS.Commands
{
    public class AddProductToBasketCommand : ICommand
    {
        public string BasketId { get; set; }
        public string ProductId { get; set; }
    }
}