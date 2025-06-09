using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DecorStore.API.Services.BackgroundServices;

namespace DecorStore.API.Extensions
{
    /// <summary>
    /// Extensions for background processing services
    /// </summary>
    public static class BackgroundServiceExtensions
    {
        /// <summary>
        /// Adds background processing services for caching, cleanup, and monitoring
        /// </summary>
        public static IServiceCollection AddBackgroundServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Add cache warmup background service
            services.AddHostedService<CacheWarmupBackgroundService>();
            
            // Add data cleanup background service
            services.AddHostedService<DataCleanupBackgroundService>();
            
            // Add performance monitoring background service
            services.AddHostedService<PerformanceMonitoringBackgroundService>();
            services.AddSingleton<PerformanceMonitoringBackgroundService>();

            return services;
        }
    }
}
