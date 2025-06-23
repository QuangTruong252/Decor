using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using DecorStore.API.Data;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DecorStore.API.DTOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DecorStore.Test
{
    public class TestBase : IDisposable
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;
        protected readonly IServiceScope _scope;
        protected readonly ApplicationDbContext _context;
        protected readonly JsonSerializerOptions _jsonOptions;

        public TestBase()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // CRITICAL: Set environment variable before UseEnvironment
                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
                    builder.UseEnvironment("Test");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        // Override with test-specific settings
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["UseInMemoryDatabase"] = "true",
                            ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                            ["JWT:SecretKey"] = "p9vB7z!Qw3rT6yU2eX8sZ4cL1nM0aJ5hR@kF#GdS$WqE^VbN*YjP",
                            ["JWT:Issuer"] = "DecorStore",
                            ["JWT:Audience"] = "DecorStoreClients",
                            ["JWT:AccessTokenExpirationMinutes"] = "60",
                            ["JWT:RefreshTokenExpirationDays"] = "7",
                            ["JWT:ClockSkewMinutes"] = "5",
                            ["JWT:RequireHttpsMetadata"] = "false",
                            ["JWT:SaveToken"] = "true",
                            ["JWT:ValidateIssuer"] = "true",
                            ["JWT:ValidateAudience"] = "true",
                            ["JWT:ValidateLifetime"] = "true",
                            ["JWT:ValidateIssuerSigningKey"] = "true",
                            ["JWT:EnableDebugEvents"] = "true",
                            ["JwtSecurity:EnableTokenEncryption"] = "false",
                            ["JwtSecurity:AccessTokenExpiryMinutes"] = "60",
                            ["JwtSecurity:RefreshTokenExpiryDays"] = "7",
                            ["JwtSecurity:RequireHttpsMetadata"] = "false",
                            ["JwtSecurity:ValidateIssuer"] = "true",
                            ["JwtSecurity:ValidateAudience"] = "true",
                            ["JwtSecurity:EnableTokenBlacklisting"] = "false",
                            ["JwtSecurity:EnableTokenRotation"] = "false",
                            ["JwtSecurity:EnableTokenReplayProtection"] = "false",
                            ["JwtSecurity:EnableSecureTokenStorage"] = "false",
                            ["JwtSecurity:EnableTokenAuditLogging"] = "false",
                            ["Cache:EnableCaching"] = "false",
                            ["Cache:EnableDistributedCache"] = "false",
                            ["PasswordSecurity:RequireUppercase"] = "false",
                            ["PasswordSecurity:RequireLowercase"] = "false",
                            ["PasswordSecurity:RequireDigit"] = "false",
                            ["PasswordSecurity:RequireSpecialCharacter"] = "false",
                            ["PasswordSecurity:MinimumLength"] = "6",
                            ["PasswordSecurity:EnableAccountLockout"] = "false",
                            ["Api:EnableSwagger"] = "false"
                        });
                    });

                    builder.ConfigureServices(services =>
                    {
                        // Enable detailed logging for JWT debugging
                        services.AddLogging(builder =>
                        {
                            builder.SetMinimumLevel(LogLevel.Information);
                            builder.AddConsole();
                        });

                        // Add debug middleware to check if JWT middleware is registered
                        services.AddTransient<DebugJwtMiddleware>();

                        // Add response caching for test environment to support [ResponseCache] attributes
                        services.AddResponseCaching();

                        // The authentication setup is now handled in AuthenticationServiceExtensions
                        // based on the Test environment variable, so no additional configuration needed here
                    });
                });

            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            _context.Database.EnsureCreated();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Match server's camelCase expectation
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                // Add the same converters as the server to ensure compatibility
                Converters =
                {
                    new JsonStringEnumConverter()
                },
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        protected async Task<string?> GetAuthTokenAsync(string email = "truongadmin@gmail.com", string password = "Anhvip@522")
        {
            // First register the admin user if not exists
            await RegisterAdminUserAsync();

            var loginDto = new LoginDTO
            {
                Email = email,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await DeserializeResponseAsync<AuthResponseDTO>(response);
                return authResponse?.Token;
            }

            return null;
        }

        protected async Task<string?> GetAdminTokenAsync()
        {
            return await GetAuthTokenAsync("truongadmin@gmail.com", "Anhvip@522");
        }

        protected async Task RegisterAdminUserAsync()
        {
            // Check if admin user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "truongadmin@gmail.com");
            if (existingUser != null)
            {
                // Update existing user to admin if not already
                if (existingUser.Role != "Admin")
                {
                    existingUser.Role = "Admin";
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            // Create admin user directly in database
            var adminUser = new DecorStore.API.Models.User
            {
                Username = "truongadmin",
                Email = "truongadmin@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Anhvip@522"),
                FullName = "truong tran",
                Phone = "123456789",
                Role = "Admin", // Set as admin directly
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }

        protected async Task<bool> RegisterTestUserAsync(string email, string password, bool isAdmin = false)
        {
            var registerDto = new RegisterDTO
            {
                Username = email.Split('@')[0],
                Email = email,
                Password = password,
                ConfirmPassword = password,
                FirstName = "Test",
                LastName = "User",
                Phone = "+1234567890",
                DateOfBirth = DateTime.Now.AddYears(-25),
                AcceptTerms = true,
                AcceptPrivacyPolicy = true
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
            
            if (response.IsSuccessStatusCode && isAdmin)
            {
                // Make user admin
                var token = await GetAdminTokenAsync();
                if (token != null)
                {
                    _client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    await _client.PostAsJsonAsync("/api/Auth/make-admin", new { Email = email });
                    
                    _client.DefaultRequestHeaders.Authorization = null;
                }
            }

            return response.IsSuccessStatusCode;
        }

        protected void SetAuthHeader(string token)
        {
            // Clear any existing authorization header first
            _client.DefaultRequestHeaders.Authorization = null;
            
            // Also clear any Authorization headers from default headers
            _client.DefaultRequestHeaders.Remove("Authorization");

            // Set the new authorization header using the proper property
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"[TEST-AUTH] Set authorization header with token: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
            Console.WriteLine($"[TEST-AUTH] Authorization header set: {_client.DefaultRequestHeaders.Authorization}");
            Console.WriteLine($"[TEST-AUTH] All default headers: {string.Join(", ", _client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
        }

        protected void ClearAuthHeader()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        protected async Task<HttpResponseMessage> PostAsJsonWithOptionsAsync<T>(string requestUri, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            Console.WriteLine($"[DEBUG] Sending JSON: {json}");
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return await _client.PostAsync(requestUri, content);
        }

        protected async Task SeedTestDataAsync()
        {
            // Generate unique identifier for this test run to avoid conflicts
            var testId = Guid.NewGuid().ToString("N")[..8]; // Use first 8 characters of GUID

            // Check if data already exists to avoid duplicates
            var existingCategories = await _context.Categories.AnyAsync();
            if (existingCategories)
            {
                return; // Data already seeded for this test context
            }

            // Add test categories with unique slugs
            var categories = new[]
            {
                new DecorStore.API.Models.Category { Name = $"Living Room {testId}", Slug = $"living-room-{testId}", Description = "Living room decor" },
                new DecorStore.API.Models.Category { Name = $"Bedroom {testId}", Slug = $"bedroom-{testId}", Description = "Bedroom decor" },
                new DecorStore.API.Models.Category { Name = $"Kitchen {testId}", Slug = $"kitchen-{testId}", Description = "Kitchen decor" }
            };

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();

            // Add test products with unique names
            var products = new[]
            {
                new DecorStore.API.Models.Product
                {
                    Name = $"Test Sofa {testId}",
                    Slug = $"test-sofa-{testId}",
                    Description = "A comfortable test sofa",
                    Price = 999.99m,
                    CategoryId = categories[0].Id,
                    SKU = $"SOFA{testId}",
                    IsActive = true,
                    IsFeatured = true,
                    StockQuantity = 10
                },
                new DecorStore.API.Models.Product
                {
                    Name = $"Test Bed {testId}",
                    Slug = $"test-bed-{testId}",
                    Description = "A comfortable test bed",
                    Price = 1299.99m,
                    CategoryId = categories[1].Id,
                    SKU = $"BED{testId}",
                    IsActive = true,
                    IsFeatured = false,
                    StockQuantity = 5
                },
                new DecorStore.API.Models.Product
                {
                    Name = $"Test Table {testId}",
                    Slug = $"test-table-{testId}",
                    Description = "A sturdy test table",
                    Price = 599.99m,
                    CategoryId = categories[2].Id,
                    SKU = $"TABLE{testId}",
                    IsActive = true,
                    IsFeatured = true,
                    StockQuantity = 8
                }
            };

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }

        protected byte[] CreateTestImageContent()
        {
            // Create a minimal valid JPEG header
            // This is a very basic JPEG file structure for testing
            var jpegHeader = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
                0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
                0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
                0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
                0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
                0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
                0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
                0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x01,
                0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01,
                0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0xFF, 0xC4,
                0x00, 0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x0C,
                0x03, 0x01, 0x00, 0x02, 0x11, 0x03, 0x11, 0x00, 0x3F, 0x00, 0xB2, 0xC0,
                0x07, 0xFF, 0xD9
            };

            return jpegHeader;
        }

        public void Dispose()
        {
            _context?.Dispose();
            _scope?.Dispose();
            _client?.Dispose();
            _factory?.Dispose();
        }
    }
}
