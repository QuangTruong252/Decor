using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.ProductValidators
{
    /// <summary>
    /// Validator for creating new products
    /// </summary>
    public class CreateProductValidator : AbstractValidator<CreateProductDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(1, 100).WithMessage("Product name must be between 1 and 100 characters")
                .MustAsync(BeUniqueName).WithMessage("Product name already exists")
                .WithErrorCode("PRODUCT_NAME_INVALID");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .Length(1, 50).WithMessage("SKU must be between 1 and 50 characters")
                .Matches(@"^[A-Z0-9\-_]+$").WithMessage("SKU can only contain uppercase letters, numbers, hyphens, and underscores")
                .MustAsync(BeUniqueSku).WithMessage("SKU already exists")
                .WithErrorCode("PRODUCT_SKU_INVALID");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Product slug is required")
                .Length(1, 100).WithMessage("Product slug must be between 1 and 100 characters")
                .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
                .MustAsync(BeUniqueSlug).WithMessage("Product slug already exists")
                .WithErrorCode("PRODUCT_SLUG_INVALID");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Price cannot exceed 1,000,000")
                .WithErrorCode("PRODUCT_PRICE_INVALID");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0")
                .MustAsync(CategoryExists).WithMessage("Category does not exist")
                .WithErrorCode("PRODUCT_CATEGORY_INVALID");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
                .WithErrorCode("PRODUCT_DESCRIPTION_INVALID");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative")
                .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000")
                .WithErrorCode("PRODUCT_STOCK_INVALID");

            RuleFor(x => x.Weight)
                .GreaterThan(0).When(x => x.Weight.HasValue)
                .WithMessage("Weight must be greater than 0 when specified")
                .LessThanOrEqualTo(1000).When(x => x.Weight.HasValue)
                .WithMessage("Weight cannot exceed 1000 kg")
                .WithErrorCode("PRODUCT_WEIGHT_INVALID");

            RuleFor(x => x.Dimensions)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Dimensions))
                .WithMessage("Dimensions cannot exceed 100 characters")
                .WithErrorCode("PRODUCT_DIMENSIONS_INVALID");

            RuleFor(x => x.Tags)
                .Must(HaveValidTags).When(x => x.Tags != null && x.Tags.Any())
                .WithMessage("Tags must be between 1 and 50 characters each")
                .WithErrorCode("PRODUCT_TAGS_INVALID");

            RuleFor(x => x.Images)
                .Must(HaveValidImageCount).When(x => x.Images != null)
                .WithMessage("Product can have maximum 10 images")
                .WithErrorCode("PRODUCT_IMAGES_INVALID");

            // Business rules
            RuleFor(x => x)
                .MustAsync(BeValidProductCombination).WithMessage("Invalid product configuration")
                .WithErrorCode("PRODUCT_CONFIGURATION_INVALID");
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            try
            {
                return !await _unitOfWork.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower());
            }
            catch
            {
                return false; // Assume not unique on error
            }
        }

        private async Task<bool> BeUniqueSku(string sku, CancellationToken cancellationToken)
        {
            try
            {
                return !await _unitOfWork.Products.SkuExistsAsync(sku);
            }
            catch
            {
                return false; // Assume not unique on error
            }
        }

        private async Task<bool> BeUniqueSlug(string slug, CancellationToken cancellationToken)
        {
            try
            {
                return !await _unitOfWork.Products.SlugExistsAsync(slug);
            }
            catch
            {
                return false; // Assume not unique on error
            }
        }

        private async Task<bool> CategoryExists(int categoryId, CancellationToken cancellationToken)
        {
            try
            {
                return await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId);
            }
            catch
            {
                return false; // Assume doesn't exist on error
            }
        }

        private bool HaveValidTags(string[]? tags)
        {
            if (tags == null || !tags.Any()) return true;

            return tags.Length <= 20 && // Maximum 20 tags
                   tags.All(tag => !string.IsNullOrWhiteSpace(tag) &&
                                  tag.Length >= 1 &&
                                  tag.Length <= 50);
        }

        private bool HaveValidImageCount(string[]? images)
        {
            return images == null || images.Length <= 10;
        }

        private async Task<bool> BeValidProductCombination(CreateProductDTO product, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            try
            {
                // Business rule: Digital products should have zero weight
                if (product.IsDigital && product.Weight.HasValue && product.Weight.Value > 0)
                {
                    return false;
                }

                // Business rule: Physical products should have weight
                if (!product.IsDigital && (!product.Weight.HasValue || product.Weight.Value <= 0))
                {
                    return false;
                }

                // Business rule: Featured products should have at least one image
                if (product.IsFeatured && (product.Images == null || !product.Images.Any()))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
