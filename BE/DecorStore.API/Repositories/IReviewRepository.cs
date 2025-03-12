using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task<Review> GetByIdAsync(int id);
        Task<Review> CreateAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
        Task<float> CalculateAverageRatingAsync(int productId);
    }
} 