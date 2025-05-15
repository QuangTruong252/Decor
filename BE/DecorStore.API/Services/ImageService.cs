using DecorStore.API.Interfaces;

namespace DecorStore.API.Services
{
    public class ImageService : IImageService
    {
        private readonly string _baseImagePath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB
        private readonly IConfiguration _configuration;
        public ImageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _baseImagePath = _configuration["ImageSettings:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            _allowedExtensions = _configuration["ImageSettings:AllowedExtensions"]?.Split(",") ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };
            _maxFileSize = _configuration["ImageSettings:MaxFileSize"] != null ? long.Parse(_configuration["ImageSettings:MaxFileSize"]) : 5 * 1024 * 1024; // 5 MB
            if(!Directory.Exists(_baseImagePath))
            {
                Directory.CreateDirectory(_baseImagePath);
            }
        }
        public Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if(file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.");
            }
            if (!IsValidImage(file))
            {
                throw new ArgumentException("Invalid file type.");
            }
            var uploadPath = Path.Combine(_baseImagePath, folderName);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            // generate a unique file name
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return Task.FromResult(Path.Combine(folderName, fileName));
        }
        public Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentException("Image URL cannot be null or empty.");
            }
            try
            {
                var filePath = Path.Combine(_baseImagePath, imageUrl);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                // log the exception
                throw new Exception("Error deleting image", ex);
            }
        }
        public async Task<string> UpdateImageAsync(string oldImageUrl, IFormFile file, string folderName)
        {
            // delete the old image if it exists
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await DeleteImageAsync(oldImageUrl);
            }

            // upload the new image
            return await UploadImageAsync(file, folderName);
        }
        public bool IsValidImage(IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                return false;
            }
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if(string.IsNullOrEmpty(fileExtension) || !_allowedExtensions.Contains(fileExtension))
            {
                return false;
            }
            return true;
        }
    }
}
