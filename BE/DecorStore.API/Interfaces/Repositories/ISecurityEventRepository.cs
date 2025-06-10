using DecorStore.API.Models;
using DecorStore.API.Interfaces.Repositories.Base;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface ISecurityEventRepository : IRepository<SecurityEvent>
    {
        Task<List<SecurityEvent>> GetEventsByUserIdAsync(int userId, int skip = 0, int take = 50);
        Task<List<SecurityEvent>> GetEventsByTypeAsync(string eventType, DateTime? from = null, DateTime? to = null);
        Task<List<SecurityEvent>> GetEventsByDateRangeAsync(DateTime from, DateTime to, int skip = 0, int take = 50);
        Task<List<SecurityEvent>> GetFailedEventsAsync(DateTime? from = null, DateTime? to = null);
        Task<List<SecurityEvent>> GetHighRiskEventsAsync(decimal minRiskScore = 0.7m, DateTime? from = null, DateTime? to = null);
        Task<List<SecurityEvent>> GetUnprocessedEventsAsync(DateTime? from = null);
        Task<int> GetEventCountAsync(DateTime? from = null, DateTime? to = null, string? eventType = null, int? userId = null);
        Task<int> CleanupOldEventsAsync(DateTime cutoffDate);
    }
}
