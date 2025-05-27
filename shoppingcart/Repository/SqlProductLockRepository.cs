using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShoppingBasket.Repository;
using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace ShoppingBasket.Repository
{
    public class SqlProductLockRepository : IProductLockRepository
    {
        private readonly string _connectionString;

        public SqlProductLockRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> TryLockProductAsync(string productId, string basketId, string userId, DateTime expiresAt)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        var existingLock = await db.QueryFirstOrDefaultAsync<ProductLock>(
                            @"SELECT * FROM ProductLocks 
                                  WHERE ProductId = @ProductId 
                                  AND IsActive = 1 
                                  AND ExpiresAt > GETUTCDATE()",
                            new { ProductId = productId },
                            transaction);

                        if (existingLock != null && existingLock.BasketId != basketId)
                        {
                            return false;
                        }

                        if (existingLock != null && existingLock.BasketId == basketId)
                        {
                            await db.ExecuteAsync(
                                @"UPDATE ProductLocks 
                                      SET ExpiresAt = @ExpiresAt, LockedAt = GETUTCDATE() 
                                      WHERE ProductId = @ProductId AND BasketId = @BasketId",
                                new { ProductId = productId, BasketId = basketId, ExpiresAt = expiresAt },
                                transaction);
                        }
                        else
                        {
                            await db.ExecuteAsync(
                                @"INSERT INTO ProductLocks (ProductId, BasketId, UserId, LockedAt, ExpiresAt, IsActive) 
                                      VALUES (@ProductId, @BasketId, @UserId, GETUTCDATE(), @ExpiresAt, 1)",
                                new { ProductId = productId, BasketId = basketId, UserId = userId, ExpiresAt = expiresAt },
                                transaction);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> IsProductLockedAsync(string productId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var lockExists = await db.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(1) FROM ProductLocks 
                          WHERE ProductId = @ProductId 
                          AND IsActive = 1 
                          AND ExpiresAt > GETUTCDATE()",
                    new { ProductId = productId });

                return lockExists > 0;
            }
        }

        public async Task<ProductLock> GetActiveLockAsync(string productId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<ProductLock>(
                    @"SELECT * FROM ProductLocks 
                          WHERE ProductId = @ProductId 
                          AND IsActive = 1 
                          AND ExpiresAt > GETUTCDATE()",
                    new { ProductId = productId });
            }
        }

        public async Task ReleaseLockAsync(string productId, string basketId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(
                    @"UPDATE ProductLocks 
                          SET IsActive = 0 
                          WHERE ProductId = @ProductId AND BasketId = @BasketId",
                    new { ProductId = productId, BasketId = basketId });
            }
        }

        public async Task ReleaseAllLocksForBasketAsync(string basketId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(
                    @"UPDATE ProductLocks 
                          SET IsActive = 0 
                          WHERE BasketId = @BasketId",
                    new { BasketId = basketId });
            }
        }

        public async Task CleanupExpiredLocksAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(
                    @"UPDATE ProductLocks 
                          SET IsActive = 0 
                          WHERE ExpiresAt <= GETUTCDATE() AND IsActive = 1");
            }
        }
    }
}