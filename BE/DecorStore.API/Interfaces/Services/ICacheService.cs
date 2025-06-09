using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using DecorStore.API.Services;

namespace DecorStore.API.Interfaces.Services
{    public interface ICacheService
    {
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, CacheItemPriority priority, string? tag = null);
        void Set<T>(string key, T value, TimeSpan? expiration = null, CacheItemPriority priority = CacheItemPriority.Normal, string? tag = null);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
        void RemoveByTag(string tag);
        void Clear();
        bool Exists(string key);
        CacheStatistics GetStatistics();
        IEnumerable<CacheKeyInfo> GetKeyInfos();
        void WarmUp();
    }
}
