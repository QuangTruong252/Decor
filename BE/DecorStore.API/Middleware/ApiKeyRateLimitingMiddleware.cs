using DecorStore.API.Common;
using DecorStore.API.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace DecorStore.API.Middleware
{
    /// <summary>
    /// Middleware for API key-based rate limiting
    /// </summary>
    public class ApiKeyRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyRateLimitingMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ApiKeyRateLimitingMiddleware(
            RequestDelegate next,
            ILogger<ApiKeyRateLimitingMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for certain paths
            if (ShouldSkipRateLimit(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Extract API key from header or query string
            var apiKey = ExtractApiKey(context.Request);

            if (!string.IsNullOrEmpty(apiKey))
            {
                using var scope = _serviceProvider.CreateScope();
                var apiKeyService = scope.ServiceProvider.GetRequiredService<IApiKeyManagementService>();
                var securityLogger = scope.ServiceProvider.GetRequiredService<ISecurityEventLogger>();

                try
                {
                    // Validate API key
                    var keyValidationResult = await apiKeyService.ValidateApiKeyAsync(apiKey);
                    if (!keyValidationResult.IsSuccess || keyValidationResult.Data == null)
                    {
                        await LogSecurityEvent(securityLogger, context, "InvalidApiKey", apiKey);
                        await WriteRateLimitResponse(context, "Invalid API key", HttpStatusCode.Unauthorized);
                        return;
                    }

                    var apiKeyData = keyValidationResult.Data;

                    // Check rate limit for this API key
                    var ipAddress = GetClientIpAddress(context);
                    var rateLimitResult = await apiKeyService.CheckRateLimitAsync(apiKeyData.Id, ipAddress);

                    if (!rateLimitResult.IsSuccess || !rateLimitResult.Data)
                    {
                        await LogSecurityEvent(securityLogger, context, "RateLimitExceeded", apiKey, apiKeyData.Id);
                        await WriteRateLimitResponse(context, "Rate limit exceeded for API key", HttpStatusCode.TooManyRequests);
                        return;
                    }

                    // Record API usage
                    _ = Task.Run(async () =>
                    {
                        using var backgroundScope = _serviceProvider.CreateScope();
                        var backgroundApiKeyService = backgroundScope.ServiceProvider.GetRequiredService<IApiKeyManagementService>();
                          await backgroundApiKeyService.RecordUsageAsync(apiKeyData.Id, new DTOs.ApiKeyUsageRequest
                        {
                            ApiKeyId = apiKeyData.Id,
                            Endpoint = context.Request.Path,
                            Method = context.Request.Method,
                            IpAddress = ipAddress ?? "Unknown",
                            UserAgent = context.Request.Headers.UserAgent.ToString(),
                            Timestamp = DateTime.UtcNow
                        });
                    });

                    // Add API key info to context for downstream usage
                    context.Items["ApiKey"] = apiKeyData;
                    context.Items["ApiKeyId"] = apiKeyData.Id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during API key rate limiting");
                    await LogSecurityEvent(securityLogger, context, "RateLimitError", apiKey);
                    
                    // Allow request to continue on error to avoid breaking functionality
                    await _next(context);
                    return;
                }
            }

            await _next(context);
        }

        private static string? ExtractApiKey(HttpRequest request)
        {
            // Check X-API-Key header first
            if (request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            {
                return apiKeyHeader.FirstOrDefault();
            }

            // Check Authorization header with Bearer scheme
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authValue = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(authValue) && authValue.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
                {
                    return authValue["ApiKey ".Length..];
                }
            }

            // Check query string as fallback
            if (request.Query.TryGetValue("apikey", out var queryApiKey))
            {
                return queryApiKey.FirstOrDefault();
            }

            return null;
        }

        private static bool ShouldSkipRateLimit(PathString path)
        {
            var pathsToSkip = new[]
            {
                "/health",
                "/ready",
                "/metrics",
                "/swagger",
                "/api/docs",
                "/favicon.ico"
            };

            return pathsToSkip.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP addresses first
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static async Task LogSecurityEvent(ISecurityEventLogger securityLogger, HttpContext context, string eventType, string apiKey, int? apiKeyId = null)
        {
            try
            {
                var ipAddress = GetClientIpAddress(context);
                var userAgent = context.Request.Headers.UserAgent.ToString();
                var endpoint = context.Request.Path;
                var method = context.Request.Method;                await securityLogger.LogSuspiciousActivityAsync(
                    apiKeyId?.ToString() ?? "Unknown",
                    eventType,
                    $"API Key: {apiKey[..Math.Min(8, apiKey.Length)]}***, Endpoint: {method} {endpoint}, UserAgent: {userAgent}",
                    ipAddress,
                    new Dictionary<string, object> { ["RiskScore"] = GetRiskScore(eventType) }
                );
            }
            catch
            {
                // Ignore logging errors to prevent cascading failures
            }
        }

        private static decimal GetRiskScore(string eventType)
        {
            return eventType switch
            {
                "InvalidApiKey" => 7.5m,
                "RateLimitExceeded" => 6.0m,
                "RateLimitError" => 3.0m,
                _ => 5.0m
            };
        }

        private static async Task WriteRateLimitResponse(HttpContext context, string message, HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "rate_limit_exceeded",
                message,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.ToString()
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
