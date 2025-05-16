using System;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;

namespace DecorStore.API.Examples
{
    // This is an example class demonstrating how to use the Unit of Work pattern with transactions
    public class UnitOfWorkExample
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public UnitOfWorkExample(IUnitOfWork unitOfWork, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        // Example of using Unit of Work with transactions
        public async Task<Order> CreateOrderWithTransactionAsync(CreateOrderDTO orderDto, int userId)
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Create order
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = 0, // Will be calculated based on items
                    OrderStatus = "Pending",
                    PaymentMethod = orderDto.PaymentMethod,
                    ShippingAddress = orderDto.ShippingAddress,
                    OrderDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Orders.CreateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Create order items
                foreach (var item in orderDto.OrderItems)
                {
                    // Get product
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new NotFoundException($"Product with ID {item.ProductId} not found");
                    }

                    // Check stock
                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Not enough stock for product {product.Name}");
                    }

                    // Create order item
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    // Add order item
                    _unitOfWork.Orders.AddOrderItem(orderItem);

                    // Update product stock
                    product.StockQuantity -= item.Quantity;
                    await _unitOfWork.Products.UpdateAsync(product);
                }

                // Save all changes
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
        }
    }
}
