using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Repositories.Base;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        // Product-specific paginated queries
        Task<PagedResult<Product>> GetPagedAsync(ProductFilterDTO filter);
        Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter);

        // Product-specific single item queries
        Task<Product?> GetBySlugAsync(string slug);

        // Product-specific count and existence checks
        Task<int> GetTotalCountAsync(ProductFilterDTO filter);
        Task<bool> SlugExistsAsync(string slug);
        Task<bool> SlugExistsAsync(string slug, int excludeProductId);
        Task<bool> SkuExistsAsync(string sku);
        Task<bool> SkuExistsAsync(string sku, int excludeProductId);

        // Product-specific advanced queries
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int count = 20);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 5);
        Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    }
}
