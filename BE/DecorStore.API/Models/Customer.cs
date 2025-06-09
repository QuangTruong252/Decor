using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DecorStore.API.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public required string Email { get; set; }

        [StringLength(255)]
        public required string Address { get; set; }

        [StringLength(100)]
        public required string City { get; set; }

        [StringLength(50)]
        public required string State { get; set; }

        [StringLength(20)]
        public required string PostalCode { get; set; }

        [StringLength(50)]
        public required string Country { get; set; }

        [StringLength(20)]
        public required string Phone { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        // Computed property for full name
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
