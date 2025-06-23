using System.Net;
using System.Net.Http.Json;
using DecorStore.API.DTOs;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class ProductsControllerTests : TestBase
    {
        [Fact]
        public async Task GetProducts_ShouldReturnProductList()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/Products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pagedResult = await DeserializeResponseAsync<PagedResult<ProductDTO>>(response);
            pagedResult.Should().NotBeNull();
            pagedResult!.Items.Should().NotBeNull();
            pagedResult.Items.Should().HaveCountGreaterThan(0);
            pagedResult.Pagination.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProduct_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            await SeedTestDataAsync();
            var productsResponse = await _client.GetAsync("/api/Products");
            var pagedProducts = await DeserializeResponseAsync<PagedResult<ProductDTO>>(productsResponse);
            var productId = pagedProducts!.Items.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/Products/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var product = await DeserializeResponseAsync<ProductDTO>(response);
            product.Should().NotBeNull();
            product!.Id.Should().Be(productId);
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/Products/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateProduct_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            await SeedTestDataAsync();
            var categoriesResponse = await _client.GetAsync("/api/Category");
            var pagedCategories = await DeserializeResponseAsync<PagedResult<CategoryDTO>>(categoriesResponse);

            // Debug: Check categories
            Console.WriteLine($"Categories response status: {categoriesResponse.StatusCode}");
            Console.WriteLine($"Categories count: {pagedCategories?.Items?.Count() ?? 0}");
            if (pagedCategories?.Items?.Any() == true)
            {
                var firstCategory = pagedCategories.Items.First();
                Console.WriteLine($"First category: ID={firstCategory.Id}, Name={firstCategory.Name}");
            }

            var categoryId = pagedCategories!.Items.First().Id;

            var adminToken = await GetAdminTokenAsync();
            adminToken.Should().NotBeNullOrEmpty("Admin token should be obtained successfully");

            // Debug: Print token info
            Console.WriteLine($"Admin token obtained: {adminToken?[..20]}...");

            SetAuthHeader(adminToken!);

            // Debug: Check if header is set
            var authHeader = _client.DefaultRequestHeaders.Authorization;
            Console.WriteLine($"Authorization header set: {authHeader?.Scheme} {authHeader?.Parameter?[..20]}...");

            var createProductDto = new CreateProductDTO
            {
                Name = "Test Product",
                Description = "A test product description",
                Price = 99.99m,
                CategoryId = categoryId,
                IsActive = true,
                IsFeatured = false,
                StockQuantity = 10,
                SKU = "TEST001",
                Slug = "test-product",
                Weight = 1.5m, // Physical products need weight according to business rules
                IsDigital = false // Explicitly set as physical product
            };

            // Debug: Print the DTO being sent
            Console.WriteLine($"CreateProductDTO: Name={createProductDto.Name}, SKU={createProductDto.SKU}, Slug={createProductDto.Slug}, Price={createProductDto.Price}, CategoryId={createProductDto.CategoryId}");

            // Act
            var response = await PostAsJsonWithOptionsAsync("/api/Products", createProductDto);

            // Assert
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");
            }
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var product = await DeserializeResponseAsync<ProductDTO>(response);
            product.Should().NotBeNull();
            product!.Name.Should().Be(createProductDto.Name);
            product.Price.Should().Be(createProductDto.Price);

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            SetAuthHeader(adminToken!);

            var createProductDto = new CreateProductDTO
            {
                Name = "", // Invalid: empty name
                Description = "A test product description",
                Price = -10, // Invalid: negative price
                CategoryId = 99999, // Invalid: non-existent category
                IsActive = true,
                IsFeatured = false,
                StockQuantity = -5 // Invalid: negative stock
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", createProductDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task UpdateProduct_WithValidData_ShouldReturnOk()
        {
            // Arrange
            await SeedTestDataAsync();
            var productsResponse = await _client.GetAsync("/api/Products");
            var pagedProducts = await DeserializeResponseAsync<PagedResult<ProductDTO>>(productsResponse);
            var product = pagedProducts!.Items.First();

            var adminToken = await GetAdminTokenAsync();
            SetAuthHeader(adminToken!);

            var updateProductDto = new UpdateProductDTO
            {
                Name = "Updated Product Name",
                Description = "Updated description",
                Price = 199.99m,
                CategoryId = product.CategoryId,
                IsActive = true,
                IsFeatured = false, // Set to false to avoid needing images for business rule validation
                StockQuantity = 20,
                SKU = product.SKU,
                Slug = "updated-product",
                Weight = 2.0m, // Physical products need weight according to business rules
                IsDigital = false // Explicitly set as physical product
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Products/{product.Id}", updateProductDto);

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response content: {content}");
            }
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Check if response has content before deserializing
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseContent}");

            if (string.IsNullOrEmpty(responseContent))
            {
                Console.WriteLine("Response body is empty - this is expected for PUT operations that return 200 OK without content");
                // For PUT operations, sometimes they return 200 OK without content, which is valid
                // We can verify the update by making a GET request
                var getResponse = await _client.GetAsync($"/api/Products/1");
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                var updatedProduct = await DeserializeResponseAsync<ProductDTO>(getResponse);
                updatedProduct.Should().NotBeNull();
                updatedProduct!.Name.Should().Be(updateProductDto.Name);
                updatedProduct.Price.Should().Be(updateProductDto.Price);
            }
            else
            {
                // Reset the response content stream for deserialization
                response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json");
                var updatedProduct = await DeserializeResponseAsync<ProductDTO>(response);
                updatedProduct.Should().NotBeNull();
                updatedProduct!.Name.Should().Be(updateProductDto.Name);
                updatedProduct.Price.Should().Be(updateProductDto.Price);
            }

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            SetAuthHeader(adminToken!);

            var updateProductDto = new UpdateProductDTO
            {
                Name = "Updated Product Name",
                Description = "Updated description",
                Price = 199.99m,
                CategoryId = 1,
                IsActive = true,
                IsFeatured = false, // Set to false to avoid needing images for business rule validation
                StockQuantity = 20,
                Weight = 2.0m, // Physical products need weight according to business rules
                IsDigital = false, // Explicitly set as physical product
                SKU = "VALID-SKU-001", // Add valid SKU to pass validation
                Slug = "updated-product-slug" // Add valid slug
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/Products/99999", updateProductDto);

            // Assert
            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Expected 404 NotFound but got {response.StatusCode}");
                Console.WriteLine($"Response content: {content}");
            }
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task DeleteProduct_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            await SeedTestDataAsync();
            var productsResponse = await _client.GetAsync("/api/Products");
            var pagedProducts = await DeserializeResponseAsync<PagedResult<ProductDTO>>(productsResponse);
            var productId = pagedProducts!.Items.First().Id;

            var adminToken = await GetAdminTokenAsync();
            SetAuthHeader(adminToken!);

            // Act
            var response = await _client.DeleteAsync($"/api/Products/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify product is deleted
            var getResponse = await _client.GetAsync($"/api/Products/{productId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task DeleteProduct_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();
            SetAuthHeader(adminToken!);

            // Act
            var response = await _client.DeleteAsync("/api/Products/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Cleanup
            ClearAuthHeader();
        }

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnFilteredProducts()
        {
            // Arrange
            await SeedTestDataAsync();
            var categoriesResponse = await _client.GetAsync("/api/Category");
            var pagedCategories = await DeserializeResponseAsync<PagedResult<CategoryDTO>>(categoriesResponse);
            var categoryId = pagedCategories!.Items.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/Products/category/{categoryId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var products = await DeserializeResponseAsync<List<ProductDTO>>(response);
            products.Should().NotBeNull();
            products!.Should().OnlyContain(p => p.CategoryId == categoryId);
        }

        [Fact]
        public async Task SearchProducts_ShouldReturnMatchingProducts()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/Products/search?query=Test");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var products = await DeserializeResponseAsync<List<ProductDTO>>(response);
            products.Should().NotBeNull();
        }

        [Fact]
        public async Task GetFeaturedProducts_ShouldReturnOnlyFeaturedProducts()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/Products/featured");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var products = await DeserializeResponseAsync<List<ProductDTO>>(response);
            products.Should().NotBeNull();
            products!.Should().OnlyContain(p => p.IsFeatured);
        }
    }
}
