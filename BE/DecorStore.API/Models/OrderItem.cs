using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public int ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public OrderItem()
        {
            Order = null!;
            Product = null!;
        }
    }
} 