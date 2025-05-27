using FluentValidation;
using DecorStore.API.DTOs.Excel;
using System.Text.RegularExpressions;

namespace DecorStore.API.Validators.Excel
{
    /// <summary>
    /// FluentValidation validator for OrderExcelDTO
    /// </summary>
    public class OrderExcelValidator : AbstractValidator<OrderExcelDTO>
    {
        public OrderExcelValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).When(x => x.UserId.HasValue)
                .WithMessage("User ID must be greater than 0 when specified");

            RuleFor(x => x.UserEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.UserEmail))
                .WithMessage("User email must be a valid email address")
                .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.UserEmail))
                .WithMessage("User email must not exceed 255 characters");

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).When(x => x.CustomerId.HasValue)
                .WithMessage("Customer ID must be greater than 0 when specified");

            RuleFor(x => x.CustomerEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.CustomerEmail))
                .WithMessage("Customer email must be a valid email address")
                .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.CustomerEmail))
                .WithMessage("Customer email must not exceed 255 characters");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Total amount must be greater than 0")
                .LessThanOrEqualTo(999999.99m).WithMessage("Total amount cannot exceed 999,999.99");

            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0).WithMessage("Shipping cost cannot be negative")
                .LessThanOrEqualTo(9999.99m).WithMessage("Shipping cost cannot exceed 9,999.99");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative")
                .LessThanOrEqualTo(99999.99m).WithMessage("Tax amount cannot exceed 99,999.99");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative")
                .LessThanOrEqualTo(x => x.TotalAmount).WithMessage("Discount amount cannot exceed total amount");

            RuleFor(x => x.OrderStatus)
                .NotEmpty().WithMessage("Order status is required")
                .MaximumLength(50).WithMessage("Order status must not exceed 50 characters")
                .Must(BeValidOrderStatus).WithMessage("Order status must be one of: Pending, Processing, Shipped, Delivered, Cancelled, Refunded, On Hold");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required")
                .MaximumLength(100).WithMessage("Payment method must not exceed 100 characters");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required")
                .MaximumLength(255).WithMessage("Shipping address must not exceed 255 characters");

            RuleFor(x => x.ShippingCity)
                .NotEmpty().WithMessage("Shipping city is required")
                .MaximumLength(100).WithMessage("Shipping city must not exceed 100 characters");

            RuleFor(x => x.ShippingState)
                .NotEmpty().WithMessage("Shipping state is required")
                .MaximumLength(100).WithMessage("Shipping state must not exceed 100 characters");

            RuleFor(x => x.ShippingPostalCode)
                .NotEmpty().WithMessage("Shipping postal code is required")
                .MaximumLength(20).WithMessage("Shipping postal code must not exceed 20 characters")
                .Must(BeValidPostalCode).WithMessage("Shipping postal code must be a valid format");

            RuleFor(x => x.ShippingCountry)
                .NotEmpty().WithMessage("Shipping country is required")
                .MaximumLength(100).WithMessage("Shipping country must not exceed 100 characters");

            RuleFor(x => x.ContactPhone)
                .Must(BeValidPhoneNumber).When(x => !string.IsNullOrEmpty(x.ContactPhone))
                .WithMessage("Contact phone must be a valid phone number format")
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.ContactPhone))
                .WithMessage("Contact phone must not exceed 20 characters");

            RuleFor(x => x.ContactEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
                .WithMessage("Contact email must be a valid email address")
                .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.ContactEmail))
                .WithMessage("Contact email must not exceed 255 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Notes must not exceed 1000 characters");

            RuleFor(x => x.OrderDate)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Order date cannot be in the future")
                .GreaterThan(DateTime.UtcNow.AddYears(-10)).WithMessage("Order date cannot be more than 10 years ago");

            RuleFor(x => x.OrderItems)
                .Must(BeValidOrderItemsFormat).When(x => !string.IsNullOrEmpty(x.OrderItems))
                .WithMessage("Order items must be in format 'ProductName:Quantity:UnitPrice' separated by commas");

            // Business rule: Must have either UserId or UserEmail
            RuleFor(x => x)
                .Must(x => x.UserId.HasValue && x.UserId.Value > 0 || !string.IsNullOrEmpty(x.UserEmail))
                .WithMessage("Either User ID or User Email must be provided")
                .WithName("UserIdentification");

            // Business rule: Total amount should match calculated total
            RuleFor(x => x)
                .Must(HaveValidTotalCalculation).WithMessage("Total amount does not match calculated total from order items, tax, shipping, and discount")
                .WithName("TotalCalculation");

            // Conditional validation for updates
            When(x => x.Id.HasValue && x.Id.Value > 0, () =>
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Order ID must be greater than 0 for updates");
            });
        }

        private bool BeValidOrderStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return false;

            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded", "On Hold" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeValidPostalCode(string? postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                return false;

            // Support various postal code formats
            var patterns = new[]
            {
                @"^\d{5}$",                    // US ZIP (12345)
                @"^\d{5}-\d{4}$",             // US ZIP+4 (12345-6789)
                @"^[A-Z]\d[A-Z] \d[A-Z]\d$",  // Canadian (A1A 1A1)
                @"^\d{4}$",                   // Simple 4-digit
                @"^\d{6}$",                   // Simple 6-digit
                @"^[A-Z]{1,2}\d[A-Z\d]? \d[A-Z]{2}$", // UK format
                @"^\d{4,10}$"                 // General numeric format
            };

            return patterns.Any(pattern => Regex.IsMatch(postalCode.ToUpper(), pattern));
        }

        private bool BeValidPhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            // Remove common phone number characters
            var cleanPhone = Regex.Replace(phone, @"[\s\-\(\)\+]", "");
            
            // Check if remaining characters are digits and length is reasonable
            return Regex.IsMatch(cleanPhone, @"^\d{7,15}$");
        }

        private bool BeValidOrderItemsFormat(string? orderItems)
        {
            if (string.IsNullOrWhiteSpace(orderItems))
                return true;

            var items = orderItems.Split(',');
            foreach (var item in items)
            {
                var parts = item.Trim().Split(':');
                if (parts.Length != 3)
                    return false;

                if (string.IsNullOrWhiteSpace(parts[0])) // Product name
                    return false;

                if (!int.TryParse(parts[1], out var quantity) || quantity <= 0) // Quantity
                    return false;

                if (!decimal.TryParse(parts[2], out var price) || price <= 0) // Unit price
                    return false;
            }

            return true;
        }

        private bool HaveValidTotalCalculation(OrderExcelDTO order)
        {
            if (string.IsNullOrEmpty(order.OrderItems))
                return true; // Can't validate without order items

            try
            {
                var items = order.ParseOrderItems();
                if (!items.Any())
                    return true;

                var itemsTotal = items.Sum(i => i.TotalPrice);
                var calculatedTotal = itemsTotal + order.TaxAmount + order.ShippingCost - order.DiscountAmount;

                // Allow for small rounding differences
                return Math.Abs(calculatedTotal - order.TotalAmount) <= 0.01m;
            }
            catch
            {
                return false; // Invalid order items format
            }
        }
    }

    /// <summary>
    /// FluentValidation validator for OrderExcelDTO import operations
    /// </summary>
    public class OrderExcelImportValidator : AbstractValidator<OrderExcelDTO>
    {
        public OrderExcelImportValidator()
        {
            Include(new OrderExcelValidator());

            // Additional import-specific validations
            RuleFor(x => x.UserEmail)
                .Must(BeValidUserEmail).When(x => !string.IsNullOrEmpty(x.UserEmail))
                .WithMessage("User email must exist in the system");

            RuleFor(x => x.CustomerEmail)
                .Must(BeValidCustomerEmail).When(x => !string.IsNullOrEmpty(x.CustomerEmail))
                .WithMessage("Customer email must exist in the system");

            RuleFor(x => x.OrderItems)
                .Must(HaveValidProductReferences).When(x => !string.IsNullOrEmpty(x.OrderItems))
                .WithMessage("All products in order items must exist in the system");

            // Validate stock availability
            RuleFor(x => x.OrderItems)
                .Must(HaveAvailableStock).When(x => !string.IsNullOrEmpty(x.OrderItems))
                .WithMessage("Insufficient stock for one or more products in the order");
        }

        private bool BeValidUserEmail(string userEmail)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual validation is done in the service
            return true;
        }

        private bool BeValidCustomerEmail(string customerEmail)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual validation is done in the service
            return true;
        }

        private bool HaveValidProductReferences(string orderItems)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual validation is done in the service
            return true;
        }

        private bool HaveAvailableStock(string orderItems)
        {
            // This would be implemented with stock check in the service layer
            // For now, just return true as the actual validation is done in the service
            return true;
        }
    }

    /// <summary>
    /// FluentValidation validator for OrderExcelDTO export operations
    /// </summary>
    public class OrderExcelExportValidator : AbstractValidator<OrderExcelDTO>
    {
        public OrderExcelExportValidator()
        {
            // Export validation is typically less strict
            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Total amount must be non-negative for export");

            RuleFor(x => x.OrderStatus)
                .NotEmpty().WithMessage("Order status is required for export");

            RuleFor(x => x.OrderDate)
                .NotEmpty().WithMessage("Order date is required for export");
        }
    }

    /// <summary>
    /// FluentValidation validator for complex order business rules
    /// </summary>
    public class OrderBusinessRulesValidator : AbstractValidator<OrderExcelDTO>
    {
        public OrderBusinessRulesValidator()
        {
            // Complex business rules validation
            RuleFor(x => x)
                .Must(HaveValidOrderProgression).WithMessage("Order status progression is invalid")
                .Must(HaveValidPaymentForStatus).WithMessage("Payment method is not valid for the current order status")
                .Must(HaveValidShippingForStatus).WithMessage("Shipping information is required for shipped/delivered orders")
                .WithName("BusinessRules");

            // Validate order value thresholds
            RuleFor(x => x.TotalAmount)
                .Must((order, total) => ValidateOrderValueThresholds(order, total))
                .WithMessage("Order value does not meet business requirements");

            // Validate order item consistency
            RuleFor(x => x.OrderItems)
                .Must((order, items) => ValidateOrderItemConsistency(order, items))
                .When(x => !string.IsNullOrEmpty(x.OrderItems))
                .WithMessage("Order items are inconsistent with order totals");
        }

        private bool HaveValidOrderProgression(OrderExcelDTO order)
        {
            // Implement order status progression rules
            // For example: Cancelled orders shouldn't have shipping info
            if (order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrEmpty(order.TrackingNumber);
            }

            return true;
        }

        private bool HaveValidPaymentForStatus(OrderExcelDTO order)
        {
            // Implement payment validation rules
            // For example: Delivered orders should have valid payment methods
            if (order.OrderStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
            {
                return !string.IsNullOrEmpty(order.PaymentMethod) && 
                       !order.PaymentMethod.Equals("Pending", StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private bool HaveValidShippingForStatus(OrderExcelDTO order)
        {
            // Implement shipping validation rules
            var shippingRequiredStatuses = new[] { "Shipped", "Delivered" };
            
            if (shippingRequiredStatuses.Contains(order.OrderStatus, StringComparer.OrdinalIgnoreCase))
            {
                return !string.IsNullOrEmpty(order.ShippingAddress) &&
                       !string.IsNullOrEmpty(order.ShippingCity) &&
                       !string.IsNullOrEmpty(order.ShippingCountry);
            }

            return true;
        }

        private bool ValidateOrderValueThresholds(OrderExcelDTO order, decimal totalAmount)
        {
            // Implement business rules for order values
            // For example: Free shipping threshold, minimum order amount, etc.
            
            // Free shipping for orders over $100
            if (totalAmount >= 100 && order.ShippingCost > 0)
            {
                // This might be a warning rather than an error
                return true; // Allow but could be flagged
            }

            // Minimum order amount
            if (totalAmount < 1)
            {
                return false;
            }

            return true;
        }

        private bool ValidateOrderItemConsistency(OrderExcelDTO order, string orderItems)
        {
            try
            {
                var items = order.ParseOrderItems();
                if (!items.Any())
                    return true;

                // Check if item count matches
                var calculatedItemCount = items.Sum(i => i.Quantity);
                if (order.ItemCount > 0 && order.ItemCount != calculatedItemCount)
                {
                    return false;
                }

                // Check if subtotal is reasonable
                var itemsTotal = items.Sum(i => i.TotalPrice);
                if (itemsTotal <= 0)
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
