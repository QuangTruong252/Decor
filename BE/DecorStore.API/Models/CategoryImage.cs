using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    public class CategoryImage
    {
        public int CategoryId { get; set; }
        public int ImageId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
        
        [ForeignKey("ImageId")]
        public virtual Image Image { get; set; } = null!;
    }
}
