using DecorStore.API.Configuration;
using DecorStore.API.Services;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace DecorStore.API.Extensions.Authentication
{
    public static class SecurityExtensions
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure and validate ApiSettings for security
            services.Configure<ApiSettings>(configuration.GetSection("Api"));
            services.AddSingleton<IValidateOptions<ApiSettings>, Configuration.Validators.ApiSettingsValidator>();

            // Configure API Key settings
            services.Configure<ApiKeySettings>(configuration.GetSection("ApiKeySettings"));
            
            // Configure Service Authentication settings
            services.Configure<ServiceAuthSettings>(configuration.GetSection("ServiceAuth"));
            
            // Configure Security Testing settings
            services.Configure<SecurityTestingSettings>(configuration.GetSection("SecurityTesting"));

            // Register all security services
            services.AddScoped<Interfaces.Services.IApiKeyManagementService, Services.ApiKeyManagementService>();
            services.AddScoped<Interfaces.Services.IJwtTokenService, Services.JwtTokenService>();
            services.AddScoped<Interfaces.Services.ISecurityEventLogger, Services.SecurityEventLogger>();
            services.AddScoped<Interfaces.Services.IPasswordSecurityService, Services.PasswordSecurityService>();
            services.AddScoped<Interfaces.Services.IDataEncryptionService, Services.DataEncryptionService>();
            services.AddScoped<IDataAnonymizationService, DataAnonymizationService>();
            services.AddScoped<IServiceAuthenticationService, ServiceAuthenticationService>();
            services.AddScoped<ISecurityDashboardService, SecurityDashboardService>();
            services.AddScoped<IGdprComplianceService, GdprComplianceService>();
            services.AddScoped<ISecurityTestingService, SecurityTestingService>();

            var apiSettings = configuration.GetSection("Api").Get<ApiSettings>() ?? new ApiSettings();

            // Add rate limiting
            services.AddRateLimiter(options =>
            {
                // Global rate limiting policy
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = apiSettings.RequestsPerMinute,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // API-specific rate limiting policy
                options.AddFixedWindowLimiter("ApiPolicy", options =>
                {
                    options.PermitLimit = apiSettings.RequestsPerMinute;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = apiSettings.BurstLimit;
                });

                // Strict rate limiting for authentication endpoints
                options.AddFixedWindowLimiter("AuthPolicy", options =>
                {
                    options.PermitLimit = 10; // 10 requests per minute for auth endpoints
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 5;
                });

                // Rate limiting rejection response
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    await context.HttpContext.Response.WriteAsync(
                        "Rate limit exceeded. Please try again later.", token);
                };
            });

            // Add data protection
            services.AddDataProtection();

            // Add HSTS
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            // Add HTTPS redirection
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443;
            });

            return services;
        }

        public static WebApplication UseSecurityMiddleware(this WebApplication app)
        {
            // Use HTTPS redirection in production
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
                app.UseHsts();
            }

            // Use security headers
            app.UseSecurityHeaders();

            // Skip API key middleware in test environment to avoid JWT interference
            if (!app.Environment.IsEnvironment("Test"))
            {
                // Use API key rate limiting middleware
                app.UseMiddleware<ApiKeyRateLimitingMiddleware>();

                // Use rate limiting
                app.UseRateLimiter();
            }

            return app;
        }

        public static WebApplication UseSecurityHeaders(this WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                // Security headers
                var headers = context.Response.Headers;

                // Prevent clickjacking
                headers.Append("X-Frame-Options", "DENY");

                // Prevent MIME type sniffing
                headers.Append("X-Content-Type-Options", "nosniff");

                // XSS protection
                headers.Append("X-XSS-Protection", "1; mode=block");

                // Referrer policy
                headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                // Content Security Policy
                var csp = "default-src 'self'; " +
                         "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                         "style-src 'self' 'unsafe-inline'; " +
                         "img-src 'self' data: https:; " +
                         "font-src 'self'; " +
                         "connect-src 'self'; " +
                         "frame-ancestors 'none';";
                headers.Append("Content-Security-Policy", csp);

                // Permissions policy
                headers.Append("Permissions-Policy", 
                    "camera=(), microphone=(), geolocation=(), payment=()");

                // Remove server header
                headers.Remove("Server");

                await next();
            });

            return app;
        }
    }
}
