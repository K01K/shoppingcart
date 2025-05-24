using System;
using System.Collections.Generic;

namespace ShoppingBasket.Models
{
    public class Basket
    {
        public string BasketId { get; set; }
        public string UserId { get; set; }
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsFinalized { get; set; }
    }
}