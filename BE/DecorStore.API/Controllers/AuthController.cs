using System.Threading.Tasks;
using System.Linq;
using DecorStore.API.DTOs;
using DecorStore.API.Services;
using DecorStore.API.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger) 
            : base(logger)
        {
            _authService = authService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO registerDto)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var registerResult = await _authService.RegisterAsync(registerDto);
            return HandleCreateResult(registerResult);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDto)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var loginResult = await _authService.LoginAsync(loginDto);
            return HandleResult(loginResult);
        }

        // GET: api/Auth/user
        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user authentication");
            }

            var userResult = await _authService.GetUserByIdAsync(userId);
            return HandleResult(userResult);
        }

        // GET: api/Auth/check-claims
        [HttpGet("check-claims")]
        [Authorize]
        public ActionResult<object> CheckClaims()
        {
            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            var isAdmin = User.IsInRole("Admin");

            return Ok(new {
                Claims = claims,
                IsAdmin = isAdmin,
                NameIdentifier = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }

        // POST: api/Auth/make-admin
        [HttpPost("make-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> MakeAdmin([FromBody] MakeAdminRequest request)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            if (string.IsNullOrWhiteSpace(request?.Email))
            {
                return BadRequest("Email is required");
            }

            var makeAdminResult = await _authService.MakeAdminAsync(request.Email);
            return HandleResult(makeAdminResult);
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            // Get user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user authentication");
            }

            var changePasswordResult = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            return HandleResult(changePasswordResult);
        }

        // POST: api/Auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDTO>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
            {
                return BadRequest("Token is required");
            }

            var refreshResult = await _authService.RefreshTokenAsync(request.Token);
            return HandleResult(refreshResult);
        }
    }

    // Helper classes for request bodies
    public class MakeAdminRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
