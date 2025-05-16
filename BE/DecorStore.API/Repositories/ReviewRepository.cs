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
            return review;
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
        }

        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsDeleted = true;
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

        public async Task UpdateProductAverageRatingAsync(int productId)
        {
            float averageRating = await CalculateAverageRatingAsync(productId);

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.AverageRating = averageRating;
                product.UpdatedAt = System.DateTime.UtcNow;
            }
        }
    }
}