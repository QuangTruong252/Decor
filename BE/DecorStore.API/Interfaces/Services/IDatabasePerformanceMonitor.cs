using System.Threading.Tasks;

namespace DecorStore.API.Interfaces.Services
{
    public class DatabaseMetrics
    {
        public double AverageQueryTimeMs { get; set; }
        public int SlowQueryCount { get; set; }
        public int TotalQueryCount { get; set; }
        public int ActiveConnections { get; set; }
        public int AvailableConnections { get; set; }
        public double CpuUsagePercent { get; set; }
        public long MemoryUsageMB { get; set; }
    }

    public interface IDatabasePerformanceMonitor
    {
        Task<DatabaseMetrics> GetMetricsAsync();
    }
}
