﻿using System.Text.Json;

namespace ShoppingBasket.Services
{
    public class HttpProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public HttpProductService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["ProductServiceUrl"]);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<Product> GetProductAsync(string productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var productDto = JsonSerializer.Deserialize<ProductDto>(content, _jsonOptions);

                return new Product
                {
                    ProductId = productDto.ProductId,
                    Name = productDto.Name,
                    Price = productDto.Price,
                    IsAvailable = productDto.IsAvailable
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        private class ProductDto
        {
            public string ProductId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public bool IsAvailable { get; set; }
        }
    }
}