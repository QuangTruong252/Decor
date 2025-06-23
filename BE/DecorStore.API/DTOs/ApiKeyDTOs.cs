using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    /// <summary>
    /// DTO for creating a new API key
    /// </summary>
    public class CreateApiKeyDTO
    {
        [Required(ErrorMessage = "API key name is required")]
        [StringLength(100, ErrorMessage = "API key name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Scopes are required")]
        public string[] Scopes { get; set; } = Array.Empty<string>();

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing API key
    /// </summary>
    public class UpdateApiKeyDTO
    {
        [Required(ErrorMessage = "API key name is required")]
        [StringLength(100, ErrorMessage = "API key name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public string[] Scopes { get; set; } = Array.Empty<string>();

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for API key information
    /// </summary>
    public class ApiKeyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string KeyPreview { get; set; } = string.Empty; // Only first 8 characters
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for API key validation result
    /// </summary>
    public class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for API key usage request
    /// </summary>
    public class ApiKeyUsageRequest
    {
        public int ApiKeyId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
        
        // Security-related properties
        public bool IsSuspicious { get; set; } = false;
        public string SuspiciousReason { get; set; } = string.Empty;
        public decimal RiskScore { get; set; } = 0.0m;
    }

    /// <summary>
    /// DTO for API key usage statistics
    /// </summary>
    public class ApiKeyUsageStatsDTO
    {
        public int ApiKeyId { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int RequestsToday { get; set; }
        public int RequestsThisWeek { get; set; }
        public int RequestsThisMonth { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string MostUsedEndpoint { get; set; } = string.Empty;
        public Dictionary<string, int> EndpointUsage { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> DailyUsage { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// DTO for API key analytics
    /// </summary>
    public class ApiKeyAnalyticsDTO
    {
        public int TotalApiKeys { get; set; }
        public int ActiveApiKeys { get; set; }
        public int ExpiredApiKeys { get; set; }
        public int RevokedApiKeys { get; set; }
        public int TotalRequests { get; set; }
        public int RequestsToday { get; set; }
        public int UniqueUsersToday { get; set; }
        public List<ApiKeyUsageStatsDTO> TopUsedKeys { get; set; } = new List<ApiKeyUsageStatsDTO>();
        public Dictionary<string, int> RequestsByScope { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RequestsByDay { get; set; } = new Dictionary<string, int>();
        public List<string> RecentActivity { get; set; } = new List<string>();
    }
}
