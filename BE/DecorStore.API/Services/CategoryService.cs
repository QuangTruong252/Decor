using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using DecorStore.API.Common;
using AutoMapper;

namespace DecorStore.API.Services
{
    public class CategoryService(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper) : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly string _folderImageName = "categories";
        private readonly IImageService _imageService = imageService;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<PagedResult<CategoryDTO>>> GetPagedCategoriesAsync(CategoryFilterDTO filter)
        {
            try
            {
                if (filter == null)
                {
                    return Result<PagedResult<CategoryDTO>>.Failure("INVALID_REQUEST", "Filter cannot be null");
                }

                var pagedCategories = await _unitOfWork.Categories.GetPagedAsync(filter);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(pagedCategories.Items);

                var result = new PagedResult<CategoryDTO>(categoryDtos, pagedCategories.Pagination.TotalCount,
                    pagedCategories.Pagination.CurrentPage, pagedCategories.Pagination.PageSize);

                return Result<PagedResult<CategoryDTO>>.Success(result);
            }            catch (Exception ex)
            {
                return Result<PagedResult<CategoryDTO>>.Failure("Failed to retrieve paged categories", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<CategoryDTO>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
                return Result<IEnumerable<CategoryDTO>>.Success(categoryDtos);
            }            catch (Exception ex)
            {
                return Result<IEnumerable<CategoryDTO>>.Failure("Failed to retrieve all categories", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<CategoryDTO>>> GetHierarchicalCategoriesAsync()
        {
            try
            {
                var rootCategories = await _unitOfWork.Categories.GetRootCategoriesWithChildrenAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(rootCategories);
                return Result<IEnumerable<CategoryDTO>>.Success(categoryDtos);
            }            catch (Exception ex)
            {
                return Result<IEnumerable<CategoryDTO>>.Failure("Failed to retrieve hierarchical categories", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<CategoryDTO>> GetCategoryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result<CategoryDTO>.Failure("INVALID_ID", "Category ID must be greater than 0");
                }

                var category = await _unitOfWork.Categories.GetByIdWithChildrenAsync(id);
                if (category == null)
                {
                    return Result<CategoryDTO>.Failure("NOT_FOUND", $"Category with ID {id} not found");
                }

                var categoryDto = _mapper.Map<CategoryDTO>(category);
                return Result<CategoryDTO>.Success(categoryDto);
            }            catch (Exception ex)
            {
                return Result<CategoryDTO>.Failure("Failed to retrieve category by ID", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<CategoryDTO>> GetCategoryBySlugAsync(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return Result<CategoryDTO>.Failure("INVALID_SLUG", "Category slug cannot be null or empty");
                }

                var category = await _unitOfWork.Categories.GetBySlugAsync(slug);
                if (category == null)
                {
                    return Result<CategoryDTO>.Failure("NOT_FOUND", $"Category with slug '{slug}' not found");
                }

                var categoryDto = _mapper.Map<CategoryDTO>(category);
                return Result<CategoryDTO>.Success(categoryDto);
            }            catch (Exception ex)
            {
                return Result<CategoryDTO>.Failure("Failed to retrieve category by slug", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO categoryDto)
        {
            try
            {
                // Validate input
                var validationResult = await ValidateCreateCategoryDto(categoryDto);
                if (validationResult.IsFailure)
                {
                    return Result<CategoryDTO>.Failure(validationResult.ErrorCode ?? "VALIDATION_ERROR", validationResult.Error ?? "Validation failed", validationResult.ErrorDetails);
                }

                // Check if slug exists
                if (await _unitOfWork.Categories.SlugExistsAsync(categoryDto.Slug))
                {
                    return Result<CategoryDTO>.Failure("SLUG_EXISTS", "A category with this slug already exists");
                }

                // Check if ParentId exists if it's provided
                if (categoryDto.ParentId.HasValue)
                {
                    var parentExists = await _unitOfWork.Categories.ExistsAsync(categoryDto.ParentId.Value);
                    if (!parentExists)
                    {
                        return Result<CategoryDTO>.Failure("PARENT_NOT_FOUND", $"Parent category with ID {categoryDto.ParentId.Value} does not exist");
                    }
                }

                // Map DTO to entity
                var category = _mapper.Map<Category>(categoryDto);

                // Create the category first
                await _unitOfWork.Categories.CreateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                // Handle image assignment using ImageIds via many-to-many relationship
                if (categoryDto.ImageIds != null && categoryDto.ImageIds.Count > 0)
                {
                    var imageResult = await AssignImagesToCategory(categoryDto.ImageIds, category.Id);
                    if (imageResult.IsFailure)
                    {
                        // Rollback category creation if image assignment fails
                        await _unitOfWork.Categories.DeleteAsync(category.Id);
                        await _unitOfWork.SaveChangesAsync();
                        return Result<CategoryDTO>.Failure(imageResult.ErrorCode ?? "IMAGE_ERROR", imageResult.Error ?? "Failed to assign images to category");
                    }
                }

                var createdCategoryDto = _mapper.Map<CategoryDTO>(category);
                return Result<CategoryDTO>.Success(createdCategoryDto);
            }            catch (Exception ex)
            {
                return Result<CategoryDTO>.Failure("Failed to create category", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result> UpdateAsync(int id, UpdateCategoryDTO categoryDto)
        {
            try
            {
                if (id <= 0)
                {
                    return Result.Failure("INVALID_ID", "Category ID must be greater than 0");
                }

                // Validate input
                var validationResult = await ValidateUpdateCategoryDto(categoryDto);
                if (validationResult.IsFailure)
                {
                    return Result.Failure(validationResult.ErrorCode ?? "VALIDATION_ERROR", validationResult.Error ?? "Validation failed", validationResult.ErrorDetails);
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return Result.Failure("NOT_FOUND", $"Category with ID {id} not found");
                }

                // Check slug uniqueness
                if (!string.IsNullOrEmpty(categoryDto.Slug) &&
                    categoryDto.Slug != category.Slug &&
                    await _unitOfWork.Categories.SlugExistsAsync(categoryDto.Slug))
                {
                    return Result.Failure("SLUG_EXISTS", "A category with this slug already exists");
                }

                // Check if ParentId exists if it's provided
                if (categoryDto.ParentId.HasValue)
                {
                    // Prevent circular reference (category can't be its own parent)
                    if (categoryDto.ParentId.Value == id)
                    {
                        return Result.Failure("CIRCULAR_REFERENCE", "Category cannot be its own parent");
                    }

                    var parentExists = await _unitOfWork.Categories.ExistsAsync(categoryDto.ParentId.Value);
                    if (!parentExists)
                    {
                        return Result.Failure("PARENT_NOT_FOUND", $"Parent category with ID {categoryDto.ParentId.Value} does not exist");
                    }
                }

                // Map basic category properties (excluding images)
                _mapper.Map(categoryDto, category);

                // Handle image associations: Remove old associations and create new ones
                await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Step 1: Remove all existing image associations for this category
                        var existingCategoryImages = await _unitOfWork.Images.GetCategoryImagesByCategoryIdAsync(id);
                        foreach (var categoryImage in existingCategoryImages)
                        {
                            _unitOfWork.Images.RemoveCategoryImage(categoryImage.ImageId, categoryImage.CategoryId);
                        }

                        // Step 2: Associate new images if provided
                        if (categoryDto.ImageIds != null && categoryDto.ImageIds.Count > 0)
                        {
                            var imageResult = await AssignImagesToCategory(categoryDto.ImageIds, category.Id);
                            if (imageResult.IsFailure)
                            {
                                throw new InvalidOperationException(imageResult.Error ?? "Failed to assign images to category");
                            }
                        }

                        // Step 3: Update category and save changes
                        await _unitOfWork.Categories.UpdateAsync(category);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        return Task.CompletedTask;
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                });

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to update category", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result.Failure("INVALID_ID", "Category ID must be greater than 0");
                }

                var category = await _unitOfWork.Categories.GetByIdWithChildrenAsync(id);
                if (category == null)
                {
                    return Result.Failure("NOT_FOUND", $"Category with ID {id} not found");
                }

                // Check if the category has subcategories
                if (category.Subcategories != null && category.Subcategories.Count > 0)
                {
                    return Result.Failure("HAS_SUBCATEGORIES", "Cannot delete category with subcategories");
                }

                // Check if the category is used in any products
                if (category.Products != null && category.Products.Count > 0)
                {
                    return Result.Failure("HAS_PRODUCTS", "Cannot delete category that is used in products");
                }

                // Delete images from storage and unassociate them
                if (category.Images != null && category.Images.Count > 0)
                {
                    foreach (var img in category.Images)
                    {
                        await _imageService.DeleteImageAsync(img.FilePath);
                    }
                }

                await _unitOfWork.Categories.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to delete category", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        // Advanced query methods
        public async Task<Result<IEnumerable<CategoryDTO>>> GetCategoriesWithProductCountAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetCategoriesWithProductCountAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
                return Result<IEnumerable<CategoryDTO>>.Success(categoryDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CategoryDTO>>.Failure("Failed to retrieve categories with product count", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<CategoryDTO>>> GetSubcategoriesAsync(int parentId)
        {
            try
            {
                if (parentId <= 0)
                {
                    return Result<IEnumerable<CategoryDTO>>.Failure("INVALID_ID", "Parent ID must be greater than 0");
                }

                var subcategories = await _unitOfWork.Categories.GetSubcategoriesAsync(parentId);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(subcategories);
                return Result<IEnumerable<CategoryDTO>>.Success(categoryDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CategoryDTO>>.Failure("Failed to retrieve subcategories", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<int>> GetProductCountByCategoryAsync(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return Result<int>.Failure("INVALID_ID", "Category ID must be greater than 0");
                }

                var count = await _unitOfWork.Categories.GetProductCountByCategoryAsync(categoryId);
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Failed to get product count", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<CategoryDTO>>> GetPopularCategoriesAsync(int count = 10)
        {
            try
            {
                if (count <= 0)
                {
                    return Result<IEnumerable<CategoryDTO>>.Failure("INVALID_COUNT", "Count must be greater than 0");
                }

                var categories = await _unitOfWork.Categories.GetPopularCategoriesAsync(count);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
                return Result<IEnumerable<CategoryDTO>>.Success(categoryDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CategoryDTO>>.Failure("Failed to retrieve popular categories", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<CategoryDTO>>> GetRootCategoriesAsync()
        {
            try
            {
                var filter = new CategoryFilterDTO { IsRootCategory = true };
                var pagedResult = await _unitOfWork.Categories.GetPagedAsync(filter);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(pagedResult.Items);
                return Result<IEnumerable<CategoryDTO>>.Success(categoryDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CategoryDTO>>.Failure("Failed to retrieve root categories", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        #region Private Helper Methods

        private async Task<Result> ValidateCreateCategoryDto(CreateCategoryDTO categoryDto)
        {
            var errors = new List<string>();

            if (categoryDto == null)
            {
                return Result.Failure("INVALID_REQUEST", "Category data cannot be null");
            }

            if (string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                errors.Add("Category name is required");
            }            if (string.IsNullOrWhiteSpace(categoryDto.Slug))
            {
                errors.Add("Category slug is required");
            }

            if (errors.Any())
            {
                return Result.Failure("Validation failed", "VALIDATION_ERROR", errors);
            }

            return Result.Success();
        }

        private async Task<Result> ValidateUpdateCategoryDto(UpdateCategoryDTO categoryDto)
        {
            var errors = new List<string>();

            if (categoryDto == null)
            {
                return Result.Failure("INVALID_REQUEST", "Category data cannot be null");
            }

            // For update, fields are optional but if provided, should be valid
            if (categoryDto.Name != null && string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                errors.Add("Category name cannot be empty");
            }            if (categoryDto.Slug != null && string.IsNullOrWhiteSpace(categoryDto.Slug))
            {
                errors.Add("Category slug cannot be empty");
            }

            if (errors.Any())
            {
                return Result.Failure("Validation failed", "VALIDATION_ERROR", errors);
            }

            return Result.Success();
        }

        private async Task<Result> AssignImagesToCategory(List<int> imageIds, int categoryId)
        {
            try
            {                // Verify that all image IDs exist
                var imagesResult = await _imageService.GetImagesByIdsAsync(imageIds);
                
                if (imagesResult.IsFailure)
                {
                    return Result.Failure(imagesResult.Error ?? "Failed to retrieve images", imagesResult.ErrorCode ?? "IMAGE_RETRIEVAL_ERROR");
                }

                if (imagesResult.Data.Count != imageIds.Count)
                {
                    var foundIds = imagesResult.Data.Select(img => img.Id).ToList();
                    var missingIds = imageIds.Except(foundIds).ToList();
                    return Result.Failure("IMAGES_NOT_FOUND", $"The following image IDs were not found: {string.Join(", ", missingIds)}");
                }

                // Associate images with the category using junction table
                foreach (var imageId in imageIds)
                {
                    await _unitOfWork.Images.AddCategoryImageAsync(imageId, categoryId);
                }

                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to assign images to category", "IMAGE_ASSIGNMENT_ERROR", new[] { ex.Message });
            }
        }

        #endregion
    }
}
