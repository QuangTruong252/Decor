namespace DecorStore.API.Configuration
{
    public class CacheSettings
    {
        public bool EnableCaching { get; set; } = true;
        public bool EnableDistributedCache { get; set; } = false;
        public bool EnableCacheWarming { get; set; } = true;
        public string CacheKeyPrefix { get; set; } = "DecorStore";
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int ShortTermExpiryMinutes { get; set; } = 5;
        public int LongTermExpiryMinutes { get; set; } = 120;
        public int SlidingExpirationMinutes { get; set; } = 10;
        public int MaxCacheSizeMB { get; set; } = 256;
        public int DefaultSizeLimit { get; set; } = 10000;        public int RedisDatabase { get; set; } = 0;
        public int RedisTimeoutMs { get; set; } = 5000;
        public string RedisConnectionString { get; set; } = "localhost:6379";
        public string[] CacheWarmupKeys { get; set; } = new[]
        {
            "categories:all",
            "products:featured",
            "products:top-rated",
            "banners:active"
        };
    }
}
