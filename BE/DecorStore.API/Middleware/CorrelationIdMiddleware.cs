namespace DecorStore.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get or generate correlation ID
            var correlationId = GetOrGenerateCorrelationId(context);

            // Store in HttpContext items
            context.Items[CorrelationIdHeaderName] = correlationId;

            // Add to response headers
            context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);

            // Add to logging context
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private string GetOrGenerateCorrelationId(HttpContext context)
        {
            // Try to get from request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
            {
                var id = correlationId.FirstOrDefault();
                if (!string.IsNullOrEmpty(id))
                {
                    return id;
                }
            }

            // Generate new correlation ID
            return Guid.NewGuid().ToString();
        }
    }
}
