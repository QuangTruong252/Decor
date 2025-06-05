using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Common;

namespace DecorStore.API.Interfaces.Services
{
    public interface IProductService
    {
        // Pagination methods
        Task<Result<PagedResult<ProductDTO>>> GetPagedProductsAsync(ProductFilterDTO filter);
        Task<Result<IEnumerable<Product>>> GetAllAsync(ProductFilterDTO filter);

        // CRUD operations
        Task<Result<ProductDTO>> CreateAsync(CreateProductDTO productDto);
        Task<Result> UpdateAsync(int id, UpdateProductDTO productDto);
        Task<Result> DeleteProductAsync(int id);
        Task<Result> BulkDeleteProductsAsync(BulkDeleteDTO bulkDeleteDto);

        // Single item queries
        Task<Result<IEnumerable<ProductDTO>>> GetAllProductsAsync();
        Task<Result<ProductDTO>> GetProductByIdAsync(int id);

        // Image management
        Task<Result> AddImageToProductAsync(int productId, IFormFile image);
        Task<Result> RemoveImageFromProductAsync(int productId, int imageId);

        // Utility methods (kept unchanged for internal use)
        IEnumerable<ProductDTO> MapToProductDTOs(IEnumerable<Product> products);

        // Advanced queries
        Task<Result<IEnumerable<ProductDTO>>> GetFeaturedProductsAsync(int count = 10);
        Task<Result<IEnumerable<ProductDTO>>> GetProductsByCategoryAsync(int categoryId, int count = 20);
        Task<Result<IEnumerable<ProductDTO>>> GetRelatedProductsAsync(int productId, int count = 5);
        Task<Result<IEnumerable<ProductDTO>>> GetTopRatedProductsAsync(int count = 10);
        Task<Result<IEnumerable<ProductDTO>>> GetLowStockProductsAsync(int threshold = 10);
    }
}
