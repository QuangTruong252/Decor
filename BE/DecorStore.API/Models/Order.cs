using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DecorStore.API.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
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
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
} 