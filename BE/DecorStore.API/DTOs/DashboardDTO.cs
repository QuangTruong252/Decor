using System;
using System.Collections.Generic;

namespace DecorStore.API.DTOs
{
    // Summary dashboard data
    public class DashboardSummaryDTO
    {
        // Key metrics
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // Recent orders
        public List<RecentOrderDTO> RecentOrders { get; set; } = new();
        
        // Popular products
        public List<PopularProductDTO> PopularProducts { get; set; } = new();
        
        // Sales by category
        public List<CategorySalesDTO> SalesByCategory { get; set; } = new();
        
        // Order status distribution
        public OrderStatusDistributionDTO OrderStatusDistribution { get; set; } = new();
        
        // Sales trend (last 7 days)
        public List<SalesTrendPointDTO> RecentSalesTrend { get; set; } = new();
    }
    
    // Sales trend data
    public class SalesTrendDTO
    {
        public string Period { get; set; } = "daily"; // daily, weekly, monthly
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SalesTrendPointDTO> Data { get; set; } = new();
    }
    
    // Individual data point for sales trend
    public class SalesTrendPointDTO
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
    
    // Popular product data
    public class PopularProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
    
    // Sales by category data
    public class CategorySalesDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Percentage { get; set; } // Percentage of total sales
    }
    
    // Order status distribution data
    public class OrderStatusDistributionDTO
    {
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Shipped { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
        public int Total { get; set; }
    }
    
    // Recent order data for dashboard
    public class RecentOrderDTO
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
    }
}
