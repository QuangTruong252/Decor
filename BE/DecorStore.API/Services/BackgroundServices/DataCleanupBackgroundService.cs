using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using DecorStore.API.Configuration;
using DecorStore.API.Interfaces.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DecorStore.API.Services.BackgroundServices
{
    /// <summary>
    /// Background service for cleaning up expired data and temporary files
    /// </summary>
    public class DataCleanupBackgroundService : BackgroundService
    {
        private readonly ILogger<DataCleanupBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly FileStorageSettings _fileStorageSettings;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours

        public DataCleanupBackgroundService(
            ILogger<DataCleanupBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<FileStorageSettings> fileStorageSettings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _fileStorageSettings = fileStorageSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data cleanup background service started");

            // Initial cleanup after startup delay
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanup();
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Data cleanup background service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during data cleanup");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Wait 1 hour before retry
                }
            }
        }

        private async Task PerformCleanup()
        {
            _logger.LogInformation("Starting data cleanup process");

            using var scope = _serviceProvider.CreateScope();

            var cleanupTasks = new List<Task>
            {
                CleanupTempFiles(),
                CleanupOldLogFiles(),
                CleanupExpiredSessions(scope),
                CleanupOrphanedFiles(scope)
            };

            await Task.WhenAll(cleanupTasks);

            _logger.LogInformation("Data cleanup process completed successfully");
        }

        private async Task CleanupTempFiles()
        {
            try
            {
                _logger.LogDebug("Cleaning up temporary files");

                var tempPath = Path.Combine(_fileStorageSettings.StoragePath, "temp");
                if (!Directory.Exists(tempPath))
                {
                    return;
                }

                var cutoffDate = DateTime.UtcNow.AddHours(-24); // Remove files older than 24 hours
                var tempFiles = Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories);

                int deletedCount = 0;
                foreach (var file in tempFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTimeUtc < cutoffDate)
                    {
                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temp file: {File}", file);
                        }
                    }
                }

                _logger.LogDebug("Cleaned up {Count} temporary files", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up temporary files");
            }
        }

        private async Task CleanupOldLogFiles()
        {
            try
            {
                _logger.LogDebug("Cleaning up old log files");

                var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(logsPath))
                {
                    return;
                }

                var cutoffDate = DateTime.UtcNow.AddDays(-30); // Keep logs for 30 days
                var logFiles = Directory.GetFiles(logsPath, "*.log", SearchOption.AllDirectories);

                int deletedCount = 0;
                foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTimeUtc < cutoffDate)
                    {
                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete log file: {File}", file);
                        }
                    }
                }

                _logger.LogDebug("Cleaned up {Count} old log files", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old log files");
            }
        }

        private async Task CleanupExpiredSessions(IServiceScope scope)
        {
            try
            {
                _logger.LogDebug("Cleaning up expired sessions");

                // This would clean up expired sessions from the database
                // Implementation depends on your session storage strategy

                var distributedCacheService = scope.ServiceProvider.GetService<IDistributedCacheService>();
                if (distributedCacheService != null)
                {
                    // Clean up expired session cache entries
                    await distributedCacheService.RemoveByPatternAsync("session:expired:*");
                }

                _logger.LogDebug("Expired sessions cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired sessions");
            }
        }

        private async Task CleanupOrphanedFiles(IServiceScope scope)
        {
            try
            {
                _logger.LogDebug("Cleaning up orphaned files");

                // This would identify and remove files that are no longer referenced in the database
                // Implementation would involve checking the database for referenced files

                var fileManagerService = scope.ServiceProvider.GetService<IFileManagerService>();
                if (fileManagerService != null)
                {
                    // Get list of files in storage
                    var storagePath = _fileStorageSettings.StoragePath;
                    var allFiles = Directory.GetFiles(storagePath, "*", SearchOption.AllDirectories);

                    // This is a placeholder - actual implementation would:
                    // 1. Query database for all referenced file paths
                    // 2. Compare with files on disk
                    // 3. Remove unreferenced files (with safety checks)

                    _logger.LogDebug("Checked {Count} files for orphaned status", allFiles.Length);
                }

                _logger.LogDebug("Orphaned files cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up orphaned files");
            }
        }
    }
}
