using Microsoft.Extensions.Logging;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for managing cache invalidation strategies and patterns
    /// </summary>
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly ICacheService _cacheService;
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<CacheInvalidationService> _logger;
        private readonly CacheSettings _cacheSettings;

        // Cache dependency mapping - defines which caches should be invalidated when data changes
        private readonly Dictionary<string, string[]> _invalidationRules = new()
        {
            ["product"] = new[] { "products:*", "categories:*", "dashboard:*" },
            ["category"] = new[] { "categories:*", "products:*", "dashboard:*" },
            ["order"] = new[] { "orders:*", "dashboard:*", "customers:*" },
            ["customer"] = new[] { "customers:*", "orders:*", "dashboard:*" },
            ["banner"] = new[] { "banners:*" },
            ["review"] = new[] { "reviews:*", "products:*", "dashboard:*" },
            ["cart"] = new[] { "carts:*" }
        };

        public CacheInvalidationService(
            ICacheService cacheService,
            IDistributedCacheService distributedCacheService,
            ILogger<CacheInvalidationService> logger,
            IOptions<CacheSettings> cacheSettings)
        {
            _cacheService = cacheService;
            _distributedCacheService = distributedCacheService;
            _logger = logger;
            _cacheSettings = cacheSettings.Value;
        }

        /// <summary>
        /// Invalidate cache when a product is created, updated, or deleted
        /// </summary>
        public async Task InvalidateProductCacheAsync(int? productId = null, int? categoryId = null)
        {
            var invalidationTasks = new List<Task>();

            // Invalidate product-specific caches
            if (productId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"product:{productId.Value}:*"));
            }

            // Invalidate category-related caches
            if (categoryId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"category:{categoryId.Value}:*"));
            }

            // Invalidate general product caches
            invalidationTasks.Add(InvalidateCacheByTagAsync("products"));
            invalidationTasks.Add(InvalidateCachePatternAsync("products:*"));

            // Invalidate related caches
            invalidationTasks.Add(InvalidateCachePatternAsync("dashboard:*"));

            await Task.WhenAll(invalidationTasks);
            
            _logger.LogInformation("Product cache invalidation completed for ProductId: {ProductId}, CategoryId: {CategoryId}", 
                productId, categoryId);
        }

        /// <summary>
        /// Invalidate cache when a category is created, updated, or deleted
        /// </summary>
        public async Task InvalidateCategoryCacheAsync(int? categoryId = null, int? parentCategoryId = null)
        {
            var invalidationTasks = new List<Task>();

            // Invalidate specific category cache
            if (categoryId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"category:{categoryId.Value}:*"));
            }

            // Invalidate parent category cache
            if (parentCategoryId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"category:{parentCategoryId.Value}:*"));
            }

            // Invalidate category hierarchy caches
            invalidationTasks.Add(InvalidateCacheByTagAsync("categories"));
            invalidationTasks.Add(InvalidateCachePatternAsync("categories:*"));

            // Invalidate related product caches since category structure affects product display
            invalidationTasks.Add(InvalidateCachePatternAsync("products:*"));

            await Task.WhenAll(invalidationTasks);
            
            _logger.LogInformation("Category cache invalidation completed for CategoryId: {CategoryId}, ParentCategoryId: {ParentCategoryId}", 
                categoryId, parentCategoryId);
        }

        /// <summary>
        /// Invalidate cache when an order is created, updated, or deleted
        /// </summary>
        public async Task InvalidateOrderCacheAsync(int? orderId = null, int? customerId = null)
        {
            var invalidationTasks = new List<Task>();

            // Invalidate specific order cache
            if (orderId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"order:{orderId.Value}:*"));
            }

            // Invalidate customer-related order caches
            if (customerId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"customer:{customerId.Value}:orders:*"));
            }

            // Invalidate general order caches
            invalidationTasks.Add(InvalidateCacheByTagAsync("orders"));
            invalidationTasks.Add(InvalidateCachePatternAsync("orders:*"));

            // Invalidate dashboard statistics
            invalidationTasks.Add(InvalidateCachePatternAsync("dashboard:*"));

            await Task.WhenAll(invalidationTasks);
            
            _logger.LogInformation("Order cache invalidation completed for OrderId: {OrderId}, CustomerId: {CustomerId}", 
                orderId, customerId);
        }

        /// <summary>
        /// Invalidate cache when a customer is created, updated, or deleted
        /// </summary>
        public async Task InvalidateCustomerCacheAsync(int? customerId = null)
        {
            var invalidationTasks = new List<Task>();

            // Invalidate specific customer cache
            if (customerId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"customer:{customerId.Value}:*"));
            }

            // Invalidate general customer caches
            invalidationTasks.Add(InvalidateCacheByTagAsync("customers"));
            invalidationTasks.Add(InvalidateCachePatternAsync("customers:*"));

            // Invalidate dashboard statistics
            invalidationTasks.Add(InvalidateCachePatternAsync("dashboard:*"));

            await Task.WhenAll(invalidationTasks);
            
            _logger.LogInformation("Customer cache invalidation completed for CustomerId: {CustomerId}", customerId);
        }

        /// <summary>
        /// Invalidate cache when a banner is created, updated, or deleted
        /// </summary>
        public async Task InvalidateBannerCacheAsync(int? bannerId = null)
        {
            var invalidationTasks = new List<Task>();

            // Invalidate specific banner cache
            if (bannerId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"banner:{bannerId.Value}:*"));
            }

            // Invalidate general banner caches
            invalidationTasks.Add(InvalidateCacheByTagAsync("banners"));
            invalidationTasks.Add(InvalidateCachePatternAsync("banners:*"));

            await Task.WhenAll(invalidationTasks);
            
            _logger.LogInformation("Banner cache invalidation completed for BannerId: {BannerId}", bannerId);
        }

        /// <summary>
        /// Invalidate cache when a review is created, updated, or deleted
        /// </summary>
        public async Task InvalidateReviewCacheAsync(int? reviewId = null, int? productId = null)
        {
            var invalidationTasks = new List<Task>();

            // Invalidate specific review cache
            if (reviewId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"review:{reviewId.Value}:*"));
            }

            // Invalidate product-related review caches
            if (productId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"product:{productId.Value}:reviews:*"));
                invalidationTasks.Add(InvalidateCachePatternAsync($"product:{productId.Value}:*")); // Product rating may change
            }

            // Invalidate general review caches
            invalidationTasks.Add(InvalidateCacheByTagAsync("reviews"));
            invalidationTasks.Add(InvalidateCachePatternAsync("reviews:*"));

            // Invalidate top-rated products cache
            invalidationTasks.Add(InvalidateCachePatternAsync("products:top-rated*"));

            await Task.WhenAll(invalidationTasks);
            
            _logger.LogInformation("Review cache invalidation completed for ReviewId: {ReviewId}, ProductId: {ProductId}", 
                reviewId, productId);
        }

        /// <summary>
        /// Invalidate cache when cart is updated
        /// </summary>
        public async Task InvalidateCartCacheAsync(int? customerId = null, string? sessionId = null)
        {
            var invalidationTasks = new List<Task>();

            // Invalidate customer-specific cart cache
            if (customerId.HasValue)
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"cart:customer:{customerId.Value}:*"));
            }

            // Invalidate session-specific cart cache
            if (!string.IsNullOrEmpty(sessionId))
            {
                invalidationTasks.Add(InvalidateCachePatternAsync($"cart:session:{sessionId}:*"));
            }

            // Invalidate general cart caches
            invalidationTasks.Add(InvalidateCacheByTagAsync("carts"));

            await Task.WhenAll(invalidationTasks);
            
            _logger.LogInformation("Cart cache invalidation completed for CustomerId: {CustomerId}, SessionId: {SessionId}", 
                customerId, sessionId);
        }

        /// <summary>
        /// Force refresh cache for specific keys
        /// </summary>
        public async Task RefreshCacheAsync(string[] cacheKeys)
        {
            foreach (var key in cacheKeys)
            {
                _cacheService.Remove(key);
                
                if (_distributedCacheService != null)
                {
                    await _distributedCacheService.RemoveAsync(key);
                }
            }

            _logger.LogInformation("Cache refresh completed for {Count} keys", cacheKeys.Length);
        }

        /// <summary>
        /// Warm up specific cache keys
        /// </summary>
        public async Task WarmUpCacheAsync(string[] cacheKeys)
        {
            if (!_cacheSettings.EnableCacheWarming)
            {
                _logger.LogDebug("Cache warming is disabled");
                return;
            }

            // This would trigger cache warming for specific keys
            // Implementation would depend on having access to the services that generate the cached data
            _logger.LogInformation("Cache warmup initiated for {Count} keys", cacheKeys.Length);
        }

        private async Task InvalidateCachePatternAsync(string pattern)
        {
            try
            {
                // Invalidate from memory cache
                _cacheService.RemoveByPrefix(pattern.Replace("*", ""));

                // Invalidate from distributed cache
                if (_distributedCacheService != null)
                {
                    await _distributedCacheService.RemoveByPatternAsync(pattern);
                }

                _logger.LogDebug("Invalidated cache pattern: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache pattern: {Pattern}", pattern);
            }
        }

        private async Task InvalidateCacheByTagAsync(string tag)
        {
            try
            {
                // Invalidate from memory cache
                _cacheService.RemoveByTag(tag);

                _logger.LogDebug("Invalidated cache by tag: {Tag}", tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache by tag: {Tag}", tag);
            }
        }
    }
}
