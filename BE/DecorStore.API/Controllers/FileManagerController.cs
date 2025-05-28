using DecorStore.API.DTOs.FileManagement;
using DecorStore.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]// Require authentication for all endpoints
    public class FileManagerController : ControllerBase
    {
        private readonly IFileManagerService _fileManagerService;
        private readonly ILogger<FileManagerController> _logger;

        public FileManagerController(
            IFileManagerService fileManagerService,
            ILogger<FileManagerController> logger)
        {
            _fileManagerService = fileManagerService;
            _logger = logger;
        }

        /// <summary>
        /// Browse files and folders with pagination, search, and filtering
        /// </summary>
        [HttpGet("browse")]
        public async Task<ActionResult<FileBrowseResponseDTO>> BrowseFiles([FromQuery] FileBrowseRequestDTO request)
        {
            try
            {
                if (!await _fileManagerService.ValidatePathAsync(request.Path))
                {
                    return BadRequest("Invalid path");
                }

                var result = await _fileManagerService.BrowseFilesAsync(request);
                return Ok(result);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogWarning("Directory not found: {Path}", request.Path);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt: {Path}", request.Path);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing files in path: {Path}", request.Path);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get folder structure tree
        /// </summary>
        [HttpGet("folders")]
        public async Task<ActionResult<FolderStructureDTO>> GetFolderStructure([FromQuery] string? rootPath = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(rootPath) && !await _fileManagerService.ValidatePathAsync(rootPath))
                {
                    return BadRequest("Invalid path");
                }

                var result = await _fileManagerService.GetFolderStructureAsync(rootPath);
                return Ok(result);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogWarning("Directory not found: {Path}", rootPath);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting folder structure for path: {Path}", rootPath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get detailed information about a specific file or folder
        /// </summary>
        [HttpGet("info")]
        public async Task<ActionResult<FileItemDTO>> GetFileInfo([FromQuery] string filePath)
        {
            try
            {
                if (!await _fileManagerService.ValidatePathAsync(filePath))
                {
                    return BadRequest("Invalid path");
                }

                var result = await _fileManagerService.GetFileInfoAsync(filePath);
                if (result == null)
                {
                    return NotFound("File or folder not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info for: {Path}", filePath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Upload multiple files to a specified folder
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<FileUploadResponseDTO>> UploadFiles(
            [FromForm] FileUploadRequestDTO request,
            [FromForm] IFormFileCollection files)
        {
            try
            {
                if (!files.Any())
                {
                    return BadRequest("No files provided");
                }

                if (!await _fileManagerService.ValidatePathAsync(request.FolderPath))
                {
                    return BadRequest("Invalid folder path");
                }

                var result = await _fileManagerService.UploadFilesAsync(files, request);
                
                if (result.ErrorCount > 0 && result.SuccessCount == 0)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading files to: {Path}", request.FolderPath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new folder
        /// </summary>
        [HttpPost("create-folder")]
        public async Task<ActionResult<FileItemDTO>> CreateFolder([FromBody] CreateFolderRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FolderName))
                {
                    return BadRequest("Folder name is required");
                }

                if (!await _fileManagerService.ValidatePathAsync(request.ParentPath))
                {
                    return BadRequest("Invalid parent path");
                }

                var result = await _fileManagerService.CreateFolderAsync(request);
                return CreatedAtAction(nameof(GetFileInfo), new { filePath = result.RelativePath }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder: {FolderName} in {ParentPath}", 
                    request.FolderName, request.ParentPath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete multiple files or folders
        /// </summary>
        [HttpDelete("delete")]
        public async Task<ActionResult<DeleteFileResponseDTO>> DeleteFiles([FromBody] DeleteFileRequestDTO request)
        {
            try
            {
                if (!request.FilePaths.Any())
                {
                    return BadRequest("No file paths provided");
                }

                // Validate all paths
                foreach (var path in request.FilePaths)
                {
                    if (!await _fileManagerService.ValidatePathAsync(path))
                    {
                        return BadRequest($"Invalid path: {path}");
                    }
                }

                var result = await _fileManagerService.DeleteFilesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting files: {Paths}", string.Join(", ", request.FilePaths));
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Move files or folders to a different location
        /// </summary>
        [HttpPost("move")]
        public async Task<ActionResult<FileOperationResponseDTO>> MoveFiles([FromBody] MoveFileRequestDTO request)
        {
            try
            {
                if (!request.SourcePaths.Any())
                {
                    return BadRequest("No source paths provided");
                }

                if (!await _fileManagerService.ValidatePathAsync(request.DestinationPath))
                {
                    return BadRequest("Invalid destination path");
                }

                // Validate all source paths
                foreach (var path in request.SourcePaths)
                {
                    if (!await _fileManagerService.ValidatePathAsync(path))
                    {
                        return BadRequest($"Invalid source path: {path}");
                    }
                }

                var result = await _fileManagerService.MoveFilesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving files from {Sources} to {Destination}", 
                    string.Join(", ", request.SourcePaths), request.DestinationPath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Copy files or folders to a different location
        /// </summary>
        [HttpPost("copy")]
        public async Task<ActionResult<FileOperationResponseDTO>> CopyFiles([FromBody] CopyFileRequestDTO request)
        {
            try
            {
                if (!request.SourcePaths.Any())
                {
                    return BadRequest("No source paths provided");
                }

                if (!await _fileManagerService.ValidatePathAsync(request.DestinationPath))
                {
                    return BadRequest("Invalid destination path");
                }

                // Validate all source paths
                foreach (var path in request.SourcePaths)
                {
                    if (!await _fileManagerService.ValidatePathAsync(path))
                    {
                        return BadRequest($"Invalid source path: {path}");
                    }
                }

                var result = await _fileManagerService.CopyFilesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying files from {Sources} to {Destination}", 
                    string.Join(", ", request.SourcePaths), request.DestinationPath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Generate thumbnail for an image
        /// </summary>
        [HttpPost("generate-thumbnail")]
        public async Task<ActionResult<string>> GenerateThumbnail([FromBody] string imagePath)
        {
            try
            {
                if (!await _fileManagerService.ValidatePathAsync(imagePath))
                {
                    return BadRequest("Invalid image path");
                }

                var thumbnailUrl = await _fileManagerService.GenerateThumbnailAsync(imagePath);
                if (string.IsNullOrEmpty(thumbnailUrl))
                {
                    return BadRequest("Unable to generate thumbnail for this file");
                }

                return Ok(new { thumbnailUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail for: {Path}", imagePath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get image metadata
        /// </summary>
        [HttpGet("metadata")]
        public async Task<ActionResult<ImageMetadataDTO>> GetImageMetadata([FromQuery] string imagePath)
        {
            try
            {
                if (!await _fileManagerService.ValidatePathAsync(imagePath))
                {
                    return BadRequest("Invalid image path");
                }

                var metadata = await _fileManagerService.ExtractImageMetadataAsync(imagePath);
                if (metadata == null)
                {
                    return BadRequest("Unable to extract metadata from this file");
                }

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata for: {Path}", imagePath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cleanup orphaned files (files in database but not on disk)
        /// </summary>
        [HttpPost("cleanup-orphaned")]
        public async Task<ActionResult<int>> CleanupOrphanedFiles()
        {
            try
            {
                var count = await _fileManagerService.CleanupOrphanedFilesAsync();
                return Ok(new { cleanedCount = count, message = $"Cleaned up {count} orphaned file records" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up orphaned files");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Sync database with file system (add missing files to database)
        /// </summary>
        [HttpPost("sync-database")]
        public async Task<ActionResult<int>> SyncDatabase()
        {
            try
            {
                var count = await _fileManagerService.SyncDatabaseWithFileSystemAsync();
                return Ok(new { syncedCount = count, message = $"Synced {count} files to database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing database with file system");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get list of missing files (files in database but not on disk)
        /// </summary>
        [HttpGet("missing-files")]
        public async Task<ActionResult<List<string>>> GetMissingFiles()
        {
            try
            {
                var missingFiles = await _fileManagerService.GetMissingFilesAsync();
                return Ok(new { missingFiles, count = missingFiles.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting missing files list");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult> HealthCheck()
        {
            try
            {
                // Basic validation that the uploads directory exists
                var folderStructure = await _fileManagerService.GetFolderStructureAsync();
                return Ok(new { status = "healthy", uploadsPath = folderStructure.Path });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File manager health check failed");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }
    }
}
