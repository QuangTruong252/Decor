using System;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateReviewDTO
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        
        [StringLength(500)]
        public string Comment { get; set; }
    }
    
    public class UpdateReviewDTO
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        
        [StringLength(500)]
        public string Comment { get; set; }
    }
} 