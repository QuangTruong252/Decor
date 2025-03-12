using DecorStore.API.Data;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;
        
        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<Review> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        
        public async Task<Review> CreateAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            
            // Update product average rating
            await UpdateProductAverageRatingAsync(review.ProductId);
            
            return review;
        }
        
        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            
            // Update product average rating
            await UpdateProductAverageRatingAsync(review.ProductId);
        }
        
        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                int productId = review.ProductId;
                
                review.IsDeleted = true;
                await _context.SaveChangesAsync();
                
                // Update product average rating
                await UpdateProductAverageRatingAsync(productId);
            }
        }
        
        public async Task<float> CalculateAverageRatingAsync(int productId)
        {
            var ratings = await _context.Reviews
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .Select(r => r.Rating)
                .ToListAsync();
                
            if (ratings.Count == 0)
                return 0;
                
            return (float)ratings.Average();
        }
        
        private async Task UpdateProductAverageRatingAsync(int productId)
        {
            float averageRating = await CalculateAverageRatingAsync(productId);
            
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.AverageRating = averageRating;
                product.UpdatedAt = System.DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
} 