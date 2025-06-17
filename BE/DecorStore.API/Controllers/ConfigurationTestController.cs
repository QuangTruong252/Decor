using DecorStore.API.Configuration;
using DecorStore.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationTestController : ControllerBase
    {
        private readonly IOptions<DatabaseSettings> _databaseSettings;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IOptions<FileStorageSettings> _fileStorageSettings;
        private readonly IOptions<CacheSettings> _cacheSettings;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly ApplicationDbContext _context;

        public ConfigurationTestController(
            IOptions<DatabaseSettings> databaseSettings,
            IOptions<JwtSettings> jwtSettings,
            IOptions<FileStorageSettings> fileStorageSettings,
            IOptions<CacheSettings> cacheSettings,
            IOptions<ApiSettings> apiSettings,
            ApplicationDbContext context)
        {
            _databaseSettings = databaseSettings;
            _jwtSettings = jwtSettings;
            _fileStorageSettings = fileStorageSettings;
            _cacheSettings = cacheSettings;
            _apiSettings = apiSettings;
            _context = context;
        }

        [HttpGet("test-configuration")]
        public IActionResult TestConfiguration()
        {
            var result = new
            {
                Database = new
                {
                    HasConnectionString = !string.IsNullOrEmpty(_databaseSettings.Value.ConnectionString),
                    MaxRetryCount = _databaseSettings.Value.MaxRetryCount,
                    CommandTimeout = _databaseSettings.Value.CommandTimeoutSeconds
                },
                JWT = new
                {
                    HasSecretKey = !string.IsNullOrEmpty(_jwtSettings.Value.SecretKey),
                    Issuer = _jwtSettings.Value.Issuer,
                    AccessTokenExpiration = _jwtSettings.Value.AccessTokenExpirationMinutes
                },
                FileStorage = new
                {
                    UploadPath = _fileStorageSettings.Value.UploadPath,
                    MaxFileSize = _fileStorageSettings.Value.MaxFileSizeMB,
                    AllowedExtensions = _fileStorageSettings.Value.AllowedExtensions
                },
                Cache = new
                {
                    DefaultExpiration = _cacheSettings.Value.DefaultExpirationMinutes,
                    EnableCaching = _cacheSettings.Value.EnableCaching,
                    KeyPrefix = _cacheSettings.Value.CacheKeyPrefix
                },
                Api = new
                {
                    DefaultVersion = _apiSettings.Value.DefaultVersion,
                    RequestsPerMinute = _apiSettings.Value.RequestsPerMinute,
                    EnableSwagger = _apiSettings.Value.EnableSwagger
                }
            };

            return Ok(result);
        }

        [HttpGet("test-database")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                // Test basic connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                // Get database info
                var databaseName = _context.Database.GetDbConnection().Database;
                var providerName = _context.Database.ProviderName;
                
                // Test a simple query
                var userCount = await _context.Users.CountAsync();
                var categoryCount = await _context.Categories.CountAsync();
                var productCount = await _context.Products.CountAsync();
                
                var result = new
                {
                    Success = true,
                    CanConnect = canConnect,
                    DatabaseInfo = new
                    {
                        Name = databaseName,
                        Provider = providerName,
                        IsInMemory = providerName?.Contains("InMemory") == true
                    },
                    Statistics = new
                    {
                        Users = userCount,
                        Categories = categoryCount,
                        Products = productCount
                    },
                    ConnectionString = new
                    {
                        HasConnectionString = !string.IsNullOrEmpty(_databaseSettings.Value.ConnectionString),
                        IsConfigured = true
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    DatabaseProvider = _context.Database.ProviderName
                });
            }
        }
    }
}
