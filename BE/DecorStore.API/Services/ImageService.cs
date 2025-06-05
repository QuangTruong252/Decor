using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.Services
{
    public class ImageService : IImageService
    {
        private readonly string _baseImagePath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        
        public ImageService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _baseImagePath = _configuration["ImageSettings:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            _allowedExtensions = _configuration["ImageSettings:AllowedExtensions"]?.Split(",") ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };
            _maxFileSize = !string.IsNullOrEmpty(_configuration["ImageSettings:MaxFileSize"]) ?
                long.Parse(_configuration["ImageSettings:MaxFileSize"]) : 5 * 1024 * 1024; // 5 MB
            if(!Directory.Exists(_baseImagePath))
            {
                Directory.CreateDirectory(_baseImagePath);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
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
                await file.CopyToAsync(stream);
            }
            return Path.Combine(folderName, fileName).Replace("\\", "/"); // Ensure URL-friendly path;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
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
                }

                // Delete image record from database if it exists
                var image = await _unitOfWork.Images.GetByFilePathAsync(imageUrl);
                if (image != null)
                {
                    _unitOfWork.Images.Delete(image);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }

                return false;
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
        
        public async Task<List<int>> GetOrCreateImagesAsync(List<IFormFile> files, string folderName = "images")
        {
            return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                var imageIds = new List<int>();
                var uploadedFiles = new List<(string fileName, string filePath)>();
                
                try
                {
                    await _unitOfWork.BeginTransactionAsync();
                    
                    foreach (var file in files)
                    {
                        if (!IsValidImage(file))
                        {
                            throw new ArgumentException($"Invalid file type: {file.FileName}");
                        }

                        // Upload new image
                        var imagePath = await UploadImageAsync(file, folderName);
                        uploadedFiles.Add((file.FileName, imagePath));

                        // Create image record in database
                        var image = new Image
                        {
                            FileName = file.FileName,
                            FilePath = imagePath,
                            AltText = Path.GetFileNameWithoutExtension(file.FileName),
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        await _unitOfWork.Images.AddAsync(image);
                        await _unitOfWork.SaveChangesAsync();
                        imageIds.Add(image.Id);
                    }
                    
                    await _unitOfWork.CommitTransactionAsync();
                    return imageIds;
                }
                catch
                {
                    // Rollback transaction
                    await _unitOfWork.RollbackTransactionAsync();
                    
                    // Clean up uploaded files if database operation failed
                    foreach (var (fileName, filePath) in uploadedFiles)
                    {
                        try
                        {
                            await DeleteImageAsync(filePath);
                        }
                        catch
                        {
                            // Log the cleanup failure but don't throw
                        }
                    }
                    
                    throw;
                }
            });
        }

        public async Task<bool> ImageExistsInSystemAsync(string filePath)
        {
            var images = await _unitOfWork.Images.GetAllAsync();
            return images.Any(i => i.FilePath == filePath && !i.IsDeleted);
        }

        public async Task<List<Image>> GetImagesByIdsAsync(List<int> imageIds)
        {
            return await _unitOfWork.Images.GetManyByIdsAsync(imageIds);
        }

        public async Task<List<Image>> GetAllImagesAsync()
        {
            return await _unitOfWork.Images.GetAllAsync();
        }

        public async Task<List<Image>> GetImagesByFilePathsAsync(List<string> filePaths)
        {
            if (filePaths == null || !filePaths.Any())
            {
                throw new ArgumentException("File paths list cannot be null or empty.");
            }

            // Validate and sanitize file paths to prevent path traversal attacks
            var sanitizedPaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                // Remove any path traversal attempts
                var sanitizedPath = path.Replace("../", "").Replace("..\\", "");
                
                // Ensure the path doesn't start with directory separators
                sanitizedPath = sanitizedPath.TrimStart('/', '\\');
                
                if (!string.IsNullOrWhiteSpace(sanitizedPath))
                {
                    sanitizedPaths.Add(sanitizedPath);
                }
            }

            if (!sanitizedPaths.Any())
            {
                return new List<Image>();
            }

            return await _unitOfWork.Images.GetByFilePathsAsync(sanitizedPaths);
        }

        public async Task AssignImageToProductAsync(int imageId, int productId)
        {
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                await _unitOfWork.Images.AddProductImageAsync(imageId, productId);
                await _unitOfWork.SaveChangesAsync();
                return Task.CompletedTask;
            });
        }

        public async Task AssignImageToCategoryAsync(int imageId, int categoryId)
        {
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                await _unitOfWork.Images.AddCategoryImageAsync(imageId, categoryId);
                await _unitOfWork.SaveChangesAsync();
                return Task.CompletedTask;
            });
        }

        public async Task UnassignImageFromProductAsync(int imageId, int productId)
        {
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                _unitOfWork.Images.RemoveProductImage(imageId, productId);
                await _unitOfWork.SaveChangesAsync();
                return Task.CompletedTask;
            });
        }

        public async Task UnassignImageFromCategoryAsync(int imageId, int categoryId)
        {
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                _unitOfWork.Images.RemoveCategoryImage(imageId, categoryId);
                await _unitOfWork.SaveChangesAsync();
                return Task.CompletedTask;
            });
        }
    }
}
