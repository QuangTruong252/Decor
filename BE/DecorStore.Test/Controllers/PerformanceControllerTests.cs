using Microsoft.AspNetCore.Http;
using System.Net;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class PerformanceControllerTests : TestBase
    {
        [Fact]
        public async Task GetSystemMetrics_WithAdminAuth_ShouldReturnMetrics()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/system");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("memory");
        }

        [Fact]
        public async Task GetSystemMetrics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/system");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCacheMetrics_WithAdminAuth_ShouldReturnMetrics()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/cache");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("hitRatio");
        }

        [Fact]
        public async Task GetCacheMetrics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/cache");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRedisMetrics_WithAdminAuth_ShouldReturnMetrics()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/redis");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("isConnected");
        }

        [Fact]
        public async Task GetRedisMetrics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/redis");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPerformanceDashboard_WithAdminAuth_ShouldReturnDashboard()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("Cache");
            content.Should().Contain("System");
            content.Should().Contain("Redis");
        }

        [Fact]
        public async Task GetPerformanceDashboard_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDatabaseMetrics_WithAdminAuth_ShouldReturnMetrics()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/database");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetDatabaseMetrics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/database");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetHealthCheck_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("status"); // API uses camelCase JSON serialization
        }

        [Fact]
        public async Task GetAuthTest_WithValidToken_ShouldReturnAuthInfo()
        {
            // Arrange
            var token = await GetAdminTokenAsync();
            token.Should().NotBeNull("Admin token should be generated successfully");

            // CRITICAL FIX: Use a fresh HttpClient instance to avoid any middleware interference
            using var freshClient = _factory.CreateClient();
            freshClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"[TEST] Making request with token: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
            Console.WriteLine($"[TEST] Authorization header: {freshClient.DefaultRequestHeaders.Authorization}");

            // Act - Use simple GET method to avoid any HttpRequestMessage complications
            var response = await freshClient.GetAsync("/api/Performance/auth-test");

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST] Response Status: {response.StatusCode}");
            Console.WriteLine($"[TEST] Response Content: {content}");

            response.StatusCode.Should().Be(HttpStatusCode.OK, "Authentication should succeed with valid JWT token");

            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("isAuthenticated", "Response should indicate user is authenticated");
            content.Should().Contain("truongadmin", "Response should contain the authenticated username");
            content.Should().Contain("Admin", "Response should contain the user's admin role");
        }

        [Fact]
        public async Task GetCacheStatistics_WithAdminAuth_ShouldReturnStatistics()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/cache/statistics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("hitRatio"); // JSON uses camelCase naming policy
        }

        [Fact]
        public async Task GetCacheKeys_WithAdminAuth_ShouldReturnKeys()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/cache/keys");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ClearCache_WithAdminAuth_ShouldClearCache()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.PostAsync("/api/Performance/cache/clear", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Cache cleared successfully");
        }

        [Fact]
        public async Task ClearCacheByPrefix_WithAdminAuth_ShouldClearCacheByPrefix()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.PostAsync("/api/Performance/cache/clear/products", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Cache cleared for prefix 'products' successfully");
        }

        [Fact]
        public async Task GetMemoryMetrics_WithAdminAuth_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/memory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetApiMetrics_WithAdminAuth_ShouldReturnMetrics_CorrectEndpoint()
        {
            // Arrange
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/api");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("totalRequests");
            content.Should().Contain("requestsPerSecond");
        }

        [Fact]
        public async Task GetRequestMetrics_WithAdminAuth_ShouldReturnMetrics_CorrectEndpoint()
        {
            // Arrange
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/requests");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("totalRequests");
            content.Should().Contain("requestsPerMinute");
        }

        [Fact]
        public async Task GetThreadPoolMetrics_WithAdminAuth_ShouldReturnMetrics_CorrectEndpoint()
        {
            // Arrange
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/threads");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("processThreadCount");
            content.Should().Contain("threadPoolWorkerThreads");
        }

        [Fact]
        public async Task GetAuthTest_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/Performance/auth-test");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "Request without token should be unauthorized");
        }

        [Fact]
        public async Task GetAuthTest_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.GetAsync("/api/Performance/auth-test");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "Request with invalid token should be unauthorized");
        }

        [Fact]
        public async Task GetApiMetrics_WithAdminAuth_ShouldReturnMetrics()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/api");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetApiMetrics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/api");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMemoryUsage_WithAdminAuth_ShouldReturnMemoryInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/memory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetMemoryUsage_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/memory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetGarbageCollectionInfo_WithAdminAuth_ShouldReturnGCInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/gc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetGarbageCollectionInfo_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/gc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetThreadPoolInfo_WithAdminAuth_ShouldReturnThreadInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/threads");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetThreadPoolInfo_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/threads");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRequestMetrics_WithAdminAuth_ShouldReturnRequestInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/requests");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetRequestMetrics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/requests");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPerformanceMetrics_WithAdminAuth_ShouldReturnMetrics()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Performance/metrics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetPerformanceMetrics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/metrics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
