using Microsoft.AspNetCore.Mvc;
using ShoppingBasket.Models;
using ShoppingBasket.Services;
using shoppingcart.Models;
using System.Threading.Tasks;

namespace ShoppingBasket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketsController : ControllerBase
    {
        private readonly BasketService _basketService;

        public BasketsController(BasketService basketService)
        {
            _basketService = basketService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBasket([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID jest wymagane");
            }

            await _basketService.CreateBasketAsync(userId);
            var basket = await _basketService.GetUserActiveBasketAsync(userId);
            return CreatedAtAction(nameof(GetBasket), new { basketId = basket.BasketId }, basket);
        }

        [HttpGet("{basketId}")]
        public async Task<ActionResult<BasketDto>> GetBasket(string basketId)
        {
            var basket = await _basketService.GetBasketAsync(basketId);
            if (basket == null)
            {
                return NotFound("Koszyk nie został znaleziony");
            }

            decimal totalAmount = _basketService.CalculateBasketTotal(basket);
            var basketDto = new BasketDto
            {
                BasketId = basket.BasketId,
                UserId = basket.UserId,
                Items = basket.Items,
                TotalAmount = totalAmount,
                CreatedAt = basket.CreatedAt,
                LastModified = basket.LastModified,
                IsFinalized = basket.IsFinalized
            };

            return basketDto;
        }

        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<BasketDto>> GetUserActiveBasket(string userId)
        {
            var basket = await _basketService.GetUserActiveBasketAsync(userId);
            if (basket == null)
            {
                return NotFound("Użytkownik nie ma aktywnego koszyka");
            }

            decimal totalAmount = _basketService.CalculateBasketTotal(basket);
            var basketDto = new BasketDto
            {
                BasketId = basket.BasketId,
                UserId = basket.UserId,
                Items = basket.Items,
                TotalAmount = totalAmount,
                CreatedAt = basket.CreatedAt,
                LastModified = basket.LastModified,
                IsFinalized = basket.IsFinalized
            };

            return basketDto;
        }

        [HttpPost("{basketId}/items")]
        public async Task<IActionResult> AddProductToBasket(string basketId, [FromBody] AddProductRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductId))
            {
                return BadRequest("Nieprawidłowe dane produktu");
            }

            try
            {
                // Usunięto quantity - zawsze dodajemy jeden produkt
                await _basketService.AddProductToBasketAsync(basketId, request.ProductId);
                return Ok();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{basketId}/items/{productId}")]
        public async Task<IActionResult> RemoveProductFromBasket(string basketId, string productId)
        {
            try
            {
                await _basketService.RemoveProductFromBasketAsync(basketId, productId);
                return Ok();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{basketId}/finalize")]
        public async Task<IActionResult> FinalizeBasket(string basketId)
        {
            try
            {
                await _basketService.FinalizeBasketAsync(basketId);
                return Ok();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}