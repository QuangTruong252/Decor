using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using DecorStore.API.Configuration;
using DecorStore.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DecorStore.API.Services.BackgroundServices
{
    /// <summary>
    /// Background service for collecting and monitoring performance metrics
    /// </summary>
    public class PerformanceMonitoringBackgroundService : BackgroundService
    {
        private readonly ILogger<PerformanceMonitoringBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(5); // Monitor every 5 minutes

        // Performance metrics storage
        private readonly Dictionary<string, PerformanceMetric> _metrics = new();
        private readonly object _metricsLock = new object();

        public PerformanceMonitoringBackgroundService(
            ILogger<PerformanceMonitoringBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Performance monitoring background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectPerformanceMetrics();
                    await Task.Delay(_monitoringInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Performance monitoring background service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during performance monitoring");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute before retry
                }
            }
        }

        private async Task CollectPerformanceMetrics()
        {
            _logger.LogDebug("Collecting performance metrics");

            using var scope = _serviceProvider.CreateScope();

            var metricsToCollect = new List<Task>
            {
                CollectCacheMetrics(scope),
                CollectDatabaseMetrics(scope),
                CollectMemoryMetrics(),
                CollectApplicationMetrics(scope)
            };

            await Task.WhenAll(metricsToCollect);

            await CheckPerformanceThresholds();

            _logger.LogDebug("Performance metrics collection completed");
        }

        private async Task CollectCacheMetrics(IServiceScope scope)
        {
            try
            {
                var cacheService = scope.ServiceProvider.GetService<ICacheService>();
                if (cacheService != null)
                {
                    var stats = cacheService.GetStatistics();
                    
                    lock (_metricsLock)
                    {
                        _metrics["cache.hit_ratio"] = new PerformanceMetric
                        {
                            Name = "Cache Hit Ratio",
                            Value = stats.HitRatio,
                            Unit = "%",
                            Timestamp = DateTime.UtcNow,
                            Category = "Cache"
                        };

                        _metrics["cache.total_requests"] = new PerformanceMetric
                        {
                            Name = "Cache Total Requests",
                            Value = stats.TotalRequests,
                            Unit = "count",
                            Timestamp = DateTime.UtcNow,
                            Category = "Cache"
                        };

                        _metrics["cache.memory_size"] = new PerformanceMetric
                        {
                            Name = "Cache Memory Size",
                            Value = stats.MemoryCacheSize,
                            Unit = "bytes",
                            Timestamp = DateTime.UtcNow,
                            Category = "Cache"
                        };
                    }
                }

                var distributedCacheService = scope.ServiceProvider.GetService<IDistributedCacheService>();
                if (distributedCacheService != null)
                {
                    // Test distributed cache connectivity
                    var testKey = "health_check_" + DateTime.UtcNow.Ticks;
                    await distributedCacheService.SetAsync(testKey, "test", TimeSpan.FromMinutes(1));
                    var retrieved = await distributedCacheService.GetAsync<string>(testKey);
                    await distributedCacheService.RemoveAsync(testKey);

                    lock (_metricsLock)
                    {
                        _metrics["distributed_cache.availability"] = new PerformanceMetric
                        {
                            Name = "Distributed Cache Availability",
                            Value = retrieved == "test" ? 1 : 0,
                            Unit = "boolean",
                            Timestamp = DateTime.UtcNow,
                            Category = "Distributed Cache"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting cache metrics");
            }
        }

        private async Task CollectDatabaseMetrics(IServiceScope scope)
        {
            try
            {                // Get database performance monitor service
                var databaseMonitor = scope.ServiceProvider.GetService<IDatabasePerformanceMonitor>();
                if (databaseMonitor != null)
                {
                    var dbMetrics = await databaseMonitor.GetMetricsAsync();

                    lock (_metricsLock)
                    {
                        _metrics["database.avg_query_time"] = new PerformanceMetric
                        {
                            Name = "Average Query Time",
                            Value = dbMetrics.AverageQueryTimeMs,
                            Unit = "ms",
                            Timestamp = DateTime.UtcNow,
                            Category = "Database"
                        };

                        _metrics["database.slow_queries"] = new PerformanceMetric
                        {
                            Name = "Slow Queries Count",
                            Value = dbMetrics.SlowQueryCount,
                            Unit = "count",
                            Timestamp = DateTime.UtcNow,
                            Category = "Database"
                        };

                        _metrics["database.total_queries"] = new PerformanceMetric
                        {
                            Name = "Total Queries",
                            Value = dbMetrics.TotalQueryCount,
                            Unit = "count",
                            Timestamp = DateTime.UtcNow,
                            Category = "Database"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting database metrics");
            }
        }

        private async Task CollectMemoryMetrics()
        {
            try
            {
                var totalMemory = GC.GetTotalMemory(false);
                var gen0Collections = GC.CollectionCount(0);
                var gen1Collections = GC.CollectionCount(1);
                var gen2Collections = GC.CollectionCount(2);

                lock (_metricsLock)
                {
                    _metrics["memory.total_memory"] = new PerformanceMetric
                    {
                        Name = "Total Memory",
                        Value = totalMemory,
                        Unit = "bytes",
                        Timestamp = DateTime.UtcNow,
                        Category = "Memory"
                    };

                    _metrics["memory.gc_gen0"] = new PerformanceMetric
                    {
                        Name = "GC Gen 0 Collections",
                        Value = gen0Collections,
                        Unit = "count",
                        Timestamp = DateTime.UtcNow,
                        Category = "Memory"
                    };

                    _metrics["memory.gc_gen1"] = new PerformanceMetric
                    {
                        Name = "GC Gen 1 Collections",
                        Value = gen1Collections,
                        Unit = "count",
                        Timestamp = DateTime.UtcNow,
                        Category = "Memory"
                    };

                    _metrics["memory.gc_gen2"] = new PerformanceMetric
                    {
                        Name = "GC Gen 2 Collections",
                        Value = gen2Collections,
                        Unit = "count",
                        Timestamp = DateTime.UtcNow,
                        Category = "Memory"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting memory metrics");
            }
        }

        private async Task CollectApplicationMetrics(IServiceScope scope)
        {
            try
            {
                // Collect application-specific metrics
                lock (_metricsLock)
                {
                    _metrics["application.uptime"] = new PerformanceMetric
                    {
                        Name = "Application Uptime",
                        Value = Environment.TickCount64,
                        Unit = "ms",
                        Timestamp = DateTime.UtcNow,
                        Category = "Application"
                    };

                    _metrics["application.thread_count"] = new PerformanceMetric
                    {
                        Name = "Thread Count",
                        Value = System.Diagnostics.Process.GetCurrentProcess().Threads.Count,
                        Unit = "count",
                        Timestamp = DateTime.UtcNow,
                        Category = "Application"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting application metrics");
            }
        }

        private async Task CheckPerformanceThresholds()
        {
            try
            {
                lock (_metricsLock)
                {
                    // Check cache hit ratio
                    if (_metrics.TryGetValue("cache.hit_ratio", out var cacheHitRatio))
                    {
                        if (cacheHitRatio.Value < 70) // Alert if hit ratio below 70%
                        {
                            _logger.LogWarning("Cache hit ratio is below threshold: {HitRatio}%", cacheHitRatio.Value);
                        }
                    }

                    // Check average query time
                    if (_metrics.TryGetValue("database.avg_query_time", out var avgQueryTime))
                    {
                        if (avgQueryTime.Value > 1000) // Alert if average query time > 1 second
                        {
                            _logger.LogWarning("Average database query time is above threshold: {QueryTime}ms", avgQueryTime.Value);
                        }
                    }

                    // Check memory usage
                    if (_metrics.TryGetValue("memory.total_memory", out var totalMemory))
                    {
                        var memoryMB = totalMemory.Value / 1024 / 1024;
                        if (memoryMB > 512) // Alert if memory usage > 512MB
                        {
                            _logger.LogWarning("Memory usage is above threshold: {MemoryMB}MB", memoryMB);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking performance thresholds");
            }
        }

        public Dictionary<string, PerformanceMetric> GetCurrentMetrics()
        {
            lock (_metricsLock)
            {
                return new Dictionary<string, PerformanceMetric>(_metrics);
            }
        }
    }

    public class PerformanceMetric
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
