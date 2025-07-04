using FluentValidation;
using FluentValidation.AspNetCore;
using DecorStore.API.Middleware;

namespace DecorStore.API.Extensions.Infrastructure
{
    /// <summary>
    /// Extension methods for configuring validation services
    /// </summary>
    public static class ValidationServiceExtensions
    {
        /// <summary>
        /// Adds comprehensive validation services to the DI container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddValidationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Completely disable FluentValidation's automatic integration with ASP.NET Core
            // We'll handle ALL validation manually in controllers to avoid async validation conflicts
            // Only register validators for manual use - no automatic validation
            services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

            // Register middleware services that implement IMiddleware
            services.AddScoped<GlobalExceptionHandlerMiddleware>();

            return services;
        }

        /// <summary>
        /// Configures validation middleware in the request pipeline
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The web application for chaining</returns>
        public static WebApplication UseValidationMiddleware(this WebApplication app)
        {
            // Add input sanitization (before authentication/authorization)
            app.UseMiddleware<InputSanitizationMiddleware>();

            return app;
        }
    }
}
