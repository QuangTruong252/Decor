using DecorStore.API.Models;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Order>> GetPagedAsync(OrderFilterDTO filter)
        {
            var query = GetFilteredOrders(filter);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                            .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                order.OrderStatus = status;
                order.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(order);
            }
        }

        public new async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            var orders = await _context.Orders
                .Where(o => ids.Contains(o.Id))
                .ToListAsync();

            foreach (var order in orders)
            {
                order.IsDeleted = true;
            }
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status, int count = 50)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderStatus == status && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.Where(o => !o.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        public async Task<Dictionary<string, int>> GetOrderStatusCountsAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<int> GetTotalCountAsync(OrderFilterDTO filter)
        {
            return await GetFilteredOrders(filter).CountAsync();
        }

        private IQueryable<Order> GetFilteredOrders(OrderFilterDTO filter)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(o =>
                    (o.Customer.FirstName + " " + o.Customer.LastName).ToLower().Contains(searchTerm) ||
                    o.Customer.Email.ToLower().Contains(searchTerm) ||
                    o.OrderStatus.ToLower().Contains(searchTerm)
                );
            }

            if (filter.UserId.HasValue)
                query = query.Where(o => o.UserId == filter.UserId);

            if (filter.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filter.CustomerId);

            if (!string.IsNullOrWhiteSpace(filter.OrderStatus))
                query = query.Where(o => o.OrderStatus == filter.OrderStatus);

            if (!string.IsNullOrWhiteSpace(filter.PaymentMethod))
                query = query.Where(o => o.PaymentMethod == filter.PaymentMethod);

            if (filter.MinAmount.HasValue)
                query = query.Where(o => o.TotalAmount >= filter.MinAmount);

            if (filter.MaxAmount.HasValue)
                query = query.Where(o => o.TotalAmount <= filter.MaxAmount);

            if (filter.OrderDateFrom.HasValue)
                query = query.Where(o => o.OrderDate >= filter.OrderDateFrom);

            if (filter.OrderDateTo.HasValue)
                query = query.Where(o => o.OrderDate <= filter.OrderDateTo);

            if (!string.IsNullOrWhiteSpace(filter.ShippingCity))
                query = query.Where(o => o.ShippingCity == filter.ShippingCity);

            if (!string.IsNullOrWhiteSpace(filter.ShippingState))
                query = query.Where(o => o.ShippingState == filter.ShippingState);

            if (!string.IsNullOrWhiteSpace(filter.ShippingCountry))
                query = query.Where(o => o.ShippingCountry == filter.ShippingCountry);

            if (!filter.IncludeDeleted)
                query = query.Where(o => !o.IsDeleted);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "date_asc" => query.OrderBy(o => o.OrderDate),
                "date_desc" => query.OrderByDescending(o => o.OrderDate),
                "amount_asc" => query.OrderBy(o => o.TotalAmount),
                "amount_desc" => query.OrderByDescending(o => o.TotalAmount),
                "status_asc" => query.OrderBy(o => o.OrderStatus),
                "status_desc" => query.OrderByDescending(o => o.OrderStatus),
                _ => query.OrderByDescending(o => o.OrderDate)
            };

            return query;
        }
    }
}
