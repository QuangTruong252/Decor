using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Data
{
    public class ImagePathUpdater
    {
        private readonly ApplicationDbContext _context;

        public ImagePathUpdater(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateImagePaths()
        {
            try
            {
                Console.WriteLine("Starting image path update process...");

                // Update category image paths
                var categories = await _context.Categories
                    .Where(c => c.ImageUrl != null && c.ImageUrl.StartsWith("/images/"))
                    .ToListAsync();

                int categoryCount = 0;
                foreach (var category in categories)
                {
                    category.ImageUrl = category.ImageUrl.Replace("/images/", "/");
                    categoryCount++;
                }

                // Update product image paths
                var images = await _context.Images
                    .Where(i => i.FilePath != null && i.FilePath.StartsWith("/images/"))
                    .ToListAsync();

                int imageCount = 0;
                foreach (var image in images)
                {
                    image.FilePath = image.FilePath.Replace("/images/", "/");
                    imageCount++;
                }

                // Save changes
                await _context.SaveChangesAsync();

                Console.WriteLine($"Image path update completed successfully. Updated {categoryCount} categories and {imageCount} product images.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating image paths: {ex.Message}");
                throw;
            }
        }
    }
}
