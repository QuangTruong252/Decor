using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DecorStore.API.Models
{
    public class Product
    {
        public Product()
        {
            Images = new List<Image>();
            Reviews = new List<Review>();
            OrderItems = new List<OrderItem>();
            Description = string.Empty;
            Name = string.Empty;
            Slug = string.Empty;
            SKU = string.Empty;
            Category = null!;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Slug { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; }

        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; } = true;

        public float AverageRating { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<Image> Images { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}