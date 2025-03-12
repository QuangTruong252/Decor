using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    public class ProductImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
} 