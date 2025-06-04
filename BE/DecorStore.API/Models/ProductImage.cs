using System.ComponentModel.DataAnnotations.Schema;

namespace DecorStore.API.Models
{
    public class ProductImage
    {
        public int ProductId { get; set; }
        public int ImageId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
        
        [ForeignKey("ImageId")]
        public virtual Image Image { get; set; } = null!;
    }
}
