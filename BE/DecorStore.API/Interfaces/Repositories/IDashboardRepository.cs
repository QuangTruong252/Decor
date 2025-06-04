using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.Models;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        // Basic count methods
        Task<int> GetTotalProductCountAsync();
        Task<int> GetTotalOrderCountAsync();
        Task<int> GetTotalCustomerCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        
        // Revenue methods
        Task<decimal> GetRevenueForPeriodAsync(DateTime startDate, DateTime endDate);
        
        // Recent data methods
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
        
        // Status and distribution methods
        Task<Dictionary<string, int>> GetOrderStatusCountsAsync();
        Task<Dictionary<string, decimal>> GetSalesByMonthAsync(int year);
        Task<Dictionary<string, decimal>> GetSalesByCategoryAsync();
        Task<Dictionary<string, int>> GetOrderStatusDistributionAsync();
        
        // Popular/Top items methods
        Task<IEnumerable<Product>> GetPopularProductsAsync(int count = 10);
        Task<Dictionary<string, decimal>> GetSalesTrendAsync(string interval, DateTime? startDate = null, DateTime? endDate = null);
        
        // Legacy methods for compatibility
        Task<int> GetTotalProductsCountAsync();
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetTotalCustomersCountAsync();
        Task<Dictionary<string, decimal>> GetMonthlySalesAsync(int year);
        Task<Dictionary<string, int>> GetProductCategoriesDistributionAsync();
        Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count = 10);
        Task<IEnumerable<Customer>> GetTopCustomersAsync(int count = 10);
    }
}
