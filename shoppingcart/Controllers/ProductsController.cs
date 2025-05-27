using Microsoft.AspNetCore.Mvc;
using ShoppingBasket.Models;
using System.Collections.Generic;

namespace ShoppingBasket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>
        {
            { "1", new Product { ProductId = "1", Name = "Laptop", Price = 3500.00m} },
            { "2", new Product { ProductId = "2", Name = "Smartphone", Price = 1200.00m} },
            { "3", new Product { ProductId = "3", Name = "Earpods", Price = 250.00m} },
            { "4", new Product { ProductId = "4", Name = "Monitor", Price = 800.00m} },
            { "5", new Product { ProductId = "5", Name = "Mouse", Price = 150.00m} }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            return Ok(_products.Values);
        }

        [HttpGet("{productId}")]
        public ActionResult<Product> GetProduct(string productId)
        {
            if (!_products.TryGetValue(productId, out var product))
            {
                return NotFound("Produkt nie został znaleziony");
            }

            return Ok(product);
        }

        // USUNIĘTO niepotrzebne endpointy rezerwacji - blokowanie odbywa się automatycznie w domenie koszyka
    }
}