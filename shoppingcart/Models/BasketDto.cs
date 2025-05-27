namespace shoppingcart.Models
{
    using ShoppingBasket.Models;
    using System;
    using System.Collections.Generic;

    public class BasketDto
    {
        public string BasketId { get; set; }
        public string UserId { get; set; }
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsFinalized { get; set; }
    }

    public class AddProductRequest
    {
        public string ProductId { get; set; }
    }
}