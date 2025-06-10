using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Models
{
    /// <summary>
    /// Represents a blacklisted JWT token
    /// </summary>
    public class TokenBlacklist : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string JwtId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [StringLength(50)]
        public string BlacklistReason { get; set; } = string.Empty;

        [StringLength(100)]
        public string? BlacklistedByIp { get; set; }

        [StringLength(500)]
        public string? BlacklistedByUserAgent { get; set; }

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        [Required]
        [StringLength(50)]
        public string BlacklistType { get; set; } = "Manual"; // Manual, Logout, Security, Expired

        public bool IsRevocationPermanent { get; set; } = false;

        public DateTime? AutoRemovalDate { get; set; }

        public bool IsActive => DateTime.UtcNow <= ExpiryDate;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Enumeration for blacklist reasons
    /// </summary>
    public static class BlacklistReasons
    {
        public const string UserLogout = "USER_LOGOUT";
        public const string SecurityBreach = "SECURITY_BREACH";
        public const string AdminRevocation = "ADMIN_REVOCATION";
        public const string TokenCompromised = "TOKEN_COMPROMISED";
        public const string SuspiciousActivity = "SUSPICIOUS_ACTIVITY";
        public const string AccountLocked = "ACCOUNT_LOCKED";
        public const string PasswordChanged = "PASSWORD_CHANGED";
        public const string RoleChanged = "ROLE_CHANGED";
    }

    /// <summary>
    /// Enumeration for blacklist types
    /// </summary>
    public static class BlacklistTypes
    {
        public const string Manual = "MANUAL";
        public const string Logout = "LOGOUT";
        public const string Security = "SECURITY";
        public const string Expired = "EXPIRED";
        public const string System = "SYSTEM";
    }
}
