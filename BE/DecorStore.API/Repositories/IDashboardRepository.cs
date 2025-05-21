using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.Models;

namespace DecorStore.API.Repositories
{
    public interface IDashboardRepository
    {
        // Get counts for dashboard summary
        Task<int> GetTotalProductCountAsync();
        Task<int> GetTotalOrderCountAsync();
        Task<int> GetTotalCustomerCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        
        // Get recent orders for dashboard
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 5);
        
        // Get popular products
        Task<IEnumerable<(Product Product, int TotalSold, decimal TotalRevenue)>> GetPopularProductsAsync(int count = 5);
        
        // Get sales by category
        Task<IEnumerable<(Category Category, int TotalSales, decimal TotalRevenue)>> GetSalesByCategoryAsync();
        
        // Get order status distribution
        Task<Dictionary<string, int>> GetOrderStatusDistributionAsync();
        
        // Get sales trend data
        Task<IEnumerable<(DateTime Date, decimal Revenue, int OrderCount)>> GetSalesTrendAsync(
            string period = "daily", 
            DateTime? startDate = null, 
            DateTime? endDate = null);
    }
}
