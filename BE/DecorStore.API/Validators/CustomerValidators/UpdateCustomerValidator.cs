using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;

namespace DecorStore.API.Validators.CustomerValidators
{
    /// <summary>
    /// Validator for updating existing customers
    /// </summary>
    public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 100).WithMessage("First name must be between 2 and 100 characters")
                .Must(BeValidName).WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 100).WithMessage("Last name must be between 2 and 100 characters")
                .Must(BeValidName).WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .EmailAddress().WithMessage("Please provide a valid email address");

            RuleFor(x => x.Address)
                .MaximumLength(255).WithMessage("Address cannot exceed 255 characters");

            RuleFor(x => x.City)
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
                .Must(BeValidCityName).WithMessage("City name can only contain letters, spaces, hyphens, and apostrophes")
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.State)
                .MaximumLength(50).WithMessage("State cannot exceed 50 characters")
                .Must(BeValidStateName).WithMessage("State name can only contain letters, spaces, hyphens, and apostrophes")
                .When(x => !string.IsNullOrEmpty(x.State));

            RuleFor(x => x.PostalCode)
                .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
                .Must(BeValidPostalCode).WithMessage("Invalid postal code format")
                .When(x => !string.IsNullOrEmpty(x.PostalCode));

            RuleFor(x => x.Country)
                .MaximumLength(50).WithMessage("Country cannot exceed 50 characters")
                .Must(BeValidCountryName).WithMessage("Country name can only contain letters, spaces, hyphens, and apostrophes")
                .When(x => !string.IsNullOrEmpty(x.Country));

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
                .Must(BeValidPhoneNumber).WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            // Validate complete address if any address field is provided
            RuleFor(x => x)
                .Must(HaveCompleteAddressIfAnyAddressProvided)
                .WithMessage("If providing address information, City and Country are required")
                .WithName("Address");
        }

        /// <summary>
        /// Validates customer update with email uniqueness check excluding current customer
        /// </summary>
        /// <param name="dto">The update DTO</param>
        /// <param name="customerId">The ID of the customer being updated</param>
        /// <returns>Validation result</returns>
        public async Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(UpdateCustomerDTO dto, int customerId)
        {
            // First run standard validation
            var result = await ValidateAsync(dto);

            if (!result.IsValid)
                return result;

            // Additional validation for update-specific rules
            var additionalErrors = new List<FluentValidation.Results.ValidationFailure>();

            // Check email uniqueness excluding current customer
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var emailExists = await _unitOfWork.Customers.ExistsByEmailAsync(dto.Email, customerId);
                if (emailExists)
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.Email), 
                        "Email address is already registered to another customer")
                    {
                        ErrorCode = "EMAIL_ALREADY_EXISTS"
                    });
                }
            }

            // Check phone uniqueness excluding current customer (if phone is provided)
            if (!string.IsNullOrEmpty(dto.Phone))
            {
                var phoneExists = await _unitOfWork.Customers.ExistsByPhoneAsync(dto.Phone, customerId);
                if (phoneExists)
                {
                    additionalErrors.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.Phone), 
                        "Phone number is already registered to another customer")
                    {
                        ErrorCode = "PHONE_ALREADY_EXISTS"
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

        private static bool BeValidName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            
            // Allow letters, spaces, hyphens, apostrophes, and common accented characters
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-ZÀ-ÿ\s\-'\.]+$");
        }

        private static bool BeValidCityName(string city)
        {
            if (string.IsNullOrEmpty(city)) return true; // Optional field
            
            // Allow letters, spaces, hyphens, apostrophes, periods
            return System.Text.RegularExpressions.Regex.IsMatch(city, @"^[a-zA-ZÀ-ÿ\s\-'\.]+$");
        }

        private static bool BeValidStateName(string state)
        {
            if (string.IsNullOrEmpty(state)) return true; // Optional field
            
            // Allow letters, spaces, hyphens, apostrophes, periods
            return System.Text.RegularExpressions.Regex.IsMatch(state, @"^[a-zA-ZÀ-ÿ\s\-'\.]+$");
        }

        private static bool BeValidCountryName(string country)
        {
            if (string.IsNullOrEmpty(country)) return true; // Optional field
            
            // Allow letters, spaces, hyphens, apostrophes, periods
            return System.Text.RegularExpressions.Regex.IsMatch(country, @"^[a-zA-ZÀ-ÿ\s\-'\.]+$");
        }

        private static bool BeValidPostalCode(string postalCode)
        {
            if (string.IsNullOrEmpty(postalCode)) return true; // Optional field
            
            // Basic postal code validation - alphanumeric with optional hyphens/spaces
            return System.Text.RegularExpressions.Regex.IsMatch(postalCode, @"^[A-Za-z0-9\s\-]+$");
        }

        private static bool BeValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return true; // Optional field
            
            // Allow numbers, spaces, hyphens, parentheses, plus sign, periods
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\+]?[\d\s\-\(\)\.]+$");
        }

        private static bool HaveCompleteAddressIfAnyAddressProvided(UpdateCustomerDTO customer)
        {
            var hasAnyAddressField = !string.IsNullOrEmpty(customer.Address) ||
                                   !string.IsNullOrEmpty(customer.City) ||
                                   !string.IsNullOrEmpty(customer.State) ||
                                   !string.IsNullOrEmpty(customer.PostalCode) ||
                                   !string.IsNullOrEmpty(customer.Country);

            if (!hasAnyAddressField)
                return true; // No address provided is fine

            // If any address field is provided, City and Country are required
            return !string.IsNullOrEmpty(customer.City) && !string.IsNullOrEmpty(customer.Country);
        }
    }
}
