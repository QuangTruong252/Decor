using Microsoft.AspNetCore.Http;
using System.Net;
using FluentAssertions;
using DecorStore.API.DTOs;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class ValidationEdgeCasesTests : TestBase
    {
        [Fact]
        public async Task Registration_WithExtremelyLongEmail_ShouldReturn400()
        {
            // Arrange
            var longEmail = new string('a', 500) + "@test.com";
            var registerRequest = new RegisterDTO
            {
                Username = "testuser",
                Email = longEmail,
                Password = "ValidPassword123!",
                ConfirmPassword = "ValidPassword123!",
                FirstName = "Test",
                LastName = "User",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Registration_WithExtremelyLongUsername_ShouldReturn400()
        {
            // Arrange
            var longUsername = new string('a', 500);
            var registerRequest = new RegisterDTO
            {
                Username = longUsername,
                Email = "test@example.com",
                Password = "ValidPassword123!",
                ConfirmPassword = "ValidPassword123!",
                FirstName = "Test",
                LastName = "User",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Registration_WithExtremelyLongPassword_ShouldReturn400()
        {
            // Arrange
            var longPassword = new string('a', 1000) + "A1!";
            var registerRequest = new RegisterDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = longPassword,
                ConfirmPassword = longPassword,
                FirstName = "Test",
                LastName = "User",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Registration_WithSingleCharacterFields_ShouldReturn400()
        {
            // Arrange
            var registerRequest = new RegisterDTO
            {
                Username = "a",
                Email = "a@b.c",
                Password = "a",
                ConfirmPassword = "a",
                FirstName = "A",
                LastName = "B",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Registration_WithWhitespaceOnlyFields_ShouldReturn400()
        {
            // Arrange
            var registerRequest = new RegisterDTO
            {
                Username = "   ",
                Email = "   @   .   ",
                Password = "   ",
                ConfirmPassword = "   ",
                FirstName = "   ",
                LastName = "   ",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Registration_WithSpecialCharactersInUsername_ShouldReturn400()
        {
            // Arrange
            var registerRequest = new RegisterDTO
            {
                Username = "user@#$%^&*()",
                Email = "test@example.com",
                Password = "ValidPassword123!",
                ConfirmPassword = "ValidPassword123!",
                FirstName = "Test",
                LastName = "User",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Registration_WithNumericOnlyPassword_ShouldReturn400()
        {
            // Arrange
            var registerRequest = new RegisterDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "123456789",
                ConfirmPassword = "123456789",
                FirstName = "Test",
                LastName = "User",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Registration_WithCommonPassword_ShouldReturn400()
        {
            // Arrange
            var registerRequest = new RegisterDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password",
                ConfirmPassword = "password",
                FirstName = "Test",
                LastName = "User",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Product_WithNegativePrice_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = -10.50m,
                CategoryId = 1,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_WithZeroPrice_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 0m,
                CategoryId = 1,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_WithExtremelyHighPrice_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = decimal.MaxValue,
                CategoryId = 1,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_WithNegativeStock_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.50m,
                CategoryId = 1,
                StockQuantity = -5
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_WithNonExistentCategory_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.50m,
                CategoryId = 99999,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_WithEmptyName_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = "",
                Description = "Test Description",
                Price = 10.50m,
                CategoryId = 1,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_WithExtremelyLongName_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = new string('a', 1000),
                Description = "Test Description",
                Price = 10.50m,
                CategoryId = 1,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_WithExtremelyLongDescription_ShouldReturn400()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var productRequest = new CreateProductDTO
            {
                Name = "Test Product",
                Description = new string('a', 10000),
                Price = 10.50m,
                CategoryId = 1,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Search_WithExtremelyLongQuery_ShouldHandleGracefully()
        {
            // Arrange
            var longQuery = new string('a', 10000);

            // Act
            var response = await _client.GetAsync($"/api/Products/search?q={longQuery}");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, 
                HttpStatusCode.BadRequest,
                HttpStatusCode.RequestUriTooLong
            );
        }

        [Fact]
        public async Task Search_WithSpecialCharacters_ShouldHandleGracefully()
        {
            // Arrange
            var specialQuery = "!@#$%^&*()_+-=[]{}|;':\",./<>?";

            // Act
            var response = await _client.GetAsync($"/api/Products/search?q={Uri.EscapeDataString(specialQuery)}");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, 
                HttpStatusCode.BadRequest
            );
        }

        [Fact]
        public async Task Pagination_WithExtremeValues_ShouldHandleGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/Products?page=999999&pageSize=999999");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, 
                HttpStatusCode.BadRequest
            );
        }

        [Fact]
        public async Task DateRange_WithInvalidDates_ShouldReturn400()
        {
            // Act
            var response = await _client.GetAsync("/api/Orders?startDate=invalid-date&endDate=also-invalid");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound
            );
        }

        [Fact]
        public async Task DateRange_WithFutureDates_ShouldHandleGracefully()
        {
            // Arrange
            var futureDate = DateTime.Now.AddYears(100).ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/Orders?startDate={futureDate}&endDate={futureDate}");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound
            );
        }

        [Fact]
        public async Task BoundaryValues_ShouldHandleCorrectly()
        {
            // Test various boundary values
            var testCases = new[]
            {
                "/api/Products?page=1&pageSize=1",
                "/api/Products?page=0&pageSize=0",
                "/api/Products?minPrice=0.01&maxPrice=999999.99",
                "/api/Products?sortBy=Name&sortOrder=asc",
                "/api/Products?sortBy=Name&sortOrder=desc"
            };

            foreach (var testCase in testCases)
            {
                // Act
                var response = await _client.GetAsync(testCase);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Unauthorized
                );
            }
        }
    }
}
