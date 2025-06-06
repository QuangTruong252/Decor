using DecorStore.API.Interfaces.Services;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for managing correlation IDs across requests
    /// </summary>
    public class CorrelationIdService : ICorrelationIdService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";
        private const string CorrelationIdItemKey = "CorrelationId";

        public CorrelationIdService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the current correlation ID for the request
        /// </summary>
        /// <returns>The correlation ID as a string</returns>
        public string GetCorrelationId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return GenerateCorrelationId();
            }

            // Try to get from HttpContext items first
            if (context.Items.TryGetValue(CorrelationIdItemKey, out var correlationId) && correlationId is string id)
            {
                return id;
            }

            // Try to get from request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue))
            {
                var correlationIdFromHeader = headerValue.FirstOrDefault();
                if (!string.IsNullOrEmpty(correlationIdFromHeader))
                {
                    SetCorrelationId(correlationIdFromHeader);
                    return correlationIdFromHeader;
                }
            }

            // Generate new correlation ID if none exists
            var newCorrelationId = GenerateCorrelationId();
            SetCorrelationId(newCorrelationId);
            return newCorrelationId;
        }

        /// <summary>
        /// Sets the correlation ID for the current request
        /// </summary>
        /// <param name="correlationId">The correlation ID to set</param>
        public void SetCorrelationId(string correlationId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                // Store in HttpContext items
                context.Items[CorrelationIdItemKey] = correlationId;

                // Add to response headers
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                {
                    context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
                }
            }
        }

        /// <summary>
        /// Generates a new correlation ID
        /// </summary>
        /// <returns>A new unique correlation ID</returns>
        public string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
