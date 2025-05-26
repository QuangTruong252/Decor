using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface ICategoryService
    {
        // Pagination methods
        Task<PagedResult<CategoryDTO>> GetPagedCategoriesAsync(CategoryFilterDTO filter);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryDTO>> GetHierarchicalCategoriesAsync();

        // Single item queries
        Task<CategoryDTO> GetCategoryByIdAsync(int id);
        Task<CategoryDTO> GetCategoryBySlugAsync(string slug);

        // CRUD operations
        Task<Category> CreateAsync(CreateCategoryDTO categoryDto);
        Task UpdateAsync(int id, UpdateCategoryDTO categoryDto);
        Task DeleteAsync(int id);

        // Advanced queries
        Task<IEnumerable<CategoryDTO>> GetCategoriesWithProductCountAsync();
        Task<IEnumerable<CategoryDTO>> GetSubcategoriesAsync(int parentId);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
        Task<IEnumerable<CategoryDTO>> GetPopularCategoriesAsync(int count = 10);
        Task<IEnumerable<CategoryDTO>> GetRootCategoriesAsync();
    }
}