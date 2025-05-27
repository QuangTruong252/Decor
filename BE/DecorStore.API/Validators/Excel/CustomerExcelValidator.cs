using FluentValidation;
using DecorStore.API.DTOs.Excel;
using System.Text.RegularExpressions;

namespace DecorStore.API.Validators.Excel
{
    /// <summary>
    /// FluentValidation validator for CustomerExcelDTO
    /// </summary>
    public class CustomerExcelValidator : AbstractValidator<CustomerExcelDTO>
    {
        public CustomerExcelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(1, 50).WithMessage("First name must be between 1 and 50 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(1, 50).WithMessage("Last name must be between 1 and 50 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Must(BeValidPhoneNumber).WithMessage("Phone number must be a valid format");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(255).WithMessage("Address must not exceed 255 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("City can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.State)
                .NotEmpty().WithMessage("State/Province is required")
                .MaximumLength(100).WithMessage("State/Province must not exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("State/Province can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("Postal code is required")
                .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters")
                .Must(BeValidPostalCode).WithMessage("Postal code must be a valid format");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Country can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).When(x => x.DateOfBirth.HasValue)
                .WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.Today.AddYears(-120)).When(x => x.DateOfBirth.HasValue)
                .WithMessage("Date of birth cannot be more than 120 years ago");

            // Conditional validation for updates
            When(x => x.Id.HasValue && x.Id.Value > 0, () =>
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Customer ID must be greater than 0 for updates");
            });
        }

        private bool BeValidPhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove common phone number characters
            var cleanPhone = Regex.Replace(phone, @"[\s\-\(\)\+]", "");
            
            // Check if remaining characters are digits and length is reasonable
            return Regex.IsMatch(cleanPhone, @"^\d{7,15}$");
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
    }

    /// <summary>
    /// FluentValidation validator for CustomerExcelDTO import operations
    /// </summary>
    public class CustomerExcelImportValidator : AbstractValidator<CustomerExcelDTO>
    {
        public CustomerExcelImportValidator()
        {
            Include(new CustomerExcelValidator());

            // Additional import-specific validations
            RuleFor(x => x.Email)
                .Must(BeUniqueEmail).WithMessage("Email address must be unique")
                .When(x => !x.Id.HasValue || x.Id.Value <= 0);

            // Validate age if date of birth is provided
            RuleFor(x => x.DateOfBirth)
                .Must(BeValidAge).When(x => x.DateOfBirth.HasValue)
                .WithMessage("Customer must be at least 13 years old");
        }

        private bool BeUniqueEmail(string email)
        {
            // This would be implemented with database check in the service layer
            // For now, just return true as the actual uniqueness check is done in the service
            return true;
        }

        private bool BeValidAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return true;

            var age = DateTime.Today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age))
                age--;

            return age >= 13; // Minimum age requirement
        }
    }

    /// <summary>
    /// FluentValidation validator for CustomerExcelDTO export operations
    /// </summary>
    public class CustomerExcelExportValidator : AbstractValidator<CustomerExcelDTO>
    {
        public CustomerExcelExportValidator()
        {
            // Export validation is typically less strict
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required for export");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required for export");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required for export")
                .EmailAddress().WithMessage("Email must be valid for export");
        }
    }

    /// <summary>
    /// FluentValidation validator for customer data quality checks
    /// </summary>
    public class CustomerDataQualityValidator : AbstractValidator<CustomerExcelDTO>
    {
        public CustomerDataQualityValidator()
        {
            // Data quality checks for customer information
            RuleFor(x => x.Email)
                .Must(NotBeDisposableEmail).WithMessage("Disposable email addresses are not recommended")
                .Must(HaveValidDomain).WithMessage("Email domain appears to be invalid");

            RuleFor(x => x.Phone)
                .Must(BeFormattedConsistently).WithMessage("Phone number format should be consistent");

            RuleFor(x => x.Address)
                .Must(NotContainPOBox).WithMessage("PO Box addresses may have shipping restrictions");

            RuleFor(x => x)
                .Must(HaveConsistentLocationData).WithMessage("Location data (city, state, country) should be consistent")
                .WithName("LocationConsistency");
        }

        private bool NotBeDisposableEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return true;

            // List of common disposable email domains
            var disposableDomains = new[]
            {
                "10minutemail.com", "tempmail.org", "guerrillamail.com", "mailinator.com",
                "throwaway.email", "temp-mail.org", "getnada.com"
            };

            var domain = email.Split('@').LastOrDefault()?.ToLower();
            return !disposableDomains.Contains(domain);
        }

        private bool HaveValidDomain(string email)
        {
            if (string.IsNullOrEmpty(email))
                return true;

            var domain = email.Split('@').LastOrDefault();
            if (string.IsNullOrEmpty(domain))
                return false;

            // Basic domain validation
            return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
        }

        private bool BeFormattedConsistently(string? phone)
        {
            if (string.IsNullOrEmpty(phone))
                return true;

            // Check for consistent formatting patterns
            var hasParentheses = phone.Contains('(') && phone.Contains(')');
            var hasDashes = phone.Contains('-');
            var hasSpaces = phone.Contains(' ');

            // If it has formatting, it should be consistent
            if (hasParentheses)
            {
                return Regex.IsMatch(phone, @"^\(\d{3}\)\s?\d{3}-?\d{4}$");
            }

            return true; // Allow various formats for now
        }

        private bool NotContainPOBox(string address)
        {
            if (string.IsNullOrEmpty(address))
                return true;

            var poBoxPatterns = new[]
            {
                @"\bP\.?O\.?\s*Box\b",
                @"\bPO\s*Box\b",
                @"\bPost\s*Office\s*Box\b"
            };

            return !poBoxPatterns.Any(pattern => 
                Regex.IsMatch(address, pattern, RegexOptions.IgnoreCase));
        }

        private bool HaveConsistentLocationData(CustomerExcelDTO customer)
        {
            // This could include checks like:
            // - State/Province matches Country
            // - Postal code format matches Country
            // - City exists in the specified State/Country
            // For now, just basic validation
            
            if (string.IsNullOrEmpty(customer.Country))
                return true;

            // Example: US postal codes should be 5 or 9 digits
            if (customer.Country.Equals("United States", StringComparison.OrdinalIgnoreCase) ||
                customer.Country.Equals("USA", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrEmpty(customer.PostalCode) || 
                       Regex.IsMatch(customer.PostalCode, @"^\d{5}(-\d{4})?$");
            }

            return true; // Allow other countries for now
        }
    }
}
