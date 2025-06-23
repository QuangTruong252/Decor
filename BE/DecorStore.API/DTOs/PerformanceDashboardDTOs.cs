namespace DecorStore.API.DTOs
{
    /// <summary>
    /// Performance dashboard overview data
    /// </summary>
    public class PerformanceDashboardDTO
    {
        public SystemOverviewDTO SystemOverview { get; set; } = new();
        public ApiPerformanceDTO ApiPerformance { get; set; } = new();
        public DatabasePerformanceDTO DatabasePerformance { get; set; } = new();
        public CachePerformanceDTO CachePerformance { get; set; } = new();
        public List<AlertDTO> Alerts { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // Additional properties used by PerformanceDashboardService
        public SystemInfoDTO SystemInfo { get; set; } = new();
        public ResourceUtilizationDTO ResourceUsage { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// System overview metrics
    /// </summary>
    public class SystemOverviewDTO
    {
        public double CpuUsagePercent { get; set; }
        public long MemoryUsageMB { get; set; }
        public long MemoryTotalMB { get; set; }
        public double MemoryUsagePercent { get; set; }
        public long DiskUsageMB { get; set; }
        public long DiskTotalMB { get; set; }
        public double DiskUsagePercent { get; set; }
        public int ActiveConnections { get; set; }
        public TimeSpan Uptime { get; set; }
        public int ThreadCount { get; set; }
        public long GCTotalMemoryMB { get; set; }
    }

    /// <summary>
    /// API performance metrics
    /// </summary>
    public class ApiPerformanceDTO
    {
        public int RequestsPerMinute { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public double P95ResponseTimeMs { get; set; }
        public double P99ResponseTimeMs { get; set; }
        public int ErrorRate { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public List<EndpointPerformanceDTO> SlowestEndpoints { get; set; } = new();

        // Additional properties used by PerformanceDashboardService
        public double AverageResponseTime { get; set; }
        public int ActiveUsers { get; set; }
        public double PeakResponseTime { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// Individual endpoint performance data
    /// </summary>
    public class EndpointPerformanceDTO
    {
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public double MinResponseTimeMs { get; set; }
        public double MaxResponseTimeMs { get; set; }
        public double P95ResponseTimeMs { get; set; }
        public int ErrorCount { get; set; }
        public double ErrorRate { get; set; }
        public DateTime LastAccessed { get; set; }
        
        // Additional properties used by PerformanceDashboardService
        public string Endpoint { get; set; } = string.Empty;
        public double AverageResponseTime { get; set; }
    }

    /// <summary>
    /// Database performance metrics
    /// </summary>
    public class DatabasePerformanceDTO
    {
        public int ActiveConnections { get; set; }
        public int PoolSize { get; set; }
        public double AverageQueryTimeMs { get; set; }
        public int SlowQueryCount { get; set; }
        public int DeadlockCount { get; set; }
        public long TotalQueries { get; set; }
        public double QueryTimeoutRate { get; set; }
        public List<SlowQueryDTO> SlowestQueries { get; set; } = new();
        public DatabaseMetrics Metrics { get; set; } = new();
    }

    /// <summary>
    /// Slow query information
    /// </summary>
    public class SlowQueryDTO
    {
        public string Query { get; set; } = string.Empty;
        public double ExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
        public DateTime LastExecuted { get; set; }
        public string? Parameters { get; set; }
    }

    /// <summary>
    /// Cache performance metrics
    /// </summary>
    public class CachePerformanceDTO
    {
        public double HitRatio { get; set; }
        public long TotalRequests { get; set; }
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public long EvictionCount { get; set; }
        public long MemoryUsageMB { get; set; }
        public int KeyCount { get; set; }
        public double AverageGetTimeMs { get; set; }
        public double AverageSetTimeMs { get; set; }
        public List<CacheKeyStatsDTO> TopKeys { get; set; } = new();
        
        // Additional properties used by PerformanceDashboardService
        public double AverageGetTime { get; set; }
    }

    /// <summary>
    /// Cache key statistics
    /// </summary>
    public class CacheKeyStatsDTO
    {
        public string Key { get; set; } = string.Empty;
        public long AccessCount { get; set; }
        public long SizeBytes { get; set; }
        public DateTime LastAccessed { get; set; }
        public TimeSpan? TTL { get; set; }
    }

    /// <summary>
    /// Performance metrics over time
    /// </summary>
    public class PerformanceMetricsDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<MetricDataPointDTO> ResponseTimes { get; set; } = new();
        public List<MetricDataPointDTO> RequestCounts { get; set; } = new();
        public List<MetricDataPointDTO> ErrorRates { get; set; } = new();
        public List<MetricDataPointDTO> CpuUsage { get; set; } = new();
        public List<MetricDataPointDTO> MemoryUsage { get; set; } = new();
        
        // Additional properties used by PerformanceDashboardService
        public int TotalRequests { get; set; }
        public double AverageResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public int ThroughputRpm { get; set; }
        public double PeakResponseTime { get; set; }
        public double CacheHitRatio { get; set; }
        public int DatabaseConnections { get; set; }
    }

    /// <summary>
    /// Individual metric data point
    /// </summary>
    public class MetricDataPointDTO
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string? Label { get; set; }
    }

    /// <summary>
    /// Performance trends analysis
    /// </summary>
    public class PerformanceTrendsDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Granularity { get; set; } = string.Empty;
        public List<TrendDataDTO> ResponseTimeTrend { get; set; } = new();
        public List<TrendDataDTO> ThroughputTrend { get; set; } = new();
        public List<TrendDataDTO> ErrorRateTrend { get; set; } = new();
        public List<TrendDataDTO> ResourceUsageTrend { get; set; } = new();
        public TrendAnalysisDTO Analysis { get; set; } = new();
        
        // Additional properties used by PerformanceDashboardService
        public List<TrendDataDTO> CacheHitRateTrend { get; set; } = new();
    }

    /// <summary>
    /// Trend data point
    /// </summary>
    public class TrendDataDTO
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// Trend analysis results
    /// </summary>
    public class TrendAnalysisDTO
    {
        public double ResponseTimeChange { get; set; }
        public double ThroughputChange { get; set; }
        public double ErrorRateChange { get; set; }
        public string OverallTrend { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Resource utilization data
    /// </summary>
    public class ResourceUtilizationDTO
    {
        public CpuUtilizationDTO Cpu { get; set; } = new();
        public MemoryUtilizationDTO Memory { get; set; } = new();
        public DiskUtilizationDTO Disk { get; set; } = new();
        public NetworkUtilizationDTO Network { get; set; } = new();
        public ThreadUtilizationDTO Threads { get; set; } = new();
        
        // Additional properties used by PerformanceDashboardService
        public double CpuUsagePercent { get; set; }
        public long MemoryUsageMB { get; set; }
        public double DiskUsagePercent { get; set; }
        public double NetworkInMbps { get; set; }
        public double NetworkOutMbps { get; set; }
        public int ThreadCount { get; set; }
        public long HandleCount { get; set; }
        public int GcCollections { get; set; }
    }

    /// <summary>
    /// CPU utilization details
    /// </summary>
    public class CpuUtilizationDTO
    {
        public double UsagePercent { get; set; }
        public int CoreCount { get; set; }
        public List<double> PerCoreUsage { get; set; } = new();
        public double UserTime { get; set; }
        public double SystemTime { get; set; }
        public double IdleTime { get; set; }
    }

    /// <summary>
    /// Memory utilization details
    /// </summary>
    public class MemoryUtilizationDTO
    {
        public long TotalMB { get; set; }
        public long UsedMB { get; set; }
        public long FreeMB { get; set; }
        public double UsagePercent { get; set; }
        public long WorkingSetMB { get; set; }
        public long PrivateMemoryMB { get; set; }
        public long VirtualMemoryMB { get; set; }
        public long GcTotalMemoryMB { get; set; }
        public int GcGeneration0Collections { get; set; }
        public int GcGeneration1Collections { get; set; }
        public int GcGeneration2Collections { get; set; }
    }

    /// <summary>
    /// Disk utilization details
    /// </summary>
    public class DiskUtilizationDTO
    {
        public long TotalSpaceMB { get; set; }
        public long FreeSpaceMB { get; set; }
        public long UsedSpaceMB { get; set; }
        public double UsagePercent { get; set; }
        public List<DriveInfoDTO> Drives { get; set; } = new();
    }

    /// <summary>
    /// Individual drive information
    /// </summary>
    public class DriveInfoDTO
    {
        public string Name { get; set; } = string.Empty;
        public string FileSystem { get; set; } = string.Empty;
        public long TotalSizeMB { get; set; }
        public long FreeSizeMB { get; set; }
        public double UsagePercent { get; set; }
    }

    /// <summary>
    /// Network utilization details
    /// </summary>
    public class NetworkUtilizationDTO
    {
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public long PacketsSent { get; set; }
        public long PacketsReceived { get; set; }
        public int ActiveConnections { get; set; }
    }

    /// <summary>
    /// Thread utilization details
    /// </summary>
    public class ThreadUtilizationDTO
    {
        public int TotalThreads { get; set; }
        public int ActiveThreads { get; set; }
        public int IdleThreads { get; set; }
        public int PoolThreads { get; set; }
        public int CompletionPortThreads { get; set; }
    }

    /// <summary>
    /// Performance alert information
    /// </summary>
    public class AlertDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;        public DateTime Timestamp { get; set; }
        public bool IsResolved { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    /// <summary>
    /// System information DTO
    /// </summary>
    public class SystemInfoDTO
    {
        public string ServerName { get; set; } = string.Empty;
        public string ApplicationVersion { get; set; } = string.Empty;
        public string FrameworkVersion { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public string Environment { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public long WorkingSetMemory { get; set; }
    }

    /// <summary>
    /// Trend data point
    /// </summary>
    public class TrendDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string? Label { get; set; }
    }

    /// <summary>
    /// System resource utilization summary DTO
    /// </summary>
    public class SystemResourceSummaryDTO
    {
        public double CpuUsagePercent { get; set; }
        public long MemoryUsageMB { get; set; }
        public long TotalMemoryMB { get; set; }
        public double MemoryUsagePercent { get; set; }
        public long DiskUsageMB { get; set; }
        public long DiskTotalMB { get; set; }
        public double DiskUsagePercent { get; set; }
        public int ActiveConnections { get; set; }
        public int ThreadCount { get; set; }
        public long HandleCount { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Cache performance metrics DTO
    /// </summary>
    public class CacheMetricsDTO
    {
        public long TotalRequests { get; set; }
        public long CacheHits { get; set; }
        public long CacheMisses { get; set; }
        public double HitRatio { get; set; }
        public int TotalKeys { get; set; }
        public int KeysWithTags { get; set; }
        public long MemoryCacheSize { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<CacheKeyInfoDTO> TopKeys { get; set; } = new();
    }

    /// <summary>
    /// Cache key information DTO
    /// </summary>
    public class CacheKeyInfoDTO
    {
        public string Key { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long AccessCount { get; set; }
        public string? Tag { get; set; }
    }
}
