using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using DecorStore.API.Common;
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

        public async Task<Result<string>> UploadImageAsync(IFormFile file, string folderName)
        {
            if(file == null || file.Length == 0)
            {
                return Result<string>.Failure("File cannot be null or empty", "INVALID_FILE");
            }

            var validationResult = IsValidImage(file);
            if (validationResult.IsFailure)
            {
                return Result<string>.Failure(validationResult.Error!, validationResult.ErrorCode!);
            }

            try
            {
                var uploadPath = Path.Combine(_baseImagePath, folderName);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate a unique file name
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var urlFriendlyPath = Path.Combine(folderName, fileName).Replace("\\", "/");
                return Result<string>.Success(urlFriendlyPath);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Failed to upload image: {ex.Message}", "UPLOAD_ERROR");
            }
        }

        public async Task<Result<bool>> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return Result<bool>.Failure("Image URL cannot be null or empty", "INVALID_INPUT");
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
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Success(false);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error deleting image: {ex.Message}", "DELETE_ERROR");
            }
        }

        public async Task<Result<string>> UpdateImageAsync(string oldImageUrl, IFormFile file, string folderName)
        {
            // Delete the old image if it exists
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                var deleteResult = await DeleteImageAsync(oldImageUrl);
                if (deleteResult.IsFailure)
                {
                    return Result<string>.Failure($"Failed to delete old image: {deleteResult.Error}", "DELETE_OLD_IMAGE_ERROR");
                }
            }

            // Upload the new image
            var uploadResult = await UploadImageAsync(file, folderName);
            if (uploadResult.IsFailure)
            {
                return Result<string>.Failure($"Failed to upload new image: {uploadResult.Error}", uploadResult.ErrorCode!);
            }

            return Result<string>.Success(uploadResult.Data!);
        }

        public Result<bool> IsValidImage(IFormFile file)
        {
            if (file == null)
            {
                return Result<bool>.Failure("File cannot be null", "INVALID_FILE");
            }

            if (file.Length > _maxFileSize)
            {
                return Result<bool>.Failure($"File size {file.Length} bytes exceeds maximum allowed size of {_maxFileSize} bytes", "FILE_TOO_LARGE");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if(string.IsNullOrEmpty(fileExtension) || !_allowedExtensions.Contains(fileExtension))
            {
                return Result<bool>.Failure($"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}", "INVALID_FILE_TYPE");
            }

            return Result<bool>.Success(true);
        }
        
        public async Task<Result<List<int>>> GetOrCreateImagesAsync(List<IFormFile> files, string folderName = "images")
        {
            if (files == null || !files.Any())
            {
                return Result<List<int>>.Failure("No files provided", "INVALID_INPUT");
            }

            if (files.Count > 10)
            {
                return Result<List<int>>.Failure("Maximum 10 files can be uploaded at once", "TOO_MANY_FILES");
            }

            try
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
                            var validationResult = IsValidImage(file);
                            if (validationResult.IsFailure)
                            {
                                return Result<List<int>>.Failure($"File '{file.FileName}': {validationResult.Error}", validationResult.ErrorCode!);
                            }

                            // Upload new image
                            var uploadResult = await UploadImageAsync(file, folderName);
                            if (uploadResult.IsFailure)
                            {
                                return Result<List<int>>.Failure($"Failed to upload '{file.FileName}': {uploadResult.Error}", uploadResult.ErrorCode!);
                            }

                            uploadedFiles.Add((file.FileName, uploadResult.Data!));

                            // Create image record in database
                            var image = new Image
                            {
                                FileName = file.FileName,
                                FilePath = uploadResult.Data!,
                                AltText = Path.GetFileNameWithoutExtension(file.FileName),
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            await _unitOfWork.Images.AddAsync(image);
                            await _unitOfWork.SaveChangesAsync();
                            imageIds.Add(image.Id);
                        }
                        
                        await _unitOfWork.CommitTransactionAsync();
                        return Result<List<int>>.Success(imageIds);
                    }
                    catch (Exception ex)
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
                        
                        return Result<List<int>>.Failure($"Failed to create images: {ex.Message}", "CREATE_IMAGES_ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result<List<int>>.Failure($"Execution strategy failed: {ex.Message}", "EXECUTION_STRATEGY_ERROR");
            }
        }

        public async Task<Result<bool>> ImageExistsInSystemAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Result<bool>.Failure("File path cannot be null or empty", "INVALID_INPUT");
            }

            try
            {
                var images = await _unitOfWork.Images.GetAllAsync();
                var exists = images.Any(i => i.FilePath == filePath && !i.IsDeleted);
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error checking image existence: {ex.Message}", "CHECK_EXISTENCE_ERROR");
            }
        }

        public async Task<Result<List<Image>>> GetImagesByIdsAsync(List<int> imageIds)
        {
            if (imageIds == null || !imageIds.Any())
            {
                return Result<List<Image>>.Failure("Image IDs list cannot be null or empty", "INVALID_INPUT");
            }

            if (imageIds.Any(id => id <= 0))
            {
                return Result<List<Image>>.Failure("All image IDs must be positive integers", "INVALID_ID");
            }

            try
            {
                var images = await _unitOfWork.Images.GetManyByIdsAsync(imageIds);
                return Result<List<Image>>.Success(images);
            }
            catch (Exception ex)
            {
                return Result<List<Image>>.Failure($"Error retrieving images by IDs: {ex.Message}", "RETRIEVE_IMAGES_ERROR");
            }
        }

        public async Task<Result<List<Image>>> GetAllImagesAsync()
        {
            try
            {
                var images = await _unitOfWork.Images.GetAllAsync();
                return Result<List<Image>>.Success(images);
            }
            catch (Exception ex)
            {
                return Result<List<Image>>.Failure($"Error retrieving all images: {ex.Message}", "RETRIEVE_ALL_IMAGES_ERROR");
            }
        }

        public async Task<Result<List<Image>>> GetImagesByFilePathsAsync(List<string> filePaths)
        {
            if (filePaths == null || !filePaths.Any())
            {
                return Result<List<Image>>.Failure("File paths list cannot be null or empty", "INVALID_INPUT");
            }

            try
            {
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
                    return Result<List<Image>>.Success(new List<Image>());
                }

                var images = await _unitOfWork.Images.GetByFilePathsAsync(sanitizedPaths);
                return Result<List<Image>>.Success(images);
            }
            catch (Exception ex)
            {
                return Result<List<Image>>.Failure($"Error retrieving images by file paths: {ex.Message}", "RETRIEVE_BY_PATHS_ERROR");
            }
        }

        public async Task<Result> AssignImageToProductAsync(int imageId, int productId)
        {
            if (imageId <= 0)
            {
                return Result.Failure("Image ID must be a positive integer", "INVALID_IMAGE_ID");
            }

            if (productId <= 0)
            {
                return Result.Failure("Product ID must be a positive integer", "INVALID_PRODUCT_ID");
            }

            try
            {
                await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    await _unitOfWork.Images.AddProductImageAsync(imageId, productId);
                    await _unitOfWork.SaveChangesAsync();
                    return Task.CompletedTask;
                });

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error assigning image to product: {ex.Message}", "ASSIGN_IMAGE_ERROR");
            }
        }

        public async Task<Result> AssignImageToCategoryAsync(int imageId, int categoryId)
        {
            if (imageId <= 0)
            {
                return Result.Failure("Image ID must be a positive integer", "INVALID_IMAGE_ID");
            }

            if (categoryId <= 0)
            {
                return Result.Failure("Category ID must be a positive integer", "INVALID_CATEGORY_ID");
            }

            try
            {
                await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    await _unitOfWork.Images.AddCategoryImageAsync(imageId, categoryId);
                    await _unitOfWork.SaveChangesAsync();
                    return Task.CompletedTask;
                });

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error assigning image to category: {ex.Message}", "ASSIGN_IMAGE_ERROR");
            }
        }

        public async Task<Result> UnassignImageFromProductAsync(int imageId, int productId)
        {
            if (imageId <= 0)
            {
                return Result.Failure("Image ID must be a positive integer", "INVALID_IMAGE_ID");
            }

            if (productId <= 0)
            {
                return Result.Failure("Product ID must be a positive integer", "INVALID_PRODUCT_ID");
            }

            try
            {
                await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    _unitOfWork.Images.RemoveProductImage(imageId, productId);
                    await _unitOfWork.SaveChangesAsync();
                    return Task.CompletedTask;
                });

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error unassigning image from product: {ex.Message}", "UNASSIGN_IMAGE_ERROR");
            }
        }

        public async Task<Result> UnassignImageFromCategoryAsync(int imageId, int categoryId)
        {
            if (imageId <= 0)
            {
                return Result.Failure("Image ID must be a positive integer", "INVALID_IMAGE_ID");
            }

            if (categoryId <= 0)
            {
                return Result.Failure("Category ID must be a positive integer", "INVALID_CATEGORY_ID");
            }

            try
            {
                await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    _unitOfWork.Images.RemoveCategoryImage(imageId, categoryId);
                    await _unitOfWork.SaveChangesAsync();
                    return Task.CompletedTask;
                });

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error unassigning image from category: {ex.Message}", "UNASSIGN_IMAGE_ERROR");
            }
        }
    }
}
