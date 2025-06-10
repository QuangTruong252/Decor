using DecorStore.API.Data;
using DecorStore.API.Models;
using DecorStore.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Background service for cleaning up expired tokens and security events
    /// </summary>
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync();
                    await CleanupOldSecurityEventsAsync();
                    await ProcessSecurityAlertsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token cleanup process");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Token Cleanup Service stopped");
        }

        private async Task CleanupExpiredTokensAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var cutoffTime = DateTime.UtcNow;

                // Clean up expired refresh tokens
                var expiredRefreshTokens = await context.RefreshTokens
                    .Where(rt => rt.ExpiryDate < cutoffTime || rt.IsRevoked || rt.IsUsed)
                    .ToListAsync();

                if (expiredRefreshTokens.Any())
                {
                    context.RefreshTokens.RemoveRange(expiredRefreshTokens);
                    _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredRefreshTokens.Count);
                }

                // Clean up expired blacklisted tokens
                var expiredBlacklistedTokens = await context.TokenBlacklists
                    .Where(tb => tb.ExpiryDate < cutoffTime)
                    .ToListAsync();

                if (expiredBlacklistedTokens.Any())
                {
                    context.TokenBlacklists.RemoveRange(expiredBlacklistedTokens);
                    _logger.LogInformation("Cleaned up {Count} expired blacklisted tokens", expiredBlacklistedTokens.Count);
                }

                // Clean up old token families (keep only the last 5 tokens per family)
                var tokenFamilies = await context.RefreshTokens
                    .Where(rt => !string.IsNullOrEmpty(rt.TokenFamily))
                    .GroupBy(rt => rt.TokenFamily)
                    .Where(g => g.Count() > 5)
                    .ToListAsync();

                foreach (var family in tokenFamilies)
                {
                    var tokensToRemove = family
                        .OrderByDescending(rt => rt.CreatedAt)
                        .Skip(5)
                        .ToList();

                    context.RefreshTokens.RemoveRange(tokensToRemove);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Token cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token cleanup");
            }
        }

        private async Task CleanupOldSecurityEventsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Keep security events for 90 days
                var cutoffTime = DateTime.UtcNow.AddDays(-90);

                var oldSecurityEvents = await context.SecurityEvents
                    .Where(se => se.Timestamp < cutoffTime && !se.RequiresInvestigation)
                    .ToListAsync();

                if (oldSecurityEvents.Any())
                {
                    context.SecurityEvents.RemoveRange(oldSecurityEvents);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up {Count} old security events", oldSecurityEvents.Count);
                }

                // Archive high-risk events older than 1 year
                var archiveCutoffTime = DateTime.UtcNow.AddYears(-1);
                var archiveEvents = await context.SecurityEvents
                    .Where(se => se.Timestamp < archiveCutoffTime && se.RiskScore >= 0.8m)
                    .ToListAsync();

                if (archiveEvents.Any())
                {
                    // In a real implementation, you might move these to an archive table
                    // For now, we'll just mark them as archived
                    foreach (var evt in archiveEvents)
                    {
                        evt.IsProcessed = true;
                        evt.ProcessedAt = DateTime.UtcNow;
                        evt.ProcessedBy = "ARCHIVE_SYSTEM";
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Archived {Count} high-risk security events", archiveEvents.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during security events cleanup");
            }
        }

        private async Task ProcessSecurityAlertsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var securityEventLogger = scope.ServiceProvider.GetRequiredService<ISecurityEventLogger>();

            try
            {
                await securityEventLogger.ProcessSecurityAlertsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing security alerts");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
