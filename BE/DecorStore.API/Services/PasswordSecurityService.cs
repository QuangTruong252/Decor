using System.Text.RegularExpressions;
using System.Text;
using DecorStore.API.Common;
using DecorStore.API.Configuration;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Models;
using DecorStore.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Services
{    /// <summary>
    /// Service for password security operations including validation, hashing, and policy enforcement
    /// </summary>
    public class PasswordSecurityService : IPasswordSecurityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PasswordSecurityService> _logger;
        private readonly ISecurityEventLogger _securityLogger;
        private readonly PasswordSecuritySettings _settings;

        // Common weak passwords (in real implementation, load from file or database)
        private static readonly HashSet<string> CommonPasswords = new()
        {
            "password", "123456", "password123", "admin", "qwerty", "letmein",
            "welcome", "monkey", "dragon", "password1", "123456789", "football",
            "iloveyou", "admin123", "welcome123", "sunshine", "princess", "charlie"
        };

        public PasswordSecurityService(
            IUnitOfWork unitOfWork,
            ILogger<PasswordSecurityService> logger,
            ISecurityEventLogger securityLogger,
            PasswordSecuritySettings settings)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _securityLogger = securityLogger;
            _settings = settings;
        }

        public async Task<Result<string>> HashPasswordAsync(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return Result<string>.Failure("Password cannot be empty");
                }

                var salt = BCrypt.Net.BCrypt.GenerateSalt(_settings.SaltRounds);
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
                
                return Result<string>.Success(hashedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                return Result<string>.Failure("Password hashing failed");
            }
        }

        public async Task<Result<bool>> VerifyPasswordAsync(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                {
                    return Result<bool>.Success(false);
                }

                var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return Result<bool>.Failure("Password verification failed");
            }
        }        public async Task<Result<DTOs.PasswordStrengthResult>> ValidatePasswordStrengthAsync(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return Result<DTOs.PasswordStrengthResult>.Failure("Password cannot be empty");
                }

                var issues = new List<string>();
                var suggestions = new List<string>();
                var result = new DTOs.PasswordStrengthResult
                {
                    HasMinimumLength = password.Length >= _settings.MinimumLength,
                    HasUppercase = Regex.IsMatch(password, @"[A-Z]"),
                    HasLowercase = Regex.IsMatch(password, @"[a-z]"),
                    HasNumbers = Regex.IsMatch(password, @"\d"),
                    HasSpecialCharacters = Regex.IsMatch(password, @"[\W_]"),
                    IsCommonPassword = CommonPasswords.Contains(password.ToLower()),
                    HasPersonalInfo = false // Could be enhanced to check against user data
                };

                int score = 0;

                // Check minimum length
                if (!result.HasMinimumLength)
                {
                    issues.Add($"Password must be at least {_settings.MinimumLength} characters long");
                    suggestions.Add($"Use at least {_settings.MinimumLength} characters");
                }
                else
                {
                    score += 20;
                }

                // Check maximum length
                if (password.Length > _settings.MaximumLength)
                {
                    issues.Add($"Password cannot exceed {_settings.MaximumLength} characters");
                }

                // Check for uppercase letters
                if (_settings.RequireUppercase && !result.HasUppercase)
                {
                    issues.Add("Password must contain at least one uppercase letter");
                    suggestions.Add("Add uppercase letters (A-Z)");
                }
                else if (result.HasUppercase)
                {
                    score += 20;
                }

                // Check for lowercase letters
                if (_settings.RequireLowercase && !result.HasLowercase)
                {
                    issues.Add("Password must contain at least one lowercase letter");
                    suggestions.Add("Add lowercase letters (a-z)");
                }
                else if (result.HasLowercase)
                {
                    score += 20;
                }

                // Check for digits
                if (_settings.RequireDigit && !result.HasNumbers)
                {
                    issues.Add("Password must contain at least one digit");
                    suggestions.Add("Add numbers (0-9)");
                }
                else if (result.HasNumbers)
                {
                    score += 20;
                }

                // Check for special characters
                if (_settings.RequireSpecialCharacter && !result.HasSpecialCharacters)
                {
                    issues.Add("Password must contain at least one special character");
                    suggestions.Add("Add special characters (!@#$%^&*)");
                }
                else if (result.HasSpecialCharacters)
                {
                    score += 20;
                }

                // Check for common passwords
                if (_settings.BlockCommonPasswords && result.IsCommonPassword)
                {
                    issues.Add("Password is too common and not allowed");
                    suggestions.Add("Use a more unique password");
                    score -= 40;
                }

                // Check for sequential characters
                if (_settings.BlockSequentialCharacters && HasSequentialCharacters(password))
                {
                    issues.Add("Password cannot contain sequential characters (e.g., abc, 123)");
                    suggestions.Add("Avoid sequential characters");
                    score -= 20;
                }

                // Check for repeated characters
                if (_settings.BlockRepeatedCharacters && HasRepeatedCharacters(password))
                {
                    issues.Add("Password cannot contain more than 2 consecutive identical characters");
                    suggestions.Add("Avoid repeating characters");
                    score -= 10;
                }

                result.Score = Math.Max(0, Math.Min(100, score));
                result.IsStrong = score >= 80 && issues.Count == 0;
                result.Issues = issues.ToArray();
                result.Suggestions = suggestions.ToArray();

                return Result<DTOs.PasswordStrengthResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password strength");
                return Result<DTOs.PasswordStrengthResult>.Failure("Password validation failed");
            }
        }

        public async Task<Result<bool>> CheckPasswordHistoryAsync(int userId, string password)
        {
            try
            {
                if (!_settings.EnablePasswordHistory)
                {
                    return Result<bool>.Success(true);
                }

                var passwordHistory = await _unitOfWork.Context.Set<PasswordHistory>()
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedAt)
                    .Take(_settings.PasswordHistoryCount)
                    .ToListAsync();

                foreach (var historicalPassword in passwordHistory)
                {
                    var verifyResult = await VerifyPasswordAsync(password, historicalPassword.HashedPassword);
                    if (verifyResult.IsSuccess && verifyResult.Data)
                    {
                        return Result<bool>.Failure($"Password cannot be the same as any of the last {_settings.PasswordHistoryCount} passwords");
                    }
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password history for user {UserId}", userId);
                return Result<bool>.Failure("Password history check failed");
            }
        }

        public async Task<Result<bool>> CheckPasswordBreachAsync(string password)
        {
            try
            {
                if (!_settings.EnableBreachDetection)
                {
                    return Result<bool>.Success(true);
                }

                // In a real implementation, you would check against services like HaveIBeenPwned
                // For now, we'll check against our common passwords list
                if (CommonPasswords.Contains(password.ToLower()))
                {
                    await _securityLogger.LogSecurityViolationAsync(
                        "PASSWORD_BREACH_DETECTED", 
                        null, 
                        "System", 
                        "Attempt to use breached password", 
                        0.9m);

                    return Result<bool>.Failure("This password has been found in data breaches and cannot be used");
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password breach");
                return Result<bool>.Failure("Password breach check failed");
            }
        }

        public async Task<Result<bool>> IsAccountLockedAsync(int userId)
        {
            try
            {
                if (!_settings.EnableAccountLockout)
                {
                    return Result<bool>.Success(false);
                }

                var user = await _unitOfWork.Context.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                {
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Success(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking account lockout status for user {UserId}", userId);
                return Result<bool>.Failure("Account lockout check failed");
            }
        }

        public async Task<Result> RecordFailedLoginAsync(int userId, string ipAddress)
        {
            try
            {
                if (!_settings.EnableAccountLockout)
                {
                    return Result.Success();
                }

                var user = await _unitOfWork.Context.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return Result.Failure("User not found");
                }

                user.AccessFailedCount++;
                user.UpdatedAt = DateTime.UtcNow;

                // Lock account if threshold reached
                if (user.AccessFailedCount >= _settings.MaxFailedAccessAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(_settings.LockoutDurationMinutes);
                    
                    await _securityLogger.LogSecurityViolationAsync(
                        "ACCOUNT_LOCKED", 
                        userId, 
                        ipAddress, 
                        $"Account locked due to {user.AccessFailedCount} failed login attempts", 
                        0.8m);

                    _logger.LogWarning("Account {UserId} locked due to failed login attempts from IP {IpAddress}", 
                        userId, ipAddress);
                }

                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording failed login for user {UserId}", userId);
                return Result.Failure("Failed to record login attempt");
            }
        }

        public async Task<Result> UnlockAccountAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Context.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return Result.Failure("User not found");
                }

                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                await _securityLogger.LogSecurityViolationAsync(
                    "ACCOUNT_UNLOCKED", 
                    userId, 
                    "System", 
                    "Account manually unlocked", 
                    0.3m);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking account for user {UserId}", userId);
                return Result.Failure("Failed to unlock account");
            }
        }

        public async Task<Result> AddPasswordToHistoryAsync(int userId, string hashedPassword)
        {
            try
            {
                if (!_settings.EnablePasswordHistory)
                {
                    return Result.Success();
                }

                var passwordHistory = new PasswordHistory
                {
                    UserId = userId,
                    HashedPassword = hashedPassword,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Context.Set<PasswordHistory>().Add(passwordHistory);

                // Clean up old password history entries
                var oldEntries = await _unitOfWork.Context.Set<PasswordHistory>()
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedAt)
                    .Skip(_settings.PasswordHistoryCount)
                    .ToListAsync();

                if (oldEntries.Any())
                {
                    _unitOfWork.Context.Set<PasswordHistory>().RemoveRange(oldEntries);
                }

                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding password to history for user {UserId}", userId);
                return Result.Failure("Failed to add password to history");
            }
        }

        public async Task<Result<bool>> IsPasswordExpiredAsync(int userId)
        {
            try
            {
                if (!_settings.EnablePasswordExpiration)
                {
                    return Result<bool>.Success(false);
                }

                var user = await _unitOfWork.Context.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                if (user.PasswordChangedAt.HasValue)
                {
                    var daysSinceChange = (DateTime.UtcNow - user.PasswordChangedAt.Value).TotalDays;
                    return Result<bool>.Success(daysSinceChange >= _settings.PasswordExpirationDays);
                }

                // If no password change date recorded, consider it expired if older than creation
                var daysSinceCreation = (DateTime.UtcNow - user.CreatedAt).TotalDays;
                return Result<bool>.Success(daysSinceCreation >= _settings.PasswordExpirationDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password expiration for user {UserId}", userId);
                return Result<bool>.Failure("Password expiration check failed");
            }
        }

        private static bool HasSequentialCharacters(string password)
        {
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (char.IsLetterOrDigit(password[i]) && 
                    char.IsLetterOrDigit(password[i + 1]) && 
                    char.IsLetterOrDigit(password[i + 2]))
                {
                    var char1 = char.ToLower(password[i]);
                    var char2 = char.ToLower(password[i + 1]);
                    var char3 = char.ToLower(password[i + 2]);

                    if ((char2 == char1 + 1 && char3 == char2 + 1) ||
                        (char2 == char1 - 1 && char3 == char2 - 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }        private static bool HasRepeatedCharacters(string password)
        {
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<Result<string>> GenerateSecurePasswordAsync(int length = 12, bool includeSpecialChars = true)
        {
            try
            {
                const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string lowercase = "abcdefghijklmnopqrstuvwxyz";
                const string digits = "0123456789";
                const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

                var validChars = uppercase + lowercase + digits;
                if (includeSpecialChars)
                {
                    validChars += specialChars;
                }

                var random = new Random();
                var password = new StringBuilder();

                // Ensure at least one character from each required category
                password.Append(uppercase[random.Next(uppercase.Length)]);
                password.Append(lowercase[random.Next(lowercase.Length)]);
                password.Append(digits[random.Next(digits.Length)]);
                if (includeSpecialChars)
                {
                    password.Append(specialChars[random.Next(specialChars.Length)]);
                }

                // Fill the rest with random characters
                for (int i = password.Length; i < length; i++)
                {
                    password.Append(validChars[random.Next(validChars.Length)]);
                }

                // Shuffle the password
                var chars = password.ToString().ToCharArray();
                for (int i = chars.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (chars[i], chars[j]) = (chars[j], chars[i]);
                }

                return Result<string>.Success(new string(chars));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating secure password");
                return Result<string>.Failure("Password generation failed");
            }
        }

        public async Task<Result<DTOs.AccountLockResult>> CheckAccountLockAsync(int userId)
        {
            try
            {
                if (!_settings.EnableAccountLockout)
                {
                    return Result<DTOs.AccountLockResult>.Success(new DTOs.AccountLockResult
                    {
                        IsLocked = false,
                        FailedAttempts = 0,
                        MaxAttempts = _settings.MaxFailedAccessAttempts
                    });
                }

                var lockoutRecord = await _unitOfWork.Context.Set<AccountLockout>()
                    .FirstOrDefaultAsync(al => al.UserId == userId);

                if (lockoutRecord == null)
                {
                    return Result<DTOs.AccountLockResult>.Success(new DTOs.AccountLockResult
                    {
                        IsLocked = false,
                        FailedAttempts = 0,
                        MaxAttempts = _settings.MaxFailedAccessAttempts
                    });
                }                var lockoutEnd = lockoutRecord.LockoutEnd;
                var isCurrentlyLocked = lockoutRecord.IsCurrentlyLocked;

                return Result<DTOs.AccountLockResult>.Success(new DTOs.AccountLockResult
                {
                    IsLocked = isCurrentlyLocked,
                    LockoutEnd = lockoutEnd,
                    FailedAttempts = lockoutRecord.FailedAttempts,
                    MaxAttempts = _settings.MaxFailedAccessAttempts,
                    LockoutDuration = TimeSpan.FromMinutes(_settings.LockoutDurationMinutes),
                    Reason = lockoutRecord.Reason ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking account lock status for user {UserId}", userId);
                return Result<DTOs.AccountLockResult>.Failure("Failed to check account lock status");
            }
        }

        public async Task<Result> ResetFailedLoginAttemptsAsync(int userId)
        {
            try
            {
                var lockoutRecord = await _unitOfWork.Context.Set<AccountLockout>()
                    .FirstOrDefaultAsync(al => al.UserId == userId);                if (lockoutRecord != null)
                {
                    lockoutRecord.FailedAttempts = 0;
                    lockoutRecord.LockoutEndTime = DateTime.UtcNow.AddMinutes(-1); // Set to past time to unlock
                    lockoutRecord.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }

                await _securityLogger.LogSecurityViolationAsync(
                    "FAILED_ATTEMPTS_RESET",
                    userId,
                    "System",
                    "Failed login attempts reset",
                    0.2m);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting failed login attempts for user {UserId}", userId);
                return Result.Failure("Failed to reset failed login attempts");
            }
        }

        public async Task<Result<DateTime?>> GetPasswordExpirationAsync(int userId)
        {
            try
            {
                if (!_settings.EnablePasswordExpiration)
                {
                    return Result<DateTime?>.Success(null);
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<DateTime?>.Failure("User not found");
                }

                // Get the last password change date from history or user creation date
                var lastPasswordChange = await _unitOfWork.Context.Set<PasswordHistory>()
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedAt)
                    .Select(ph => ph.CreatedAt)
                    .FirstOrDefaultAsync();

                var passwordSetDate = lastPasswordChange != default ? lastPasswordChange : user.CreatedAt;
                var expirationDate = passwordSetDate.AddDays(_settings.PasswordExpirationDays);

                return Result<DateTime?>.Success(expirationDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting password expiration for user {UserId}", userId);
                return Result<DateTime?>.Failure("Failed to get password expiration");
            }
        }
    }
}
