namespace ShoppingBasket.CQRS.Commands
{
    public class FinalizeBasketCommand : ICommand
    {
        public string BasketId { get; set; }
    }
}