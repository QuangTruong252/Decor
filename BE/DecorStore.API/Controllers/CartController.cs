using DecorStore.API.DTOs;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SESSION_ID_COOKIE = "DecorStore_CartId";

        public CartController(ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<CartDTO>> GetCart()
        {
            try
            {
                var (userId, sessionId) = GetUserIdentifiers();
                var cart = await _cartService.GetCartAsync(userId, sessionId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: api/Cart
        [HttpPost]
        public async Task<ActionResult<CartDTO>> AddToCart(AddToCartDTO addToCartDto)
        {
            try
            {
                var (userId, sessionId) = GetUserIdentifiers();
                var cart = await _cartService.AddToCartAsync(userId, sessionId, addToCartDto);
                return Ok(cart);
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT: api/Cart/items/{id}
        [HttpPut("items/{id}")]
        public async Task<ActionResult<CartDTO>> UpdateCartItem(int id, UpdateCartItemDTO updateCartItemDto)
        {
            try
            {
                var (userId, sessionId) = GetUserIdentifiers();
                var cart = await _cartService.UpdateCartItemAsync(userId, sessionId, id, updateCartItemDto);
                return Ok(cart);
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DecorStore.API.Exceptions.UnauthorizedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/Cart/items/{id}
        [HttpDelete("items/{id}")]
        public async Task<ActionResult<CartDTO>> RemoveCartItem(int id)
        {
            try
            {
                var (userId, sessionId) = GetUserIdentifiers();
                var cart = await _cartService.RemoveCartItemAsync(userId, sessionId, id);
                return Ok(cart);
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DecorStore.API.Exceptions.UnauthorizedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/Cart
        [HttpDelete]
        public async Task<ActionResult<CartDTO>> ClearCart()
        {
            try
            {
                var (userId, sessionId) = GetUserIdentifiers();
                var cart = await _cartService.ClearCartAsync(userId, sessionId);
                return Ok(cart);
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: api/Cart/merge
        [HttpPost("merge")]
        [Authorize]
        public async Task<ActionResult> MergeCarts()
        {
            try
            {
                var (userId, sessionId) = GetUserIdentifiers();
                
                if (!userId.HasValue || string.IsNullOrEmpty(sessionId))
                {
                    return BadRequest(new { message = "User ID and session ID are required for merging carts" });
                }

                await _cartService.MergeCartsAsync(userId.Value, sessionId);
                
                // Clear session ID cookie after merging
                Response.Cookies.Delete(SESSION_ID_COOKIE);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Helper method to get user ID and session ID
        private (int? userId, string? sessionId) GetUserIdentifiers()
        {
            // Get user ID if authenticated
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
                {
                    userId = id;
                }
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
