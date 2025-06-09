using DecorStore.API.Common;
using DecorStore.API.Configuration;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for analyzing and monitoring database query performance
    /// </summary>
    public interface IQueryPerformanceAnalyzer
    {
        /// <summary>
        /// Records query execution metrics
        /// </summary>
        void RecordQuery(string query, TimeSpan executionTime, string? parameters = null);

        /// <summary>
        /// Gets slow queries above threshold
        /// </summary>
        Task<Result<List<SlowQueryDTO>>> GetSlowQueriesAsync(int top = 50);

        /// <summary>
        /// Gets query performance statistics
        /// </summary>
        Task<Result<QueryPerformanceStatsDTO>> GetQueryStatsAsync();

        /// <summary>
        /// Clears old query performance data
        /// </summary>
        Task<Result<string>> CleanupOldDataAsync();

        /// <summary>
        /// Gets query execution plan for analysis
        /// </summary>
        Task<Result<string>> GetExecutionPlanAsync(string query);
    }

    /// <summary>
    /// Implementation of query performance analyzer
    /// </summary>
    public class QueryPerformanceAnalyzer : IQueryPerformanceAnalyzer
    {
        private readonly QueryPerformanceSettings _settings;
        private readonly ILogger<QueryPerformanceAnalyzer> _logger;
        private readonly ConcurrentDictionary<string, QueryMetrics> _queryMetrics;
        private readonly ConcurrentQueue<SlowQueryRecord> _slowQueries;

        public QueryPerformanceAnalyzer(
            IOptions<QueryPerformanceSettings> settings,
            ILogger<QueryPerformanceAnalyzer> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _queryMetrics = new ConcurrentDictionary<string, QueryMetrics>();
            _slowQueries = new ConcurrentQueue<SlowQueryRecord>();
        }

        public void RecordQuery(string query, TimeSpan executionTime, string? parameters = null)
        {
            if (!_settings.EnableQueryMonitoring)
                return;

            try
            {
                var normalizedQuery = NormalizeQuery(query);
                var executionTimeMs = (long)executionTime.TotalMilliseconds;

                // Update query metrics
                _queryMetrics.AddOrUpdate(normalizedQuery, 
                    new QueryMetrics
                    {
                        Query = normalizedQuery,
                        ExecutionCount = 1,
                        TotalExecutionTimeMs = executionTimeMs,
                        MinExecutionTimeMs = executionTimeMs,
                        MaxExecutionTimeMs = executionTimeMs,
                        LastExecuted = DateTime.UtcNow
                    },
                    (key, existing) =>
                    {
                        existing.ExecutionCount++;
                        existing.TotalExecutionTimeMs += executionTimeMs;
                        existing.MinExecutionTimeMs = Math.Min(existing.MinExecutionTimeMs, executionTimeMs);
                        existing.MaxExecutionTimeMs = Math.Max(existing.MaxExecutionTimeMs, executionTimeMs);
                        existing.LastExecuted = DateTime.UtcNow;
                        return existing;
                    });

                // Record slow queries
                if (executionTimeMs >= _settings.SlowQueryThresholdMs)
                {
                    var slowQuery = new SlowQueryRecord
                    {
                        Query = query,
                        ExecutionTimeMs = executionTimeMs,
                        Parameters = parameters,
                        ExecutedAt = DateTime.UtcNow
                    };

                    _slowQueries.Enqueue(slowQuery);

                    // Keep only the most recent slow queries
                    while (_slowQueries.Count > _settings.MaxSlowQueriesToTrack)
                    {
                        _slowQueries.TryDequeue(out _);
                    }

                    _logger.LogWarning("Slow query detected: {Query} took {ExecutionTime}ms", 
                        normalizedQuery, executionTimeMs);

                    // Log execution plan if enabled
                    if (_settings.EnableExecutionPlanLogging)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var planResult = await GetExecutionPlanAsync(query);
                                if (planResult.IsSuccess)
                                {
                                    _logger.LogInformation("Execution plan for slow query: {ExecutionPlan}", 
                                        planResult.Data);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to get execution plan for slow query");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording query performance metrics");
            }
        }

        public async Task<Result<List<SlowQueryDTO>>> GetSlowQueriesAsync(int top = 50)
        {
            try
            {
                var slowQueries = _slowQueries
                    .OrderByDescending(q => q.ExecutedAt)
                    .Take(top)
                    .GroupBy(q => NormalizeQuery(q.Query))
                    .Select(g => new SlowQueryDTO
                    {
                        Query = g.Key,
                        ExecutionTimeMs = (long)g.Average(q => q.ExecutionTimeMs),
                        ExecutionCount = g.Count(),
                        LastExecuted = g.Max(q => q.ExecutedAt),
                        Parameters = g.FirstOrDefault()?.Parameters ?? ""
                    })
                    .OrderByDescending(q => q.ExecutionTimeMs)
                    .ToList();

                return Result<List<SlowQueryDTO>>.Success(slowQueries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slow queries");
                return Result<List<SlowQueryDTO>>.Failure("Failed to get slow queries");
            }
        }

        public async Task<Result<QueryPerformanceStatsDTO>> GetQueryStatsAsync()
        {
            try
            {
                var metrics = _queryMetrics.Values.ToList();
                var slowQueryCount = _slowQueries.Count;

                var stats = new QueryPerformanceStatsDTO
                {
                    TotalQueries = metrics.Sum(m => m.ExecutionCount),
                    UniqueQueries = metrics.Count,
                    SlowQueryCount = slowQueryCount,
                    AverageExecutionTimeMs = metrics.Any() 
                        ? metrics.Average(m => m.TotalExecutionTimeMs / (double)m.ExecutionCount) 
                        : 0,
                    FastestQueryTimeMs = metrics.Any() ? metrics.Min(m => m.MinExecutionTimeMs) : 0,
                    SlowestQueryTimeMs = metrics.Any() ? metrics.Max(m => m.MaxExecutionTimeMs) : 0,
                    QueriesPerMinute = CalculateQueriesPerMinute(),
                    TopSlowQueries = await GetTopSlowQueries(10),
                    TopFrequentQueries = GetTopFrequentQueries(10),
                    GeneratedAt = DateTime.UtcNow
                };

                return Result<QueryPerformanceStatsDTO>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting query performance statistics");
                return Result<QueryPerformanceStatsDTO>.Failure("Failed to get query statistics");
            }
        }

        public async Task<Result<string>> CleanupOldDataAsync()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow - _settings.DataRetentionPeriod;
                var removedCount = 0;

                // Clean up old query metrics
                var keysToRemove = _queryMetrics
                    .Where(kvp => kvp.Value.LastExecuted < cutoffTime)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    if (_queryMetrics.TryRemove(key, out _))
                        removedCount++;
                }

                // Clean up old slow query records
                var slowQueriesToKeep = new Queue<SlowQueryRecord>();
                while (_slowQueries.TryDequeue(out var slowQuery))
                {
                    if (slowQuery.ExecutedAt >= cutoffTime)
                    {
                        slowQueriesToKeep.Enqueue(slowQuery);
                    }
                    else
                    {
                        removedCount++;
                    }
                }

                // Re-add the queries we want to keep
                foreach (var query in slowQueriesToKeep)
                {
                    _slowQueries.Enqueue(query);
                }

                var message = $"Cleaned up {removedCount} old query performance records";
                _logger.LogInformation(message);

                return Result<string>.Success(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up query performance data");
                return Result<string>.Failure("Failed to cleanup query performance data");
            }
        }

        public async Task<Result<string>> GetExecutionPlanAsync(string query)
        {
            try
            {
                // Note: This is a simplified implementation
                // In a real scenario, you would execute EXPLAIN or similar commands
                // depending on your database provider
                
                var plan = $"Execution plan analysis for query:\n{query}\n\n" +
                          "Note: Detailed execution plan analysis requires database-specific implementation.\n" +
                          "Consider using SQL Server Management Studio, pgAdmin, or similar tools for detailed analysis.";

                return Result<string>.Success(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution plan");
                return Result<string>.Failure("Failed to get execution plan");
            }
        }

        private string NormalizeQuery(string query)
        {
            // Remove extra whitespace and normalize the query for grouping
            return System.Text.RegularExpressions.Regex.Replace(query.Trim(), @"\s+", " ");
        }

        private double CalculateQueriesPerMinute()
        {
            var recentQueries = _queryMetrics.Values
                .Where(m => m.LastExecuted >= DateTime.UtcNow.AddMinutes(-1))
                .Sum(m => m.ExecutionCount);

            return recentQueries;
        }

        private async Task<List<SlowQueryDTO>> GetTopSlowQueries(int count)
        {
            var result = await GetSlowQueriesAsync(count);
            return result.IsSuccess ? result.Data : new List<SlowQueryDTO>();
        }

        private List<FrequentQueryDTO> GetTopFrequentQueries(int count)
        {
            return _queryMetrics.Values
                .OrderByDescending(m => m.ExecutionCount)
                .Take(count)
                .Select(m => new FrequentQueryDTO
                {
                    Query = m.Query,
                    ExecutionCount = m.ExecutionCount,
                    AverageExecutionTimeMs = m.TotalExecutionTimeMs / (double)m.ExecutionCount,
                    LastExecuted = m.LastExecuted
                })
                .ToList();
        }
    }

    /// <summary>
    /// Internal class for tracking query metrics
    /// </summary>
    internal class QueryMetrics
    {
        public string Query { get; set; } = string.Empty;
        public long ExecutionCount { get; set; }
        public long TotalExecutionTimeMs { get; set; }
        public long MinExecutionTimeMs { get; set; }
        public long MaxExecutionTimeMs { get; set; }
        public DateTime LastExecuted { get; set; }
    }

    /// <summary>
    /// Internal class for tracking slow query records
    /// </summary>
    internal class SlowQueryRecord
    {
        public string Query { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
        public string? Parameters { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
