using System.Net;
using System.Net.Http.Json;
using DecorStore.API.DTOs;
using FluentAssertions;
using Xunit;

namespace DecorStore.Test.Controllers
{
    public class AuthControllerTests : TestBase
    {
        [Fact]
        public async Task Register_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "TestPass@word1",
                ConfirmPassword = "TestPass@word1",
                FirstName = "Test",
                LastName = "User",
                Phone = "+1234567890",
                DateOfBirth = DateTime.Now.AddYears(-25),
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var authResponse = await DeserializeResponseAsync<AuthResponseDTO>(response);
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.User.Should().NotBeNull();
            authResponse.User.Email.Should().Be(registerDto.Email);
            authResponse.User.Username.Should().Be(registerDto.Username);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Username = "testuser1",
                Email = "duplicate@example.com",
                Password = "TestPass@word1",
                ConfirmPassword = "TestPass@word1",
                FirstName = "Test",
                LastName = "User",
                Phone = "+1234567890",
                DateOfBirth = DateTime.Now.AddYears(-25),
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Register first user
            await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Try to register with same email but different username
            registerDto.Username = "testuser2";

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = "TestPassword123!",
                ConfirmPassword = "TestPassword123!",
                FirstName = "Test",
                LastName = "User",
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "TestPass@word1",
                ConfirmPassword = "DifferentPass@word1",
                FirstName = "Test",
                LastName = "User",
                Phone = "+1234567890",
                DateOfBirth = DateTime.Now.AddYears(-25),
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var email = "login@example.com";
            var password = "TestPass@word1";
            await RegisterTestUserAsync(email, password);

            var loginDto = new LoginDTO
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var authResponse = await DeserializeResponseAsync<AuthResponseDTO>(response);
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.User.Should().NotBeNull();
            authResponse.User.Email.Should().Be(email);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange
            var email = "currentuser@example.com";
            var password = "TestPass@word1";
            await RegisterTestUserAsync(email, password);

            var token = await GetAuthTokenAsync(email, password);
            token.Should().NotBeNull();

            // Debug: Decode JWT token to check its contents
            var tokenParts = token!.Split('.');
            if (tokenParts.Length == 3)
            {
                var payload = tokenParts[1];
                // Add padding if needed
                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }

                try
                {
                    var decodedBytes = Convert.FromBase64String(payload);
                    var decodedPayload = System.Text.Encoding.UTF8.GetString(decodedBytes);
                    Console.WriteLine($"[DEBUG] JWT Payload: {decodedPayload}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Failed to decode JWT: {ex.Message}");
                }
            }

            SetAuthHeader(token!);

            // Debug: Verify the header is set correctly before making the request
            Console.WriteLine($"[DEBUG] Authorization header before request: {_client.DefaultRequestHeaders.Authorization}");

            // Act
            var response = await _client.GetAsync("/api/Auth/user");

            // Debug: Print response details
            Console.WriteLine($"[DEBUG] Response Status: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Response Content: {responseContent}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var userDto = await DeserializeResponseAsync<UserDTO>(response);
            userDto.Should().NotBeNull();
            userDto!.Email.Should().Be(email);
        }

        [Fact]
        public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Auth/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCurrentUser_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            SetAuthHeader("invalid-token");

            // Act
            var response = await _client.GetAsync("/api/Auth/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CheckClaims_WithValidToken_ShouldReturnClaimsInfo()
        {
            // Arrange
            var email = "claims@example.com";
            var password = "TestPass@word1";
            await RegisterTestUserAsync(email, password);
            
            var token = await GetAuthTokenAsync(email, password);
            SetAuthHeader(token!);

            // Act
            var response = await _client.GetAsync("/api/Auth/check-claims");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("claims"); // JSON uses camelCase naming policy
        }

        [Fact]
        public async Task ChangePassword_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var email = "changepass@example.com";
            var oldPassword = "OldPass@word1";
            var newPassword = "NewPass@word2";

            await RegisterTestUserAsync(email, oldPassword);
            var token = await GetAuthTokenAsync(email, oldPassword);
            SetAuthHeader(token!);

            var changePasswordDto = new ChangePasswordDTO
            {
                CurrentPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/change-password", changePasswordDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ChangePassword_WithWrongCurrentPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var email = "wrongpass@example.com";
            var password = "TestPass@word1";

            await RegisterTestUserAsync(email, password);
            var token = await GetAuthTokenAsync(email, password);
            SetAuthHeader(token!);

            var changePasswordDto = new ChangePasswordDTO
            {
                CurrentPassword = "WrongPass@word1",
                NewPassword = "NewPass@word2",
                ConfirmNewPassword = "NewPass@word2"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/change-password", changePasswordDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task TestUser_WithValidToken_ShouldShowAuthenticationStatus()
        {
            // Arrange
            var email = "testuser@example.com";
            var password = "TestPass@word1";
            await RegisterTestUserAsync(email, password);

            var token = await GetAuthTokenAsync(email, password);
            token.Should().NotBeNull();

            // Try using explicit request with Authorization header
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/test-user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            Console.WriteLine($"[DEBUG] Manual request Authorization header: {request.Headers.Authorization}");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Test endpoint response: {content}");
            
            var result = await DeserializeResponseAsync<dynamic>(response);
            // This should show us if the authentication is working at the controller level
        }
    }
}
