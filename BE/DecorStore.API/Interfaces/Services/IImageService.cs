using DecorStore.API.Models;
using DecorStore.API.Common;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.Interfaces
{
    public interface IImageService
    {
        Task<Result<string>> UploadImageAsync(IFormFile file, string folderName);
        Task<Result<bool>> DeleteImageAsync(string imageUrl);
        Task<Result<string>> UpdateImageAsync(string oldImageUrl, IFormFile file, string folderName);
        Result<bool> IsValidImage(IFormFile file);
        Task<Result<List<int>>> GetOrCreateImagesAsync(List<IFormFile> files, string folderName = "images");
        Task<Result<bool>> ImageExistsInSystemAsync(string filePath);
        Task<Result<List<Image>>> GetImagesByIdsAsync(List<int> imageIds);
        Task<Result<List<Image>>> GetAllImagesAsync();
        Task<Result<List<Image>>> GetImagesByFilePathsAsync(List<string> filePaths);
        Task<Result> AssignImageToProductAsync(int imageId, int productId);
        Task<Result> AssignImageToCategoryAsync(int imageId, int categoryId);
        Task<Result> UnassignImageFromProductAsync(int imageId, int productId);
        Task<Result> UnassignImageFromCategoryAsync(int imageId, int categoryId);
    }
}
