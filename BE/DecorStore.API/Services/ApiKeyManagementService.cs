using System.Security.Cryptography;
using System.Text;
using DecorStore.API.Common;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Models;
using DecorStore.API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DecorStore.API.Services
{
    public class ApiKeyManagementService : IApiKeyManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApiKeyManagementService> _logger;
        private readonly ISecurityEventLogger _securityLogger;
        private readonly ApiKeySettings _settings;
        private readonly IPasswordSecurityService _passwordService;

        public ApiKeyManagementService(
            IUnitOfWork unitOfWork,
            ILogger<ApiKeyManagementService> logger,
            ISecurityEventLogger securityLogger,
            IOptions<ApiKeySettings> settings,
            IPasswordSecurityService passwordService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _securityLogger = securityLogger;
            _settings = settings.Value;
            _passwordService = passwordService;
        }

        public async Task<Result<ApiKeyGenerationResult>> GenerateApiKeyAsync(CreateApiKeyRequest request)
        {
            try
            {
                // Validate request
                var validationResult = ValidateCreateRequest(request);
                if (!validationResult.IsSuccess)
                    return Result<ApiKeyGenerationResult>.Failure(validationResult.Error);

                // Generate API key
                var (keyValue, keyHash, keyPrefix) = GenerateApiKey();

                // Create API key entity
                var apiKey = new ApiKey
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    KeyHash = keyHash,
                    KeyPrefix = keyPrefix,
                    CreatedByUserId = request.CreatedByUserId,
                    ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(_settings.DefaultExpirationDays),
                    RateLimitPerHour = request.RateLimitPerHour ?? _settings.DefaultRateLimitPerHour,
                    RateLimitPerDay = request.RateLimitPerDay ?? _settings.DefaultRateLimitPerDay,
                    Environment = request.Environment ?? "production",
                    Metadata = request.Metadata
                };

                // Set scopes
                if (request.Scopes?.Any() == true)
                {
                    var validScopes = request.Scopes.Where(ApiKeyScopes.IsValidScope).ToList();
                    if (validScopes.Count != request.Scopes.Count)
                    {
                        var invalidScopes = request.Scopes.Except(validScopes);
                        _logger.LogWarning("Invalid scopes provided: {InvalidScopes}", string.Join(", ", invalidScopes));
                    }
                    apiKey.SetScopes(validScopes);
                }
                else
                {
                    // Default to read-only scopes
                    apiKey.SetScopes(ApiKeyScopes.ScopeGroups["read-only"]);
                }

                // Set IP restrictions
                if (request.AllowedIpAddresses?.Any() == true)
                {
                    apiKey.SetAllowedIpAddresses(request.AllowedIpAddresses);
                }

                // Set domain restrictions
                if (request.AllowedDomains?.Any() == true)
                {
                    apiKey.SetAllowedDomains(request.AllowedDomains);
                }

                // Save to database
                _unitOfWork.Context.Set<ApiKey>().Add(apiKey);
                await _unitOfWork.SaveChangesAsync();

                // Log security event
                await _securityLogger.LogSecurityViolationAsync(
                    "API_KEY_GENERATED",
                    request.CreatedByUserId,
                    "System",
                    $"API key '{request.Name}' generated",
                    0.3m);

                _logger.LogInformation("API key generated successfully: {KeyName} (ID: {KeyId})", request.Name, apiKey.Id);

                return Result<ApiKeyGenerationResult>.Success(new ApiKeyGenerationResult
                {
                    ApiKey = apiKey,
                    KeyValue = keyValue,
                    ExpiresAt = apiKey.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate API key");
                return Result<ApiKeyGenerationResult>.Failure("API key generation failed");
            }
        }

        public async Task<Result<ApiKey?>> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey))
                    return Result<ApiKey?>.Success(null);

                // Extract prefix and hash
                var parts = apiKey.Split('_');
                if (parts.Length != 3 || parts[0] != "sk")
                    return Result<ApiKey?>.Success(null);

                var prefix = parts[0] + "_" + parts[1];
                var keyValue = parts[2];

                // Find API key by prefix
                var storedApiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .Include(k => k.CreatedByUser)
                    .FirstOrDefaultAsync(k => k.KeyPrefix == prefix && !k.IsDeleted);

                if (storedApiKey == null)
                    return Result<ApiKey?>.Success(null);

                // Verify key hash
                var isValid = await VerifyApiKeyHash(keyValue, storedApiKey.KeyHash);
                if (!isValid)
                {
                    await _securityLogger.LogSecurityViolationAsync(
                        "API_KEY_INVALID_HASH",
                        null,
                        "System",
                        $"Invalid API key hash for prefix: {prefix}",
                        0.7m);
                    return Result<ApiKey?>.Success(null);
                }

                // Check if key is valid for use
                if (!storedApiKey.IsValidForUse)
                {
                    await _securityLogger.LogSecurityViolationAsync(
                        "API_KEY_INVALID_STATE",
                        null,
                        "System",
                        $"API key not valid for use: {storedApiKey.Name} (Expired: {storedApiKey.IsExpired}, Revoked: {storedApiKey.IsRevoked})",
                        0.5m);
                    return Result<ApiKey?>.Success(null);
                }

                // Update last used timestamp
                storedApiKey.LastUsedAt = DateTime.UtcNow;
                storedApiKey.UsageCount++;
                await _unitOfWork.SaveChangesAsync();

                return Result<ApiKey?>.Success(storedApiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate API key");
                return Result<ApiKey?>.Failure("API key validation failed");
            }
        }

        public async Task<Result<ApiKey?>> GetApiKeyAsync(int id)
        {
            try
            {
                var apiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .Include(k => k.CreatedByUser)
                    .Include(k => k.RevokedByUser)
                    .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted);

                return Result<ApiKey?>.Success(apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get API key {Id}", id);
                return Result<ApiKey?>.Failure("Failed to retrieve API key");
            }
        }

        public async Task<Result<ApiKey>> UpdateApiKeyAsync(int id, UpdateApiKeyRequest request)
        {
            try
            {
                var apiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted);

                if (apiKey == null)
                    return Result<ApiKey>.Failure("API key not found");

                // Update properties
                if (!string.IsNullOrEmpty(request.Name))
                    apiKey.Name = request.Name;

                if (request.Description != null)
                    apiKey.Description = request.Description;

                if (request.ExpiresAt.HasValue)
                    apiKey.ExpiresAt = request.ExpiresAt.Value;

                if (request.RateLimitPerHour.HasValue)
                    apiKey.RateLimitPerHour = request.RateLimitPerHour.Value;

                if (request.RateLimitPerDay.HasValue)
                    apiKey.RateLimitPerDay = request.RateLimitPerDay.Value;

                if (request.Scopes?.Any() == true)
                {
                    var validScopes = request.Scopes.Where(ApiKeyScopes.IsValidScope).ToList();
                    apiKey.SetScopes(validScopes);
                }

                if (request.AllowedIpAddresses != null)
                    apiKey.SetAllowedIpAddresses(request.AllowedIpAddresses);

                if (request.AllowedDomains != null)
                    apiKey.SetAllowedDomains(request.AllowedDomains);

                if (request.Metadata != null)
                    apiKey.Metadata = request.Metadata;

                apiKey.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                await _securityLogger.LogSecurityViolationAsync(
                    "API_KEY_UPDATED",
                    apiKey.CreatedByUserId,
                    "System",
                    $"API key '{apiKey.Name}' updated",
                    0.2m);

                return Result<ApiKey>.Success(apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update API key {Id}", id);
                return Result<ApiKey>.Failure("API key update failed");
            }
        }

        public async Task<Result> RevokeApiKeyAsync(int id, string reason, int revokedByUserId)
        {
            try
            {
                var apiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted);

                if (apiKey == null)
                    return Result.Failure("API key not found");

                if (apiKey.IsRevoked)
                    return Result.Failure("API key is already revoked");

                apiKey.IsRevoked = true;
                apiKey.RevokedAt = DateTime.UtcNow;
                apiKey.RevokedByUserId = revokedByUserId;
                apiKey.RevokedReason = reason;
                apiKey.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                await _securityLogger.LogSecurityViolationAsync(
                    "API_KEY_REVOKED",
                    revokedByUserId,
                    "System",
                    $"API key '{apiKey.Name}' revoked: {reason}",
                    0.4m);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke API key {Id}", id);
                return Result.Failure("API key revocation failed");
            }
        }

        public async Task<Result> ActivateApiKeyAsync(int id)
        {
            try
            {
                var apiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted);

                if (apiKey == null)
                    return Result.Failure("API key not found");

                apiKey.IsActive = true;
                apiKey.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate API key {Id}", id);
                return Result.Failure("API key activation failed");
            }
        }

        public async Task<Result> DeactivateApiKeyAsync(int id)
        {
            try
            {
                var apiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted);

                if (apiKey == null)
                    return Result.Failure("API key not found");

                apiKey.IsActive = false;
                apiKey.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate API key {Id}", id);
                return Result.Failure("API key deactivation failed");
            }
        }

        public async Task<Result<List<ApiKey>>> GetApiKeysAsync(ApiKeyFilterRequest? filter = null)
        {
            try
            {
                var query = _unitOfWork.Context.Set<ApiKey>()
                    .Include(k => k.CreatedByUser)
                    .Include(k => k.RevokedByUser)
                    .Where(k => !k.IsDeleted);

                if (filter != null)
                {
                    if (filter.IsActive.HasValue)
                        query = query.Where(k => k.IsActive == filter.IsActive.Value);

                    if (filter.IsRevoked.HasValue)
                        query = query.Where(k => k.IsRevoked == filter.IsRevoked.Value);

                    if (filter.CreatedByUserId.HasValue)
                        query = query.Where(k => k.CreatedByUserId == filter.CreatedByUserId.Value);

                    if (!string.IsNullOrEmpty(filter.Environment))
                        query = query.Where(k => k.Environment == filter.Environment);

                    if (filter.ExpiresAfter.HasValue)
                        query = query.Where(k => k.ExpiresAt > filter.ExpiresAfter.Value);

                    if (filter.ExpiresBefore.HasValue)
                        query = query.Where(k => k.ExpiresAt < filter.ExpiresBefore.Value);
                }

                var apiKeys = await query.OrderByDescending(k => k.CreatedAt).ToListAsync();
                return Result<List<ApiKey>>.Success(apiKeys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get API keys");
                return Result<List<ApiKey>>.Failure("Failed to retrieve API keys");
            }
        }        
        public async Task<Result> RecordUsageAsync(int apiKeyId, DTOs.ApiKeyUsageRequest usage)
        {
            try
            {
                var usageRecord = new ApiKeyUsage
                {
                    ApiKeyId = apiKeyId,
                    Endpoint = usage.Endpoint,
                    HttpMethod = usage.Method,
                    IpAddress = usage.IpAddress,
                    UserAgent = usage.UserAgent,
                    ResponseStatusCode = usage.StatusCode,
                    ResponseTimeMs = usage.ResponseTimeMs,
                    RequestSizeBytes = 0, // Not available in DTO
                    ResponseSizeBytes = 0, // Not available in DTO
                    ErrorMessage = null, // Not available in DTO
                    IsSuccessful = usage.StatusCode >= 200 && usage.StatusCode < 300,
                    IsSuspicious = usage.IsSuspicious,
                    SuspiciousReason = usage.SuspiciousReason,
                    RiskScore = usage.RiskScore
                };

                _unitOfWork.Context.Set<ApiKeyUsage>().Add(usageRecord);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record API key usage");
                return Result.Failure("Failed to record usage");
            }
        }

        public async Task<Result<ApiKeyAnalytics>> GetApiKeyAnalyticsAsync(int apiKeyId, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
                var toDate = to ?? DateTime.UtcNow;

                var usage = await _unitOfWork.Context.Set<ApiKeyUsage>()
                    .Where(u => u.ApiKeyId == apiKeyId && u.CreatedAt >= fromDate && u.CreatedAt <= toDate)
                    .ToListAsync();

                var analytics = new ApiKeyAnalytics
                {
                    ApiKeyId = apiKeyId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalRequests = usage.Count,
                    SuccessfulRequests = usage.Count(u => u.IsSuccessful),
                    FailedRequests = usage.Count(u => !u.IsSuccessful),
                    AverageResponseTime = usage.Any() ? usage.Average(u => u.ResponseTimeMs) : 0,
                    TotalDataTransferred = usage.Sum(u => u.RequestSizeBytes + u.ResponseSizeBytes),
                    UniqueIpAddresses = usage.Where(u => !string.IsNullOrEmpty(u.IpAddress))
                                             .Select(u => u.IpAddress)
                                             .Distinct()
                                             .Count(),
                    SuspiciousRequests = usage.Count(u => u.IsSuspicious),
                    AverageRiskScore = usage.Any() ? usage.Average(u => (double)u.RiskScore) : 0
                };

                return Result<ApiKeyAnalytics>.Success(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get API key analytics for {ApiKeyId}", apiKeyId);
                return Result<ApiKeyAnalytics>.Failure("Failed to retrieve analytics");
            }
        }

        public async Task<Result<int>> CleanupExpiredKeysAsync()
        {
            try
            {
                var expiredKeys = await _unitOfWork.Context.Set<ApiKey>()
                    .Where(k => k.ExpiresAt < DateTime.UtcNow && !k.IsDeleted)
                    .ToListAsync();

                if (expiredKeys.Any())
                {
                    foreach (var key in expiredKeys)
                    {
                        key.IsActive = false;
                        key.UpdatedAt = DateTime.UtcNow;
                    }

                    await _unitOfWork.SaveChangesAsync();

                    await _securityLogger.LogSecurityViolationAsync(
                        "API_KEYS_EXPIRED_CLEANUP",
                        null,
                        "System",
                        $"Deactivated {expiredKeys.Count} expired API keys",
                        0.1m);

                    _logger.LogInformation("Cleaned up {Count} expired API keys", expiredKeys.Count);
                }

                return Result<int>.Success(expiredKeys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired API keys");
                return Result<int>.Failure("Cleanup failed");
            }
        }

        public async Task<Result<bool>> CheckRateLimitAsync(int apiKeyId, string ipAddress)
        {
            try
            {
                var apiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .FirstOrDefaultAsync(k => k.Id == apiKeyId && !k.IsDeleted);

                if (apiKey == null)
                    return Result<bool>.Failure("API key not found");

                var now = DateTime.UtcNow;
                var hourAgo = now.AddHours(-1);
                var dayAgo = now.AddDays(-1);

                var hourlyUsage = await _unitOfWork.Context.Set<ApiKeyUsage>()
                    .CountAsync(u => u.ApiKeyId == apiKeyId && u.CreatedAt >= hourAgo);

                var dailyUsage = await _unitOfWork.Context.Set<ApiKeyUsage>()
                    .CountAsync(u => u.ApiKeyId == apiKeyId && u.CreatedAt >= dayAgo);

                var isWithinLimit = hourlyUsage < apiKey.RateLimitPerHour && dailyUsage < apiKey.RateLimitPerDay;

                if (!isWithinLimit)
                {
                    await _securityLogger.LogSecurityViolationAsync(
                        "API_KEY_RATE_LIMIT_EXCEEDED",
                        null,
                        ipAddress,
                        $"Rate limit exceeded for API key {apiKey.Name}. Hourly: {hourlyUsage}/{apiKey.RateLimitPerHour}, Daily: {dailyUsage}/{apiKey.RateLimitPerDay}",
                        0.6m);
                }

                return Result<bool>.Success(isWithinLimit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check rate limit for API key {ApiKeyId}", apiKeyId);
                return Result<bool>.Failure("Rate limit check failed");
            }
        }

        public async Task<Result<bool>> ValidateScopeAsync(string apiKey, string requiredScope)
        {
            try
            {
                var validationResult = await ValidateApiKeyAsync(apiKey);
                if (!validationResult.IsSuccess || validationResult.Data == null)
                    return Result<bool>.Success(false);

                var keyScopes = validationResult.Data.GetScopes();
                var hasScope = keyScopes.Contains(requiredScope);

                if (!hasScope)
                {
                    await _securityLogger.LogSecurityViolationAsync(
                        "API_KEY_INSUFFICIENT_SCOPE",
                        null,
                        "System",
                        $"API key '{validationResult.Data.Name}' lacks required scope: {requiredScope}",
                        0.5m);
                }

                return Result<bool>.Success(hasScope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate scope for API key");
                return Result<bool>.Failure("Scope validation failed");
            }        }

        // Interface-compatible methods
        public async Task<Result<ApiKeyDTO>> GenerateApiKeyAsync(int userId, CreateApiKeyDTO createApiKeyDto)
        {
            var request = new CreateApiKeyRequest
            {
                Name = createApiKeyDto.Name,
                Description = createApiKeyDto.Description,
                CreatedByUserId = userId,
                ExpiresAt = createApiKeyDto.ExpiresAt,
                Scopes = createApiKeyDto.Scopes?.ToList()
            };

            var result = await GenerateApiKeyAsync(request);
            if (!result.IsSuccess)
                return Result<ApiKeyDTO>.Failure(result.Error!);

            var dto = new ApiKeyDTO
            {
                Id = result.Data!.ApiKey.Id,
                Name = result.Data.ApiKey.Name,
                Description = result.Data.ApiKey.Description,
                KeyPreview = result.Data.KeyValue.Substring(0, Math.Min(8, result.Data.KeyValue.Length)) + "...",
                Scopes = result.Data.ApiKey.GetScopes().ToArray(),
                CreatedAt = result.Data.ApiKey.CreatedAt,
                ExpiresAt = result.Data.ExpiresAt,
                LastUsedAt = result.Data.ApiKey.LastUsedAt,
                IsActive = result.Data.ApiKey.IsActive,
                UsageCount = (int)result.Data.ApiKey.UsageCount,
                CreatedBy = result.Data.ApiKey.CreatedByUser?.Username ?? "Unknown"
            };

            return Result<ApiKeyDTO>.Success(dto);
        }

        async Task<Result<ApiKeyValidationResult>> IApiKeyManagementService.ValidateApiKeyAsync(string apiKey)
        {
            var result = await ValidateApiKeyAsync(apiKey);
            if (!result.IsSuccess)
                return Result<ApiKeyValidationResult>.Failure(result.Error!);

            if (result.Data == null)
            {
                return Result<ApiKeyValidationResult>.Success(new ApiKeyValidationResult
                {
                    IsValid = false,
                    ValidationMessage = "Invalid API key"
                });
            }

            var validationResult = new ApiKeyValidationResult
            {
                IsValid = true,
                Id = result.Data.Id,
                Scopes = result.Data.GetScopes().ToArray(),
                UserId = result.Data.CreatedByUserId ?? 0,
                Username = result.Data.CreatedByUser?.Username ?? "Unknown",
                ExpiresAt = result.Data.ExpiresAt,
                ValidationMessage = "Valid",
                IsExpired = result.Data.IsExpired,
                IsRevoked = result.Data.IsRevoked
            };

            return Result<ApiKeyValidationResult>.Success(validationResult);
        }

        public async Task<Result<IEnumerable<ApiKeyDTO>>> GetUserApiKeysAsync(int userId)
        {
            var filter = new ApiKeyFilterRequest { CreatedByUserId = userId };
            var result = await GetApiKeysAsync(filter);
            
            if (!result.IsSuccess)
                return Result<IEnumerable<ApiKeyDTO>>.Failure(result.Error!);

            var dtos = result.Data!.Select(key => new ApiKeyDTO
            {
                Id = key.Id,
                Name = key.Name,
                Description = key.Description,
                KeyPreview = key.KeyPrefix + "..." + new string('*', 20),
                Scopes = key.GetScopes().ToArray(),
                CreatedAt = key.CreatedAt,
                ExpiresAt = key.ExpiresAt,
                LastUsedAt = key.LastUsedAt,
                IsActive = key.IsActive,
                UsageCount = (int)key.UsageCount,
                CreatedBy = key.CreatedByUser?.Username ?? "Unknown"
            });

            return Result<IEnumerable<ApiKeyDTO>>.Success(dtos);
        }

        public async Task<Result> RevokeApiKeyAsync(string apiKey, int userId)
        {
            var validationResult = await ((IApiKeyManagementService)this).ValidateApiKeyAsync(apiKey);
            if (!validationResult.IsSuccess || validationResult.Data == null)
                return Result.Failure("API key not found");

            return await RevokeApiKeyAsync(validationResult.Data.Id, "Revoked by user", userId);
        }

        public async Task<Result<ApiKeyDTO>> RotateApiKeyAsync(string oldApiKey, int userId)
        {
            // This is a simplified implementation - in practice you'd want more sophisticated rotation
            var validationResult = await ((IApiKeyManagementService)this).ValidateApiKeyAsync(oldApiKey);
            if (!validationResult.IsSuccess || validationResult.Data == null)
                return Result<ApiKeyDTO>.Failure("Old API key not found");

            var oldKey = await GetApiKeyAsync(validationResult.Data.Id);
            if (!oldKey.IsSuccess || oldKey.Data == null)
                return Result<ApiKeyDTO>.Failure("Old API key not found");

            var createDto = new CreateApiKeyDTO
            {
                Name = oldKey.Data.Name + " (Rotated)",
                Description = oldKey.Data.Description,
                Scopes = oldKey.Data.GetScopes().ToArray(),
                ExpiresAt = oldKey.Data.ExpiresAt
            };

            var newKeyResult = await GenerateApiKeyAsync(userId, createDto);
            if (!newKeyResult.IsSuccess)
                return Result<ApiKeyDTO>.Failure(newKeyResult.Error!);

            // Revoke old key
            await RevokeApiKeyAsync(oldApiKey, userId);

            return Result<ApiKeyDTO>.Success(newKeyResult.Data!);
        }

        public async Task<Result<ApiKeyUsageStatsDTO>> GetApiKeyUsageAsync(string apiKey, int userId)
        {
            var validationResult = await ((IApiKeyManagementService)this).ValidateApiKeyAsync(apiKey);
            if (!validationResult.IsSuccess || validationResult.Data == null)
                return Result<ApiKeyUsageStatsDTO>.Failure("API key not found");

            var analyticsResult = await GetApiKeyAnalyticsAsync(validationResult.Data.Id);
            if (!analyticsResult.IsSuccess)
                return Result<ApiKeyUsageStatsDTO>.Failure(analyticsResult.Error!);

            var stats = new ApiKeyUsageStatsDTO
            {
                ApiKeyId = validationResult.Data.Id,
                KeyName = validationResult.Data.Name,
                TotalRequests = analyticsResult.Data!.TotalRequests,
                RequestsToday = 0, // Would need to implement daily tracking
                RequestsThisWeek = 0, // Would need to implement weekly tracking
                RequestsThisMonth = 0, // Would need to implement monthly tracking
                LastUsedAt = validationResult.Data.LastUsedAt,
                MostUsedEndpoint = "N/A",
                EndpointUsage = new Dictionary<string, int>(),
                DailyUsage = new Dictionary<string, int>()
            };

            return Result<ApiKeyUsageStatsDTO>.Success(stats);
        }

        public async Task<Result<ApiKeyDTO>> UpdateApiKeyAsync(string apiKey, UpdateApiKeyDTO updateDto, int userId)
        {
            var validationResult = await ((IApiKeyManagementService)this).ValidateApiKeyAsync(apiKey);
            if (!validationResult.IsSuccess || validationResult.Data == null)
                return Result<ApiKeyDTO>.Failure("API key not found");

            var request = new UpdateApiKeyRequest
            {
                Name = updateDto.Name,
                Description = updateDto.Description,
                ExpiresAt = updateDto.ExpiresAt,
                Scopes = updateDto.Scopes?.ToList()
            };

            var result = await UpdateApiKeyAsync(validationResult.Data.Id, request);
            if (!result.IsSuccess)
                return Result<ApiKeyDTO>.Failure(result.Error!);

            var dto = new ApiKeyDTO
            {
                Id = result.Data!.Id,
                Name = result.Data.Name,
                Description = result.Data.Description,
                KeyPreview = result.Data.KeyPrefix + "..." + new string('*', 20),
                Scopes = result.Data.GetScopes().ToArray(),
                CreatedAt = result.Data.CreatedAt,
                ExpiresAt = result.Data.ExpiresAt,
                LastUsedAt = result.Data.LastUsedAt,
                IsActive = result.Data.IsActive,
                UsageCount = (int)result.Data.UsageCount,
                CreatedBy = result.Data.CreatedByUser?.Username ?? "Unknown"
            };

            return Result<ApiKeyDTO>.Success(dto);
        }

        public async Task<Result<bool>> CheckRateLimitAsync(string apiKey)
        {
            var validationResult = await ((IApiKeyManagementService)this).ValidateApiKeyAsync(apiKey);
            if (!validationResult.IsSuccess || validationResult.Data == null)
                return Result<bool>.Success(false);

            return await CheckRateLimitAsync(validationResult.Data.Id, "Unknown");
        }

        public async Task LogApiKeyUsageAsync(string apiKey, string endpoint, string ipAddress, int statusCode, long responseTime)
        {
            var validationResult = await ((IApiKeyManagementService)this).ValidateApiKeyAsync(apiKey);
            if (!validationResult.IsSuccess || validationResult.Data == null)
                return;            var usage = new ApiKeyUsageRequest
            {
                Endpoint = endpoint,
                Method = "GET", // Default - could be passed as parameter
                IpAddress = ipAddress,
                StatusCode = statusCode,
                ResponseTimeMs = responseTime
            };

            await RecordUsageAsync(validationResult.Data.Id, usage);
        }

        public async Task<Result<ApiKeyAnalyticsDTO>> GetApiKeyAnalyticsAsync(string apiKey, int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var validationResult = await ((IApiKeyManagementService)this).ValidateApiKeyAsync(apiKey);
            if (!validationResult.IsSuccess || validationResult.Data == null)
                return Result<ApiKeyAnalyticsDTO>.Failure("API key not found");

            var analyticsResult = await GetApiKeyAnalyticsAsync(validationResult.Data.Id, startDate, endDate);
            if (!analyticsResult.IsSuccess)
                return Result<ApiKeyAnalyticsDTO>.Failure(analyticsResult.Error!);

            var dto = new ApiKeyAnalyticsDTO
            {
                TotalApiKeys = 1,
                ActiveApiKeys = validationResult.Data.IsActive ? 1 : 0,
                ExpiredApiKeys = validationResult.Data.IsExpired ? 1 : 0,
                RevokedApiKeys = validationResult.Data.IsRevoked ? 1 : 0,
                TotalRequests = analyticsResult.Data!.TotalRequests,
                RequestsToday = 0, // Would need daily tracking
                UniqueUsersToday = 1,
                TopUsedKeys = new List<ApiKeyUsageStatsDTO>(),
                RequestsByScope = new Dictionary<string, int>(),
                RequestsByDay = new Dictionary<string, int>(),
                RecentActivity = new List<string>()
            };

            return Result<ApiKeyAnalyticsDTO>.Success(dto);
        }

        private (string keyValue, string keyHash, string keyPrefix) GenerateApiKey()
        {
            // Generate key components
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString("x");
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var randomPart = Convert.ToHexString(randomBytes).ToLower();

            // Create key value and prefix
            var keyValue = randomPart;
            var keyPrefix = $"sk_{timestamp}";
            var fullKey = $"{keyPrefix}_{keyValue}";

            // Hash the key value for storage
            var keyHash = BCrypt.Net.BCrypt.HashPassword(keyValue, 12);

            return (fullKey, keyHash, keyPrefix);
        }

        private async Task<bool> VerifyApiKeyHash(string keyValue, string keyHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(keyValue, keyHash);
            }
            catch
            {
                return false;
            }
        }        private Result ValidateCreateRequest(CreateApiKeyRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
                return Result.Failure("API key name is required");

            if (request.Name.Length > 100)
                return Result.Failure("API key name cannot exceed 100 characters");

            if (request.ExpiresAt.HasValue && request.ExpiresAt <= DateTime.UtcNow)
                return Result.Failure("Expiration date must be in the future");

            if (request.RateLimitPerHour.HasValue && request.RateLimitPerHour <= 0)
                return Result.Failure("Rate limit per hour must be positive");

            if (request.RateLimitPerDay.HasValue && request.RateLimitPerDay <= 0)
                return Result.Failure("Rate limit per day must be positive");

            return Result.Success();
        }        public async Task<Result<bool>> ValidateIpAddressAsync(string apiKey, string ipAddress)
        {
            try
            {
                var validationResult = await ValidateApiKeyAsync(apiKey);
                if (!validationResult.IsSuccess || validationResult.Data == null)
                {
                    return Result<bool>.Failure("Invalid API key");
                }

                // Get the API key from database
                var keyParts = apiKey.Split('_');
                if (keyParts.Length < 3)
                {
                    return Result<bool>.Failure("Invalid API key format");
                }

                var keyPrefix = $"{keyParts[0]}_{keyParts[1]}";
                var dbApiKey = await _unitOfWork.Context.Set<ApiKey>()
                    .FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix && k.IsActive && !k.IsRevoked);

                if (dbApiKey == null)
                {
                    return Result<bool>.Failure("API key not found");
                }

                // Check if IP restrictions are configured
                if (string.IsNullOrEmpty(dbApiKey.AllowedIpAddresses))
                {
                    // No IP restrictions configured, allow all IPs
                    return Result<bool>.Success(true);
                }

                // Parse allowed IP addresses
                var allowedIps = dbApiKey.AllowedIpAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ip => ip.Trim())
                    .ToList();

                // Check if the current IP is in the allowed list
                foreach (var allowedIp in allowedIps)
                {
                    if (IsIpInRange(ipAddress, allowedIp))
                    {
                        return Result<bool>.Success(true);
                    }
                }

                // Log IP restriction violation
                await _securityLogger.LogSuspiciousActivityAsync(
                    dbApiKey.CreatedByUserId.ToString(),
                    "IP_RESTRICTION_VIOLATION",
                    $"API key access attempted from unauthorized IP: {ipAddress}",
                    ipAddress,
                    new Dictionary<string, object>
                    {
                        { "ApiKeyId", dbApiKey.Id },
                        { "AllowedIps", dbApiKey.AllowedIpAddresses },
                        { "AttemptedIp", ipAddress }
                    });

                return Result<bool>.Success(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating IP address {IpAddress} for API key", ipAddress);
                return Result<bool>.Failure("Error validating IP address");
            }
        }

        private bool IsIpInRange(string ipAddress, string allowedRange)
        {
            try
            {
                // Simple exact match
                if (ipAddress == allowedRange)
                    return true;

                // Check if it's a CIDR range (basic implementation)
                if (allowedRange.Contains('/'))
                {
                    // For now, implement simple subnet matching
                    // This would need a more robust implementation for production
                    return ipAddress.StartsWith(allowedRange.Split('/')[0].Split('.')[0]);
                }

                // Check if it's a wildcard (e.g., 192.168.1.*)
                if (allowedRange.Contains('*'))
                {
                    var pattern = allowedRange.Replace("*", ".*");
                    return System.Text.RegularExpressions.Regex.IsMatch(ipAddress, pattern);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    // Request/Response DTOs
    public class CreateApiKeyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public long? RateLimitPerHour { get; set; }
        public long? RateLimitPerDay { get; set; }
        public List<string>? Scopes { get; set; }
        public List<string>? AllowedIpAddresses { get; set; }
        public List<string>? AllowedDomains { get; set; }
        public string? Environment { get; set; }
        public string? Metadata { get; set; }
    }

    public class UpdateApiKeyRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public long? RateLimitPerHour { get; set; }
        public long? RateLimitPerDay { get; set; }
        public List<string>? Scopes { get; set; }
        public List<string>? AllowedIpAddresses { get; set; }
        public List<string>? AllowedDomains { get; set; }
        public string? Metadata { get; set; }
    }

    public class ApiKeyFilterRequest
    {
        public bool? IsActive { get; set; }
        public bool? IsRevoked { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? Environment { get; set; }
        public DateTime? ExpiresAfter { get; set; }
        public DateTime? ExpiresBefore { get; set; }
    }    public class ApiKeyGenerationResult
    {
        public ApiKey ApiKey { get; set; } = null!;
        public string KeyValue { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class ApiKeyAnalytics
    {
        public int ApiKeyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double AverageResponseTime { get; set; }
        public long TotalDataTransferred { get; set; }
        public int UniqueIpAddresses { get; set; }
        public int SuspiciousRequests { get; set; }
        public double AverageRiskScore { get; set; }
    }

    public class ApiKeySettings
    {
        public int DefaultExpirationDays { get; set; } = 365;
        public long DefaultRateLimitPerHour { get; set; } = 1000;
        public long DefaultRateLimitPerDay { get; set; } = 10000;
        public bool EnableUsageTracking { get; set; } = true;
        public bool EnableRateLimit { get; set; } = true;
        public bool EnableIpRestrictions { get; set; } = true;
        public bool EnableScopeValidation { get; set; } = true;
        public int CleanupExpiredKeysAfterDays { get; set; } = 30;
        public bool EnableAnalytics { get; set; } = true;
    }
}
