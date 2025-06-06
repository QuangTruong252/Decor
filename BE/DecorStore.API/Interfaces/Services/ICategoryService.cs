using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Common;

namespace DecorStore.API.Services
{
    public interface ICategoryService
    {
        // Pagination methods
        Task<Result<PagedResult<CategoryDTO>>> GetPagedCategoriesAsync(CategoryFilterDTO filter);
        Task<Result<IEnumerable<CategoryDTO>>> GetAllCategoriesAsync();
        Task<Result<IEnumerable<CategoryDTO>>> GetHierarchicalCategoriesAsync();

        // Single item queries
        Task<Result<CategoryDTO>> GetCategoryByIdAsync(int id);
        Task<Result<CategoryDTO>> GetCategoryBySlugAsync(string slug);

        // CRUD operations
        Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO categoryDto);
        Task<Result> UpdateAsync(int id, UpdateCategoryDTO categoryDto);
        Task<Result> DeleteAsync(int id);

        // Advanced queries
        Task<Result<IEnumerable<CategoryDTO>>> GetCategoriesWithProductCountAsync();
        Task<Result<IEnumerable<CategoryDTO>>> GetSubcategoriesAsync(int parentId);
        Task<Result<int>> GetProductCountByCategoryAsync(int categoryId);
        Task<Result<IEnumerable<CategoryDTO>>> GetPopularCategoriesAsync(int count = 10);
        Task<Result<IEnumerable<CategoryDTO>>> GetRootCategoriesAsync();
    }
}