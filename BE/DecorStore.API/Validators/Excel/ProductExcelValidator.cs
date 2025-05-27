using FluentValidation;
using DecorStore.API.DTOs.Excel;

namespace DecorStore.API.Validators.Excel
{
    /// <summary>
    /// FluentValidation validator for ProductExcelDTO
    /// </summary>
    public class ProductExcelValidator : AbstractValidator<ProductExcelDTO>
    {
        public ProductExcelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(3, 255).WithMessage("Product name must be between 3 and 255 characters");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Product slug is required")
                .MaximumLength(255).WithMessage("Product slug must not exceed 255 characters")
                .Matches(@"^[a-z0-9-]+$").WithMessage("Product slug can only contain lowercase letters, numbers, and hyphens");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(999999.99m).WithMessage("Price cannot exceed 999,999.99");

            RuleFor(x => x.OriginalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Original price cannot be negative")
                .LessThanOrEqualTo(999999.99m).WithMessage("Original price cannot exceed 999,999.99");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative")
                .LessThanOrEqualTo(999999).WithMessage("Stock quantity cannot exceed 999,999");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
                .Matches(@"^[A-Z0-9-_]+$").WithMessage("SKU can only contain uppercase letters, numbers, hyphens, and underscores");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).When(x => string.IsNullOrEmpty(x.CategoryName))
                .WithMessage("Category ID must be greater than 0 when Category Name is not provided");

            RuleFor(x => x.CategoryName)
                .NotEmpty().When(x => x.CategoryId <= 0)
                .WithMessage("Category Name is required when Category ID is not provided");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.ImageUrls)
                .Must(BeValidImageUrls).When(x => !string.IsNullOrEmpty(x.ImageUrls))
                .WithMessage("Image URLs must be valid URLs separated by commas");

            // Business rule: Original price should be greater than or equal to price
            RuleFor(x => x)
                .Must(x => x.OriginalPrice == 0 || x.OriginalPrice >= x.Price)
                .WithMessage("Original price must be greater than or equal to current price")
                .WithName("OriginalPrice");

            // Conditional validation for updates
            When(x => x.Id.HasValue && x.Id.Value > 0, () =>
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Product ID must be greater than 0 for updates");
            });
        }

        private bool BeValidImageUrls(string? imageUrls)
        {
            if (string.IsNullOrWhiteSpace(imageUrls))
                return true;

            var urls = imageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return urls.All(url => Uri.TryCreate(url.Trim(), UriKind.Absolute, out var result) &&
                                  (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps));
        }
    }

    /// <summary>
    /// FluentValidation validator for ProductExcelDTO import operations
    /// </summary>
    public class ProductExcelImportValidator : AbstractValidator<ProductExcelDTO>
    {
        public ProductExcelImportValidator()
        {
            Include(new ProductExcelValidator());

            // Additional import-specific validations
            RuleFor(x => x.Name)
                .Must(BeUniqueProductName).WithMessage("Product name must be unique")
                .When(x => !x.Id.HasValue || x.Id.Value <= 0);

            RuleFor(x => x.SKU)
                .Must(BeUniqueSKU).WithMessage("SKU must be unique")
                .When(x => !x.Id.HasValue || x.Id.Value <= 0);
        }

        private bool BeUniqueProductName(string name)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual uniqueness check is done in the service
            return true;
        }

        private bool BeUniqueSKU(string sku)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual uniqueness check is done in the service
            return true;
        }
    }

    /// <summary>
    /// FluentValidation validator for ProductExcelDTO export operations
    /// </summary>
    public class ProductExcelExportValidator : AbstractValidator<ProductExcelDTO>
    {
        public ProductExcelExportValidator()
        {
            // Export validation is typically less strict
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required for export");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required for export");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative for export");
        }
    }
}
