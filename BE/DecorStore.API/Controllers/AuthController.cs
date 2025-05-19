using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
            {
                return BadRequest("Email is already in use");
            }

            return Ok(result);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(result);
        }

        // GET: api/Auth/user
        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            // Get user ID from claims
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
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
        public async Task<ActionResult<UserDTO>> MakeAdmin(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }

            var user = await _authService.MakeAdminAsync(email);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
    }
}