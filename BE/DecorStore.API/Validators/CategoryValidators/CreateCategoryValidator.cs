using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.CategoryValidators
{
    /// <summary>
    /// Validator for creating new categories
    /// </summary>
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .Length(1, 50).WithMessage("Category name must be between 1 and 50 characters")
                .MustAsync(async (name, cancellation) => 
                    !await _unitOfWork.Categories.ExistsByNameAsync(name))
                .WithMessage("Category name already exists")
                .WithErrorCode("CATEGORY_NAME_EXISTS");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Category slug is required")
                .Length(1, 100).WithMessage("Category slug must be between 1 and 100 characters")
                .Must(BeValidSlug).WithMessage("Category slug must be URL-safe (only letters, numbers, hyphens)")
                .MustAsync(async (slug, cancellation) => 
                    !await _unitOfWork.Categories.ExistsBySlugAsync(slug))
                .WithMessage("Category slug already exists")
                .WithErrorCode("CATEGORY_SLUG_EXISTS");

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

        private static bool BeValidSlug(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return false;
            
            // Check if slug contains only letters, numbers, hyphens, and underscores
            return System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-zA-Z0-9\-_]+$");
        }
    }
}
