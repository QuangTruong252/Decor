using DecorStore.API.Common;
using DecorStore.API.DTOs;

namespace DecorStore.API.Interfaces.Services
{
    /// <summary>
    /// Interface for JWT token management operations
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate a JWT token for a user
        /// </summary>
        Task<Result<string>> GenerateTokenAsync(int userId, Dictionary<string, object>? additionalClaims = null);

        /// <summary>
        /// Validate a JWT token
        /// </summary>
        Task<Result<TokenValidationResult>> ValidateTokenAsync(string token);

        /// <summary>
        /// Refresh a JWT token
        /// </summary>
        Task<Result<string>> RefreshTokenAsync(string token);

        /// <summary>
        /// Revoke a JWT token (add to blacklist)
        /// </summary>
        Task<Result> RevokeTokenAsync(string token);

        /// <summary>
        /// Check if token is blacklisted
        /// </summary>
        Task<Result<bool>> IsTokenBlacklistedAsync(string token);

        /// <summary>
        /// Generate refresh token
        /// </summary>
        Task<Result<string>> GenerateRefreshTokenAsync(int userId);

        /// <summary>
        /// Validate refresh token
        /// </summary>
        Task<Result<RefreshTokenValidationResult>> ValidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Cleanup expired tokens from blacklist
        /// </summary>
        Task<Result<int>> CleanupExpiredTokensAsync();

        /// <summary>
        /// Get token information
        /// </summary>
        Task<Result<TokenInfoDTO>> GetTokenInfoAsync(string token);
    }
}
