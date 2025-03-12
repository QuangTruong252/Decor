using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId);
        Task<OrderDTO> GetOrderByIdAsync(int id);
        Task<Order> CreateOrderAsync(CreateOrderDTO orderDto);
        Task UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO statusDto);
        Task DeleteOrderAsync(int id);
    }
} 