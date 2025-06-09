using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DecorStore.API.Configuration;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Services;
using StackExchange.Redis;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DecorStore.API.Extensions
{    /// <summary>
    /// Extensions for Redis distributed caching services
    /// </summary>
    public static class DistributedCacheServiceExtensions
    {
        /// <summary>
        /// Adds Redis distributed caching services
        /// </summary>
        public static IServiceCollection AddDistributedCacheServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>() ?? new CacheSettings();
            
            if (cacheSettings.EnableDistributedCache)
            {
                // Add Redis cache
                var redisConnectionString = configuration.GetConnectionString("Redis");
                if (!string.IsNullOrEmpty(redisConnectionString))
                {
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = redisConnectionString;
                        options.InstanceName = cacheSettings.CacheKeyPrefix;
                    });

                    // Add Redis connection multiplexer
                    services.AddRedisConnection(configuration);
                }
                else
                {
                    // Fall back to in-memory distributed cache if Redis is not configured
                    services.AddDistributedMemoryCache();
                }
                
                // Add distributed cache service wrapper
                services.AddScoped<IDistributedCacheService, DistributedCacheService>();
            }
            else
            {
                // Add in-memory distributed cache as fallback
                services.AddDistributedMemoryCache();
                services.AddScoped<IDistributedCacheService, DistributedCacheService>();
            }

            return services;
        }
    }    /// <summary>
    /// Implementation of distributed cache service
    /// </summary>
    public class DistributedCacheService : IDistributedCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer? _redis;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<DistributedCacheService> _logger;

        public DistributedCacheService(
            IDistributedCache distributedCache,
            IOptions<CacheSettings> cacheSettings,
            ILogger<DistributedCacheService> logger,
            IConnectionMultiplexer? redis = null)
        {
            _distributedCache = distributedCache;
            _redis = redis;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                var cachedValue = await _distributedCache.GetStringAsync(fullKey, cancellationToken);
                
                if (cachedValue == null)
                    return default(T);

                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from distributed cache for key: {Key}", key);
                return default(T);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                var serializedValue = JsonSerializer.Serialize(value);
                
                var options = new DistributedCacheEntryOptions();
                if (expiration.HasValue)
                {
                    options.SetAbsoluteExpiration(expiration.Value);
                }
                else
                {
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes));
                }

                await _distributedCache.SetStringAsync(fullKey, serializedValue, options, cancellationToken);
                _logger.LogDebug("Set distributed cache value for key: {Key}", fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in distributed cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                await _distributedCache.RemoveAsync(fullKey, cancellationToken);
                _logger.LogDebug("Removed distributed cache value for key: {Key}", fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from distributed cache for key: {Key}", key);
            }
        }        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_redis == null)
                {
                    _logger.LogWarning("Redis connection not available for pattern removal: {Pattern}", pattern);
                    return;
                }

                var database = _redis.GetDatabase(_cacheSettings.RedisDatabase);
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                
                var fullPattern = GetFullKey(pattern);
                var keys = server.Keys(database: _cacheSettings.RedisDatabase, pattern: fullPattern);
                
                var tasks = keys.Select(key => database.KeyDeleteAsync(key));
                await Task.WhenAll(tasks);
                
                _logger.LogDebug("Removed distributed cache values matching pattern: {Pattern}", fullPattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing values by pattern from distributed cache: {Pattern}", pattern);
            }
        }        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {            var fullKey = GetFullKey(key);
            
            if (_redis == null)
                {
                    // Fall back to trying to get the value from distributed cache
                    var value = await _distributedCache.GetStringAsync(fullKey, cancellationToken);
                    return !string.IsNullOrEmpty(value);
                }

                var database = _redis.GetDatabase(_cacheSettings.RedisDatabase);
                return await database.KeyExistsAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in distributed cache: {Key}", key);
                return false;
            }
        }        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
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

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_redis == null)
                {
                    _logger.LogWarning("Redis connection not available for cache clear");
                    return;
                }

                var database = _redis.GetDatabase(_cacheSettings.RedisDatabase);
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                
                var pattern = GetFullKey("*");
                var keys = server.Keys(database: _cacheSettings.RedisDatabase, pattern: pattern);
                
                var tasks = keys.Select(key => database.KeyDeleteAsync(key));
                await Task.WhenAll(tasks);
                
                _logger.LogDebug("Cleared all distributed cache values");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing distributed cache");
            }
        }

        public async Task<long> GetKeysCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_redis == null)
                {
                    return 0;
                }

                var database = _redis.GetDatabase(_cacheSettings.RedisDatabase);
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                
                var pattern = GetFullKey("*");
                var keys = server.Keys(database: _cacheSettings.RedisDatabase, pattern: pattern);
                
                return keys.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keys count from distributed cache");
                return 0;
            }
        }

        public async Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", CancellationToken cancellationToken = default)
        {
            try
            {
                if (_redis == null)
                {
                    return Enumerable.Empty<string>();
                }

                var database = _redis.GetDatabase(_cacheSettings.RedisDatabase);
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                
                var fullPattern = GetFullKey(pattern);
                var keys = server.Keys(database: _cacheSettings.RedisDatabase, pattern: fullPattern);
                
                return keys.Select(k => k.ToString()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keys from distributed cache with pattern: {Pattern}", pattern);
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_redis == null)
                {
                    return false;
                }

                var database = _redis.GetDatabase(_cacheSettings.RedisDatabase);
                await database.PingAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Redis connection");
                return false;
            }
        }

        private string GetFullKey(string key)
        {
            return $"{_cacheSettings.CacheKeyPrefix}:distributed:{key}";
        }
    }

    /// <summary>
    /// Redis connection multiplexer factory
    /// </summary>
    public static class RedisConnectionFactory
    {
        public static IServiceCollection AddRedisConnection(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>() ?? new CacheSettings();
                    var configurationOptions = ConfigurationOptions.Parse(connectionString);
                    configurationOptions.ConnectTimeout = cacheSettings.RedisTimeoutMs;
                    configurationOptions.SyncTimeout = cacheSettings.RedisTimeoutMs;
                    
                    return ConnectionMultiplexer.Connect(configurationOptions);
                });
            }

            return services;
        }
    }
}
