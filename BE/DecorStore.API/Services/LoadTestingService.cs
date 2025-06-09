using DecorStore.API.Common;
using DecorStore.API.DTOs;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service interface for load testing functionality
    /// </summary>
    public interface ILoadTestingService
    {
        /// <summary>
        /// Executes a load test based on configuration
        /// </summary>
        Task<Result<LoadTestResultDTO>> ExecuteLoadTestAsync(LoadTestConfigurationDTO configuration);

        /// <summary>
        /// Gets available endpoints for testing
        /// </summary>
        Task<Result<List<string>>> GetAvailableEndpointsAsync();

        /// <summary>
        /// Validates load test configuration
        /// </summary>
        Result<LoadTestConfigurationDTO> ValidateConfiguration(LoadTestConfigurationDTO configuration);
    }

    /// <summary>
    /// Implementation of load testing service
    /// </summary>
    public class LoadTestingService : ILoadTestingService
    {
        private readonly ILogger<LoadTestingService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoadTestingService(
            ILogger<LoadTestingService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<Result<LoadTestResultDTO>> ExecuteLoadTestAsync(LoadTestConfigurationDTO configuration)
        {
            try
            {
                // Validate configuration
                var validationResult = ValidateConfiguration(configuration);
                if (!validationResult.IsSuccess)
                {
                    return Result<LoadTestResultDTO>.Failure(validationResult.Error);
                }

                var validConfig = validationResult.Data;
                var startTime = DateTime.UtcNow;
                
                _logger.LogInformation("Starting load test: {TestName} with {ConcurrentUsers} users for {Duration} minutes",
                    validConfig.TestName, validConfig.ConcurrentUsers, validConfig.DurationMinutes);

                var results = new ConcurrentBag<RequestResult>();
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(validConfig.DurationMinutes));
                var semaphore = new SemaphoreSlim(validConfig.ConcurrentUsers);

                // Create tasks for concurrent users
                var tasks = new List<Task>();
                
                for (int i = 0; i < validConfig.ConcurrentUsers; i++)
                {
                    var userId = i;
                    tasks.Add(Task.Run(async () =>
                    {
                        // Ramp-up delay
                        var rampUpDelay = (validConfig.RampUpMinutes * 60 * 1000) / validConfig.ConcurrentUsers * userId;
                        if (rampUpDelay > 0)
                        {
                            await Task.Delay(rampUpDelay, cancellationTokenSource.Token);
                        }

                        await ExecuteUserRequestsAsync(validConfig, results, semaphore, cancellationTokenSource.Token);
                    }, cancellationTokenSource.Token));
                }

                // Wait for all tasks to complete or timeout
                await Task.WhenAll(tasks);

                var endTime = DateTime.UtcNow;
                var testResults = results.ToList();

                // Calculate metrics
                var loadTestResult = CalculateResults(validConfig, testResults, startTime, endTime);

                _logger.LogInformation("Load test completed: {TestName}. Total requests: {TotalRequests}, Success rate: {SuccessRate}%",
                    validConfig.TestName, loadTestResult.TotalRequests, loadTestResult.SuccessRate);

                return Result<LoadTestResultDTO>.Success(loadTestResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing load test");
                return Result<LoadTestResultDTO>.Failure("Failed to execute load test: " + ex.Message);
            }
        }

        public async Task<Result<List<string>>> GetAvailableEndpointsAsync()
        {
            try
            {
                // Define commonly tested endpoints
                var endpoints = new List<string>
                {
                    "/api/products",
                    "/api/products/featured",
                    "/api/categories",
                    "/api/dashboard/summary",
                    "/api/performance/metrics"
                };

                return Result<List<string>>.Success(endpoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available endpoints");
                return Result<List<string>>.Failure("Failed to get available endpoints");
            }
        }

        public Result<LoadTestConfigurationDTO> ValidateConfiguration(LoadTestConfigurationDTO configuration)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(configuration.TestName))
                    errors.Add("Test name is required");

                if (configuration.ConcurrentUsers <= 0 || configuration.ConcurrentUsers > 1000)
                    errors.Add("Concurrent users must be between 1 and 1000");

                if (configuration.DurationMinutes <= 0 || configuration.DurationMinutes > 60)
                    errors.Add("Duration must be between 1 and 60 minutes");

                if (configuration.RampUpMinutes < 0 || configuration.RampUpMinutes >= configuration.DurationMinutes)
                    errors.Add("Ramp-up time must be less than test duration");

                if (!configuration.TargetEndpoints.Any())
                    errors.Add("At least one target endpoint is required");

                if (configuration.ResponseTimeThresholdMs <= 0)
                    errors.Add("Response time threshold must be greater than 0");

                if (configuration.ExpectedSuccessRate <= 0 || configuration.ExpectedSuccessRate > 100)
                    errors.Add("Expected success rate must be between 0 and 100");

                if (errors.Any())
                {
                    return Result<LoadTestConfigurationDTO>.Failure(string.Join("; ", errors));
                }

                return Result<LoadTestConfigurationDTO>.Success(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating load test configuration");
                return Result<LoadTestConfigurationDTO>.Failure("Failed to validate configuration");
            }
        }

        private async Task ExecuteUserRequestsAsync(
            LoadTestConfigurationDTO config,
            ConcurrentBag<RequestResult> results,
            SemaphoreSlim semaphore,
            CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var random = new Random();
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7001";

            while (!cancellationToken.IsCancellationRequested)
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var endpoint = config.TargetEndpoints[random.Next(config.TargetEndpoints.Count)];
                    var url = $"{baseUrl.TrimEnd('/')}{endpoint}";

                    var stopwatch = Stopwatch.StartNew();
                    var requestResult = new RequestResult
                    {
                        Endpoint = endpoint,
                        StartTime = DateTime.UtcNow
                    };

                    try
                    {
                        var response = await httpClient.GetAsync(url, cancellationToken);
                        stopwatch.Stop();

                        requestResult.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                        requestResult.StatusCode = (int)response.StatusCode;
                        requestResult.IsSuccess = response.IsSuccessStatusCode;
                        requestResult.EndTime = DateTime.UtcNow;

                        results.Add(requestResult);
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        requestResult.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                        requestResult.IsSuccess = false;
                        requestResult.Error = ex.Message;
                        requestResult.EndTime = DateTime.UtcNow;

                        results.Add(requestResult);
                    }

                    // Small delay between requests to avoid overwhelming the server
                    await Task.Delay(random.Next(100, 1000), cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        private LoadTestResultDTO CalculateResults(
            LoadTestConfigurationDTO config,
            List<RequestResult> results,
            DateTime startTime,
            DateTime endTime)
        {
            var totalRequests = results.Count;
            var successfulRequests = results.Count(r => r.IsSuccess);
            var failedRequests = totalRequests - successfulRequests;
            var successRate = totalRequests > 0 ? (double)successfulRequests / totalRequests * 100 : 0;

            var responseTimes = results.Where(r => r.IsSuccess).Select(r => r.ResponseTimeMs).ToList();
            var averageResponseTime = responseTimes.Any() ? responseTimes.Average() : 0;

            var p95ResponseTime = responseTimes.Any() ? CalculatePercentile(responseTimes, 0.95) : 0;
            var p99ResponseTime = responseTimes.Any() ? CalculatePercentile(responseTimes, 0.99) : 0;

            var duration = endTime - startTime;
            var requestsPerSecond = duration.TotalSeconds > 0 ? totalRequests / duration.TotalSeconds : 0;

            var regressionDetected = 
                averageResponseTime > config.ResponseTimeThresholdMs ||
                successRate < config.ExpectedSuccessRate;

            var errors = results
                .Where(r => !r.IsSuccess && !string.IsNullOrEmpty(r.Error))
                .GroupBy(r => r.Error)
                .Select(g => $"{g.Key} (Count: {g.Count()})")
                .ToList();

            return new LoadTestResultDTO
            {
                Configuration = config,
                TotalRequests = totalRequests,
                SuccessfulRequests = successfulRequests,
                FailedRequests = failedRequests,
                SuccessRate = successRate,
                AverageResponseTimeMs = averageResponseTime,
                P95ResponseTimeMs = p95ResponseTime,
                P99ResponseTimeMs = p99ResponseTime,
                RequestsPerSecond = requestsPerSecond,
                StartTime = startTime,
                EndTime = endTime,
                RegressionDetected = regressionDetected,
                Errors = errors
            };
        }

        private double CalculatePercentile(List<long> values, double percentile)
        {
            if (!values.Any()) return 0;

            var sorted = values.OrderBy(x => x).ToList();
            var index = (int)Math.Ceiling(sorted.Count * percentile) - 1;
            index = Math.Max(0, Math.Min(index, sorted.Count - 1));
            
            return sorted[index];
        }
    }

    /// <summary>
    /// Internal class for tracking individual request results
    /// </summary>
    internal class RequestResult
    {
        public string Endpoint { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long ResponseTimeMs { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
    }
}
