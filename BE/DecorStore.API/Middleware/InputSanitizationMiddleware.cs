using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using DecorStore.API.Configuration;

namespace DecorStore.API.Middleware
{
    /// <summary>
    /// Middleware for sanitizing input to prevent XSS and injection attacks
    /// </summary>
    public class InputSanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InputSanitizationMiddleware> _logger;
        private readonly ApiSettings _apiSettings;

        // Patterns for detecting potentially dangerous input
        private static readonly Regex[] DangerousPatterns = new[]
        {
            new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
            new Regex(@"javascript:", RegexOptions.IgnoreCase),
            new Regex(@"vbscript:", RegexOptions.IgnoreCase),
            new Regex(@"onload\s*=", RegexOptions.IgnoreCase),
            new Regex(@"onerror\s*=", RegexOptions.IgnoreCase),
            new Regex(@"onclick\s*=", RegexOptions.IgnoreCase),
            new Regex(@"onmouseover\s*=", RegexOptions.IgnoreCase),
            new Regex(@"<iframe[^>]*>.*?</iframe>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
            new Regex(@"<object[^>]*>.*?</object>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
            new Regex(@"<embed[^>]*>.*?</embed>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
            new Regex(@"<link[^>]*>", RegexOptions.IgnoreCase),
            new Regex(@"<meta[^>]*>", RegexOptions.IgnoreCase),
            new Regex(@"expression\s*\(", RegexOptions.IgnoreCase),
            new Regex(@"url\s*\(", RegexOptions.IgnoreCase),
            new Regex(@"@import", RegexOptions.IgnoreCase)
        };

        // SQL injection patterns
        private static readonly Regex[] SqlInjectionPatterns = new[]
        {
            new Regex(@"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|UPDATE|UNION( +ALL)?)\b)", RegexOptions.IgnoreCase),
            new Regex(@"(\b(AND|OR)\b.{1,6}?(=|>|<|\!|<=|>=))", RegexOptions.IgnoreCase),
            new Regex(@"(\b(GRANT|REVOKE)\b)", RegexOptions.IgnoreCase),
            new Regex(@"(\b(GROUP\s+BY|ORDER\s+BY|HAVING)\b)", RegexOptions.IgnoreCase),
            new Regex(@"(\b(CAST|CONVERT|ASCII|CHAR|NCHAR|NVARCHAR|VARCHAR)\b)", RegexOptions.IgnoreCase),
            new Regex(@"(\b(COUNT|SUM|AVG|MIN|MAX)\b\s*\()", RegexOptions.IgnoreCase),
            new Regex(@"(\b(SUBSTRING|CHARINDEX|PATINDEX|LEN|DATALENGTH|@@)\b)", RegexOptions.IgnoreCase),
            new Regex(@"(\b(sp_executesql|sp_sqlexec|sp_prepare|sp_unprepare)\b)", RegexOptions.IgnoreCase),
            new Regex(@"(\b(xp_cmdshell|xp_regread|xp_regwrite)\b)", RegexOptions.IgnoreCase),
            new Regex(@"(;|\s)+(shutdown|drop)", RegexOptions.IgnoreCase)
        };

        public InputSanitizationMiddleware(RequestDelegate next, ILogger<InputSanitizationMiddleware> logger, IOptions<ApiSettings> apiSettings)
        {
            _next = next;
            _logger = logger;
            _apiSettings = apiSettings.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip sanitization for certain paths
            if (ShouldSkipSanitization(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString();

            try
            {
                // Validate request size
                if (!ValidateRequestSize(context.Request, correlationId))
                {
                    await WriteErrorResponse(context, "Request size exceeds maximum allowed limit", 413);
                    return;
                }

                // Validate content type
                if (!ValidateContentType(context.Request, correlationId))
                {
                    await WriteErrorResponse(context, "Unsupported content type", 415);
                    return;
                }

                // Sanitize request body if it's JSON
                if (context.Request.ContentType?.Contains("application/json") == true)
                {
                    await SanitizeJsonRequest(context, correlationId);
                }

                // Sanitize query parameters
                SanitizeQueryParameters(context, correlationId);

                // Sanitize headers
                SanitizeHeaders(context, correlationId);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in input sanitization middleware [{CorrelationId}]", correlationId);
                await WriteErrorResponse(context, "Request processing failed", 400);
            }
        }

        private bool ShouldSkipSanitization(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? string.Empty;
            
            return pathValue.Contains("/health") ||
                   pathValue.Contains("/swagger") ||
                   pathValue.Contains("/uploads") ||
                   pathValue.Contains("/.well-known");
        }

        private bool ValidateRequestSize(HttpRequest request, string correlationId)
        {
            var maxSizeBytes = _apiSettings.MaxRequestBodySizeMB * 1024 * 1024;
            
            if (request.ContentLength.HasValue && request.ContentLength.Value > maxSizeBytes)
            {
                _logger.LogWarning("Request size limit exceeded [{CorrelationId}]: {Size} bytes > {MaxSize} bytes", 
                    correlationId, request.ContentLength.Value, maxSizeBytes);
                return false;
            }

            return true;
        }

        private bool ValidateContentType(HttpRequest request, string correlationId)
        {
            if (request.Method == "GET" || request.Method == "DELETE")
                return true;

            var contentType = request.ContentType?.ToLower();
            if (string.IsNullOrEmpty(contentType))
                return true;

            var allowedTypes = new[]
            {
                "application/json",
                "application/x-www-form-urlencoded",
                "multipart/form-data",
                "text/plain"
            };

            var isAllowed = allowedTypes.Any(type => contentType.Contains(type));
            
            if (!isAllowed)
            {
                _logger.LogWarning("Unsupported content type [{CorrelationId}]: {ContentType}", 
                    correlationId, contentType);
            }

            return isAllowed;
        }        private async Task SanitizeJsonRequest(HttpContext context, string correlationId)
        {
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (string.IsNullOrEmpty(body))
                return;

            // Check for dangerous patterns
            var threats = DetectThreats(body);
            if (threats.Any())
            {
                _logger.LogWarning("Potential security threats detected in request body [{CorrelationId}]: {Threats}", 
                    correlationId, string.Join(", ", threats));

                // For now, we log but don't block. In production, you might want to block or sanitize
                // Uncomment the following lines to block requests with threats:
                // context.Response.StatusCode = 400;
                // await context.Response.WriteAsync("Request contains potentially dangerous content");
                // return;
            }

            try
            {
                // Validate JSON structure
                using var document = JsonDocument.Parse(body);
                
                // Recursively sanitize JSON values
                var sanitizedJson = SanitizeJsonElement(document.RootElement);
                
                if (sanitizedJson != body)
                {
                    _logger.LogInformation("Request body sanitized [{CorrelationId}]", correlationId);
                    
                    // Replace request body with sanitized version
                    var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedJson);
                    context.Request.Body = new MemoryStream(sanitizedBytes);
                    context.Request.Body.Position = 0;
                    context.Request.ContentLength = sanitizedBytes.Length;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("Invalid JSON in request body [{CorrelationId}]: {Error}", 
                    correlationId, ex.Message);
            }
        }

        private void SanitizeQueryParameters(HttpContext context, string correlationId)
        {
            var threats = new List<string>();
            
            foreach (var param in context.Request.Query)
            {
                foreach (var value in param.Value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var paramThreats = DetectThreats(value);
                        if (paramThreats.Any())
                        {
                            threats.AddRange(paramThreats.Select(t => $"{param.Key}={t}"));
                        }
                    }
                }
            }

            if (threats.Any())
            {
                _logger.LogWarning("Potential security threats detected in query parameters [{CorrelationId}]: {Threats}", 
                    correlationId, string.Join(", ", threats));
            }
        }

        private void SanitizeHeaders(HttpContext context, string correlationId)
        {
            var threats = new List<string>();
            
            foreach (var header in context.Request.Headers)
            {
                foreach (var value in header.Value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var headerThreats = DetectThreats(value);
                        if (headerThreats.Any())
                        {
                            threats.AddRange(headerThreats.Select(t => $"{header.Key}={t}"));
                        }
                    }
                }
            }

            if (threats.Any())
            {
                _logger.LogWarning("Potential security threats detected in headers [{CorrelationId}]: {Threats}", 
                    correlationId, string.Join(", ", threats));
            }
        }

        private string SanitizeJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return JsonSerializer.Serialize(SanitizeString(element.GetString() ?? string.Empty));
                
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        obj[property.Name] = JsonSerializer.Deserialize<object>(SanitizeJsonElement(property.Value));
                    }
                    return JsonSerializer.Serialize(obj);
                
                case JsonValueKind.Array:
                    var array = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        array.Add(JsonSerializer.Deserialize<object>(SanitizeJsonElement(item)));
                    }
                    return JsonSerializer.Serialize(array);
                
                default:
                    return element.GetRawText();
            }
        }

        private string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // HTML encode dangerous characters
            var sanitized = System.Net.WebUtility.HtmlEncode(input);
            
            // Remove or replace dangerous patterns
            foreach (var pattern in DangerousPatterns)
            {
                sanitized = pattern.Replace(sanitized, string.Empty);
            }

            return sanitized;
        }

        private List<string> DetectThreats(string input)
        {
            var threats = new List<string>();
            
            if (string.IsNullOrEmpty(input))
                return threats;

            // Check for XSS patterns
            foreach (var pattern in DangerousPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    threats.Add($"XSS:{pattern}");
                }
            }

            // Check for SQL injection patterns
            foreach (var pattern in SqlInjectionPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    threats.Add($"SQLi:{pattern}");
                }
            }

            return threats;
        }

        private async Task WriteErrorResponse(HttpContext context, string message, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = message,
                timestamp = DateTime.UtcNow,
                correlationId = context.Items["X-Correlation-ID"]?.ToString()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
