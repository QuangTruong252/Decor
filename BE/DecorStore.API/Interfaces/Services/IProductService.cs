using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Interfaces.Services
{
    public interface IProductService
    {
        // Pagination methods
        Task<PagedResult<ProductDTO>> GetPagedProductsAsync(ProductFilterDTO filter);
        Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter);

        // CRUD operations
        Task<Product> CreateAsync(CreateProductDTO productDto);
        Task UpdateAsync(int id, UpdateProductDTO productDto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> BulkDeleteProductsAsync(BulkDeleteDTO bulkDeleteDto);

        // Single item queries
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(int id);

        // Image management
        Task<bool> AddImageToProductAsync(int productId, IFormFile image);
        Task<bool> RemoveImageFromProductAsync(int productId, int imageId);

        // Utility methods
        IEnumerable<ProductDTO> MapToProductDTOs(IEnumerable<Product> products);

        // Advanced queries
        Task<IEnumerable<ProductDTO>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(int categoryId, int count = 20);
        Task<IEnumerable<ProductDTO>> GetRelatedProductsAsync(int productId, int count = 5);
        Task<IEnumerable<ProductDTO>> GetTopRatedProductsAsync(int count = 10);
        Task<IEnumerable<ProductDTO>> GetLowStockProductsAsync(int threshold = 10);
    }
}
