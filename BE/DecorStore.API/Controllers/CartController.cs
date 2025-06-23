using DecorStore.API.DTOs;
using DecorStore.API.Services;
using DecorStore.API.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SESSION_ID_COOKIE = "DecorStore_CartId";

        public CartController(ICartService cartService, IHttpContextAccessor httpContextAccessor, ILogger<CartController> logger)
            : base(logger)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<CartDTO>> GetCart()
        {
            var (userId, sessionId) = GetUserIdentifiers();
            var result = await _cartService.GetCartAsync(userId, sessionId);
            return HandleResult(result);
        }        // POST: api/Cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDTO addToCartDto)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualAddToCartDto = await TryManualDeserializationAsync(addToCartDto, _logger);

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            var (userId, sessionId) = GetUserIdentifiers();
            var result = await _cartService.AddToCartAsync(userId, sessionId, actualAddToCartDto);
            if (result.IsSuccess)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        // PUT: api/Cart/items/{id}
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, UpdateCartItemDTO updateCartItemDto)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualUpdateCartItemDto = await TryManualDeserializationAsync(updateCartItemDto, _logger);

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            var (userId, sessionId) = GetUserIdentifiers();
            var result = await _cartService.UpdateCartItemAsync(userId, sessionId, id, actualUpdateCartItemDto);
            if (result.IsSuccess)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        // DELETE: api/Cart/items/{id}
        [HttpDelete("items/{id}")]
        public async Task<ActionResult<CartDTO>> RemoveCartItem(int id)
        {
            var (userId, sessionId) = GetUserIdentifiers();
            var result = await _cartService.RemoveCartItemAsync(userId, sessionId, id);
            return HandleResult(result);
        }

        // DELETE: api/Cart
        [HttpDelete]
        public async Task<ActionResult<CartDTO>> ClearCart()
        {
            var (userId, sessionId) = GetUserIdentifiers();
            var result = await _cartService.ClearCartAsync(userId, sessionId);
            return HandleResult(result);
        }

        // POST: api/Cart/merge
        [HttpPost("merge")]
        [Authorize]
        public async Task<ActionResult> MergeCarts()
        {
            var (userId, sessionId) = GetUserIdentifiers();
            
            if (!userId.HasValue || string.IsNullOrEmpty(sessionId))
            {
                return BadRequest(new { message = "User ID and session ID are required for merging carts" });
            }

            var result = await _cartService.MergeCartsAsync(userId.Value, sessionId);
            
            if (result.IsSuccess)
            {
                // Clear session ID cookie after merging
                Response.Cookies.Delete(SESSION_ID_COOKIE);
                return NoContent();
            }
            
            return HandleResult(result);
        }

        // Helper method to get user ID and session ID
        private (int? userId, string? sessionId) GetUserIdentifiers()
        {
            // Get user ID if authenticated using BaseController method
            var userIdString = GetCurrentUserId();
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int id))
            {
                userId = id;
            }

            // Get or create session ID for anonymous users
            string? sessionId = Request.Cookies[SESSION_ID_COOKIE];
            if (string.IsNullOrEmpty(sessionId) && userId == null)
            {
                sessionId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append(SESSION_ID_COOKIE, sessionId, cookieOptions);
            }

            return (userId, sessionId);
        }
    }
}
