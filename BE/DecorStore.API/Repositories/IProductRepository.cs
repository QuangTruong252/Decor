using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;

namespace DecorStore.API.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter);
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetBySlugAsync(string slug);
        Task<int> GetTotalCountAsync(ProductFilterDTO filter);
        Task<bool> ExistsAsync(int id);
        Task<bool> SlugExistsAsync(string slug);
        Task<bool> SkuExistsAsync(string sku);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
} 