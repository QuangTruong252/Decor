using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    // DTO for cart item
    public class CartItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    // DTO for cart
    public class CartDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? SessionId { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
    }

    // DTO for adding item to cart
    public class AddToCartDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }

    // DTO for updating cart item
    public class UpdateCartItemDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
