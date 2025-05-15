using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryDTO>> GetHierarchicalCategoriesAsync();
        Task<CategoryDTO> GetCategoryByIdAsync(int id);
        Task<CategoryDTO> GetCategoryBySlugAsync(string slug);
        Task<Category> CreateAsync(CreateCategoryDTO categoryDto);
        Task UpdateAsync(int id, UpdateCategoryDTO categoryDto);
        Task DeleteAsync(int id);
    }
} 