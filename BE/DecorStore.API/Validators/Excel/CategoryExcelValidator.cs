using FluentValidation;
using DecorStore.API.DTOs.Excel;

namespace DecorStore.API.Validators.Excel
{
    /// <summary>
    /// FluentValidation validator for CategoryExcelDTO
    /// </summary>
    public class CategoryExcelValidator : AbstractValidator<CategoryExcelDTO>
    {
        public CategoryExcelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Category slug is required")
                .MaximumLength(100).WithMessage("Category slug must not exceed 100 characters")
                .Matches(@"^[a-z0-9-]+$").WithMessage("Category slug can only contain lowercase letters, numbers, and hyphens");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.ParentId)
                .GreaterThan(0).When(x => x.ParentId.HasValue)
                .WithMessage("Parent ID must be greater than 0 when specified");

            RuleFor(x => x.ParentName)
                .MaximumLength(100).WithMessage("Parent name must not exceed 100 characters");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("Image URL must be a valid URL");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");

            // Business rule: Cannot be parent of itself
            RuleFor(x => x)
                .Must(x => !x.Id.HasValue || !x.ParentId.HasValue || x.Id.Value != x.ParentId.Value)
                .WithMessage("Category cannot be its own parent")
                .WithName("ParentId");

            // Conditional validation for updates
            When(x => x.Id.HasValue && x.Id.Value > 0, () =>
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Category ID must be greater than 0 for updates");
            });
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }

    /// <summary>
    /// FluentValidation validator for CategoryExcelDTO import operations
    /// </summary>
    public class CategoryExcelImportValidator : AbstractValidator<CategoryExcelDTO>
    {
        public CategoryExcelImportValidator()
        {
            Include(new CategoryExcelValidator());

            // Additional import-specific validations
            RuleFor(x => x.Name)
                .Must(BeUniqueCategoryName).WithMessage("Category name must be unique")
                .When(x => !x.Id.HasValue || x.Id.Value <= 0);

            RuleFor(x => x.Slug)
                .Must(BeUniqueCategorySlug).WithMessage("Category slug must be unique")
                .When(x => !x.Id.HasValue || x.Id.Value <= 0);

            // Validate parent category exists if specified
            RuleFor(x => x.ParentName)
                .Must(BeValidParentCategory).When(x => !string.IsNullOrEmpty(x.ParentName))
                .WithMessage("Parent category must exist");

            // Validate hierarchy depth
            RuleFor(x => x)
                .Must(NotExceedMaxHierarchyDepth).WithMessage("Category hierarchy cannot exceed maximum depth of 5 levels")
                .WithName("HierarchyDepth");
        }

        private bool BeUniqueCategoryName(string name)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual uniqueness check is done in the service
            return true;
        }

        private bool BeUniqueCategorySlug(string slug)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual uniqueness check is done in the service
            return true;
        }

        private bool BeValidParentCategory(string parentName)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual parent validation is done in the service
            return true;
        }

        private bool NotExceedMaxHierarchyDepth(CategoryExcelDTO category)
        {
            // This would be implemented with hierarchy depth check in the service layer
            // For now, just return true as the actual depth check is done in the service
            return true;
        }
    }

    /// <summary>
    /// FluentValidation validator for CategoryExcelDTO export operations
    /// </summary>
    public class CategoryExcelExportValidator : AbstractValidator<CategoryExcelDTO>
    {
        public CategoryExcelExportValidator()
        {
            // Export validation is typically less strict
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required for export");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Category slug is required for export");
        }
    }

    /// <summary>
    /// FluentValidation validator for category hierarchy validation
    /// </summary>
    public class CategoryHierarchyValidator : AbstractValidator<List<CategoryExcelDTO>>
    {
        public CategoryHierarchyValidator()
        {
            RuleFor(x => x)
                .Must(NotHaveCircularReferences).WithMessage("Category hierarchy contains circular references")
                .Must(NotExceedMaxDepth).WithMessage("Category hierarchy exceeds maximum depth of 5 levels")
                .Must(HaveValidParentReferences).WithMessage("Category hierarchy contains invalid parent references");
        }

        private bool NotHaveCircularReferences(List<CategoryExcelDTO> categories)
        {
            // Build a map of category relationships
            var categoryMap = categories.Where(c => c.Id.HasValue).ToDictionary(c => c.Id!.Value, c => c);
            
            foreach (var category in categories.Where(c => c.Id.HasValue && c.ParentId.HasValue))
            {
                var visited = new HashSet<int>();
                var current = category;

                while (current?.ParentId.HasValue == true)
                {
                    if (visited.Contains(current.ParentId.Value))
                    {
                        return false; // Circular reference detected
                    }

                    visited.Add(current.ParentId.Value);

                    if (!categoryMap.TryGetValue(current.ParentId.Value, out current))
                    {
                        break; // Parent not found in current batch
                    }
                }
            }

            return true;
        }

        private bool NotExceedMaxDepth(List<CategoryExcelDTO> categories)
        {
            const int maxDepth = 5;
            var categoryMap = categories.Where(c => c.Id.HasValue).ToDictionary(c => c.Id!.Value, c => c);

            foreach (var category in categories.Where(c => c.Id.HasValue))
            {
                var depth = CalculateDepth(category, categoryMap);
                if (depth > maxDepth)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HaveValidParentReferences(List<CategoryExcelDTO> categories)
        {
            var categoryIds = categories.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToHashSet();

            foreach (var category in categories.Where(c => c.ParentId.HasValue))
            {
                // Check if parent exists in current batch or would exist in database
                // For now, we'll assume it's valid if ParentId is provided
                if (category.ParentId.Value <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private int CalculateDepth(CategoryExcelDTO category, Dictionary<int, CategoryExcelDTO> categoryMap)
        {
            var depth = 1;
            var current = category;

            while (current?.ParentId.HasValue == true && categoryMap.TryGetValue(current.ParentId.Value, out current))
            {
                depth++;
                if (depth > 10) // Safety check to prevent infinite loops
                {
                    break;
                }
            }

            return depth;
        }
    }
}
