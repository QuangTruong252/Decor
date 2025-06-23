using DecorStore.API.Configuration;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Services;
using DecorStore.API.Services.BackgroundServices;
using DecorStore.API.Middleware;
using StackExchange.Redis;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace DecorStore.API.Extensions.Infrastructure
{
    public static class PerformanceServiceExtensions
    {        public static IServiceCollection AddPerformanceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure cache settings
            services.Configure<CacheSettings>(configuration.GetSection("Cache"));
            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>() ?? new CacheSettings();

            // Add memory cache with optimized configuration
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = cacheSettings.DefaultSizeLimit;
                options.CompactionPercentage = 0.1; // Compact 10% when cache is full
            });

            // Add response compression with multiple algorithms
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "application/json",
                    "application/javascript",
                    "text/css",
                    "text/json",
                    "text/plain",
                    "text/xml"
                });
            });

            // Configure compression levels
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            // Add response caching
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024 * 1024; // 1MB
                options.UseCaseSensitivePaths = false;
            });

            // Add output caching (for .NET 7+)
            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(builder => builder.Cache());
                
                // Add specific policies for different endpoints
                options.AddPolicy("ProductsCache", builder =>
                    builder.Cache()
                           .Expire(TimeSpan.FromMinutes(5))
                           .SetVaryByQuery("page", "pageSize", "categoryId", "searchTerm"));

                options.AddPolicy("CategoriesCache", builder =>
                    builder.Cache()
                           .Expire(TimeSpan.FromMinutes(30)));

                options.AddPolicy("DashboardCache", builder =>
                    builder.Cache()
                           .Expire(TimeSpan.FromMinutes(10)));
            });

            // Register cache services
            services.AddScoped<ICacheService, CacheService>();
            
            // Add Redis distributed cache if enabled
            if (cacheSettings.EnableDistributedCache)
            {
                var redisConnectionString = configuration.GetConnectionString("Redis");
                if (!string.IsNullOrEmpty(redisConnectionString))
                {
                    services.AddRedisConnection(configuration);
                    services.AddDistributedCacheServices(configuration);
                }
            }

            // Add background services for cache management
            if (cacheSettings.EnableCacheWarming)
            {
                services.AddHostedService<CacheWarmupService>();
            }
            
            services.AddHostedService<CacheCleanupService>();
            services.AddHostedService<PerformanceMonitoringService>();

            // Add JSON serialization optimizations
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.WriteIndented = false; // Minimize JSON size
            });

            return services;
        }

        public static IApplicationBuilder UsePerformanceMiddleware(this IApplicationBuilder app)
        {
            return app.UseResponseOptimization();
        }

        public static IServiceCollection AddDatabaseOptimizationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Entity Framework for performance
            services.AddDbContextPool<DecorStore.API.Data.ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    
                    sqlOptions.CommandTimeout(30); // 30 seconds timeout
                });

                // Performance optimizations
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            return services;
        }
    }
}
