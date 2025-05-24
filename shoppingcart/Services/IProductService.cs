using System;
using System.Threading.Tasks;
using ShoppingBasket.Models;

namespace ShoppingBasket.Services
{
    public interface IProductService
    {
        Task<Product> GetProductAsync(string productId);
        Task<bool> ReserveProductAsync(string productId, string basketId, DateTime reservedUntil);
        Task ReleaseProductReservationAsync(string productId, string basketId);
    }
}