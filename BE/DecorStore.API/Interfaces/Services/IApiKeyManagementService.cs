using DecorStore.API.Common;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Interfaces.Services
{
    /// <summary>
    /// Interface for API key management operations
    /// </summary>
    public interface IApiKeyManagementService
    {
        /// <summary>
        /// Generate a new API key for a user
        /// </summary>
        Task<Result<ApiKeyDTO>> GenerateApiKeyAsync(int userId, CreateApiKeyDTO createApiKeyDto);

        /// <summary>
        /// Validate an API key
        /// </summary>
        Task<Result<ApiKeyValidationResult>> ValidateApiKeyAsync(string apiKey);

        /// <summary>
        /// Get all API keys for a user
        /// </summary>
        Task<Result<IEnumerable<ApiKeyDTO>>> GetUserApiKeysAsync(int userId);

        /// <summary>
        /// Revoke an API key
        /// </summary>
        Task<Result> RevokeApiKeyAsync(string apiKey, int userId);

        /// <summary>
        /// Rotate an API key
        /// </summary>
        Task<Result<ApiKeyDTO>> RotateApiKeyAsync(string oldApiKey, int userId);

        /// <summary>
        /// Get API key usage statistics
        /// </summary>
        Task<Result<ApiKeyUsageStatsDTO>> GetApiKeyUsageAsync(string apiKey, int userId);

        /// <summary>
        /// Update API key settings
        /// </summary>
        Task<Result<ApiKeyDTO>> UpdateApiKeyAsync(string apiKey, UpdateApiKeyDTO updateDto, int userId);        /// <summary>
        /// Check if API key rate limit is exceeded
        /// </summary>
        Task<Result<bool>> CheckRateLimitAsync(string apiKey);

        /// <summary>
        /// Check if API key rate limit is exceeded with IP context
        /// </summary>
        Task<Result<bool>> CheckRateLimitAsync(int apiKeyId, string ipAddress);

        /// <summary>
        /// Validate IP address for API key
        /// </summary>
        Task<Result<bool>> ValidateIpAddressAsync(string apiKey, string ipAddress);

        /// <summary>
        /// Log API key usage
        /// </summary>
        Task LogApiKeyUsageAsync(string apiKey, string endpoint, string ipAddress, int statusCode, long responseTime);

        /// <summary>
        /// Cleanup expired API keys
        /// </summary>
        Task<Result<int>> CleanupExpiredKeysAsync();

        /// <summary>
        /// Get API key analytics
        /// </summary>
        Task<Result<ApiKeyAnalyticsDTO>> GetApiKeyAnalyticsAsync(string apiKey, int userId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Record API key usage
        /// </summary>
        Task<Result> RecordUsageAsync(int apiKeyId, ApiKeyUsageRequest request);

    }
}
