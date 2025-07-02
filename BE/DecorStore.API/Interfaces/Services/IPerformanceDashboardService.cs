using DecorStore.API.Common;
using DecorStore.API.DTOs;

namespace DecorStore.API.Interfaces.Services
{
    /// <summary>
    /// Interface for performance dashboard service
    /// </summary>
    public interface IPerformanceDashboardService
    {
        Task<Result<PerformanceDashboardDTO>> GetPerformanceDashboardAsync();
        Task<Result<PerformanceMetricsDTO>> GetPerformanceMetricsAsync(DateTime? startDate, DateTime? endDate, int hours);
        Task<Result<List<EndpointPerformanceDTO>>> GetEndpointPerformanceAsync(int top, string orderBy);
        Task<Result<DatabasePerformanceDTO>> GetDatabasePerformanceAsync();
        Task<Result<CachePerformanceDTO>> GetCachePerformanceAsync();
        Task<Result<PerformanceTrendsDTO>> GetPerformanceTrendsAsync(int days, string granularity);
        Task<Result<ResourceUtilizationDTO>> GetResourceUtilizationAsync();
        Task<Result<List<SlowQueryDTO>>> GetSlowQueriesAsync(int top, int thresholdMs);
        Task<Result<string>> CleanupPerformanceDataAsync(int olderThanDays);
        Task<Result<byte[]>> ExportPerformanceDataAsync(DateTime? startDate, DateTime? endDate, string format);
        
        // Additional methods used by the controller
        Task<Result<DatabasePerformanceDTO>> GetDatabaseMetricsAsync();
        Task<Result<CachePerformanceDTO>> GetCacheMetricsAsync();
        Task<Result<PerformanceTrendsDTO>> GetPerformanceTrendsAsync(DateTime startDate, DateTime endDate, string granularity);
        Task<Result<SystemHealthDTO>> GetSystemHealthAsync();
    }
}
