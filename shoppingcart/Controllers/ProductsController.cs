using Microsoft.AspNetCore.Mvc;
using ShoppingBasket.Models;
using System;
using System.Collections.Generic;

namespace ShoppingBasket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>
        {
            { "1", new Product { ProductId = "1", Name = "Laptop", Price = 3500.00m, Quantity =2 } },
            { "2", new Product { ProductId = "2", Name = "Smartphone", Price = 1200.00m, Quantity =2} },
            { "3", new Product { ProductId = "3", Name = "Earpods", Price = 250.00m, Quantity =2} },
            { "4", new Product { ProductId = "4", Name = "Monitor", Price = 800.00m, Quantity =2} },
            { "5", new Product { ProductId = "5", Name = "Mouse", Price = 150.00m, Quantity =2} }
        };

        private readonly Dictionary<string, Dictionary<string, DateTime>> _reservations = new Dictionary<string, Dictionary<string, DateTime>>();

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

        [HttpPost("{productId}/reserve")]
        public ActionResult ReserveProduct(string productId, [FromBody] ReservationRequest request)
        {
            if (!_products.ContainsKey(productId))
            {
                return NotFound("Produkt nie został znaleziony");
            }

            lock (_reservations)
            {
                if (_reservations.TryGetValue(productId, out var basketReservations))
                {
                    foreach (var kvp in basketReservations)
                    {
                        if (kvp.Key != request.BasketId && kvp.Value > DateTime.UtcNow)
                        {
                            return BadRequest("Produkt jest już zarezerwowany");
                        }
                    }

                    basketReservations[request.BasketId] = request.ReservedUntil;
                }
                else
                {
                    _reservations[productId] = new Dictionary<string, DateTime>
                    {
                        { request.BasketId, request.ReservedUntil }
                    };
                }
            }

            return Ok();
        }

        [HttpDelete("{productId}/reserve/{basketId}")]
        public ActionResult ReleaseProductReservation(string productId, string basketId)
        {
            if (!_products.ContainsKey(productId))
            {
                return NotFound("Produkt nie został znaleziony");
            }

            lock (_reservations)
            {
                if (_reservations.TryGetValue(productId, out var basketReservations))
                {
                    basketReservations.Remove(basketId);
                    if (basketReservations.Count == 0)
                    {
                        _reservations.Remove(productId);
                    }
                }
            }

            return Ok();
        }
    }

    public class ReservationRequest
    {
        public string BasketId { get; set; }
        public DateTime ReservedUntil { get; set; }
    }
}