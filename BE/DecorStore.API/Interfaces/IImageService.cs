namespace DecorStore.API.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<string> UpdateImageAsync(string oldImageUrl, IFormFile file, string folderName);
        bool IsValidImage(IFormFile file);
    }
}
