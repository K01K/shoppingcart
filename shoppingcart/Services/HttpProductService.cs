using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ShoppingBasket.Models;

namespace ShoppingBasket.Services
{
    public class HttpProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public HttpProductService(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task<Product> GetProductAsync(string productId)
        {
            var response = await _httpClient.GetAsync($"/api/products/{productId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Product>(content);
        }

        public async Task<bool> ReserveProductAsync(string productId, string basketId, DateTime reservedUntil)
        {
            var request = new
            {
                BasketId = basketId,
                ReservedUntil = reservedUntil
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"/api/products/{productId}/reserve", content);
            return response.IsSuccessStatusCode;
        }

        public async Task ReleaseProductReservationAsync(string productId, string basketId)
        {
            await _httpClient.DeleteAsync($"/api/products/{productId}/reserve/{basketId}");
        }
    }
}