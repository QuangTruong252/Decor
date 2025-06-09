using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted && p.IsActive);
        }

        public async Task<int> GetTotalOrderCountAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .CountAsync(o => !o.IsDeleted);
        }

        public async Task<int> GetTotalCustomerCountAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .CountAsync(c => !c.IsDeleted);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.OrderStatus == "Completed")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<decimal> GetRevenueForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && 
                           o.OrderStatus == "Completed" &&
                           o.OrderDate >= startDate && 
                           o.OrderDate <= endDate)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders
                .AsNoTracking()
                .AsSplitQuery()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems.Where(oi => !oi.IsDeleted))
                    .ThenInclude(oi => oi.Product)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Take(1)) // Only first image for performance
                    .ThenInclude(pi => pi.Image)
                .Where(p => !p.IsDeleted && 
                           p.IsActive && 
                           p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetOrderStatusCountsAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted)
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<string, decimal>> GetSalesByMonthAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);

            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && 
                           o.OrderStatus == "Completed" &&
                           o.OrderDate >= startDate && 
                           o.OrderDate <= endDate)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToDictionaryAsync(x => GetMonthName(x.Month), x => x.Total);
        }

        public async Task<Dictionary<string, decimal>> GetSalesByCategoryAsync()
        {
            return await _context.OrderItems
                .AsNoTracking()
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => !oi.IsDeleted && 
                            !oi.Order.IsDeleted && 
                            !oi.Product.IsDeleted &&
                            oi.Order.OrderStatus == "Completed")
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new { Category = g.Key, Total = g.Sum(oi => oi.UnitPrice * oi.Quantity) })
                .ToDictionaryAsync(x => x.Category, x => x.Total);
        }

        public async Task<Dictionary<string, int>> GetOrderStatusDistributionAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted)
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<IEnumerable<Product>> GetPopularProductsAsync(int count = 10)
        {
            return await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Take(1)) // Only first image for performance
                    .ThenInclude(pi => pi.Image)
                .Include(p => p.OrderItems.Where(oi => !oi.IsDeleted && !oi.Order.IsDeleted))
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity))
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetSalesTrendAsync(string interval, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.Where(o => !o.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            return interval.ToLower() switch
            {
                "daily" => await query
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Total = g.Sum(o => o.TotalAmount) })
                    .ToDictionaryAsync(x => x.Date, x => x.Total),

                "weekly" => await query
                    .GroupBy(o => o.OrderDate.AddDays(-(int)o.OrderDate.DayOfWeek).Date)
                    .Select(g => new { WeekStart = g.Key.ToString("yyyy-MM-dd"), Total = g.Sum(o => o.TotalAmount) })
                    .ToDictionaryAsync(x => x.WeekStart, x => x.Total),

                "monthly" => await query
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new { Date = $"{g.Key.Year}-{g.Key.Month:D2}", Total = g.Sum(o => o.TotalAmount) })
                    .ToDictionaryAsync(x => x.Date, x => x.Total),

                _ => throw new ArgumentException("Invalid interval. Use 'daily', 'weekly', or 'monthly'.")
            };
        }

        private static string GetMonthName(int month)
        {
            return new DateTime(2000, month, 1).ToString("MMMM");
        }

        // Legacy method implementations for backward compatibility
        public async Task<int> GetTotalProductsCountAsync()
        {
            return await GetTotalProductCountAsync();
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await GetTotalOrderCountAsync();
        }

        public async Task<int> GetTotalCustomersCountAsync()
        {
            return await GetTotalCustomerCountAsync();
        }

        public async Task<Dictionary<string, decimal>> GetMonthlySalesAsync(int year)
        {
            return await GetSalesByMonthAsync(year);
        }

        public async Task<Dictionary<string, int>> GetProductCategoriesDistributionAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }

        public async Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count = 10)
        {
            return await GetPopularProductsAsync(count);
        }

        public async Task<IEnumerable<Customer>> GetTopCustomersAsync(int count = 10)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Orders.Sum(o => o.TotalAmount))
                .Take(count)
                .ToListAsync();
        }
    }
}
