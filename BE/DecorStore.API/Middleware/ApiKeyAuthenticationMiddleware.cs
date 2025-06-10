using System.Security.Claims;
using DecorStore.API.Models;
using DecorStore.API.DTOs;
using DecorStore.API.Services;
using DecorStore.API.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DecorStore.API.Middleware
{
    /// <summary>
    /// Middleware for API key authentication and authorization
    /// </summary>
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
        private readonly ApiKeyMiddlewareSettings _settings;

        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            ILogger<ApiKeyAuthenticationMiddleware> logger,
            IOptions<ApiKeyMiddlewareSettings> settings)
        {
            _next = next;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task InvokeAsync(HttpContext context, IApiKeyManagementService apiKeyService)
        {
            // Skip API key authentication for certain paths
            if (ShouldSkipAuthentication(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var apiKey = ExtractApiKey(context.Request);
            
            if (string.IsNullOrEmpty(apiKey))
            {
                // If API key is required but not provided
                if (_settings.RequireApiKeyForAllEndpoints)
                {
                    await HandleUnauthorized(context, "API key is required");
                    return;
                }

                // Continue to next middleware if API key is optional
                await _next(context);
                return;
            }

            try
            {
                // Validate API key
                var validationResult = await apiKeyService.ValidateApiKeyAsync(apiKey);
                if (!validationResult.IsSuccess || validationResult.Data == null)
                {
                    await HandleUnauthorized(context, "Invalid API key");
                    return;
                }

                var apiKeyEntity = validationResult.Data;

                // Check IP address restrictions
                var clientIp = GetClientIpAddress(context);
                var ipValidationResult = await apiKeyService.ValidateIpAddressAsync(apiKey, clientIp);
                if (!ipValidationResult.IsSuccess || !ipValidationResult.Data)
                {
                    await HandleForbidden(context, "IP address not allowed");
                    return;
                }

                // Check rate limits
                var rateLimitResult = await apiKeyService.CheckRateLimitAsync(apiKeyEntity.Id, clientIp);
                if (!rateLimitResult.IsSuccess || !rateLimitResult.Data)
                {
                    await HandleRateLimitExceeded(context, "Rate limit exceeded");
                    return;
                }

                // Set authentication context
                SetApiKeyContext(context, apiKeyEntity);

                // Record usage (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        await _next(context);
                        stopwatch.Stop();                        var usage = new DTOs.ApiKeyUsageRequest
                        {
                            ApiKeyId = apiKeyEntity.Id,
                            Endpoint = context.Request.Path,
                            Method = context.Request.Method,
                            IpAddress = clientIp ?? "Unknown",
                            UserAgent = context.Request.Headers.UserAgent.ToString(),
                            StatusCode = context.Response.StatusCode,
                            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                            IsSuspicious = IsSuspiciousRequest(context),
                            RiskScore = CalculateRiskScore(context, apiKeyEntity),
                            Timestamp = DateTime.UtcNow
                        };

                        await apiKeyService.RecordUsageAsync(apiKeyEntity.Id, usage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to record API key usage");
                    }
                });

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in API key authentication middleware");
                await HandleInternalError(context, "Authentication error");
            }
        }

        private bool ShouldSkipAuthentication(string path)
        {
            var skipPaths = new[]
            {
                "/health",
                "/metrics",
                "/swagger",
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh",
                "/favicon.ico"
            };

            return skipPaths.Any(skipPath => path.StartsWith(skipPath, StringComparison.OrdinalIgnoreCase));
        }

        private string? ExtractApiKey(HttpRequest request)
        {
            // Try Authorization header first (Bearer token format)
            var authHeader = request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (token.StartsWith("sk_"))
                {
                    return token;
                }
            }

            // Try X-API-Key header
            var apiKeyHeader = request.Headers["X-API-Key"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKeyHeader))
            {
                return apiKeyHeader;
            }

            // Try query parameter (less secure, but sometimes needed)
            if (_settings.AllowApiKeyInQueryString)
            {
                var queryApiKey = request.Query["api_key"].FirstOrDefault();
                if (!string.IsNullOrEmpty(queryApiKey))
                {
                    return queryApiKey;
                }
            }

            return null;
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Try to get real IP from headers (for load balancers/proxies)
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                var ips = xForwardedFor.Split(',');
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // Fallback to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }        private void SetApiKeyContext(HttpContext context, ApiKeyValidationResult apiKey)
        {
            // Create claims for the API key
            var claims = new List<Claim>
            {
                new("api_key_id", apiKey.Id.ToString()),
                new("api_key_name", apiKey.Name),
                new("auth_type", "api_key"),
                new("user_id", apiKey.UserId.ToString())
            };

            // Add scope claims
            foreach (var scope in apiKey.Scopes)
            {
                claims.Add(new Claim("scope", scope));
            }

            // Add user claims
            claims.Add(new Claim("username", apiKey.Username));

            var identity = new ClaimsIdentity(claims, "ApiKey");
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;

            // Add API key info to context items for later use
            context.Items["ApiKey"] = apiKey;
            context.Items["ApiKeyId"] = apiKey.Id;
            context.Items["ApiKeyScopes"] = apiKey.Scopes;
        }

        private bool IsSuspiciousRequest(HttpContext context)
        {
            var suspiciousIndicators = 0;

            // Check for suspicious patterns
            var userAgent = context.Request.Headers.UserAgent.ToString();
            if (string.IsNullOrEmpty(userAgent) || 
                userAgent.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
                userAgent.Contains("scanner", StringComparison.OrdinalIgnoreCase))
            {
                suspiciousIndicators++;
            }

            // Check for unusual request patterns
            if (context.Request.Headers.Count > 50)
            {
                suspiciousIndicators++;
            }

            // Check for SQL injection patterns in query strings
            var queryString = context.Request.QueryString.ToString();
            if (!string.IsNullOrEmpty(queryString))
            {
                var sqlPatterns = new[] { "union", "select", "drop", "insert", "delete", "exec", "script" };
                if (sqlPatterns.Any(pattern => queryString.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    suspiciousIndicators++;
                }
            }

            // Check for unusual request size
            if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB
            {
                suspiciousIndicators++;
            }

            return suspiciousIndicators >= 2;
        }        private decimal CalculateRiskScore(HttpContext context, ApiKeyValidationResult apiKey)
        {
            decimal riskScore = 0.0m;

            // Base risk for new or unused keys
            if (apiKey.LastUsedAt == null)
            {
                riskScore += 0.1m; // First time usage
            }

            // Risk based on key expiration
            if (apiKey.ExpiresAt.HasValue)
            {
                var daysUntilExpiry = (apiKey.ExpiresAt.Value - DateTime.UtcNow).TotalDays;
                if (daysUntilExpiry < 7)
                {
                    riskScore += 0.2m; // Soon to expire
                }
            }            // Risk based on request characteristics
            if (IsSuspiciousRequest(context))
            {
                riskScore += 0.5m;
            }

            // Risk based on IP address
            var clientIp = GetClientIpAddress(context);
            if (IsKnownMaliciousIp(clientIp))
            {
                riskScore += 0.8m;
            }

            // Risk based on time of day (unusual hours)
            var hour = DateTime.UtcNow.Hour;
            if (hour < 6 || hour > 22) // Outside normal business hours
            {
                riskScore += 0.1m;
            }

            // Cap at 1.0
            return Math.Min(riskScore, 1.0m);
        }

        private bool IsKnownMaliciousIp(string ipAddress)
        {
            // In a real implementation, this would check against threat intelligence feeds
            // For now, just check against some common malicious patterns
            var maliciousPatterns = new[]
            {
                "127.0.0.1", // localhost (for testing)
                "0.0.0.0"    // invalid IP
            };

            return maliciousPatterns.Contains(ipAddress);
        }

        private async Task HandleUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized",
                message = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private async Task HandleForbidden(HttpContext context, string message)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Forbidden",
                message = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private async Task HandleRateLimitExceeded(HttpContext context, string message)
        {
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Retry-After", "3600"); // 1 hour

            var response = new
            {
                error = "Rate Limit Exceeded",
                message = message,
                retryAfter = "3600",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private async Task HandleInternalError(HttpContext context, string message)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Internal Server Error",
                message = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }

    /// <summary>
    /// Attribute to require specific API key scopes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireApiKeyScopeAttribute : Attribute
    {
        public string[] RequiredScopes { get; }

        public RequireApiKeyScopeAttribute(params string[] requiredScopes)
        {
            RequiredScopes = requiredScopes ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Authorization filter for API key scope validation
    /// </summary>
    public class ApiKeyScopeAuthorizationFilter : IAuthorizationFilter
    {
        private readonly ILogger<ApiKeyScopeAuthorizationFilter> _logger;

        public ApiKeyScopeAuthorizationFilter(ILogger<ApiKeyScopeAuthorizationFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Get required scopes from attribute
            var requiredScopeAttribute = context.ActionDescriptor.EndpointMetadata
                .OfType<RequireApiKeyScopeAttribute>()
                .FirstOrDefault();

            if (requiredScopeAttribute == null || requiredScopeAttribute.RequiredScopes.Length == 0)
            {
                return; // No scope requirements
            }

            // Check if user is authenticated via API key
            if (!context.HttpContext.User.Identity?.IsAuthenticated == true ||
                !context.HttpContext.User.HasClaim("auth_type", "api_key"))
            {
                return; // Not authenticated via API key, let other auth handle it
            }

            // Get user scopes
            var userScopes = context.HttpContext.User.Claims
                .Where(c => c.Type == "scope")
                .Select(c => c.Value)
                .ToList();

            // Check if user has required scopes
            var hasRequiredScope = requiredScopeAttribute.RequiredScopes
                .Any(requiredScope => userScopes.Contains(requiredScope));

            if (!hasRequiredScope)
            {
                _logger.LogWarning("API key lacks required scope. Required: {RequiredScopes}, User has: {UserScopes}",
                    string.Join(", ", requiredScopeAttribute.RequiredScopes),
                    string.Join(", ", userScopes));

                context.Result = new ForbidResult("Insufficient API key scope");
            }
        }
    }

    /// <summary>
    /// Configuration settings for API key middleware
    /// </summary>
    public class ApiKeyMiddlewareSettings
    {
        public bool RequireApiKeyForAllEndpoints { get; set; } = false;
        public bool AllowApiKeyInQueryString { get; set; } = false;
        public bool EnableUsageTracking { get; set; } = true;
        public bool EnableRiskScoring { get; set; } = true;
        public bool EnableIpValidation { get; set; } = true;
        public bool EnableRateLimitCheck { get; set; } = true;
        public int MaxSuspiciousRequestsPerHour { get; set; } = 100;
        public List<string> ExemptPaths { get; set; } = new()
        {
            "/health", "/metrics", "/swagger", "/api/auth"
        };
    }
}
