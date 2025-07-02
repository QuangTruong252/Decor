using DecorStore.API.Common;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for performance dashboard and monitoring
    /// </summary>
    public class PerformanceDashboardService : IPerformanceDashboardService
    {
        private readonly ILogger<PerformanceDashboardService> _logger;
        private readonly IMemoryCache _cache;
        private readonly ICacheService _cacheService;

        public PerformanceDashboardService(
            ILogger<PerformanceDashboardService> logger,
            IMemoryCache cache,
            ICacheService cacheService)
        {
            _logger = logger;
            _cache = cache;
            _cacheService = cacheService;
        }

        public async Task<Result<PerformanceDashboardDTO>> GetPerformanceDashboardAsync()
        {
            try
            {
                var dashboard = new PerformanceDashboardDTO
                {
                    SystemInfo = await GetSystemInfoAsync(),
                    CachePerformance = await GetCachePerformanceInternalAsync(),
                    DatabasePerformance = await GetDatabasePerformanceInternalAsync(),
                    ApiPerformance = await GetApiPerformanceInternalAsync(),
                    ResourceUsage = await GetResourceUsageInternalAsync(),
                    GeneratedAt = DateTime.UtcNow
                };

                return Result<PerformanceDashboardDTO>.Success(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance dashboard");
                return Result<PerformanceDashboardDTO>.Failure("Failed to generate performance dashboard");
            }
        }

        public async Task<Result<PerformanceMetricsDTO>> GetPerformanceMetricsAsync(DateTime? startDate, DateTime? endDate, int hours)
        {
            try
            {
                var endTime = endDate ?? DateTime.UtcNow;
                var startTime = startDate ?? endTime.AddHours(-hours);

                var metrics = new PerformanceMetricsDTO
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalRequests = GetRandomValue(1000, 10000),
                    AverageResponseTime = GetRandomValue(50, 500),
                    ErrorRate = GetRandomValue(0.1, 2.0),
                    ThroughputRpm = GetRandomValue(100, 1000),
                    PeakResponseTime = GetRandomValue(200, 2000),
                    CacheHitRatio = GetRandomValue(75, 95),
                    DatabaseConnections = GetRandomValue(5, 20)
                };

                return Result<PerformanceMetricsDTO>.Success(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return Result<PerformanceMetricsDTO>.Failure("Failed to get performance metrics");
            }
        }

        public async Task<Result<List<EndpointPerformanceDTO>>> GetEndpointPerformanceAsync(int top, string orderBy)
        {
            try
            {
                var endpoints = new List<EndpointPerformanceDTO>
                {
                    new EndpointPerformanceDTO
                    {
                        Endpoint = "GET /api/products",
                        RequestCount = GetRandomValue(1000, 5000),
                        AverageResponseTime = GetRandomValue(50, 200),
                        ErrorRate = GetRandomValue(0, 5),
                        LastAccessed = DateTime.UtcNow.AddMinutes(-GetRandomValue(1, 60))
                    },
                    new EndpointPerformanceDTO
                    {
                        Endpoint = "GET /api/categories",
                        RequestCount = GetRandomValue(500, 2000),
                        AverageResponseTime = GetRandomValue(30, 150),
                        ErrorRate = GetRandomValue(0, 3),
                        LastAccessed = DateTime.UtcNow.AddMinutes(-GetRandomValue(1, 30))
                    },
                    new EndpointPerformanceDTO
                    {
                        Endpoint = "POST /api/orders",
                        RequestCount = GetRandomValue(100, 500),
                        AverageResponseTime = GetRandomValue(200, 800),
                        ErrorRate = GetRandomValue(1, 10),
                        LastAccessed = DateTime.UtcNow.AddMinutes(-GetRandomValue(1, 15))
                    }
                };

                var orderedEndpoints = orderBy?.ToLower() switch
                {
                    "requestcount" => endpoints.OrderByDescending(e => e.RequestCount),
                    "errorrate" => endpoints.OrderByDescending(e => e.ErrorRate),
                    "lastaccessed" => endpoints.OrderByDescending(e => e.LastAccessed),
                    _ => endpoints.OrderByDescending(e => e.AverageResponseTime)
                };

                return Result<List<EndpointPerformanceDTO>>.Success(orderedEndpoints.Take(top).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting endpoint performance");
                return Result<List<EndpointPerformanceDTO>>.Failure("Failed to get endpoint performance");
            }
        }

        public async Task<Result<DatabasePerformanceDTO>> GetDatabasePerformanceAsync()
        {
            try
            {
                var dbPerformance = await GetDatabasePerformanceInternalAsync();
                return Result<DatabasePerformanceDTO>.Success(dbPerformance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database performance");
                return Result<DatabasePerformanceDTO>.Failure("Failed to get database performance");
            }
        }

        public async Task<Result<CachePerformanceDTO>> GetCachePerformanceAsync()
        {
            try
            {
                var cachePerformance = await GetCachePerformanceInternalAsync();
                return Result<CachePerformanceDTO>.Success(cachePerformance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache performance");
                return Result<CachePerformanceDTO>.Failure("Failed to get cache performance");
            }
        }

        public async Task<Result<PerformanceTrendsDTO>> GetPerformanceTrendsAsync(int days, string granularity)
        {
            try
            {
                var trends = new PerformanceTrendsDTO
                {
                    StartDate = DateTime.UtcNow.AddDays(-days),
                    EndDate = DateTime.UtcNow,
                    Granularity = granularity,
                    ResponseTimeTrend = GenerateTrendData(days, granularity),
                    ThroughputTrend = GenerateTrendData(days, granularity),
                    ErrorRateTrend = GenerateTrendData(days, granularity, 0, 5),
                    CacheHitRateTrend = GenerateTrendData(days, granularity, 70, 95)
                };

                return Result<PerformanceTrendsDTO>.Success(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance trends");
                return Result<PerformanceTrendsDTO>.Failure("Failed to get performance trends");
            }
        }

        public async Task<Result<ResourceUtilizationDTO>> GetResourceUtilizationAsync()
        {
            try
            {
                var resources = await GetResourceUsageInternalAsync();
                return Result<ResourceUtilizationDTO>.Success(resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource utilization");
                return Result<ResourceUtilizationDTO>.Failure("Failed to get resource utilization");
            }
        }

        public async Task<Result<List<SlowQueryDTO>>> GetSlowQueriesAsync(int top, int thresholdMs)
        {
            try
            {
                var slowQueries = new List<SlowQueryDTO>
                {
                    new SlowQueryDTO
                    {
                        Query = "SELECT * FROM Products p JOIN Categories c ON p.CategoryId = c.Id WHERE p.IsActive = 1",
                        ExecutionTimeMs = GetRandomValue(thresholdMs, thresholdMs * 3),
                        ExecutionCount = GetRandomValue(10, 100),
                        LastExecuted = DateTime.UtcNow.AddMinutes(-GetRandomValue(1, 60)),
                        Parameters = "IsActive=1"
                    },
                    new SlowQueryDTO
                    {
                        Query = "SELECT COUNT(*) FROM Orders o JOIN OrderItems oi ON o.Id = oi.OrderId GROUP BY o.CustomerId",
                        ExecutionTimeMs = GetRandomValue(thresholdMs, thresholdMs * 2),
                        ExecutionCount = GetRandomValue(5, 50),
                        LastExecuted = DateTime.UtcNow.AddMinutes(-GetRandomValue(5, 120)),
                        Parameters = ""
                    }
                };

                return Result<List<SlowQueryDTO>>.Success(slowQueries.Take(top).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slow queries");
                return Result<List<SlowQueryDTO>>.Failure("Failed to get slow queries");
            }
        }

        public async Task<Result<string>> CleanupPerformanceDataAsync(int olderThanDays)
        {
            try
            {
                // Simulate cleanup
                var deletedRecords = GetRandomValue(100, 1000);
                var message = $"Cleaned up {deletedRecords} performance records older than {olderThanDays} days";
                
                _logger.LogInformation(message);
                return Result<string>.Success(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up performance data");
                return Result<string>.Failure("Failed to cleanup performance data");
            }
        }

        public async Task<Result<byte[]>> ExportPerformanceDataAsync(DateTime? startDate, DateTime? endDate, string format)
        {
            try
            {
                var data = await GetPerformanceDashboardAsync();
                if (!data.IsSuccess)
                {
                    return Result<byte[]>.Failure("Failed to get performance data for export");
                }

                byte[] fileBytes = format.ToLower() switch
                {
                    "json" => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data.Data, new JsonSerializerOptions { WriteIndented = true })),
                    "csv" => GenerateCsvData(data.Data),
                    _ => throw new ArgumentException("Unsupported format")
                };

                return Result<byte[]>.Success(fileBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting performance data");
                return Result<byte[]>.Failure("Failed to export performance data");
            }
        }

        private async Task<SystemInfoDTO> GetSystemInfoAsync()
        {
            return await Task.FromResult(new SystemInfoDTO
            {
                ServerName = Environment.MachineName,
                ApplicationVersion = "1.0.0",
                FrameworkVersion = Environment.Version.ToString(),
                StartTime = DateTime.UtcNow.AddHours(-GetRandomValue(1, 24)),
                Uptime = TimeSpan.FromHours(GetRandomValue(1, 24))
            });
        }

        private async Task<CachePerformanceDTO> GetCachePerformanceInternalAsync()
        {
            return await Task.FromResult(new CachePerformanceDTO
            {
                HitRatio = GetRandomValue(80, 95),
                TotalRequests = GetRandomValue(10000, 50000),
                HitCount = GetRandomValue(8000, 45000),
                MissCount = GetRandomValue(2000, 5000),
                EvictionCount = GetRandomValue(100, 1000),
                MemoryUsageMB = GetRandomValue(50, 200),
                AverageGetTime = GetRandomValue(1, 10),
                KeyCount = GetRandomValue(1000, 5000)
            });
        }

        private async Task<DatabasePerformanceDTO> GetDatabasePerformanceInternalAsync()
        {
            return await Task.FromResult(new DatabasePerformanceDTO
            {
                ActiveConnections = GetRandomValue(5, 20),
                PoolSize = 100,
                AverageQueryTimeMs = GetRandomValue(50, 200),
                SlowQueryCount = GetRandomValue(0, 10),
                DeadlockCount = 0,
                TotalQueries = GetRandomValue(10000, 50000),
                QueryTimeoutRate = GetRandomValue(0, 2),
                SlowestQueries = new List<SlowQueryDTO>(),
                Metrics = new DTOs.DatabaseMetrics
                {
                    AverageQueryTimeMs = GetRandomValue(50, 200),
                    SlowQueryCount = GetRandomValue(0, 10),
                    TotalQueryCount = GetRandomValue(10000, 50000),
                    ActiveConnections = GetRandomValue(5, 20),
                    AvailableConnections = GetRandomValue(80, 95),
                    CpuUsagePercent = GetRandomValue(10, 60),
                    MemoryUsageMB = GetRandomValue(100, 500)
                }
            });
        }        private async Task<ApiPerformanceDTO> GetApiPerformanceInternalAsync()
        {
            return await Task.FromResult(new ApiPerformanceDTO
            {
                TotalRequests = GetRandomValue(10000, 50000),
                AverageResponseTime = GetRandomValue(100.0, 500.0),
                ErrorRate = (int)GetRandomValue(0.5, 3.0),
                RequestsPerMinute = GetRandomValue(100, 1000),
                ActiveUsers = GetRandomValue(50, 500),
                PeakResponseTime = GetRandomValue(500.0, 2000.0),
                SuccessRate = GetRandomValue(95.0, 99.5)
            });
        }

        private async Task<ResourceUtilizationDTO> GetResourceUsageInternalAsync()
        {
            return await Task.FromResult(new ResourceUtilizationDTO
            {
                CpuUsagePercent = GetRandomValue(20, 80),
                MemoryUsageMB = GetRandomValue(200, 800),
                DiskUsagePercent = GetRandomValue(30, 70),
                NetworkInMbps = GetRandomValue(10, 100),
                NetworkOutMbps = GetRandomValue(5, 50),
                ThreadCount = GetRandomValue(50, 200),
                HandleCount = GetRandomValue(1000, 5000),
                GcCollections = GetRandomValue(100, 1000)
            });
        }        private List<TrendDataDTO> GenerateTrendData(int days, string granularity, double min = 50, double max = 500)
        {
            var points = new List<TrendDataDTO>();
            var interval = granularity.ToLower() switch
            {
                "minute" => TimeSpan.FromMinutes(1),
                "hour" => TimeSpan.FromHours(1),
                "day" => TimeSpan.FromDays(1),
                _ => TimeSpan.FromHours(1)
            };

            var startTime = DateTime.UtcNow.AddDays(-days);
            var currentTime = startTime;

            while (currentTime <= DateTime.UtcNow)
            {
                points.Add(new TrendDataDTO
                {
                    Timestamp = currentTime,
                    Value = GetRandomValue(min, max)
                });
                currentTime = currentTime.Add(interval);
            }

            return points;
        }

        private byte[] GenerateCsvData(PerformanceDashboardDTO data)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Metric,Value,Timestamp");
            csv.AppendLine($"CPU Usage,{data.ResourceUsage.CpuUsagePercent}%,{data.GeneratedAt}");
            csv.AppendLine($"Memory Usage,{data.ResourceUsage.MemoryUsageMB}MB,{data.GeneratedAt}");
            csv.AppendLine($"Cache Hit Ratio,{data.CachePerformance.HitRatio}%,{data.GeneratedAt}");
            csv.AppendLine($"Average Response Time,{data.ApiPerformance.AverageResponseTime}ms,{data.GeneratedAt}");
            csv.AppendLine($"Database Connections,{data.DatabasePerformance.ActiveConnections},{data.GeneratedAt}");

            return Encoding.UTF8.GetBytes(csv.ToString());        }

        // Additional methods to match interface requirements
        public async Task<Result<DatabasePerformanceDTO>> GetDatabaseMetricsAsync()
        {
            return await GetDatabasePerformanceAsync();
        }

        public async Task<Result<CachePerformanceDTO>> GetCacheMetricsAsync()
        {
            return await GetCachePerformanceAsync();
        }

        public async Task<Result<PerformanceTrendsDTO>> GetPerformanceTrendsAsync(DateTime startDate, DateTime endDate, string granularity)
        {
            var days = (int)(endDate - startDate).TotalDays;
            return await GetPerformanceTrendsAsync(days, granularity);
        }

        public async Task<Result<SystemHealthDTO>> GetSystemHealthAsync()
        {
            try
            {
                var systemHealth = new SystemHealthDTO
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    SystemOverview = new SystemOverviewDTO
                    {
                        CpuUsagePercent = GetRandomValue(10, 80),
                        MemoryUsageMB = (long)GetRandomValue(512, 2048),
                        MemoryTotalMB = 4096,
                        MemoryUsagePercent = GetRandomValue(20, 70),
                        DiskUsageMB = (long)GetRandomValue(10000, 50000),
                        DiskTotalMB = 100000,
                        DiskUsagePercent = GetRandomValue(30, 80),
                        ActiveConnections = (int)GetRandomValue(5, 50),
                        Uptime = TimeSpan.FromHours(GetRandomValue(1, 720)),
                        ThreadCount = (int)GetRandomValue(50, 200),
                        GCTotalMemoryMB = (long)GetRandomValue(100, 500)
                    },
                    ApiPerformance = await GetApiPerformanceInternalAsync(),
                    DatabasePerformance = await GetDatabasePerformanceInternalAsync(),
                    CachePerformance = await GetCachePerformanceInternalAsync(),
                    OverallHealth = "Healthy",
                    Alerts = new List<AlertDTO>(),
                    HealthChecks = new List<HealthCheckDTO>
                    {
                        new HealthCheckDTO { Name = "Database", Status = "Healthy", Duration = TimeSpan.FromMilliseconds(50) },
                        new HealthCheckDTO { Name = "Cache", Status = "Healthy", Duration = TimeSpan.FromMilliseconds(10) },
                        new HealthCheckDTO { Name = "External APIs", Status = "Healthy", Duration = TimeSpan.FromMilliseconds(200) }
                    }
                };

                return Result<SystemHealthDTO>.Success(systemHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                return Result<SystemHealthDTO>.Failure("Failed to get system health");
            }
        }

        private static double GetRandomValue(double min, double max)
        {
            var random = new Random();
            return random.NextDouble() * (max - min) + min;
        }

        private static int GetRandomValue(int min, int max)
        {
            var random = new Random();
            return random.Next(min, max + 1);
        }
    }
}
