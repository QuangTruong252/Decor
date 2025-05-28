using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Image>> GetAllAsync()
        {
            return await _context.Images
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Image?> GetByIdAsync(int id)
        {
            return await _context.Images
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<Image?> GetByFilePathAsync(string filePath)
        {
            return await _context.Images
                .FirstOrDefaultAsync(i => i.FilePath == filePath && !i.IsDeleted);
        }

        public async Task<IEnumerable<Image>> GetByProductIdAsync(int productId)
        {
            return await _context.Images
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Image>> GetOrphanedImagesAsync()
        {
            return await _context.Images
                .Where(i => !i.IsDeleted && i.ProductId == null)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Image> CreateAsync(Image image)
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<Image> UpdateAsync(Image image)
        {
            _context.Images.Update(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var image = await GetByIdAsync(id);
            if (image == null) return false;

            image.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByFilePathAsync(string filePath)
        {
            var image = await GetByFilePathAsync(filePath);
            if (image == null) return false;

            image.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Images
                .CountAsync(i => !i.IsDeleted);
        }

        public async Task<IEnumerable<Image>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            var query = _context.Images
                .Where(i => !i.IsDeleted);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(i => 
                    i.FileName.Contains(searchTerm) || 
                    i.AltText.Contains(searchTerm) ||
                    i.FilePath.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string filePath)
        {
            return await _context.Images
                .AnyAsync(i => i.FilePath == filePath && !i.IsDeleted);
        }

        public async Task<IEnumerable<Image>> GetByFolderAsync(string folderPath)
        {
            return await _context.Images
                .Where(i => !i.IsDeleted && i.FilePath.StartsWith(folderPath))
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CleanupOrphanedImagesAsync()
        {
            var orphanedImages = await _context.Images
                .Where(i => !i.IsDeleted && i.ProductId == null)
                .ToListAsync();

            foreach (var image in orphanedImages)
            {
                image.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return orphanedImages.Count;
        }
    }
}
