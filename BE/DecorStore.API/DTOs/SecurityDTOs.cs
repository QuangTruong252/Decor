using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    /// <summary>
    /// DTO for token refresh request
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for token response
    /// </summary>
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserDTO User { get; set; } = null!;
    }

    /// <summary>
    /// DTO for token revocation request
    /// </summary>
    public class RevokeTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;

        public string Reason { get; set; } = "User logout";
    }

    /// <summary>
    /// DTO for token blacklist request
    /// </summary>
    public class BlacklistTokenRequest
    {
        [Required(ErrorMessage = "JWT ID is required")]
        public string JwtId { get; set; } = string.Empty;

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(50)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for security event response
    /// </summary>
    public class SecurityEventDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Resource { get; set; }
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Details { get; set; }
        public decimal? RiskScore { get; set; }
        public string? ThreatType { get; set; }
        public bool RequiresInvestigation { get; set; }
        public bool IsAnomaly { get; set; }
        public string? Recommendations { get; set; }
    }

    /// <summary>
    /// DTO for security events filter
    /// </summary>
    public class SecurityEventsFilter : PaginationParameters
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? EventType { get; set; }
        public int? UserId { get; set; }
        public string? Severity { get; set; }
        public bool? RequiresInvestigation { get; set; }
        public string? IpAddress { get; set; }
        public new string? SortBy { get; set; } = "Timestamp";
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// DTO for security summary response
    /// </summary>
    public class SecuritySummaryDto
    {
        public int TotalEvents { get; set; }
        public int AuthenticationEvents { get; set; }
        public int AuthorizationEvents { get; set; }
        public int SecurityViolations { get; set; }
        public int FailedEvents { get; set; }
        public int HighRiskEvents { get; set; }
        public int EventsRequiringInvestigation { get; set; }
        public int UniqueIpAddresses { get; set; }
        public int UniqueUsers { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<SecurityTrendDto> DailyTrends { get; set; } = new();
        public List<TopThreatDto> TopThreats { get; set; } = new();
        public List<RiskScoreDistributionDto> RiskDistribution { get; set; } = new();
    }

    /// <summary>
    /// DTO for security trend data
    /// </summary>
    public class SecurityTrendDto
    {
        public DateTime Date { get; set; }
        public int EventCount { get; set; }
        public int FailedEvents { get; set; }
        public int HighRiskEvents { get; set; }
        public decimal AverageRiskScore { get; set; }
    }

    /// <summary>
    /// DTO for top threat data
    /// </summary>
    public class TopThreatDto
    {
        public string ThreatType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal AverageRiskScore { get; set; }
        public DateTime LastOccurrence { get; set; }
    }

    /// <summary>
    /// DTO for risk score distribution
    /// </summary>
    public class RiskScoreDistributionDto
    {
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// DTO for refresh token information
    /// </summary>
    public class RefreshTokenDto
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? TokenFamily { get; set; }
        public int TokenVersion { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
    }

    /// <summary>
    /// DTO for token blacklist information
    /// </summary>
    public class TokenBlacklistDto
    {
        public int Id { get; set; }
        public string JwtId { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BlacklistReason { get; set; } = string.Empty;
        public string? BlacklistedByIp { get; set; }
        public string BlacklistType { get; set; } = string.Empty;
        public bool IsRevocationPermanent { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for token management filter
    /// </summary>
    public class TokenManagementFilter : PaginationParameters
    {
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsRevoked { get; set; }
        public string? TokenFamily { get; set; }
        public new string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// DTO for blacklist management filter
    /// </summary>
    public class BlacklistManagementFilter : PaginationParameters
    {
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? BlacklistType { get; set; }
        public string? BlacklistReason { get; set; }
        public bool? IsActive { get; set; }
        public new string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// DTO for user security overview
    /// </summary>
    public class UserSecurityOverviewDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ActiveTokens { get; set; }
        public int TotalTokens { get; set; }
        public int BlacklistedTokens { get; set; }
        public int SecurityEvents { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public decimal CurrentRiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public bool RequiresInvestigation { get; set; }
        public List<SecurityEventDto> RecentEvents { get; set; } = new();
        public List<RefreshTokenDto> ActiveTokensList { get; set; } = new();
    }

    /// <summary>
    /// DTO for JWT security settings configuration
    /// </summary>
    public class JwtSecuritySettingsDto
    {
        public int AccessTokenExpiryMinutes { get; set; } = 15;
        public int RefreshTokenExpiryDays { get; set; } = 7;
        public bool RequireHttpsMetadata { get; set; } = true;
        public bool ValidateIssuer { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public bool EnableTokenEncryption { get; set; } = true;
        public bool EnableTokenRotation { get; set; } = true;
        public bool EnableTokenBlacklisting { get; set; } = true;
        public int MaxRefreshTokenFamilySize { get; set; } = 5;
        public int TokenBindingDurationMinutes { get; set; } = 30;
        public bool EnableTokenReplayProtection { get; set; } = true;
        public int TokenReplayWindowMinutes { get; set; } = 5;
        public bool EnableSecureTokenStorage { get; set; } = true;
        public bool EnableTokenAuditLogging { get; set; } = true;
    }

    /// <summary>
    /// DTO for security configuration update request
    /// </summary>
    public class UpdateSecurityConfigRequest
    {
        [Range(5, 60, ErrorMessage = "Access token expiry must be between 5 and 60 minutes")]
        public int AccessTokenExpiryMinutes { get; set; } = 15;

        [Range(1, 30, ErrorMessage = "Refresh token expiry must be between 1 and 30 days")]
        public int RefreshTokenExpiryDays { get; set; } = 7;

        public bool EnableTokenEncryption { get; set; } = true;
        public bool EnableTokenRotation { get; set; } = true;
        public bool EnableTokenBlacklisting { get; set; } = true;

        [Range(1, 10, ErrorMessage = "Max refresh token family size must be between 1 and 10")]
        public int MaxRefreshTokenFamilySize { get; set; } = 5;

        [Range(1, 60, ErrorMessage = "Token binding duration must be between 1 and 60 minutes")]
        public int TokenBindingDurationMinutes { get; set; } = 30;

        public bool EnableTokenReplayProtection { get; set; } = true;

        [Range(1, 30, ErrorMessage = "Token replay window must be between 1 and 30 minutes")]
        public int TokenReplayWindowMinutes { get; set; } = 5;

        public bool EnableTokenAuditLogging { get; set; } = true;
    }

    /// <summary>
    /// DTO for token validation result
    /// </summary>
    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public string? JwtId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public DateTime? ExpiresAt { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public bool IsBlacklisted { get; set; }
        public System.Security.Claims.ClaimsPrincipal? ClaimsPrincipal { get; set; }

        /// <summary>
        /// Find first claim with the specified type
        /// </summary>
        public System.Security.Claims.Claim? FindFirst(string claimType)
        {
            return ClaimsPrincipal?.FindFirst(claimType);
        }
    }

    /// <summary>
    /// DTO for refresh token validation result
    /// </summary>
    public class RefreshTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string? TokenFamily { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }
    }

    /// <summary>
    /// DTO for token information
    /// </summary>
    public class TokenInfoDTO
    {
        public string JwtId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string[] Audiences { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// DTO for password strength validation result
    /// </summary>
    public class PasswordStrengthResult
    {
        public bool IsStrong { get; set; }
        public int Score { get; set; }
        public string[] Issues { get; set; } = Array.Empty<string>();
        public string[] Suggestions { get; set; } = Array.Empty<string>();
        public bool HasMinimumLength { get; set; }
        public bool HasUppercase { get; set; }
        public bool HasLowercase { get; set; }
        public bool HasNumbers { get; set; }
        public bool HasSpecialCharacters { get; set; }
        public bool IsCommonPassword { get; set; }
        public bool HasPersonalInfo { get; set; }
    }

    /// <summary>
    /// DTO for account lock result
    /// </summary>
    public class AccountLockResult
    {
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int FailedAttempts { get; set; }
        public int MaxAttempts { get; set; }
        public TimeSpan LockoutDuration { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for security event information
    /// </summary>
    public class SecurityEventDTO
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool Success { get; set; }
        public decimal RiskScore { get; set; }
        public bool RequiresInvestigation { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// DTO for security event filter
    /// </summary>
    public class SecurityEventFilterDTO
    {
        public string? EventType { get; set; }
        public string? UserId { get; set; }
        public string? Severity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? IpAddress { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }

    /// <summary>
    /// DTO for security statistics
    /// </summary>
    public class SecurityStatsDTO
    {
        public int TotalEvents { get; set; }
        public int AuthenticationEvents { get; set; }
        public int AuthorizationEvents { get; set; }
        public int SecurityViolations { get; set; }
        public int FailedEvents { get; set; }
        public int HighRiskEvents { get; set; }
        public int EventsRequiringInvestigation { get; set; }
        public int UniqueIpAddresses { get; set; }
        public int UniqueUsers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EventsToday { get; set; }
        public int EventsThisWeek { get; set; }
        public int HighSeverityEvents { get; set; }
        public int FailedLogins { get; set; }
        public int SuccessfulLogins { get; set; }
        public int BlockedRequests { get; set; }
        public List<string> TopThreats { get; set; } = new List<string>();
        public Dictionary<string, int> EventsByType { get; set; } = new Dictionary<string, int>();
    }    /// <summary>
    /// DTO for security alert
    /// </summary>
    public class SecurityAlertDTO
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public decimal RiskScore { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ThreatType { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic paginated result DTO
    /// </summary>
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }

}
