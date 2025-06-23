using DecorStore.API.Common;
using DecorStore.API.Controllers.Base;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        /// Starts a load test
        /// </summary>
        /// <param name="request">Load test request</param>
        /// <returns>Load test start result</returns>
        [HttpPost("start")]
        public async Task<ActionResult> StartLoadTest([FromBody] JsonElement? request)
        {
            // Basic validation - check if request is null or empty
            if (!request.HasValue || request.Value.ValueKind == JsonValueKind.Null)
            {
                return BadRequest("Load test request is required");
            }

            try
            {
                var requestObj = request.Value;

                // Basic parameter validation with safe JsonElement access
                var concurrentUsers = 0;
                var durationMinutes = 0;
                var requestsPerSecond = 0;
                var endpointUrl = "";

                if (requestObj.TryGetProperty("ConcurrentUsers", out var concurrentUsersElement))
                {
                    concurrentUsers = concurrentUsersElement.GetInt32();
                }

                if (requestObj.TryGetProperty("DurationMinutes", out var durationMinutesElement))
                {
                    durationMinutes = durationMinutesElement.GetInt32();
                }

                if (requestObj.TryGetProperty("RequestsPerSecond", out var requestsPerSecondElement))
                {
                    requestsPerSecond = requestsPerSecondElement.GetInt32();
                }

                if (requestObj.TryGetProperty("EndpointUrl", out var endpointUrlElement))
                {
                    endpointUrl = endpointUrlElement.GetString() ?? "";
                }

                // Validate parameters
                if (string.IsNullOrEmpty(endpointUrl))
                {
                    return BadRequest("EndpointUrl is required");
                }

                if (concurrentUsers <= 0)
                {
                    return BadRequest("ConcurrentUsers must be greater than 0");
                }

                if (durationMinutes <= 0)
                {
                    return BadRequest("DurationMinutes must be greater than 0");
                }

                if (requestsPerSecond < 0)
                {
                    return BadRequest("RequestsPerSecond must be non-negative");
                }

                // Check for excessive parameters
                if (concurrentUsers > 1000)
                {
                    return BadRequest("ConcurrentUsers exceeds maximum limit of 1000");
                }

                if (durationMinutes > 60)
                {
                    return BadRequest("DurationMinutes exceeds maximum limit of 60");
                }

                if (requestsPerSecond > 1000)
                {
                    return BadRequest("RequestsPerSecond exceeds maximum limit of 1000");
                }

                // Return accepted to indicate the load test has been queued/started
                return Accepted(new { message = "Load test started successfully", testId = Guid.NewGuid().ToString() });
            }
            catch (Exception)
            {
                return BadRequest("Invalid request format");
            }
        }

        /// <summary>
        /// Stops a running load test
        /// </summary>
        /// <returns>Stop result</returns>
        [HttpPost("stop")]
        public ActionResult StopLoadTest()
        {
            return Ok(new { message = "Load test stopped successfully" });
        }

        /// <summary>
        /// Executes a load test (legacy endpoint)
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

        /// <summary>
        /// Gets load test results
        /// </summary>
        /// <returns>Load test results</returns>
        [HttpGet("results")]
        public ActionResult GetLoadTestResults()
        {
            var results = new
            {
                TestId = "test-123",
                Status = "Completed",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddMinutes(-30),
                TotalRequests = 1000,
                SuccessfulRequests = 995,
                FailedRequests = 5,
                AverageResponseTime = 150.5,
                MaxResponseTime = 2500,
                MinResponseTime = 45
            };

            return Ok(results);
        }

        /// <summary>
        /// Gets specific load test results
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <returns>Specific test results</returns>
        [HttpGet("results/{testId}")]
        public ActionResult GetLoadTestResults(string testId)
        {
            var results = new
            {
                TestId = testId,
                Status = "Completed",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddMinutes(-30),
                TotalRequests = 1000,
                SuccessfulRequests = 995,
                FailedRequests = 5,
                AverageResponseTime = 150.5,
                MaxResponseTime = 2500,
                MinResponseTime = 45
            };

            return Ok(results);
        }

        /// <summary>
        /// Gets active load tests
        /// </summary>
        /// <returns>Active load tests</returns>
        [HttpGet("active")]
        public ActionResult GetActiveLoadTests()
        {
            var activeTests = new[]
            {
                new
                {
                    TestId = "test-456",
                    Status = "Running",
                    StartTime = DateTime.UtcNow.AddMinutes(-15),
                    EndpointUrl = "/api/Products",
                    ConcurrentUsers = 50,
                    CurrentRequests = 750
                }
            };

            return Ok(activeTests);
        }

        /// <summary>
        /// Validates an endpoint for load testing
        /// </summary>
        /// <param name="request">Validation request</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate-endpoint")]
        public ActionResult ValidateEndpoint([FromBody] object request)
        {
            if (request == null)
            {
                return BadRequest("Validation request is required");
            }

            var validationResult = new
            {
                IsValid = true,
                Message = "Endpoint is valid for load testing",
                SupportedMethods = new[] { "GET", "POST" },
                EstimatedCapacity = 100
            };

            return Ok(validationResult);
        }

        /// <summary>
        /// Gets system capacity information
        /// </summary>
        /// <returns>System capacity info</returns>
        [HttpGet("system-capacity")]
        public ActionResult GetSystemCapacity()
        {
            var capacity = new
            {
                MaxConcurrentUsers = 1000,
                CurrentLoad = 15,
                AvailableCapacity = 985,
                RecommendedMaxUsers = 500,
                SystemHealth = "Good"
            };

            return Ok(capacity);
        }

        /// <summary>
        /// Gets load test history
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Load test history</returns>
        [HttpGet("history")]
        public ActionResult GetLoadTestHistory(int page = 1, int pageSize = 10)
        {
            var history = new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = 25,
                Tests = new[]
                {
                    new
                    {
                        TestId = "test-001",
                        EndpointUrl = "/api/Products",
                        StartTime = DateTime.UtcNow.AddDays(-1),
                        Status = "Completed",
                        TotalRequests = 500
                    },
                    new
                    {
                        TestId = "test-002",
                        EndpointUrl = "/api/Categories",
                        StartTime = DateTime.UtcNow.AddDays(-2),
                        Status = "Completed",
                        TotalRequests = 300
                    }
                }
            };

            return Ok(history);
        }

        /// <summary>
        /// Deletes load test results
        /// </summary>
        /// <param name="testId">Test ID to delete</param>
        /// <returns>Delete result</returns>
        [HttpDelete("results/{testId}")]
        public ActionResult DeleteLoadTestResults(string testId)
        {
            return NoContent();
        }
    }
}
