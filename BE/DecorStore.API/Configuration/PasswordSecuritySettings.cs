namespace DecorStore.API.Configuration
{
    /// <summary>
    /// Configuration settings for password security
    /// </summary>
    public class PasswordSecuritySettings
    {
        public int MinimumLength { get; set; } = 8;
        public int MaximumLength { get; set; } = 128;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialCharacter { get; set; } = true;
        public bool BlockCommonPasswords { get; set; } = true;
        public bool BlockSequentialCharacters { get; set; } = true;
        public bool BlockRepeatedCharacters { get; set; } = true;
        public int SaltRounds { get; set; } = 12;

        // Account lockout settings
        public bool EnableAccountLockout { get; set; } = true;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 30;

        // Password history settings
        public bool EnablePasswordHistory { get; set; } = true;
        public int PasswordHistoryCount { get; set; } = 5;

        // Password expiration settings
        public bool EnablePasswordExpiration { get; set; } = true;
        public int PasswordExpirationDays { get; set; } = 90;

        // Breach detection settings
        public bool EnableBreachDetection { get; set; } = true;
    }
}
