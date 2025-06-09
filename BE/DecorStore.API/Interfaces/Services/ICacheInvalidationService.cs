using System.Threading.Tasks;

namespace DecorStore.API.Interfaces.Services
{
    /// <summary>
    /// Interface for cache invalidation service
    /// </summary>
    public interface ICacheInvalidationService
    {
        Task InvalidateProductCacheAsync(int? productId = null, int? categoryId = null);
        Task InvalidateCategoryCacheAsync(int? categoryId = null, int? parentCategoryId = null);
        Task InvalidateOrderCacheAsync(int? orderId = null, int? customerId = null);
        Task InvalidateCustomerCacheAsync(int? customerId = null);
        Task InvalidateBannerCacheAsync(int? bannerId = null);
        Task InvalidateReviewCacheAsync(int? reviewId = null, int? productId = null);
        Task InvalidateCartCacheAsync(int? customerId = null, string? sessionId = null);
        Task RefreshCacheAsync(string[] cacheKeys);
        Task WarmUpCacheAsync(string[] cacheKeys);
    }
}
