using FluentValidation;
using DecorStore.API.DTOs;

namespace DecorStore.API.Validators.AuthValidators
{
    /// <summary>
    /// Validator for user login
    /// </summary>
    public class LoginValidator : AbstractValidator<LoginDTO>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Please provide a valid email address")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .WithErrorCode("EMAIL_INVALID");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(1).WithMessage("Password cannot be empty")
                .WithErrorCode("PASSWORD_REQUIRED");

            RuleFor(x => x.RememberMe)
                .NotNull().WithMessage("Remember me option must be specified")
                .WithErrorCode("REMEMBER_ME_INVALID");
        }
    }
}
