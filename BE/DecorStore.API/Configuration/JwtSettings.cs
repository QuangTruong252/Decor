using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.Configuration
{
    public class JwtSettings
    {
        [Required(ErrorMessage = "JWT Secret Key is required")]
        [MinLength(32, ErrorMessage = "JWT Secret Key must be at least 32 characters for security")]
        public string SecretKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "JWT Issuer is required")]
        [MinLength(3, ErrorMessage = "JWT Issuer must be at least 3 characters")]
        public string Issuer { get; set; } = string.Empty;

        [Required(ErrorMessage = "JWT Audience is required")]
        [MinLength(3, ErrorMessage = "JWT Audience must be at least 3 characters")]
        public string Audience { get; set; } = string.Empty;        [Range(1, 43200, ErrorMessage = "Access token expiry must be between 1 and 43200 minutes (30 days)")]
        public int AccessTokenExpirationMinutes { get; set; } = 60;

        [Range(1, 365, ErrorMessage = "Refresh token expiry must be between 1 and 365 days")]
        public int RefreshTokenExpirationDays { get; set; } = 7;

        [Range(0, 30, ErrorMessage = "Clock skew must be between 0 and 30 minutes")]
        public int ClockSkewMinutes { get; set; } = 5;

        public bool RequireHttpsMetadata { get; set; } = true;

        public bool SaveToken { get; set; } = true;

        public bool ValidateIssuer { get; set; } = true;

        public bool ValidateAudience { get; set; } = true;

        public bool ValidateLifetime { get; set; } = true;

        public bool ValidateIssuerSigningKey { get; set; } = true;

        public bool EnableDebugEvents { get; set; } = false;
    }
}
