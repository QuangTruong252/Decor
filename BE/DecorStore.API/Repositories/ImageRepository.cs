using DecorStore.API.Models;
using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace DecorStore.API.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Image?> GetByIdAsync(int id)
        {
            return await _context.Images
                .Include(i => i.ProductImages)
                .ThenInclude(pi => pi.Product)
                .Include(i => i.CategoryImages)
                .ThenInclude(ci => ci.Category)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Image>> GetManyByIdsAsync(List<int> ids)
        {
            return await _context.Images
                .Include(i => i.ProductImages)
                .ThenInclude(pi => pi.Product)
                .Include(i => i.CategoryImages)
                .ThenInclude(ci => ci.Category)
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();
        }

        public async Task<List<Image>> GetByProductIdAsync(int productId)
        {
            return await _context.Images
                .Include(i => i.ProductImages)
                .Where(i => i.ProductImages.Any(pi => pi.ProductId == productId))
                .ToListAsync();
        }

        public async Task<List<Image>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Images
                .Include(i => i.CategoryImages)
                .Where(i => i.CategoryImages.Any(ci => ci.CategoryId == categoryId))
                .ToListAsync();
        }

        public async Task AddAsync(Image image)
        {
            await _context.Images.AddAsync(image);
        }

        public void Update(Image image)
        {
            _context.Entry(image).State = EntityState.Modified;
        }

        public void Delete(Image image)
        {
            _context.Images.Remove(image);
        }

        public async Task<int> GetTotalImageCountAsync()
        {
            return await _context.Images.CountAsync();
        }

        public async Task<List<Image>> GetAllAsync()
        {
            return await _context.Images
                .Include(i => i.ProductImages)
                .ThenInclude(pi => pi.Product)
                .Include(i => i.CategoryImages)
                .ThenInclude(ci => ci.Category)
                .ToListAsync();
        }

        public async Task AddProductImageAsync(int imageId, int productId)
        {
            var productImage = new ProductImage { ImageId = imageId, ProductId = productId };
            await _context.ProductImages.AddAsync(productImage);
        }

        public async Task AddCategoryImageAsync(int imageId, int categoryId)
        {
            var categoryImage = new CategoryImage { ImageId = imageId, CategoryId = categoryId };
            await _context.CategoryImages.AddAsync(categoryImage);
        }

        public void RemoveProductImage(int imageId, int productId)
        {
            var productImage = _context.ProductImages
                .FirstOrDefault(pi => pi.ImageId == imageId && pi.ProductId == productId);
            if (productImage != null)
            {
                _context.ProductImages.Remove(productImage);
            }
        }

        public void RemoveCategoryImage(int imageId, int categoryId)
        {
            var categoryImage = _context.CategoryImages
                .FirstOrDefault(ci => ci.ImageId == imageId && ci.CategoryId == categoryId);
            if (categoryImage != null)
            {
                _context.CategoryImages.Remove(categoryImage);
            }
        }

        // Additional methods implementation
        public async Task<Image> CreateAsync(Image image)
        {
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<Image> UpdateAsync(Image image)
        {
            _context.Entry(image).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task DeleteAsync(Image image)
        {
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Images.AnyAsync(i => i.Id == id);
        }

        public async Task<Image?> GetByFilePathAsync(string filePath)
        {
            return await _context.Images
                .Include(i => i.ProductImages)
                .Include(i => i.CategoryImages)
                .FirstOrDefaultAsync(i => i.FilePath == filePath);
        }

        public async Task<List<Image>> GetByFilePathsAsync(List<string> filePaths)
        {
            return await _context.Images
                 .Include(i => i.ProductImages)
                 .ThenInclude(pi => pi.Product)
                 .Include(i => i.CategoryImages)
                 .ThenInclude(ci => ci.Category)
                .Where(i => filePaths.Contains(i.FilePath))
                .ToListAsync();
        }

        public async Task<List<Image>> GetByFolderAsync(string folderName)
        {
            return await _context.Images
                .Include(i => i.ProductImages)
                .Include(i => i.CategoryImages)
                .Where(i => i.FilePath.Contains(folderName))
                .ToListAsync();
        }

        public async Task DeleteByFilePathAsync(string filePath)
        {
            var image = await _context.Images.FirstOrDefaultAsync(i => i.FilePath == filePath);
            if (image != null)
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ProductImage>> GetProductImagesByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .Include(pi => pi.Image)
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
        }

        public async Task<List<CategoryImage>> GetCategoryImagesByCategoryIdAsync(int categoryId)
        {
            return await _context.CategoryImages
                .Include(ci => ci.Image)
                .Where(ci => ci.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}
