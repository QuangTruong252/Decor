using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerFullName { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }

    public class CreateOrderDTO
    {
        [Required]
        public int UserId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string ShippingCity { get; set; }

        [StringLength(50)]
        public string ShippingState { get; set; }

        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        [StringLength(50)]
        public string ShippingCountry { get; set; }

        [StringLength(100)]
        [Phone]
        public string ContactPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string ContactEmail { get; set; }

        [StringLength(255)]
        public string Notes { get; set; }

        [Required]
        public List<CreateOrderItemDTO> OrderItems { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; }
    }
}