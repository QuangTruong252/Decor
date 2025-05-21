using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using DecorStore.API.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DecorStore.API.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IImageService _imageService;

        // Cache keys
        public const string DASHBOARD_SUMMARY_CACHE_KEY = "DashboardSummary";
        public const string SALES_TREND_CACHE_PREFIX = "SalesTrend_";
        public const string POPULAR_PRODUCTS_CACHE_PREFIX = "PopularProducts_";
        public const string SALES_BY_CATEGORY_CACHE_KEY = "SalesByCategory";
        public const string ORDER_STATUS_DISTRIBUTION_CACHE_KEY = "OrderStatusDistribution";

        public DashboardService(
            IUnitOfWork unitOfWork,
            IDashboardRepository dashboardRepository,
            IMapper mapper,
            IMemoryCache cache,
            IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _dashboardRepository = dashboardRepository;
            _mapper = mapper;
            _cache = cache;
            _imageService = imageService;
        }

        public async Task<DashboardSummaryDTO> GetDashboardSummaryAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue("DashboardSummary", out DashboardSummaryDTO cachedSummary))
            {
                return cachedSummary;
            }

            // If not in cache, generate the summary
            var summary = new DashboardSummaryDTO
            {
                TotalProducts = await _dashboardRepository.GetTotalProductCountAsync(),
                TotalOrders = await _dashboardRepository.GetTotalOrderCountAsync(),
                TotalCustomers = await _dashboardRepository.GetTotalCustomerCountAsync(),
                TotalRevenue = await _dashboardRepository.GetTotalRevenueAsync(),

                // Get recent orders
                RecentOrders = await GetRecentOrdersAsync(5),

                // Get popular products
                PopularProducts = await GetPopularProductsAsync(5),

                // Get sales by category
                SalesByCategory = await GetSalesByCategoryAsync(),

                // Get order status distribution
                OrderStatusDistribution = await GetOrderStatusDistributionAsync(),

                // Get recent sales trend (last 7 days)
                RecentSalesTrend = await GetRecentSalesTrendAsync()
            };

            // Cache the result for 15 minutes
            _cache.Set("DashboardSummary", summary, TimeSpan.FromMinutes(15));

            return summary;
        }

        public async Task<SalesTrendDTO> GetSalesTrendAsync(string period = "daily", DateTime? startDate = null, DateTime? endDate = null)
        {
            // Create cache key based on parameters
            string cacheKey = $"SalesTrend_{period}_{startDate}_{endDate}";

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out SalesTrendDTO cachedTrend))
            {
                return cachedTrend;
            }

            // If not in cache, get the data
            var salesTrendData = await _dashboardRepository.GetSalesTrendAsync(period, startDate, endDate);

            // Map to DTO
            var result = new SalesTrendDTO
            {
                Period = period,
                StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTime.UtcNow,
                Data = salesTrendData.Select(st => new SalesTrendPointDTO
                {
                    Date = st.Date,
                    Revenue = st.Revenue,
                    OrderCount = st.OrderCount
                }).ToList()
            };

            // Cache the result for 15 minutes
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }

        public async Task<List<PopularProductDTO>> GetPopularProductsAsync(int limit = 5)
        {
            // Create cache key based on limit
            string cacheKey = $"PopularProducts_{limit}";

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out List<PopularProductDTO> cachedProducts))
            {
                return cachedProducts;
            }

            // If not in cache, get the data
            var popularProducts = await _dashboardRepository.GetPopularProductsAsync(limit);

            // Map to DTO
            var result = popularProducts.Select(pp => new PopularProductDTO
            {
                ProductId = pp.Product.Id,
                Name = pp.Product.Name,
                ImageUrl = pp.Product.Images.FirstOrDefault()?.FilePath ?? "",
                Price = pp.Product.Price,
                TotalSold = pp.TotalSold,
                TotalRevenue = pp.TotalRevenue
            }).ToList();

            // Cache the result for 15 minutes
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }

        public async Task<List<CategorySalesDTO>> GetSalesByCategoryAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue("SalesByCategory", out List<CategorySalesDTO> cachedCategories))
            {
                return cachedCategories;
            }

            // If not in cache, get the data
            var categorySales = await _dashboardRepository.GetSalesByCategoryAsync();

            // Calculate total revenue for percentage calculation
            decimal totalRevenue = categorySales.Sum(cs => cs.TotalRevenue);

            // Map to DTO
            var result = categorySales.Select(cs => new CategorySalesDTO
            {
                CategoryId = cs.Category.Id,
                CategoryName = cs.Category.Name,
                TotalSales = cs.TotalSales,
                TotalRevenue = cs.TotalRevenue,
                Percentage = totalRevenue > 0 ? Math.Round((cs.TotalRevenue / totalRevenue) * 100, 2) : 0
            }).ToList();

            // Cache the result for 15 minutes
            _cache.Set("SalesByCategory", result, TimeSpan.FromMinutes(15));

            return result;
        }

        public async Task<OrderStatusDistributionDTO> GetOrderStatusDistributionAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue("OrderStatusDistribution", out OrderStatusDistributionDTO cachedDistribution))
            {
                return cachedDistribution;
            }

            // If not in cache, get the data
            var statusDistribution = await _dashboardRepository.GetOrderStatusDistributionAsync();

            // Map to DTO
            var result = new OrderStatusDistributionDTO
            {
                Pending = statusDistribution.GetValueOrDefault("Pending"),
                Processing = statusDistribution.GetValueOrDefault("Processing"),
                Shipped = statusDistribution.GetValueOrDefault("Shipped"),
                Delivered = statusDistribution.GetValueOrDefault("Delivered"),
                Cancelled = statusDistribution.GetValueOrDefault("Cancelled"),
                Total = statusDistribution.Values.Sum()
            };

            // Cache the result for 15 minutes
            _cache.Set("OrderStatusDistribution", result, TimeSpan.FromMinutes(15));

            return result;
        }

        private async Task<List<RecentOrderDTO>> GetRecentOrdersAsync(int count)
        {
            var recentOrders = await _dashboardRepository.GetRecentOrdersAsync(count);

            return recentOrders.Select(o => new RecentOrderDTO
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                CustomerName = o.Customer?.FullName ?? o.User?.Username ?? "Guest",
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus
            }).ToList();
        }

        private async Task<List<SalesTrendPointDTO>> GetRecentSalesTrendAsync()
        {
            // Get sales trend for the last 7 days
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-6);

            var salesTrend = await _dashboardRepository.GetSalesTrendAsync("daily", startDate, endDate);

            return salesTrend.Select(st => new SalesTrendPointDTO
            {
                Date = st.Date,
                Revenue = st.Revenue,
                OrderCount = st.OrderCount
            }).ToList();
        }
    }
}
