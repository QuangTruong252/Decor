using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.DTOs
{
    public class ImageResponseDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ImageUploadResponseDTO
    {
        public List<ImageResponseDTO> Images { get; set; } = new List<ImageResponseDTO>();
    }

    public class ImageUploadDTO
    {
        [Required]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
