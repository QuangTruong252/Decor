using Microsoft.AspNetCore.Mvc;
using DecorStore.API.DTOs;
using DecorStore.API.Common;
using DecorStore.API.Controllers.Base;
using DecorStore.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using DatabaseMetrics = DecorStore.API.DTOs.DatabaseMetrics;

namespace DecorStore.API.Controllers
{
    /// <summary>
    /// Controller for performance monitoring and dashboard
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class PerformanceDashboardController : BaseController
    {
        private readonly ILogger<PerformanceDashboardController> _logger;
        private readonly IPerformanceDashboardService _performanceService;

        public PerformanceDashboardController(
            ILogger<PerformanceDashboardController> logger,
            IPerformanceDashboardService performanceService) : base(logger)
        {
            _logger = logger;
            _performanceService = performanceService;
        }

        /// <summary>
        /// Get real-time performance dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(PerformanceDashboardDTO), 200)]
        public async Task<ActionResult<PerformanceDashboardDTO>> GetPerformanceDashboard()
        {
            try
            {
                var result = await _performanceService.GetPerformanceDashboardAsync();
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance dashboard");
                return StatusCode(500, "An error occurred while retrieving performance data");
            }
        }        /// <summary>
        /// Get database performance metrics
        /// </summary>
        [HttpGet("database")]
        [ProducesResponseType(typeof(DatabasePerformanceDTO), 200)]
        public async Task<ActionResult<DatabasePerformanceDTO>> GetDatabaseMetrics()
        {
            try
            {
                var result = await _performanceService.GetDatabaseMetricsAsync();
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database metrics");
                return StatusCode(500, "An error occurred while retrieving database metrics");
            }
        }        /// <summary>
        /// Get performance trends over time
        /// </summary>
        [HttpGet("trends")]
        [ProducesResponseType(typeof(PerformanceTrendsDTO), 200)]
        public async Task<ActionResult<PerformanceTrendsDTO>> GetPerformanceTrends(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string metric = "ResponseTime"){
            try
            {
                var actualStartDate = startDate ?? DateTime.UtcNow.AddDays(-30);
                var actualEndDate = endDate ?? DateTime.UtcNow;
                var result = await _performanceService.GetPerformanceTrendsAsync(actualStartDate, actualEndDate, metric);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance trends");
                return StatusCode(500, "An error occurred while retrieving performance trends");
            }
        }

        /// <summary>
        /// Get system resource utilization
        /// </summary>
        [HttpGet("resources")]
        [ProducesResponseType(typeof(ResourceUtilizationDTO), 200)]
        public async Task<ActionResult<ResourceUtilizationDTO>> GetResourceUtilization()
        {
            try
            {
                var result = await _performanceService.GetResourceUtilizationAsync();
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource utilization");
                return StatusCode(500, "An error occurred while retrieving resource utilization");
            }
        }        /// <summary>
        /// Get cache performance metrics
        /// </summary>
        [HttpGet("cache")]
        [ProducesResponseType(typeof(CachePerformanceDTO), 200)]
        public async Task<ActionResult<CachePerformanceDTO>> GetCacheMetrics()
        {
            try
            {
                var result = await _performanceService.GetCacheMetricsAsync();
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache metrics");
                return StatusCode(500, "An error occurred while retrieving cache metrics");
            }
        }
    }
}
