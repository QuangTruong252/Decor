using DecorStore.API.Models;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IImageRepository
    {
        Task<IEnumerable<Image>> GetAllAsync();
        Task<Image?> GetByIdAsync(int id);
        Task<Image?> GetByFilePathAsync(string filePath);
        Task<IEnumerable<Image>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Image>> GetOrphanedImagesAsync();
        Task<Image> CreateAsync(Image image);
        Task<Image> UpdateAsync(Image image);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByFilePathAsync(string filePath);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Image>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);
        Task<bool> ExistsAsync(string filePath);
        Task<IEnumerable<Image>> GetByFolderAsync(string folderPath);
        Task<int> CleanupOrphanedImagesAsync();
    }
}
