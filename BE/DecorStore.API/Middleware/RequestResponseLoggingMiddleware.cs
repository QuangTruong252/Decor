using System.Text;

namespace DecorStore.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for health checks and static files
            if (ShouldSkipLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            // Log request
            await LogRequest(context.Request, correlationId);

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            finally
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                // Log response
                await LogResponse(context.Response, correlationId, duration);

                // Copy response back to original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogRequest(HttpRequest request, string correlationId)
        {
            try
            {
                var requestBody = string.Empty;

                // Read request body if it's not too large and not a file upload
                if (request.ContentLength.HasValue && 
                    request.ContentLength.Value < 10240 && // 10KB limit
                    request.ContentType != null && 
                    !request.ContentType.Contains("multipart/form-data"))
                {
                    request.EnableBuffering();
                    var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                    await request.Body.ReadExactlyAsync(buffer, 0, buffer.Length);
                    requestBody = Encoding.UTF8.GetString(buffer);
                    request.Body.Position = 0;
                }

                _logger.LogInformation(
                    "HTTP Request [{CorrelationId}]: {Method} {Path} {QueryString} | Content-Type: {ContentType} | Content-Length: {ContentLength} | Body: {RequestBody}",
                    correlationId,
                    request.Method,
                    request.Path,
                    request.QueryString,
                    request.ContentType ?? "N/A",
                    request.ContentLength ?? 0,
                    string.IsNullOrEmpty(requestBody) ? "N/A" : requestBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging request for correlation ID: {CorrelationId}", correlationId);
            }
        }

        private async Task LogResponse(HttpResponse response, string correlationId, TimeSpan duration)
        {
            try
            {
                var responseBody = string.Empty;

                // Read response body if it's not too large
                if (response.Body.Length < 10240) // 10KB limit
                {
                    response.Body.Seek(0, SeekOrigin.Begin);
                    responseBody = await new StreamReader(response.Body).ReadToEndAsync();
                    response.Body.Seek(0, SeekOrigin.Begin);
                }

                var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

                _logger.Log(logLevel,
                    "HTTP Response [{CorrelationId}]: {StatusCode} | Content-Type: {ContentType} | Content-Length: {ContentLength} | Duration: {DurationMs}ms | Body: {ResponseBody}",
                    correlationId,
                    response.StatusCode,
                    response.ContentType ?? "N/A",
                    response.ContentLength ?? response.Body.Length,
                    duration.TotalMilliseconds,
                    string.IsNullOrEmpty(responseBody) ? "N/A" : responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging response for correlation ID: {CorrelationId}", correlationId);
            }
        }

        private static bool ShouldSkipLogging(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? string.Empty;
            
            return pathValue.Contains("/health") ||
                   pathValue.Contains("/swagger") ||
                   pathValue.Contains("/favicon.ico") ||
                   pathValue.Contains("/uploads") ||
                   pathValue.Contains("/.well-known");
        }
    }
}
