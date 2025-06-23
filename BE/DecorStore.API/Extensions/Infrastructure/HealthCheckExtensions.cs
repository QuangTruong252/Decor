using DecorStore.API.Configuration;
using DecorStore.API.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace DecorStore.API.Extensions.Infrastructure
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            // Add database health check
            var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
            if (databaseSettings != null && !string.IsNullOrEmpty(databaseSettings.ConnectionString))
            {
                healthChecksBuilder.AddDbContextCheck<ApplicationDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database", "sql", "ready" });
            }

            // Add cache health check
            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>();
            if (cacheSettings != null && cacheSettings.EnableDistributedCache && !string.IsNullOrEmpty(cacheSettings.RedisConnectionString))
            {
                healthChecksBuilder.AddRedis(
                    cacheSettings.RedisConnectionString,
                    name: "redis",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "cache", "redis", "ready" });
            }

            // Add memory health check
            healthChecksBuilder.AddCheck<MemoryHealthCheck>(
                name: "memory",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "memory", "ready" });

            // Add disk space health check
            healthChecksBuilder.AddCheck<DiskSpaceHealthCheck>(
                name: "disk_space",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "disk", "ready" });

            // Add file storage health check
            var fileStorageSettings = configuration.GetSection("FileStorage").Get<FileStorageSettings>();
            if (fileStorageSettings != null)
            {
                healthChecksBuilder.AddCheck<FileStorageHealthCheck>(
                    name: "file_storage",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "storage", "files", "ready" });
            }

            // Add application health check
            healthChecksBuilder.AddCheck<ApplicationHealthCheck>(
                name: "application",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "application", "ready" });

            return services;
        }

        public static WebApplication UseHealthCheckEndpoints(this WebApplication app)
        {
            // Basic health check endpoint
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = WriteHealthCheckResponse
            });

            // Ready endpoint (for Kubernetes readiness probe)
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = WriteHealthCheckResponse
            });

            // Live endpoint (for Kubernetes liveness probe)
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live") || check.Name == "application",
                ResponseWriter = WriteHealthCheckResponse
            });

            // Detailed health check endpoint (for monitoring)
            app.MapHealthChecks("/health/detailed", new HealthCheckOptions
            {
                ResponseWriter = WriteDetailedHealthCheckResponse
            });

            return app;
        }

        private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        private static async Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description,
                    data = entry.Value.Data,
                    exception = entry.Value.Exception?.Message,
                    tags = entry.Value.Tags
                })
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
        }
    }

    // Custom health check implementations
    public class MemoryHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var allocatedBytes = GC.GetTotalMemory(false);
            var allocatedMB = allocatedBytes / 1024 / 1024;

            var data = new Dictionary<string, object>
            {
                ["AllocatedMemoryMB"] = allocatedMB,
                ["Gen0Collections"] = GC.CollectionCount(0),
                ["Gen1Collections"] = GC.CollectionCount(1),
                ["Gen2Collections"] = GC.CollectionCount(2)
            };

            var status = allocatedMB < 500 ? HealthStatus.Healthy : 
                        allocatedMB < 1000 ? HealthStatus.Degraded : HealthStatus.Unhealthy;

            return Task.FromResult(new HealthCheckResult(
                status,
                description: $"Memory usage: {allocatedMB}MB",
                data: data));
        }
    }

    public class DiskSpaceHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
                var freeSpaceGB = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                var totalSpaceGB = drive.TotalSize / 1024 / 1024 / 1024;
                var usedSpacePercentage = ((double)(totalSpaceGB - freeSpaceGB) / totalSpaceGB) * 100;

                var data = new Dictionary<string, object>
                {
                    ["FreeSpaceGB"] = freeSpaceGB,
                    ["TotalSpaceGB"] = totalSpaceGB,
                    ["UsedSpacePercentage"] = Math.Round(usedSpacePercentage, 2)
                };

                var status = usedSpacePercentage < 80 ? HealthStatus.Healthy :
                            usedSpacePercentage < 90 ? HealthStatus.Degraded : HealthStatus.Unhealthy;

                return Task.FromResult(new HealthCheckResult(
                    status,
                    description: $"Disk usage: {Math.Round(usedSpacePercentage, 1)}% ({freeSpaceGB}GB free)",
                    data: data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    description: "Unable to check disk space",
                    exception: ex));
            }
        }
    }

    public class FileStorageHealthCheck : IHealthCheck
    {
        private readonly IOptions<FileStorageSettings> _fileStorageSettings;

        public FileStorageHealthCheck(IOptions<FileStorageSettings> fileStorageSettings)
        {
            _fileStorageSettings = fileStorageSettings;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var settings = _fileStorageSettings.Value;
                var uploadPath = Path.Combine(Environment.CurrentDirectory, settings.UploadPath);
                var thumbnailPath = Path.Combine(uploadPath, settings.ThumbnailPath);

                var data = new Dictionary<string, object>
                {
                    ["UploadPath"] = uploadPath,
                    ["ThumbnailPath"] = thumbnailPath,
                    ["UploadPathExists"] = Directory.Exists(uploadPath),
                    ["ThumbnailPathExists"] = Directory.Exists(thumbnailPath)
                };

                // Check if directories exist and are writable
                var uploadPathExists = Directory.Exists(uploadPath);
                var thumbnailPathExists = Directory.Exists(thumbnailPath);

                if (!uploadPathExists || !thumbnailPathExists)
                {
                    return Task.FromResult(new HealthCheckResult(
                        HealthStatus.Unhealthy,
                        description: "File storage directories do not exist",
                        data: data));
                }

                // Test write access
                var testFile = Path.Combine(uploadPath, $"health_check_{Guid.NewGuid()}.tmp");
                File.WriteAllText(testFile, "health check");
                File.Delete(testFile);

                return Task.FromResult(new HealthCheckResult(
                    HealthStatus.Healthy,
                    description: "File storage is accessible",
                    data: data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    description: "File storage check failed",
                    exception: ex));
            }
        }
    }

    public class ApplicationHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>
            {
                ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                ["MachineName"] = Environment.MachineName,
                ["ProcessId"] = Environment.ProcessId,
                ["WorkingSet"] = Environment.WorkingSet / 1024 / 1024, // MB
                ["Uptime"] = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
            };

            return Task.FromResult(new HealthCheckResult(
                HealthStatus.Healthy,
                description: "Application is running",
                data: data));
        }
    }
}
