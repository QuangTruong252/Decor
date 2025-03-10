using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.Models;

namespace DecorStore.API.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    }
} 