using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Repositories;
using DecorStore.API.Exceptions;

namespace DecorStore.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapOrderToDto);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(MapOrderToDto);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(id);
            if (order == null)
                return null;

            return MapOrderToDto(order);
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDTO orderDto)
        {
            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                throw new InvalidOperationException("Order must have at least one item");

            // Tạo order mới
            var order = new Order
            {
                UserId = orderDto.UserId,
                PaymentMethod = orderDto.PaymentMethod,
                ShippingAddress = orderDto.ShippingAddress,
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            // Tạo các order items
            foreach (var itemDto in orderDto.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID {itemDto.ProductId} not found");

                if (product.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Not enough stock for product {product.Name}");

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price, // Giá tại thời điểm mua
                };

                // Cập nhật tổng tiền
                totalAmount += orderItem.UnitPrice * orderItem.Quantity;

                // Cập nhật số lượng tồn kho
                product.StockQuantity -= itemDto.Quantity;
                await _productRepository.UpdateAsync(product);

                order.OrderItems.Add(orderItem);
            }

            // Cập nhật tổng tiền cho order
            order.TotalAmount = totalAmount;

            // Lưu order vào database
            return await _orderRepository.CreateAsync(order);
        }

        public async Task UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO statusDto)
        {
            // Kiểm tra order có tồn tại không
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("Order not found");

            // Kiểm tra trạng thái mới có hợp lệ không
            if (!IsValidOrderStatus(statusDto.OrderStatus))
                throw new InvalidOperationException($"Invalid order status: {statusDto.OrderStatus}");

            // Cập nhật trạng thái
            await _orderRepository.UpdateStatusAsync(id, statusDto.OrderStatus);
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("Order not found");

            // Chỉ cho phép xóa order ở trạng thái Pending
            if (order.OrderStatus != "Pending")
                throw new InvalidOperationException("Cannot delete orders that are not in Pending status");

            await _orderRepository.DeleteAsync(id);
        }

        private OrderDTO MapOrderToDto(Order order)
        {
            if (order == null)
                return null;

            return new OrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                UserFullName = order.User?.FullName ?? string.Empty,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    ProductImageUrl = oi.Product?.Images?.FirstOrDefault(i => i.IsDefault)?.ImageUrl ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.UnitPrice * oi.Quantity
                }).ToList() ?? new List<OrderItemDTO>()
            };
        }

        private bool IsValidOrderStatus(string status)
        {
            // Danh sách các trạng thái hợp lệ
            string[] validStatuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            return validStatuses.Contains(status);
        }
    }
} 