using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
