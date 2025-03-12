using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Repositories;
using DecorStore.API.Exceptions;
using System;

namespace DecorStore.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;

        public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByProductIdAsync(int productId)
        {
            // Kiểm tra sản phẩm có tồn tại không
            var productExists = await _productRepository.ExistsAsync(productId);
            if (!productExists)
                throw new NotFoundException($"Product with ID {productId} not found");
                
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);
            return reviews.Select(MapReviewToDto);
        }

        public async Task<ReviewDTO> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return null;
                
            return MapReviewToDto(review);
        }

        public async Task<Review> CreateReviewAsync(CreateReviewDTO reviewDto)
        {
            // Kiểm tra sản phẩm có tồn tại không
            var productExists = await _productRepository.ExistsAsync(reviewDto.ProductId);
            if (!productExists)
                throw new NotFoundException($"Product with ID {reviewDto.ProductId} not found");
                
            var review = new Review
            {
                UserId = reviewDto.UserId,
                ProductId = reviewDto.ProductId,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            
            // Lưu review và cập nhật average rating
            return await _reviewRepository.CreateAsync(review);
        }

        public async Task UpdateReviewAsync(int id, UpdateReviewDTO reviewDto)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                throw new NotFoundException("Review not found");
                
            review.Rating = reviewDto.Rating;
            review.Comment = reviewDto.Comment ?? string.Empty;
            
            await _reviewRepository.UpdateAsync(review);
        }

        public async Task DeleteReviewAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                throw new NotFoundException("Review not found");
                
            await _reviewRepository.DeleteAsync(id);
        }

        public async Task<float> GetAverageRatingForProductAsync(int productId)
        {
            // Kiểm tra sản phẩm có tồn tại không
            var productExists = await _productRepository.ExistsAsync(productId);
            if (!productExists)
                throw new NotFoundException($"Product with ID {productId} not found");
                
            return await _reviewRepository.CalculateAverageRatingAsync(productId);
        }

        private ReviewDTO MapReviewToDto(Review review)
        {
            if (review == null)
                return null;
                
            return new ReviewDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = review.User?.FullName ?? review.User?.Username ?? "Anonymous",
                ProductId = review.ProductId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
} 