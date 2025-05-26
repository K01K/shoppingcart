
using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace ShoppingBasket.Repository
{
    public interface IProductLockRepository
    {
        Task<bool> TryLockProductAsync(string productId, string basketId, string userId, DateTime expiresAt);
        Task<bool> IsProductLockedAsync(string productId);
        Task ReleaseLockAsync(string productId, string basketId);
        Task ReleaseAllLocksForBasketAsync(string basketId);
        Task CleanupExpiredLocksAsync();
        Task<ProductLock> GetActiveLockAsync(string productId);
    }
}