using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Common;

namespace DecorStore.API.Services
{
    public interface IReviewService
    {
        Task<Result<IEnumerable<ReviewDTO>>> GetReviewsByProductIdAsync(int productId);
        Task<Result<ReviewDTO>> GetReviewByIdAsync(int id);
        Task<Result<ReviewDTO>> CreateReviewAsync(CreateReviewDTO reviewDto);
        Task<Result> UpdateReviewAsync(int id, UpdateReviewDTO reviewDto);
        Task<Result> DeleteReviewAsync(int id);
        Task<Result<float>> GetAverageRatingForProductAsync(int productId);
    }
}