using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using DecorStore.API.Configuration;
using DecorStore.API.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DecorStore.API.Services.BackgroundServices
{
    /// <summary>
    /// Background service for warming up cache with frequently accessed data
    /// </summary>
    public class CacheWarmupBackgroundService : BackgroundService
    {
        private readonly ILogger<CacheWarmupBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly CacheSettings _cacheSettings;
        private readonly TimeSpan _warmupInterval = TimeSpan.FromHours(4); // Run every 4 hours

        public CacheWarmupBackgroundService(
            ILogger<CacheWarmupBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<CacheSettings> cacheSettings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cacheSettings = cacheSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cache warmup background service started");

            // Initial warmup after startup delay
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WarmupCache();
                    await Task.Delay(_warmupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Cache warmup background service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cache warmup");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Wait 30 minutes before retry
                }
            }
        }

        private async Task WarmupCache()
        {
            if (!_cacheSettings.EnableCacheWarming)
            {
                _logger.LogDebug("Cache warming is disabled");
                return;
            }

            _logger.LogInformation("Starting cache warmup process");

            using var scope = _serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetService<ICacheService>();
            var productService = scope.ServiceProvider.GetService<IProductService>();
            var categoryService = scope.ServiceProvider.GetService<ICategoryService>();
            var bannerService = scope.ServiceProvider.GetService<IBannerService>();

            if (cacheService == null)
            {
                _logger.LogWarning("Cache service not available for warmup");
                return;
            }

            var warmupTasks = new List<Task>();

            // Warmup categories
            if (categoryService != null)
            {
                warmupTasks.Add(WarmupCategoriesAsync(categoryService, cacheService));
            }

            // Warmup featured products
            if (productService != null)
            {
                warmupTasks.Add(WarmupProductsAsync(productService, cacheService));
            }

            // Warmup active banners
            if (bannerService != null)
            {
                warmupTasks.Add(WarmupBannersAsync(bannerService, cacheService));
            }

            await Task.WhenAll(warmupTasks);

            _logger.LogInformation("Cache warmup process completed successfully");
        }

        private async Task WarmupCategoriesAsync(ICategoryService categoryService, ICacheService cacheService)
        {
            try
            {
                _logger.LogDebug("Warming up categories cache");
                
                // This will cache the categories
                await cacheService.GetOrCreateAsync("categories:all", 
                    async () =>
                    {
                        var result = await categoryService.GetAllCategoriesAsync();
                        return result.IsSuccess ? result.Data : null;
                    },
                    TimeSpan.FromMinutes(_cacheSettings.LongTermExpiryMinutes));

                // Cache root categories with children
                await cacheService.GetOrCreateAsync("categories:root-with-children",
                    async () =>
                    {
                        var result = await categoryService.GetRootCategoriesWithChildrenAsync();
                        return result.IsSuccess ? result.Data : null;
                    },
                    TimeSpan.FromMinutes(_cacheSettings.LongTermExpiryMinutes));

                _logger.LogDebug("Categories cache warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up categories cache");
            }
        }

        private async Task WarmupProductsAsync(IProductService productService, ICacheService cacheService)
        {
            try
            {
                _logger.LogDebug("Warming up products cache");

                // Cache featured products
                await cacheService.GetOrCreateAsync("products:featured",
                    async () =>
                    {
                        var result = await productService.GetFeaturedProductsAsync();
                        return result.IsSuccess ? result.Data : null;
                    },
                    TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));

                // Cache top-rated products
                await cacheService.GetOrCreateAsync("products:top-rated",
                    async () =>
                    {
                        var result = await productService.GetTopRatedProductsAsync(20);
                        return result.IsSuccess ? result.Data : null;
                    },
                    TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));

                _logger.LogDebug("Products cache warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up products cache");
            }
        }

        private async Task WarmupBannersAsync(IBannerService bannerService, ICacheService cacheService)
        {
            try
            {
                _logger.LogDebug("Warming up banners cache");

                await cacheService.GetOrCreateAsync("banners:active",
                    async () =>
                    {
                        var result = await bannerService.GetActiveBannersAsync();
                        return result.IsSuccess ? result.Data : null;
                    },
                    TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));

                _logger.LogDebug("Banners cache warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up banners cache");
            }
        }
    }
}
