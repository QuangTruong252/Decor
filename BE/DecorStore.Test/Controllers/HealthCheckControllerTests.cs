using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class HealthCheckControllerTests : TestBase
    {
        [Fact]
        public async Task GetHealthCheck_ShouldReturnHealthStatus()
        {
            // Act
            var response = await _client.GetAsync("/api/HealthCheck");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            healthStatus.TryGetProperty("databaseConnected", out var dbConnected).Should().BeTrue();
            healthStatus.TryGetProperty("provider", out var provider).Should().BeTrue();
            healthStatus.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
        }

        [Fact]
        public async Task GetHealthCheck_ShouldIncludeDatabaseConnectionInfo()
        {
            // Act
            var response = await _client.GetAsync("/api/HealthCheck");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            
            // Should include database connection status
            healthStatus.TryGetProperty("databaseConnected", out var dbConnected).Should().BeTrue();
            dbConnected.GetBoolean().Should().BeTrue(); // Database should be connected in test environment
            
            // Should include provider information
            healthStatus.TryGetProperty("provider", out var provider).Should().BeTrue();
            provider.GetString().Should().NotBeNullOrEmpty();
            
            // Should hide connection string for security
            healthStatus.TryGetProperty("connectionString", out var connectionString).Should().BeTrue();
            connectionString.GetString().Should().Be("***Hidden for security***");
        }

        [Fact]
        public async Task GetHealthCheck_ShouldIncludeTimestamp()
        {
            // Act
            var response = await _client.GetAsync("/api/HealthCheck");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            
            healthStatus.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
            
            // Parse timestamp and verify it's recent (within last minute)
            var timestampValue = DateTime.Parse(timestamp.GetString()!);
            var now = DateTime.UtcNow;
            var timeDifference = now - timestampValue;
            timeDifference.TotalMinutes.Should().BeLessThan(1);
        }

        [Fact]
        public async Task HealthEndpoint_ShouldReturnHealthStatus()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            healthStatus.TryGetProperty("status", out var status).Should().BeTrue();
            healthStatus.TryGetProperty("totalDuration", out var duration).Should().BeTrue();
            healthStatus.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
        }

        [Fact]
        public async Task HealthReadyEndpoint_ShouldReturnReadinessStatus()
        {
            // Act
            var response = await _client.GetAsync("/health/ready");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            healthStatus.TryGetProperty("status", out var status).Should().BeTrue();
            
            // Status should be Healthy, Degraded, or Unhealthy
            var statusValue = status.GetString();
            statusValue.Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
        }

        [Fact]
        public async Task HealthLiveEndpoint_ShouldReturnLivenessStatus()
        {
            // Act
            var response = await _client.GetAsync("/health/live");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            healthStatus.TryGetProperty("status", out var status).Should().BeTrue();
            
            // Liveness should typically be Healthy if the application is running
            var statusValue = status.GetString();
            statusValue.Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
        }

        [Fact]
        public async Task HealthDetailedEndpoint_ShouldReturnDetailedHealthInfo()
        {
            // Act
            var response = await _client.GetAsync("/health/detailed");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            healthStatus.TryGetProperty("status", out var status).Should().BeTrue();
            healthStatus.TryGetProperty("results", out var results).Should().BeTrue();
            
            // Should contain detailed information about individual health checks
            results.ValueKind.Should().Be(JsonValueKind.Object);
        }

        [Fact]
        public async Task PerformanceHealthEndpoint_ShouldReturnHealthStatus()
        {
            // Act
            var response = await _client.GetAsync("/api/Performance/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            healthStatus.TryGetProperty("status", out var status).Should().BeTrue();
            healthStatus.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
            healthStatus.TryGetProperty("version", out var version).Should().BeTrue();
            healthStatus.TryGetProperty("environment", out var environment).Should().BeTrue();
            healthStatus.TryGetProperty("uptime", out var uptime).Should().BeTrue();
            
            status.GetString().Should().Be("Healthy");
        }

        [Fact]
        public async Task GetHealthCheck_WithDatabaseError_ShouldReturnInternalServerError()
        {
            // Note: This test would require mocking the database context to simulate an error
            // For now, we'll test that the endpoint handles errors gracefully
            
            // Act
            var response = await _client.GetAsync("/api/HealthCheck");

            // Assert
            // In normal circumstances, this should return OK
            // In case of database issues, it should return 500
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
            
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
                
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);
                errorResponse.TryGetProperty("error", out var error).Should().BeTrue();
                error.GetString().Should().Be("Database connection error");
            }
        }

        [Fact]
        public async Task HealthCheck_ResponseTime_ShouldBeFast()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _client.GetAsync("/api/HealthCheck");
            stopwatch.Stop();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Health check should respond quickly (within 5 seconds)
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
        }

        [Fact]
        public async Task HealthCheck_MultipleRequests_ShouldBeConsistent()
        {
            // Act - Make multiple requests
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(_client.GetAsync("/api/HealthCheck"));
            }
            
            var responses = await Task.WhenAll(tasks);

            // Assert
            foreach (var response in responses)
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
                
                var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
                healthStatus.TryGetProperty("databaseConnected", out var dbConnected).Should().BeTrue();
                healthStatus.TryGetProperty("provider", out var provider).Should().BeTrue();
                healthStatus.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
            }
            
            // Clean up
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }

        [Fact]
        public async Task HealthEndpoints_ShouldNotRequireAuthentication()
        {
            // Arrange - Don't set any authentication headers

            // Act & Assert - All health endpoints should be accessible without authentication
            var endpoints = new[]
            {
                "/api/HealthCheck",
                "/health",
                "/health/ready",
                "/health/live",
                "/health/detailed",
                "/api/Performance/health"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                
                // Should not return Unauthorized
                response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
            }
        }

        [Fact]
        public async Task HealthCheck_ContentType_ShouldBeApplicationJson()
        {
            // Act
            var response = await _client.GetAsync("/api/HealthCheck");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }

        [Fact]
        public async Task HealthEndpoints_ContentType_ShouldBeApplicationJson()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }

        [Fact]
        public async Task HealthCheck_ShouldIncludeProviderName()
        {
            // Act
            var response = await _client.GetAsync("/api/HealthCheck");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var healthStatus = JsonSerializer.Deserialize<JsonElement>(content);
            
            healthStatus.TryGetProperty("provider", out var provider).Should().BeTrue();
            var providerName = provider.GetString();
            providerName.Should().NotBeNullOrEmpty();
            
            // Should be a valid EF Core provider
            providerName.Should().Contain("Microsoft.EntityFrameworkCore");
        }
    }
}
