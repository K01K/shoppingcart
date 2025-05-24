namespace ShoppingBasket.CQRS.Commands
{
    public class CreateBasketCommand : ICommand
    {
        public string UserId { get; set; }
    }
}