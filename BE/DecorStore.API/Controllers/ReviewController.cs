using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/Review/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviewsByProduct(int productId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
                return Ok(reviews);
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
        }

        // GET: api/Review/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDTO>> GetReview(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // GET: api/Review/product/5/rating
        [HttpGet("product/{productId}/rating")]
        public async Task<ActionResult<float>> GetAverageRating(int productId)
        {
            try
            {
                var rating = await _reviewService.GetAverageRatingForProductAsync(productId);
                return Ok(rating);
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/Review
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Review>> CreateReview(CreateReviewDTO reviewDto)
        {
            try
            {
                // Gán UserId từ token vào reviewDto
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                reviewDto.UserId = currentUserId;

                var review = await _reviewService.CreateReviewAsync(reviewDto);
                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
        }

        // PUT: api/Review/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, UpdateReviewDTO reviewDto)
        {
            try
            {
                // Lấy thông tin review hiện tại để kiểm tra quyền
                var existingReview = await _reviewService.GetReviewByIdAsync(id);
                if (existingReview == null)
                {
                    return NotFound();
                }

                // Kiểm tra quyền: chỉ chủ sở hữu review hoặc admin mới được cập nhật
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId != existingReview.UserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _reviewService.UpdateReviewAsync(id, reviewDto);
                return NoContent();
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound();
            }
        }

        // DELETE: api/Review/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                // Lấy thông tin review hiện tại để kiểm tra quyền
                var existingReview = await _reviewService.GetReviewByIdAsync(id);
                if (existingReview == null)
                {
                    return NotFound();
                }

                // Kiểm tra quyền: chỉ chủ sở hữu review hoặc admin mới được xóa
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId != existingReview.UserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _reviewService.DeleteReviewAsync(id);
                return NoContent();
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound();
            }
        }
    }
}