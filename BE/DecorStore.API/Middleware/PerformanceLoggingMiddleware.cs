using System.Diagnostics;

namespace DecorStore.API.Middleware
{
    public class PerformanceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceLoggingMiddleware> _logger;

        public PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip performance logging for health checks and static files
            if (ShouldSkipPerformanceLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var endTime = DateTime.UtcNow;
                var duration = stopwatch.Elapsed;

                LogPerformanceMetrics(context, correlationId, duration, startTime, endTime);
            }
        }

        private void LogPerformanceMetrics(HttpContext context, string correlationId, TimeSpan duration, DateTime startTime, DateTime endTime)
        {
            var request = context.Request;
            var response = context.Response;

            var performanceData = new
            {
                CorrelationId = correlationId,
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                StatusCode = response.StatusCode,
                DurationMs = duration.TotalMilliseconds,
                StartTime = startTime,
                EndTime = endTime,
                ContentLength = response.ContentLength ?? 0,
                UserAgent = request.Headers.UserAgent.FirstOrDefault(),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                RequestSize = request.ContentLength ?? 0
            };

            // Determine log level based on performance
            var logLevel = LogLevel.Information;
            if (duration.TotalMilliseconds > 5000) // 5 seconds
            {
                logLevel = LogLevel.Error;
            }
            else if (duration.TotalMilliseconds > 2000) // 2 seconds
            {
                logLevel = LogLevel.Warning;
            }
            else if (duration.TotalMilliseconds > 1000) // 1 second
            {
                logLevel = LogLevel.Warning;
            }

            _logger.Log(logLevel,
                "Performance [{CorrelationId}]: {Method} {Path} completed in {DurationMs}ms with status {StatusCode}. {@PerformanceData}",
                correlationId,
                request.Method,
                request.Path,
                duration.TotalMilliseconds,
                response.StatusCode,
                performanceData);

            // Log additional warnings for slow requests
            if (duration.TotalMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected [{CorrelationId}]: {Method} {Path} took {DurationMs}ms",
                    correlationId,
                    request.Method,
                    request.Path,
                    duration.TotalMilliseconds);
            }

            // Log memory usage periodically (every 100th request)
            if (GetRequestCounter() % 100 == 0)
            {
                LogMemoryUsage(correlationId);
            }
        }

        private void LogMemoryUsage(string correlationId)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var memoryUsage = new
                {
                    WorkingSetMB = process.WorkingSet64 / 1024 / 1024,
                    PrivateMemoryMB = process.PrivateMemorySize64 / 1024 / 1024,
                    VirtualMemoryMB = process.VirtualMemorySize64 / 1024 / 1024,
                    GCTotalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                    ThreadCount = process.Threads.Count
                };

                _logger.LogInformation(
                    "Memory Usage [{CorrelationId}]: {@MemoryUsage}",
                    correlationId,
                    memoryUsage);

                // Log warning if memory usage is high
                if (memoryUsage.WorkingSetMB > 500) // 500MB
                {
                    _logger.LogWarning(
                        "High memory usage detected [{CorrelationId}]: Working set is {WorkingSetMB}MB",
                        correlationId,
                        memoryUsage.WorkingSetMB);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging memory usage for correlation ID: {CorrelationId}", correlationId);
            }
        }

        private static int _requestCounter = 0;
        private static int GetRequestCounter()
        {
            return Interlocked.Increment(ref _requestCounter);
        }

        private static bool ShouldSkipPerformanceLogging(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? string.Empty;
            
            return pathValue.Contains("/health") ||
                   pathValue.Contains("/swagger") ||
                   pathValue.Contains("/favicon.ico") ||
                   pathValue.Contains("/.well-known");
        }
    }
}
