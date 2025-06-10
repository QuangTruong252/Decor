using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    /// <summary>
    /// Represents account lockout information for security tracking
    /// </summary>
    public class AccountLockout
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime LockoutStartTime { get; set; }

        [Required]
        public DateTime LockoutEndTime { get; set; }        [Required]
        [StringLength(50)]
        public string LockoutReason { get; set; } = string.Empty;

        [Required]
        public int FailedAttempts { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? UnlockedAt { get; set; }

        [StringLength(100)]
        public string? UnlockedBy { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Additional properties for compatibility
        public DateTime LockoutEnd => LockoutEndTime;
        public string Reason => LockoutReason;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public AccountLockout()
        {
        }

        public AccountLockout(int userId, string username, string email, string reason, int failedAttempts, TimeSpan lockoutDuration)
        {
            UserId = userId;
            Username = username;
            Email = email;
            LockoutReason = reason;
            FailedAttempts = failedAttempts;
            LockoutStartTime = DateTime.UtcNow;
            LockoutEndTime = DateTime.UtcNow.Add(lockoutDuration);
            IsActive = true;
        }

        /// <summary>
        /// Checks if the lockout is still active
        /// </summary>
        public bool IsCurrentlyLocked => IsActive && DateTime.UtcNow < LockoutEndTime;

        /// <summary>
        /// Gets the remaining lockout duration
        /// </summary>
        public TimeSpan RemainingLockoutDuration => IsCurrentlyLocked ? LockoutEndTime - DateTime.UtcNow : TimeSpan.Zero;

        /// <summary>
        /// Unlocks the account
        /// </summary>
        public void Unlock(string unlockedBy = "System")
        {
            IsActive = false;
            UnlockedAt = DateTime.UtcNow;
            UnlockedBy = unlockedBy;
        }
    }
}
