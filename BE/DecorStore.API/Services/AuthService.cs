using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DecorStore.API.Services
{
    public class AuthService(ApplicationDbContext context, IConfiguration configuration) : IAuthService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
        {
            try
            {
                // Check if database connection is successful
                await _context.Database.CanConnectAsync();

                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return null; // User already exists
                }

                // Create new user
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Role = "User" // Default role
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(user);

                return new AuthResponseDTO
                {
                    Token = token,
                    User = MapUserToDto(user)
                };
            }
            catch (Npgsql.NpgsqlException ex)
            {
                // Handle PostgreSQL connection error
                Console.WriteLine($"PostgreSQL Connection Error: {ex.Message}");
                throw new ApplicationException("Cannot connect to the database. Please try again later.", ex);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Error in RegisterAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // Check if user exists and password is correct
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                User = MapUserToDto(user)
            };
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user != null ? MapUserToDto(user) : null;
        }

        // Helper method to generate JWT token
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JWT");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured");
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"] ?? "60")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Helper method to map User entity to UserDTO
        private UserDTO MapUserToDto(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}