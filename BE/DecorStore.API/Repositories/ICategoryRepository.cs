using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetRootCategoriesWithChildrenAsync();
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByIdWithChildrenAsync(int id);
        Task<Category> GetBySlugAsync(string slug);
        Task<bool> ExistsAsync(int id);
        Task<bool> SlugExistsAsync(string slug);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
} 