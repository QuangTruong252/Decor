using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DecorStore.API.Interfaces;
using System.Text.Json;

namespace DecorStore.API.Models
{
    /// <summary>
    /// Represents a security event for auditing and monitoring
    /// </summary>
    public class SecurityEvent : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EventCategory { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = string.Empty;

        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [StringLength(100)]
        public string? Username { get; set; }

        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(200)]
        public string Action { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Resource { get; set; }

        public bool Success { get; set; } = true;

        [StringLength(100)]
        public string? ErrorCode { get; set; }

        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Details { get; set; }

        [StringLength(100)]
        public string? SessionId { get; set; }

        [StringLength(100)]
        public string? CorrelationId { get; set; }

        [StringLength(200)]
        public string? RequestPath { get; set; }

        [StringLength(20)]
        public string? HttpMethod { get; set; }

        public int? ResponseStatusCode { get; set; }

        public long? ResponseTimeMs { get; set; }

        [StringLength(100)]
        public string? GeolocationCountry { get; set; }

        [StringLength(100)]
        public string? GeolocationCity { get; set; }

        public bool IsAnomaly { get; set; } = false;

        public decimal? RiskScore { get; set; }

        [StringLength(500)]
        public string? ThreatType { get; set; }

        [StringLength(1000)]
        public string? Recommendations { get; set; }

        public bool RequiresInvestigation { get; set; } = false;

        public bool IsProcessed { get; set; } = false;

        public DateTime? ProcessedAt { get; set; }

        [StringLength(100)]
        public string? ProcessedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // Helper method to set details as JSON
        public void SetDetailsObject(object details)
        {
            Details = JsonSerializer.Serialize(details, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
        }

        // Helper method to get details as typed object
        public T? GetDetailsObject<T>() where T : class
        {
            if (string.IsNullOrEmpty(Details))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(Details, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Security event types
    /// </summary>
    public static class SecurityEventTypes
    {
        public const string Authentication = "AUTHENTICATION";
        public const string Authorization = "AUTHORIZATION";
        public const string TokenManagement = "TOKEN_MANAGEMENT";
        public const string DataAccess = "DATA_ACCESS";
        public const string ApiAccess = "API_ACCESS";
        public const string SecurityViolation = "SECURITY_VIOLATION";
        public const string AccountManagement = "ACCOUNT_MANAGEMENT";
        public const string SystemAccess = "SYSTEM_ACCESS";
    }

    /// <summary>
    /// Security event categories
    /// </summary>
    public static class SecurityEventCategories
    {
        public const string Security = "SECURITY";
        public const string Audit = "AUDIT";
        public const string Compliance = "COMPLIANCE";
        public const string Performance = "PERFORMANCE";
        public const string Error = "ERROR";
    }

    /// <summary>
    /// Security event severity levels
    /// </summary>
    public static class SecurityEventSeverity
    {
        public const string Low = "LOW";
        public const string Medium = "MEDIUM";
        public const string High = "HIGH";
        public const string Critical = "CRITICAL";
    }
}
