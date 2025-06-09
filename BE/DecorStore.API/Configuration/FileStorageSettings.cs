using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.Configuration
{
    public class FileStorageSettings
    {        [Required(ErrorMessage = "Upload path is required")]
        public string UploadPath { get; set; } = "Uploads";

        [Required(ErrorMessage = "Storage path is required")]
        public string StoragePath { get; set; } = "Storage";

        [Required(ErrorMessage = "Thumbnail path is required")]
        public string ThumbnailPath { get; set; } = ".thumbnails";

        [Range(1, 100, ErrorMessage = "Max file size must be between 1 and 100 MB")]
        public int MaxFileSizeMB { get; set; } = 10;        [Required(ErrorMessage = "At least one allowed file extension must be specified")]
        [MinLength(1, ErrorMessage = "At least one allowed file extension must be specified")]
        public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        [Range(50, 2000, ErrorMessage = "Thumbnail width must be between 50 and 2000 pixels")]
        public int ThumbnailWidth { get; set; } = 200;

        [Range(50, 2000, ErrorMessage = "Thumbnail height must be between 50 and 2000 pixels")]
        public int ThumbnailHeight { get; set; } = 200;

        [Range(1, 100, ErrorMessage = "Image quality must be between 1 and 100")]
        public int ImageQuality { get; set; } = 85;

        public bool EnableImageOptimization { get; set; } = true;

        public bool GenerateThumbnails { get; set; } = true;

        [Range(1, 10000, ErrorMessage = "Max files per directory must be between 1 and 10000")]
        public int MaxFilesPerDirectory { get; set; } = 1000;

        public bool UseSubdirectories { get; set; } = true;

        [Required(ErrorMessage = "Base URL is required")]
        public string BaseUrl { get; set; } = "/uploads";

        public string ThumbnailUrl { get; set; } = "/.thumbnails";
    }
}
