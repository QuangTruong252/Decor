using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Models
{
    /// <summary>
    /// Represents a refresh token for JWT token rotation
    /// </summary>
    public class RefreshToken : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string JwtId { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        [StringLength(256)]
        public string? ReplacedByToken { get; set; }

        [StringLength(100)]
        public string? RevokedByIp { get; set; }

        [StringLength(500)]
        public string? RevokedReason { get; set; }

        [Required]
        [StringLength(100)]
        public string CreatedByIp { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(50)]
        public string? TokenFamily { get; set; }

        public int TokenVersion { get; set; } = 1;

        public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow <= ExpiryDate;

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }
}
