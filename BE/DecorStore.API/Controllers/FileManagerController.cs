using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DecorStore.API.Controllers.Base;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.DTOs.FileManagement;

namespace DecorStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class FileManagerController : BaseController
    {
        private readonly IFileManagerService _fileManagerService;

        public FileManagerController(
            IFileManagerService fileManagerService,
            ILogger<FileManagerController> logger) : base(logger)
        {
            _fileManagerService = fileManagerService;
        }        [HttpPost("browse")]
        public async Task<IActionResult> BrowseFiles([FromBody] FileBrowseRequestDTO request)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualRequest = await TryManualDeserializationAsync(request, _logger);

            var modelValidation = ValidateModelState();
            if (modelValidation != null)
                return modelValidation;

            var result = await _fileManagerService.BrowseFilesAsync(actualRequest);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet("folder-structure")]
        public async Task<ActionResult<FolderStructureDTO>> GetFolderStructure([FromQuery] string? rootPath = null)
        {
            var result = await _fileManagerService.GetFolderStructureAsync(rootPath);
            return HandleResult(result);
        }

        [HttpGet("file-info")]
        public async Task<ActionResult<FileItemDTO>> GetFileInfo([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("File path is required");

            var result = await _fileManagerService.GetFileInfoAsync(filePath);
            var actionResult = HandleResult(result);
            return actionResult.Result ?? actionResult;
        }

        [HttpGet("download")]
        public async Task<ActionResult<(Stream FileStream, string ContentType, string FileName)>> DownloadFile([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("File path is required");

            var result = await _fileManagerService.DownloadFileAsync(filePath);

            if (!result.IsSuccess)
                return HandleResult(result);

            var (fileStream, contentType, fileName) = result.Data;
            return File(fileStream, contentType, fileName);
        }        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] IFormFileCollection files, [FromForm] FileUploadRequestDTO request)
        {
            if (files == null || !files.Any())
                return BadRequest("No files provided");

            var modelValidation = ValidateModelState();            if (modelValidation != null)
                return modelValidation;            var result = await _fileManagerService.UploadFilesAsync(files, request);
            if (result.IsSuccess)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }        [HttpPost("create-folder")]
        public async Task<ActionResult<FileItemDTO>> CreateFolder([FromBody] CreateFolderRequestDTO request)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualRequest = await TryManualDeserializationAsync(request, _logger);

            var modelValidation = ValidateModelState();
            if (modelValidation != null)
                return BadRequest(modelValidation);

            var result = await _fileManagerService.CreateFolderAsync(actualRequest);
            return HandleCreateResult(result);
        }        [HttpDelete("delete")]
        public async Task<ActionResult<DeleteFileResponseDTO>> DeleteFiles([FromBody] DeleteFileRequestDTO request)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualRequest = await TryManualDeserializationAsync(request, _logger);

            var modelValidation = ValidateModelState();
            if (modelValidation != null)
                return BadRequest(modelValidation);

            var result = await _fileManagerService.DeleteFilesAsync(actualRequest);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }        [HttpPost("move")]
        public async Task<ActionResult<FileOperationResponseDTO>> MoveFiles([FromBody] MoveFileRequestDTO request)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualRequest = await TryManualDeserializationAsync(request, _logger);

            var modelValidation = ValidateModelState();
            if (modelValidation != null)
                return BadRequest(modelValidation);

            var result = await _fileManagerService.MoveFilesAsync(actualRequest);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }        [HttpPost("copy")]
        public async Task<ActionResult<FileOperationResponseDTO>> CopyFiles([FromBody] CopyFileRequestDTO request)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualRequest = await TryManualDeserializationAsync(request, _logger);

            var modelValidation = ValidateModelState();
            if (modelValidation != null)
                return BadRequest(modelValidation);

            var result = await _fileManagerService.CopyFilesAsync(actualRequest);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }        [HttpGet("validate-path")]
        public async Task<ActionResult<bool>> ValidatePath([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest("Path is required");

            var result = await _fileManagerService.ValidatePathAsync(path);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet("image-metadata")]
        public async Task<ActionResult<ImageMetadataDTO>> ExtractImageMetadata([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("File path is required");

            var result = await _fileManagerService.ExtractImageMetadataAsync(filePath);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPost("generate-thumbnail")]
        public async Task<IActionResult> GenerateThumbnail([FromQuery] string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return BadRequest("Image path is required");

            var result = await _fileManagerService.GenerateThumbnailAsync(imagePath);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPost("cleanup-orphaned")]
        public async Task<ActionResult<int>> CleanupOrphanedFiles()
        {
            var result = await _fileManagerService.CleanupOrphanedFilesAsync();
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPost("sync-database")]
        public async Task<ActionResult<int>> SyncDatabaseWithFileSystem()
        {
            var result = await _fileManagerService.SyncDatabaseWithFileSystemAsync();
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet("missing-files")]
        public async Task<ActionResult<List<string>>> GetMissingFiles()
        {
            var result = await _fileManagerService.GetMissingFilesAsync();
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }
    }
}
