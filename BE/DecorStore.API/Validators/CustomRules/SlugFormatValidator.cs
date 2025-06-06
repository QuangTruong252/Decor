using FluentValidation;
using System.Text.RegularExpressions;

namespace DecorStore.API.Validators.CustomRules
{
    /// <summary>
    /// Custom validator for URL slug format validation
    /// </summary>
    public static class SlugFormatValidator
    {
        private static readonly Regex SlugRegex = new Regex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly string[] ReservedSlugs = new[]
        {
            "admin", "api", "www", "mail", "ftp", "localhost", "test", "staging",
            "dev", "development", "prod", "production", "app", "application",
            "service", "services", "support", "help", "about", "contact",
            "privacy", "terms", "legal", "blog", "news", "home", "index",
            "login", "logout", "register", "signup", "signin", "auth",
            "account", "profile", "settings", "config", "dashboard",
            "search", "browse", "category", "categories", "product", "products",
            "order", "orders", "cart", "checkout", "payment", "billing"
        };

        /// <summary>
        /// Validates that a string is a valid URL slug
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustBeValidSlug<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Slug is required")
                .Length(1, 100).WithMessage("Slug must be between 1 and 100 characters")
                .Must(BeValidSlugFormat).WithMessage("Slug can only contain lowercase letters, numbers, and hyphens. Must start and end with alphanumeric characters")
                .Must(NotBeReservedSlug).WithMessage("This slug is reserved and cannot be used")
                .Must(NotHaveConsecutiveHyphens).WithMessage("Slug cannot contain consecutive hyphens")
                .Must(NotStartOrEndWithHyphen).WithMessage("Slug cannot start or end with a hyphen")
                .WithErrorCode("SLUG_FORMAT_INVALID");
        }

        /// <summary>
        /// Validates that a string is a valid URL slug with uniqueness check
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustBeValidUniqueSlug<T>(this IRuleBuilder<T, string> ruleBuilder, 
            Func<string, CancellationToken, Task<bool>> uniquenessCheck)
        {
            return ruleBuilder
                .MustBeValidSlug()
                .MustAsync(uniquenessCheck).WithMessage("This slug is already in use")
                .WithErrorCode("SLUG_NOT_UNIQUE");
        }

        /// <summary>
        /// Generates a valid slug from a given string
        /// </summary>
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase
            string slug = input.ToLowerInvariant();

            // Remove diacritics (accented characters)
            slug = RemoveDiacritics(slug);

            // Replace spaces and invalid characters with hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "-");

            // Remove consecutive hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Remove leading and trailing hyphens
            slug = slug.Trim('-');

            // Ensure it's not too long
            if (slug.Length > 100)
            {
                slug = slug.Substring(0, 100).TrimEnd('-');
            }

            // If the result is empty or reserved, generate a fallback
            if (string.IsNullOrEmpty(slug) || ReservedSlugs.Contains(slug))
            {
                slug = $"item-{Guid.NewGuid().ToString("N")[..8]}";
            }

            return slug;
        }

        /// <summary>
        /// Validates slug format using regex
        /// </summary>
        private static bool BeValidSlugFormat(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            return SlugRegex.IsMatch(slug);
        }

        /// <summary>
        /// Checks if slug is not in the reserved list
        /// </summary>
        private static bool NotBeReservedSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            return !ReservedSlugs.Contains(slug.ToLowerInvariant());
        }

        /// <summary>
        /// Checks for consecutive hyphens
        /// </summary>
        private static bool NotHaveConsecutiveHyphens(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            return !slug.Contains("--");
        }

        /// <summary>
        /// Checks that slug doesn't start or end with hyphen
        /// </summary>
        private static bool NotStartOrEndWithHyphen(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            return !slug.StartsWith("-") && !slug.EndsWith("-");
        }

        /// <summary>
        /// Removes diacritics (accented characters) from a string
        /// </summary>
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        /// <summary>
        /// Validates that a slug is URL-safe
        /// </summary>
        public static bool IsUrlSafe(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Check if the slug would be the same after URL encoding/decoding
            var encoded = Uri.EscapeDataString(slug);
            var decoded = Uri.UnescapeDataString(encoded);

            return slug == decoded && slug == encoded;
        }

        /// <summary>
        /// Gets suggestions for improving an invalid slug
        /// </summary>
        public static string[] GetSlugSuggestions(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new[] { "Please provide a valid input" };

            var suggestions = new List<string>();
            var baseSlug = GenerateSlug(input);

            if (!string.IsNullOrEmpty(baseSlug))
            {
                suggestions.Add(baseSlug);
                
                // Add variations
                suggestions.Add($"{baseSlug}-1");
                suggestions.Add($"{baseSlug}-new");
                suggestions.Add($"{baseSlug}-{DateTime.Now.Year}");
            }

            return suggestions.Take(5).ToArray();
        }
    }
}
