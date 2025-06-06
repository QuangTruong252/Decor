using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    public class OrderDTO
    {
        public OrderDTO()
        {
            OrderItems = new List<OrderItemDTO>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public required string UserFullName { get; set; }
        public int? CustomerId { get; set; }
        public required string CustomerFullName { get; set; }
        public decimal TotalAmount { get; set; }
        public required string OrderStatus { get; set; }
        public required string PaymentMethod { get; set; }
        public required string ShippingAddress { get; set; }
        public required string ShippingCity { get; set; }
        public required string ShippingState { get; set; }
        public required string ShippingPostalCode { get; set; }
        public required string ShippingCountry { get; set; }
        public required string ContactPhone { get; set; }
        public required string ContactEmail { get; set; }
        public string? Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }

    public class CreateOrderDTO
    {
        public CreateOrderDTO()
        {
            OrderItems = new List<CreateOrderItemDTO>();
        }

        [Required]
        public int UserId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public required string PaymentMethod { get; set; }

        [Required]
        [StringLength(255)]
        public required string ShippingAddress { get; set; }

        [StringLength(100)]
        public required string ShippingCity { get; set; }

        [StringLength(50)]
        public required string ShippingState { get; set; }

        [StringLength(20)]
        public required string ShippingPostalCode { get; set; }

        [StringLength(50)]
        public required string ShippingCountry { get; set; }

        [StringLength(100)]
        [Phone]
        public required string ContactPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public required string ContactEmail { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }

        [Required]
        public List<CreateOrderItemDTO> OrderItems { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        [Required]
        [StringLength(50)]
        public required string OrderStatus { get; set; }
    }

    public class UpdateOrderDTO
    {
        public int? CustomerId { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        [StringLength(100)]
        public string? ShippingCity { get; set; }

        [StringLength(50)]
        public string? ShippingState { get; set; }

        [StringLength(20)]
        public string? ShippingPostalCode { get; set; }

        [StringLength(50)]
        public string? ShippingCountry { get; set; }

        [StringLength(100)]
        [Phone]
        public string? ContactPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? ContactEmail { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }
    }
}