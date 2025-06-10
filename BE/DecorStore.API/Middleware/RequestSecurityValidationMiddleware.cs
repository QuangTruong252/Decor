using System.Text;
using System.Text.RegularExpressions;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Services;

namespace DecorStore.API.Middleware
{
    /// <summary>
    /// Middleware for validating request security including size, headers, content type, and origin
    /// </summary>
    public class RequestSecurityValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestSecurityValidationMiddleware> _logger;
        private readonly ISecurityEventLogger _securityLogger;
        private readonly RequestSecurityOptions _options;

        // Common attack patterns
        private static readonly Regex[] SqlInjectionPatterns = {
            new Regex(@"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE){0,1}|INSERT( +INTO){0,1}|MERGE|SELECT|UPDATE|UNION( +ALL){0,1})\b)", RegexOptions.IgnoreCase),
            new Regex(@"(\b(AND|OR)\b.{1,6}?(=|<|>|\bin\b|\blike\b))", RegexOptions.IgnoreCase),
            new Regex(@"\/\*.*\*\/", RegexOptions.IgnoreCase),
            new Regex(@"'(\s*;\s*)+'", RegexOptions.IgnoreCase)
        };

        private static readonly Regex[] XssPatterns = {
            new Regex(@"<\s*script[^>]*>.*?<\s*/\s*script\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
            new Regex(@"javascript\s*:", RegexOptions.IgnoreCase),
            new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase),
            new Regex(@"<\s*iframe[^>]*>", RegexOptions.IgnoreCase),
            new Regex(@"<\s*object[^>]*>", RegexOptions.IgnoreCase),
            new Regex(@"<\s*embed[^>]*>", RegexOptions.IgnoreCase)
        };

        public RequestSecurityValidationMiddleware(
            RequestDelegate next,
            ILogger<RequestSecurityValidationMiddleware> logger,
            ISecurityEventLogger securityLogger,
            RequestSecurityOptions options)
        {
            _next = next;
            _logger = logger;
            _securityLogger = securityLogger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Validate request size
                if (!await ValidateRequestSizeAsync(context))
                {
                    return;
                }

                // Validate headers
                if (!await ValidateHeadersAsync(context))
                {
                    return;
                }

                // Validate content type
                if (!await ValidateContentTypeAsync(context))
                {
                    return;
                }

                // Validate origin
                if (!await ValidateOriginAsync(context))
                {
                    return;
                }

                // Validate for common attack patterns
                if (!await ValidateForAttackPatternsAsync(context))
                {
                    return;
                }

                // Set request timeout
                SetRequestTimeout(context);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RequestSecurityValidationMiddleware");
                await HandleSecurityViolationAsync(context, "MIDDLEWARE_ERROR", "Internal security validation error");
            }
        }

        private async Task<bool> ValidateRequestSizeAsync(HttpContext context)
        {
            if (context.Request.ContentLength > _options.MaxRequestSizeBytes)
            {
                await HandleSecurityViolationAsync(context, "REQUEST_SIZE_EXCEEDED", 
                    $"Request size {context.Request.ContentLength} exceeds maximum {_options.MaxRequestSizeBytes}");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateHeadersAsync(HttpContext context)
        {
            var headers = context.Request.Headers;

            // Check for suspicious headers
            var suspiciousHeaders = new[] { "X-Forwarded-Host", "X-Original-URL", "X-Rewrite-URL" };
            foreach (var header in suspiciousHeaders)
            {
                if (headers.ContainsKey(header) && !_options.AllowedSuspiciousHeaders.Contains(header))
                {
                    await HandleSecurityViolationAsync(context, "SUSPICIOUS_HEADER", 
                        $"Suspicious header detected: {header}");
                    return false;
                }
            }

            // Validate User-Agent
            var userAgent = headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(userAgent) && _options.RequireUserAgent)
            {
                await HandleSecurityViolationAsync(context, "MISSING_USER_AGENT", "User-Agent header is required");
                return false;
            }

            // Check for malicious User-Agent patterns
            if (!string.IsNullOrEmpty(userAgent) && _options.BlockedUserAgentPatterns.Any(pattern => 
                Regex.IsMatch(userAgent, pattern, RegexOptions.IgnoreCase)))
            {
                await HandleSecurityViolationAsync(context, "MALICIOUS_USER_AGENT", 
                    $"Blocked User-Agent pattern detected: {userAgent}");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateContentTypeAsync(HttpContext context)
        {
            if (context.Request.ContentLength > 0)
            {
                var contentType = context.Request.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    await HandleSecurityViolationAsync(context, "MISSING_CONTENT_TYPE", 
                        "Content-Type header is required for requests with body");
                    return false;
                }

                if (_options.AllowedContentTypes.Any() && 
                    !_options.AllowedContentTypes.Any(allowed => contentType.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)))
                {
                    await HandleSecurityViolationAsync(context, "INVALID_CONTENT_TYPE", 
                        $"Content-Type {contentType} is not allowed");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateOriginAsync(HttpContext context)
        {
            var origin = context.Request.Headers["Origin"].ToString();
            var referer = context.Request.Headers["Referer"].ToString();

            if (_options.AllowedOrigins.Any())
            {
                // Check Origin header
                if (!string.IsNullOrEmpty(origin) && 
                    !_options.AllowedOrigins.Any(allowed => origin.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
                {
                    await HandleSecurityViolationAsync(context, "INVALID_ORIGIN", 
                        $"Origin {origin} is not allowed");
                    return false;
                }

                // Check Referer header as fallback
                if (string.IsNullOrEmpty(origin) && !string.IsNullOrEmpty(referer))
                {
                    var refererOrigin = new Uri(referer).GetLeftPart(UriPartial.Authority);
                    if (!_options.AllowedOrigins.Any(allowed => refererOrigin.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
                    {
                        await HandleSecurityViolationAsync(context, "INVALID_REFERER", 
                            $"Referer {refererOrigin} is not allowed");
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task<bool> ValidateForAttackPatternsAsync(HttpContext context)
        {
            var request = context.Request;
            
            // Check query parameters for SQL injection and XSS
            foreach (var param in request.Query)
            {
                var value = param.Value.ToString();
                if (await DetectSqlInjectionAsync(context, value, $"Query parameter: {param.Key}"))
                    return false;
                if (await DetectXssAsync(context, value, $"Query parameter: {param.Key}"))
                    return false;
            }

            // Check request body for POST/PUT requests
            if (request.Method == "POST" || request.Method == "PUT")
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                if (await DetectSqlInjectionAsync(context, body, "Request body"))
                    return false;
                if (await DetectXssAsync(context, body, "Request body"))
                    return false;
            }

            return true;
        }

        private async Task<bool> DetectSqlInjectionAsync(HttpContext context, string input, string source)
        {
            if (string.IsNullOrEmpty(input)) return false;

            foreach (var pattern in SqlInjectionPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    await HandleSecurityViolationAsync(context, "SQL_INJECTION_ATTEMPT", 
                        $"SQL injection pattern detected in {source}: {pattern.ToString().Substring(0, Math.Min(pattern.ToString().Length, 50))}...");
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> DetectXssAsync(HttpContext context, string input, string source)
        {
            if (string.IsNullOrEmpty(input)) return false;

            foreach (var pattern in XssPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    await HandleSecurityViolationAsync(context, "XSS_ATTEMPT", 
                        $"XSS pattern detected in {source}: {pattern.ToString().Substring(0, Math.Min(pattern.ToString().Length, 50))}...");
                    return true;
                }
            }

            return false;
        }

        private void SetRequestTimeout(HttpContext context)
        {
            if (_options.RequestTimeoutSeconds > 0)
            {
                context.RequestAborted.Register(() =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 408; // Request Timeout
                    }
                });
            }
        }

        private async Task HandleSecurityViolationAsync(HttpContext context, string violationType, string details)
        {
            var ipAddress = GetClientIpAddress(context);
            var userId = GetUserId(context);

            // Log security violation
            await _securityLogger.LogSecurityViolationAsync(violationType, userId, ipAddress, details, 0.8m);

            // Set response
            context.Response.StatusCode = 400; // Bad Request
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Security validation failed",
                message = _options.ExposeDetailedErrors ? details : "Request validation failed",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress.Split(',')[0].Trim();
            }

            ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private int? GetUserId(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    /// <summary>
    /// Configuration options for request security validation
    /// </summary>
    public class RequestSecurityOptions
    {
        public long MaxRequestSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
        public int RequestTimeoutSeconds { get; set; } = 30;
        public bool RequireUserAgent { get; set; } = true;
        public bool ExposeDetailedErrors { get; set; } = false;
        public List<string> AllowedContentTypes { get; set; } = new List<string>
        {
            "application/json",
            "application/x-www-form-urlencoded",
            "multipart/form-data",
            "text/plain"
        };
        public List<string> AllowedOrigins { get; set; } = new List<string>();
        public List<string> AllowedSuspiciousHeaders { get; set; } = new List<string>();
        public List<string> BlockedUserAgentPatterns { get; set; } = new List<string>
        {
            @"sqlmap",
            @"nikto",
            @"nmap",
            @"masscan",
            @"nessus",
            @"openvas",
            @"qualys",
            @"acunetix",
            @"w3af",
            @"burp",
            @"owasp.zap"
        };
    }

    /// <summary>
    /// Extension methods for request security validation middleware
    /// </summary>
    public static class RequestSecurityValidationExtensions
    {
        public static IApplicationBuilder UseRequestSecurityValidation(
            this IApplicationBuilder builder,
            Action<RequestSecurityOptions>? configureOptions = null)
        {
            var options = new RequestSecurityOptions();
            configureOptions?.Invoke(options);

            return builder.UseMiddleware<RequestSecurityValidationMiddleware>(options);
        }

        public static IServiceCollection AddRequestSecurityValidation(
            this IServiceCollection services,
            Action<RequestSecurityOptions>? configureOptions = null)
        {
            var options = new RequestSecurityOptions();
            configureOptions?.Invoke(options);

            services.AddSingleton(options);
            return services;
        }
    }
}
