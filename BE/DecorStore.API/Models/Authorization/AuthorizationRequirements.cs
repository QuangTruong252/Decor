using Microsoft.AspNetCore.Authorization;

namespace DecorStore.API.Models.Authorization
{
    /// <summary>
    /// Custom authorization requirement for resource ownership validation
    /// </summary>
    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
        public string ResourceType { get; }
        public string ResourceIdParameter { get; }

        public ResourceOwnerRequirement(string resourceType = "resource", string resourceIdParameter = "id")
        {
            ResourceType = resourceType;
            ResourceIdParameter = resourceIdParameter;
        }
    }

    /// <summary>
    /// Custom authorization requirement for minimum age validation
    /// </summary>
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public int MinimumAge { get; }

        public MinimumAgeRequirement(int minimumAge)
        {
            MinimumAge = minimumAge;
        }
    }

    /// <summary>
    /// Custom authorization requirement for business hours validation
    /// </summary>
    public class BusinessHoursRequirement : IAuthorizationRequirement
    {
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public DayOfWeek[] AllowedDays { get; }

        public BusinessHoursRequirement(TimeSpan startTime, TimeSpan endTime, params DayOfWeek[] allowedDays)
        {
            StartTime = startTime;
            EndTime = endTime;
            AllowedDays = allowedDays ?? new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        }
    }

    /// <summary>
    /// Custom authorization requirement for geolocation validation
    /// </summary>
    public class GeolocationRequirement : IAuthorizationRequirement
    {
        public string[] AllowedCountries { get; }
        public string[] BlockedCountries { get; }

        public GeolocationRequirement(string[]? allowedCountries = null, string[]? blockedCountries = null)
        {
            AllowedCountries = allowedCountries ?? Array.Empty<string>();
            BlockedCountries = blockedCountries ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Custom authorization requirement for two-factor authentication
    /// </summary>
    public class TwoFactorRequirement : IAuthorizationRequirement
    {
        public bool RequireRecentAuthentication { get; }
        public TimeSpan MaxAuthenticationAge { get; }

        public TwoFactorRequirement(bool requireRecentAuthentication = true, TimeSpan? maxAuthenticationAge = null)
        {
            RequireRecentAuthentication = requireRecentAuthentication;
            MaxAuthenticationAge = maxAuthenticationAge ?? TimeSpan.FromMinutes(30);
        }
    }

    /// <summary>
    /// Custom authorization requirement for account status validation
    /// </summary>
    public class AccountStatusRequirement : IAuthorizationRequirement
    {
        public string[] RequiredStatuses { get; }
        public string[] ForbiddenStatuses { get; }

        public AccountStatusRequirement(string[]? requiredStatuses = null, string[]? forbiddenStatuses = null)
        {
            RequiredStatuses = requiredStatuses ?? new[] { "Active" };
            ForbiddenStatuses = forbiddenStatuses ?? new[] { "Suspended", "Banned", "Inactive" };
        }
    }

    /// <summary>
    /// Custom authorization requirement for API key validation
    /// </summary>
    public class ApiKeyRequirement : IAuthorizationRequirement
    {
        public string[] RequiredScopes { get; }
        public bool RequireValidKey { get; }

        public ApiKeyRequirement(string[]? requiredScopes = null, bool requireValidKey = true)
        {
            RequiredScopes = requiredScopes ?? Array.Empty<string>();
            RequireValidKey = requireValidKey;
        }
    }
}
