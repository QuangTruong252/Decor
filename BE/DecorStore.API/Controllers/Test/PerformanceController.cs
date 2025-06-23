using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Services;
using System.Diagnostics;
using System.Runtime;
using System.Security.Claims;

namespace DecorStore.API.Controllers.Test
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only admins can access performance metrics
    public class PerformanceController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<PerformanceController> _logger;

        public PerformanceController(
            ICacheService cacheService,
            IDistributedCacheService distributedCacheService,
            ILogger<PerformanceController> logger)
        {
            _cacheService = cacheService;
            _distributedCacheService = distributedCacheService;
            _logger = logger;
        }

        /// <summary>
        /// Get cache performance statistics
        /// </summary>
        [HttpGet("cache/statistics")]
        public ActionResult<CacheStatistics> GetCacheStatistics()
        {
            try
            {
                var statistics = _cacheService.GetStatistics();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get cache metrics (alias for cache/statistics for test compatibility)
        /// </summary>
        [HttpGet("cache")]
        public ActionResult<CacheStatistics> GetCacheMetrics()
        {
            return GetCacheStatistics();
        }

        /// <summary>
        /// Get cache key information
        /// </summary>
        [HttpGet("cache/keys")]
        public ActionResult<IEnumerable<CacheKeyInfo>> GetCacheKeys()
        {
            try
            {
                var keyInfos = _cacheService.GetKeyInfos();
                return Ok(keyInfos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache keys");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get system performance metrics
        /// </summary>
        [HttpGet("system")]
        public ActionResult<object> GetSystemMetrics()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var workingSet = GC.GetTotalMemory(false);
                var gcCollections = new[]
                {
                    GC.CollectionCount(0),
                    GC.CollectionCount(1),
                    GC.CollectionCount(2)
                };

                var metrics = new
                {
                    Memory = new
                    {
                        WorkingSetMB = workingSet / (1024 * 1024),
                        PrivateMemoryMB = process.PrivateMemorySize64 / (1024 * 1024),
                        VirtualMemoryMB = process.VirtualMemorySize64 / (1024 * 1024)
                    },
                    CPU = new
                    {
                        TotalProcessorTime = process.TotalProcessorTime.TotalMilliseconds,
                        UserProcessorTime = process.UserProcessorTime.TotalMilliseconds,
                        ProcessorCount = Environment.ProcessorCount
                    },
                    GarbageCollection = new
                    {
                        Gen0Collections = gcCollections[0],
                        Gen1Collections = gcCollections[1],
                        Gen2Collections = gcCollections[2],
                        TotalMemory = workingSet
                    },
                    Threading = new
                    {
                        ThreadCount = process.Threads.Count,
                        ThreadPoolWorkerThreads = ThreadPool.ThreadCount,
                        ThreadPoolCompletionPortThreads = ThreadPool.CompletedWorkItemCount
                    },
                    Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get Redis connection status and metrics
        /// </summary>
        [HttpGet("redis")]
        public async Task<ActionResult<object>> GetRedisMetrics()
        {
            try
            {
                var isConnected = await _distributedCacheService.IsConnectedAsync();
                var keysCount = await _distributedCacheService.GetKeysCountAsync();
                var sampleKeys = await _distributedCacheService.GetKeysAsync("*");

                var metrics = new
                {
                    IsConnected = isConnected,
                    KeysCount = keysCount,
                    SampleKeys = sampleKeys.Take(10), // Show first 10 keys as sample
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Redis metrics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Clear all cache (memory and distributed)
        /// </summary>
        [HttpPost("cache/clear")]
        public async Task<ActionResult> ClearCache()
        {
            try
            {
                _cacheService.Clear();
                await _distributedCacheService.ClearAsync();
                
                _logger.LogInformation("Cache cleared by admin user");
                return Ok(new { Message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Clear cache by prefix
        /// </summary>
        [HttpPost("cache/clear/{prefix}")]
        public async Task<ActionResult> ClearCacheByPrefix(string prefix)
        {
            try
            {
                _cacheService.RemoveByPrefix(prefix);
                await _distributedCacheService.RemoveByPatternAsync($"{prefix}*");
                
                _logger.LogInformation("Cache cleared for prefix {Prefix} by admin user", prefix);
                return Ok(new { Message = $"Cache cleared for prefix '{prefix}' successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache by prefix {Prefix}", prefix);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Warm up cache manually
        /// </summary>
        [HttpPost("cache/warmup")]
        public ActionResult WarmUpCache()
        {
            try
            {
                _cacheService.WarmUp();
                
                _logger.LogInformation("Cache warmup triggered by admin user");
                return Ok(new { Message = "Cache warmup initiated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up cache");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Force garbage collection
        /// </summary>
        [HttpPost("system/gc")]
        public ActionResult ForceGarbageCollection()
        {
            try
            {
                var beforeMemory = GC.GetTotalMemory(false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var afterMemory = GC.GetTotalMemory(true);

                var result = new
                {
                    BeforeMemoryMB = beforeMemory / (1024 * 1024),
                    AfterMemoryMB = afterMemory / (1024 * 1024),
                    FreedMemoryMB = (beforeMemory - afterMemory) / (1024 * 1024),
                    CollectedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Garbage collection forced by admin user, freed {FreedMB}MB", result.FreedMemoryMB);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing garbage collection");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get performance dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetPerformanceDashboard()
        {
            try
            {
                var cacheStats = _cacheService.GetStatistics();
                dynamic systemMetrics = GetSystemMetricsInternal();
                var redisConnected = await _distributedCacheService.IsConnectedAsync();
                var redisKeysCount = await _distributedCacheService.GetKeysCountAsync();

                var dashboard = new
                {
                    Cache = new
                    {
                        HitRatio = cacheStats.HitRatio,
                        TotalRequests = cacheStats.TotalRequests,
                        TotalKeys = cacheStats.TotalKeys,
                        MemorySizeMB = cacheStats.MemoryCacheSize / (1024 * 1024)
                    },
                    System = new
                    {
                        MemoryUsageMB = systemMetrics.MemoryUsageMB,
                        CpuTime = systemMetrics.CpuTime,
                        ThreadCount = systemMetrics.ThreadCount,
                        UptimeHours = systemMetrics.UptimeHours
                    },
                    Redis = new
                    {
                        IsConnected = redisConnected,
                        KeysCount = redisKeysCount
                    },
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance dashboard");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get database performance metrics
        /// </summary>
        [HttpGet("database")]
        public ActionResult<object> GetDatabaseMetrics()
        {
            try
            {
                // Mock database metrics for now
                var metrics = new
                {
                    ConnectionPoolSize = 10,
                    ActiveConnections = 3,
                    AverageQueryTimeMs = 45.2,
                    SlowQueryCount = 2,
                    TotalQueries = 1250,
                    DatabaseSizeMB = 512,
                    IndexHitRatio = 0.95,
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database metrics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get health check status
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous] // Health check should be accessible without authentication
        public ActionResult<object> GetHealthCheck()
        {
            try
            {
                var health = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
                };

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health check");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Test JWT authentication (requires any authenticated user)
        /// </summary>
        [HttpGet("auth-test")]
        [Authorize] // Only requires authentication, not specific roles
        public ActionResult<object> GetAuthTest()
        {
            try
            {
                var user = HttpContext.User;
                var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();

                var authInfo = new
                {
                    IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                    Name = user.Identity?.Name,
                    UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    Email = user.FindFirst(ClaimTypes.Email)?.Value,
                    Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
                    Claims = claims,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(authInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auth test info");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get API performance metrics
        /// </summary>
        [HttpGet("api")]
        public ActionResult<object> GetApiMetrics()
        {
            try
            {
                // Mock API metrics for now
                var metrics = new
                {
                    TotalRequests = 15420,
                    RequestsPerSecond = 12.5,
                    AverageResponseTimeMs = 125.3,
                    ErrorRate = 0.02,
                    EndpointMetrics = new[]
                    {
                        new { Endpoint = "/api/Products", RequestCount = 5200, AvgResponseMs = 95.2 },
                        new { Endpoint = "/api/Categories", RequestCount = 3100, AvgResponseMs = 78.5 },
                        new { Endpoint = "/api/Auth/login", RequestCount = 2800, AvgResponseMs = 156.7 }
                    },
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API metrics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get memory usage information
        /// </summary>
        [HttpGet("memory")]
        public ActionResult<object> GetMemoryUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var workingSet = GC.GetTotalMemory(false);

                var memory = new
                {
                    WorkingSetMB = process.WorkingSet64 / (1024 * 1024),
                    PrivateMemoryMB = process.PrivateMemorySize64 / (1024 * 1024),
                    VirtualMemoryMB = process.VirtualMemorySize64 / (1024 * 1024),
                    GCMemoryMB = workingSet / (1024 * 1024),
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(memory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memory usage");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get garbage collection information
        /// </summary>
        [HttpGet("gc")]
        public ActionResult<object> GetGarbageCollectionInfo()
        {
            try
            {
                var gcInfo = new
                {
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    TotalMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024),
                    MaxGeneration = GC.MaxGeneration,
                    IsServerGC = GCSettings.IsServerGC,
                    LatencyMode = GCSettings.LatencyMode.ToString(),
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(gcInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting garbage collection info");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get thread pool information
        /// </summary>
        [HttpGet("threads")]
        public ActionResult<object> GetThreadPoolInfo()
        {
            try
            {
                ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
                ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);

                var process = Process.GetCurrentProcess();

                var threadInfo = new
                {
                    ProcessThreadCount = process.Threads.Count,
                    ThreadPoolWorkerThreads = new
                    {
                        Available = availableWorkerThreads,
                        Max = maxWorkerThreads,
                        Min = minWorkerThreads,
                        InUse = maxWorkerThreads - availableWorkerThreads
                    },
                    ThreadPoolCompletionPortThreads = new
                    {
                        Available = availableCompletionPortThreads,
                        Max = maxCompletionPortThreads,
                        Min = minCompletionPortThreads,
                        InUse = maxCompletionPortThreads - availableCompletionPortThreads
                    },
                    CompletedWorkItems = ThreadPool.CompletedWorkItemCount,
                    PendingWorkItems = ThreadPool.PendingWorkItemCount,
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(threadInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting thread pool info");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get request metrics
        /// </summary>
        [HttpGet("requests")]
        public ActionResult<object> GetRequestMetrics()
        {
            try
            {
                // Mock request metrics for now
                var metrics = new
                {
                    TotalRequests = 25680,
                    RequestsPerMinute = 45.2,
                    AverageResponseTimeMs = 142.5,
                    MedianResponseTimeMs = 98.3,
                    P95ResponseTimeMs = 285.7,
                    P99ResponseTimeMs = 456.2,
                    ErrorCount = 23,
                    ErrorRate = 0.0009,
                    StatusCodeDistribution = new
                    {
                        Status200 = 24890,
                        Status400 = 12,
                        Status401 = 156,
                        Status404 = 89,
                        Status500 = 8
                    },
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting request metrics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get comprehensive performance metrics
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<object>> GetPerformanceMetrics()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var workingSet = GC.GetTotalMemory(false);
                var cacheStats = _cacheService.GetStatistics();
                var redisConnected = await _distributedCacheService.IsConnectedAsync();

                var metrics = new
                {
                    System = new
                    {
                        MemoryUsageMB = workingSet / (1024 * 1024),
                        CpuTimeMs = process.TotalProcessorTime.TotalMilliseconds,
                        ThreadCount = process.Threads.Count,
                        UptimeHours = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalHours
                    },
                    Cache = new
                    {
                        HitRatio = cacheStats.HitRatio,
                        TotalRequests = cacheStats.TotalRequests,
                        TotalKeys = cacheStats.TotalKeys
                    },
                    Redis = new
                    {
                        IsConnected = redisConnected
                    },
                    GarbageCollection = new
                    {
                        Gen0Collections = GC.CollectionCount(0),
                        Gen1Collections = GC.CollectionCount(1),
                        Gen2Collections = GC.CollectionCount(2)
                    },
                    CollectedAt = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return StatusCode(500, "Internal server error");
            }
        }

        private object GetSystemMetricsInternal()
        {
            var process = Process.GetCurrentProcess();
            var workingSet = GC.GetTotalMemory(false);

            return new
            {
                MemoryUsageMB = workingSet / (1024 * 1024),
                CpuTime = process.TotalProcessorTime.TotalMilliseconds,
                ThreadCount = process.Threads.Count,
                UptimeHours = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalHours
            };
        }
    }
}
