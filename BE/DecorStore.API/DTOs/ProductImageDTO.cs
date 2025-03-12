using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    public class ProductImageDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
    
    public class CreateProductImageDTO
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        
        public bool IsDefault { get; set; } = false;
    }
} 