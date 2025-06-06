using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    // DTO for image data
    public class ImageDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // DTO for returning product data
    public class ProductDTO
    {
        public ProductDTO()
        {
            // Initialize collections to prevent null reference warnings
            Images = Array.Empty<string>();
            ImageDetails = new List<ImageDTO>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public float AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Backward compatibility
        public string[] Images { get; set; }

        // Detailed image information
        public List<ImageDTO> ImageDetails { get; set; }
    }
    public class CreateProductDTO
    {
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public decimal OriginalPrice { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDigital { get; set; }

        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }

        public string[]? Tags { get; set; }

        public string[]? Images { get; set; }

        // Use ImageIds to associate existing images with product
        public List<int> ImageIds { get; set; } = new List<int>();
    }

    // DTO for updating a product
    public class UpdateProductDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public decimal OriginalPrice { get; set; }

        public int StockQuantity { get; set; }

        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }

        public bool IsDigital { get; set; }

        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }

        public string[]? Tags { get; set; }

        public string[]? Images { get; set; }

        // Use ImageIds to associate existing images with product  
        public List<int> ImageIds { get; set; } = new List<int>();
    }

    // DTO for product filtering/searching
    public class ProductFilterDTO : PaginationParameters
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int? StockQuantityMin { get; set; }
        public int? StockQuantityMax { get; set; }
        public float? MinRating { get; set; }
        public string? SKU { get; set; }
    }
}
