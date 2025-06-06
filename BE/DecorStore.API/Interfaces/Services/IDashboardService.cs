using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Common;

namespace DecorStore.API.Services
{
    public interface IDashboardService
    {
        // Get dashboard summary data
        Task<Result<DashboardSummaryDTO>> GetDashboardSummaryAsync();
        
        // Get sales trend data
        Task<Result<SalesTrendDTO>> GetSalesTrendAsync(string period = "daily", DateTime? startDate = null, DateTime? endDate = null);
        
        // Get popular products
        Task<Result<List<PopularProductDTO>>> GetPopularProductsAsync(int limit = 5);
        
        // Get sales by category
        Task<Result<List<CategorySalesDTO>>> GetSalesByCategoryAsync();
        
        // Get order status distribution
        Task<Result<OrderStatusDistributionDTO>> GetOrderStatusDistributionAsync();
    }
}
