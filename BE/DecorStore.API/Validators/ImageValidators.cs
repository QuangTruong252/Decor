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
    }
}
