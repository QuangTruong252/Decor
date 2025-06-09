using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace DecorStore.API.Middleware
{
    public static class ResponseCompressionExtensions
    {
        public static IServiceCollection AddResponseCompressionServices(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                
                // Configure MIME types to compress
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "application/json",
                    "application/javascript",
                    "application/xml",
                    "text/css",
                    "text/html",
                    "text/json",
                    "text/plain",
                    "text/xml",
                    "image/svg+xml",
                    "application/font-woff",
                    "application/font-woff2"
                });
            });

            // Configure compression levels
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            return services;
        }
    }

    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseCachingMiddleware> _logger;

        public ResponseCachingMiddleware(RequestDelegate next, ILogger<ResponseCachingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip for non-GET requests
            if (context.Request.Method != HttpMethods.Get)
            {
                await _next(context);
                return;
            }

            // Skip for authenticated requests (for now)
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLowerInvariant();
            
            // Configure cache headers based on endpoint
            if (ShouldCache(path))
            {
                var cacheMaxAge = GetCacheMaxAge(path);
                
                context.Response.Headers.CacheControl = $"public, max-age={cacheMaxAge}";
                context.Response.Headers.Expires = DateTime.UtcNow.AddSeconds(cacheMaxAge).ToString("R");
                
                // Add ETag for better caching
                var etag = $"\"{path?.GetHashCode()}\"";
                context.Response.Headers.ETag = etag;
                
                // Check if client has cached version
                if (context.Request.Headers.IfNoneMatch == etag)
                {
                    context.Response.StatusCode = 304; // Not Modified
                    return;
                }

                _logger.LogDebug("Applied response caching headers for path: {Path}", path);
            }

            await _next(context);
        }

        private static bool ShouldCache(string? path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            var cacheablePaths = new[]
            {
                "/api/categories",
                "/api/products",
                "/api/banners",
                "/api/dashboard/stats"
            };

            return cacheablePaths.Any(cp => path.StartsWith(cp));
        }

        private static int GetCacheMaxAge(string? path)
        {
            return path switch
            {
                var p when p?.Contains("/categories") == true => 1800, // 30 minutes
                var p when p?.Contains("/banners") == true => 900,     // 15 minutes
                var p when p?.Contains("/products") == true => 600,    // 10 minutes
                var p when p?.Contains("/dashboard") == true => 300,   // 5 minutes
                _ => 300 // Default 5 minutes
            };
        }
    }

    public class JsonOptimizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JsonOptimizationMiddleware> _logger;

        public JsonOptimizationMiddleware(RequestDelegate next, ILogger<JsonOptimizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await _next(context);

            // Only process JSON responses
            if (context.Response.ContentType?.Contains("application/json") == true)
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();

                // Minify JSON if not already minified
                if (ShouldMinifyJson(responseBody))
                {
                    var minifiedJson = MinifyJson(responseBody);
                    var minifiedBytes = System.Text.Encoding.UTF8.GetBytes(minifiedJson);
                    
                    context.Response.ContentLength = minifiedBytes.Length;
                    await originalBodyStream.WriteAsync(minifiedBytes);
                    
                    _logger.LogDebug("Minified JSON response, saved {Bytes} bytes", 
                        responseBodyStream.Length - minifiedBytes.Length);
                }
                else
                {
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
        }

        private static bool ShouldMinifyJson(string json)
        {
            // Simple check - if it contains indentation, it's probably not minified
            return json.Contains("  ") || json.Contains("\n");
        }

        private static string MinifyJson(string json)
        {
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(json);
                return System.Text.Json.JsonSerializer.Serialize(document, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false
                });
            }
            catch
            {
                // If parsing fails, return original
                return json;
            }
        }
    }

    public static class ResponseOptimizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseOptimization(this IApplicationBuilder builder)
        {
            return builder
                .UseMiddleware<ResponseCachingMiddleware>()
                .UseResponseCompression()
                .UseMiddleware<JsonOptimizationMiddleware>();
        }
    }
}
