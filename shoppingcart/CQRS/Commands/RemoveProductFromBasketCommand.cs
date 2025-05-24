namespace ShoppingBasket.CQRS.Commands
{
    public class RemoveProductFromBasketCommand : ICommand
    {
        public string BasketId { get; set; }
        public string ProductId { get; set; }
    }
}