using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShoppingBasket.Models;
using ShoppingBasket.Services;

namespace ShoppingBasket.Services
{
    public class LocalProductService : IProductService
    {
        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>
        {
            { "1", new Product { ProductId = "1", Name = "Laptop", Price = 3500.00m} },
            { "2", new Product { ProductId = "2", Name = "Smartphone", Price = 1200.00m} },
            { "3", new Product { ProductId = "3", Name = "Earpods", Price = 250.00m} },
            { "4", new Product { ProductId = "4", Name = "Monitor", Price = 800.00m} },
            { "5", new Product { ProductId = "5", Name = "Mouse", Price = 150.00m} }
        };

        public Task<Product> GetProductAsync(string productId)
        {
            _products.TryGetValue(productId, out var product);
            return Task.FromResult(product);
        }

        public Task<bool> ReserveProductAsync(string productId, string basketId, DateTime reservedUntil)
        {
            // W tej implementacji blokady są w ProductLockRepository, więc zawsze zwracamy true
            return Task.FromResult(_products.ContainsKey(productId));
        }

        public Task ReleaseProductReservationAsync(string productId, string basketId)
        {
            // W tej implementacji blokady są w ProductLockRepository
            return Task.CompletedTask;
        }
    }
}