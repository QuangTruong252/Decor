using DecorStore.API.Common;
using DecorStore.API.DTOs;

namespace DecorStore.API.Interfaces.Services
{
    /// <summary>
    /// Interface for security event logging operations
    /// </summary>
    public interface ISecurityEventLogger
    {
        /// <summary>
        /// Log authentication attempt
        /// </summary>
        Task LogAuthenticationAttemptAsync(string userId, bool success, string ipAddress, string userAgent = "", Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Log authorization failure
        /// </summary>
        Task LogAuthorizationFailureAsync(string userId, string resource, string action, string ipAddress, string reason = "");

        /// <summary>
        /// Log suspicious activity
        /// </summary>
        Task LogSuspiciousActivityAsync(string userId, string activityType, string description, string ipAddress, Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Log data access
        /// </summary>
        Task LogDataAccessAsync(string userId, string resource, string action, string ipAddress, bool success = true);

        /// <summary>
        /// Log privilege escalation attempt
        /// </summary>
        Task LogPrivilegeEscalationAsync(string userId, string fromRole, string toRole, string ipAddress, bool success = false);

        /// <summary>
        /// Log API abuse
        /// </summary>
        Task LogApiAbuseAsync(string userId, string endpoint, string ipAddress, string abuseType, Dictionary<string, object>? details = null);

        /// <summary>
        /// Log security configuration change
        /// </summary>
        Task LogSecurityConfigChangeAsync(string userId, string configType, string oldValue, string newValue, string ipAddress);

        /// <summary>
        /// Log password change
        /// </summary>
        Task LogPasswordChangeAsync(string userId, bool success, string ipAddress, string reason = "");

        /// <summary>
        /// Get security events
        /// </summary>
        Task<Result<PaginatedResult<SecurityEventDTO>>> GetSecurityEventsAsync(SecurityEventFilterDTO filter);

        /// <summary>
        /// Get security statistics
        /// </summary>
        Task<Result<SecurityStatsDTO>> GetSecurityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);        /// <summary>
        /// Check for security alerts
        /// </summary>
        Task<Result<IEnumerable<SecurityAlertDTO>>> CheckSecurityAlertsAsync();

        /// <summary>
        /// Process security alerts and trigger actions
        /// </summary>
        Task ProcessSecurityAlertsAsync();

        /// <summary>
        /// Archive old security events
        /// </summary>
        Task<Result<int>> ArchiveOldEventsAsync(int daysToKeep = 365);

        /// <summary>
        /// Log security violation with risk score
        /// </summary>
        Task LogSecurityViolationAsync(string eventType, int? userId, string ipAddress, string details, decimal riskScore, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Log system event
        /// </summary>
        Task LogSystemEventAsync(string eventType, int? userId, string details, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Log system event with success flag
        /// </summary>
        Task LogSystemEventAsync(string eventType, string details, bool success, string? additionalData = null);
    }
}
