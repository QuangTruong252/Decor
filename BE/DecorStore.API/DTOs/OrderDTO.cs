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
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }
    
    public class CreateOrderDTO
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; }
        
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