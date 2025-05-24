using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ShoppingBasket.Models;
using ShoppingBasket.Repository;
using Dapper;

namespace ShoppingBasket.Repository
{
    public class SqlBasketRepository : IBasketRepository
    {
        private readonly string _connectionString;

        public SqlBasketRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Basket> GetBasketAsync(string basketId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var basket = await db.QueryFirstOrDefaultAsync<Basket>(
                    "SELECT * FROM Baskets WHERE Id = @Id",
                    new { Id = basketId });

                if (basket != null)
                {
                    basket.Items = new List<BasketItem>();
                    var items = await db.QueryAsync<BasketItem>(
                        "SELECT * FROM BasketItems WHERE BasketId = @BasketId",
                        new { BasketId = basketId });
                    basket.Items.AddRange(items);
                }

                return basket;
            }
        }

        public async Task<Basket> GetUserActiveBasketAsync(string userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var basket = await db.QueryFirstOrDefaultAsync<Basket>(
                    "SELECT * FROM Baskets WHERE UserId = @UserId AND IsFinalized = 0",
                    new { UserId = userId });

                if (basket != null)
                {
                    basket.Items = new List<BasketItem>();
                    var items = await db.QueryAsync<BasketItem>(
                        "SELECT * FROM BasketItems WHERE BasketId = @BasketId",
                        new { BasketId = basket.BasketId });
                    basket.Items.AddRange(items);
                }

                return basket;
            }
        }

        public async Task SaveBasketAsync(Basket basket)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        var exists = await db.ExecuteScalarAsync<int>(
                            "SELECT COUNT(1) FROM Baskets WHERE Id = @Id",
                            new { Id = basket.BasketId },
                            transaction);

                        if (exists > 0)
                        {
                            await db.ExecuteAsync(
                                "UPDATE Baskets SET UserId = @UserId, CreatedAt = @CreatedAt, LastModified = @LastModified, IsFinalized = @IsFinalized WHERE Id = @Id",
                                basket,
                                transaction);

                            // Delete all items to replace with new ones
                            await db.ExecuteAsync(
                                "DELETE FROM BasketItems WHERE BasketId = @BasketId",
                                new { BasketId = basket.BasketId },
                                transaction);
                        }
                        else
                        {
                            await db.ExecuteAsync(
                                "INSERT INTO Baskets (Id, UserId, CreatedAt, LastModified, IsFinalized) VALUES (@Id, @UserId, @CreatedAt, @LastModified, @IsFinalized)",
                                basket,
                                transaction);
                        }

                        if (basket.Items != null && basket.Items.Count > 0)
                        {
                            foreach (var item in basket.Items)
                            {
                                await db.ExecuteAsync(
                                    "INSERT INTO BasketItems (BasketId, ProductId, Name, Price, Quantity, ReservedUntil) VALUES (@BasketId, @ProductId, @Name, @Price, @Quantity, @ReservedUntil)",
                                    new
                                    {
                                        BasketId = basket.BasketId,
                                        item.ProductId,
                                        item.Name,
                                        item.Price,
                                        item.Quantity,
                                        item.ReservedUntil
                                    },
                                    transaction);
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task DeleteBasketAsync(string basketId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(
                    "DELETE FROM BasketItems WHERE BasketId = @Id;" +
                    "DELETE FROM Baskets WHERE Id = @Id",
                    new { Id = basketId });
            }
        }
    }
}