using DecorStore.API.DTOs;
using System;

namespace DecorStore.API.DTOs.FileManagement
{
    public class FileBrowseRequestDTO : PaginationParameters
    {
        public string Path { get; set; } = string.Empty;
        public string Search { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty; // "all", "image", "folder"
        public string Extension { get; set; } = string.Empty; // ".jpg", ".png", etc.
        public string SortBy { get; set; } = "name"; // "name", "size", "date", "type"
        public string SortOrder { get; set; } = "asc"; // "asc", "desc"
        public long? MinSize { get; set; }
        public long? MaxSize { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class FileBrowseResponseDTO
    {
        public string CurrentPath { get; set; } = string.Empty;
        public string ParentPath { get; set; } = string.Empty;
        public List<FileItemDTO> Items { get; set; } = new List<FileItemDTO>();
        public int TotalItems { get; set; }
        public int TotalFiles { get; set; }
        public int TotalFolders { get; set; }
        public long TotalSize { get; set; }
        public string FormattedTotalSize { get; set; } = string.Empty;
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
