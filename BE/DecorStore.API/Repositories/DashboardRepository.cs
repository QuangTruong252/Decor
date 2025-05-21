using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.Data;
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
                .Where(p => !p.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetTotalOrderCountAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetTotalCustomerCountAsync()
        {
            return await _context.Customers
                .Where(c => !c.IsDeleted)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 5)
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.User)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<(Product Product, int TotalSold, decimal TotalRevenue)>> GetPopularProductsAsync(int count = 5)
        {
            // Get products with their sales data
            var productSales = await _context.OrderItems
                .Where(oi => !oi.IsDeleted && !oi.Order.IsDeleted)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .ToListAsync();

            // Get the product details for these top-selling products
            var productIds = productSales.Select(p => p.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Images)
                .ToListAsync();

            // Join the product details with sales data
            return productSales
                .Join(products,
                    ps => ps.ProductId,
                    p => p.Id,
                    (ps, p) => (p, ps.TotalSold, ps.TotalRevenue))
                .ToList();
        }

        public async Task<IEnumerable<(Category Category, int TotalSales, decimal TotalRevenue)>> GetSalesByCategoryAsync()
        {
            // Get all categories with their sales data
            var categorySales = await _context.OrderItems
                .Where(oi => !oi.IsDeleted && !oi.Order.IsDeleted)
                .Join(_context.Products,
                    oi => oi.ProductId,
                    p => p.Id,
                    (oi, p) => new { OrderItem = oi, Product = p })
                .GroupBy(x => x.Product.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    TotalSales = g.Sum(x => x.OrderItem.Quantity),
                    TotalRevenue = g.Sum(x => x.OrderItem.Quantity * x.OrderItem.UnitPrice)
                })
                .ToListAsync();

            // Get the category details
            var categoryIds = categorySales.Select(c => c.CategoryId).ToList();
            var categories = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();

            // Join the category details with sales data
            return categorySales
                .Join(categories,
                    cs => cs.CategoryId,
                    c => c.Id,
                    (cs, c) => (c, cs.TotalSales, cs.TotalRevenue))
                .OrderByDescending(x => x.TotalSales)
                .ToList();
        }

        public async Task<Dictionary<string, int>> GetOrderStatusDistributionAsync()
        {
            var statusCounts = await _context.Orders
                .Where(o => !o.IsDeleted)
                .GroupBy(o => o.OrderStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            
            // Ensure all statuses are represented
            string[] validStatuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            foreach (var status in validStatuses)
            {
                result[status] = statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0;
            }

            return result;
        }

        public async Task<IEnumerable<(DateTime Date, decimal Revenue, int OrderCount)>> GetSalesTrendAsync(
            string period = "daily", 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            // Set default date range if not provided
            endDate ??= DateTime.UtcNow.Date;
            
            // Default start date based on period if not provided
            if (startDate == null)
            {
                startDate = period.ToLower() switch
                {
                    "daily" => endDate.Value.AddDays(-30),
                    "weekly" => endDate.Value.AddDays(-90),
                    "monthly" => endDate.Value.AddMonths(-12),
                    _ => endDate.Value.AddDays(-30)
                };
            }

            // Get all orders within the date range
            var orders = await _context.Orders
                .Where(o => !o.IsDeleted && 
                           o.OrderDate >= startDate.Value && 
                           o.OrderDate <= endDate.Value)
                .ToListAsync();

            // Group by the appropriate period
            var groupedOrders = period.ToLower() switch
            {
                "daily" => orders.GroupBy(o => o.OrderDate.Date),
                "weekly" => orders.GroupBy(o => new DateTime(
                    o.OrderDate.Year, 
                    o.OrderDate.Month, 
                    ((o.OrderDate.Day - 1) / 7) * 7 + 1)),
                "monthly" => orders.GroupBy(o => new DateTime(
                    o.OrderDate.Year, 
                    o.OrderDate.Month, 
                    1)),
                _ => orders.GroupBy(o => o.OrderDate.Date)
            };

            // Calculate revenue and order count for each period
            var result = groupedOrders
                .Select(g => (
                    Date: g.Key,
                    Revenue: g.Sum(o => o.TotalAmount),
                    OrderCount: g.Count()
                ))
                .OrderBy(x => x.Date)
                .ToList();

            return result;
        }
    }
}
