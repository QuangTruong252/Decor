using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Models
{
    /// <summary>
    /// Represents an API key for service-to-service authentication
    /// </summary>
    public class ApiKey : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string KeyHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string KeyPrefix { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedByUser { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? LastUsedAt { get; set; }

        [StringLength(45)]
        public string? LastUsedFromIp { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        public int? RevokedByUserId { get; set; }

        [ForeignKey(nameof(RevokedByUserId))]
        public virtual User? RevokedByUser { get; set; }

        [StringLength(500)]
        public string? RevokedReason { get; set; }

        // Usage tracking
        public long UsageCount { get; set; } = 0;

        public long RateLimitPerHour { get; set; } = 1000;

        public long RateLimitPerDay { get; set; } = 10000;

        // Scopes and permissions
        [Required]
        [StringLength(1000)]
        public string Scopes { get; set; } = string.Empty;

        [StringLength(500)]
        public string AllowedIpAddresses { get; set; } = string.Empty;

        [StringLength(500)]
        public string AllowedDomains { get; set; } = string.Empty;

        // Environment restrictions
        [StringLength(50)]
        public string Environment { get; set; } = "production";

        // Metadata
        [StringLength(2000)]
        public string? Metadata { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<ApiKeyUsage> UsageHistory { get; set; } = new List<ApiKeyUsage>();

        // Helper methods
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public bool IsValidForUse => IsActive && !IsRevoked && !IsExpired && !IsDeleted;

        public List<string> GetScopes()
        {
            return string.IsNullOrEmpty(Scopes) 
                ? new List<string>() 
                : Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .ToList();
        }

        public void SetScopes(IEnumerable<string> scopes)
        {
            Scopes = string.Join(",", scopes);
        }

        public List<string> GetAllowedIpAddresses()
        {
            return string.IsNullOrEmpty(AllowedIpAddresses)
                ? new List<string>()
                : AllowedIpAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(ip => ip.Trim())
                                  .ToList();
        }

        public void SetAllowedIpAddresses(IEnumerable<string> ipAddresses)
        {
            AllowedIpAddresses = string.Join(",", ipAddresses);
        }

        public List<string> GetAllowedDomains()
        {
            return string.IsNullOrEmpty(AllowedDomains)
                ? new List<string>()
                : AllowedDomains.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(domain => domain.Trim())
                               .ToList();
        }

        public void SetAllowedDomains(IEnumerable<string> domains)
        {
            AllowedDomains = string.Join(",", domains);
        }
    }

    /// <summary>
    /// Tracks API key usage for analytics and monitoring
    /// </summary>
    public class ApiKeyUsage : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ApiKeyId { get; set; }

        [ForeignKey(nameof(ApiKeyId))]
        public virtual ApiKey ApiKey { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string HttpMethod { get; set; } = string.Empty;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        public int ResponseStatusCode { get; set; }

        public long ResponseTimeMs { get; set; }

        public long RequestSizeBytes { get; set; }

        public long ResponseSizeBytes { get; set; }

        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        // Security tracking
        public bool IsSuccessful { get; set; }

        public bool IsSuspicious { get; set; }

        [StringLength(500)]
        public string? SuspiciousReason { get; set; }

        public decimal RiskScore { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// API key scopes and permissions
    /// </summary>
    public static class ApiKeyScopes
    {
        // Read permissions
        public const string ReadProducts = "products:read";
        public const string ReadCategories = "categories:read";
        public const string ReadOrders = "orders:read";
        public const string ReadCustomers = "customers:read";
        public const string ReadReviews = "reviews:read";
        public const string ReadBanners = "banners:read";
        public const string ReadImages = "images:read";
        public const string ReadDashboard = "dashboard:read";

        // Write permissions
        public const string WriteProducts = "products:write";
        public const string WriteCategories = "categories:write";
        public const string WriteOrders = "orders:write";
        public const string WriteCustomers = "customers:write";
        public const string WriteReviews = "reviews:write";
        public const string WriteBanners = "banners:write";
        public const string WriteImages = "images:write";

        // Delete permissions
        public const string DeleteProducts = "products:delete";
        public const string DeleteCategories = "categories:delete";
        public const string DeleteOrders = "orders:delete";
        public const string DeleteCustomers = "customers:delete";
        public const string DeleteReviews = "reviews:delete";
        public const string DeleteBanners = "banners:delete";
        public const string DeleteImages = "images:delete";

        // Admin permissions
        public const string AdminAccess = "admin:access";
        public const string AdminUsers = "admin:users";
        public const string AdminSecurity = "admin:security";
        public const string AdminSystem = "admin:system";

        // Analytics permissions
        public const string AnalyticsRead = "analytics:read";
        public const string AnalyticsWrite = "analytics:write";

        // File management permissions
        public const string FilesRead = "files:read";
        public const string FilesWrite = "files:write";
        public const string FilesDelete = "files:delete";

        // All available scopes
        public static readonly List<string> AllScopes = new()
        {
            ReadProducts, ReadCategories, ReadOrders, ReadCustomers, ReadReviews, ReadBanners, ReadImages, ReadDashboard,
            WriteProducts, WriteCategories, WriteOrders, WriteCustomers, WriteReviews, WriteBanners, WriteImages,
            DeleteProducts, DeleteCategories, DeleteOrders, DeleteCustomers, DeleteReviews, DeleteBanners, DeleteImages,
            AdminAccess, AdminUsers, AdminSecurity, AdminSystem,
            AnalyticsRead, AnalyticsWrite,
            FilesRead, FilesWrite, FilesDelete
        };

        // Scope groups for easy assignment
        public static readonly Dictionary<string, List<string>> ScopeGroups = new()
        {
            ["read-only"] = new() { ReadProducts, ReadCategories, ReadOrders, ReadCustomers, ReadReviews, ReadBanners, ReadImages, ReadDashboard },
            ["content-manager"] = new() { ReadProducts, ReadCategories, ReadBanners, ReadImages, WriteProducts, WriteCategories, WriteBanners, WriteImages },
            ["order-manager"] = new() { ReadOrders, ReadCustomers, WriteOrders, WriteCustomers },
            ["full-access"] = AllScopes,
            ["analytics"] = new() { ReadDashboard, AnalyticsRead, AnalyticsWrite },
            ["file-manager"] = new() { FilesRead, FilesWrite, FilesDelete }
        };

        public static bool IsValidScope(string scope)
        {
            return AllScopes.Contains(scope);
        }

        public static List<string> GetScopesForGroup(string groupName)
        {
            return ScopeGroups.TryGetValue(groupName, out var scopes) ? scopes : new List<string>();
        }
    }
}
