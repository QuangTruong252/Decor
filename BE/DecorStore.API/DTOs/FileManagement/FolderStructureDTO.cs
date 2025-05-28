using System.Collections.Generic;

namespace DecorStore.API.DTOs.FileManagement
{
    public class FolderStructureDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public int FolderCount { get; set; }
        public int TotalItems { get; set; }
        public long TotalSize { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public List<FolderStructureDTO> Subfolders { get; set; } = new List<FolderStructureDTO>();
        public bool IsExpanded { get; set; } = false;
        public bool HasChildren { get; set; } = false;
    }
}
