using DecorStore.API.DTOs.FileManagement;
using DecorStore.API.Common;

namespace DecorStore.API.Interfaces.Services
{
    public interface IFileManagerService
    {
        // Browse and listing
        Task<Result<FileBrowseResponseDTO>> BrowseFilesAsync(FileBrowseRequestDTO request);
        Task<Result<FolderStructureDTO>> GetFolderStructureAsync(string? rootPath = null);
        Task<Result<FileItemDTO>> GetFileInfoAsync(string filePath);
        Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadFileAsync(string filePath);
        
        // File operations
        Task<Result<FileUploadResponseDTO>> UploadFilesAsync(IFormFileCollection files, FileUploadRequestDTO request);
        Task<Result<FileItemDTO>> CreateFolderAsync(CreateFolderRequestDTO request);
        Task<Result<DeleteFileResponseDTO>> DeleteFilesAsync(DeleteFileRequestDTO request);
        Task<Result<FileOperationResponseDTO>> MoveFilesAsync(MoveFileRequestDTO request);
        Task<Result<FileOperationResponseDTO>> CopyFilesAsync(CopyFileRequestDTO request);
        
        // Utility methods
        Task<Result<bool>> ValidatePathAsync(string path);
        Task<Result<string>> GetSafeFileNameAsync(string fileName);
        Task<Result<string>> FormatFileSizeAsync(long bytes);
        Task<Result<ImageMetadataDTO>> ExtractImageMetadataAsync(string filePath);
        Task<Result<string>> GenerateThumbnailAsync(string imagePath);
        
        // Cleanup and maintenance
        Task<Result<int>> CleanupOrphanedFilesAsync();
        Task<Result<int>> SyncDatabaseWithFileSystemAsync();
        Task<Result<List<string>>> GetMissingFilesAsync();
    }
}
