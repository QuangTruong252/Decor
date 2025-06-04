using DecorStore.API.Models;
using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Review> CreateAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            return review;
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Entry(review).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                review.IsDeleted = true;
                await UpdateAsync(review);
            }
        }

        public async Task<float> CalculateAverageRatingAsync(int productId)
        {
            var ratings = await _context.Reviews
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .Select(r => r.Rating)
                .ToListAsync();

            if (!ratings.Any()) return 0;
            return (float)ratings.Average();
        }

        public async Task UpdateProductAverageRatingAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.AverageRating = await CalculateAverageRatingAsync(productId);
                _context.Entry(product).State = EntityState.Modified;
            }
        }
    }
}
