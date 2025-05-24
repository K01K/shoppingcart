using System.Threading.Tasks;
using ShoppingBasket.Models;

namespace ShoppingBasket.Repository
{
    public interface IBasketRepository
    {
        Task<Basket> GetBasketAsync(string basketId);
        Task<Basket> GetUserActiveBasketAsync(string userId);
        Task SaveBasketAsync(Basket basket);
        Task DeleteBasketAsync(string basketId);
    }
}