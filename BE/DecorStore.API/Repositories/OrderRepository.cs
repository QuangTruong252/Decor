using DecorStore.API.Data;
using DecorStore.API.Models;
using DecorStore.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Order>> GetPagedAsync(OrderFilterDTO filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var query = BuildOrderQuery(filter);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            var items = await query
                .Skip(filter.Skip)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> GetByIdWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _context.Orders.Add(order);
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            order.UpdatedAt = System.DateTime.UtcNow;
            _context.Orders.Update(order);
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.OrderStatus = status;
                order.UpdatedAt = System.DateTime.UtcNow;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.IsDeleted = true;
                order.UpdatedAt = System.DateTime.UtcNow;
            }
        }

        public async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            var orders = await _context.Orders
                .Where(o => ids.Contains(o.Id))
                .ToListAsync();

            foreach (var order in orders)
            {
                order.IsDeleted = true;
                order.UpdatedAt = System.DateTime.UtcNow;
            }
        }

        private IQueryable<Order> BuildOrderQuery(OrderFilterDTO filter)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Apply base filters
            if (!filter.IncludeDeleted)
            {
                query = query.Where(o => !o.IsDeleted);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(o =>
                    o.User.FullName.ToLower().Contains(searchTerm) ||
                    o.User.Username.ToLower().Contains(searchTerm) ||
                    o.User.Email.ToLower().Contains(searchTerm) ||
                    (o.Customer != null && (o.Customer.FirstName.ToLower().Contains(searchTerm) ||
                                           o.Customer.LastName.ToLower().Contains(searchTerm) ||
                                           o.Customer.Email.ToLower().Contains(searchTerm))) ||
                    o.ShippingAddress.ToLower().Contains(searchTerm) ||
                    (o.ContactEmail != null && o.ContactEmail.ToLower().Contains(searchTerm)) ||
                    o.OrderStatus.ToLower().Contains(searchTerm) ||
                    o.PaymentMethod.ToLower().Contains(searchTerm));
            }

            // Apply user filter
            if (filter.UserId.HasValue)
            {
                query = query.Where(o => o.UserId == filter.UserId.Value);
            }

            // Apply customer filter
            if (filter.CustomerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(filter.OrderStatus))
            {
                query = query.Where(o => o.OrderStatus.ToLower() == filter.OrderStatus.ToLower());
            }

            // Apply payment method filter
            if (!string.IsNullOrEmpty(filter.PaymentMethod))
            {
                query = query.Where(o => o.PaymentMethod.ToLower() == filter.PaymentMethod.ToLower());
            }

            // Apply amount range filters
            if (filter.MinAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= filter.MaxAmount.Value);
            }

            // Apply date range filters
            if (filter.OrderDateFrom.HasValue)
            {
                query = query.Where(o => o.OrderDate >= filter.OrderDateFrom.Value);
            }

            if (filter.OrderDateTo.HasValue)
            {
                query = query.Where(o => o.OrderDate <= filter.OrderDateTo.Value);
            }

            // Apply location filters
            if (!string.IsNullOrEmpty(filter.ShippingCity))
            {
                query = query.Where(o => o.ShippingCity != null &&
                                        o.ShippingCity.ToLower().Contains(filter.ShippingCity.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.ShippingState))
            {
                query = query.Where(o => o.ShippingState != null &&
                                        o.ShippingState.ToLower().Contains(filter.ShippingState.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.ShippingCountry))
            {
                query = query.Where(o => o.ShippingCountry != null &&
                                        o.ShippingCountry.ToLower().Contains(filter.ShippingCountry.ToLower()));
            }

            return query;
        }

        private IQueryable<Order> ApplySorting(IQueryable<Order> query, PaginationParameters filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
            {
                return query.OrderByDescending(o => o.OrderDate);
            }

            return filter.SortBy.ToLower() switch
            {
                "orderdate" => filter.IsDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate),
                "totalamount" => filter.IsDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                "orderstatus" => filter.IsDescending ? query.OrderByDescending(o => o.OrderStatus) : query.OrderBy(o => o.OrderStatus),
                "paymentmethod" => filter.IsDescending ? query.OrderByDescending(o => o.PaymentMethod) : query.OrderBy(o => o.PaymentMethod),
                "customer" => filter.IsDescending ?
                    query.OrderByDescending(o => o.Customer != null ? o.Customer.LastName : o.User.FullName) :
                    query.OrderBy(o => o.Customer != null ? o.Customer.LastName : o.User.FullName),
                "updatedat" => filter.IsDescending ? query.OrderByDescending(o => o.UpdatedAt) : query.OrderBy(o => o.UpdatedAt),
                _ => query.OrderByDescending(o => o.OrderDate)
            };
        }

        public async Task<int> GetTotalCountAsync(OrderFilterDTO filter)
        {
            var query = BuildOrderQuery(filter);
            return await query.CountAsync();
        }

        // Advanced query methods
        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status, int count = 50)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
                .Where(o => o.OrderStatus.ToLower() == status.ToLower() && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Customer)
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
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
    }
}