using System.Security.Cryptography;
using System.Text;

namespace DecorStore.API.Middleware
{
    /// <summary>
    /// Middleware for handling ETag caching and conditional requests
    /// </summary>
    public class ETagMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ETagMiddleware> _logger;

        public ETagMiddleware(RequestDelegate next, ILogger<ETagMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply ETag logic to GET requests
            if (context.Request.Method != HttpMethods.Get)
            {
                await _next(context);
                return;
            }

            // Skip ETag for certain paths (e.g., real-time endpoints, performance monitoring)
            if (ShouldSkipETag(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Check if client sent If-None-Match header
            var ifNoneMatch = context.Request.Headers["If-None-Match"].FirstOrDefault();

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);

                // Only process successful responses
                if (context.Response.StatusCode == 200)
                {
                    // Get response content
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    var responseContent = await new StreamReader(responseBodyStream).ReadToEndAsync();

                    // Generate ETag from response content
                    var etag = GenerateETag(responseContent);

                    // Add ETag header
                    context.Response.Headers["ETag"] = etag;

                    // Check if client has the same version
                    if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch.Trim('"') == etag.Trim('"'))
                    {
                        // Client has the same version, return 304 Not Modified
                        context.Response.StatusCode = 304;
                        context.Response.ContentLength = 0;
                        
                        _logger.LogDebug("ETag match found for {Path}. Returning 304 Not Modified", context.Request.Path);
                        return;
                    }

                    // Copy response content back to original stream
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalBodyStream);

                    _logger.LogDebug("ETag generated for {Path}: {ETag}", context.Request.Path, etag);
                }
                else
                {
                    // For non-200 responses, copy content without ETag processing
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private bool ShouldSkipETag(PathString path)
        {
            var pathStr = path.Value?.ToLowerInvariant() ?? "";
            
            // Skip ETag for these paths
            var skipPaths = new[]
            {
                "/api/performance",
                "/api/dashboard/realtime",
                "/api/health",
                "/api/metrics",
                "/api/loadtest"
            };

            return skipPaths.Any(skip => pathStr.StartsWith(skip));
        }

        private string GenerateETag(string content)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            var etag = Convert.ToBase64String(hash);
            
            // Return ETag in the proper format with quotes
            return $"\"{etag}\"";
        }
    }

    /// <summary>
    /// Extension method to add ETag middleware to the pipeline
    /// </summary>
    public static class ETagMiddlewareExtensions
    {
        public static IApplicationBuilder UseETagCaching(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ETagMiddleware>();
        }
    }
}
