using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.DTOs
{
    public class BannerDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Link { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateBannerDTO
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        public IFormFile ImageFile { get; set; }
        
        [StringLength(255)]
        public string Link { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
    }
    
    public class UpdateBannerDTO
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        public IFormFile ImageFile { get; set; }
        
        [StringLength(255)]
        public string Link { get; set; }
        
        public bool IsActive { get; set; }
        
        public int DisplayOrder { get; set; }
    }
} 