using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace DecorStore.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id);
            if (order == null)
                return null;

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO orderDto)
        {
            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                throw new InvalidOperationException("Order must have at least one item");

            // Use execution strategy to handle retries with transactions
            var order = await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Map DTO to entity
                    var order = _mapper.Map<Order>(orderDto);
                    order.OrderStatus = "Pending";
                    order.OrderDate = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;
                    order.OrderItems = new List<OrderItem>();

                    decimal totalAmount = 0;

                    // Tạo các order items
                    foreach (var itemDto in orderDto.OrderItems)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
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
                        await _unitOfWork.Products.UpdateAsync(product);

                        order.OrderItems.Add(orderItem);
                    }

                    // Cập nhật tổng tiền cho order
                    order.TotalAmount = totalAmount;

                    // Lưu order vào database
                    await _unitOfWork.Orders.CreateAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();

                    return order;
                }
                catch (Exception)
                {
                    // Rollback transaction on error
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });

            // Xóa cache liên quan đến dashboard
            InvalidateDashboardCache();

            // Map the entity to DTO before returning
            return await GetOrderByIdAsync(order.Id);
        }

        public async Task UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO statusDto)
        {
            // Kiểm tra order có tồn tại không
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("Order not found");

            // Kiểm tra trạng thái mới có hợp lệ không
            if (!IsValidOrderStatus(statusDto.OrderStatus))
                throw new InvalidOperationException($"Invalid order status: {statusDto.OrderStatus}");

            // Cập nhật trạng thái
            await _unitOfWork.Orders.UpdateStatusAsync(id, statusDto.OrderStatus);
            await _unitOfWork.SaveChangesAsync();

            // Xóa cache liên quan đến dashboard để đảm bảo dữ liệu mới nhất
            InvalidateDashboardCache();
        }

        private void InvalidateDashboardCache()
        {
            // Xóa tất cả các cache liên quan đến dashboard
            _cache.Remove(DashboardService.DASHBOARD_SUMMARY_CACHE_KEY);
            _cache.Remove(DashboardService.SALES_BY_CATEGORY_CACHE_KEY);
            _cache.Remove(DashboardService.ORDER_STATUS_DISTRIBUTION_CACHE_KEY);

            // Xóa cache sales trend (có thể có nhiều key với prefix khác nhau)
            // Trong trường hợp thực tế, bạn có thể cần một cơ chế phức tạp hơn để xóa cache theo prefix
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("Order not found");

            // Chỉ cho phép xóa order ở trạng thái Pending
            if (order.OrderStatus != "Pending")
                throw new InvalidOperationException("Cannot delete orders that are not in Pending status");

            await _unitOfWork.Orders.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            // Xóa cache liên quan đến dashboard
            InvalidateDashboardCache();
        }

        private bool IsValidOrderStatus(string status)
        {
            string[] validStatuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            return validStatuses.Contains(status);
        }
    }
}