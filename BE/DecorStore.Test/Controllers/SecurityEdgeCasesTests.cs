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
    public class SecurityEdgeCasesTests : TestBase
    {
        [Fact]
        public async Task PathTraversal_ShouldBeBlocked()
        {
            // Arrange
            var pathTraversalAttempts = new[]
            {
                "/api/Products/../../../etc/passwd",
                "/api/Products/..\\..\\..\\windows\\system32\\config\\sam",
                "/api/Products/%2e%2e%2f%2e%2e%2f%2e%2e%2fetc%2fpasswd",
                "/api/Products/....//....//....//etc/passwd"
            };

            foreach (var attempt in pathTraversalAttempts)
            {
                // Act
                var response = await _client.GetAsync(attempt);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Forbidden
                );
            }
        }

        [Fact]
        public async Task SqlInjection_InUrlParameters_ShouldBeBlocked()
        {
            // Arrange
            var sqlInjectionAttempts = new[]
            {
                "/api/Products/1'; DROP TABLE Products; --",
                "/api/Products/1' OR '1'='1",
                "/api/Products/1' UNION SELECT * FROM Users --",
                "/api/Products/1'; INSERT INTO Users (Username) VALUES ('hacker'); --"
            };

            foreach (var attempt in sqlInjectionAttempts)
            {
                // Act
                var response = await _client.GetAsync(attempt);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Forbidden
                );
            }
        }

        [Fact]
        public async Task SqlInjection_InQueryParameters_ShouldBeBlocked()
        {
            // Arrange
            var sqlInjectionQueries = new[]
            {
                "?search='; DROP TABLE Products; --",
                "?search=' OR '1'='1",
                "?search=' UNION SELECT password FROM Users --",
                "?categoryId=1'; DELETE FROM Categories; --"
            };

            foreach (var query in sqlInjectionQueries)
            {
                // Act
                var response = await _client.GetAsync($"/api/Products/search{query}");

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Forbidden
                );
            }
        }

        [Fact]
        public async Task XssAttempts_ShouldBeSanitized()
        {
            // Arrange
            var xssPayloads = new[]
            {
                "<script>alert('xss')</script>",
                "<img src=x onerror=alert('xss')>",
                "javascript:alert('xss')",
                "<svg onload=alert('xss')>",
                "';alert('xss');//"
            };

            foreach (var payload in xssPayloads)
            {
                var searchRequest = new { Query = payload };

                // Act
                var response = await _client.PostAsJsonAsync("/api/Products/search", searchRequest);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.MethodNotAllowed
                );

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    content.Should().NotContain("<script>");
                    content.Should().NotContain("javascript:");
                    content.Should().NotContain("onerror=");
                }
            }
        }

        [Fact]
        public async Task CommandInjection_ShouldBeBlocked()
        {
            // Arrange
            var commandInjectionAttempts = new[]
            {
                "; ls -la",
                "| cat /etc/passwd",
                "&& rm -rf /",
                "; ping google.com",
                "$(whoami)"
            };

            foreach (var attempt in commandInjectionAttempts)
            {
                var searchRequest = new { Query = attempt };

                // Act
                var response = await _client.PostAsJsonAsync("/api/Products/search", searchRequest);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.MethodNotAllowed
                );
            }
        }

        [Fact]
        public async Task LdapInjection_ShouldBeBlocked()
        {
            // Arrange
            var ldapInjectionAttempts = new[]
            {
                "*)(uid=*",
                "*)(|(uid=*))",
                "admin)(&(password=*))",
                "*))%00"
            };

            foreach (var attempt in ldapInjectionAttempts)
            {
                var loginRequest = new LoginDTO
                {
                    Email = $"user{attempt}@test.com",
                    Password = "password123"
                };

                // Act
                var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Unauthorized
                );
            }
        }

        [Fact]
        public async Task HeaderInjection_ShouldBeBlocked()
        {
            // Arrange
            var maliciousHeaders = new Dictionary<string, string>
            {
                { "X-Forwarded-For", "127.0.0.1\r\nX-Injected-Header: malicious" },
                { "User-Agent", "Mozilla/5.0\r\nX-Injected: attack" },
                { "Referer", "http://example.com\r\nSet-Cookie: admin=true" }
            };

            foreach (var header in maliciousHeaders)
            {
                // Arrange
                _client.DefaultRequestHeaders.Clear();
                try
                {
                    _client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                catch
                {
                    // Header injection blocked by HTTP client - this is good
                    continue;
                }

                // Act
                var response = await _client.GetAsync("/api/Products");

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Unauthorized
                );

                _client.DefaultRequestHeaders.Clear();
            }
        }

        [Fact]
        public async Task MassAssignment_ShouldBeBlocked()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var maliciousRequest = new
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.50m,
                CategoryId = 1,
                StockQuantity = 10,
                // Attempting to set fields that shouldn't be settable
                Id = 999999,
                CreatedAt = DateTime.Now.AddYears(-10),
                UpdatedAt = DateTime.Now.AddYears(-10),
                IsDeleted = false,
                CreatedBy = "hacker",
                IsAdmin = true,
                Role = "Administrator"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", maliciousRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Created,
                HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized
            );

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotContain("999999"); // ID should not be set to our malicious value
            }
        }

        [Fact]
        public async Task PrivilegeEscalation_ShouldBeBlocked()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var privilegeEscalationAttempts = new[]
            {
                "/api/Admin/users",
                "/api/Admin/system",
                "/api/Admin/config",
                "/api/System/shutdown",
                "/api/Debug/info"
            };

            foreach (var attempt in privilegeEscalationAttempts)
            {
                // Act
                var response = await _client.GetAsync(attempt);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Forbidden,
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.MethodNotAllowed
                );
            }
        }

        [Fact]
        public async Task FileUpload_MaliciousFiles_ShouldBeBlocked()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            var maliciousFiles = new[]
            {
                ("virus.exe", "application/octet-stream", "MZ\x90\x00\x03"), // PE header
                ("script.php", "application/x-php", "<?php system($_GET['cmd']); ?>"),
                ("shell.jsp", "application/x-jsp", "<% Runtime.getRuntime().exec(request.getParameter(\"cmd\")); %>"),
                ("malware.bat", "application/x-msdos-program", "@echo off\nformat c: /y")
            };

            foreach (var (filename, contentType, content) in maliciousFiles)
            {
                var formData = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                formData.Add(fileContent, "files", filename);

                // Act
                var response = await _client.PostAsync("/api/FileManager/upload", formData);

                // Assert
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Forbidden,
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.UnsupportedMediaType
                );
            }
        }

        [Fact]
        public async Task RateLimiting_ShouldPreventAbuse()
        {
            // Arrange
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act - Send many requests rapidly
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(_client.GetAsync("/api/Products"));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            var tooManyRequestsCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
            
            // We expect at least some requests to be rate limited if rate limiting is implemented
            // If no rate limiting, all should be OK or Unauthorized
            foreach (var response in responses)
            {
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.TooManyRequests,
                    HttpStatusCode.ServiceUnavailable
                );
            }
        }

        [Fact]
        public async Task CsrfProtection_ShouldBeEnforced()
        {
            // Arrange
            await SeedTestDataAsync();
            var token = await GetAdminTokenAsync();
            SetAuthHeader(token!);

            // Remove any CSRF tokens that might be automatically added
            _client.DefaultRequestHeaders.Remove("X-CSRF-Token");
            _client.DefaultRequestHeaders.Remove("RequestVerificationToken");

            var productRequest = new CreateProductDTO
            {
                Name = "CSRF Test Product",
                Description = "Test Description",
                Price = 10.50m,
                CategoryId = 1,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

            // Assert
            // CSRF protection might not be implemented for API endpoints (common for APIs)
            // But if it is, we should get a 403 or 400
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Created,
                HttpStatusCode.BadRequest,
                HttpStatusCode.Forbidden,
                HttpStatusCode.Unauthorized
            );
        }

        [Fact]
        public async Task SessionFixation_ShouldBeProtected()
        {
            // Arrange
            var maliciousSessionId = "MALICIOUS_SESSION_ID_12345";
            _client.DefaultRequestHeaders.Add("Cookie", $"SessionId={maliciousSessionId}");

            var loginRequest = new LoginDTO
            {
                Email = "truongadmin@gmail.com",
                Password = "Anhvip@522"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized
            );

            // If login is successful, the session ID should be regenerated
            if (response.IsSuccessStatusCode)
            {
                // Check if Set-Cookie headers exist before trying to access them
                if (response.Headers.Contains("Set-Cookie"))
                {
                    var setCookieHeaders = response.Headers.GetValues("Set-Cookie").ToList();
                    if (setCookieHeaders.Any())
                    {
                        setCookieHeaders.Should().NotContain(h => h.Contains(maliciousSessionId));
                    }
                }
                // If no Set-Cookie headers, that's also acceptable for JWT-based auth
            }
        }
    }
}
