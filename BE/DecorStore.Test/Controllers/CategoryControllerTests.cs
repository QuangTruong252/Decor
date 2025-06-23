using System.Net;
using System.Net.Http.Json;
using DecorStore.API.DTOs;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class CategoryControllerTests : TestBase
    {
        [Fact]
        public async Task GetCategories_ShouldReturnCategoryList()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/Category");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pagedResult = await DeserializeResponseAsync<PagedResult<CategoryDTO>>(response);
            pagedResult.Should().NotBeNull();
            pagedResult!.Items.Should().NotBeNull();
            pagedResult.Items.Should().HaveCountGreaterThan(0);
            pagedResult.Pagination.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCategory_WithValidId_ShouldReturnCategory()
        {
            // Arrange
            await SeedTestDataAsync();
            var categoriesResponse = await _client.GetAsync("/api/Category");
            var pagedCategories = await DeserializeResponseAsync<PagedResult<CategoryDTO>>(categoriesResponse);
            var categoryId = pagedCategories!.Items.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/Category/{categoryId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var category = await DeserializeResponseAsync<CategoryDTO>(response);
            category.Should().NotBeNull();
            category!.Id.Should().Be(categoryId);
        }

        [Fact]
        public async Task GetCategory_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/Category/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); // API should return 404 for non-existent category
        }

        [Fact]
        public async Task CreateCategory_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            SetAuthHeader(adminToken!);

            var createCategoryDto = new CreateCategoryDTO
            {
                Name = "Test Category",
                Description = "A test category description",
                Slug = "test-category"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Category")
            {
                Content = JsonContent.Create(createCategoryDto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var category = await DeserializeResponseAsync<CategoryDTO>(response);
            category.Should().NotBeNull();
            category!.Name.Should().Be(createCategoryDto.Name);
            category.Slug.Should().Be(createCategoryDto.Slug);

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task CreateCategory_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();

            var createCategoryDto = new CreateCategoryDTO
            {
                Name = "", // Invalid: empty name
                Description = "A test category description",
                Slug = "" // Invalid: empty slug
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Category")
            {
                Content = JsonContent.Create(createCategoryDto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCategory_WithValidData_ShouldReturnOk()
        {
            // Arrange
            await SeedTestDataAsync();
            var categoriesResponse = await _client.GetAsync("/api/Category");
            var pagedCategories = await DeserializeResponseAsync<PagedResult<CategoryDTO>>(categoriesResponse);
            var category = pagedCategories!.Items.First();

            var adminToken = await GetAdminTokenAsync();

            var updateCategoryDto = new UpdateCategoryDTO
            {
                Name = "Updated Category Name",
                Description = "Updated description",
                Slug = "updated-category"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Category/{category.Id}")
            {
                Content = JsonContent.Create(updateCategoryDto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the category was updated by fetching it
            var getResponse = await _client.GetAsync($"/api/Category/{category.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedCategory = await DeserializeResponseAsync<CategoryDTO>(getResponse);
            updatedCategory.Should().NotBeNull();
            updatedCategory!.Name.Should().Be(updateCategoryDto.Name);
            updatedCategory.Slug.Should().Be(updateCategoryDto.Slug);

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task UpdateCategory_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();

            var updateCategoryDto = new UpdateCategoryDTO
            {
                Name = "Updated Category Name",
                Description = "Updated description",
                Slug = "updated-category"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Put, "/api/Category/99999")
            {
                Content = JsonContent.Create(updateCategoryDto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); // API should return 404 for non-existent category
        }

        [Fact]
        public async Task DeleteCategory_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            await SeedTestDataAsync();
            var categoriesResponse = await _client.GetAsync("/api/Category");
            var pagedCategories = await DeserializeResponseAsync<PagedResult<CategoryDTO>>(categoriesResponse);
            var categoryId = pagedCategories!.Items.First().Id;

            var adminToken = await GetAdminTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Category/{categoryId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify category is deleted
            var getResponse = await _client.GetAsync($"/api/Category/{categoryId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound); // API should return 404 for soft-deleted category

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task DeleteCategory_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Category/99999");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); // API should return 404 for non-existent category
        }

        [Fact]
        public async Task GetCategoryBySlug_WithValidSlug_ShouldReturnCategory()
        {
            // Arrange
            await SeedTestDataAsync();
            var categoriesResponse = await _client.GetAsync("/api/Category");
            var pagedCategories = await DeserializeResponseAsync<PagedResult<CategoryDTO>>(categoriesResponse);
            var category = pagedCategories!.Items.First();

            // Act
            var response = await _client.GetAsync($"/api/Category/slug/{category.Slug}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var foundCategory = await DeserializeResponseAsync<CategoryDTO>(response);
            foundCategory.Should().NotBeNull();
            foundCategory!.Id.Should().Be(category.Id);
            foundCategory.Slug.Should().Be(category.Slug);
        }

        [Fact]
        public async Task GetCategoryBySlug_WithInvalidSlug_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/Category/slug/non-existent-slug");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); // API should return 404 for non-existent category slug
        }

        [Fact]
        public async Task GetCategoryProducts_ShouldReturnProductsInCategory()
        {
            // Arrange - Create fresh test data to ensure isolation
            var testId = Guid.NewGuid().ToString("N")[..8];

            // Create a test category
            var category = new DecorStore.API.Models.Category
            {
                Name = $"Test Category {testId}",
                Slug = $"test-category-{testId}",
                Description = "Test category for products"
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Create test products for this category
            var products = new[]
            {
                new DecorStore.API.Models.Product
                {
                    Name = $"Test Product 1 {testId}",
                    Slug = $"test-product-1-{testId}",
                    Description = "Test product 1",
                    Price = 99.99m,
                    CategoryId = category.Id,
                    SKU = $"PROD1{testId}",
                    IsActive = true,
                    IsFeatured = false,
                    StockQuantity = 10
                },
                new DecorStore.API.Models.Product
                {
                    Name = $"Test Product 2 {testId}",
                    Slug = $"test-product-2-{testId}",
                    Description = "Test product 2",
                    Price = 149.99m,
                    CategoryId = category.Id,
                    SKU = $"PROD2{testId}",
                    IsActive = true,
                    IsFeatured = true,
                    StockQuantity = 5
                }
            };
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/Products/category/{category.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var returnedProducts = await DeserializeResponseAsync<List<ProductDTO>>(response);
            returnedProducts.Should().NotBeNull();
            returnedProducts!.Should().HaveCount(2);
            returnedProducts.Should().OnlyContain(p => p.CategoryId == category.Id);
        }
    }
}
