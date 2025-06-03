using DecorStore.API.Models;

namespace DecorStore.API.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<string> UpdateImageAsync(string oldImageUrl, IFormFile file, string folderName);
        bool IsValidImage(IFormFile file);
        
        // New methods for enhanced image service
        Task<List<int>> GetOrCreateImagesAsync(List<IFormFile> files, string folderName = "images");
        Task<bool> ImageExistsInSystemAsync(string fileName);
        Task<List<Image>> GetImagesByIdsAsync(List<int> imageIds);
        Task<List<Image>> GetAllImagesAsync();
    }
}
