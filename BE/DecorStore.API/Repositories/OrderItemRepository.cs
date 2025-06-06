using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using DecorStore.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DecorStore.API.Repositories
{
    /// <summary>
    /// Repository implementation for OrderItem entity operations
    /// </summary>
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        #region IOrderItemRepository Implementation

        public async Task<IEnumerable<OrderItem>> AddRangeAsync(IEnumerable<OrderItem> orderItems)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            var entities = orderItems.ToList();
            foreach (var entity in entities)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.IsDeleted = false;
            }

            await _context.OrderItems.AddRangeAsync(entities);
            return entities;
        }

        public async Task UpdateRangeAsync(IEnumerable<OrderItem> orderItems)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            var entities = orderItems.ToList();
            foreach (var entity in entities)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }

            _context.OrderItems.UpdateRange(entities);
        }

        public async Task DeleteAsync(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            // Soft delete
            orderItem.IsDeleted = true;
            orderItem.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(orderItem);
        }

        public async Task DeleteRangeAsync(IEnumerable<OrderItem> orderItems)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            var entities = orderItems.ToList();
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            _context.OrderItems.UpdateRange(entities);
        }

        public async Task<bool> ExistsAsync(Expression<Func<OrderItem, bool>> predicate)
        {
            return await _context.OrderItems
                .Where(oi => !oi.IsDeleted)
                .AnyAsync(predicate);
        }

        public new async Task<OrderItem?> FindAsync(Expression<Func<OrderItem, bool>> predicate)
        {
            return await _context.OrderItems
                .AsNoTracking()
                .Where(oi => !oi.IsDeleted)
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<OrderItem>> FindAllAsync(Expression<Func<OrderItem, bool>> predicate)
        {
            return await _context.OrderItems
                .AsNoTracking()
                .Where(oi => !oi.IsDeleted)
                .Where(predicate)
                .ToListAsync();
        }

        public new async Task<int> CountAsync(Expression<Func<OrderItem, bool>>? predicate = null)
        {
            var query = _context.OrderItems.Where(oi => !oi.IsDeleted);
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            return await query.CountAsync();
        }

        #endregion

        #region Business-specific Operations

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(int productId)
        {
            return await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .Include(oi => oi.Order)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalValueByOrderIdAsync(int orderId)
        {
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .SumAsync(oi => oi.Quantity * oi.UnitPrice);
        }

        public async Task<int> GetTotalQuantityByOrderIdAsync(int orderId)
        {
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .SumAsync(oi => oi.Quantity);
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByProductIdAsync(int productId)
        {
            return await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .Include(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<bool> HasPendingOrdersForProductAsync(int productId)
        {
            return await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == productId && 
                               oi.Order.OrderStatus == "Pending");
        }

        #endregion
    }
}
