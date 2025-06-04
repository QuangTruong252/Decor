using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.DTOs
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CategoryDTO> Subcategories { get; set; }
        
        // Image support - detailed image information
        public List<ImageDTO> ImageDetails { get; set; } = new List<ImageDTO>();
    }
    
    public class CreateCategoryDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Slug { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
        
        public int? ParentId { get; set; }
        
        public List<int> ImageIds { get; set; } = new List<int>();

    }
    
    public class UpdateCategoryDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        
        [StringLength(100)]
        public string Slug { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
        
        public int? ParentId { get; set; }
        
        public List<int> ImageIds { get; set; } = new List<int>();
    }
}
