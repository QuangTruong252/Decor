using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Services
{
    public interface ICacheService
    {
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
        void Clear();
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _keys = new HashSet<string>();
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedItem))
            {
                return cachedItem;
            }

            T item = factory();
            
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration ?? _defaultExpiration);
            
            _cache.Set(key, item, cacheEntryOptions);
            _keys.Add(key);
            
            return item;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedItem))
            {
                return cachedItem;
            }

            T item = await factory();
            
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration ?? _defaultExpiration);
            
            _cache.Set(key, item, cacheEntryOptions);
            _keys.Add(key);
            
            return item;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _keys.Remove(key);
        }

        public void RemoveByPrefix(string prefix)
        {
            var keysToRemove = new List<string>();
            
            foreach (var key in _keys)
            {
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    keysToRemove.Add(key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
        }

        public void Clear()
        {
            foreach (var key in _keys)
            {
                _cache.Remove(key);
            }
            
            _keys.Clear();
        }
    }
}
