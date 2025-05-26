using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using AutoMapper;

namespace DecorStore.API.Services
{
    public class CategoryService(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper) : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly string _folderImageName = "categories";
        private readonly IImageService _imageService = imageService;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedResult<CategoryDTO>> GetPagedCategoriesAsync(CategoryFilterDTO filter)
        {
            var pagedCategories = await _unitOfWork.Categories.GetPagedAsync(filter);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(pagedCategories.Items);

            return new PagedResult<CategoryDTO>(categoryDtos, pagedCategories.Pagination.TotalCount,
                pagedCategories.Pagination.CurrentPage, pagedCategories.Pagination.PageSize);
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<IEnumerable<CategoryDTO>> GetHierarchicalCategoriesAsync()
        {
            var rootCategories = await _unitOfWork.Categories.GetRootCategoriesWithChildrenAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(rootCategories);
        }

        public async Task<CategoryDTO> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdWithChildrenAsync(id)
                ?? throw new NotFoundException("Category not found");

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> GetCategoryBySlugAsync(string slug)
        {
            var category = await _unitOfWork.Categories.GetBySlugAsync(slug)
                ?? throw new NotFoundException("Category not found");

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<Category> CreateAsync(CreateCategoryDTO categoryDto)
        {
            if (await _unitOfWork.Categories.SlugExistsAsync(categoryDto.Slug))
            {
                throw new InvalidOperationException("Slug already exists");
            }

            // Check if ParentId exists if it's provided
            if (categoryDto.ParentId.HasValue)
            {
                var parentExists = await _unitOfWork.Categories.ExistsAsync(categoryDto.ParentId.Value);
                if (!parentExists)
                {
                    throw new InvalidOperationException($"Parent category with ID {categoryDto.ParentId.Value} does not exist");
                }
            }

            // Map DTO to entity
            var category = _mapper.Map<Category>(categoryDto);

            // Handle image upload
            if (categoryDto.ImageFile != null)
            {
                var imagePath = await _imageService.UploadImageAsync(categoryDto.ImageFile, _folderImageName);
                category.ImageUrl = imagePath;
            }

            await _unitOfWork.Categories.CreateAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO categoryDto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id)
                ?? throw new NotFoundException("Category not found");

            // Check slug uniqueness
            if (!string.IsNullOrEmpty(categoryDto.Slug) &&
                categoryDto.Slug != category.Slug &&
                await _unitOfWork.Categories.SlugExistsAsync(categoryDto.Slug))
            {
                throw new InvalidOperationException("Slug already exists");
            }

            // Check if ParentId exists if it's provided
            if (categoryDto.ParentId.HasValue)
            {
                // Prevent circular reference (category can't be its own parent)
                if (categoryDto.ParentId.Value == id)
                {
                    throw new InvalidOperationException("Category cannot be its own parent");
                }

                var parentExists = await _unitOfWork.Categories.ExistsAsync(categoryDto.ParentId.Value);
                if (!parentExists)
                {
                    throw new InvalidOperationException($"Parent category with ID {categoryDto.ParentId.Value} does not exist");
                }
            }

            // Map DTO to entity
            _mapper.Map(categoryDto, category);

            // Handle image update if new image is provided
            if (categoryDto.ImageFile != null)
            {
                var imagePath = await _imageService.UpdateImageAsync(category.ImageUrl, categoryDto.ImageFile, _folderImageName);
                category.ImageUrl = imagePath;
            }

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdWithChildrenAsync(id)
                ?? throw new NotFoundException("Category not found");

            // Check if the category has subcategories
            if (category.Subcategories != null && category.Subcategories.Count > 0)
                throw new InvalidOperationException("Cannot delete category with subcategories");
            // Check if the category is used in any products
            if (category.Products != null && category.Products.Count > 0)
                throw new InvalidOperationException("Cannot delete category that is used in products");
            // Delete the image if it exists
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                await _imageService.DeleteImageAsync(category.ImageUrl);
            }
            await _unitOfWork.Categories.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        // Advanced query methods
        public async Task<IEnumerable<CategoryDTO>> GetCategoriesWithProductCountAsync()
        {
            var categories = await _unitOfWork.Categories.GetCategoriesWithProductCountAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<IEnumerable<CategoryDTO>> GetSubcategoriesAsync(int parentId)
        {
            var subcategories = await _unitOfWork.Categories.GetSubcategoriesAsync(parentId);
            return _mapper.Map<IEnumerable<CategoryDTO>>(subcategories);
        }

        public async Task<int> GetProductCountByCategoryAsync(int categoryId)
        {
            return await _unitOfWork.Categories.GetProductCountByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<CategoryDTO>> GetPopularCategoriesAsync(int count = 10)
        {
            var categories = await _unitOfWork.Categories.GetPopularCategoriesAsync(count);
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<IEnumerable<CategoryDTO>> GetRootCategoriesAsync()
        {
            var filter = new CategoryFilterDTO { IsRootCategory = true };
            var pagedResult = await _unitOfWork.Categories.GetPagedAsync(filter);
            return _mapper.Map<IEnumerable<CategoryDTO>>(pagedResult.Items);
        }
    }
}