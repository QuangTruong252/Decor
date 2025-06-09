using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DecorStore.API.Services
{
    public interface IDistributedCacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
        Task<long> GetKeysCountAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", CancellationToken cancellationToken = default);
        Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    }
}
