using System;

namespace DecorStore.API.DTOs.FileManagement
{
    public class FileItemDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "folder" | "image" | "file"
        public long Size { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string Extension { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string FullUrl { get; set; } = string.Empty;
        public ImageMetadataDTO? Metadata { get; set; }
        public bool IsSelected { get; set; } = false;
    }

    public class ImageMetadataDTO
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = string.Empty;
        public double AspectRatio { get; set; }
        public string ColorSpace { get; set; } = string.Empty;
    }
}
