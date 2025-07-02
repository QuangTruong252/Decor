using DecorStore.API.Common;
using DecorStore.API.Models;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DecorStore.API.Services
{
    public class SecurityEventLogger : ISecurityEventLogger
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SecurityEventLogger> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityEventLogger(
            IUnitOfWork unitOfWork,
            ILogger<SecurityEventLogger> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // Interface implementation methods
        public async Task LogAuthenticationAttemptAsync(string userId, bool success, string ipAddress, string userAgent = "", Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.Authentication,
                    EventCategory = SecurityEventCategories.Security,
                    Severity = success ? SecurityEventSeverity.Low : SecurityEventSeverity.Medium,
                    Username = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Action = success ? "LOGIN_SUCCESS" : "LOGIN_FAILED",
                    Success = success,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                if (additionalData != null)
                {
                    securityEvent.SetDetailsObject(additionalData);
                }

                if (!success)
                {
                    securityEvent.RiskScore = await CalculateAuthenticationRiskScoreAsync(userId, ipAddress);
                    if (securityEvent.RiskScore > 0.7m)
                    {
                        securityEvent.RequiresInvestigation = true;
                        securityEvent.Severity = SecurityEventSeverity.High;
                    }
                }

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging authentication attempt for {UserId}", userId);
            }
        }

        public async Task LogAuthorizationFailureAsync(string userId, string resource, string action, string ipAddress, string reason = "")
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.Authorization,
                    EventCategory = SecurityEventCategories.Security,
                    Severity = SecurityEventSeverity.Medium,
                    Username = userId,
                    IpAddress = ipAddress,
                    Action = "AUTHORIZATION_DENIED",
                    Resource = resource,
                    Success = false,
                    ErrorMessage = reason,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod(),
                    RiskScore = 0.6m
                };

                var details = new Dictionary<string, object>
                {
                    ["resource"] = resource,
                    ["action"] = action,
                    ["reason"] = reason
                };

                securityEvent.SetDetailsObject(details);
                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging authorization failure for user {UserId}", userId);
            }
        }

        public async Task LogSuspiciousActivityAsync(string userId, string activityType, string description, string ipAddress, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.SecurityViolation,
                    EventCategory = SecurityEventCategories.Security,
                    Severity = SecurityEventSeverity.High,
                    Username = userId,
                    IpAddress = ipAddress,
                    Action = "SUSPICIOUS_ACTIVITY",
                    Success = false,
                    Details = description,
                    RiskScore = 0.8m,
                    RequiresInvestigation = true,
                    ThreatType = activityType,
                    IsAnomaly = true,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                if (metadata != null)
                {
                    securityEvent.SetDetailsObject(metadata);
                }

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging suspicious activity {ActivityType}", activityType);
            }
        }

        public async Task LogDataAccessAsync(string userId, string resource, string action, string ipAddress, bool success = true)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.DataAccess,
                    EventCategory = SecurityEventCategories.Audit,
                    Severity = SecurityEventSeverity.Low,
                    Username = userId,
                    IpAddress = ipAddress,
                    Action = action,
                    Resource = resource,
                    Success = success,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                if (action.Contains("DELETE") || action.Contains("UPDATE") || resource.Contains("ADMIN"))
                {
                    securityEvent.Severity = SecurityEventSeverity.Medium;
                }

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging data access for user {UserId}", userId);
            }
        }

        public async Task LogPrivilegeEscalationAsync(string userId, string fromRole, string toRole, string ipAddress, bool success = false)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.Authorization,
                    EventCategory = SecurityEventCategories.Security,
                    Severity = SecurityEventSeverity.High,
                    Username = userId,
                    IpAddress = ipAddress,
                    Action = "PRIVILEGE_ESCALATION",
                    Success = success,
                    RiskScore = success ? 0.9m : 0.7m,
                    RequiresInvestigation = true,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                var details = new Dictionary<string, object>
                {
                    ["from_role"] = fromRole,
                    ["to_role"] = toRole,
                    ["success"] = success
                };

                securityEvent.SetDetailsObject(details);
                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging privilege escalation for user {UserId}", userId);
            }
        }

        public async Task LogApiAbuseAsync(string userId, string endpoint, string ipAddress, string abuseType, Dictionary<string, object>? details = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.ApiAccess,
                    EventCategory = SecurityEventCategories.Security,
                    Severity = SecurityEventSeverity.High,
                    Username = userId,
                    IpAddress = ipAddress,
                    Action = "API_ABUSE",
                    Resource = endpoint,
                    Success = false,
                    ThreatType = abuseType,
                    RiskScore = 0.8m,
                    RequiresInvestigation = true,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = endpoint,
                    HttpMethod = GetHttpMethod()
                };

                if (details != null)
                {
                    securityEvent.SetDetailsObject(details);
                }

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging API abuse for user {UserId}", userId);
            }
        }

        public async Task LogSecurityConfigChangeAsync(string userId, string configType, string oldValue, string newValue, string ipAddress)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.SystemAccess,
                    EventCategory = SecurityEventCategories.Audit,
                    Severity = SecurityEventSeverity.Medium,
                    Username = userId,
                    IpAddress = ipAddress,
                    Action = "CONFIG_CHANGE",
                    Resource = configType,
                    Success = true,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                var details = new Dictionary<string, object>
                {
                    ["config_type"] = configType,
                    ["old_value"] = oldValue,
                    ["new_value"] = newValue
                };

                securityEvent.SetDetailsObject(details);
                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security config change for user {UserId}", userId);
            }
        }

        public async Task LogPasswordChangeAsync(string userId, bool success, string ipAddress, string reason = "")
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.Authentication,
                    EventCategory = SecurityEventCategories.Security,
                    Severity = SecurityEventSeverity.Medium,
                    Username = userId,
                    IpAddress = ipAddress,
                    Action = success ? "PASSWORD_CHANGE_SUCCESS" : "PASSWORD_CHANGE_FAILED",
                    Success = success,
                    ErrorMessage = reason,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging password change for user {UserId}", userId);
            }
        }

        public async Task<Result<PaginatedResult<SecurityEventDTO>>> GetSecurityEventsAsync(SecurityEventFilterDTO filter)
        {
            try
            {
                var query = _unitOfWork.Context.Set<SecurityEvent>().AsQueryable();

                if (filter.StartDate.HasValue)
                    query = query.Where(e => e.Timestamp >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(e => e.Timestamp <= filter.EndDate.Value);

                if (!string.IsNullOrEmpty(filter.EventType))
                    query = query.Where(e => e.EventType == filter.EventType);

                if (!string.IsNullOrEmpty(filter.UserId))
                    query = query.Where(e => e.Username == filter.UserId);

                if (!string.IsNullOrEmpty(filter.IpAddress))
                    query = query.Where(e => e.IpAddress == filter.IpAddress);                if (!string.IsNullOrEmpty(filter.Severity))
                    query = query.Where(e => e.Severity == filter.Severity);

                var totalCount = await query.CountAsync();
                
                var events = await query
                    .OrderByDescending(e => e.Timestamp)
                    .Skip(filter.Skip ?? 0)
                    .Take(filter.Take ?? 50)
                    .Select(e => new SecurityEventDTO
                    {
                        Id = e.Id,
                        EventType = e.EventType,
                        Action = e.Action,
                        UserId = e.Username,
                        IpAddress = e.IpAddress,
                        Success = e.Success,
                        Timestamp = e.Timestamp,
                        Severity = e.Severity,
                        RiskScore = e.RiskScore ?? 0.0m,
                        RequiresInvestigation = e.RequiresInvestigation
                    })
                    .ToListAsync();

                var result = new PaginatedResult<SecurityEventDTO>
                {
                    Items = events,
                    TotalCount = totalCount,
                    Page = (filter.Skip ?? 0) / (filter.Take ?? 50) + 1,
                    PageSize = filter.Take ?? 50
                };

                return Result<PaginatedResult<SecurityEventDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security events");
                return Result<PaginatedResult<SecurityEventDTO>>.Failure("Failed to retrieve security events");
            }
        }

        public async Task<Result<SecurityStatsDTO>> GetSecurityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var fromDate = startDate ?? DateTime.UtcNow.AddDays(-7);
                var toDate = endDate ?? DateTime.UtcNow;

                var events = await _unitOfWork.Context.Set<SecurityEvent>()
                    .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate)
                    .ToListAsync();

                var stats = new SecurityStatsDTO
                {
                    TotalEvents = events.Count,
                    AuthenticationEvents = events.Count(e => e.EventType == SecurityEventTypes.Authentication),
                    AuthorizationEvents = events.Count(e => e.EventType == SecurityEventTypes.Authorization),
                    SecurityViolations = events.Count(e => e.EventType == SecurityEventTypes.SecurityViolation),
                    FailedEvents = events.Count(e => !e.Success),
                    HighRiskEvents = events.Count(e => e.RiskScore >= 0.7m),
                    EventsRequiringInvestigation = events.Count(e => e.RequiresInvestigation),
                    UniqueIpAddresses = events.Select(e => e.IpAddress).Distinct().Count(),
                    UniqueUsers = events.Where(e => !string.IsNullOrEmpty(e.Username)).Select(e => e.Username).Distinct().Count(),
                    From = fromDate,
                    To = toDate
                };

                return Result<SecurityStatsDTO>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security stats");
                return Result<SecurityStatsDTO>.Failure("Failed to generate security stats");
            }
        }

        public async Task<Result<IEnumerable<SecurityAlertDTO>>> CheckSecurityAlertsAsync()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-24);
                var alertEvents = await _unitOfWork.Context.Set<SecurityEvent>()
                    .Where(e => e.RequiresInvestigation && 
                               e.Timestamp >= cutoffTime && 
                               !e.IsProcessed)
                    .OrderByDescending(e => e.RiskScore)
                    .Take(50)
                    .ToListAsync();                var alerts = alertEvents.Select(e => new SecurityAlertDTO
                {
                    Id = e.Id,
                    EventType = e.EventType,
                    Action = e.Action,
                    UserId = e.Username,
                    IpAddress = e.IpAddress,
                    RiskScore = e.RiskScore ?? 0.0m,
                    Severity = e.Severity,
                    Timestamp = e.Timestamp,
                    Description = e.Details ?? e.Action,
                    ThreatType = e.ThreatType,
                    AlertType = e.EventType,
                    Message = e.Details ?? e.Action
                });

                return Result<IEnumerable<SecurityAlertDTO>>.Success(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking security alerts");
                return Result<IEnumerable<SecurityAlertDTO>>.Failure("Failed to check security alerts");
            }
        }

        public async Task<Result<int>> ArchiveOldEventsAsync(int daysToKeep = 365)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldEvents = await _unitOfWork.Context.Set<SecurityEvent>()
                    .Where(e => e.Timestamp < cutoffDate)
                    .ToListAsync();

                var archivedCount = oldEvents.Count;
                
                if (archivedCount > 0)
                {
                    _unitOfWork.Context.Set<SecurityEvent>().RemoveRange(oldEvents);
                    await _unitOfWork.SaveChangesAsync();
                    
                    _logger.LogInformation("Archived {Count} old security events", archivedCount);
                }

                return Result<int>.Success(archivedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving old security events");
                return Result<int>.Failure("Failed to archive old security events");
            }
        }

        // Additional legacy methods for backward compatibility
        public async Task<Result> LogAuthenticationAttemptAsync(string? username, string email, bool success, string ipAddress, string? userAgent = null, string? errorCode = null)
        {
            var additionalData = new Dictionary<string, object>
            {
                ["email"] = email,
                ["username"] = username ?? "",
                ["attempt_type"] = "login",
                ["user_agent"] = userAgent ?? ""
            };

            await LogAuthenticationAttemptAsync(username ?? email, success, ipAddress, userAgent ?? "", additionalData);
            return Result.Success();
        }

        public async Task<Result> LogTokenEventAsync(string eventType, int? userId, string ipAddress, bool success, string? details = null)
        {
            await LogDataAccessAsync(userId?.ToString() ?? "UNKNOWN", "TOKEN", eventType, ipAddress, success);
            return Result.Success();
        }

        // Helper methods
        private async Task SaveSecurityEventAsync(SecurityEvent securityEvent)
        {
            try
            {
                _unitOfWork.Context.Set<SecurityEvent>().Add(securityEvent);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving security event");
                throw;
            }
        }

        private async Task<decimal> CalculateAuthenticationRiskScoreAsync(string userId, string ipAddress)
        {
            try
            {
                var recentFailures = await _unitOfWork.Context.Set<SecurityEvent>()
                    .Where(e => e.EventType == SecurityEventTypes.Authentication && 
                               !e.Success && 
                               (e.Username == userId || e.IpAddress == ipAddress) &&
                               e.Timestamp >= DateTime.UtcNow.AddHours(-1))
                    .CountAsync();

                return recentFailures switch
                {
                    >= 5 => 0.9m,
                    >= 3 => 0.7m,
                    >= 2 => 0.5m,
                    1 => 0.3m,
                    _ => 0.1m
                };
            }
            catch
            {
                return 0.1m;
            }
        }

        private string? GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.TraceIdentifier;
        }

        private string? GetRequestPath()
        {
            return _httpContextAccessor.HttpContext?.Request.Path;
        }

        private string? GetHttpMethod()
        {
            return _httpContextAccessor.HttpContext?.Request.Method;
        }

        private string? GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress.Split(',')[0].Trim();
            }

            ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }

        public async Task LogSecurityViolationAsync(string eventType, int? userId, string ipAddress, string details, decimal riskScore, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.SecurityViolation,
                    EventCategory = SecurityEventCategories.Security,
                    Severity = riskScore > 0.7m ? SecurityEventSeverity.High : SecurityEventSeverity.Medium,
                    Username = userId?.ToString(),
                    IpAddress = ipAddress,
                    Action = eventType,
                    Success = false,
                    Details = details,
                    RiskScore = riskScore,
                    RequiresInvestigation = riskScore > 0.7m,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                if (additionalData != null)
                {
                    securityEvent.SetDetailsObject(additionalData);
                }

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security violation {EventType}", eventType);
            }
        }

        public async Task LogSystemEventAsync(string eventType, int? userId, string details, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.SystemAccess,
                    EventCategory = SecurityEventCategories.Audit,
                    Severity = SecurityEventSeverity.Low,
                    Username = userId?.ToString(),
                    IpAddress = GetClientIpAddress() ?? "SYSTEM",
                    Action = eventType,
                    Success = true,
                    Details = details,
                    RiskScore = 0.1m,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                if (additionalData != null)
                {
                    securityEvent.SetDetailsObject(additionalData);
                }

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging system event {EventType}", eventType);
            }
        }        public async Task LogSystemEventAsync(string eventType, string details, bool success, string? additionalData = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = SecurityEventTypes.SystemAccess,
                    EventCategory = SecurityEventCategories.Audit,
                    Severity = success ? SecurityEventSeverity.Low : SecurityEventSeverity.Medium,
                    IpAddress = GetClientIpAddress() ?? "SYSTEM",
                    Action = eventType,
                    Success = success,
                    Details = details,
                    RiskScore = success ? 0.1m : 0.3m,
                    CorrelationId = GetCorrelationId(),
                    RequestPath = GetRequestPath(),
                    HttpMethod = GetHttpMethod()
                };

                if (!string.IsNullOrEmpty(additionalData))
                {
                    securityEvent.Details = $"{details} - {additionalData}";
                }

                await SaveSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging system event {EventType}", eventType);
            }
        }

        public async Task ProcessSecurityAlertsAsync()
        {
            try
            {
                var alertsResult = await CheckSecurityAlertsAsync();
                if (!alertsResult.IsSuccess || !alertsResult.Data.Any())
                {
                    return;
                }

                foreach (var alert in alertsResult.Data)
                {
                    await ProcessIndividualAlertAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing security alerts");
            }
        }

        private async Task ProcessIndividualAlertAsync(SecurityAlertDTO alert)
        {
            try
            {                // Log the alert processing
                var userIdInt = alert.UserId != null && int.TryParse(alert.UserId, out var parsedUserId) ? parsedUserId : (int?)null;
                await LogSystemEventAsync("ALERT_PROCESSING", userIdInt, 
                    $"Processing security alert: {alert.AlertType} - {alert.Message}", 
                    new Dictionary<string, object> 
                    { 
                        { "AlertId", alert.Id },
                        { "AlertType", alert.AlertType },
                        { "Severity", alert.Severity }
                    });

                // Take action based on alert type and severity
                switch (alert.AlertType.ToUpper())
                {
                    case "BRUTE_FORCE":
                        await HandleBruteForceAlert(alert);
                        break;
                    case "SUSPICIOUS_LOGIN":
                        await HandleSuspiciousLoginAlert(alert);
                        break;
                    case "PRIVILEGE_ESCALATION":
                        await HandlePrivilegeEscalationAlert(alert);
                        break;
                    case "API_ABUSE":
                        await HandleApiAbuseAlert(alert);
                        break;
                    default:
                        await HandleGenericAlert(alert);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing individual alert {AlertId}", alert.Id);
            }
        }

        private async Task HandleBruteForceAlert(SecurityAlertDTO alert)
        {
            // Implement brute force response
            await LogSystemEventAsync("BRUTE_FORCE_RESPONSE", 
                $"Responding to brute force attack from {alert.IpAddress}", true);
        }

        private async Task HandleSuspiciousLoginAlert(SecurityAlertDTO alert)
        {
            // Implement suspicious login response
            await LogSystemEventAsync("SUSPICIOUS_LOGIN_RESPONSE", 
                $"Responding to suspicious login from {alert.IpAddress}", true);
        }

        private async Task HandlePrivilegeEscalationAlert(SecurityAlertDTO alert)
        {
            // Implement privilege escalation response
            await LogSystemEventAsync("PRIVILEGE_ESCALATION_RESPONSE", 
                $"Responding to privilege escalation attempt by user {alert.UserId}", true);
        }

        private async Task HandleApiAbuseAlert(SecurityAlertDTO alert)
        {
            // Implement API abuse response
            await LogSystemEventAsync("API_ABUSE_RESPONSE", 
                $"Responding to API abuse from {alert.IpAddress}", true);
        }

        private async Task HandleGenericAlert(SecurityAlertDTO alert)
        {
            // Implement generic alert response
            await LogSystemEventAsync("GENERIC_ALERT_RESPONSE", 
                $"Responding to security alert: {alert.AlertType}", true);
        }
    }
}
