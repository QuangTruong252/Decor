using DecorStore.API.Common;
using DecorStore.API.DTOs;

namespace DecorStore.API.Interfaces.Services
{
    /// <summary>
    /// Interface for password security operations
    /// </summary>
    public interface IPasswordSecurityService
    {
        /// <summary>
        /// Hash a password using secure algorithms
        /// </summary>
        Task<Result<string>> HashPasswordAsync(string password);

        /// <summary>
        /// Verify a password against its hash
        /// </summary>
        Task<Result<bool>> VerifyPasswordAsync(string password, string hash);

        /// <summary>
        /// Validate password strength
        /// </summary>
        Task<Result<PasswordStrengthResult>> ValidatePasswordStrengthAsync(string password);

        /// <summary>
        /// Check if password has been breached
        /// </summary>
        Task<Result<bool>> CheckPasswordBreachAsync(string password);

        /// <summary>
        /// Generate a secure random password
        /// </summary>
        Task<Result<string>> GenerateSecurePasswordAsync(int length = 12, bool includeSpecialChars = true);

        /// <summary>
        /// Check password history to prevent reuse
        /// </summary>
        Task<Result<bool>> CheckPasswordHistoryAsync(int userId, string newPassword);        /// <summary>
        /// Add password to user's history
        /// </summary>
        Task<Result> AddPasswordToHistoryAsync(int userId, string passwordHash);

        /// <summary>
        /// Check if account should be locked due to failed attempts
        /// </summary>
        Task<Result<AccountLockResult>> CheckAccountLockAsync(int userId);

        /// <summary>
        /// Check if account is currently locked
        /// </summary>
        Task<Result<bool>> IsAccountLockedAsync(int userId);

        /// <summary>
        /// Record failed login attempt
        /// </summary>
        Task<Result> RecordFailedLoginAsync(int userId, string ipAddress);

        /// <summary>
        /// Unlock a locked account
        /// </summary>
        Task<Result> UnlockAccountAsync(int userId);

        /// <summary>
        /// Reset failed login attempts
        /// </summary>
        Task<Result> ResetFailedLoginAttemptsAsync(int userId);

        /// <summary>
        /// Check if password has expired
        /// </summary>
        Task<Result<bool>> IsPasswordExpiredAsync(int userId);

        /// <summary>
        /// Get password expiration date
        /// </summary>
        Task<Result<DateTime?>> GetPasswordExpirationAsync(int userId);
    }
}
