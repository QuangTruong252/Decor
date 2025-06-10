using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DecorStore.API.Common;
using DecorStore.API.Configuration;
using DecorStore.API.Models;
using DecorStore.API.Interfaces;
using DecorStore.API.DTOs;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DecorStore.API.Interfaces.Services;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Enhanced JWT token service with rotation, encryption, and blacklisting support
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly JwtSecuritySettings _securitySettings;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly byte[] _encryptionKey;

        public JwtTokenService(
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings,
            IOptions<JwtSecuritySettings> securitySettings,
            ILogger<JwtTokenService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
            _securitySettings = securitySettings.Value;
            _logger = logger;
            _encryptionKey = Encoding.UTF8.GetBytes(_securitySettings.EncryptionKey);
        }

        public async Task<Result<AuthTokenResult>> GenerateTokensAsync(User user, string ipAddress, string? userAgent = null)
        {
            try
            {
                var jwtId = Guid.NewGuid().ToString();
                var tokenFamily = Guid.NewGuid().ToString();                // Generate access token
                var accessToken = GenerateAccessToken(user, jwtId);
                
                // Generate refresh token
                var refreshTokenValue = GenerateRefreshTokenValue();
                var refreshToken = new RefreshToken
                {
                    Token = HashToken(refreshTokenValue),
                    JwtId = jwtId,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(_securitySettings.RefreshTokenExpiryDays),
                    CreatedByIp = ipAddress,
                    UserAgent = userAgent,
                    TokenFamily = tokenFamily
                };

                // Clean up old refresh tokens for user
                await CleanupUserRefreshTokensAsync(user.Id);

                // Save refresh token
                await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Add refresh token (assuming we'll create repository for it)
                        // For now, we'll use Entity Framework directly
                        _unitOfWork.Context.Set<RefreshToken>().Add(refreshToken);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        return Result.Success();
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                });

                var result = new AuthTokenResult
                {
                    AccessToken = _securitySettings.EnableTokenEncryption ? EncryptToken(accessToken) : accessToken,
                    RefreshToken = refreshTokenValue,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_securitySettings.AccessTokenExpiryMinutes),
                    TokenType = "Bearer"
                };

                return Result<AuthTokenResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tokens for user {UserId}", user.Id);
                return Result<AuthTokenResult>.Failure("Failed to generate tokens", "TOKEN_GENERATION_ERROR");
            }
        }

        public async Task<Result<AuthTokenResult>> RefreshTokenAsync(string refreshToken, string ipAddress, string? userAgent = null)
        {
            try
            {
                var hashedToken = HashToken(refreshToken);
                var storedToken = await _unitOfWork.Context.Set<RefreshToken>()
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == hashedToken && rt.IsActive);

                if (storedToken == null)
                {
                    return Result<AuthTokenResult>.Failure("Invalid refresh token", "INVALID_REFRESH_TOKEN");
                }

                if (storedToken.IsExpired)
                {
                    return Result<AuthTokenResult>.Failure("Refresh token expired", "REFRESH_TOKEN_EXPIRED");
                }

                // Mark current token as used
                storedToken.IsUsed = true;
                storedToken.UpdatedAt = DateTime.UtcNow;

                // Generate new tokens
                var newTokens = await GenerateTokensAsync(storedToken.User, ipAddress, userAgent);

                if (!newTokens.IsSuccess)
                {
                    return newTokens;
                }

                // Mark old token as replaced
                storedToken.ReplacedByToken = newTokens.Data!.RefreshToken;

                await _unitOfWork.SaveChangesAsync();

                return newTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return Result<AuthTokenResult>.Failure("Failed to refresh token", "TOKEN_REFRESH_ERROR");
            }
        }

        public async Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress, string reason)
        {
            try
            {
                var hashedToken = HashToken(refreshToken);
                var storedToken = await _unitOfWork.Context.Set<RefreshToken>()
                    .FirstOrDefaultAsync(rt => rt.Token == hashedToken);

                if (storedToken == null)
                {
                    return Result.Failure("Token not found", "TOKEN_NOT_FOUND");
                }

                // Revoke token and its family
                var familyTokens = await _unitOfWork.Context.Set<RefreshToken>()
                    .Where(rt => rt.TokenFamily == storedToken.TokenFamily && rt.IsActive)
                    .ToListAsync();

                foreach (var token in familyTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = ipAddress;
                    token.RevokedReason = reason;
                    token.UpdatedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return Result.Failure("Failed to revoke token", "TOKEN_REVOCATION_ERROR");
            }
        }

        public async Task<Result> BlacklistTokenAsync(string jwtId, int userId, string reason, string ipAddress)
        {
            try
            {
                var blacklistEntry = new TokenBlacklist
                {
                    JwtId = jwtId,
                    TokenHash = HashToken(jwtId),
                    UserId = userId,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(_securitySettings.AccessTokenExpiryMinutes),
                    BlacklistReason = reason,
                    BlacklistedByIp = ipAddress,
                    BlacklistType = BlacklistTypes.Security
                };

                _unitOfWork.Context.Set<TokenBlacklist>().Add(blacklistEntry);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting token {JwtId}", jwtId);
                return Result.Failure("Failed to blacklist token", "TOKEN_BLACKLIST_ERROR");
            }
        }

        public async Task<Result<bool>> IsTokenBlacklistedAsync(string jwtId)
        {
            try
            {
                var hashedJwtId = HashToken(jwtId);
                var isBlacklisted = await _unitOfWork.Context.Set<TokenBlacklist>()
                    .AnyAsync(tb => tb.JwtId == jwtId || tb.TokenHash == hashedJwtId && tb.IsActive);

                return Result<bool>.Success(isBlacklisted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token is blacklisted");
                return Result<bool>.Failure("Failed to check token blacklist status", "TOKEN_BLACKLIST_CHECK_ERROR");
            }
        }        public async Task<Result<ClaimsPrincipal>> ValidateTokenInternalAsync(string token)
        {
            try
            {
                // Decrypt token if encryption is enabled
                if (_securitySettings.EnableTokenEncryption)
                {
                    token = DecryptToken(token);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = _jwtSettings.ValidateIssuer,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = _jwtSettings.ValidateAudience,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = _jwtSettings.ValidateLifetime,
                    ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                // Check if token is blacklisted
                var jwtToken = validatedToken as JwtSecurityToken;
                var jwtId = jwtToken?.Claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                
                if (!string.IsNullOrEmpty(jwtId))
                {
                    var blacklistCheck = await IsTokenBlacklistedAsync(jwtId);
                    if (blacklistCheck.IsSuccess && blacklistCheck.Data)
                    {
                        return Result<ClaimsPrincipal>.Failure("Token is blacklisted", "TOKEN_BLACKLISTED");
                    }
                }

                return Result<ClaimsPrincipal>.Success(principal);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return Result<ClaimsPrincipal>.Failure("Invalid token", "INVALID_TOKEN");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Result<ClaimsPrincipal>.Failure("Token validation error", "TOKEN_VALIDATION_ERROR");
            }
        }

        // Interface implementation methods
        public async Task<Result<string>> GenerateTokenAsync(int userId, Dictionary<string, object>? additionalClaims = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<string>.Failure("User not found", "USER_NOT_FOUND");
                }

                var jwtId = Guid.NewGuid().ToString();
                var accessToken = GenerateAccessToken(user, jwtId, additionalClaims);

                return Result<string>.Success(_securitySettings.EnableTokenEncryption ? EncryptToken(accessToken) : accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for user {UserId}", userId);
                return Result<string>.Failure("Failed to generate token", "TOKEN_GENERATION_ERROR");
            }
        }

        public async Task<Result<DTOs.TokenValidationResult>> ValidateTokenAsync(string token)
        {
            try
            {
                var internalResult = await ValidateTokenInternalAsync(token);            if (!internalResult.IsSuccess)
                {
                    return Result<DTOs.TokenValidationResult>.Failure(internalResult.ErrorMessage, internalResult.ErrorCode);
                }

                var principal = internalResult.Data!;
                var jwtToken = GetJwtTokenFromPrincipal(token);
                
                var result = new DTOs.TokenValidationResult
                {
                    IsValid = true,
                    JwtId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value,
                    UserId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                    Username = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                    Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
                    ExpiresAt = jwtToken?.ValidTo,
                    ValidationMessage = "Token is valid",
                    IsExpired = jwtToken?.ValidTo < DateTime.UtcNow,
                    IsBlacklisted = false
                };

                return Result<DTOs.TokenValidationResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Result<DTOs.TokenValidationResult>.Failure("Token validation error", "TOKEN_VALIDATION_ERROR");
            }
        }

        public async Task<Result<string>> RefreshTokenAsync(string token)
        {
            try
            {
                var refreshResult = await RefreshTokenAsync(token, "unknown", null);
                if (!refreshResult.IsSuccess)
                {
                    return Result<string>.Failure(refreshResult.ErrorMessage, refreshResult.ErrorCode);
                }

                return Result<string>.Success(refreshResult.Data!.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return Result<string>.Failure("Failed to refresh token", "TOKEN_REFRESH_ERROR");
            }
        }

        public async Task<Result> RevokeTokenAsync(string token)
        {
            try
            {
                return await RevokeTokenAsync(token, "unknown", "Token revoked via API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return Result.Failure("Failed to revoke token", "TOKEN_REVOCATION_ERROR");
            }
        }

        public async Task<Result<string>> GenerateRefreshTokenAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<string>.Failure("User not found", "USER_NOT_FOUND");
                }

                var tokensResult = await GenerateTokensAsync(user, "unknown", null);
                if (!tokensResult.IsSuccess)
                {
                    return Result<string>.Failure(tokensResult.ErrorMessage, tokensResult.ErrorCode);
                }

                return Result<string>.Success(tokensResult.Data!.RefreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token for user {UserId}", userId);
                return Result<string>.Failure("Failed to generate refresh token", "REFRESH_TOKEN_GENERATION_ERROR");
            }
        }

        public async Task<Result<DTOs.RefreshTokenValidationResult>> ValidateRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var hashedToken = HashToken(refreshToken);
                var storedToken = await _unitOfWork.Context.Set<RefreshToken>()
                    .FirstOrDefaultAsync(rt => rt.Token == hashedToken && rt.IsActive);                if (storedToken == null)
                {
                    return Result<DTOs.RefreshTokenValidationResult>.Success(new DTOs.RefreshTokenValidationResult
                    {
                        IsValid = false,
                        ValidationMessage = "Invalid refresh token",
                        IsExpired = false,
                        IsRevoked = false,
                        IsUsed = false
                    });
                }

                var result = new DTOs.RefreshTokenValidationResult
                {
                    IsValid = !storedToken.IsExpired && !storedToken.IsRevoked && !storedToken.IsUsed,
                    TokenFamily = storedToken.TokenFamily,
                    UserId = storedToken.UserId,
                    ExpiresAt = storedToken.ExpiryDate,
                    ValidationMessage = storedToken.IsExpired ? "Token expired" :
                                      storedToken.IsRevoked ? "Token revoked" :
                                      storedToken.IsUsed ? "Token already used" : "Token is valid",
                    IsExpired = storedToken.IsExpired,
                    IsRevoked = storedToken.IsRevoked,
                    IsUsed = storedToken.IsUsed
                };                return Result<DTOs.RefreshTokenValidationResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                return Result<DTOs.RefreshTokenValidationResult>.Failure("Refresh token validation error", "REFRESH_TOKEN_VALIDATION_ERROR");
            }
        }

        public async Task<Result<int>> CleanupExpiredTokensAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow;

                // Remove expired refresh tokens
                var expiredRefreshTokens = await _unitOfWork.Context.Set<RefreshToken>()
                    .Where(rt => rt.ExpiryDate < cutoffDate || rt.IsDeleted)
                    .ToListAsync();

                // Remove expired blacklist entries
                var expiredBlacklistEntries = await _unitOfWork.Context.Set<TokenBlacklist>()
                    .Where(tb => tb.ExpiryDate < cutoffDate || tb.IsDeleted)
                    .ToListAsync();

                _unitOfWork.Context.Set<RefreshToken>().RemoveRange(expiredRefreshTokens);
                _unitOfWork.Context.Set<TokenBlacklist>().RemoveRange(expiredBlacklistEntries);

                await _unitOfWork.SaveChangesAsync();

                var totalCleaned = expiredRefreshTokens.Count + expiredBlacklistEntries.Count;

                _logger.LogInformation("Cleaned up {RefreshTokens} expired refresh tokens and {BlacklistEntries} expired blacklist entries",
                    expiredRefreshTokens.Count, expiredBlacklistEntries.Count);

                return Result<int>.Success(totalCleaned);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
                return Result<int>.Failure("Failed to cleanup expired tokens", "TOKEN_CLEANUP_ERROR");
            }
        }

        public async Task<Result<DTOs.TokenInfoDTO>> GetTokenInfoAsync(string token)
        {
            try
            {
                var internalResult = await ValidateTokenInternalAsync(token);            if (!internalResult.IsSuccess)
                {
                    return Result<DTOs.TokenInfoDTO>.Failure(internalResult.ErrorMessage, internalResult.ErrorCode);
                }

                var principal = internalResult.Data!;
                var jwtToken = GetJwtTokenFromPrincipal(token);

                var tokenInfo = new DTOs.TokenInfoDTO
                {
                    JwtId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty,
                    UserId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                    Username = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                    Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
                    IssuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value ?? "0")).DateTime,
                    ExpiresAt = jwtToken?.ValidTo ?? DateTime.MinValue,
                    Issuer = jwtToken?.Issuer ?? string.Empty,
                    Audiences = jwtToken?.Audiences?.ToArray() ?? Array.Empty<string>(),
                    Claims = principal.Claims.ToDictionary(c => c.Type, c => (object)c.Value)
                };                return Result<DTOs.TokenInfoDTO>.Success(tokenInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token info");
                return Result<DTOs.TokenInfoDTO>.Failure("Failed to get token info", "TOKEN_INFO_ERROR");
            }
        }

        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }

        public string EncryptToken(string token)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _encryptionKey.Take(32).ToArray(); // Ensure 32 bytes for AES-256
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();
                ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV
                
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using var writer = new StreamWriter(cs);
                writer.Write(token);
                writer.Flush();
                cs.FlushFinalBlock();

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting token");
                throw;
            }
        }

        public string DecryptToken(string encryptedToken)
        {
            try
            {
                var data = Convert.FromBase64String(encryptedToken);
                
                using var aes = Aes.Create();
                aes.Key = _encryptionKey.Take(32).ToArray();
                
                var iv = new byte[16];
                Array.Copy(data, 0, iv, 0, 16);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(data, 16, data.Length - 16);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var reader = new StreamReader(cs);
                
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting token");
                throw;
            }
        }        private string GenerateAccessToken(User user, string jwtId, Dictionary<string, object>? additionalClaims = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add additional claims if provided
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value.ToString() ?? string.Empty));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_securitySettings.AccessTokenExpiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private JwtSecurityToken? GetJwtTokenFromPrincipal(string token)
        {
            try
            {
                // Decrypt token if encryption is enabled
                if (_securitySettings.EnableTokenEncryption)
                {
                    token = DecryptToken(token);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.ReadJwtToken(token);
            }
            catch
            {
                return null;
            }
        }

        private static string GenerateRefreshTokenValue()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private async Task CleanupUserRefreshTokensAsync(int userId)
        {
            var userTokens = await _unitOfWork.Context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();

            if (userTokens.Count >= _securitySettings.MaxRefreshTokenFamilySize)
            {
                var tokensToRevoke = userTokens.Skip(_securitySettings.MaxRefreshTokenFamilySize - 1);
                foreach (var token in tokensToRevoke)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedReason = "Exceeded maximum token family size";
                    token.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }

    /// <summary>
    /// Result object for authentication tokens
    /// </summary>
    public class AuthTokenResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}
