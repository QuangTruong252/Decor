using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        // Paginated queries
        Task<PagedResult<Order>> GetPagedAsync(OrderFilterDTO filter);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

        // Single item queries
        Task<Order> GetByIdAsync(int id);
        Task<Order> GetByIdWithItemsAsync(int id);

        // CRUD operations
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task UpdateStatusAsync(int id, string status);
        Task DeleteAsync(int id);
        Task BulkDeleteAsync(IEnumerable<int> ids);
        void AddOrderItem(OrderItem orderItem);

        // Advanced queries
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status, int count = 50);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetOrderStatusCountsAsync();
        Task<int> GetTotalCountAsync(OrderFilterDTO filter);
    }
}
