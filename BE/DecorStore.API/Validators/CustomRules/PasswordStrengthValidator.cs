using FluentValidation;
using System.Text.RegularExpressions;

namespace DecorStore.API.Validators.CustomRules
{
    /// <summary>
    /// Custom validator for password strength requirements
    /// </summary>
    public static class PasswordStrengthValidator
    {
        private static readonly string[] CommonPasswords = new[]
        {
            "password", "123456", "password123", "admin", "qwerty", "letmein",
            "welcome", "monkey", "1234567890", "abc123", "111111", "123123",
            "password1", "1234", "12345", "dragon", "master", "login"
        };

        /// <summary>
        /// Validates password strength according to security requirements
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .MaximumLength(128).WithMessage("Password cannot exceed 128 characters")
                .Must(HaveUppercaseLetter).WithMessage("Password must contain at least one uppercase letter")
                .Must(HaveLowercaseLetter).WithMessage("Password must contain at least one lowercase letter")
                .Must(HaveDigit).WithMessage("Password must contain at least one number")
                .Must(HaveSpecialCharacter).WithMessage("Password must contain at least one special character (!@#$%^&*)")
                .Must(NotBeCommonPassword).WithMessage("Password is too common. Please choose a more secure password")
                .Must(NotHaveRepeatingCharacters).WithMessage("Password cannot have more than 3 consecutive repeating characters")
                .Must(NotBeSequential).WithMessage("Password cannot contain sequential characters (e.g., 123, abc)")
                .WithErrorCode("PASSWORD_STRENGTH_INVALID");
        }

        /// <summary>
        /// Validates password confirmation matches the original password
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustMatchPassword<T>(this IRuleBuilder<T, string> ruleBuilder, Func<T, string> passwordSelector)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Password confirmation is required")
                .Must((model, confirmPassword) => confirmPassword == passwordSelector(model))
                .WithMessage("Password confirmation does not match the password")
                .WithErrorCode("PASSWORD_CONFIRMATION_MISMATCH");
        }

        private static bool HaveUppercaseLetter(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
        }

        private static bool HaveLowercaseLetter(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
        }

        private static bool HaveDigit(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
        }

        private static bool HaveSpecialCharacter(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            
            var specialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            return password.Any(c => specialCharacters.Contains(c));
        }

        private static bool NotBeCommonPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            
            return !CommonPasswords.Contains(password.ToLower());
        }

        private static bool NotHaveRepeatingCharacters(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 4) return true;
            
            for (int i = 0; i <= password.Length - 4; i++)
            {
                if (password[i] == password[i + 1] && 
                    password[i + 1] == password[i + 2] && 
                    password[i + 2] == password[i + 3])
                {
                    return false;
                }
            }
            
            return true;
        }

        private static bool NotBeSequential(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 3) return true;
            
            // Check for ascending sequences
            for (int i = 0; i <= password.Length - 3; i++)
            {
                if (char.IsLetterOrDigit(password[i]) && 
                    char.IsLetterOrDigit(password[i + 1]) && 
                    char.IsLetterOrDigit(password[i + 2]))
                {
                    if (password[i + 1] == password[i] + 1 && 
                        password[i + 2] == password[i + 1] + 1)
                    {
                        return false;
                    }
                }
            }
            
            // Check for descending sequences
            for (int i = 0; i <= password.Length - 3; i++)
            {
                if (char.IsLetterOrDigit(password[i]) && 
                    char.IsLetterOrDigit(password[i + 1]) && 
                    char.IsLetterOrDigit(password[i + 2]))
                {
                    if (password[i + 1] == password[i] - 1 && 
                        password[i + 2] == password[i + 1] - 1)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Calculates password strength score (0-100)
        /// </summary>
        public static int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;
            
            int score = 0;
            
            // Length scoring
            if (password.Length >= 8) score += 20;
            if (password.Length >= 12) score += 10;
            if (password.Length >= 16) score += 10;
            
            // Character variety scoring
            if (HaveUppercaseLetter(password)) score += 15;
            if (HaveLowercaseLetter(password)) score += 15;
            if (HaveDigit(password)) score += 15;
            if (HaveSpecialCharacter(password)) score += 15;
            
            // Bonus points for complexity
            if (NotBeCommonPassword(password)) score += 5;
            if (NotHaveRepeatingCharacters(password)) score += 5;
            if (NotBeSequential(password)) score += 5;
            
            // Penalty for common patterns
            if (Regex.IsMatch(password, @"(.)\1{2,}")) score -= 10; // Repeating characters
            if (Regex.IsMatch(password, @"(012|123|234|345|456|567|678|789|890|abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)", RegexOptions.IgnoreCase)) score -= 10;
            
            return Math.Max(0, Math.Min(100, score));
        }

        /// <summary>
        /// Gets password strength description
        /// </summary>
        public static string GetPasswordStrengthDescription(int score)
        {
            return score switch
            {
                >= 80 => "Very Strong",
                >= 60 => "Strong",
                >= 40 => "Moderate",
                >= 20 => "Weak",
                _ => "Very Weak"
            };
        }
    }
}
