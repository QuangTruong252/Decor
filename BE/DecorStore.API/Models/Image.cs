using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    public class Image
    {
        public Image()
        {
            ProductImages = new List<ProductImage>();
            CategoryImages = new List<CategoryImage>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(255)]
        public string AltText { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for many-to-many relationships
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<CategoryImage> CategoryImages { get; set; }
    }
}
