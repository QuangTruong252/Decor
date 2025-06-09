using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DecorStore.API.Configuration;
using DecorStore.API.Interfaces.Services;

namespace DecorStore.API.Services
{
    public class RedisCacheService : IDistributedCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly CacheSettings _settings;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(
            IDistributedCache distributedCache,
            IConnectionMultiplexer redis,
            IOptions<CacheSettings> settings,
            ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _database = _redis.GetDatabase(_settings.RedisDatabase);
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                var value = await _distributedCache.GetStringAsync(fullKey);

                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogDebug("Redis cache miss for key: {Key}", fullKey);
                    return default;
                }

                _logger.LogDebug("Redis cache hit for key: {Key}", fullKey);
                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.SetAbsoluteExpiration(expiration.Value);
                }
                else
                {
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes));
                }

                // Add sliding expiration if configured
                if (_settings.SlidingExpirationMinutes > 0)
                {
                    options.SetSlidingExpiration(TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes));
                }

                await _distributedCache.SetStringAsync(fullKey, serializedValue, options);
                _logger.LogDebug("Redis cache set for key: {Key}", fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in Redis cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                await _distributedCache.RemoveAsync(fullKey);
                _logger.LogDebug("Redis cache removed for key: {Key}", fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from Redis cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var fullPattern = GetFullKey(pattern);
                
                await foreach (var key in server.KeysAsync(pattern: fullPattern))
                {
                    await _database.KeyDeleteAsync(key);
                }

                _logger.LogDebug("Redis cache removed keys matching pattern: {Pattern}", fullPattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing keys by pattern from Redis cache: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                return await _database.KeyExistsAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking key existence in Redis cache: {Key}", key);
                return false;
            }
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                await server.FlushDatabaseAsync(_settings.RedisDatabase);
                _logger.LogInformation("Redis cache cleared completely");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing Redis cache");
            }
        }

        public async Task<long> GetKeysCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = GetFullKey("*");
                long count = 0;

                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    count++;
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keys count from Redis cache");
                return 0;
            }
        }

        public async Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", CancellationToken cancellationToken = default)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var fullPattern = GetFullKey(pattern);
                var keys = new List<string>();

                await foreach (var key in server.KeysAsync(pattern: fullPattern))
                {
                    keys.Add(key.ToString().Replace($"{_settings.CacheKeyPrefix}:", ""));
                }

                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keys from Redis cache");
                return new List<string>();
            }
        }        public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _database.PingAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis connection check failed");
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory();
            await SetAsync(key, value, expiration, cancellationToken);
            return value;
        }

        private string GetFullKey(string key)
        {
            return $"{_settings.CacheKeyPrefix}:{key}";
        }
    }

    public class HybridCacheService : ICacheService
    {
        private readonly ICacheService _memoryCache;
        private readonly IDistributedCacheService _distributedCache;
        private readonly CacheSettings _settings;
        private readonly ILogger<HybridCacheService> _logger;

        public HybridCacheService(
            ICacheService memoryCache,
            IDistributedCacheService distributedCache,
            IOptions<CacheSettings> settings,
            ILogger<HybridCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            // Try memory cache first
            if (_memoryCache.Exists(key))
            {
                return _memoryCache.GetOrCreate(key, factory, expiration);
            }

            // Try distributed cache if enabled
            if (_settings.EnableDistributedCache)
            {
                try
                {
                    var distributedValue = _distributedCache.GetAsync<T>(key).Result;
                    if (distributedValue != null)
                    {
                        // Store in memory cache for faster subsequent access
                        _memoryCache.Set(key, distributedValue, TimeSpan.FromMinutes(_settings.ShortTermExpiryMinutes));
                        return distributedValue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get value from distributed cache, falling back to factory");
                }
            }

            // Create new value
            var value = factory();
            
            // Store in both caches
            _memoryCache.Set(key, value, expiration);
            
            if (_settings.EnableDistributedCache)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _distributedCache.SetAsync(key, value, expiration);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to set value in distributed cache");
                    }
                });
            }

            return value;
        }        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            // Try memory cache first
            if (_memoryCache.Exists(key))
            {
                return await _memoryCache.GetOrCreateAsync(key, factory, expiration);
            }

            // Try distributed cache if enabled
            if (_settings.EnableDistributedCache)
            {
                try
                {
                    var distributedValue = await _distributedCache.GetAsync<T>(key);
                    if (distributedValue != null)
                    {
                        // Store in memory cache for faster subsequent access
                        _memoryCache.Set(key, distributedValue, TimeSpan.FromMinutes(_settings.ShortTermExpiryMinutes));
                        return distributedValue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get value from distributed cache, falling back to factory");
                }
            }

            // Create new value
            var value = await factory();
            
            // Store in both caches
            _memoryCache.Set(key, value, expiration);
            
            if (_settings.EnableDistributedCache)
            {
                try
                {
                    await _distributedCache.SetAsync(key, value, expiration);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to set value in distributed cache");
                }
            }

            return value;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority, string? tag = null)
        {
            // Try memory cache first
            if (_memoryCache.Exists(key))
            {
                return await _memoryCache.GetOrCreateAsync(key, factory, expiration, priority, tag);
            }

            // Try distributed cache if enabled
            if (_settings.EnableDistributedCache)
            {
                try
                {
                    var distributedValue = await _distributedCache.GetAsync<T>(key);
                    if (distributedValue != null)
                    {
                        // Store in memory cache for faster subsequent access
                        _memoryCache.Set(key, distributedValue, TimeSpan.FromMinutes(_settings.ShortTermExpiryMinutes), priority, tag);
                        return distributedValue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get value from distributed cache, falling back to factory");
                }
            }

            // Create new value
            var value = await factory();
            
            // Store in both caches
            _memoryCache.Set(key, value, expiration, priority, tag);
            
            if (_settings.EnableDistributedCache)
            {
                try
                {
                    await _distributedCache.SetAsync(key, value, expiration);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to set value in distributed cache");
                }
            }

            return value;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal, string? tag = null)
        {
            _memoryCache.Set(key, value, expiration, priority, tag);
            
            if (_settings.EnableDistributedCache)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _distributedCache.SetAsync(key, value, expiration);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to set value in distributed cache");
                    }
                });
            }
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            
            if (_settings.EnableDistributedCache)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _distributedCache.RemoveAsync(key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to remove value from distributed cache");
                    }
                });
            }
        }

        public void RemoveByPrefix(string prefix)
        {
            _memoryCache.RemoveByPrefix(prefix);
            
            if (_settings.EnableDistributedCache)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _distributedCache.RemoveByPatternAsync($"{prefix}*");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to remove values by prefix from distributed cache");
                    }
                });
            }
        }

        public void RemoveByTag(string tag)
        {
            _memoryCache.RemoveByTag(tag);
            // Note: Redis doesn't have native tag support, would need additional implementation
        }

        public void Clear()
        {
            _memoryCache.Clear();
            
            if (_settings.EnableDistributedCache)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _distributedCache.ClearAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clear distributed cache");
                    }
                });
            }
        }

        public bool Exists(string key)
        {
            return _memoryCache.Exists(key);
        }

        public CacheStatistics GetStatistics()
        {
            return _memoryCache.GetStatistics();
        }

        public IEnumerable<CacheKeyInfo> GetKeyInfos()
        {
            return _memoryCache.GetKeyInfos();
        }

        public void WarmUp()
        {
            _memoryCache.WarmUp();
        }
    }
}
