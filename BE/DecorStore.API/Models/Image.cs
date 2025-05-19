using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    public class Image
    {
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

        public int? ProductId { get; set; }
        // Future: public int? CategoryId { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProductId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Product? Product { get; set; }
        // Future: [ForeignKey("CategoryId")] public virtual Category Category { get; set; }
    }
}