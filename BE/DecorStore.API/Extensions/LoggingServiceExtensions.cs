using DecorStore.API.Configuration;
using DecorStore.API.Middleware;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace DecorStore.API.Extensions
{
    public static class LoggingServiceExtensions
    {
        public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure and validate ApiSettings for logging
            services.Configure<ApiSettings>(configuration.GetSection("Api"));
            services.AddSingleton<IValidateOptions<ApiSettings>, Configuration.Validators.ApiSettingsValidator>();
            
            // Enable options validation on startup
            services.AddOptions<ApiSettings>()
                .Bind(configuration.GetSection("Api"))
                .ValidateOnStart();

            var apiSettings = configuration.GetSection("Api").Get<ApiSettings>() ?? new ApiSettings();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(ParseLogLevel(apiSettings.DefaultLogLevel))
                .MinimumLevel.Override("Microsoft", ParseLogLevel(apiSettings.MicrosoftLogLevel))
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "DecorStore.API")
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/decorstore-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();

            // Add Serilog to the logging pipeline
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger);
            });

            // Add correlation ID support
            services.AddScoped<ICorrelationIdService, CorrelationIdService>();

            // Add performance logging
            services.AddScoped<IPerformanceLogger, PerformanceLogger>();

            // Add structured logging helpers
            services.AddScoped<IStructuredLogger, StructuredLogger>();

            return services;
        }

        public static WebApplication UseLoggingMiddleware(this WebApplication app)
        {
            // Add correlation ID middleware
            app.UseMiddleware<CorrelationIdMiddleware>();

            // Add request/response logging middleware
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            // Add performance logging middleware
            app.UseMiddleware<PerformanceLoggingMiddleware>();

            return app;
        }

        private static LogEventLevel ParseLogLevel(string logLevel)
        {
            return logLevel.ToLower() switch
            {
                "trace" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "critical" => LogEventLevel.Fatal,
                "none" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }

    // Correlation ID service interface and implementation
    public interface ICorrelationIdService
    {
        string GetCorrelationId();
        void SetCorrelationId(string correlationId);
    }

    public class CorrelationIdService : ICorrelationIdService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCorrelationId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.ContainsKey(CorrelationIdHeaderName) == true)
            {
                return context.Items[CorrelationIdHeaderName]?.ToString() ?? Guid.NewGuid().ToString();
            }
            return Guid.NewGuid().ToString();
        }

        public void SetCorrelationId(string correlationId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items[CorrelationIdHeaderName] = correlationId;
            }
        }
    }

    // Performance logger interface and implementation
    public interface IPerformanceLogger
    {
        void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object>? additionalData = null);
    }

    public class PerformanceLogger : IPerformanceLogger
    {
        private readonly ILogger<PerformanceLogger> _logger;
        private readonly ICorrelationIdService _correlationIdService;

        public PerformanceLogger(ILogger<PerformanceLogger> logger, ICorrelationIdService correlationIdService)
        {
            _logger = logger;
            _correlationIdService = correlationIdService;
        }

        public void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object>? additionalData = null)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            var logData = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["DurationMs"] = duration.TotalMilliseconds,
                ["CorrelationId"] = correlationId
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            if (duration.TotalMilliseconds > 1000) // Log as warning if operation takes more than 1 second
            {
                _logger.LogWarning("Slow operation detected: {Operation} took {DurationMs}ms. Data: {@LogData}", 
                    operation, duration.TotalMilliseconds, logData);
            }
            else
            {
                _logger.LogInformation("Operation completed: {Operation} took {DurationMs}ms. Data: {@LogData}", 
                    operation, duration.TotalMilliseconds, logData);
            }
        }
    }

    // Structured logger interface and implementation
    public interface IStructuredLogger
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogError(string message, params object[] args);
    }

    public class StructuredLogger : IStructuredLogger
    {
        private readonly ILogger<StructuredLogger> _logger;
        private readonly ICorrelationIdService _correlationIdService;

        public StructuredLogger(ILogger<StructuredLogger> logger, ICorrelationIdService correlationIdService)
        {
            _logger = logger;
            _correlationIdService = correlationIdService;
        }

        public void LogInformation(string message, params object[] args)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            _logger.LogInformation("[{CorrelationId}] " + message, new object[] { correlationId }.Concat(args).ToArray());
        }

        public void LogWarning(string message, params object[] args)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            _logger.LogWarning("[{CorrelationId}] " + message, new object[] { correlationId }.Concat(args).ToArray());
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            _logger.LogError(exception, "[{CorrelationId}] " + message, new object[] { correlationId }.Concat(args).ToArray());
        }

        public void LogError(string message, params object[] args)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            _logger.LogError("[{CorrelationId}] " + message, new object[] { correlationId }.Concat(args).ToArray());
        }
    }
}
