using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Services;
using DecorStore.API.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : base(logger)
        {
            _dashboardService = dashboardService;
        }        /// <summary>
        /// Get dashboard summary data including key metrics, recent orders, popular products, etc.
        /// </summary>
        /// <returns>Dashboard summary data</returns>
        [HttpGet("summary")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public async Task<ActionResult<DashboardSummaryDTO>> GetDashboardSummary()
        {
            var result = await _dashboardService.GetDashboardSummaryAsync();
            return HandleResult(result);
        }        /// <summary>
        /// Get sales trend data over time
        /// </summary>
        /// <param name="period">Period type (daily, weekly, monthly)</param>
        /// <param name="startDate">Start date for the trend data</param>
        /// <param name="endDate">End date for the trend data</param>
        /// <returns>Sales trend data</returns>
        [HttpGet("sales-trend")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Client, VaryByQueryKeys = new[] { "period", "startDate", "endDate" })]
        public async Task<ActionResult<SalesTrendDTO>> GetSalesTrend(
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _dashboardService.GetSalesTrendAsync(period, startDate, endDate);
            return HandleResult(result);
        }        /// <summary>
        /// Get popular products with sales metrics
        /// </summary>
        /// <param name="limit">Number of products to return</param>
        /// <returns>List of popular products</returns>
        [HttpGet("popular-products")]
        [ResponseCache(Duration = 900, Location = ResponseCacheLocation.Client, VaryByQueryKeys = new[] { "limit" })]
        public async Task<ActionResult<List<PopularProductDTO>>> GetPopularProducts([FromQuery] int limit = 5)
        {
            var result = await _dashboardService.GetPopularProductsAsync(limit);
            return HandleResult(result);
        }

        /// <summary>
        /// Get sales data grouped by product categories
        /// </summary>
        /// <returns>Sales data by category</returns>
        [HttpGet("sales-by-category")]
        public async Task<ActionResult<List<CategorySalesDTO>>> GetSalesByCategory()
        {
            var result = await _dashboardService.GetSalesByCategoryAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Get order status distribution
        /// </summary>
        /// <returns>Count of orders in each status</returns>
        [HttpGet("order-status-distribution")]
        public async Task<ActionResult<OrderStatusDistributionDTO>> GetOrderStatusDistribution()
        {
            var result = await _dashboardService.GetOrderStatusDistributionAsync();
            return HandleResult(result);
        }
    }
}
