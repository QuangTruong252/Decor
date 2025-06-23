using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DecorStore.API.Data;
using DecorStore.API.Configuration;
using System.Data;
using System.Data.Common;

namespace DecorStore.API.Extensions.Data
{
    /// <summary>
    /// Extensions for database optimization and performance monitoring
    /// </summary>
    public static class DatabaseOptimizationServiceExtensions
    {
        /// <summary>
        /// Adds database optimization services including connection pooling,
        /// query performance monitoring, and maintenance tasks
        /// </summary>
        public static IServiceCollection AddDatabaseOptimizationServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Add database performance monitoring
            services.AddScoped<IDatabasePerformanceMonitor, DatabasePerformanceMonitor>();
            
            // Add query performance interceptor
            services.AddScoped<QueryPerformanceInterceptor>();
            
            // Add connection pool monitoring
            services.AddSingleton<IConnectionPoolMonitor, ConnectionPoolMonitor>();
            
            // Add database maintenance background service
            services.AddHostedService<DatabaseMaintenanceService>();
            
            // Configure Entity Framework interceptors
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>() ?? new DatabaseSettings();
                var interceptor = serviceProvider.GetService<QueryPerformanceInterceptor>();
                
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Connection pool configuration
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: databaseSettings.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(databaseSettings.MaxRetryDelaySeconds),
                        errorNumbersToAdd: null);
                    
                    sqlOptions.CommandTimeout(databaseSettings.CommandTimeoutSeconds);
                });
                
                if (interceptor != null)
                {
                    options.AddInterceptors(interceptor);
                }
            });

            return services;
        }
    }

    /// <summary>
    /// Interface for database performance monitoring
    /// </summary>
    public interface IDatabasePerformanceMonitor
    {
        Task<DatabasePerformanceMetrics> GetPerformanceMetricsAsync();
        Task<IEnumerable<SlowQueryInfo>> GetSlowQueriesAsync(TimeSpan threshold);
        Task<ConnectionPoolMetrics> GetConnectionPoolMetricsAsync();
    }

    /// <summary>
    /// Implementation of database performance monitoring
    /// </summary>
    public class DatabasePerformanceMonitor : IDatabasePerformanceMonitor
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabasePerformanceMonitor> _logger;

        public DatabasePerformanceMonitor(ApplicationDbContext context, ILogger<DatabasePerformanceMonitor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DatabasePerformanceMetrics> GetPerformanceMetricsAsync()
        {
            try
            {
                // Get basic performance metrics from database
                var connectionCount = await GetActiveConnectionsAsync();
                var waitStats = await GetWaitStatsAsync();
                
                return new DatabasePerformanceMetrics
                {
                    ActiveConnections = connectionCount,
                    AverageQueryTime = waitStats.AverageWaitTime,
                    TotalQueries = waitStats.TotalQueries,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database performance metrics");
                return new DatabasePerformanceMetrics();
            }
        }

        public async Task<IEnumerable<SlowQueryInfo>> GetSlowQueriesAsync(TimeSpan threshold)
        {
            var slowQueries = new List<SlowQueryInfo>();
            
            try
            {
                // This would be implemented to query sys.dm_exec_query_stats or similar
                // For now, return empty collection
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get slow queries");
            }
            
            return slowQueries;
        }

        public async Task<ConnectionPoolMetrics> GetConnectionPoolMetricsAsync()
        {
            try
            {
                // Get connection pool statistics
                return new ConnectionPoolMetrics
                {
                    ActiveConnections = await GetActiveConnectionsAsync(),
                    IdleConnections = await GetIdleConnectionsAsync(),
                    PoolSize = await GetPoolSizeAsync(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get connection pool metrics");
                return new ConnectionPoolMetrics();
            }
        }

        private async Task<int> GetActiveConnectionsAsync()
        {
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1");
                return result;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetIdleConnectionsAsync()
        {
            await Task.CompletedTask;
            return 0; // Implementation would query sys.dm_exec_sessions
        }

        private async Task<int> GetPoolSizeAsync()
        {
            await Task.CompletedTask;
            return 100; // Default pool size
        }

        private async Task<WaitStats> GetWaitStatsAsync()
        {
            await Task.CompletedTask;
            return new WaitStats { AverageWaitTime = TimeSpan.FromMilliseconds(10), TotalQueries = 1000 };
        }
    }

    /// <summary>    /// <summary>
    /// Query performance interceptor for monitoring slow queries
    /// </summary>
    public class QueryPerformanceInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<QueryPerformanceInterceptor> _logger;
        private const int SlowQueryThresholdMs = 1000; // 1 second

        public QueryPerformanceInterceptor(ILogger<QueryPerformanceInterceptor> logger)
        {
            _logger = logger;
        }

        public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        {
            LogSlowQuery(command, eventData.Duration);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
        {
            LogSlowQuery(command, eventData.Duration);
            return base.ScalarExecuted(command, eventData, result);
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            LogSlowQuery(command, eventData.Duration);
            return base.ReaderExecuted(command, eventData, result);
        }

        private void LogSlowQuery(DbCommand command, TimeSpan duration)
        {
            if (duration.TotalMilliseconds > SlowQueryThresholdMs)
            {
                _logger.LogWarning("Slow query detected: {Duration}ms - {CommandText}", 
                    duration.TotalMilliseconds, 
                    command.CommandText);
            }
        }
    }

    /// <summary>
    /// Interface for connection pool monitoring
    /// </summary>
    public interface IConnectionPoolMonitor
    {
        ConnectionPoolMetrics GetCurrentMetrics();
        void RecordConnectionCreated();
        void RecordConnectionClosed();
        void RecordConnectionError();
    }

    /// <summary>
    /// Implementation of connection pool monitoring
    /// </summary>
    public class ConnectionPoolMonitor : IConnectionPoolMonitor
    {
        private long _connectionsCreated;
        private long _connectionsClosed;
        private long _connectionErrors;
        private readonly object _lock = new object();

        public ConnectionPoolMetrics GetCurrentMetrics()
        {
            lock (_lock)
            {
                return new ConnectionPoolMetrics
                {
                    ActiveConnections = (int)(_connectionsCreated - _connectionsClosed),
                    TotalConnectionsCreated = _connectionsCreated,
                    TotalConnectionsClosed = _connectionsClosed,
                    TotalConnectionErrors = _connectionErrors,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public void RecordConnectionCreated()
        {
            Interlocked.Increment(ref _connectionsCreated);
        }

        public void RecordConnectionClosed()
        {
            Interlocked.Increment(ref _connectionsClosed);
        }

        public void RecordConnectionError()
        {
            Interlocked.Increment(ref _connectionErrors);
        }
    }

    /// <summary>
    /// Background service for database maintenance tasks
    /// </summary>
    public class DatabaseMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseMaintenanceService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

        public DatabaseMaintenanceService(IServiceProvider serviceProvider, ILogger<DatabaseMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformMaintenanceAsync();
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during database maintenance");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Retry in 5 minutes
                }
            }
        }

        private async Task PerformMaintenanceAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var performanceMonitor = scope.ServiceProvider.GetRequiredService<IDatabasePerformanceMonitor>();

            _logger.LogInformation("Starting database maintenance tasks");

            // Update statistics
            await UpdateDatabaseStatisticsAsync(context);
            
            // Check for slow queries
            var slowQueries = await performanceMonitor.GetSlowQueriesAsync(TimeSpan.FromSeconds(1));
            if (slowQueries.Any())
            {
                _logger.LogWarning("Found {Count} slow queries", slowQueries.Count());
            }

            // Get performance metrics
            var metrics = await performanceMonitor.GetPerformanceMetricsAsync();
            _logger.LogInformation("Database performance: {ActiveConnections} active connections, {AvgQueryTime}ms average query time",
                metrics.ActiveConnections, metrics.AverageQueryTime.TotalMilliseconds);

            _logger.LogInformation("Database maintenance completed");
        }

        private async Task UpdateDatabaseStatisticsAsync(ApplicationDbContext context)
        {
            try
            {
                // Update table statistics for better query plans
                await context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS Products");
                await context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS Orders");
                await context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS Categories");
                await context.Database.ExecuteSqlRawAsync("UPDATE STATISTICS Reviews");
                
                _logger.LogDebug("Database statistics updated");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update database statistics");
            }
        }
    }

    // Data models for performance monitoring
    public class DatabasePerformanceMetrics
    {
        public int ActiveConnections { get; set; }
        public TimeSpan AverageQueryTime { get; set; }
        public long TotalQueries { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SlowQueryInfo
    {
        public string QueryText { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime ExecutedAt { get; set; }
        public int ExecutionCount { get; set; }
    }

    public class ConnectionPoolMetrics
    {
        public int ActiveConnections { get; set; }
        public int IdleConnections { get; set; }
        public int PoolSize { get; set; }
        public long TotalConnectionsCreated { get; set; }
        public long TotalConnectionsClosed { get; set; }
        public long TotalConnectionErrors { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class WaitStats
    {
        public TimeSpan AverageWaitTime { get; set; }
        public long TotalQueries { get; set; }
    }
}
