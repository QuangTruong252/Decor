using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.Models;

namespace DecorStore.API.Interfaces.Repositories
{    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task<Review> GetByIdAsync(int id);
        Task<Review> GetByCustomerAndProductAsync(int customerId, int productId);
        Task<Review> CreateAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
        Task<float> CalculateAverageRatingAsync(int productId);
        Task UpdateProductAverageRatingAsync(int productId);
    }
}
