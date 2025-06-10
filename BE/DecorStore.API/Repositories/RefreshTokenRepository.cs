using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using DecorStore.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Repositories
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && rt.IsActive && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<RefreshToken>> GetTokensByFamilyAsync(string tokenFamily)
        {
            return await _dbSet
                .Where(rt => rt.TokenFamily == tokenFamily && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<RefreshToken>> GetExpiredTokensAsync(DateTime cutoffDate)
        {
            return await _dbSet
                .Where(rt => rt.ExpiryDate <= cutoffDate && !rt.IsDeleted)
                .ToListAsync();
        }

        public async Task<int> CleanupExpiredTokensAsync(DateTime cutoffDate)
        {
            var expiredTokens = await GetExpiredTokensAsync(cutoffDate);
            
            foreach (var token in expiredTokens)
            {
                token.IsDeleted = true;
                token.UpdatedAt = DateTime.UtcNow;
            }

            return expiredTokens.Count;
        }

        public async Task<int> CleanupUserTokensAsync(int userId, int maxTokensToKeep = 5)
        {
            var userTokens = await _dbSet
                .Where(rt => rt.UserId == userId && rt.IsActive && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();

            if (userTokens.Count <= maxTokensToKeep)
                return 0;

            var tokensToRemove = userTokens.Skip(maxTokensToKeep).ToList();
            
            foreach (var token in tokensToRemove)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "Exceeded maximum active tokens";
                token.UpdatedAt = DateTime.UtcNow;
            }

            return tokensToRemove.Count;
        }

        public async Task RevokeTokenFamilyAsync(string tokenFamily, string reason, string? revokedByIp = null)
        {
            var familyTokens = await GetTokensByFamilyAsync(tokenFamily);
            
            foreach (var token in familyTokens.Where(t => t.IsActive))
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = reason;
                token.RevokedByIp = revokedByIp;
                token.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var refreshToken = await GetByTokenAsync(token);
            return refreshToken?.IsActive == true;
        }

        public async Task<int> GetTokenCountByUserAsync(int userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && rt.IsActive && !rt.IsDeleted)
                .CountAsync();
        }
    }
}
