using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Repositories;
using DecorStore.API.Exceptions;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace DecorStore.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly string _uploadDirectory;

        public CategoryService(ICategoryRepository categoryRepository, IHttpContextAccessor httpContextAccessor)
        {
            _categoryRepository = categoryRepository;
            // Đặt thư mục uploads trong wwwroot
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
            
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => MapCategoryToDto(c, false));
        }

        public async Task<IEnumerable<CategoryDTO>> GetHierarchicalCategoriesAsync()
        {
            var rootCategories = await _categoryRepository.GetRootCategoriesWithChildrenAsync();
            return rootCategories.Select(c => MapCategoryToDto(c, true));
        }

        public async Task<CategoryDTO> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithChildrenAsync(id);
            if (category == null)
                return null;

            return MapCategoryToDto(category, true);
        }

        public async Task<CategoryDTO> GetCategoryBySlugAsync(string slug)
        {
            var category = await _categoryRepository.GetBySlugAsync(slug);
            if (category == null)
                return null;

            return MapCategoryToDto(category, false);
        }

        public async Task<Category> CreateAsync(CreateCategoryDTO categoryDto)
        {
            // Kiểm tra xem slug đã tồn tại chưa
            if (await _categoryRepository.SlugExistsAsync(categoryDto.Slug))
            {
                throw new InvalidOperationException("Slug already exists");
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Slug = categoryDto.Slug,
                Description = categoryDto.Description,
                ParentId = categoryDto.ParentId,
                CreatedAt = DateTime.UtcNow
            };

            // Xử lý upload ảnh nếu có
            if (categoryDto.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryDto.ImageFile.FileName);
                string filePath = Path.Combine(_uploadDirectory, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await categoryDto.ImageFile.CopyToAsync(stream);
                }
                
                // Lưu đường dẫn tương đối
                category.ImageUrl = "/uploads/categories/" + fileName;
            }

            return await _categoryRepository.CreateAsync(category);
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            // Kiểm tra xem slug đã tồn tại ở category khác chưa
            if (!string.IsNullOrEmpty(categoryDto.Slug) && 
                categoryDto.Slug != category.Slug && 
                await _categoryRepository.SlugExistsAsync(categoryDto.Slug))
            {
                throw new InvalidOperationException("Slug already exists");
            }

            category.Name = categoryDto.Name;
            if (!string.IsNullOrEmpty(categoryDto.Slug))
                category.Slug = categoryDto.Slug;
            
            category.Description = categoryDto.Description;
            category.ParentId = categoryDto.ParentId;

            // Xử lý upload ảnh mới nếu có
            if (categoryDto.ImageFile != null)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                        category.ImageUrl.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // Lưu ảnh mới
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryDto.ImageFile.FileName);
                string filePath = Path.Combine(_uploadDirectory, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await categoryDto.ImageFile.CopyToAsync(stream);
                }
                
                // Cập nhật đường dẫn
                category.ImageUrl = "/uploads/categories/" + fileName;
            }

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithChildrenAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            // Kiểm tra xem có danh mục con không
            if (category.Subcategories != null && category.Subcategories.Any())
                throw new InvalidOperationException("Cannot delete category with subcategories");

            await _categoryRepository.DeleteAsync(id);
        }

        private CategoryDTO MapCategoryToDto(Category category, bool includeChildren)
        {
            if (category == null)
                return null;

            var categoryDto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ParentId = category.ParentId,
                ParentName = category.ParentCategory?.Name,
                ImageUrl = category.ImageUrl,
                CreatedAt = category.CreatedAt,
                Subcategories = includeChildren && category.Subcategories != null 
                    ? category.Subcategories.Select(c => MapCategoryToDto(c, includeChildren)).ToList()
                    : new List<CategoryDTO>()
            };

            return categoryDto;
        }
    }
} 