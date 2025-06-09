using System.ComponentModel.DataAnnotations;
using DecorStore.API.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.Validators.CustomRules
{
    /// <summary>
    /// Validation attribute for SKU format
    /// </summary>
    public class ValidSkuAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            var sku = value.ToString();
            if (string.IsNullOrEmpty(sku)) return false;
            
            // SKU can contain letters, numbers, hyphens, and underscores
            return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[a-zA-Z0-9\-_]+$");
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} can only contain letters, numbers, hyphens, and underscores";
        }
    }

    /// <summary>
    /// Validation attribute for slug format
    /// </summary>
    public class ValidSlugAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            var slug = value.ToString();
            if (string.IsNullOrEmpty(slug)) return false;
            
            // Slug can contain letters, numbers, hyphens, and underscores
            return System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-zA-Z0-9\-_]+$");
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be URL-safe (only letters, numbers, hyphens, and underscores)";
        }
    }

    /// <summary>
    /// Validation attribute for image files
    /// </summary>
    public class ValidImageAttribute : ValidationAttribute
    {
        private readonly long _maxSizeInBytes;
        private readonly string[] _allowedExtensions;

        public ValidImageAttribute(long maxSizeInMB = 5, params string[] allowedExtensions)
        {
            _maxSizeInBytes = maxSizeInMB * 1024 * 1024; // Convert MB to bytes
            _allowedExtensions = allowedExtensions.Length > 0 
                ? allowedExtensions 
                : new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true; // Allow null for optional fields

            if (value is IFormFile file)
            {
                // Check file size
                if (file.Length > _maxSizeInBytes)
                    return false;

                // Check file extension
                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
                    return false;

                return true;
            }

            if (value is IEnumerable<IFormFile> files)
            {
                foreach (var imageFile in files)
                {
                    if (!IsValid(imageFile))
                        return false;
                }
                return true;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var extensions = string.Join(", ", _allowedExtensions);
            var maxSizeMB = _maxSizeInBytes / (1024 * 1024);
            return $"{name} must be a valid image file ({extensions}) and not exceed {maxSizeMB}MB";
        }
    }

    /// <summary>
    /// Validation attribute for order status transitions
    /// </summary>
    public class ValidOrderStatusAttribute : ValidationAttribute
    {
        private readonly string[] _validStatuses = 
        {
            "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded", "Returned"
        };

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            var status = value.ToString();
            if (string.IsNullOrEmpty(status)) return false;
            
            return _validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        public override string FormatErrorMessage(string name)
        {
            var validStatuses = string.Join(", ", _validStatuses);
            return $"{name} must be one of: {validStatuses}";
        }
    }

    /// <summary>
    /// Validation attribute for phone numbers
    /// </summary>
    public class ValidPhoneNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true; // Allow null for optional fields
            
            var phoneNumber = value.ToString();
            if (string.IsNullOrEmpty(phoneNumber)) return true; // Allow empty for optional fields
            
            // Basic phone number validation - numbers, spaces, hyphens, parentheses, plus sign, periods
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\+]?[\d\s\-\(\)\.]+$");
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} is not in a valid phone number format";
        }
    }

    /// <summary>
    /// Validation attribute for postal codes
    /// </summary>
    public class ValidPostalCodeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true; // Allow null for optional fields
            
            var postalCode = value.ToString();
            if (string.IsNullOrEmpty(postalCode)) return true; // Allow empty for optional fields
            
            // Basic postal code validation - alphanumeric with optional hyphens/spaces
            return System.Text.RegularExpressions.Regex.IsMatch(postalCode, @"^[A-Za-z0-9\s\-]+$");
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} is not in a valid postal code format";
        }
    }

    /// <summary>
    /// Validation attribute for names (first name, last name, etc.)
    /// </summary>
    public class ValidNameAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            var name = value.ToString();
            if (string.IsNullOrEmpty(name)) return false;
            
            // Allow letters, spaces, hyphens, apostrophes, and common accented characters
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-ZÀ-ÿ\s\-'\.]+$");
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} can only contain letters, spaces, hyphens, and apostrophes";
        }
    }

    /// <summary>
    /// Validation attribute for price ranges
    /// </summary>
    public class ValidPriceAttribute : ValidationAttribute
    {
        private readonly decimal _minValue;
        private readonly decimal _maxValue;

        public ValidPriceAttribute(double minValue = 0.01, double maxValue = 1000000)
        {
            _minValue = (decimal)minValue;
            _maxValue = (decimal)maxValue;
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            if (decimal.TryParse(value.ToString(), out var price))
            {
                return price >= _minValue && price <= _maxValue;
            }
            
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between {_minValue:C} and {_maxValue:C}";
        }
    }

    /// <summary>
    /// Validation attribute for quantity ranges
    /// </summary>
    public class ValidQuantityAttribute : ValidationAttribute
    {
        private readonly int _minValue;
        private readonly int _maxValue;

        public ValidQuantityAttribute(int minValue = 1, int maxValue = 1000)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            if (int.TryParse(value.ToString(), out var quantity))
            {
                return quantity >= _minValue && quantity <= _maxValue;
            }
            
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between {_minValue} and {_maxValue}";
        }
    }
}
