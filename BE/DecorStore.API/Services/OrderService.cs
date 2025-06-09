using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using DecorStore.API.Common;
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

        public async Task<Result<PagedResult<OrderDTO>>> GetPagedOrdersAsync(OrderFilterDTO filter)
        {
            try
            {
                if (filter == null)
                {
                    return Result<PagedResult<OrderDTO>>.Failure("INVALID_REQUEST", "Filter cannot be null");
                }

                var pagedOrders = await _unitOfWork.Orders.GetPagedAsync(filter);
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(pagedOrders.Items);

                var result = new PagedResult<OrderDTO>(orderDtos, pagedOrders.Pagination.TotalCount,
                    pagedOrders.Pagination.CurrentPage, pagedOrders.Pagination.PageSize);

                return Result<PagedResult<OrderDTO>>.Success(result);
            }            catch (Exception ex)
            {
                return Result<PagedResult<OrderDTO>>.Failure("Failed to retrieve paged orders", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<OrderDTO>>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetAllAsync();
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
                return Result<IEnumerable<OrderDTO>>.Success(orderDtos);
            }            catch (Exception ex)
            {
                return Result<IEnumerable<OrderDTO>>.Failure("Failed to retrieve all orders", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<OrderDTO>>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return Result<IEnumerable<OrderDTO>>.Failure("INVALID_USER_ID", "User ID must be greater than 0");
                }

                var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
                return Result<IEnumerable<OrderDTO>>.Success(orderDtos);
            }            catch (Exception ex)
            {
                return Result<IEnumerable<OrderDTO>>.Failure("Failed to retrieve orders by user ID", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<OrderDTO>> GetOrderByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result<OrderDTO>.Failure("INVALID_ID", "Order ID must be greater than 0");
                }

                var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id);
                if (order == null)
                {
                    return Result<OrderDTO>.Failure("NOT_FOUND", $"Order with ID {id} not found");
                }

                var orderDto = _mapper.Map<OrderDTO>(order);
                return Result<OrderDTO>.Success(orderDto);
            }            catch (Exception ex)
            {
                return Result<OrderDTO>.Failure("Failed to retrieve order by ID", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<OrderDTO>> CreateOrderAsync(CreateOrderDTO orderDto)
        {
            try
            {
                // Validate input
                var validationResult = await ValidateCreateOrderDto(orderDto);
                if (validationResult.IsFailure)
                {
                    return Result<OrderDTO>.Failure(validationResult.ErrorCode!, validationResult.Error!, validationResult.ErrorDetails);
                }

                // Validate Customer exists if CustomerId is provided
                if (orderDto.CustomerId.HasValue)
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(orderDto.CustomerId.Value);
                    if (customer == null)
                    {
                        return Result<OrderDTO>.Failure("CUSTOMER_NOT_FOUND", $"Customer with ID {orderDto.CustomerId.Value} not found");
                    }
                }

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

                        // Create order items and validate inventory
                        foreach (var itemDto in orderDto.OrderItems)
                        {
                            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                            if (product == null)
                            {
                                throw new NotFoundException($"Product with ID {itemDto.ProductId} not found");
                            }

                            if (product.StockQuantity < itemDto.Quantity)
                            {
                                throw new InvalidOperationException($"Not enough stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}");
                            }

                            var orderItem = new OrderItem
                            {
                                ProductId = product.Id,
                                Quantity = itemDto.Quantity,
                                UnitPrice = product.Price, // Price at time of purchase
                            };

                            // Update total amount
                            totalAmount += orderItem.UnitPrice * orderItem.Quantity;

                            // Update stock quantity
                            product.StockQuantity -= itemDto.Quantity;
                            await _unitOfWork.Products.UpdateAsync(product);

                            order.OrderItems.Add(orderItem);
                        }

                        // Update total amount for order
                        order.TotalAmount = totalAmount;

                        // Save order to database
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

                // Invalidate related cache
                InvalidateDashboardCache();

                // Get the created order with all details
                var createdOrderResult = await GetOrderByIdAsync(order.Id);
                if (createdOrderResult.IsFailure)
                {
                    return Result<OrderDTO>.Failure("ORDER_RETRIEVAL_ERROR", "Order was created but could not be retrieved");
                }

                return Result<OrderDTO>.Success(createdOrderResult.Data!);
            }
            catch (NotFoundException ex)
            {
                return Result<OrderDTO>.Failure("NOT_FOUND", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Result<OrderDTO>.Failure("BUSINESS_RULE_VIOLATION", ex.Message);
            }            catch (Exception ex)
            {
                return Result<OrderDTO>.Failure("Failed to create order", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result> UpdateOrderAsync(int id, UpdateOrderDTO orderDto)
        {
            try
            {
                if (id <= 0)
                {
                    return Result.Failure("INVALID_ID", "Order ID must be greater than 0");
                }

                // Validate input
                var validationResult = await ValidateUpdateOrderDto(orderDto);
                if (validationResult.IsFailure)
                {
                    return Result.Failure(validationResult.ErrorCode!, validationResult.Error!, validationResult.ErrorDetails);
                }

                // Check if order exists
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return Result.Failure("NOT_FOUND", $"Order with ID {id} not found");
                }

                // Validate Customer exists if CustomerId is provided
                if (orderDto.CustomerId.HasValue)
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(orderDto.CustomerId.Value);
                    if (customer == null)
                    {
                        return Result.Failure("CUSTOMER_NOT_FOUND", $"Customer with ID {orderDto.CustomerId.Value} not found");
                    }
                }

                // Map DTO to entity (only update provided fields)
                _mapper.Map(orderDto, order);

                // Update the order
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate related cache
                InvalidateDashboardCache();

                return Result.Success();
            }            catch (Exception ex)
            {
                return Result.Failure("Failed to update order", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO statusDto)
        {
            try
            {
                if (id <= 0)
                {
                    return Result.Failure("INVALID_ID", "Order ID must be greater than 0");
                }

                if (statusDto == null || string.IsNullOrWhiteSpace(statusDto.OrderStatus))
                {
                    return Result.Failure("INVALID_STATUS", "Order status cannot be null or empty");
                }

                // Check if order exists
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return Result.Failure("NOT_FOUND", $"Order with ID {id} not found");
                }

                // Validate new status
                if (!IsValidOrderStatus(statusDto.OrderStatus))
                {
                    return Result.Failure("INVALID_STATUS", $"Invalid order status: {statusDto.OrderStatus}. Valid statuses are: Pending, Processing, Shipped, Delivered, Cancelled");
                }

                // Business rule: validate status transitions
                var transitionResult = ValidateStatusTransition(order.OrderStatus, statusDto.OrderStatus);
                if (transitionResult.IsFailure)
                {
                    return transitionResult;
                }

                // Update status
                await _unitOfWork.Orders.UpdateStatusAsync(id, statusDto.OrderStatus);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate related cache
                InvalidateDashboardCache();

                return Result.Success();
            }            catch (Exception ex)
            {
                return Result.Failure("Failed to update order status", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result> DeleteOrderAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result.Failure("INVALID_ID", "Order ID must be greater than 0");
                }

                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return Result.Failure("NOT_FOUND", $"Order with ID {id} not found");
                }

                // Business rule: Only allow deletion of pending orders
                if (order.OrderStatus != "Pending")
                {
                    return Result.Failure("INVALID_STATUS", "Cannot delete orders that are not in Pending status");
                }

                await _unitOfWork.Orders.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate related cache
                InvalidateDashboardCache();

                return Result.Success();
            }            catch (Exception ex)
            {
                return Result.Failure("Failed to delete order", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result> BulkDeleteOrdersAsync(BulkDeleteDTO bulkDeleteDto)
        {
            try
            {
                if (bulkDeleteDto?.Ids == null || !bulkDeleteDto.Ids.Any())
                {
                    return Result.Failure("INVALID_REQUEST", "No order IDs provided for deletion");
                }

                // Use execution strategy to handle retries with transactions
                await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Validate all orders before deletion
                        var invalidOrders = new List<(int Id, string Reason)>();
                        var validOrderIds = new List<int>();

                        foreach (var id in bulkDeleteDto.Ids)
                        {
                            var order = await _unitOfWork.Orders.GetByIdAsync(id);
                            if (order == null)
                            {
                                invalidOrders.Add((id, "Order not found"));
                                continue;
                            }

                            // Only allow deletion of pending orders
                            if (order.OrderStatus != "Pending")
                            {
                                invalidOrders.Add((id, "Cannot delete orders that are not in Pending status"));
                                continue;
                            }

                            validOrderIds.Add(id);
                        }

                        // If there are invalid orders, throw an exception with details
                        if (invalidOrders.Any())
                        {
                            var errorMessage = "Some orders could not be deleted: " +
                                string.Join(", ", invalidOrders.Select(o => $"Order #{o.Id}: {o.Reason}"));
                            throw new InvalidOperationException(errorMessage);
                        }

                        // Delete all valid orders
                        await _unitOfWork.Orders.BulkDeleteAsync(validOrderIds);
                        await _unitOfWork.SaveChangesAsync();

                        // Commit transaction
                        await _unitOfWork.CommitTransactionAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        // Rollback transaction on error
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                });

                // Invalidate related cache
                InvalidateDashboardCache();

                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure("BUSINESS_RULE_VIOLATION", ex.Message);
            }            catch (Exception ex)
            {
                return Result.Failure("Failed to bulk delete orders", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        // Advanced query methods
        public async Task<Result<IEnumerable<OrderDTO>>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                if (count <= 0)
                {
                    return Result<IEnumerable<OrderDTO>>.Failure("INVALID_COUNT", "Count must be greater than 0");
                }

                var orders = await _unitOfWork.Orders.GetRecentOrdersAsync(count);
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
                return Result<IEnumerable<OrderDTO>>.Success(orderDtos);
            }            catch (Exception ex)
            {
                return Result<IEnumerable<OrderDTO>>.Failure("Failed to retrieve recent orders", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<OrderDTO>>> GetOrdersByStatusAsync(string status, int count = 50)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return Result<IEnumerable<OrderDTO>>.Failure("INVALID_STATUS", "Status cannot be null or empty");
                }

                if (count <= 0)
                {
                    return Result<IEnumerable<OrderDTO>>.Failure("INVALID_COUNT", "Count must be greater than 0");
                }

                if (!IsValidOrderStatus(status))
                {
                    return Result<IEnumerable<OrderDTO>>.Failure("INVALID_STATUS", $"Invalid order status: {status}");
                }

                var orders = await _unitOfWork.Orders.GetOrdersByStatusAsync(status, count);
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
                return Result<IEnumerable<OrderDTO>>.Success(orderDtos);
            }            catch (Exception ex)
            {
                return Result<IEnumerable<OrderDTO>>.Failure("Failed to retrieve orders by status", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<IEnumerable<OrderDTO>>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return Result<IEnumerable<OrderDTO>>.Failure("INVALID_DATE_RANGE", "Start date cannot be greater than end date");
                }

                var orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(startDate, endDate);
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
                return Result<IEnumerable<OrderDTO>>.Success(orderDtos);
            }            catch (Exception ex)
            {
                return Result<IEnumerable<OrderDTO>>.Failure("Failed to retrieve orders by date range", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return Result<decimal>.Failure("INVALID_DATE_RANGE", "Start date cannot be greater than end date");
                }

                var revenue = await _unitOfWork.Orders.GetTotalRevenueAsync(startDate, endDate);
                return Result<decimal>.Success(revenue);
            }            catch (Exception ex)
            {
                return Result<decimal>.Failure("Failed to retrieve total revenue", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<Dictionary<string, int>>> GetOrderStatusCountsAsync()
        {
            try
            {
                var statusCounts = await _unitOfWork.Orders.GetOrderStatusCountsAsync();
                return Result<Dictionary<string, int>>.Success(statusCounts);
            }            catch (Exception ex)
            {
                return Result<Dictionary<string, int>>.Failure("Failed to retrieve order status counts", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        #region Private Helper Methods

        private void InvalidateDashboardCache()
        {
            // Remove all dashboard-related cache entries
            _cache.Remove(DashboardService.DASHBOARD_SUMMARY_CACHE_KEY);
            _cache.Remove(DashboardService.SALES_BY_CATEGORY_CACHE_KEY);
            _cache.Remove(DashboardService.ORDER_STATUS_DISTRIBUTION_CACHE_KEY);
        }

        private bool IsValidOrderStatus(string status)
        {
            string[] validStatuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            return validStatuses.Contains(status);
        }

        private Result ValidateStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<string, string[]>
            {
                ["Pending"] = new[] { "Processing", "Cancelled" },
                ["Processing"] = new[] { "Shipped", "Cancelled" },
                ["Shipped"] = new[] { "Delivered" },
                ["Delivered"] = new string[0], // No transitions from delivered
                ["Cancelled"] = new string[0]  // No transitions from cancelled
            };

            if (!validTransitions.ContainsKey(currentStatus))
            {
                return Result.Failure("INVALID_CURRENT_STATUS", $"Invalid current status: {currentStatus}");
            }

            if (!validTransitions[currentStatus].Contains(newStatus))
            {
                return Result.Failure("INVALID_STATUS_TRANSITION", 
                    $"Cannot transition from {currentStatus} to {newStatus}. Valid transitions: {string.Join(", ", validTransitions[currentStatus])}");
            }

            return Result.Success();
        }        private Task<Result> ValidateCreateOrderDto(CreateOrderDTO orderDto)
        {
            var errors = new List<string>();

            if (orderDto == null)
            {
                return Task.FromResult(Result.Failure("INVALID_REQUEST", "Order data cannot be null"));
            }

            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
            {
                errors.Add("Order must have at least one item");
            }
            else
            {
                foreach (var item in orderDto.OrderItems)
                {
                    if (item.ProductId <= 0)
                    {
                        errors.Add($"Invalid product ID: {item.ProductId}");
                    }

                    if (item.Quantity <= 0)
                    {
                        errors.Add($"Invalid quantity for product {item.ProductId}: {item.Quantity}");
                    }
                }
            }

            if (errors.Any())
            {
                return Task.FromResult(Result.Failure("Validation failed", "VALIDATION_ERROR", errors));
            }

            return Task.FromResult(Result.Success());
        }private async Task<Result> ValidateUpdateOrderDto(UpdateOrderDTO orderDto)
        {
            var errors = new List<string>();

            if (orderDto == null)
            {
                return Result.Failure("INVALID_REQUEST", "Order data cannot be null");
            }

            // For update, validation is more lenient as fields are optional
            if (orderDto.CustomerId.HasValue && orderDto.CustomerId.Value <= 0)
            {
                errors.Add("Invalid customer ID");
            }

            if (errors.Any())
            {
                return Result.Failure("Validation failed", "VALIDATION_ERROR", errors);
            }

            await Task.CompletedTask;
            return Result.Success();
        }

        #endregion
    }
}