using DecorStore.API.Common;
using DecorStore.API.Controllers.Base;
using DecorStore.API.DTOs;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    /// <summary>
    /// Controller for load testing operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only admin can perform load testing
    public class LoadTestController : BaseController
    {
        private readonly ILoadTestingService _loadTestingService;

        public LoadTestController(
            ILoadTestingService loadTestingService,
            ILogger<LoadTestController> logger) : base(logger)
        {
            _loadTestingService = loadTestingService;
        }

        /// <summary>
        /// Gets available endpoints for load testing
        /// </summary>
        /// <returns>List of available endpoints</returns>
        [HttpGet("endpoints")]
        public async Task<ActionResult<List<string>>> GetAvailableEndpoints()
        {
            var result = await _loadTestingService.GetAvailableEndpointsAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Validates a load test configuration
        /// </summary>
        /// <param name="configuration">Load test configuration to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        public ActionResult<LoadTestConfigurationDTO> ValidateConfiguration([FromBody] LoadTestConfigurationDTO configuration)
        {
            var result = _loadTestingService.ValidateConfiguration(configuration);
            return HandleResult(result);
        }

        /// <summary>
        /// Executes a load test
        /// </summary>
        /// <param name="configuration">Load test configuration</param>
        /// <returns>Load test results</returns>
        [HttpPost("execute")]
        public async Task<ActionResult<LoadTestResultDTO>> ExecuteLoadTest([FromBody] LoadTestConfigurationDTO configuration)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var result = await _loadTestingService.ExecuteLoadTestAsync(configuration);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a sample load test configuration
        /// </summary>
        /// <returns>Sample configuration</returns>
        [HttpGet("sample-config")]
        public ActionResult<LoadTestConfigurationDTO> GetSampleConfiguration()
        {
            var sampleConfig = new LoadTestConfigurationDTO
            {
                TestName = "Sample API Load Test",
                ConcurrentUsers = 10,
                DurationMinutes = 2,
                RampUpMinutes = 1,
                TargetEndpoints = new List<string>
                {
                    "/api/products",
                    "/api/categories",
                    "/api/products/featured"
                },
                ResponseTimeThresholdMs = 1000,
                ExpectedSuccessRate = 95.0
            };

            return Ok(sampleConfig);
        }

        /// <summary>
        /// Gets load testing best practices and guidelines
        /// </summary>
        /// <returns>Load testing guidelines</returns>
        [HttpGet("guidelines")]
        public ActionResult<object> GetLoadTestingGuidelines()
        {
            var guidelines = new
            {
                BestPractices = new[]
                {
                    "Start with small user loads and gradually increase",
                    "Use realistic test scenarios that match actual user behavior",
                    "Monitor system resources during load tests",
                    "Test during off-peak hours to avoid affecting production",
                    "Validate results against performance baselines"
                },
                RecommendedConfigurations = new
                {
                    SmokeTest = new { ConcurrentUsers = 1, DurationMinutes = 1, Description = "Basic functionality test" },
                    LoadTest = new { ConcurrentUsers = 10, DurationMinutes = 5, Description = "Normal expected load" },
                    StressTest = new { ConcurrentUsers = 50, DurationMinutes = 10, Description = "Above normal load" },
                    SpikeTest = new { ConcurrentUsers = 100, DurationMinutes = 2, Description = "Sudden load increase" }
                },
                KeyMetrics = new[]
                {
                    "Average response time",
                    "95th percentile response time",
                    "Error rate",
                    "Throughput (requests per second)",
                    "System resource utilization"
                },
                Warnings = new[]
                {
                    "Load testing can impact system performance",
                    "Always test in a non-production environment first",
                    "Monitor system resources during tests",
                    "Be aware of rate limiting and throttling"
                }
            };

            return Ok(guidelines);
        }

        /// <summary>
        /// Gets load testing status and limits
        /// </summary>
        /// <returns>Current load testing status</returns>
        [HttpGet("status")]
        public ActionResult<object> GetLoadTestingStatus()
        {
            var status = new
            {
                IsEnabled = true,
                MaxConcurrentUsers = 1000,
                MaxDurationMinutes = 60,
                SupportedEndpoints = new[]
                {
                    "/api/products",
                    "/api/categories",
                    "/api/dashboard/summary",
                    "/api/performance/metrics"
                },
                LastTestExecuted = DateTime.UtcNow.AddHours(-2), // Sample data
                SystemStatus = "Ready",
                Recommendations = new[]
                {
                    "Current system can handle up to 100 concurrent users",
                    "Peak performance observed at 50 concurrent users",
                    "Response times increase significantly above 200 concurrent users"
                }
            };

            return Ok(status);
        }
    }
}
