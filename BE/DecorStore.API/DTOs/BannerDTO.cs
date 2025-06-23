using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.DTOs
{    
    public class BannerDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string ImageUrl { get; set; }
        public required string Link { get; set; }
        public required string LinkUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBannerDTO
    {
        [Required]
        [StringLength(100)]
        public required string Title { get; set; }
        
        [Required]
        public required IFormFile ImageFile { get; set; }
        
        [StringLength(255)]
        public required string Link { get; set; }
        
        [StringLength(255)]
        public required string LinkUrl { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateBannerDTO
    {
        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        public IFormFile? ImageFile { get; set; }

        [StringLength(255)]
        public required string Link { get; set; }

        [StringLength(255)]
        public required string LinkUrl { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }
    }
}
