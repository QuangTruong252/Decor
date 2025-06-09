using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Configuration;
using System.Linq;

namespace DecorStore.API.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _settings;
        private readonly ILogger<CacheService> _logger;
        private readonly ConcurrentDictionary<string, string> _keyTags = new();
        private readonly ConcurrentDictionary<string, DateTime> _keyCreationTimes = new();
        private readonly ConcurrentDictionary<string, long> _keyAccessCounts = new();
        private readonly object _lockObject = new object();

        // Performance metrics
        private long _cacheHits = 0;
        private long _cacheMisses = 0;
        private long _totalRequests = 0;

        public CacheService(IMemoryCache cache, IOptions<CacheSettings> settings, ILogger<CacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            if (!_settings.EnableCaching)
            {
                return factory();
            }

            var fullKey = GetFullKey(key);
            Interlocked.Increment(ref _totalRequests);

            if (_cache.TryGetValue(fullKey, out var cachedItem) && cachedItem is T typedItem)
            {
                Interlocked.Increment(ref _cacheHits);
                RecordAccess(fullKey);
                _logger.LogDebug("Cache hit for key: {Key}", fullKey);
                return typedItem;
            }

            Interlocked.Increment(ref _cacheMisses);
            _logger.LogDebug("Cache miss for key: {Key}", fullKey);

            T item = factory();
            Set(fullKey, item, expiration);
            
            return item;
        }        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (!_settings.EnableCaching)
            {
                return await factory();
            }

            var fullKey = GetFullKey(key);
            Interlocked.Increment(ref _totalRequests);

            if (_cache.TryGetValue(fullKey, out var cachedItem) && cachedItem is T typedItem)
            {
                Interlocked.Increment(ref _cacheHits);
                RecordAccess(fullKey);
                _logger.LogDebug("Cache hit for key: {Key}", fullKey);
                return typedItem;
            }

            Interlocked.Increment(ref _cacheMisses);
            _logger.LogDebug("Cache miss for key: {Key}", fullKey);

            T item = await factory();
            Set(fullKey, item, expiration);
            
            return item;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, CacheItemPriority priority, string? tag = null)
        {
            if (!_settings.EnableCaching)
            {
                return await factory();
            }

            var fullKey = GetFullKey(key);
            Interlocked.Increment(ref _totalRequests);

            if (_cache.TryGetValue(fullKey, out var cachedItem) && cachedItem is T typedItem)
            {
                Interlocked.Increment(ref _cacheHits);
                RecordAccess(fullKey);
                _logger.LogDebug("Cache hit for key: {Key}", fullKey);
                return typedItem;
            }

            Interlocked.Increment(ref _cacheMisses);
            _logger.LogDebug("Cache miss for key: {Key}", fullKey);

            T item = await factory();
            Set(fullKey, item, expiration, priority, tag);
            
            return item;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null, CacheItemPriority priority = CacheItemPriority.Normal, string? tag = null)
        {
            if (!_settings.EnableCaching) return;

            var fullKey = GetFullKey(key);
            var expirationTime = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expirationTime)
                .SetPriority(priority)
                .SetSize(1)
                .RegisterPostEvictionCallback(OnCacheItemEvicted);

            // Add sliding expiration if configured
            if (_settings.SlidingExpirationMinutes > 0)
            {
                cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes));
            }

            _cache.Set(fullKey, value, cacheEntryOptions);
            
            // Track metadata
            _keyCreationTimes[fullKey] = DateTime.UtcNow;
            _keyAccessCounts[fullKey] = 0;
            
            if (!string.IsNullOrEmpty(tag))
            {
                _keyTags[fullKey] = tag;
            }

            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", fullKey, expirationTime);
        }

        public void Remove(string key)
        {
            var fullKey = GetFullKey(key);
            _cache.Remove(fullKey);
            CleanupKeyMetadata(fullKey);
            _logger.LogDebug("Cache removed for key: {Key}", fullKey);
        }

        public void RemoveByPrefix(string prefix)
        {
            var fullPrefix = GetFullKey(prefix);
            var keysToRemove = new List<string>();
            
            lock (_lockObject)
            {
                foreach (var key in _keyCreationTimes.Keys)
                {
                    if (key.StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        keysToRemove.Add(key);
                    }
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                CleanupKeyMetadata(key);
            }

            _logger.LogDebug("Cache removed {Count} keys with prefix: {Prefix}", keysToRemove.Count, fullPrefix);
        }

        public void RemoveByTag(string tag)
        {
            var keysToRemove = new List<string>();
            
            lock (_lockObject)
            {
                foreach (var kvp in _keyTags)
                {
                    if (string.Equals(kvp.Value, tag, StringComparison.OrdinalIgnoreCase))
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                CleanupKeyMetadata(key);
            }

            _logger.LogDebug("Cache removed {Count} keys with tag: {Tag}", keysToRemove.Count, tag);
        }

        public void Clear()
        {
            // Get all keys to remove
            var allKeys = _keyCreationTimes.Keys.ToList();
            
            foreach (var key in allKeys)
            {
                _cache.Remove(key);
            }
            
            // Clear all metadata
            _keyCreationTimes.Clear();
            _keyAccessCounts.Clear();
            _keyTags.Clear();

            _logger.LogInformation("Cache cleared completely");
        }

        public bool Exists(string key)
        {
            var fullKey = GetFullKey(key);
            return _cache.TryGetValue(fullKey, out _);
        }

        public CacheStatistics GetStatistics()
        {
            var totalRequests = _totalRequests;
            var hitRatio = totalRequests > 0 ? (double)_cacheHits / totalRequests * 100 : 0;

            return new CacheStatistics
            {
                TotalRequests = totalRequests,
                CacheHits = _cacheHits,
                CacheMisses = _cacheMisses,
                HitRatio = hitRatio,
                TotalKeys = _keyCreationTimes.Count,
                KeysWithTags = _keyTags.Count,
                MemoryCacheSize = GetApproximateCacheSize()
            };
        }

        public IEnumerable<CacheKeyInfo> GetKeyInfos()
        {
            lock (_lockObject)
            {
                return _keyCreationTimes.Select(kvp => new CacheKeyInfo
                {
                    Key = kvp.Key,
                    CreatedAt = kvp.Value,
                    AccessCount = _keyAccessCounts.GetValueOrDefault(kvp.Key, 0),
                    Tag = _keyTags.GetValueOrDefault(kvp.Key)
                }).ToList();
            }
        }

        public void WarmUp()
        {
            if (!_settings.EnableCacheWarming || _settings.CacheWarmupKeys == null)
                return;

            _logger.LogInformation("Starting cache warm-up for {Count} keys", _settings.CacheWarmupKeys.Length);

            foreach (var key in _settings.CacheWarmupKeys)
            {
                try
                {
                    // This would be implemented by specific services
                    _logger.LogDebug("Cache warm-up placeholder for key: {Key}", key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to warm up cache for key: {Key}", key);
                }
            }
        }

        private string GetFullKey(string key)
        {
            return $"{_settings.CacheKeyPrefix}:{key}";
        }

        private void RecordAccess(string fullKey)
        {
            if (_keyAccessCounts.ContainsKey(fullKey))
            {
                _keyAccessCounts.AddOrUpdate(fullKey, 1, (k, v) => v + 1);
            }
        }

        private void CleanupKeyMetadata(string fullKey)
        {
            _keyCreationTimes.TryRemove(fullKey, out _);
            _keyAccessCounts.TryRemove(fullKey, out _);
            _keyTags.TryRemove(fullKey, out _);
        }

        private void OnCacheItemEvicted(object key, object value, EvictionReason reason, object state)
        {
            var keyString = key.ToString();
            CleanupKeyMetadata(keyString);
            _logger.LogDebug("Cache item evicted: {Key}, Reason: {Reason}", keyString, reason);
        }

        private long GetApproximateCacheSize()
        {
            // This is an approximation - actual implementation would depend on cache implementation details
            return _keyCreationTimes.Count * 1024; // Rough estimate
        }
    }

    public class CacheStatistics
    {
        public long TotalRequests { get; set; }
        public long CacheHits { get; set; }
        public long CacheMisses { get; set; }
        public double HitRatio { get; set; }
        public int TotalKeys { get; set; }
        public int KeysWithTags { get; set; }
        public long MemoryCacheSize { get; set; }
    }

    public class CacheKeyInfo
    {
        public string Key { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long AccessCount { get; set; }
        public string? Tag { get; set; }
    }
}
