using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DecorStore.API.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Password security fields
        public int AccessFailedCount { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }

        public DateTime? PasswordChangedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; }

        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; }

        [JsonIgnore]
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        [JsonIgnore]
        public virtual ICollection<TokenBlacklist> BlacklistedTokens { get; set; }

        [JsonIgnore]
        public virtual ICollection<SecurityEvent> SecurityEvents { get; set; }

        [JsonIgnore]
        public virtual ICollection<PasswordHistory> PasswordHistory { get; set; }

        public User()
        {
            Orders = new List<Order>();
            Reviews = new List<Review>();
            RefreshTokens = new List<RefreshToken>();
            BlacklistedTokens = new List<TokenBlacklist>();
            SecurityEvents = new List<SecurityEvent>();
            PasswordHistory = new List<PasswordHistory>();
            Username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            FullName = string.Empty;
            Address = string.Empty;
            Phone = string.Empty;
        }
    }
}
