using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.CategoryValidators
{
    /// <summary>
    /// Validator for updating existing categories
    /// </summary>
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .Length(1, 50).WithMessage("Category name must be between 1 and 50 characters");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Category slug is required")
                .Length(1, 100).WithMessage("Category slug must be between 1 and 100 characters")
                .Must(BeValidSlug).WithMessage("Category slug must be URL-safe (only letters, numbers, hyphens)");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.ParentId)
                .MustAsync(async (parentId, cancellation) =>
                {
                    if (!parentId.HasValue) return true;
                    return await _unitOfWork.Categories.ExistsAsync(parentId.Value);
                })
                .WithMessage("Parent category does not exist")
                .WithErrorCode("PARENT_CATEGORY_NOT_FOUND");

            RuleFor(x => x.ImageIds)
                .Must(imageIds => imageIds == null || imageIds.Count <= 5)
                .WithMessage("Maximum 5 images can be associated with a category")
                .MustAsync(async (imageIds, cancellation) =>
                {
                    if (imageIds == null || !imageIds.Any()) return true;
                    
                    foreach (var imageId in imageIds)
                    {
                        if (!await _unitOfWork.Images.ExistsAsync(imageId))
                            return false;
                    }
                    return true;
                })
                .WithMessage("One or more image IDs do not exist")
                .WithErrorCode("INVALID_IMAGE_IDS");
        }

        /// <summary>
        /// Validates category update with circular reference prevention
        /// </summary>
        /// <param name="dto">The update DTO</param>
        /// <param name="categoryId">The ID of the category being updated</param>
        /// <returns>Validation result</returns>
        public async Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(UpdateCategoryDTO dto, int categoryId)
        {
            // First run standard validation
            var result = await ValidateAsync(dto);

            if (!result.IsValid)
                return result;

            // Additional validation for update-specific rules
            var additionalErrors = new List<FluentValidation.Results.ValidationFailure>();

            // Check for circular reference if parent is being changed
            if (dto.ParentId.HasValue)
            {
                if (await WouldCreateCircularReference(categoryId, dto.ParentId.Value))
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.ParentId), 
                        "Setting this parent would create a circular reference")
                    {
                        ErrorCode = "CIRCULAR_REFERENCE"
                    });
                }
            }

            // Check name uniqueness excluding current category
            if (!string.IsNullOrEmpty(dto.Name))
            {
                var nameExists = await _unitOfWork.Categories.ExistsByNameAsync(dto.Name, categoryId);
                if (nameExists)
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.Name), 
                        "Category name already exists")
                    {
                        ErrorCode = "CATEGORY_NAME_EXISTS"
                    });
                }
            }

            // Check slug uniqueness excluding current category
            if (!string.IsNullOrEmpty(dto.Slug))
            {
                var slugExists = await _unitOfWork.Categories.ExistsBySlugAsync(dto.Slug, categoryId);
                if (slugExists)
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.Slug), 
                        "Category slug already exists")
                    {
                        ErrorCode = "CATEGORY_SLUG_EXISTS"
                    });
                }
            }

            // Combine results
            if (additionalErrors.Any())
            {
                var combinedErrors = result.Errors.Concat(additionalErrors).ToList();
                return new FluentValidation.Results.ValidationResult(combinedErrors);
            }

            return result;
        }

        private async Task<bool> WouldCreateCircularReference(int categoryId, int parentId)
        {
            // If trying to set parent as itself
            if (categoryId == parentId)
                return true;

            // Check if the proposed parent is already a descendant of this category
            var descendants = await _unitOfWork.Categories.GetDescendantIdsAsync(categoryId);
            return descendants.Contains(parentId);
        }

        private static bool BeValidSlug(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return false;
            
            // Check if slug contains only letters, numbers, hyphens, and underscores
            return System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-zA-Z0-9\-_]+$");
        }
    }
}
