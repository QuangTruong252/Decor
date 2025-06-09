using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.OrderValidators
{
    /// <summary>
    /// Validator for updating order status with business rule validation
    /// </summary>
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(x => x.OrderStatus)
                .NotEmpty().WithMessage("Order status is required")
                .MaximumLength(50).WithMessage("Order status cannot exceed 50 characters")
                .Must(BeValidOrderStatus).WithMessage("Invalid order status")
                .WithErrorCode("INVALID_ORDER_STATUS");
        }

        /// <summary>
        /// Validates order status update with transition rules
        /// </summary>
        /// <param name="dto">The update DTO</param>
        /// <param name="currentOrderStatus">The current status of the order</param>
        /// <returns>Validation result</returns>
        public async Task<FluentValidation.Results.ValidationResult> ValidateStatusTransitionAsync(
            UpdateOrderStatusDTO dto, string currentOrderStatus)
        {
            // First run standard validation
            var result = await ValidateAsync(dto);

            if (!result.IsValid)
                return result;

            // Additional validation for status transition rules
            var additionalErrors = new List<FluentValidation.Results.ValidationFailure>();

            if (!IsValidStatusTransition(currentOrderStatus, dto.OrderStatus))
            {
                additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(dto.OrderStatus),
                    $"Cannot change order status from '{currentOrderStatus}' to '{dto.OrderStatus}'")
                {
                    ErrorCode = "INVALID_STATUS_TRANSITION"
                });
            }

            // Combine results
            if (additionalErrors.Any())
            {
                var combinedErrors = result.Errors.Concat(additionalErrors).ToList();
                return new FluentValidation.Results.ValidationResult(combinedErrors);
            }

            return result;
        }

        /// <summary>
        /// Validates status transition with inventory considerations
        /// </summary>
        /// <param name="dto">The update DTO</param>
        /// <param name="orderId">The order ID</param>
        /// <param name="currentOrderStatus">Current order status</param>
        /// <returns>Validation result</returns>
        public async Task<FluentValidation.Results.ValidationResult> ValidateStatusTransitionWithInventoryAsync(
            UpdateOrderStatusDTO dto, int orderId, string currentOrderStatus)
        {
            // First validate basic transition
            var result = await ValidateStatusTransitionAsync(dto, currentOrderStatus);

            if (!result.IsValid)
                return result;

            var additionalErrors = new List<FluentValidation.Results.ValidationFailure>();

            // Special validation when cancelling order
            if (dto.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                // Check if order can be cancelled (not yet shipped)
                if (currentOrderStatus.Equals("Shipped", StringComparison.OrdinalIgnoreCase) ||
                    currentOrderStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.OrderStatus),
                        "Cannot cancel order that has already been shipped or delivered")
                    {
                        ErrorCode = "CANNOT_CANCEL_SHIPPED_ORDER"
                    });
                }
            }

            // Special validation when marking as shipped
            if (dto.OrderStatus.Equals("Shipped", StringComparison.OrdinalIgnoreCase))
            {
                // Verify all items are still in stock (in case of inventory changes)
                var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(orderId);
                if (order != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                        if (product == null || product.StockQuantity < item.Quantity)
                        {
                            additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                                nameof(dto.OrderStatus),
                                $"Insufficient stock for product '{product?.Name ?? "Unknown"}' to ship order")
                            {
                                ErrorCode = "INSUFFICIENT_STOCK_FOR_SHIPPING"
                            });
                            break; // Only report first stock issue
                        }
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

        private static bool BeValidOrderStatus(string orderStatus)
        {
            if (string.IsNullOrEmpty(orderStatus)) return false;

            var validStatuses = new[]
            {
                "Pending",
                "Processing",
                "Shipped",
                "Delivered",
                "Cancelled",
                "Refunded",
                "Returned"
            };

            return validStatuses.Contains(orderStatus, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            if (string.IsNullOrEmpty(currentStatus) || string.IsNullOrEmpty(newStatus))
                return false;

            // Normalize status strings for comparison
            currentStatus = currentStatus.Trim();
            newStatus = newStatus.Trim();

            // Same status is always valid (no change)
            if (currentStatus.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
                return true;

            // Define valid transitions
            var validTransitions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Pending"] = new[] { "Processing", "Cancelled" },
                ["Processing"] = new[] { "Shipped", "Cancelled" },
                ["Shipped"] = new[] { "Delivered", "Returned" },
                ["Delivered"] = new[] { "Returned", "Refunded" },
                ["Cancelled"] = new string[] { }, // Terminal state - no transitions allowed
                ["Returned"] = new[] { "Refunded" },
                ["Refunded"] = new string[] { } // Terminal state - no transitions allowed
            };

            if (!validTransitions.ContainsKey(currentStatus))
                return false;

            return validTransitions[currentStatus].Contains(newStatus, StringComparer.OrdinalIgnoreCase);
        }
    }
}
