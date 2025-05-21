using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;

namespace DecorStore.API.Services
{
    public interface IDashboardService
    {
        // Get dashboard summary data
        Task<DashboardSummaryDTO> GetDashboardSummaryAsync();
        
        // Get sales trend data
        Task<SalesTrendDTO> GetSalesTrendAsync(string period = "daily", DateTime? startDate = null, DateTime? endDate = null);
        
        // Get popular products
        Task<List<PopularProductDTO>> GetPopularProductsAsync(int limit = 5);
        
        // Get sales by category
        Task<List<CategorySalesDTO>> GetSalesByCategoryAsync();
        
        // Get order status distribution
        Task<OrderStatusDistributionDTO> GetOrderStatusDistributionAsync();
    }
}
