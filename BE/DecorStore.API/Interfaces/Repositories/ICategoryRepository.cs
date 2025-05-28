using DecorStore.API.Models;
using DecorStore.API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public interface ICategoryRepository
    {
        // Paginated queries
        Task<PagedResult<Category>> GetPagedAsync(CategoryFilterDTO filter);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetRootCategoriesWithChildrenAsync();

        // Single item queries
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByIdWithChildrenAsync(int id);
        Task<Category> GetBySlugAsync(string slug);

        // Count and existence checks
        Task<bool> ExistsAsync(int id);
        Task<bool> SlugExistsAsync(string slug);
        Task<int> GetTotalCountAsync(CategoryFilterDTO filter);

        // CRUD operations
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);

        // Advanced queries
        Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync();
        Task<IEnumerable<Category>> GetSubcategoriesAsync(int parentId);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
        Task<IEnumerable<Category>> GetPopularCategoriesAsync(int count = 10);

        /// <summary>
        /// Gets all categories with navigation properties for Excel export
        /// </summary>
        Task<IEnumerable<Category>> GetAllForExcelExportAsync();
    }
}