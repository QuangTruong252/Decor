using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.Configuration
{
    public class DatabaseSettings
    {
        [Required(ErrorMessage = "Connection string is required")]
        [MinLength(10, ErrorMessage = "Connection string must be at least 10 characters")]
        public string ConnectionString { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Max retry count must be between 1 and 10")]
        public int MaxRetryCount { get; set; } = 5;

        [Range(1, 300, ErrorMessage = "Max retry delay must be between 1 and 300 seconds")]
        public int MaxRetryDelaySeconds { get; set; } = 30;

        [Required(ErrorMessage = "Migration history table name is required")]
        public string MigrationHistoryTable { get; set; } = "__EFMigrationsHistory";

        public bool EnableSensitiveDataLogging { get; set; } = false;

        public bool EnableDetailedErrors { get; set; } = false;

        [Range(10, 3600, ErrorMessage = "Command timeout must be between 10 and 3600 seconds")]
        public int CommandTimeoutSeconds { get; set; } = 30;

        // Connection Pool Settings
        [Range(1, 100, ErrorMessage = "Min pool size must be between 1 and 100")]
        public int MinPoolSize { get; set; } = 5;

        [Range(10, 1000, ErrorMessage = "Max pool size must be between 10 and 1000")]
        public int MaxPoolSize { get; set; } = 100;

        [Range(5, 300, ErrorMessage = "Connection timeout must be between 5 and 300 seconds")]
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        // Security Settings
        public bool EnableEncryption { get; set; } = true;
        public bool TrustServerCertificate { get; set; } = false;

        // Performance Settings
        public bool EnableConnectionPooling { get; set; } = true;
        public bool EnableMultipleActiveResultSets { get; set; } = true;

        // Health Check Settings
        public bool EnableHealthChecks { get; set; } = true;
        
        [Range(5, 300, ErrorMessage = "Health check timeout must be between 5 and 300 seconds")]
        public int HealthCheckTimeoutSeconds { get; set; } = 30;

        // Migration Settings
        public bool AutoMigrateOnStartup { get; set; } = false;
        public bool SeedDataOnStartup { get; set; } = false;
    }
}
