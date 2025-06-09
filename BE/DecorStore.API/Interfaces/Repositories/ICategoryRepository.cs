using DecorStore.API.Models;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Repositories.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // Paginated queries (GetPagedAsync inherited from base)
        Task<PagedResult<Category>> GetPagedAsync(CategoryFilterDTO filter);
        Task<IEnumerable<Category>> GetRootCategoriesWithChildrenAsync();

        // Single item queries (GetByIdAsync inherited from base)
        Task<Category> GetByIdWithChildrenAsync(int id);
        Task<Category> GetBySlugAsync(string slug);

        // Count and existence checks (ExistsAsync inherited from base)
        Task<bool> SlugExistsAsync(string slug);
        Task<bool> SlugExistsAsync(string slug, int excludeCategoryId);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int excludeCategoryId);
        Task<bool> ExistsBySlugAsync(string slug);
        Task<bool> ExistsBySlugAsync(string slug, int excludeCategoryId);
        Task<List<int>> GetDescendantIdsAsync(int categoryId);
        Task<int> GetTotalCountAsync(CategoryFilterDTO filter);

        // Advanced queries
        Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync();
        Task<IEnumerable<Category>> GetSubcategoriesAsync(int parentId);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
        Task<IEnumerable<Category>> GetPopularCategoriesAsync(int count = 10);
        Task<IEnumerable<Category>> GetAllForExcelExportAsync();
    }
}
