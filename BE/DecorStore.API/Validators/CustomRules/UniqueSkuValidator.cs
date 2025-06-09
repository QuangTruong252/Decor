using FluentValidation;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.CustomRules
{
    /// <summary>
    /// Custom validator for SKU uniqueness
    /// </summary>
    public class UniqueSkuValidator : AbstractValidator<string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UniqueSkuValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(sku => sku)
                .NotEmpty().WithMessage("SKU is required")
                .Length(1, 50).WithMessage("SKU must be between 1 and 50 characters")
                .Must(BeValidSkuFormat).WithMessage("SKU can only contain letters, numbers, hyphens, and underscores")
                .MustAsync(async (sku, cancellation) => 
                    !await _unitOfWork.Products.SkuExistsAsync(sku))
                .WithMessage("SKU already exists")
                .WithErrorCode("SKU_ALREADY_EXISTS");
        }

        /// <summary>
        /// Validates SKU uniqueness excluding a specific product
        /// </summary>
        /// <param name="sku">The SKU to validate</param>
        /// <param name="excludeProductId">Product ID to exclude from uniqueness check</param>
        /// <returns>Validation result</returns>
        public async Task<FluentValidation.Results.ValidationResult> ValidateUniqueSkuAsync(string sku, int excludeProductId)
        {
            var validator = new InlineValidator<string>();
            
            validator.RuleFor(x => x)
                .NotEmpty().WithMessage("SKU is required")
                .Length(1, 50).WithMessage("SKU must be between 1 and 50 characters")
                .Must(BeValidSkuFormat).WithMessage("SKU can only contain letters, numbers, hyphens, and underscores")
                .MustAsync(async (skuValue, cancellation) => 
                    !await _unitOfWork.Products.SkuExistsAsync(skuValue, excludeProductId))
                .WithMessage("SKU already exists")
                .WithErrorCode("SKU_ALREADY_EXISTS");

            return await validator.ValidateAsync(sku);
        }

        private static bool BeValidSkuFormat(string sku)
        {
            if (string.IsNullOrEmpty(sku)) return false;
            
            // SKU can contain letters, numbers, hyphens, and underscores
            return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[a-zA-Z0-9\-_]+$");
        }
    }

    /// <summary>
    /// Custom validator for category existence
    /// </summary>
    public class CategoryExistsValidator : AbstractValidator<int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryExistsValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(categoryId => categoryId)
                .GreaterThan(0).WithMessage("Valid category ID is required")
                .MustAsync(async (categoryId, cancellation) => 
                    await _unitOfWork.Categories.ExistsAsync(categoryId))
                .WithMessage("Category does not exist")
                .WithErrorCode("CATEGORY_NOT_FOUND");
        }
    }

    /// <summary>
    /// Custom validator for product availability
    /// </summary>
    public class ProductAvailabilityValidator : AbstractValidator<int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductAvailabilityValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(productId => productId)
                .GreaterThan(0).WithMessage("Valid product ID is required")
                .MustAsync(async (productId, cancellation) => 
                    await _unitOfWork.Products.ExistsAsync(productId))
                .WithMessage("Product does not exist")
                .WithErrorCode("PRODUCT_NOT_FOUND")
                .MustAsync(async (productId, cancellation) =>
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(productId);
                    return product?.IsActive == true;
                })
                .WithMessage("Product is not available for purchase")
                .WithErrorCode("PRODUCT_NOT_AVAILABLE");
        }
    }

    /// <summary>
    /// Custom validator for stock availability
    /// </summary>
    public class StockAvailabilityValidator
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockAvailabilityValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Validates if requested quantity is available in stock
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="requestedQuantity">Requested quantity</param>
        /// <returns>Validation result</returns>
        public async Task<(bool IsValid, string ErrorMessage, string ErrorCode)> ValidateStockAsync(int productId, int requestedQuantity)
        {
            if (productId <= 0)
                return (false, "Valid product ID is required", "INVALID_PRODUCT_ID");

            if (requestedQuantity <= 0)
                return (false, "Quantity must be greater than 0", "INVALID_QUANTITY");

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product does not exist", "PRODUCT_NOT_FOUND");

            if (!product.IsActive)
                return (false, "Product is not available", "PRODUCT_NOT_AVAILABLE");

            if (product.StockQuantity < requestedQuantity)
                return (false, $"Only {product.StockQuantity} items available in stock", "INSUFFICIENT_STOCK");

            return (true, string.Empty, string.Empty);
        }

        /// <summary>
        /// Validates stock for multiple products
        /// </summary>
        /// <param name="items">List of product ID and quantity pairs</param>
        /// <returns>Validation results</returns>
        public async Task<List<(int ProductId, bool IsValid, string ErrorMessage, string ErrorCode)>> ValidateMultipleStockAsync(
            IEnumerable<(int ProductId, int Quantity)> items)
        {
            var results = new List<(int ProductId, bool IsValid, string ErrorMessage, string ErrorCode)>();

            foreach (var item in items)
            {
                var result = await ValidateStockAsync(item.ProductId, item.Quantity);
                results.Add((item.ProductId, result.IsValid, result.ErrorMessage, result.ErrorCode));
            }

            return results;
        }
    }
}
