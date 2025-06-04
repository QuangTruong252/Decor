using DecorStore.API.Models;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<string> UpdateImageAsync(string oldImageUrl, IFormFile file, string folderName);
        bool IsValidImage(IFormFile file);
        Task<List<int>> GetOrCreateImagesAsync(List<IFormFile> files, string folderName = "images");
        Task<bool> ImageExistsInSystemAsync(string filePath);
        Task<List<Image>> GetImagesByIdsAsync(List<int> imageIds);
        Task<List<Image>> GetAllImagesAsync();
        Task AssignImageToProductAsync(int imageId, int productId);
        Task AssignImageToCategoryAsync(int imageId, int categoryId);
        void UnassignImageFromProduct(int imageId, int productId);
        void UnassignImageFromCategory(int imageId, int categoryId);
    }
}
