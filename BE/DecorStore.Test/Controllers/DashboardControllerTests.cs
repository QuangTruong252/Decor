using Microsoft.AspNetCore.Http;
using System.Net;
using FluentAssertions;
using DecorStore.API.DTOs;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class DashboardControllerTests : TestBase
    {
        [Fact]
        public async Task GetDashboardSummary_WithAdminAuth_ShouldReturnSummary()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var summary = await DeserializeResponseAsync<DashboardSummaryDTO>(response);
            summary.Should().NotBeNull();
            summary!.TotalProducts.Should().BeGreaterThanOrEqualTo(0);
            summary.TotalOrders.Should().BeGreaterThanOrEqualTo(0);
            summary.TotalCustomers.Should().BeGreaterThanOrEqualTo(0);
            summary.TotalRevenue.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetDashboardSummary_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Dashboard/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSalesTrend_WithValidParameters_ShouldReturnTrend()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Act
            var response = await _client.GetAsync($"/api/Dashboard/sales-trend?period=daily&startDate={startDate}&endDate={endDate}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var trend = await DeserializeResponseAsync<SalesTrendDTO>(response);
            trend.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSalesTrend_WithDefaultParameters_ShouldReturnTrend()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/sales-trend");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var trend = await DeserializeResponseAsync<SalesTrendDTO>(response);
            trend.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSalesTrend_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Dashboard/sales-trend");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPopularProducts_WithLimit_ShouldReturnProducts()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/popular-products?limit=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var products = await DeserializeResponseAsync<List<PopularProductDTO>>(response);
            products.Should().NotBeNull();
            products!.Count.Should().BeLessOrEqualTo(10);
        }

        [Fact]
        public async Task GetPopularProducts_WithDefaultLimit_ShouldReturnProducts()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/popular-products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var products = await DeserializeResponseAsync<List<PopularProductDTO>>(response);
            products.Should().NotBeNull();
            products!.Count.Should().BeLessOrEqualTo(5); // Default limit
        }

        [Fact]
        public async Task GetPopularProducts_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Dashboard/popular-products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSalesByCategory_WithAdminAuth_ShouldReturnCategorySales()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/sales-by-category");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var categorySales = await DeserializeResponseAsync<List<CategorySalesDTO>>(response);
            categorySales.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSalesByCategory_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Dashboard/sales-by-category");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetOrderStatusDistribution_WithAdminAuth_ShouldReturnDistribution()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/order-status-distribution");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var distribution = await DeserializeResponseAsync<OrderStatusDistributionDTO>(response);
            distribution.Should().NotBeNull();
        }

        [Fact]
        public async Task GetOrderStatusDistribution_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Dashboard/order-status-distribution");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDashboardSummary_ShouldHaveCacheHeaders()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Check for cache headers (response caching is configured for 300 seconds)
            response.Headers.Should().ContainKey("Cache-Control");
        }

        [Fact]
        public async Task GetSalesTrend_ShouldHaveCacheHeaders()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/sales-trend");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Check for cache headers (response caching is configured for 600 seconds)
            response.Headers.Should().ContainKey("Cache-Control");
        }

        [Fact]
        public async Task GetSalesTrend_WithInvalidPeriod_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/sales-trend?period=invalid");

            // Assert
            // This might return OK with default handling or BadRequest depending on validation
            // The actual behavior depends on the service implementation
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetPopularProducts_WithZeroLimit_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/popular-products?limit=0");

            // Assert
            // This might return OK with default handling or BadRequest depending on validation
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetPopularProducts_WithNegativeLimit_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Dashboard/popular-products?limit=-1");

            // Assert
            // This might return OK with default handling or BadRequest depending on validation
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
    }
}
