using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.Models;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IImageRepository
    {
        Task<Image?> GetByIdAsync(int id);
        Task<List<Image>> GetManyByIdsAsync(List<int> ids);
        Task<List<Image>> GetByProductIdAsync(int productId);
        Task<List<Image>> GetByCategoryIdAsync(int categoryId);
        Task<List<Image>> GetAllAsync();
        Task<int> GetTotalImageCountAsync();
        Task AddAsync(Image image);
        void Update(Image image);
        void Delete(Image image);        Task AddProductImageAsync(int imageId, int productId);
        Task AddCategoryImageAsync(int imageId, int categoryId);
        void RemoveProductImage(int imageId, int productId);
        void RemoveCategoryImage(int imageId, int categoryId);
        Task<List<ProductImage>> GetProductImagesByProductIdAsync(int productId);
        Task<List<CategoryImage>> GetCategoryImagesByCategoryIdAsync(int categoryId);
        
        // Additional methods needed by services
        Task<Image> CreateAsync(Image image);
        Task<Image> UpdateAsync(Image image);
        Task DeleteAsync(Image image);
        Task<bool> ExistsAsync(int id);
        Task<Image?> GetByFilePathAsync(string filePath);
        Task<List<Image>> GetByFolderAsync(string folderName);
        Task DeleteByFilePathAsync(string filePath);
    }
}
