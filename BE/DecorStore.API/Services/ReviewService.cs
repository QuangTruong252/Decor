using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
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

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByProductIdAsync(int productId)
        {
            // Kiểm tra sản phẩm có tồn tại không
            var productExists = await _unitOfWork.Products.ExistsAsync(productId);
            if (!productExists)
                throw new NotFoundException($"Product with ID {productId} not found");

            var reviews = await _unitOfWork.Reviews.GetByProductIdAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO> GetReviewByIdAsync(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
                return null;

            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<Review> CreateReviewAsync(CreateReviewDTO reviewDto)
        {
            // Kiểm tra sản phẩm có tồn tại không
            var productExists = await _unitOfWork.Products.ExistsAsync(reviewDto.ProductId);
            if (!productExists)
                throw new NotFoundException($"Product with ID {reviewDto.ProductId} not found");

            // Map DTO to entity
            var review = _mapper.Map<Review>(reviewDto);

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Lưu review
                await _unitOfWork.Reviews.CreateAsync(review);

                // Cập nhật average rating
                await _unitOfWork.Reviews.UpdateProductAverageRatingAsync(review.ProductId);

                // Save changes
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                return review;
            }
            catch (Exception)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateReviewAsync(int id, UpdateReviewDTO reviewDto)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
                throw new NotFoundException("Review not found");

            // Map DTO to entity
            _mapper.Map(reviewDto, review);

            // Begin transaction
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
            }
            catch (Exception)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteReviewAsync(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
                throw new NotFoundException("Review not found");

            int productId = review.ProductId;

            // Begin transaction
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
            }
            catch (Exception)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<float> GetAverageRatingForProductAsync(int productId)
        {
            // Kiểm tra sản phẩm có tồn tại không
            var productExists = await _unitOfWork.Products.ExistsAsync(productId);
            if (!productExists)
                throw new NotFoundException($"Product with ID {productId} not found");

            return await _unitOfWork.Reviews.CalculateAverageRatingAsync(productId);
        }


    }
}