using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface IOrderService
    {
        // Pagination methods
        Task<PagedResult<OrderDTO>> GetPagedOrdersAsync(OrderFilterDTO filter);
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId);

        // Single item queries
        Task<OrderDTO> GetOrderByIdAsync(int id);

        // CRUD operations
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO orderDto);
        Task UpdateOrderAsync(int id, UpdateOrderDTO orderDto);
        Task UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO statusDto);
        Task DeleteOrderAsync(int id);
        Task BulkDeleteOrdersAsync(BulkDeleteDTO bulkDeleteDto);

        // Advanced queries
        Task<IEnumerable<OrderDTO>> GetRecentOrdersAsync(int count = 10);
        Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status, int count = 50);
        Task<IEnumerable<OrderDTO>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetOrderStatusCountsAsync();
    }
}