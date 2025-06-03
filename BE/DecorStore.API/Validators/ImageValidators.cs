using FluentValidation;
using DecorStore.API.DTOs;

namespace DecorStore.API.Validators
{
    public class ImageUploadDTOValidator : AbstractValidator<ImageUploadDTO>
    {
        public ImageUploadDTOValidator()
        {
            RuleFor(x => x.Files)
                .NotNull().WithMessage("Files are required")
                .Must(files => files != null && files.Count > 0)
                .WithMessage("At least one file must be provided")
                .Must(files => files == null || files.Count <= 10)
                .WithMessage("Maximum 10 files can be uploaded at once");

            RuleForEach(x => x.Files)
                .Must(file => file != null && file.Length > 0)
                .WithMessage("File cannot be empty")
                .Must(file => file == null || file.Length <= 5 * 1024 * 1024) // 5MB
                .WithMessage("File size cannot exceed 5MB")
                .Must(file => file == null || IsValidImageExtension(file.FileName))
                .WithMessage("Only .jpg, .jpeg, .png, .gif files are allowed");
        }

        private bool IsValidImageExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return allowedExtensions.Contains(extension);
        }
    }    public class CreateProductDTOValidator : AbstractValidator<CreateProductDTO>
    {
        public CreateProductDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(3, 255).WithMessage("Product name must be between 3 and 255 characters");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Product slug is required")
                .MaximumLength(255).WithMessage("Product slug cannot exceed 255 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be 0 or greater");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Valid category ID is required");

            // Image validation - only ImageIds allowed
            RuleFor(x => x.ImageIds)
                .Must(imageIds => imageIds == null || imageIds.Count <= 20)
                .WithMessage("Maximum 20 images can be associated with a product")
                .Must(imageIds => imageIds == null || imageIds.All(id => id > 0))
                .WithMessage("All image IDs must be positive integers");
        }
    }    public class UpdateProductDTOValidator : AbstractValidator<UpdateProductDTO>
    {
        public UpdateProductDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(3, 255).WithMessage("Product name must be between 3 and 255 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be 0 or greater");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Valid category ID is required");

            // Image validation - only ImageIds allowed
            RuleFor(x => x.ImageIds)
                .Must(imageIds => imageIds == null || imageIds.Count <= 20)
                .WithMessage("Maximum 20 images can be associated with a product")
                .Must(imageIds => imageIds == null || imageIds.All(id => id > 0))
                .WithMessage("All image IDs must be positive integers");
        }
    }
}
