using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using DecorStore.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Repositories
{
    public class SecurityEventRepository : BaseRepository<SecurityEvent>, ISecurityEventRepository
    {
        public SecurityEventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SecurityEvent>> GetEventsByUserIdAsync(int userId, int skip = 0, int take = 50)
        {
            return await _dbSet
                .Where(se => se.UserId == userId && !se.IsDeleted)
                .OrderByDescending(se => se.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SecurityEvent>> GetEventsByTypeAsync(string eventType, DateTime? from = null, DateTime? to = null)
        {
            var query = _dbSet.Where(se => se.EventType == eventType && !se.IsDeleted);

            if (from.HasValue)
                query = query.Where(se => se.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(se => se.Timestamp <= to.Value);

            return await query
                .OrderByDescending(se => se.Timestamp)
                .ToListAsync();
        }

        public async Task<List<SecurityEvent>> GetEventsByDateRangeAsync(DateTime from, DateTime to, int skip = 0, int take = 50)
        {
            return await _dbSet
                .Where(se => se.Timestamp >= from && se.Timestamp <= to && !se.IsDeleted)
                .OrderByDescending(se => se.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SecurityEvent>> GetFailedEventsAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _dbSet.Where(se => !se.Success && !se.IsDeleted);

            if (from.HasValue)
                query = query.Where(se => se.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(se => se.Timestamp <= to.Value);

            return await query
                .OrderByDescending(se => se.Timestamp)
                .ToListAsync();
        }

        public async Task<List<SecurityEvent>> GetHighRiskEventsAsync(decimal minRiskScore = 0.7m, DateTime? from = null, DateTime? to = null)
        {
            var query = _dbSet.Where(se => se.RiskScore >= minRiskScore && !se.IsDeleted);

            if (from.HasValue)
                query = query.Where(se => se.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(se => se.Timestamp <= to.Value);

            return await query
                .OrderByDescending(se => se.Timestamp)
                .ToListAsync();
        }

        public async Task<List<SecurityEvent>> GetUnprocessedEventsAsync(DateTime? from = null)
        {
            var query = _dbSet.Where(se => !se.IsProcessed && se.RequiresInvestigation && !se.IsDeleted);

            if (from.HasValue)
                query = query.Where(se => se.Timestamp >= from.Value);

            return await query
                .OrderByDescending(se => se.Timestamp)
                .ToListAsync();
        }

        public async Task<int> GetEventCountAsync(DateTime? from = null, DateTime? to = null, string? eventType = null, int? userId = null)
        {
            var query = _dbSet.Where(se => !se.IsDeleted);

            if (from.HasValue)
                query = query.Where(se => se.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(se => se.Timestamp <= to.Value);

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(se => se.EventType == eventType);

            if (userId.HasValue)
                query = query.Where(se => se.UserId == userId.Value);

            return await query.CountAsync();
        }

        public async Task<int> CleanupOldEventsAsync(DateTime cutoffDate)
        {
            var oldEvents = await _dbSet
                .Where(se => se.Timestamp <= cutoffDate && !se.IsDeleted)
                .ToListAsync();

            foreach (var securityEvent in oldEvents)
            {
                securityEvent.IsDeleted = true;
                securityEvent.UpdatedAt = DateTime.UtcNow;
            }

            return oldEvents.Count;
        }
    }
}
