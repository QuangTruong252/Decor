using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using DecorStore.API.Common;
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

        public async Task<Result<DashboardSummaryDTO>> GetDashboardSummaryAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue("DashboardSummary", out DashboardSummaryDTO cachedSummary))
                {
                    return Result<DashboardSummaryDTO>.Success(cachedSummary);
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
                    PopularProducts = (await GetPopularProductsAsync(5)).Data ?? new List<PopularProductDTO>(),

                    // Get sales by category
                    SalesByCategory = (await GetSalesByCategoryAsync()).Data ?? new List<CategorySalesDTO>(),

                    // Get order status distribution
                    OrderStatusDistribution = (await GetOrderStatusDistributionAsync()).Data ?? new OrderStatusDistributionDTO(),

                    // Get recent sales trend (last 7 days)
                    RecentSalesTrend = await GetRecentSalesTrendAsync()
                };

                // Cache the result for 15 minutes
                _cache.Set("DashboardSummary", summary, TimeSpan.FromMinutes(15));

                return Result<DashboardSummaryDTO>.Success(summary);
            }
            catch (Exception ex)
            {
                return Result<DashboardSummaryDTO>.Failure($"Failed to get dashboard summary: {ex.Message}", "DASHBOARD_ERROR");
            }
        }

        public async Task<Result<SalesTrendDTO>> GetSalesTrendAsync(string period = "daily", DateTime? startDate = null, DateTime? endDate = null)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(period))
            {
                return Result<SalesTrendDTO>.Failure("Period is required", "INVALID_INPUT");
            }

            if (!new[] { "daily", "weekly", "monthly" }.Contains(period.ToLower()))
            {
                return Result<SalesTrendDTO>.Failure("Invalid period. Valid values are 'daily', 'weekly', or 'monthly'", "INVALID_PERIOD");
            }

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return Result<SalesTrendDTO>.Failure("Start date cannot be after end date", "INVALID_DATE_RANGE");
            }

            try
            {
                // Create cache key based on parameters
                string cacheKey = $"SalesTrend_{period}_{startDate}_{endDate}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out SalesTrendDTO cachedTrend))
                {
                    return Result<SalesTrendDTO>.Success(cachedTrend);
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
                        Date = DateTime.Parse(st.Key),
                        Revenue = st.Value,
                        OrderCount = 0 // Will need to be calculated separately if needed
                    }).ToList()
                };

                // Cache the result for 15 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

                return Result<SalesTrendDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<SalesTrendDTO>.Failure($"Failed to get sales trend: {ex.Message}", "SALES_TREND_ERROR");
            }
        }

        public async Task<Result<List<PopularProductDTO>>> GetPopularProductsAsync(int limit = 5)
        {
            // Input validation
            if (limit <= 0 || limit > 50)
            {
                return Result<List<PopularProductDTO>>.Failure("Limit must be between 1 and 50", "INVALID_LIMIT");
            }

            try
            {
                // Create cache key based on limit
                string cacheKey = $"PopularProducts_{limit}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out List<PopularProductDTO> cachedProducts))
                {
                    return Result<List<PopularProductDTO>>.Success(cachedProducts);
                }

                // If not in cache, get the data
                var popularProducts = await _dashboardRepository.GetPopularProductsAsync(limit);

                // Map to DTO
                var result = popularProducts.Select(p => new PopularProductDTO
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ProductImages.FirstOrDefault()?.Image?.FilePath ?? "",
                    Price = p.Price,
                    TotalSold = p.OrderItems?.Count ?? 0,
                    TotalRevenue = p.OrderItems?.Sum(oi => oi.UnitPrice * oi.Quantity) ?? 0
                }).ToList();

                // Cache the result for 15 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));

                return Result<List<PopularProductDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<PopularProductDTO>>.Failure($"Failed to get popular products: {ex.Message}", "POPULAR_PRODUCTS_ERROR");
            }
        }

        public async Task<Result<List<CategorySalesDTO>>> GetSalesByCategoryAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue("SalesByCategory", out List<CategorySalesDTO> cachedCategories))
                {
                    return Result<List<CategorySalesDTO>>.Success(cachedCategories);
                }

                // If not in cache, get the data
                var categorySales = await _dashboardRepository.GetSalesByCategoryAsync();

                // Calculate total revenue for percentage calculation
                decimal totalRevenue = categorySales.Values.Sum();

                // Map to DTO
                var result = categorySales.Select(cs => new CategorySalesDTO
                {
                    CategoryId = 0, // Will need to be populated differently
                    CategoryName = cs.Key,
                    TotalSales = 0, // Will need to be calculated differently  
                    TotalRevenue = cs.Value,
                    Percentage = totalRevenue > 0 ? Math.Round((cs.Value / totalRevenue) * 100, 2) : 0
                }).ToList();

                // Cache the result for 15 minutes
                _cache.Set("SalesByCategory", result, TimeSpan.FromMinutes(15));

                return Result<List<CategorySalesDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<CategorySalesDTO>>.Failure($"Failed to get sales by category: {ex.Message}", "CATEGORY_SALES_ERROR");
            }
        }

        public async Task<Result<OrderStatusDistributionDTO>> GetOrderStatusDistributionAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue("OrderStatusDistribution", out OrderStatusDistributionDTO cachedDistribution))
                {
                    return Result<OrderStatusDistributionDTO>.Success(cachedDistribution);
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

                return Result<OrderStatusDistributionDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<OrderStatusDistributionDTO>.Failure($"Failed to get order status distribution: {ex.Message}", "ORDER_STATUS_ERROR");
            }
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
                Date = DateTime.Parse(st.Key),
                Revenue = st.Value,
                OrderCount = 0 // Will need to be calculated separately if needed
            }).ToList();
        }
    }
}
