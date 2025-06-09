using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.OrderValidators
{
    /// <summary>
    /// Validator for creating new orders
    /// </summary>
    public class CreateOrderValidator : AbstractValidator<CreateOrderDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required")
                .MustAsync(async (userId, cancellation) => 
                    await _unitOfWork.Users.ExistsAsync(userId))
                .WithMessage("User does not exist")
                .WithErrorCode("USER_NOT_FOUND");

            RuleFor(x => x.CustomerId)
                .MustAsync(async (customerId, cancellation) =>
                {
                    if (!customerId.HasValue) return true;
                    return await _unitOfWork.Customers.ExistsAsync(customerId.Value);
                })
                .WithMessage("Customer does not exist")
                .WithErrorCode("CUSTOMER_NOT_FOUND");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required")
                .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters")
                .Must(BeValidPaymentMethod).WithMessage("Invalid payment method")
                .WithErrorCode("INVALID_PAYMENT_METHOD");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(255).WithMessage("Shipping address cannot exceed 255 characters");

            RuleFor(x => x.ShippingCity)
                .NotEmpty().WithMessage("Shipping city is required")
                .MaximumLength(100).WithMessage("Shipping city cannot exceed 100 characters");

            RuleFor(x => x.ShippingState)
                .NotEmpty().WithMessage("Shipping state is required")
                .MaximumLength(50).WithMessage("Shipping state cannot exceed 50 characters");

            RuleFor(x => x.ShippingPostalCode)
                .NotEmpty().WithMessage("Shipping postal code is required")
                .MaximumLength(20).WithMessage("Shipping postal code cannot exceed 20 characters")
                .Must(BeValidPostalCode).WithMessage("Invalid postal code format");

            RuleFor(x => x.ShippingCountry)
                .NotEmpty().WithMessage("Shipping country is required")
                .MaximumLength(50).WithMessage("Shipping country cannot exceed 50 characters");

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage("Contact phone is required")
                .MaximumLength(100).WithMessage("Contact phone cannot exceed 100 characters")
                .Must(BeValidPhoneNumber).WithMessage("Invalid phone number format");

            RuleFor(x => x.ContactEmail)
                .NotEmpty().WithMessage("Contact email is required")
                .MaximumLength(100).WithMessage("Contact email cannot exceed 100 characters")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Notes)
                .MaximumLength(255).WithMessage("Notes cannot exceed 255 characters");

            RuleFor(x => x.OrderItems)
                .NotNull().WithMessage("Order items are required")
                .NotEmpty().WithMessage("At least one order item is required")
                .Must(items => items.Count <= 100).WithMessage("Maximum 100 items allowed per order");

            RuleForEach(x => x.OrderItems).SetValidator(new CreateOrderItemValidator(_unitOfWork));
        }

        private static bool BeValidPaymentMethod(string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod)) return false;

            var validMethods = new[] { "Credit Card", "Debit Card", "PayPal", "Bank Transfer", "Cash on Delivery", "Stripe" };
            return validMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase);
        }

        private static bool BeValidPostalCode(string postalCode)
        {
            if (string.IsNullOrEmpty(postalCode)) return false;

            // Basic postal code validation - alphanumeric with optional hyphens/spaces
            return System.Text.RegularExpressions.Regex.IsMatch(postalCode, @"^[A-Za-z0-9\s\-]+$");
        }

        private static bool BeValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return false;

            // Basic phone number validation - numbers, spaces, hyphens, parentheses, plus sign
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\+]?[\d\s\-\(\)]+$");
        }
    }

    /// <summary>
    /// Validator for order items within an order
    /// </summary>
    public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderItemValidator(IUnitOfWork unitOfWork)
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
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 per item");


            // Validate stock availability
            RuleFor(x => x)
                .MustAsync(async (orderItem, cancellation) =>
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId);
                    if (product == null) return false;
                    
                    return product.StockQuantity >= orderItem.Quantity;
                })
                .WithMessage("Insufficient stock for the requested quantity")
                .WithErrorCode("INSUFFICIENT_STOCK");

        }
    }
}
