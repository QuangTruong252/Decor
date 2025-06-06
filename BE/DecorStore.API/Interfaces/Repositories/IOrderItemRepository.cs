using DecorStore.API.Models;
using System.Linq.Expressions;

namespace DecorStore.API.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for OrderItem entity operations
    /// </summary>
    public interface IOrderItemRepository
    {
        // Basic CRUD operations
        Task<OrderItem?> GetByIdAsync(int id);
        Task<IEnumerable<OrderItem>> GetAllAsync();
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderItem>> GetByProductIdAsync(int productId);
        Task<OrderItem> AddAsync(OrderItem orderItem);
        Task<IEnumerable<OrderItem>> AddRangeAsync(IEnumerable<OrderItem> orderItems);
        Task UpdateAsync(OrderItem orderItem);
        Task UpdateRangeAsync(IEnumerable<OrderItem> orderItems);
        Task DeleteAsync(OrderItem orderItem);
        Task DeleteRangeAsync(IEnumerable<OrderItem> orderItems);

        // Query operations
        Task<bool> ExistsAsync(Expression<Func<OrderItem, bool>> predicate);
        Task<OrderItem?> FindAsync(Expression<Func<OrderItem, bool>> predicate);
        Task<IEnumerable<OrderItem>> FindAllAsync(Expression<Func<OrderItem, bool>> predicate);
        Task<int> CountAsync(Expression<Func<OrderItem, bool>>? predicate = null);

        // Business-specific operations
        Task<decimal> GetTotalValueByOrderIdAsync(int orderId);
        Task<int> GetTotalQuantityByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderItem>> GetOrderItemsByProductIdAsync(int productId);
        Task<IEnumerable<OrderItem>> GetOrderItemsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> HasPendingOrdersForProductAsync(int productId);
    }
}
