using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DecorStore.API.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Slug { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public int? ParentId { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ParentId")]
        public virtual Category ParentCategory { get; set; }

        public virtual ICollection<Category> Subcategories { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }

        public Category()
        {
            Products = new List<Product>();
            Subcategories = new List<Category>();
            Name = string.Empty;
            Slug = string.Empty;
            Description = string.Empty;
            ImageUrl = string.Empty;
        }
    }
}
