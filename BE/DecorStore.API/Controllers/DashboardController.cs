using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard summary data including key metrics, recent orders, popular products, etc.
        /// </summary>
        /// <returns>Dashboard summary data</returns>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDTO>> GetDashboardSummary()
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard summary", error = ex.Message });
            }
        }

        /// <summary>
        /// Get sales trend data over time
        /// </summary>
        /// <param name="period">Period type (daily, weekly, monthly)</param>
        /// <param name="startDate">Start date for the trend data</param>
        /// <param name="endDate">End date for the trend data</param>
        /// <returns>Sales trend data</returns>
        [HttpGet("sales-trend")]
        public async Task<ActionResult<SalesTrendDTO>> GetSalesTrend(
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Validate period
                if (period != "daily" && period != "weekly" && period != "monthly")
                {
                    return BadRequest(new { message = "Invalid period. Valid values are 'daily', 'weekly', or 'monthly'." });
                }

                // Validate date range
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { message = "Start date cannot be after end date." });
                }

                var salesTrend = await _dashboardService.GetSalesTrendAsync(period, startDate, endDate);
                return Ok(salesTrend);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving sales trend data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get popular products with sales metrics
        /// </summary>
        /// <param name="limit">Number of products to return</param>
        /// <returns>List of popular products</returns>
        [HttpGet("popular-products")]
        public async Task<ActionResult<List<PopularProductDTO>>> GetPopularProducts([FromQuery] int limit = 5)
        {
            try
            {
                // Validate limit
                if (limit <= 0 || limit > 50)
                {
                    return BadRequest(new { message = "Limit must be between 1 and 50." });
                }

                var popularProducts = await _dashboardService.GetPopularProductsAsync(limit);
                return Ok(popularProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving popular products", error = ex.Message });
            }
        }

        /// <summary>
        /// Get sales data grouped by product categories
        /// </summary>
        /// <returns>Sales data by category</returns>
        [HttpGet("sales-by-category")]
        public async Task<ActionResult<List<CategorySalesDTO>>> GetSalesByCategory()
        {
            try
            {
                var salesByCategory = await _dashboardService.GetSalesByCategoryAsync();
                return Ok(salesByCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving sales by category", error = ex.Message });
            }
        }

        /// <summary>
        /// Get order status distribution
        /// </summary>
        /// <returns>Count of orders in each status</returns>
        [HttpGet("order-status-distribution")]
        public async Task<ActionResult<OrderStatusDistributionDTO>> GetOrderStatusDistribution()
        {
            try
            {
                var statusDistribution = await _dashboardService.GetOrderStatusDistributionAsync();
                return Ok(statusDistribution);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving order status distribution", error = ex.Message });
            }
        }
    }
}
