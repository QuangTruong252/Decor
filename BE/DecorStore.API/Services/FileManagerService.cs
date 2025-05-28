using DecorStore.API.DTOs.FileManagement;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
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

        public async Task<FileBrowseResponseDTO> BrowseFilesAsync(FileBrowseRequestDTO request)
        {
            var safePath = await GetSafePathAsync(request.Path);
            var fullPath = Path.Combine(_uploadsPath, safePath);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {request.Path}");
            }

            var allItems = new List<FileItemDTO>();

            // Get directories
            var directories = Directory.GetDirectories(fullPath)
                .Where(d => !Path.GetFileName(d).StartsWith(".")) // Skip hidden folders
                .Select(d => CreateFolderItem(d, safePath));
            allItems.AddRange(directories);

            // Get files
            var files = Directory.GetFiles(fullPath)
                .Where(f => !Path.GetFileName(f).StartsWith(".")) // Skip hidden files
                .Select(f => CreateFileItem(f, safePath));
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

            return new FileBrowseResponseDTO
            {
                CurrentPath = safePath,
                ParentPath = GetParentPath(safePath),
                Items = paginatedItems,
                TotalItems = totalItems,
                TotalFiles = allItems.Count(i => i.Type != "folder"),
                TotalFolders = allItems.Count(i => i.Type == "folder"),
                TotalSize = allItems.Where(i => i.Type != "folder").Sum(i => i.Size),
                FormattedTotalSize = await FormatFileSizeAsync(allItems.Where(i => i.Type != "folder").Sum(i => i.Size)),
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalItems,
                HasPreviousPage = request.PageNumber > 1
            };
        }

        public async Task<FolderStructureDTO> GetFolderStructureAsync(string? rootPath = null)
        {
            var safePath = await GetSafePathAsync(rootPath ?? "");
            var fullPath = Path.Combine(_uploadsPath, safePath);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {rootPath}");
            }

            return await BuildFolderStructureAsync(fullPath, safePath);
        }

        public async Task<FileItemDTO?> GetFileInfoAsync(string filePath)
        {
            var safePath = await GetSafePathAsync(filePath);
            var fullPath = Path.Combine(_uploadsPath, safePath);

            if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            {
                return null;
            }

            if (Directory.Exists(fullPath))
            {
                return CreateFolderItem(fullPath, Path.GetDirectoryName(safePath) ?? "");
            }

            return CreateFileItem(fullPath, Path.GetDirectoryName(safePath) ?? "");
        }

        public async Task<FileUploadResponseDTO> UploadFilesAsync(IFormFileCollection files, FileUploadRequestDTO request)
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

                    var safeFileName = await GetSafeFileNameAsync(file.FileName);
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

                    var fileItem = CreateFileItem(filePath, safeFolderPath);
                    
                    // Generate thumbnail if requested
                    if (request.CreateThumbnails && IsImageFile(fileName))
                    {
                        fileItem.ThumbnailUrl = await GenerateThumbnailAsync(filePath);
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

            response.FormattedTotalSize = await FormatFileSizeAsync(response.TotalSize);
            return response;
        }

        public async Task<FileItemDTO> CreateFolderAsync(CreateFolderRequestDTO request)
        {
            var safeParentPath = await GetSafePathAsync(request.ParentPath);
            var safeFolderName = await GetSafeFileNameAsync(request.FolderName);
            
            var parentPath = Path.Combine(_uploadsPath, safeParentPath);
            var folderPath = Path.Combine(parentPath, safeFolderName);

            if (Directory.Exists(folderPath))
            {
                throw new InvalidOperationException($"Folder '{safeFolderName}' already exists");
            }

            Directory.CreateDirectory(folderPath);
            return CreateFolderItem(folderPath, safeParentPath);
        }

        public async Task<DeleteFileResponseDTO> DeleteFilesAsync(DeleteFileRequestDTO request)
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
                            await _imageRepository.DeleteAsync(image.Id);
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

            return response;
        }

        public async Task<FileOperationResponseDTO> MoveFilesAsync(MoveFileRequestDTO request)
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

            return response;
        }

        public async Task<FileOperationResponseDTO> CopyFilesAsync(CopyFileRequestDTO request)
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

            return response;
        }

        public async Task<bool> ValidatePathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            // Check for path traversal attempts
            var normalizedPath = Path.GetFullPath(Path.Combine(_uploadsPath, path));
            return normalizedPath.StartsWith(_uploadsPath);
        }

        public async Task<string> GetSafeFileNameAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "unnamed_file";

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            // Remove multiple consecutive dots and spaces
            safeName = Regex.Replace(safeName, @"\.{2,}", ".");
            safeName = Regex.Replace(safeName, @"\s+", " ");
            
            return safeName.Trim();
        }

        public async Task<string> FormatFileSizeAsync(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public async Task<ImageMetadataDTO?> ExtractImageMetadataAsync(string filePath)
        {
            try
            {
                if (!IsImageFile(filePath) || !File.Exists(filePath))
                    return null;

                using var image = System.Drawing.Image.FromFile(filePath);
                return new ImageMetadataDTO
                {
                    Width = image.Width,
                    Height = image.Height,
                    Format = image.RawFormat.ToString(),
                    AspectRatio = Math.Round((double)image.Width / image.Height, 2),
                    ColorSpace = image.PixelFormat.ToString()
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GenerateThumbnailAsync(string imagePath)
        {
            try
            {
                if (!IsImageFile(imagePath) || !File.Exists(imagePath))
                    return string.Empty;

                var fileName = Path.GetFileNameWithoutExtension(imagePath);
                var extension = Path.GetExtension(imagePath);
                var thumbnailFileName = $"{fileName}_thumb{extension}";
                var thumbnailPath = Path.Combine(_thumbnailsPath, thumbnailFileName);

                if (File.Exists(thumbnailPath))
                    return $"/.thumbnails/{thumbnailFileName}";

                using var originalImage = System.Drawing.Image.FromFile(imagePath);
                var thumbnailSize = CalculateThumbnailSize(originalImage.Width, originalImage.Height, 150, 150);
                
                using var thumbnail = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);
                using var graphics = Graphics.FromImage(thumbnail);
                
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalImage, 0, 0, thumbnailSize.Width, thumbnailSize.Height);
                
                thumbnail.Save(thumbnailPath, ImageFormat.Jpeg);
                
                return $"/.thumbnails/{thumbnailFileName}";
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<int> CleanupOrphanedFilesAsync()
        {
            var count = 0;
            var dbImages = await _imageRepository.GetAllAsync();
            
            foreach (var image in dbImages)
            {
                var fullPath = Path.Combine(_uploadsPath, image.FilePath);
                if (!File.Exists(fullPath))
                {
                    await _imageRepository.DeleteAsync(image.Id);
                    count++;
                }
            }
            
            return count;
        }

        public async Task<int> SyncDatabaseWithFileSystemAsync()
        {
            var count = 0;
            var imageFiles = Directory.GetFiles(_uploadsPath, "*.*", SearchOption.AllDirectories)
                .Where(f => IsImageFile(f))
                .ToList();

            foreach (var filePath in imageFiles)
            {
                var relativePath = Path.GetRelativePath(_uploadsPath, filePath).Replace("\\", "/");
                var exists = await _imageRepository.ExistsAsync(relativePath);
                
                if (!exists)
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
            
            return count;
        }

        public async Task<List<string>> GetMissingFilesAsync()
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
            
            return missingFiles;
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
        }

        private FileItemDTO CreateFileItem(string fullPath, string relativeFolderPath)
        {
            var fileInfo = new FileInfo(fullPath);
            var fileName = fileInfo.Name;
            var relativePath = Path.Combine(relativeFolderPath, fileName).Replace("\\", "/");
            
            return new FileItemDTO
            {
                Name = fileName,
                Path = fullPath,
                RelativePath = relativePath,
                Type = IsImageFile(fileName) ? "image" : "file",
                Size = fileInfo.Length,
                FormattedSize = FormatFileSizeAsync(fileInfo.Length).Result,
                CreatedAt = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc,
                Extension = fileInfo.Extension.ToLowerInvariant(),
                FullUrl = $"/uploads/{relativePath}",
                ThumbnailUrl = IsImageFile(fileName) ? GetThumbnailUrl(fullPath) : ""
            };
        }

        private FileItemDTO CreateFolderItem(string fullPath, string relativeFolderPath)
        {
            var dirInfo = new DirectoryInfo(fullPath);
            var folderName = dirInfo.Name;
            var relativePath = Path.Combine(relativeFolderPath, folderName).Replace("\\", "/");
            
            var fileCount = dirInfo.GetFiles().Length;
            var folderCount = dirInfo.GetDirectories().Length;
            var totalSize = dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            
            return new FileItemDTO
            {
                Name = folderName,
                Path = fullPath,
                RelativePath = relativePath,
                Type = "folder",
                Size = totalSize,
                FormattedSize = FormatFileSizeAsync(totalSize).Result,
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
                FormattedSize = await FormatFileSizeAsync(totalSize),
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
