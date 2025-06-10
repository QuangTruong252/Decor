using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Interfaces.Repositories.Base;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        // Paginated queries (GetPagedAsync inherited from base)
        Task<PagedResult<Order>> GetPagedAsync(OrderFilterDTO filter);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);        // Single item queries (GetByIdAsync inherited from base)
        Task<Order?> GetByIdWithItemsAsync(int id);

        // CRUD operations (basic CRUD inherited from base)
        Task UpdateStatusAsync(int id, string status);
        new Task BulkDeleteAsync(IEnumerable<int> ids);
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
