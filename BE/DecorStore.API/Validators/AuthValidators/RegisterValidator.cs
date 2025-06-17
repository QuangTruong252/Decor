using FluentValidation;
using DecorStore.API.DTOs;
using DecorStore.API.Validators.CustomRules;

namespace DecorStore.API.Validators.AuthValidators
{
    /// <summary>
    /// Validator for user registration
    /// </summary>
    public class RegisterValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterValidator()
        {RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Please provide a valid email address")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .WithErrorCode("EMAIL_INVALID");

            RuleFor(x => x.Password)
                .MustBeStrongPassword()
                .WithErrorCode("PASSWORD_WEAK");

            RuleFor(x => x.ConfirmPassword)
                .MustMatchPassword(x => x.Password)
                .WithErrorCode("PASSWORD_CONFIRMATION_MISMATCH");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(1, 50).WithMessage("First name must be between 1 and 50 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name can only contain letters, spaces, hyphens, apostrophes, and periods")
                .WithErrorCode("FIRST_NAME_INVALID");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(1, 50).WithMessage("Last name must be between 1 and 50 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name can only contain letters, spaces, hyphens, apostrophes, and periods")
                .WithErrorCode("LAST_NAME_INVALID");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Please provide a valid phone number")
                .WithErrorCode("PHONE_INVALID");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .Must(dateOfBirth => dateOfBirth.HasValue && BeValidAge(dateOfBirth.Value)).WithMessage("You must be at least 13 years old to register")
                .WithErrorCode("AGE_INVALID");

            RuleFor(x => x.AcceptTerms)
                .Equal(true).WithMessage("You must accept the terms and conditions")
                .WithErrorCode("TERMS_NOT_ACCEPTED");

            RuleFor(x => x.AcceptPrivacyPolicy)
                .Equal(true).WithMessage("You must accept the privacy policy")
                .WithErrorCode("PRIVACY_POLICY_NOT_ACCEPTED");
        }        private bool BeValidAge(DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
                age--;

            return age >= 13;
        }
    }
}
