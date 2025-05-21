using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DecorStore.API.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Pending";

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string? ShippingCity { get; set; }

        [StringLength(50)]
        public string? ShippingState { get; set; }

        [StringLength(20)]
        public string? ShippingPostalCode { get; set; }

        [StringLength(50)]
        public string? ShippingCountry { get; set; }

        [StringLength(100)]
        [Phone]
        public string? ContactPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? ContactEmail { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User User { get; set; }

        [ForeignKey("CustomerId")]
        [JsonIgnore]
        public virtual Customer Customer { get; set; }

        [JsonIgnore]
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}