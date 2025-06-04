using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IProductRepository
    {
        // Paginated queries
        Task<PagedResult<Product>> GetPagedAsync(ProductFilterDTO filter);
        Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter);

        // Single item queries
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetBySlugAsync(string slug);

        // Count and existence checks
        Task<int> GetTotalCountAsync(ProductFilterDTO filter);
        Task<bool> ExistsAsync(int id);
        Task<bool> SlugExistsAsync(string slug);
        Task<bool> SkuExistsAsync(string sku);

        // CRUD operations
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task BulkDeleteAsync(IEnumerable<int> ids);

        // Advanced queries
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int count = 20);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 5);
        Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    }
}
