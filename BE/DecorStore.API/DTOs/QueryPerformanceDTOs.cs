namespace DecorStore.API.DTOs
{
    /// <summary>
    /// DTO for query performance statistics
    /// </summary>
    public class QueryPerformanceStatsDTO
    {
        /// <summary>
        /// Total number of queries executed
        /// </summary>
        public long TotalQueries { get; set; }

        /// <summary>
        /// Number of unique query patterns
        /// </summary>
        public int UniqueQueries { get; set; }

        /// <summary>
        /// Number of slow queries detected
        /// </summary>
        public int SlowQueryCount { get; set; }

        /// <summary>
        /// Average execution time across all queries in milliseconds
        /// </summary>
        public double AverageExecutionTimeMs { get; set; }

        /// <summary>
        /// Fastest query execution time in milliseconds
        /// </summary>
        public long FastestQueryTimeMs { get; set; }

        /// <summary>
        /// Slowest query execution time in milliseconds
        /// </summary>
        public long SlowestQueryTimeMs { get; set; }

        /// <summary>
        /// Queries executed per minute
        /// </summary>
        public double QueriesPerMinute { get; set; }

        /// <summary>
        /// Top slowest queries
        /// </summary>
        public List<SlowQueryDTO> TopSlowQueries { get; set; } = new();

        /// <summary>
        /// Most frequently executed queries
        /// </summary>
        public List<FrequentQueryDTO> TopFrequentQueries { get; set; } = new();

        /// <summary>
        /// When these statistics were generated
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// DTO for frequently executed queries
    /// </summary>
    public class FrequentQueryDTO
    {
        /// <summary>
        /// The SQL query text
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Number of times this query was executed
        /// </summary>
        public long ExecutionCount { get; set; }

        /// <summary>
        /// Average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTimeMs { get; set; }

        /// <summary>
        /// When this query was last executed
        /// </summary>
        public DateTime LastExecuted { get; set; }
    }

    /// <summary>
    /// DTO for pagination optimization settings
    /// </summary>
    public class PaginationOptimizationDTO
    {
        /// <summary>
        /// Enable cursor-based pagination
        /// </summary>
        public bool EnableCursorPagination { get; set; }

        /// <summary>
        /// Default page size
        /// </summary>
        public int DefaultPageSize { get; set; } = 20;

        /// <summary>
        /// Maximum page size allowed
        /// </summary>
        public int MaxPageSize { get; set; } = 100;

        /// <summary>
        /// Enable count optimization for large datasets
        /// </summary>
        public bool EnableCountOptimization { get; set; }

        /// <summary>
        /// Threshold for using count optimization
        /// </summary>
        public int CountOptimizationThreshold { get; set; } = 10000;
    }

    /// <summary>
    /// DTO for cursor-based pagination request
    /// </summary>
    public class CursorPaginationRequestDTO
    {
        /// <summary>
        /// Cursor value for pagination
        /// </summary>
        public string? Cursor { get; set; }

        /// <summary>
        /// Number of items to return
        /// </summary>
        public int Limit { get; set; } = 20;

        /// <summary>
        /// Direction of pagination (forward/backward)
        /// </summary>
        public string Direction { get; set; } = "forward";
    }

    /// <summary>
    /// DTO for cursor-based pagination response
    /// </summary>
    public class CursorPaginationResponseDTO<T>
    {
        /// <summary>
        /// The data items
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Next cursor for pagination
        /// </summary>
        public string? NextCursor { get; set; }

        /// <summary>
        /// Previous cursor for pagination
        /// </summary>
        public string? PreviousCursor { get; set; }

        /// <summary>
        /// Whether there are more items available
        /// </summary>
        public bool HasMore { get; set; }

        /// <summary>
        /// Total count (if available)
        /// </summary>
        public long? TotalCount { get; set; }
    }

    /// <summary>
    /// DTO for ETag response optimization
    /// </summary>
    public class ETagResponseDTO
    {
        /// <summary>
        /// ETag value
        /// </summary>
        public string ETag { get; set; } = string.Empty;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Cache control directives
        /// </summary>
        public string CacheControl { get; set; } = string.Empty;

        /// <summary>
        /// Whether the resource has been modified
        /// </summary>
        public bool IsModified { get; set; }
    }

    /// <summary>
    /// DTO for response optimization metrics
    /// </summary>
    public class ResponseOptimizationMetricsDTO
    {
        /// <summary>
        /// Average response size in bytes
        /// </summary>
        public long AverageResponseSizeBytes { get; set; }

        /// <summary>
        /// Compression ratio achieved
        /// </summary>
        public double CompressionRatio { get; set; }

        /// <summary>
        /// Cache hit ratio for responses
        /// </summary>
        public double ResponseCacheHitRatio { get; set; }

        /// <summary>
        /// ETag validation success rate
        /// </summary>
        public double ETagValidationSuccessRate { get; set; }

        /// <summary>
        /// Number of 304 Not Modified responses
        /// </summary>
        public long NotModifiedResponses { get; set; }

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// When these metrics were generated
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// DTO for load testing configuration
    /// </summary>
    public class LoadTestConfigurationDTO
    {
        /// <summary>
        /// Test name
        /// </summary>
        public string TestName { get; set; } = string.Empty;

        /// <summary>
        /// Number of concurrent users
        /// </summary>
        public int ConcurrentUsers { get; set; } = 10;

        /// <summary>
        /// Test duration in minutes
        /// </summary>
        public int DurationMinutes { get; set; } = 5;

        /// <summary>
        /// Ramp-up time in minutes
        /// </summary>
        public int RampUpMinutes { get; set; } = 1;

        /// <summary>
        /// Target endpoints to test
        /// </summary>
        public List<string> TargetEndpoints { get; set; } = new();

        /// <summary>
        /// Expected response time threshold in milliseconds
        /// </summary>
        public int ResponseTimeThresholdMs { get; set; } = 1000;

        /// <summary>
        /// Expected success rate percentage
        /// </summary>
        public double ExpectedSuccessRate { get; set; } = 95.0;
    }

    /// <summary>
    /// DTO for load testing results
    /// </summary>
    public class LoadTestResultDTO
    {
        /// <summary>
        /// Test configuration used
        /// </summary>
        public LoadTestConfigurationDTO Configuration { get; set; } = new();

        /// <summary>
        /// Total requests made
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Successful requests
        /// </summary>
        public long SuccessfulRequests { get; set; }

        /// <summary>
        /// Failed requests
        /// </summary>
        public long FailedRequests { get; set; }

        /// <summary>
        /// Success rate percentage
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// 95th percentile response time
        /// </summary>
        public double P95ResponseTimeMs { get; set; }

        /// <summary>
        /// 99th percentile response time
        /// </summary>
        public double P99ResponseTimeMs { get; set; }

        /// <summary>
        /// Requests per second
        /// </summary>
        public double RequestsPerSecond { get; set; }

        /// <summary>
        /// Test start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Test end time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Performance regression detected
        /// </summary>
        public bool RegressionDetected { get; set; }

        /// <summary>
        /// Detailed error information
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }
}
