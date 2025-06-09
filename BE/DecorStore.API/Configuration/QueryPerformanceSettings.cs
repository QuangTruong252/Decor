namespace DecorStore.API.Configuration
{
    /// <summary>
    /// Configuration settings for query performance monitoring and optimization
    /// </summary>
    public class QueryPerformanceSettings
    {
        public const string SectionName = "QueryPerformance";

        /// <summary>
        /// Enable query performance monitoring
        /// </summary>
        public bool EnableQueryMonitoring { get; set; } = true;

        /// <summary>
        /// Threshold in milliseconds for slow query detection
        /// </summary>
        public int SlowQueryThresholdMs { get; set; } = 1000;

        /// <summary>
        /// Enable query execution plan logging
        /// </summary>
        public bool EnableExecutionPlanLogging { get; set; } = false;

        /// <summary>
        /// Query timeout settings by operation type
        /// </summary>
        public QueryTimeoutSettings Timeouts { get; set; } = new();

        /// <summary>
        /// Connection pool optimization settings
        /// </summary>
        public ConnectionPoolSettings ConnectionPool { get; set; } = new();

        /// <summary>
        /// Maximum number of slow queries to track
        /// </summary>
        public int MaxSlowQueriesToTrack { get; set; } = 100;

        /// <summary>
        /// Retention period for query performance data
        /// </summary>
        public TimeSpan DataRetentionPeriod { get; set; } = TimeSpan.FromDays(7);
    }

    /// <summary>
    /// Query timeout settings for different operation types
    /// </summary>
    public class QueryTimeoutSettings
    {
        /// <summary>
        /// Default query timeout in seconds
        /// </summary>
        public int DefaultTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Read operation timeout in seconds
        /// </summary>
        public int ReadTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Write operation timeout in seconds
        /// </summary>
        public int WriteTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Complex aggregation query timeout in seconds
        /// </summary>
        public int AggregationTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Report generation timeout in seconds
        /// </summary>
        public int ReportTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Bulk operation timeout in seconds
        /// </summary>
        public int BulkOperationTimeoutSeconds { get; set; } = 600;
    }

    /// <summary>
    /// Connection pool optimization settings
    /// </summary>
    public class ConnectionPoolSettings
    {
        /// <summary>
        /// Maximum number of connections in the pool
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// Minimum number of connections in the pool
        /// </summary>
        public int MinPoolSize { get; set; } = 5;

        /// <summary>
        /// Connection lifetime in minutes
        /// </summary>
        public int ConnectionLifetimeMinutes { get; set; } = 30;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable connection pool monitoring
        /// </summary>
        public bool EnablePoolMonitoring { get; set; } = true;

        /// <summary>
        /// Pool size warning threshold percentage
        /// </summary>
        public double PoolUsageWarningThreshold { get; set; } = 0.8;

        /// <summary>
        /// Enable connection retry on failure
        /// </summary>
        public bool EnableConnectionRetry { get; set; } = true;

        /// <summary>
        /// Maximum number of connection retries
        /// </summary>
        public int MaxConnectionRetries { get; set; } = 3;

        /// <summary>
        /// Retry delay in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;
    }
}
