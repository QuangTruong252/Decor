using DecorStore.API.Models;
using DecorStore.API.Interfaces.Repositories.Base;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
        Task<List<RefreshToken>> GetTokensByFamilyAsync(string tokenFamily);
        Task<List<RefreshToken>> GetExpiredTokensAsync(DateTime cutoffDate);
        Task<int> CleanupExpiredTokensAsync(DateTime cutoffDate);
        Task<int> CleanupUserTokensAsync(int userId, int maxTokensToKeep = 5);
        Task RevokeTokenFamilyAsync(string tokenFamily, string reason, string? revokedByIp = null);
        Task<bool> IsTokenValidAsync(string token);
        Task<int> GetTokenCountByUserAsync(int userId);
    }
}
