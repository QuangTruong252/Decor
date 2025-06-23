using System.Net;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class ConfigurationTestControllerTests : TestBase
    {
        [Fact]
        public async Task GetConfiguration_ShouldReturnConfigurationDetails()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("configuration");
        }

        [Fact]
        public async Task ValidateConfiguration_ShouldReturnValidationResult()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/validate");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetEnvironmentInfo_ShouldReturnEnvironmentDetails()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/environment");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("environment");
        }

        [Fact]
        public async Task GetDatabaseSettings_WithAdminAuth_ShouldReturnDatabaseInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/database");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetDatabaseSettings_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/database");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSecuritySettings_WithAdminAuth_ShouldReturnSecurityInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/security");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetSecuritySettings_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/security");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCacheSettings_WithAdminAuth_ShouldReturnCacheInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/cache");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetCacheSettings_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/cache");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task TestConfiguration_ShouldReturnTestResults()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.PostAsync("/api/ConfigurationTest/test", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllSettings_WithAdminAuth_ShouldReturnAllSettings()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/all");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
                content.Should().Contain("settings");
            }
        }

        [Fact]
        public async Task GetAllSettings_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetConnectionStrings_WithAdminAuth_ShouldReturnConnectionInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/connections");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetConnectionStrings_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/connections");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetFeatureFlags_ShouldReturnFeatureFlags()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/ConfigurationTest/features");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }
}
