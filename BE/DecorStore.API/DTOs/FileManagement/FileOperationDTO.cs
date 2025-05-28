using System.Collections.Generic;

namespace DecorStore.API.DTOs.FileManagement
{
    public class FileUploadRequestDTO
    {
        public string FolderPath { get; set; } = string.Empty;
        public bool CreateThumbnails { get; set; } = true;
        public bool OverwriteExisting { get; set; } = false;
    }

    public class FileUploadResponseDTO
    {
        public List<FileItemDTO> UploadedFiles { get; set; } = new List<FileItemDTO>();
        public List<string> Errors { get; set; } = new List<string>();
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public long TotalSize { get; set; }
        public string FormattedTotalSize { get; set; } = string.Empty;
    }

    public class CreateFolderRequestDTO
    {
        public string ParentPath { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
    }

    public class DeleteFileRequestDTO
    {
        public List<string> FilePaths { get; set; } = new List<string>();
        public bool Permanent { get; set; } = false;
    }

    public class DeleteFileResponseDTO
    {
        public List<string> DeletedFiles { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
    }

    public class MoveFileRequestDTO
    {
        public List<string> SourcePaths { get; set; } = new List<string>();
        public string DestinationPath { get; set; } = string.Empty;
        public bool OverwriteExisting { get; set; } = false;
    }

    public class CopyFileRequestDTO
    {
        public List<string> SourcePaths { get; set; } = new List<string>();
        public string DestinationPath { get; set; } = string.Empty;
        public bool OverwriteExisting { get; set; } = false;
    }

    public class FileOperationResponseDTO
    {
        public List<string> ProcessedFiles { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public string Operation { get; set; } = string.Empty;
    }
}
