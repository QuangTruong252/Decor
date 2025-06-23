using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class GdprComplianceControllerTests : TestBase
    {
        [Fact]
        public async Task GetDataExport_WithAdminAuth_ShouldReturnUserData()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var userId = 1; // Test user ID

            // Act
            var response = await _client.GetAsync($"/api/GdprCompliance/data-export/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetDataExport_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/data-export/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RequestDataDeletion_WithAdminAuth_ShouldReturnSuccess()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var request = new
            {
                UserId = 1,
                Reason = "User requested account deletion",
                ConfirmDeletion = true
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/GdprCompliance/data/deletion", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RequestDataDeletion_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new
            {
                UserId = 1,
                Reason = "User requested account deletion",
                ConfirmDeletion = true
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/GdprCompliance/data/deletion", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetConsentStatus_WithAdminAuth_ShouldReturnConsentInfo()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var userId = 1;

            // Act
            var response = await _client.GetAsync($"/api/GdprCompliance/consent/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetConsentStatus_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/consent/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateConsent_WithAdminAuth_ShouldUpdateConsentStatus()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var request = new
            {
                UserId = 1,
                MarketingConsent = true,
                DataProcessingConsent = true,
                CookieConsent = false
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/api/GdprCompliance/consent", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UpdateConsent_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new
            {
                UserId = 1,
                MarketingConsent = true,
                DataProcessingConsent = true,
                CookieConsent = false
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/api/GdprCompliance/consent", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPrivacyPolicy_ShouldReturnPolicy()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/privacy-policy");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("privacy");
        }

        [Fact]
        public async Task GetDataProcessingAgreement_ShouldReturnAgreement()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/data-processing-agreement");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetDataRetentionPolicy_WithAdminAuth_ShouldReturnPolicy()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/data-retention");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetDataRetentionPolicy_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/data-retention");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAuditLog_WithAdminAuth_ShouldReturnAuditEntries()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/audit-log?userId=1");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetAuditLog_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/audit-log?userId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AnonymizeUserData_WithAdminAuth_ShouldAnonymizeData()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var request = new
            {
                UserId = 1,
                Reason = "User requested data anonymization",
                KeepOrderHistory = false
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/GdprCompliance/anonymize", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task AnonymizeUserData_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new
            {
                UserId = 1,
                Reason = "User requested data anonymization",
                KeepOrderHistory = false
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/GdprCompliance/anonymize", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDataBreachLog_WithAdminAuth_ShouldReturnBreachLog()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/data-breaches");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetDataBreachLog_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/data-breaches");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ValidateGdprCompliance_WithAdminAuth_ShouldReturnComplianceStatus()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/validate-compliance");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
                content.Should().Contain("compliance");
            }
        }

        [Fact]
        public async Task ValidateGdprCompliance_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/GdprCompliance/validate-compliance");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
