using System;
using System.Threading.Tasks;
using ShoppingBasket.Models;

namespace ShoppingBasket.Services
{
    public interface IProductService
    {
        Task<Product> GetProductAsync(string productId);
    }
}