using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Common;
using System;
using DecorStore.API.Interfaces;
using AutoMapper;

namespace DecorStore.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ReviewDTO>>> GetReviewsByProductIdAsync(int productId)
        {
            if (productId <= 0)
            {
                return Result<IEnumerable<ReviewDTO>>.Failure("Product ID must be a positive integer", "INVALID_PRODUCT_ID");
            }

            try
            {
                // Check if product exists
                var productExists = await _unitOfWork.Products.ExistsAsync(productId);
                if (!productExists)
                {
                    return Result<IEnumerable<ReviewDTO>>.Failure($"Product with ID {productId} not found", "PRODUCT_NOT_FOUND");
                }

                var reviews = await _unitOfWork.Reviews.GetByProductIdAsync(productId);
                var reviewDtos = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
                
                return Result<IEnumerable<ReviewDTO>>.Success(reviewDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ReviewDTO>>.Failure($"Error retrieving reviews for product {productId}: {ex.Message}", "RETRIEVE_REVIEWS_ERROR");
            }
        }

        public async Task<Result<ReviewDTO>> GetReviewByIdAsync(int id)
        {
            if (id <= 0)
            {
                return Result<ReviewDTO>.Failure("Review ID must be a positive integer", "INVALID_REVIEW_ID");
            }

            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(id);
                if (review == null)
                {
                    return Result<ReviewDTO>.Failure($"Review with ID {id} not found", "REVIEW_NOT_FOUND");
                }

                var reviewDto = _mapper.Map<ReviewDTO>(review);
                return Result<ReviewDTO>.Success(reviewDto);
            }
            catch (Exception ex)
            {
                return Result<ReviewDTO>.Failure($"Error retrieving review {id}: {ex.Message}", "RETRIEVE_REVIEW_ERROR");
            }
        }

        public async Task<Result<ReviewDTO>> CreateReviewAsync(CreateReviewDTO reviewDto)
        {
            if (reviewDto == null)
            {
                return Result<ReviewDTO>.Failure("Review data cannot be null", "INVALID_INPUT");
            }

            if (reviewDto.ProductId <= 0)
            {
                return Result<ReviewDTO>.Failure("Product ID must be a positive integer", "INVALID_PRODUCT_ID");
            }

            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            {
                return Result<ReviewDTO>.Failure("Rating must be between 1 and 5", "INVALID_RATING");
            }

            if (string.IsNullOrWhiteSpace(reviewDto.Comment) || reviewDto.Comment.Length > 1000)
            {
                return Result<ReviewDTO>.Failure("Comment is required and must be less than 1000 characters", "INVALID_COMMENT");
            }

            try
            {
                return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    // Check if product exists
                    var productExists = await _unitOfWork.Products.ExistsAsync(reviewDto.ProductId);
                    if (!productExists)
                    {
                        return Result<ReviewDTO>.Failure($"Product with ID {reviewDto.ProductId} not found", "PRODUCT_NOT_FOUND");
                    }

                    // Check for duplicate review
                    var existingReview = await _unitOfWork.Reviews.GetByCustomerAndProductAsync(reviewDto.CustomerId, reviewDto.ProductId);
                    if (existingReview != null)
                    {
                        return Result<ReviewDTO>.Failure("Customer has already reviewed this product", "DUPLICATE_REVIEW");
                    }

                    // Map DTO to entity
                    var review = _mapper.Map<Review>(reviewDto);
                    review.CreatedAt = DateTime.UtcNow;

                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Save review
                        await _unitOfWork.Reviews.CreateAsync(review);

                        // Update average rating
                        await _unitOfWork.Reviews.UpdateProductAverageRatingAsync(review.ProductId);

                        // Save changes
                        await _unitOfWork.SaveChangesAsync();

                        // Commit transaction
                        await _unitOfWork.CommitTransactionAsync();

                        var reviewDto = _mapper.Map<ReviewDTO>(review);
                        return Result<ReviewDTO>.Success(reviewDto);
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction on error
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<ReviewDTO>.Failure($"Failed to create review: {ex.Message}", "CREATE_REVIEW_ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result<ReviewDTO>.Failure($"Execution strategy failed: {ex.Message}", "EXECUTION_STRATEGY_ERROR");
            }
        }

        public async Task<Result> UpdateReviewAsync(int id, UpdateReviewDTO reviewDto)
        {
            if (id <= 0)
            {
                return Result.Failure("Review ID must be a positive integer", "INVALID_REVIEW_ID");
            }

            if (reviewDto == null)
            {
                return Result.Failure("Review data cannot be null", "INVALID_INPUT");
            }

            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            {
                return Result.Failure("Rating must be between 1 and 5", "INVALID_RATING");
            }

            if (string.IsNullOrWhiteSpace(reviewDto.Comment) || reviewDto.Comment.Length > 1000)
            {
                return Result.Failure("Comment is required and must be less than 1000 characters", "INVALID_COMMENT");
            }

            try
            {
                return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    var review = await _unitOfWork.Reviews.GetByIdAsync(id);
                    if (review == null)
                    {
                        return Result.Failure($"Review with ID {id} not found", "REVIEW_NOT_FOUND");
                    }

                    // Map DTO to entity
                    _mapper.Map(reviewDto, review);
                    review.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Update review
                        await _unitOfWork.Reviews.UpdateAsync(review);

                        // Update average rating
                        await _unitOfWork.Reviews.UpdateProductAverageRatingAsync(review.ProductId);

                        // Save changes
                        await _unitOfWork.SaveChangesAsync();

                        // Commit transaction
                        await _unitOfWork.CommitTransactionAsync();

                        return Result.Success();
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction on error
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result.Failure($"Failed to update review: {ex.Message}", "UPDATE_REVIEW_ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Execution strategy failed: {ex.Message}", "EXECUTION_STRATEGY_ERROR");
            }
        }

        public async Task<Result> DeleteReviewAsync(int id)
        {
            if (id <= 0)
            {
                return Result.Failure("Review ID must be a positive integer", "INVALID_REVIEW_ID");
            }

            try
            {
                return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    var review = await _unitOfWork.Reviews.GetByIdAsync(id);
                    if (review == null)
                    {
                        return Result.Failure($"Review with ID {id} not found", "REVIEW_NOT_FOUND");
                    }

                    int productId = review.ProductId;

                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Delete review
                        await _unitOfWork.Reviews.DeleteAsync(id);

                        // Update average rating
                        await _unitOfWork.Reviews.UpdateProductAverageRatingAsync(productId);

                        // Save changes
                        await _unitOfWork.SaveChangesAsync();

                        // Commit transaction
                        await _unitOfWork.CommitTransactionAsync();

                        return Result.Success();
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction on error
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result.Failure($"Failed to delete review: {ex.Message}", "DELETE_REVIEW_ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Execution strategy failed: {ex.Message}", "EXECUTION_STRATEGY_ERROR");
            }
        }

        public async Task<Result<float>> GetAverageRatingForProductAsync(int productId)
        {
            if (productId <= 0)
            {
                return Result<float>.Failure("Product ID must be a positive integer", "INVALID_PRODUCT_ID");
            }

            try
            {
                // Check if product exists
                var productExists = await _unitOfWork.Products.ExistsAsync(productId);
                if (!productExists)
                {
                    return Result<float>.Failure($"Product with ID {productId} not found", "PRODUCT_NOT_FOUND");
                }

                var averageRating = await _unitOfWork.Reviews.CalculateAverageRatingAsync(productId);
                return Result<float>.Success(averageRating);
            }
            catch (Exception ex)
            {
                return Result<float>.Failure($"Error calculating average rating for product {productId}: {ex.Message}", "CALCULATE_RATING_ERROR");
            }
        }


    }
}
