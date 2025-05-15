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
using DecorStore.API.Interfaces;

namespace DecorStore.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly string _folderImageName = "categories";
        private readonly IImageService _imageService;

        public CategoryService(ICategoryRepository categoryRepository, IHttpContextAccessor httpContextAccessor, IImageService imageService)
        {
            _categoryRepository = categoryRepository;
            _imageService = imageService;
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
                throw new NotFoundException("Category not found");

            return MapCategoryToDto(category, true);
        }

        public async Task<CategoryDTO> GetCategoryBySlugAsync(string slug)
        {
            var category = await _categoryRepository.GetBySlugAsync(slug);
            if (category == null)
                throw new NotFoundException("Category not found");

            return MapCategoryToDto(category, false);
        }

        public async Task<Category> CreateAsync(CreateCategoryDTO categoryDto)
        {
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

            // Handle image upload
            if (categoryDto.ImageFile != null)
            {
                var imagePath = await _imageService.UploadImageAsync(categoryDto.ImageFile, _folderImageName);
                category.ImageUrl = imagePath;
            }

            return await _categoryRepository.CreateAsync(category);
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            // Check slug uniqueness
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

            // Handle image update if new image is provided
            if (categoryDto.ImageFile != null)
            {
                var imagePath = await _imageService.UpdateImageAsync(category.ImageUrl, categoryDto.ImageFile, _folderImageName);
                category.ImageUrl = imagePath;
            }

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithChildrenAsync(id);
            if (category == null)
                throw new NotFoundException("Category not found");

            // Check if the category has subcategories
            if (category.Subcategories != null && category.Subcategories.Any())
                throw new InvalidOperationException("Cannot delete category with subcategories");
            // Check if the category is used in any products
            if (category.Products != null && category.Products.Any())
                throw new InvalidOperationException("Cannot delete category that is used in products");
            // Delete the image if it exists
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                await _imageService.DeleteImageAsync(category.ImageUrl);
            }
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