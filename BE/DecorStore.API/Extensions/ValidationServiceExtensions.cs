using FluentValidation;
using FluentValidation.AspNetCore;
using DecorStore.API.Middleware;

namespace DecorStore.API.Extensions
{
    /// <summary>
    /// Extension methods for configuring validation services
    /// </summary>
    public static class ValidationServiceExtensions
    {        /// <summary>
        /// Adds comprehensive validation services to the DI container
        /// </summary>
        public static IServiceCollection AddValidationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add FluentValidation
            services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters()
                    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

            // Register middleware services that implement IMiddleware
            services.AddScoped<GlobalExceptionHandlerMiddleware>();

            return services;
        }        /// <summary>
        /// Configures validation middleware in the request pipeline
        /// </summary>
        public static WebApplication UseValidationMiddleware(this WebApplication app)
        {
            // Add input sanitization (before authentication/authorization)
            app.UseMiddleware<InputSanitizationMiddleware>();

            return app;
        }
    }
}
