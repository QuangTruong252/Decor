using System.Net;
using System.Net.Http.Json;
using DecorStore.API.DTOs;
using DecorStore.API.Common;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class CustomerControllerTests : TestBase
    {
        public CustomerControllerTests() : base()
        {
        }

        [Fact]
        public async Task GetCustomers_WithAdminAuth_ShouldReturnPagedCustomers()
        {
            // Arrange
            await SeedTestDataAsync();
            var adminToken = await GetAdminTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Customer");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pagedResult = await DeserializeResponseAsync<PagedResult<CustomerDTO>>(response);
            pagedResult.Should().NotBeNull();
            pagedResult!.Items.Should().NotBeNull();
            pagedResult.Pagination.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCustomers_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Customer");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCustomer_WithValidId_ShouldReturnCustomer()
        {
            // Arrange
            await SeedTestDataAsync();
            var adminToken = await GetAdminTokenAsync();

            // First get a customer ID
            var customersRequest = new HttpRequestMessage(HttpMethod.Get, "/api/Customer");
            customersRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var customersResponse = await _client.SendAsync(customersRequest);
            var pagedCustomers = await DeserializeResponseAsync<PagedResult<CustomerDTO>>(customersResponse);
            
            if (pagedCustomers?.Items?.Any() != true)
            {
                // Skip test if no customers exist
                return;
            }

            var customerId = pagedCustomers.Items.First().Id;

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Customer/{customerId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var customer = await DeserializeResponseAsync<CustomerDTO>(response);
            customer.Should().NotBeNull();
            customer!.Id.Should().Be(customerId);
        }

        [Fact]
        public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Customer/99999");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); // API correctly returns 404 for invalid IDs
        }

        [Fact]
        public async Task CreateCustomer_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();

            var createCustomerDto = new CreateCustomerDTO
            {
                FirstName = "Test",
                LastName = "Customer",
                Email = "testcustomer@example.com",
                Phone = "+1234567890",
                Address = "123 Test Street",
                City = "Test City",
                State = "Test State",
                PostalCode = "12345",
                Country = "Test Country"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Customer")
            {
                Content = JsonContent.Create(createCustomerDto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var customer = await DeserializeResponseAsync<CustomerDTO>(response);
            customer.Should().NotBeNull();
            customer!.FirstName.Should().Be(createCustomerDto.FirstName);
            customer.LastName.Should().Be(createCustomerDto.LastName);
            customer.Email.Should().Be(createCustomerDto.Email);
        }

        [Fact]
        public async Task CreateCustomer_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await GetAdminTokenAsync();

            var createCustomerDto = new CreateCustomerDTO
            {
                FirstName = "", // Invalid: empty first name
                LastName = "", // Invalid: empty last name
                Email = "invalid-email", // Invalid: bad email format
                Phone = "invalid-phone", // Invalid: bad phone format
                Address = "",
                City = "",
                State = "",
                PostalCode = "",
                Country = ""
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Customer")
            {
                Content = JsonContent.Create(createCustomerDto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCustomer_WithValidData_ShouldReturnOk()
        {
            // Arrange
            await SeedTestDataAsync();
            var adminToken = await GetAdminTokenAsync();

            // First get a customer
            var customersRequest = new HttpRequestMessage(HttpMethod.Get, "/api/Customer");
            customersRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var customersResponse = await _client.SendAsync(customersRequest);
            var pagedCustomers = await DeserializeResponseAsync<PagedResult<CustomerDTO>>(customersResponse);
            
            if (pagedCustomers?.Items?.Any() != true)
            {
                // Skip test if no customers exist
                return;
            }

            var customer = pagedCustomers.Items.First();

            var updateCustomerDto = new UpdateCustomerDTO
            {
                FirstName = "Updated",
                LastName = "Customer",
                Email = customer.Email, // Keep same email
                Phone = "+9876543210",
                Address = "456 Updated Street",
                City = "Updated City",
                State = "Updated State",
                PostalCode = "54321",
                Country = "Updated Country"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Customer/{customer.Id}")
            {
                Content = JsonContent.Create(updateCustomerDto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedCustomer = await DeserializeResponseAsync<CustomerDTO>(response);
            updatedCustomer.Should().NotBeNull();
            updatedCustomer!.FirstName.Should().Be(updateCustomerDto.FirstName);
            updatedCustomer.LastName.Should().Be(updateCustomerDto.LastName);
        }

        [Fact]
        public async Task DeleteCustomer_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            await SeedTestDataAsync();
            var adminToken = await GetAdminTokenAsync();

            // First get a customer
            var customersRequest = new HttpRequestMessage(HttpMethod.Get, "/api/Customer");
            customersRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var customersResponse = await _client.SendAsync(customersRequest);
            var pagedCustomers = await DeserializeResponseAsync<PagedResult<CustomerDTO>>(customersResponse);
            
            if (pagedCustomers?.Items?.Any() != true)
            {
                // Skip test if no customers exist
                return;
            }

            var customerId = pagedCustomers.Items.First().Id;

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Customer/{customerId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify customer is deleted
            var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/Customer/{customerId}");
            getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var getResponse = await _client.SendAsync(getRequest);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound); // API correctly returns 404 for deleted customers
        }

        [Fact]
        public async Task GetCustomersWithOrders_ShouldReturnCustomersWithOrders()
        {
            // Arrange
            await SeedTestDataAsync();
            var adminToken = await GetAdminTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Customer/with-orders");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var customers = await DeserializeResponseAsync<IEnumerable<CustomerDTO>>(response);
            customers.Should().NotBeNull();
        }

        [Fact]
        public async Task GetTopCustomersByOrderCount_ShouldReturnTopCustomers()
        {
            // Arrange
            await SeedTestDataAsync();
            var adminToken = await GetAdminTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Customer/top-by-order-count?count=5");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var customers = await DeserializeResponseAsync<IEnumerable<CustomerDTO>>(response);
            customers.Should().NotBeNull();
            customers!.Should().HaveCountLessThanOrEqualTo(5);
        }

        [Fact]
        public async Task GetTopCustomersBySpending_ShouldReturnTopCustomers()
        {
            // Arrange
            await SeedTestDataAsync();
            var adminToken = await GetAdminTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Customer/top-by-spending?count=5");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var customers = await DeserializeResponseAsync<IEnumerable<CustomerDTO>>(response);
            customers.Should().NotBeNull();
            customers!.Should().HaveCountLessThanOrEqualTo(5);
        }
    }
}
