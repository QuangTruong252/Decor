using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDTO>> GetReviewsByProductIdAsync(int productId);
        Task<ReviewDTO> GetReviewByIdAsync(int id);
        Task<Review> CreateReviewAsync(CreateReviewDTO reviewDto);
        Task UpdateReviewAsync(int id, UpdateReviewDTO reviewDto);
        Task DeleteReviewAsync(int id);
        Task<float> GetAverageRatingForProductAsync(int productId);
    }
} 