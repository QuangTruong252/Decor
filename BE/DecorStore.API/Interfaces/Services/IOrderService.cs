using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Common;

namespace DecorStore.API.Services
{
    public interface IOrderService
    {
        // Pagination methods
        Task<Result<PagedResult<OrderDTO>>> GetPagedOrdersAsync(OrderFilterDTO filter);
        Task<Result<IEnumerable<OrderDTO>>> GetAllOrdersAsync();
        Task<Result<IEnumerable<OrderDTO>>> GetOrdersByUserIdAsync(int userId);

        // Single item queries
        Task<Result<OrderDTO>> GetOrderByIdAsync(int id);

        // CRUD operations
        Task<Result<OrderDTO>> CreateOrderAsync(CreateOrderDTO orderDto);
        Task<Result> UpdateOrderAsync(int id, UpdateOrderDTO orderDto);
        Task<Result> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO statusDto);
        Task<Result> DeleteOrderAsync(int id);
        Task<Result> BulkDeleteOrdersAsync(BulkDeleteDTO bulkDeleteDto);

        // Advanced queries
        Task<Result<IEnumerable<OrderDTO>>> GetRecentOrdersAsync(int count = 10);
        Task<Result<IEnumerable<OrderDTO>>> GetOrdersByStatusAsync(string status, int count = 50);
        Task<Result<IEnumerable<OrderDTO>>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Result<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Result<Dictionary<string, int>>> GetOrderStatusCountsAsync();
    }
}