using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Services;
using DecorStore.API.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger) 
            : base(logger)
        {
            _reviewService = reviewService;
        }

        // GET: api/Review/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviewsByProduct(int productId)
        {
            var reviewsResult = await _reviewService.GetReviewsByProductIdAsync(productId);
            return HandleResult(reviewsResult);
        }

        // GET: api/Review/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDTO>> GetReview(int id)
        {
            var reviewResult = await _reviewService.GetReviewByIdAsync(id);
            return HandleResult(reviewResult);
        }

        // GET: api/Review/product/5/rating
        [HttpGet("product/{productId}/rating")]
        public async Task<ActionResult<float>> GetAverageRating(int productId)
        {
            var ratingResult = await _reviewService.GetAverageRatingForProductAsync(productId);
            return HandleResult(ratingResult);
        }

        // POST: api/Review
        [HttpPost]
        [Authorize]        public async Task<ActionResult<ReviewDTO>> CreateReview(CreateReviewDTO reviewDto)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Assign UserId from token to reviewDto
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user authentication");
            }

            reviewDto.CustomerId = currentUserId;

            var reviewResult = await _reviewService.CreateReviewAsync(reviewDto);
            return HandleCreateResult(reviewResult);
        }

        // PUT: api/Review/5
        [HttpPut("{id}")]
        [Authorize]        public async Task<ActionResult<ReviewDTO>> UpdateReview(int id, UpdateReviewDTO reviewDto)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Get existing review to check permissions
            var existingReviewResult = await _reviewService.GetReviewByIdAsync(id);
            if (existingReviewResult.IsFailure)
            {
                return HandleResult(existingReviewResult);
            }

            // Check permissions: only review owner or admin can update
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user authentication");
            }

            if (currentUserId != existingReviewResult.Data!.CustomerId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only update your own reviews");
            }

            var updateResult = await _reviewService.UpdateReviewAsync(id, reviewDto);
            return HandleResult(updateResult);
        }

        // DELETE: api/Review/5        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ReviewDTO>> DeleteReview(int id)
        {
            // Get existing review to check permissions
            var existingReviewResult = await _reviewService.GetReviewByIdAsync(id);
            if (existingReviewResult.IsFailure)
            {
                return HandleResult(existingReviewResult);
            }

            // Check permissions: only review owner or admin can delete
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user authentication");
            }

            if (currentUserId != existingReviewResult.Data!.CustomerId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only delete your own reviews");
            }

            var deleteResult = await _reviewService.DeleteReviewAsync(id);
            return HandleResult(deleteResult);
        }
    }
}
