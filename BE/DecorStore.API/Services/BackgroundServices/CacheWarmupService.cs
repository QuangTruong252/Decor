using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DecorStore.API.Configuration;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Interfaces.Repositories;

namespace DecorStore.API.Services.BackgroundServices
{
    public class CacheWarmupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<CacheWarmupService> _logger;
        private readonly TimeSpan _warmupInterval;

        public CacheWarmupService(
            IServiceProvider serviceProvider,
            IOptions<CacheSettings> cacheSettings,
            ILogger<CacheWarmupService> logger)
        {
            _serviceProvider = serviceProvider;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
            _warmupInterval = TimeSpan.FromHours(6); // Warm up every 6 hours
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_cacheSettings.EnableCacheWarming)
            {
                _logger.LogInformation("Cache warming is disabled");
                return;
            }

            _logger.LogInformation("Cache warmup service started");

            // Initial warmup
            await WarmupCache(stoppingToken);

            // Periodic warmup
            using var timer = new PeriodicTimer(_warmupInterval);
            
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await WarmupCache(stoppingToken);
            }
        }

        private async Task WarmupCache(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting cache warmup process");
            var startTime = DateTime.UtcNow;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var categoryRepository = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
                var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var bannerService = scope.ServiceProvider.GetService<IBannerService>();

                var warmupTasks = new List<Task>();

                // Warm up categories
                warmupTasks.Add(WarmupCategories(cacheService, categoryRepository, cancellationToken));

                // Warm up featured products
                warmupTasks.Add(WarmupFeaturedProducts(cacheService, productRepository, cancellationToken));

                // Warm up top-rated products
                warmupTasks.Add(WarmupTopRatedProducts(cacheService, productRepository, cancellationToken));

                // Warm up banners if service is available
                if (bannerService != null)
                {
                    warmupTasks.Add(WarmupBanners(cacheService, bannerService, cancellationToken));
                }

                // Warm up dashboard stats
                warmupTasks.Add(WarmupDashboardStats(cacheService, productRepository, categoryRepository, cancellationToken));

                await Task.WhenAll(warmupTasks);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Cache warmup completed in {Duration}ms", duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache warmup");
            }
        }

        private async Task WarmupCategories(ICacheService cacheService, ICategoryRepository categoryRepository, CancellationToken cancellationToken)
        {
            try
            {
                await cacheService.GetOrCreateAsync("categories:all", async () =>
                {
                    return await categoryRepository.GetRootCategoriesWithChildrenAsync();
                }, TimeSpan.FromMinutes(_cacheSettings.LongTermExpiryMinutes));

                _logger.LogDebug("Categories cache warmed up");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warm up categories cache");
            }
        }

        private async Task WarmupFeaturedProducts(ICacheService cacheService, IProductRepository productRepository, CancellationToken cancellationToken)
        {
            try
            {
                await cacheService.GetOrCreateAsync("products:featured", async () =>
                {
                    return await productRepository.GetFeaturedProductsAsync(20);
                }, TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));

                _logger.LogDebug("Featured products cache warmed up");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warm up featured products cache");
            }
        }

        private async Task WarmupTopRatedProducts(ICacheService cacheService, IProductRepository productRepository, CancellationToken cancellationToken)
        {
            try
            {
                await cacheService.GetOrCreateAsync("products:top-rated", async () =>
                {
                    return await productRepository.GetTopRatedProductsAsync(20);
                }, TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));

                _logger.LogDebug("Top-rated products cache warmed up");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warm up top-rated products cache");
            }
        }

        private async Task WarmupBanners(ICacheService cacheService, IBannerService bannerService, CancellationToken cancellationToken)
        {
            try
            {
                await cacheService.GetOrCreateAsync("banners:active", async () =>
                {
                    return await bannerService.GetActiveBannersAsync();
                }, TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));

                _logger.LogDebug("Banners cache warmed up");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warm up banners cache");
            }
        }

        private async Task WarmupDashboardStats(ICacheService cacheService, IProductRepository productRepository, ICategoryRepository categoryRepository, CancellationToken cancellationToken)
        {
            try
            {
                await cacheService.GetOrCreateAsync("dashboard:stats", async () =>
                {
                    var totalProducts = await productRepository.CountAsync();
                    var totalCategories = await categoryRepository.CountAsync();
                    var lowStockProducts = await productRepository.GetLowStockProductsAsync(10);

                    return new
                    {
                        TotalProducts = totalProducts,
                        TotalCategories = totalCategories,
                        LowStockCount = lowStockProducts.Count(),
                        LastUpdated = DateTime.UtcNow
                    };
                }, TimeSpan.FromMinutes(_cacheSettings.ShortTermExpiryMinutes));

                _logger.LogDebug("Dashboard stats cache warmed up");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warm up dashboard stats cache");
            }
        }
    }

    public class CacheCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<CacheCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval;

        public CacheCleanupService(
            IServiceProvider serviceProvider,
            IOptions<CacheSettings> cacheSettings,
            ILogger<CacheCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
            _cleanupInterval = TimeSpan.FromHours(1); // Cleanup every hour
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cache cleanup service started");

            using var timer = new PeriodicTimer(_cleanupInterval);
            
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PerformCleanup(stoppingToken);
            }
        }

        private async Task PerformCleanup(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting cache cleanup process");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var statistics = cacheService.GetStatistics();
                _logger.LogInformation("Cache statistics before cleanup: {Stats}", System.Text.Json.JsonSerializer.Serialize(statistics));

                // Get cache key information
                var keyInfos = cacheService.GetKeyInfos();
                var expiredKeys = keyInfos
                    .Where(ki => ki.CreatedAt.AddMinutes(_cacheSettings.DefaultExpirationMinutes) < DateTime.UtcNow)
                    .Select(ki => ki.Key)
                    .ToList();

                // Remove expired keys
                foreach (var key in expiredKeys)
                {
                    cacheService.Remove(key.Replace($"{_cacheSettings.CacheKeyPrefix}:", ""));
                }

                if (expiredKeys.Any())
                {
                    _logger.LogInformation("Cleaned up {Count} expired cache keys", expiredKeys.Count);
                }

                // Check if we need to clear cache due to memory pressure
                if (statistics.MemoryCacheSize > _cacheSettings.MaxCacheSizeMB * 1024 * 1024)
                {
                    _logger.LogWarning("Cache size ({Size}MB) exceeds limit ({Limit}MB), performing aggressive cleanup", 
                        statistics.MemoryCacheSize / (1024 * 1024), _cacheSettings.MaxCacheSizeMB);
                    
                    // Remove least accessed items
                    var leastAccessedKeys = keyInfos
                        .OrderBy(ki => ki.AccessCount)
                        .Take(statistics.TotalKeys / 4) // Remove 25% of cache
                        .Select(ki => ki.Key)
                        .ToList();

                    foreach (var key in leastAccessedKeys)
                    {
                        cacheService.Remove(key.Replace($"{_cacheSettings.CacheKeyPrefix}:", ""));
                    }

                    _logger.LogInformation("Removed {Count} least accessed cache keys due to memory pressure", leastAccessedKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache cleanup");
            }
        }
    }

    public class PerformanceMonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly TimeSpan _monitoringInterval;

        public PerformanceMonitoringService(
            IServiceProvider serviceProvider,
            ILogger<PerformanceMonitoringService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _monitoringInterval = TimeSpan.FromMinutes(5); // Monitor every 5 minutes
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Performance monitoring service started");

            using var timer = new PeriodicTimer(_monitoringInterval);
            
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await MonitorPerformance(stoppingToken);
            }
        }

        private async Task MonitorPerformance(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var statistics = cacheService.GetStatistics();
                
                // Log performance metrics
                _logger.LogInformation("Performance Metrics - Cache Hit Ratio: {HitRatio:F2}%, Total Requests: {TotalRequests}, Cache Size: {CacheSize}KB", 
                    statistics.HitRatio, statistics.TotalRequests, statistics.MemoryCacheSize / 1024);

                // Alert on poor performance
                if (statistics.HitRatio < 50 && statistics.TotalRequests > 100)
                {
                    _logger.LogWarning("Low cache hit ratio detected: {HitRatio:F2}%", statistics.HitRatio);
                }

                // Monitor memory usage
                var workingSet = GC.GetTotalMemory(false);
                _logger.LogDebug("Current memory usage: {MemoryUsage}MB", workingSet / (1024 * 1024));

                if (workingSet > 512 * 1024 * 1024) // 512MB
                {
                    _logger.LogWarning("High memory usage detected: {MemoryUsage}MB", workingSet / (1024 * 1024));
                    GC.Collect(); // Force garbage collection
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during performance monitoring");
            }
        }
    }
}
