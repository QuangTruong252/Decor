using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Common;
using DecorStore.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;

namespace DecorStore.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<Result<AuthResponseDTO>> RegisterAsync(RegisterDTO registerDto)
        {
            if (registerDto == null)
            {
                return Result<AuthResponseDTO>.Failure("Registration data cannot be null", "INVALID_INPUT");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Email))
            {
                return Result<AuthResponseDTO>.Failure("Email is required", "INVALID_EMAIL");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Username))
            {
                return Result<AuthResponseDTO>.Failure("Username is required", "INVALID_USERNAME");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Password) || registerDto.Password.Length < 6)
            {
                return Result<AuthResponseDTO>.Failure("Password must be at least 6 characters long", "INVALID_PASSWORD");
            }

            if (!IsValidEmail(registerDto.Email))
            {
                return Result<AuthResponseDTO>.Failure("Invalid email format", "INVALID_EMAIL_FORMAT");
            }

            try
            {
                return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    // Check if user already exists
                    var existingUser = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
                    if (existingUser != null)
                    {
                        return Result<AuthResponseDTO>.Failure("User with this email already exists", "USER_ALREADY_EXISTS");
                    }

                    // Check if username already exists
                    var existingUsername = await _unitOfWork.Users.GetByUsernameAsync(registerDto.Username);
                    if (existingUsername != null)
                    {
                        return Result<AuthResponseDTO>.Failure("Username is already taken", "USERNAME_TAKEN");
                    }

                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Create new user
                        var user = new User
                        {
                            Username = registerDto.Username,
                            Email = registerDto.Email.ToLowerInvariant(),
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Role = "User" // Default role
                        };

                        await _unitOfWork.Users.CreateAsync(user);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        // Generate JWT token
                        var token = GenerateJwtToken(user);
                        var userDto = _mapper.Map<UserDTO>(user);

                        var authResponse = new AuthResponseDTO
                        {
                            Token = token,
                            User = userDto
                        };

                        return Result<AuthResponseDTO>.Success(authResponse);
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<AuthResponseDTO>.Failure($"Failed to register user: {ex.Message}", "REGISTRATION_ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result<AuthResponseDTO>.Failure($"Execution strategy failed: {ex.Message}", "EXECUTION_STRATEGY_ERROR");
            }
        }

        public async Task<Result<AuthResponseDTO>> LoginAsync(LoginDTO loginDto)
        {
            if (loginDto == null)
            {
                return Result<AuthResponseDTO>.Failure("Login data cannot be null", "INVALID_INPUT");
            }

            if (string.IsNullOrWhiteSpace(loginDto.Email))
            {
                return Result<AuthResponseDTO>.Failure("Email is required", "INVALID_EMAIL");
            }

            if (string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return Result<AuthResponseDTO>.Failure("Password is required", "INVALID_PASSWORD");
            }

            try
            {
                // Find user by email
                var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email.ToLowerInvariant());

                // Check if user exists and password is correct
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return Result<AuthResponseDTO>.Failure("Invalid email or password", "INVALID_CREDENTIALS");
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDTO>(user);

                var authResponse = new AuthResponseDTO
                {
                    Token = token,
                    User = userDto
                };

                return Result<AuthResponseDTO>.Success(authResponse);
            }
            catch (Exception ex)
            {
                return Result<AuthResponseDTO>.Failure($"Error during login: {ex.Message}", "LOGIN_ERROR");
            }
        }

        public async Task<Result<UserDTO>> GetUserByIdAsync(int id)
        {
            if (id <= 0)
            {
                return Result<UserDTO>.Failure("User ID must be a positive integer", "INVALID_USER_ID");
            }

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return Result<UserDTO>.Failure($"User with ID {id} not found", "USER_NOT_FOUND");
                }

                var userDto = _mapper.Map<UserDTO>(user);
                return Result<UserDTO>.Success(userDto);
            }
            catch (Exception ex)
            {
                return Result<UserDTO>.Failure($"Error retrieving user {id}: {ex.Message}", "RETRIEVE_USER_ERROR");
            }
        }

        public async Task<Result<UserDTO>> MakeAdminAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<UserDTO>.Failure("Email is required", "INVALID_EMAIL");
            }

            if (!IsValidEmail(email))
            {
                return Result<UserDTO>.Failure("Invalid email format", "INVALID_EMAIL_FORMAT");
            }

            try
            {
                return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    var user = await _unitOfWork.Users.GetByEmailAsync(email.ToLowerInvariant());

                    if (user == null)
                    {
                        return Result<UserDTO>.Failure($"User with email {email} not found", "USER_NOT_FOUND");
                    }

                    if (user.Role == "Admin")
                    {
                        return Result<UserDTO>.Failure("User is already an admin", "ALREADY_ADMIN");
                    }

                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        user.Role = "Admin";
                        user.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.Users.UpdateAsync(user);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        var userDto = _mapper.Map<UserDTO>(user);
                        return Result<UserDTO>.Success(userDto);
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<UserDTO>.Failure($"Failed to make user admin: {ex.Message}", "MAKE_ADMIN_ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result<UserDTO>.Failure($"Execution strategy failed: {ex.Message}", "EXECUTION_STRATEGY_ERROR");
            }
        }

        public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDto)
        {
            if (userId <= 0)
            {
                return Result.Failure("User ID must be a positive integer", "INVALID_USER_ID");
            }

            if (changePasswordDto == null)
            {
                return Result.Failure("Change password data cannot be null", "INVALID_INPUT");
            }

            if (string.IsNullOrWhiteSpace(changePasswordDto.CurrentPassword))
            {
                return Result.Failure("Current password is required", "INVALID_CURRENT_PASSWORD");
            }

            if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword) || changePasswordDto.NewPassword.Length < 6)
            {
                return Result.Failure("New password must be at least 6 characters long", "INVALID_NEW_PASSWORD");
            }

            if (changePasswordDto.CurrentPassword == changePasswordDto.NewPassword)
            {
                return Result.Failure("New password must be different from current password", "SAME_PASSWORD");
            }

            try
            {
                return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user == null)
                    {
                        return Result.Failure($"User with ID {userId} not found", "USER_NOT_FOUND");
                    }

                    // Verify current password
                    if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                    {
                        return Result.Failure("Current password is incorrect", "INCORRECT_PASSWORD");
                    }

                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Update password
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                        user.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.Users.UpdateAsync(user);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        return Result.Success();
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result.Failure($"Failed to change password: {ex.Message}", "CHANGE_PASSWORD_ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Execution strategy failed: {ex.Message}", "EXECUTION_STRATEGY_ERROR");
            }
        }

        public async Task<Result<AuthResponseDTO>> RefreshTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Result<AuthResponseDTO>.Failure("Token is required", "INVALID_TOKEN");
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSettings = _configuration.GetSection("JWT");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured");
                var key = Encoding.ASCII.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = false, // Don't validate expiry for refresh
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Result<AuthResponseDTO>.Failure("Invalid token claims", "INVALID_TOKEN_CLAIMS");
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<AuthResponseDTO>.Failure("User not found", "USER_NOT_FOUND");
                }

                // Generate new token
                var newToken = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDTO>(user);

                var authResponse = new AuthResponseDTO
                {
                    Token = newToken,
                    User = userDto
                };

                return Result<AuthResponseDTO>.Success(authResponse);
            }
            catch (SecurityTokenException)
            {
                return Result<AuthResponseDTO>.Failure("Invalid token", "INVALID_TOKEN");
            }
            catch (Exception ex)
            {
                return Result<AuthResponseDTO>.Failure($"Error refreshing token: {ex.Message}", "REFRESH_TOKEN_ERROR");
            }
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

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
