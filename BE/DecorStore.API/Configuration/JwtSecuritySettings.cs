using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.Configuration
{
    /// <summary>
    /// Enhanced JWT security configuration settings
    /// </summary>
    public class JwtSecuritySettings
    {
        [Range(5, 60, ErrorMessage = "Access token expiry must be between 5 and 60 minutes")]
        public int AccessTokenExpiryMinutes { get; set; } = 15;

        [Range(1, 30, ErrorMessage = "Refresh token expiry must be between 1 and 30 days")]
        public int RefreshTokenExpiryDays { get; set; } = 7;

        public bool RequireHttpsMetadata { get; set; } = true;

        public bool ValidateIssuer { get; set; } = true;

        public bool ValidateAudience { get; set; } = true;

        public bool EnableTokenEncryption { get; set; } = true;

        [Required(ErrorMessage = "Encryption key is required when token encryption is enabled")]
        [MinLength(32, ErrorMessage = "Encryption key must be at least 32 characters")]
        public string EncryptionKey { get; set; } = string.Empty;

        public bool EnableTokenRotation { get; set; } = true;

        public bool EnableTokenBlacklisting { get; set; } = true;

        [Range(1, 10, ErrorMessage = "Max refresh token family size must be between 1 and 10")]
        public int MaxRefreshTokenFamilySize { get; set; } = 5;

        [Range(1, 60, ErrorMessage = "Token binding duration must be between 1 and 60 minutes")]
        public int TokenBindingDurationMinutes { get; set; } = 30;

        public bool EnableTokenReplayProtection { get; set; } = true;

        [Range(1, 30, ErrorMessage = "Token replay window must be between 1 and 30 minutes")]
        public int TokenReplayWindowMinutes { get; set; } = 5;

        public bool EnableSecureTokenStorage { get; set; } = true;

        public bool EnableTokenAuditLogging { get; set; } = true;
    }
}
