using DecorStore.API.DTOs.FileManagement;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using DecorStore.API.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace DecorStore.API.Services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IImageRepository _imageRepository;
        private readonly IImageService _imageService;
        private readonly IConfiguration _configuration;
        private readonly string _uploadsPath;
        private readonly string _thumbnailsPath;
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public FileManagerService(
            IImageRepository imageRepository,
            IImageService imageService,
            IConfiguration configuration)
        {
            _imageRepository = imageRepository;
            _imageService = imageService;
            _configuration = configuration;
            _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            _thumbnailsPath = Path.Combine(_uploadsPath, ".thumbnails");
            
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);
            
            if (!Directory.Exists(_thumbnailsPath))
                Directory.CreateDirectory(_thumbnailsPath);
        }

        public async Task<Result<FileBrowseResponseDTO>> BrowseFilesAsync(FileBrowseRequestDTO request)
        {
            // Input validation
            if (request == null)
            {
                return Result<FileBrowseResponseDTO>.Failure("Request cannot be null", "INVALID_INPUT");
            }

            if (request.PageNumber <= 0)
            {
                return Result<FileBrowseResponseDTO>.Failure("Page number must be greater than 0", "INVALID_PAGE_NUMBER");
            }

            if (request.PageSize <= 0 || request.PageSize > 100)
            {
                return Result<FileBrowseResponseDTO>.Failure("Page size must be between 1 and 100", "INVALID_PAGE_SIZE");
            }

            try
            {
                var safePath = await GetSafePathAsync(request.Path);
                var fullPath = Path.Combine(_uploadsPath, safePath);

                if (!Directory.Exists(fullPath))
                {
                    return Result<FileBrowseResponseDTO>.Failure($"Directory not found: {request.Path}", "DIRECTORY_NOT_FOUND");
                }

                var allItems = new List<FileItemDTO>();

                // Get directories
                var directoryTasks = Directory.GetDirectories(fullPath)
                    .Where(d => !Path.GetFileName(d).StartsWith(".")) // Skip hidden folders
                    .Select(d => CreateFolderItem(d, safePath))
                    .ToList();
                var directories = await Task.WhenAll(directoryTasks);
                allItems.AddRange(directories);

                // Get files
                var fileTasks = Directory.GetFiles(fullPath)
                    .Where(f => !Path.GetFileName(f).StartsWith(".")) // Skip hidden files
                    .Select(f => CreateFileItem(f, safePath))
                    .ToList();
                var files = await Task.WhenAll(fileTasks);
                allItems.AddRange(files);

                // Apply filters
                allItems = await ApplyFiltersAsync(allItems, request);

                // Apply sorting
                allItems = ApplySorting(allItems, request.SortBy, request.SortOrder);

                // Apply pagination
                var totalItems = allItems.Count;
                var paginatedItems = allItems
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var response = new FileBrowseResponseDTO
                {
                    CurrentPath = safePath,
                    ParentPath = GetParentPath(safePath),
                    Items = paginatedItems,
                    TotalItems = totalItems,
                    TotalFiles = allItems.Count(i => i.Type != "folder"),
                    TotalFolders = allItems.Count(i => i.Type == "folder"),
                    TotalSize = allItems.Where(i => i.Type != "folder").Sum(i => i.Size),
                    FormattedTotalSize = (await FormatFileSizeAsync(allItems.Where(i => i.Type != "folder").Sum(i => i.Size))).Data,
                    Page = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize),
                    HasNextPage = request.PageNumber * request.PageSize < totalItems,
                    HasPreviousPage = request.PageNumber > 1
                };

                return Result<FileBrowseResponseDTO>.Success(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Result<FileBrowseResponseDTO>.Failure("Access denied to the specified path", "ACCESS_DENIED");
            }
            catch (Exception ex)
            {
                return Result<FileBrowseResponseDTO>.Failure($"Failed to browse files: {ex.Message}", "BROWSE_ERROR");
            }
        }

        public async Task<Result<FolderStructureDTO>> GetFolderStructureAsync(string? rootPath = null)
        {
            try
            {
                var safePath = await GetSafePathAsync(rootPath ?? "");
                var fullPath = Path.Combine(_uploadsPath, safePath);

                if (!Directory.Exists(fullPath))
                {
                    return Result<FolderStructureDTO>.Failure($"Directory not found: {rootPath}", "DIRECTORY_NOT_FOUND");
                }

                var structure = await BuildFolderStructureAsync(fullPath, safePath);
                return Result<FolderStructureDTO>.Success(structure);
            }
            catch (UnauthorizedAccessException)
            {
                return Result<FolderStructureDTO>.Failure("Access denied to the specified path", "ACCESS_DENIED");
            }
            catch (Exception ex)
            {
                return Result<FolderStructureDTO>.Failure($"Failed to get folder structure: {ex.Message}", "FOLDER_STRUCTURE_ERROR");
            }
        }

        public async Task<Result<FileItemDTO>> GetFileInfoAsync(string filePath)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Result<FileItemDTO>.Failure("File path is required", "INVALID_INPUT");
            }

            try
            {
                var safePath = await GetSafePathAsync(filePath);
                var fullPath = Path.Combine(_uploadsPath, safePath);

                if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
                {
                    return Result<FileItemDTO>.Failure($"File or directory not found: {filePath}", "FILE_NOT_FOUND");
                }

                FileItemDTO fileItem;
                if (Directory.Exists(fullPath))
                {
                    fileItem = await CreateFolderItem(fullPath, Path.GetDirectoryName(safePath) ?? "");
                }
                else
                {
                    fileItem = await CreateFileItem(fullPath, Path.GetDirectoryName(safePath) ?? "");
                }

                return Result<FileItemDTO>.Success(fileItem);
            }
            catch (UnauthorizedAccessException)
            {
                return Result<FileItemDTO>.Failure("Access denied to the specified path", "ACCESS_DENIED");
            }
            catch (Exception ex)
            {
                return Result<FileItemDTO>.Failure($"Failed to get file info: {ex.Message}", "FILE_INFO_ERROR");
            }
        }

        public async Task<Result<FileUploadResponseDTO>> UploadFilesAsync(IFormFileCollection files, FileUploadRequestDTO request)
        {
            try
            {
                var response = new FileUploadResponseDTO();
                var safeFolderPath = await GetSafePathAsync(request.FolderPath);
                var uploadPath = Path.Combine(_uploadsPath, safeFolderPath);

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var file in files)
                {
                    try
                    {
                        if (!_imageService.IsValidImage(file))
                        {
                            response.Errors.Add($"Invalid file type: {file.FileName}");
                            response.ErrorCount++;
                            continue;
                        }

                        var safeFileNameResult = await GetSafeFileNameAsync(file.FileName);
                        if (!safeFileNameResult.IsSuccess)
                        {
                            response.Errors.Add($"Error processing file name: {file.FileName}");
                            response.ErrorCount++;
                            continue;
                        }

                        var safeFileName = safeFileNameResult.Data;
                        var fileName = request.OverwriteExisting ? safeFileName : 
                            await GetUniqueFileNameAsync(uploadPath, safeFileName);
                        
                        var filePath = Path.Combine(uploadPath, fileName);
                        var relativePath = Path.Combine(safeFolderPath, fileName).Replace("\\", "/");

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Create database record
                        var image = new DecorStore.API.Models.Image
                        {
                            FileName = fileName,
                            FilePath = relativePath,
                            AltText = Path.GetFileNameWithoutExtension(fileName),
                            CreatedAt = DateTime.UtcNow
                        };

                        await _imageRepository.CreateAsync(image);

                        var fileItem = await CreateFileItem(filePath, safeFolderPath);
                        
                        // Generate thumbnail if requested
                        if (request.CreateThumbnails && IsImageFile(fileName))
                        {
                            var thumbnailResult = await GenerateThumbnailAsync(filePath);
                            if (thumbnailResult.IsSuccess)
                            {
                                fileItem.ThumbnailUrl = thumbnailResult.Data;
                            }
                        }

                        response.UploadedFiles.Add(fileItem);
                        response.SuccessCount++;
                        response.TotalSize += file.Length;
                    }
                    catch (Exception ex)
                    {
                        response.Errors.Add($"Error uploading {file.FileName}: {ex.Message}");
                        response.ErrorCount++;
                    }
                }

                var formattedSizeResult = await FormatFileSizeAsync(response.TotalSize);
                response.FormattedTotalSize = formattedSizeResult.IsSuccess ? formattedSizeResult.Data : "Unknown";
                return Result<FileUploadResponseDTO>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<FileUploadResponseDTO>.Failure($"Failed to upload files: {ex.Message}", "UPLOAD_ERROR");
            }
        }

        public async Task<Result<FileItemDTO>> CreateFolderAsync(CreateFolderRequestDTO request)
        {
            try
            {
                var safeParentPath = await GetSafePathAsync(request.ParentPath);
                var safeFolderNameResult = await GetSafeFileNameAsync(request.FolderName);
                
                if (!safeFolderNameResult.IsSuccess)
                {
                    return Result<FileItemDTO>.Failure($"Invalid folder name: {safeFolderNameResult.ErrorMessage}", "INVALID_FOLDER_NAME");
                }
                
                var safeFolderName = safeFolderNameResult.Data;
                var parentPath = Path.Combine(_uploadsPath, safeParentPath);
                var folderPath = Path.Combine(parentPath, safeFolderName);

                if (Directory.Exists(folderPath))
                {
                    return Result<FileItemDTO>.Failure($"Folder '{safeFolderName}' already exists", "FOLDER_EXISTS");
                }

                Directory.CreateDirectory(folderPath);
                var folderItem = await CreateFolderItem(folderPath, safeParentPath);
                return Result<FileItemDTO>.Success(folderItem);
            }
            catch (Exception ex)
            {
                return Result<FileItemDTO>.Failure($"Failed to create folder: {ex.Message}", "CREATE_FOLDER_ERROR");
            }
        }

        public async Task<Result<DeleteFileResponseDTO>> DeleteFilesAsync(DeleteFileRequestDTO request)
        {
            try
            {
                var response = new DeleteFileResponseDTO();

                foreach (var filePath in request.FilePaths)
                {
                    try
                    {
                        var safePath = await GetSafePathAsync(filePath);
                        var fullPath = Path.Combine(_uploadsPath, safePath);

                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            await _imageRepository.DeleteByFilePathAsync(safePath.Replace("\\", "/"));
                            
                            // Delete thumbnail if exists
                            var thumbnailPath = GetThumbnailPath(fullPath);
                            if (File.Exists(thumbnailPath))
                            {
                                File.Delete(thumbnailPath);
                            }
                        }
                        else if (Directory.Exists(fullPath))
                        {
                            Directory.Delete(fullPath, true);
                              // Delete related database records
                            var folderImages = await _imageRepository.GetByFolderAsync(safePath.Replace("\\", "/"));
                            foreach (var image in folderImages)
                            {
                                await _imageRepository.DeleteAsync(image);
                            }
                        }

                        response.DeletedFiles.Add(filePath);
                        response.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        response.Errors.Add($"Error deleting {filePath}: {ex.Message}");
                        response.ErrorCount++;
                    }
                }

                return Result<DeleteFileResponseDTO>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<DeleteFileResponseDTO>.Failure($"Failed to delete files: {ex.Message}", "DELETE_ERROR");
            }
        }

        public async Task<Result<FileOperationResponseDTO>> MoveFilesAsync(MoveFileRequestDTO request)
        {
            try
            {
                var response = new FileOperationResponseDTO { Operation = "Move" };
                var safeDestPath = await GetSafePathAsync(request.DestinationPath);
                var destPath = Path.Combine(_uploadsPath, safeDestPath);

                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }

                foreach (var sourcePath in request.SourcePaths)
                {
                    try
                    {
                        var safeSrcPath = await GetSafePathAsync(sourcePath);
                        var srcPath = Path.Combine(_uploadsPath, safeSrcPath);
                        var fileName = Path.GetFileName(srcPath);
                        var newPath = Path.Combine(destPath, fileName);

                        if (File.Exists(srcPath))
                        {
                            if (!request.OverwriteExisting && File.Exists(newPath))
                            {
                                response.Errors.Add($"File already exists: {fileName}");
                                response.ErrorCount++;
                                continue;
                            }

                            File.Move(srcPath, newPath, request.OverwriteExisting);
                            
                            // Update database record
                            var image = await _imageRepository.GetByFilePathAsync(safeSrcPath.Replace("\\", "/"));
                            if (image != null)
                            {
                                image.FilePath = Path.Combine(safeDestPath, fileName).Replace("\\", "/");
                                await _imageRepository.UpdateAsync(image);
                            }
                        }
                        else if (Directory.Exists(srcPath))
                        {
                            Directory.Move(srcPath, newPath);
                        }

                        response.ProcessedFiles.Add(sourcePath);
                        response.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        response.Errors.Add($"Error moving {sourcePath}: {ex.Message}");
                        response.ErrorCount++;
                    }
                }

                return Result<FileOperationResponseDTO>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<FileOperationResponseDTO>.Failure($"Failed to move files: {ex.Message}", "MOVE_ERROR");
            }
        }

        public async Task<Result<FileOperationResponseDTO>> CopyFilesAsync(CopyFileRequestDTO request)
        {
            try
            {
                var response = new FileOperationResponseDTO { Operation = "Copy" };
                var safeDestPath = await GetSafePathAsync(request.DestinationPath);
                var destPath = Path.Combine(_uploadsPath, safeDestPath);

                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }

                foreach (var sourcePath in request.SourcePaths)
                {
                    try
                    {
                        var safeSrcPath = await GetSafePathAsync(sourcePath);
                        var srcPath = Path.Combine(_uploadsPath, safeSrcPath);
                        var fileName = Path.GetFileName(srcPath);
                        var newPath = Path.Combine(destPath, fileName);

                        if (File.Exists(srcPath))
                        {
                            if (!request.OverwriteExisting && File.Exists(newPath))
                            {
                                newPath = await GetUniqueFileNameAsync(destPath, fileName);
                            }

                            File.Copy(srcPath, newPath, request.OverwriteExisting);
                            
                            // Create new database record
                            var originalImage = await _imageRepository.GetByFilePathAsync(safeSrcPath.Replace("\\", "/"));
                            if (originalImage != null)
                            {
                                var newImage = new DecorStore.API.Models.Image
                                {
                                    FileName = Path.GetFileName(newPath),
                                    FilePath = Path.Combine(safeDestPath, Path.GetFileName(newPath)).Replace("\\", "/"),
                                    AltText = originalImage.AltText,
                                    CreatedAt = DateTime.UtcNow
                                };
                                await _imageRepository.CreateAsync(newImage);
                            }
                        }

                        response.ProcessedFiles.Add(sourcePath);
                        response.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        response.Errors.Add($"Error copying {sourcePath}: {ex.Message}");
                        response.ErrorCount++;
                    }
                }

                return Result<FileOperationResponseDTO>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<FileOperationResponseDTO>.Failure($"Failed to copy files: {ex.Message}", "COPY_ERROR");
            }
        }

        public async Task<Result<bool>> ValidatePathAsync(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return Result<bool>.Success(true);
                
                // Remove any leading or trailing slashes
                path = path.Trim('/');
                
                // Check for invalid characters
                if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                    return Result<bool>.Success(false);
                
                // Check for path traversal attempts
                var normalizedPath = Path.GetFullPath(Path.Combine(_uploadsPath, path));
                
                // Check if the normalized path is within the uploads directory
                var isValid = normalizedPath.StartsWith(_uploadsPath);
                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to validate path: {ex.Message}", "PATH_VALIDATION_ERROR");
            }
        }

        public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadFileAsync(string filePath)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Result<(Stream, string, string)>.Failure("File path cannot be null or empty", "INVALID_INPUT");
            }

            try
            {
                // Remove any leading or trailing slashes
                filePath = filePath.Trim('/');

                // Validate the file path
                var pathValidation = await ValidatePathAsync(filePath);
                if (!pathValidation.IsSuccess || !pathValidation.Data)
                {
                    return Result<(Stream, string, string)>.Failure("Invalid file path", "INVALID_PATH");
                }

                var physicalPath = Path.Combine(_uploadsPath, filePath);

                if (!File.Exists(physicalPath))
                {
                    return Result<(Stream, string, string)>.Failure("File not found", "FILE_NOT_FOUND");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var contentType = GetContentType(physicalPath);
                var fileName = Path.GetFileName(physicalPath);

                return Result<(Stream, string, string)>.Success((memory, contentType, fileName));
            }
            catch (UnauthorizedAccessException)
            {
                return Result<(Stream, string, string)>.Failure("Access denied to the specified file", "ACCESS_DENIED");
            }
            catch (Exception ex)
            {
                return Result<(Stream, string, string)>.Failure($"Failed to download file: {ex.Message}", "DOWNLOAD_ERROR");
            }
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            // Common MIME types, can be expanded
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".zip", "application/zip"}
            };
        }

        public async Task<Result<string>> GetSafeFileNameAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return Result<string>.Success("unnamed_file");

                // Remove invalid characters
                var invalidChars = Path.GetInvalidFileNameChars();
                var safeName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
                
                // Remove multiple consecutive dots and spaces
                safeName = Regex.Replace(safeName, @"\.{2,}", ".");
                safeName = Regex.Replace(safeName, @"\s+", " ");
                
                var result = safeName.Trim();
                return Result<string>.Success(string.IsNullOrEmpty(result) ? "unnamed_file" : result);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Failed to create safe file name: {ex.Message}", "SAFE_NAME_ERROR");
            }
        }
        
        public async Task<Result<string>> FormatFileSizeAsync(long bytes)
        {
            try
            {
                if (bytes < 0)
                    return Result<string>.Failure("File size cannot be negative", "INVALID_SIZE");

                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = bytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return Result<string>.Success($"{len:0.##} {sizes[order]}");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Failed to format file size: {ex.Message}", "FORMAT_SIZE_ERROR");
            }
        }

        public async Task<Result<ImageMetadataDTO>> ExtractImageMetadataAsync(string filePath)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Result<ImageMetadataDTO>.Failure("File path is required", "INVALID_INPUT");
            }

            try
            {
                if (!IsImageFile(filePath))
                {
                    return Result<ImageMetadataDTO>.Failure("File is not a valid image", "INVALID_IMAGE_TYPE");
                }

                if (!File.Exists(filePath))
                {
                    return Result<ImageMetadataDTO>.Failure("File not found", "FILE_NOT_FOUND");
                }

                using var image = System.Drawing.Image.FromFile(filePath);
                var metadata = new ImageMetadataDTO
                {
                    Width = image.Width,
                    Height = image.Height,
                    Format = image.RawFormat.ToString(),
                    AspectRatio = Math.Round((double)image.Width / image.Height, 2),
                    ColorSpace = image.PixelFormat.ToString()
                };

                return Result<ImageMetadataDTO>.Success(metadata);
            }
            catch (Exception ex)
            {
                return Result<ImageMetadataDTO>.Failure($"Failed to extract image metadata: {ex.Message}", "METADATA_EXTRACTION_ERROR");
            }
        }

        public async Task<Result<string>> GenerateThumbnailAsync(string imagePath)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return Result<string>.Failure("Image path is required", "INVALID_INPUT");
            }

            try
            {
                if (!IsImageFile(imagePath))
                {
                    return Result<string>.Failure("File is not a valid image", "INVALID_IMAGE_TYPE");
                }

                if (!File.Exists(imagePath))
                {
                    return Result<string>.Failure("Image file not found", "FILE_NOT_FOUND");
                }

                var fileName = Path.GetFileNameWithoutExtension(imagePath);
                var extension = Path.GetExtension(imagePath);
                var thumbnailFileName = $"{fileName}_thumb{extension}";
                var thumbnailPath = Path.Combine(_thumbnailsPath, thumbnailFileName);

                if (File.Exists(thumbnailPath))
                    return Result<string>.Success($"/.thumbnails/{thumbnailFileName}");

                using var originalImage = System.Drawing.Image.FromFile(imagePath);
                var thumbnailSize = CalculateThumbnailSize(originalImage.Width, originalImage.Height, 150, 150);
                
                using var thumbnail = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);
                using var graphics = Graphics.FromImage(thumbnail);
                
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalImage, 0, 0, thumbnailSize.Width, thumbnailSize.Height);
                
                thumbnail.Save(thumbnailPath, ImageFormat.Jpeg);
                
                return Result<string>.Success($"/.thumbnails/{thumbnailFileName}");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Failed to generate thumbnail: {ex.Message}", "THUMBNAIL_GENERATION_ERROR");
            }
        }

        public async Task<Result<int>> CleanupOrphanedFilesAsync()
        {
            try
            {
                var count = 0;
                var dbImages = await _imageRepository.GetAllAsync();
                
                foreach (var image in dbImages)
                {
                    var fullPath = Path.Combine(_uploadsPath, image.FilePath);
                    if (!File.Exists(fullPath))
                    {
                        await _imageRepository.DeleteAsync(image);
                        count++;
                    }
                }
                
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Failed to cleanup orphaned files: {ex.Message}", "CLEANUP_ERROR");
            }
        }

        public async Task<Result<int>> SyncDatabaseWithFileSystemAsync()
        {
            try
            {
                var count = 0;
                var imageFiles = Directory.GetFiles(_uploadsPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => IsImageFile(f))
                    .ToList();
                
                foreach (var filePath in imageFiles)
                {
                    var relativePath = Path.GetRelativePath(_uploadsPath, filePath).Replace("\\", "/");
                    var existingImage = await _imageRepository.GetByFilePathAsync(relativePath);
                    
                    if (existingImage == null)
                    {
                        var image = new DecorStore.API.Models.Image
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = relativePath,
                            AltText = Path.GetFileNameWithoutExtension(filePath),
                            CreatedAt = File.GetCreationTimeUtc(filePath)
                        };
                        
                        await _imageRepository.CreateAsync(image);
                        count++;
                    }
                }
                
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Failed to sync database with file system: {ex.Message}", "SYNC_ERROR");
            }
        }

        public async Task<Result<List<string>>> GetMissingFilesAsync()
        {
            try
            {
                var missingFiles = new List<string>();
                var dbImages = await _imageRepository.GetAllAsync();
                
                foreach (var image in dbImages)
                {
                    var fullPath = Path.Combine(_uploadsPath, image.FilePath);
                    if (!File.Exists(fullPath))
                    {
                        missingFiles.Add(image.FilePath);
                    }
                }
                
                return Result<List<string>>.Success(missingFiles);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.Failure($"Failed to get missing files: {ex.Message}", "MISSING_FILES_ERROR");
            }
        }

        // Private helper methods
        private async Task<string> GetSafePathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            var normalizedPath = path.Replace("/", "\\").Trim('\\');
            var fullPath = Path.GetFullPath(Path.Combine(_uploadsPath, normalizedPath));
            
            if (!fullPath.StartsWith(_uploadsPath))
                throw new UnauthorizedAccessException("Invalid path");
                
            return normalizedPath;
        }        private async Task<FileItemDTO> CreateFileItem(string fullPath, string relativeFolderPath)
        {
            var fileInfo = new FileInfo(fullPath);
            var fileName = fileInfo.Name;
            var relativePath = Path.Combine(relativeFolderPath, fileName).Replace("\\", "/");
              var formatSizeResult = await FormatFileSizeAsync(fileInfo.Length);
            return new FileItemDTO
            {
                Name = fileName,
                Path = fullPath,
                RelativePath = relativePath,
                Type = IsImageFile(fileName) ? "image" : "file",
                Size = fileInfo.Length,
                FormattedSize = formatSizeResult.IsSuccess ? formatSizeResult.Data : "Unknown",
                CreatedAt = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc,
                Extension = fileInfo.Extension.ToLowerInvariant(),
                FullUrl = $"/uploads/{relativePath}",
                ThumbnailUrl = IsImageFile(fileName) ? GetThumbnailUrl(fullPath) : ""
            };
        }

        private async Task<FileItemDTO> CreateFolderItem(string fullPath, string relativeFolderPath)
        {
            var dirInfo = new DirectoryInfo(fullPath);
            var folderName = dirInfo.Name;
            var relativePath = Path.Combine(relativeFolderPath, folderName).Replace("\\", "/");
            
            var fileCount = dirInfo.GetFiles().Length;
            var folderCount = dirInfo.GetDirectories().Length;
            var totalSize = dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            
            var formatSizeResult = await FormatFileSizeAsync(totalSize);
            
            return new FileItemDTO
            {
                Name = folderName,
                Path = fullPath,
                RelativePath = relativePath,
                Type = "folder",
                Size = totalSize,
                FormattedSize = formatSizeResult.IsSuccess ? formatSizeResult.Data : "Unknown",
                CreatedAt = dirInfo.CreationTimeUtc,
                ModifiedAt = dirInfo.LastWriteTimeUtc,
                Extension = "",
                FullUrl = "",
                ThumbnailUrl = ""
            };
        }

        private async Task<FolderStructureDTO> BuildFolderStructureAsync(string fullPath, string relativePath)
        {
            var dirInfo = new DirectoryInfo(fullPath);
            var subfolders = new List<FolderStructureDTO>();
            
            foreach (var subDir in dirInfo.GetDirectories().Where(d => !d.Name.StartsWith(".")))
            {
                var subRelativePath = Path.Combine(relativePath, subDir.Name).Replace("\\", "/");
                var subfolder = await BuildFolderStructureAsync(subDir.FullName, subRelativePath);
                subfolders.Add(subfolder);
            }
            
            var fileCount = dirInfo.GetFiles().Length;
            var folderCount = dirInfo.GetDirectories().Length;
            var totalSize = dirInfo.GetFiles().Sum(f => f.Length);
            
            return new FolderStructureDTO
            {
                Name = dirInfo.Name,
                Path = fullPath,
                RelativePath = relativePath,
                FileCount = fileCount,
                FolderCount = folderCount,
                TotalItems = fileCount + folderCount,
                TotalSize = totalSize,
                FormattedSize = (await FormatFileSizeAsync(totalSize)).Data,
                Subfolders = subfolders,
                HasChildren = subfolders.Any()
            };
        }

        private async Task<List<FileItemDTO>> ApplyFiltersAsync(List<FileItemDTO> items, FileBrowseRequestDTO request)
        {
            var filtered = items.AsEnumerable();

            // Search filter
            if (!string.IsNullOrEmpty(request.Search))
            {
                filtered = filtered.Where(i => i.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase));
            }

            // File type filter
            if (!string.IsNullOrEmpty(request.FileType) && request.FileType != "all")
            {
                filtered = filtered.Where(i => i.Type == request.FileType);
            }

            // Extension filter
            if (!string.IsNullOrEmpty(request.Extension))
            {
                filtered = filtered.Where(i => i.Extension.Equals(request.Extension, StringComparison.OrdinalIgnoreCase));
            }

            // Size filter
            if (request.MinSize.HasValue)
            {
                filtered = filtered.Where(i => i.Size >= request.MinSize.Value);
            }

            if (request.MaxSize.HasValue)
            {
                filtered = filtered.Where(i => i.Size <= request.MaxSize.Value);
            }

            // Date filter
            if (request.FromDate.HasValue)
            {
                filtered = filtered.Where(i => i.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                filtered = filtered.Where(i => i.CreatedAt <= request.ToDate.Value);
            }

            return filtered.ToList();
        }

        private List<FileItemDTO> ApplySorting(List<FileItemDTO> items, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLowerInvariant() switch
            {
                "name" => isDescending ? items.OrderByDescending(i => i.Name).ToList() : items.OrderBy(i => i.Name).ToList(),
                "size" => isDescending ? items.OrderByDescending(i => i.Size).ToList() : items.OrderBy(i => i.Size).ToList(),
                "date" => isDescending ? items.OrderByDescending(i => i.ModifiedAt).ToList() : items.OrderBy(i => i.ModifiedAt).ToList(),
                "type" => isDescending ? items.OrderByDescending(i => i.Type).ToList() : items.OrderBy(i => i.Type).ToList(),
                _ => items.OrderBy(i => i.Type == "folder" ? 0 : 1).ThenBy(i => i.Name).ToList()
            };
        }

        private string GetParentPath(string currentPath)
        {
            if (string.IsNullOrEmpty(currentPath))
                return "";
                
            var parent = Path.GetDirectoryName(currentPath);
            return parent?.Replace("\\", "/") ?? "";
        }

        private bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _imageExtensions.Contains(extension);
        }

        private string GetThumbnailPath(string imagePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            var extension = Path.GetExtension(imagePath);
            return Path.Combine(_thumbnailsPath, $"{fileName}_thumb{extension}");
        }

        private string GetThumbnailUrl(string imagePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            var extension = Path.GetExtension(imagePath);
            var thumbnailFileName = $"{fileName}_thumb{extension}";
            var thumbnailPath = Path.Combine(_thumbnailsPath, thumbnailFileName);
            
            return File.Exists(thumbnailPath) ? $"/.thumbnails/{thumbnailFileName}" : "";
        }

        private async Task<string> GetUniqueFileNameAsync(string directory, string fileName)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var counter = 1;
            var newFileName = fileName;

            while (File.Exists(Path.Combine(directory, newFileName)))
            {
                newFileName = $"{nameWithoutExt}_{counter}{extension}";
                counter++;
            }

            return newFileName;
        }

        private (int Width, int Height) CalculateThumbnailSize(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
        }
    }
}
