using System;

namespace ShoppingBasket.Models
{
    public class BasketItem
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime ReservedUntil { get; set; }
    }
}