using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Models
{
    public class Category : IBaseEntity
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
        public string Description { get; set; }        public int? ParentId { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;        // Navigation properties
        [ForeignKey("ParentId")]
        [JsonIgnore]
        public virtual Category? ParentCategory { get; set; }

        [JsonIgnore]
        public virtual ICollection<Category> Subcategories { get; set; }

        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }

        [JsonIgnore]
        public virtual ICollection<CategoryImage> CategoryImages { get; set; }

        // Computed property for Images access
        [NotMapped]
        [JsonIgnore]
        public virtual ICollection<Image> Images => CategoryImages?.Select(ci => ci.Image).Where(i => i != null).ToList() ?? new List<Image>();

        public Category()
        {
            Products = new List<Product>();
            Subcategories = new List<Category>();
            CategoryImages = new List<CategoryImage>();
            Name = string.Empty;
            Slug = string.Empty;
            Description = string.Empty;
        }
    }
}
