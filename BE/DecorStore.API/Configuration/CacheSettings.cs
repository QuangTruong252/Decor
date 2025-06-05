using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.Configuration
{
    public class CacheSettings
    {        [Range(1, 1440, ErrorMessage = "Default expiry must be between 1 and 1440 minutes (24 hours)")]
        public int DefaultExpirationMinutes { get; set; } = 60;

        [Range(1, 10080, ErrorMessage = "Long term expiry must be between 1 and 10080 minutes (7 days)")]
        public int LongTermExpiryMinutes { get; set; } = 1440; // 24 hours

        [Range(1, 60, ErrorMessage = "Short term expiry must be between 1 and 60 minutes")]
        public int ShortTermExpiryMinutes { get; set; } = 15;

        public bool EnableCaching { get; set; } = true;

        public bool EnableDistributedCache { get; set; } = false;

        [Required(ErrorMessage = "Cache key prefix is required")]
        [MinLength(2, ErrorMessage = "Cache key prefix must be at least 2 characters")]
        public string CacheKeyPrefix { get; set; } = "DecorStore";        [Range(1, 1000, ErrorMessage = "Max cache size must be between 1 and 1000 MB")]
        public int MaxCacheSizeMB { get; set; } = 100;

        [Range(1, 10000000, ErrorMessage = "Default size limit must be between 1 and 10,000,000")]
        public long DefaultSizeLimit { get; set; } = 1000000; // 1 million items

        [Range(0, 1440, ErrorMessage = "Sliding expiration must be between 0 and 1440 minutes")]
        public int SlidingExpirationMinutes { get; set; } = 30;

        public bool CompressLargeValues { get; set; } = true;

        [Range(1024, 1048576, ErrorMessage = "Compression threshold must be between 1KB and 1MB")]
        public int CompressionThresholdBytes { get; set; } = 10240; // 10KB

        // Redis settings (for distributed cache)
        public string? RedisConnectionString { get; set; }

        [Range(0, 15, ErrorMessage = "Redis database must be between 0 and 15")]
        public int RedisDatabase { get; set; } = 0;

        [Range(1000, 30000, ErrorMessage = "Redis timeout must be between 1000 and 30000 milliseconds")]
        public int RedisTimeoutMs { get; set; } = 5000;

        // Cache warming settings
        public bool EnableCacheWarming { get; set; } = true;

        public string[] CacheWarmupKeys { get; set; } = new[]
        {
            "categories:all",
            "products:featured",
            "dashboard:stats"
        };
    }
}
