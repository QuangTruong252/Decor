using DecorStore.API.DTOs.FileManagement;

namespace DecorStore.API.Interfaces.Services
{
    public interface IFileManagerService
    {
        // Browse and listing
        Task<FileBrowseResponseDTO> BrowseFilesAsync(FileBrowseRequestDTO request);
        Task<FolderStructureDTO> GetFolderStructureAsync(string? rootPath = null);
        Task<FileItemDTO?> GetFileInfoAsync(string filePath);
        Task<(Stream FileStream, string ContentType, string FileName)> DownloadFileAsync(string filePath); // Added for download functionality
        
        // File operations
        Task<FileUploadResponseDTO> UploadFilesAsync(IFormFileCollection files, FileUploadRequestDTO request);
        Task<FileItemDTO> CreateFolderAsync(CreateFolderRequestDTO request);
        Task<DeleteFileResponseDTO> DeleteFilesAsync(DeleteFileRequestDTO request);
        Task<FileOperationResponseDTO> MoveFilesAsync(MoveFileRequestDTO request);
        Task<FileOperationResponseDTO> CopyFilesAsync(CopyFileRequestDTO request);
        
        // Utility methods
        Task<bool> ValidatePathAsync(string path);
        Task<string> GetSafeFileNameAsync(string fileName);
        Task<string> FormatFileSizeAsync(long bytes);
        Task<ImageMetadataDTO?> ExtractImageMetadataAsync(string filePath);
        Task<string> GenerateThumbnailAsync(string imagePath);
        
        // Cleanup and maintenance
        Task<int> CleanupOrphanedFilesAsync();
        Task<int> SyncDatabaseWithFileSystemAsync();
        Task<List<string>> GetMissingFilesAsync();
    }
}
