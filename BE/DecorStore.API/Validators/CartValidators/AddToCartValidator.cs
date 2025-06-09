using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.CartValidators
{
    /// <summary>
    /// Validator for adding items to cart
    /// </summary>
    public class AddToCartValidator : AbstractValidator<AddToCartDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddToCartValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Valid product ID is required")
                .MustAsync(async (productId, cancellation) => 
                    await _unitOfWork.Products.ExistsAsync(productId))
                .WithMessage("Product does not exist")
                .WithErrorCode("PRODUCT_NOT_FOUND");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Maximum 100 items can be added at once");

            // Validate product is available and in stock
            RuleFor(x => x)
                .MustAsync(async (dto, cancellation) =>
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                    if (product == null) return false;
                    
                    // Check if product is active/available
                    return product.IsActive;
                })
                .WithMessage("Product is not available for purchase")
                .WithErrorCode("PRODUCT_NOT_AVAILABLE");

            // Validate stock availability
            RuleFor(x => x)
                .MustAsync(async (dto, cancellation) =>
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                    if (product == null) return false;
                    
                    return product.StockQuantity >= dto.Quantity;
                })
                .WithMessage("Insufficient stock available")
                .WithErrorCode("INSUFFICIENT_STOCK");
        }

        /// <summary>
        /// Validates adding to cart with existing cart context
        /// </summary>
        /// <param name="dto">The add to cart DTO</param>
        /// <param name="userId">The user ID (for user carts)</param>
        /// <param name="sessionId">The session ID (for guest carts)</param>
        /// <returns>Validation result</returns>
        public async Task<FluentValidation.Results.ValidationResult> ValidateAddToCartAsync(
            AddToCartDTO dto, int? userId = null, string? sessionId = null)
        {
            // First run standard validation
            var result = await ValidateAsync(dto);

            if (!result.IsValid)
                return result;

            var additionalErrors = new List<FluentValidation.Results.ValidationFailure>();

            // Get existing cart
            var cart = userId.HasValue
                ? await _unitOfWork.Carts.GetByUserIdAsync(userId.Value)
                : await _unitOfWork.Carts.GetBySessionIdAsync(sessionId ?? string.Empty);

            if (cart != null)
            {
                // Check if product is already in cart
                var existingItem = cart.Items?.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
                
                if (existingItem != null)
                {
                    // Calculate total quantity if item is already in cart
                    var totalQuantity = existingItem.Quantity + dto.Quantity;

                    // Validate total quantity doesn't exceed stock
                    var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                    if (product != null && totalQuantity > product.StockQuantity)
                    {
                        additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                            nameof(dto.Quantity),
                            $"Total quantity ({totalQuantity}) would exceed available stock ({product.StockQuantity})")
                        {
                            ErrorCode = "TOTAL_QUANTITY_EXCEEDS_STOCK"
                        });
                    }

                    // Validate total quantity doesn't exceed per-item limit
                    if (totalQuantity > 100)
                    {
                        additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                            nameof(dto.Quantity),
                            "Maximum 100 items of the same product allowed in cart")
                        {
                            ErrorCode = "ITEM_QUANTITY_LIMIT_EXCEEDED"
                        });
                    }
                }

                // Validate cart item count limit
                var currentItemCount = cart.Items?.Count ?? 0;
                if (existingItem == null && currentItemCount >= 50)
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.ProductId),
                        "Maximum 50 different products allowed in cart")
                    {
                        ErrorCode = "CART_ITEM_LIMIT_EXCEEDED"
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
    }

    /// <summary>
    /// Validator for updating cart item quantities
    /// </summary>
    public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCartItemValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Maximum 100 items allowed per product");
        }

        /// <summary>
        /// Validates updating cart item with stock availability
        /// </summary>
        /// <param name="dto">The update DTO</param>
        /// <param name="cartItemId">The cart item ID</param>
        /// <returns>Validation result</returns>
        public async Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(
            UpdateCartItemDTO dto, int cartItemId)
        {
            // First run standard validation
            var result = await ValidateAsync(dto);

            if (!result.IsValid)
                return result;

            var additionalErrors = new List<FluentValidation.Results.ValidationFailure>();

            // Get cart item and product
            var cartItem = await _unitOfWork.Carts.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null)
            {
                additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                    "CartItemId",
                    "Cart item not found")
                {
                    ErrorCode = "CART_ITEM_NOT_FOUND"
                });
            }
            else
            {
                var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.Quantity),
                        "Product no longer exists")
                    {
                        ErrorCode = "PRODUCT_NOT_FOUND"
                    });
                }
                else
                {
                    // Validate product is still available
                    if (!product.IsActive)
                    {
                        additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                            nameof(dto.Quantity),
                            "Product is no longer available")
                        {
                            ErrorCode = "PRODUCT_NOT_AVAILABLE"
                        });
                    }

                    // Validate stock availability
                    if (dto.Quantity > product.StockQuantity)
                    {
                        additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                            nameof(dto.Quantity),
                            $"Only {product.StockQuantity} items available in stock")
                        {
                            ErrorCode = "INSUFFICIENT_STOCK"
                        });
                    }
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
    }
}
