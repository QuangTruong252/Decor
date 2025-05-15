using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    // DTO for returning product data
    public class ProductDTO
    {
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
        public string[] Images { get; set; }
    }
    
    // DTO for creating a new product
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
        public string SKU { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        public bool IsFeatured { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // For image upload
        public List<IFormFile>? Images { get; set; }
    }
    
    // DTO for updating a product
    public class UpdateProductDTO
    {
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Name { get; set; }
        
        [StringLength(255)]
        public string Slug { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        public decimal OriginalPrice { get; set; }
        
        public int StockQuantity { get; set; }
        
        [StringLength(50)]
        public string SKU { get; set; }
        
        public int CategoryId { get; set; }
        
        public bool IsFeatured { get; set; }
        
        public bool IsActive { get; set; }
        
        // For image upload
        public List<IFormFile> Images { get; set; }
    }
    
    // DTO for product filtering/searching
    public class ProductFilterDTO
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsFeatured { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
} 