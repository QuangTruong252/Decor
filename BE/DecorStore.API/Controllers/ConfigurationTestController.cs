using DecorStore.API.Configuration;
using DecorStore.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IWebHostEnvironment _environment;

        public ConfigurationTestController(
            IOptions<DatabaseSettings> databaseSettings,
            IOptions<JwtSettings> jwtSettings,
            IOptions<FileStorageSettings> fileStorageSettings,
            IOptions<CacheSettings> cacheSettings,
            IOptions<ApiSettings> apiSettings,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _databaseSettings = databaseSettings;
            _jwtSettings = jwtSettings;
            _fileStorageSettings = fileStorageSettings;
            _cacheSettings = cacheSettings;
            _apiSettings = apiSettings;
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Get basic configuration information
        /// </summary>
        [HttpGet]
        public IActionResult GetConfiguration()
        {
            var result = new
            {
                Configuration = new
                {
                    Environment = _environment.EnvironmentName,
                    ApplicationName = _environment.ApplicationName,
                    Database = new
                    {
                        Provider = _context.Database.ProviderName,
                        IsConfigured = !string.IsNullOrEmpty(_databaseSettings.Value.ConnectionString)
                    },
                    Cache = new
                    {
                        Enabled = _cacheSettings.Value.EnableCaching,
                        DefaultExpirationMinutes = _cacheSettings.Value.DefaultExpirationMinutes
                    },
                    Api = new
                    {
                        Version = _apiSettings.Value.DefaultVersion,
                        SwaggerEnabled = _apiSettings.Value.EnableSwagger
                    }
                }
            };

            return Ok(result);
        }

        /// <summary>
        /// Validate configuration settings
        /// </summary>
        [HttpGet("validate")]
        public IActionResult ValidateConfiguration()
        {
            var validationResults = new List<object>();

            // Validate Database settings
            validationResults.Add(new
            {
                Component = "Database",
                IsValid = !string.IsNullOrEmpty(_databaseSettings.Value.ConnectionString),
                Message = string.IsNullOrEmpty(_databaseSettings.Value.ConnectionString) 
                    ? "Connection string is missing" 
                    : "Database configuration is valid"
            });

            // Validate JWT settings
            validationResults.Add(new
            {
                Component = "JWT",
                IsValid = !string.IsNullOrEmpty(_jwtSettings.Value.SecretKey),
                Message = string.IsNullOrEmpty(_jwtSettings.Value.SecretKey) 
                    ? "JWT secret key is missing" 
                    : "JWT configuration is valid"
            });

            // Validate File Storage settings
            validationResults.Add(new
            {
                Component = "FileStorage",
                IsValid = !string.IsNullOrEmpty(_fileStorageSettings.Value.UploadPath),
                Message = string.IsNullOrEmpty(_fileStorageSettings.Value.UploadPath) 
                    ? "Upload path is not configured" 
                    : "File storage configuration is valid"
            });

            var allValid = validationResults.All(r => (bool)r.GetType().GetProperty("IsValid")!.GetValue(r)!);

            return Ok(new
            {
                IsValid = allValid,
                ValidationResults = validationResults,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get environment information
        /// </summary>
        [HttpGet("environment")]
        public IActionResult GetEnvironmentInfo()
        {
            var result = new
            {
                Environment = new
                {
                    Name = _environment.EnvironmentName,
                    ApplicationName = _environment.ApplicationName,
                    ContentRootPath = _environment.ContentRootPath,
                    WebRootPath = _environment.WebRootPath,
                    IsDevelopment = _environment.IsDevelopment(),
                    IsProduction = _environment.IsProduction(),
                    IsStaging = _environment.IsStaging()
                },
                System = new
                {
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    Version = Environment.Version.ToString()
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }

        /// <summary>
        /// Get database settings (Admin only)
        /// </summary>
        [HttpGet("database")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDatabaseSettings()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var databaseName = _context.Database.GetDbConnection().Database;
                var providerName = _context.Database.ProviderName;
                
                var userCount = await _context.Users.CountAsync();
                var categoryCount = await _context.Categories.CountAsync();
                var productCount = await _context.Products.CountAsync();
                
                var result = new
                {
                    Database = new
                    {
                        Name = databaseName,
                        Provider = providerName,
                        CanConnect = canConnect,
                        IsInMemory = providerName?.Contains("InMemory") == true,
                        Statistics = new
                        {
                            Users = userCount,
                            Categories = categoryCount,
                            Products = productCount
                        },
                        Settings = new
                        {
                            MaxRetryCount = _databaseSettings.Value.MaxRetryCount,
                            CommandTimeout = _databaseSettings.Value.CommandTimeoutSeconds,
                            EnableSensitiveDataLogging = _databaseSettings.Value.EnableSensitiveDataLogging
                        }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Database connection failed",
                    Message = ex.Message,
                    Provider = _context.Database.ProviderName
                });
            }
        }

        /// <summary>
        /// Get security settings (Admin only)
        /// </summary>
        [HttpGet("security")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetSecuritySettings()
        {
            var result = new
            {
                Security = new
                {
                    JWT = new
                    {
                        Issuer = _jwtSettings.Value.Issuer,
                        Audience = _jwtSettings.Value.Audience,
                        AccessTokenExpirationMinutes = _jwtSettings.Value.AccessTokenExpirationMinutes,
                        RefreshTokenExpirationDays = _jwtSettings.Value.RefreshTokenExpirationDays,
                        HasSecretKey = !string.IsNullOrEmpty(_jwtSettings.Value.SecretKey),
                        ValidateIssuer = _jwtSettings.Value.ValidateIssuer,
                        ValidateAudience = _jwtSettings.Value.ValidateAudience,
                        ValidateLifetime = _jwtSettings.Value.ValidateLifetime
                    },
                    FileStorage = new
                    {
                        MaxFileSizeMB = _fileStorageSettings.Value.MaxFileSizeMB,
                        AllowedExtensions = _fileStorageSettings.Value.AllowedExtensions,
                        UploadPath = _fileStorageSettings.Value.UploadPath,
                        EnableImageOptimization = _fileStorageSettings.Value.EnableImageOptimization
                    }
                }
            };

            return Ok(result);
        }

        /// <summary>
        /// Get cache settings (Admin only)
        /// </summary>
        [HttpGet("cache")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetCacheSettings()
        {
            var result = new
            {
                Cache = new
                {
                    EnableCaching = _cacheSettings.Value.EnableCaching,
                    DefaultExpirationMinutes = _cacheSettings.Value.DefaultExpirationMinutes,
                    SlidingExpirationMinutes = _cacheSettings.Value.SlidingExpirationMinutes,
                    DefaultSizeLimit = _cacheSettings.Value.DefaultSizeLimit,
                    CacheKeyPrefix = _cacheSettings.Value.CacheKeyPrefix,
                    EnableDistributedCache = _cacheSettings.Value.EnableDistributedCache
                }
            };

            return Ok(result);
        }

        /// <summary>
        /// Test configuration
        /// </summary>
        [HttpPost("test")]
        public IActionResult TestConfiguration()
        {
            var testResults = new List<object>();

            try
            {
                // Test Database
                var dbTest = _context.Database.CanConnectAsync().Result;
                testResults.Add(new
                {
                    Test = "Database Connection",
                    Success = dbTest,
                    Message = dbTest ? "Database connection successful" : "Database connection failed"
                });
            }
            catch (Exception ex)
            {
                testResults.Add(new
                {
                    Test = "Database Connection",
                    Success = false,
                    Message = $"Database test failed: {ex.Message}"
                });
            }

            // Test JWT Configuration
            testResults.Add(new
            {
                Test = "JWT Configuration",
                Success = !string.IsNullOrEmpty(_jwtSettings.Value.SecretKey),
                Message = !string.IsNullOrEmpty(_jwtSettings.Value.SecretKey) ? "JWT configuration valid" : "JWT secret key missing"
            });

            // Test File Storage Configuration
            testResults.Add(new
            {
                Test = "File Storage Configuration",
                Success = !string.IsNullOrEmpty(_fileStorageSettings.Value.UploadPath),
                Message = !string.IsNullOrEmpty(_fileStorageSettings.Value.UploadPath) ? "File storage configuration valid" : "Upload path not configured"
            });

            var allTestsPassed = testResults.All(r => (bool)r.GetType().GetProperty("Success")!.GetValue(r)!);

            return Ok(new
            {
                OverallSuccess = allTestsPassed,
                TestResults = testResults,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get all settings (Admin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllSettings()
        {
            var result = new
            {
                Settings = new
                {
                    Database = new
                    {
                        Provider = _context.Database.ProviderName,
                        HasConnectionString = !string.IsNullOrEmpty(_databaseSettings.Value.ConnectionString),
                        MaxRetryCount = _databaseSettings.Value.MaxRetryCount,
                        CommandTimeout = _databaseSettings.Value.CommandTimeoutSeconds,
                        EnableSensitiveDataLogging = _databaseSettings.Value.EnableSensitiveDataLogging
                    },
                    JWT = new
                    {
                        Issuer = _jwtSettings.Value.Issuer,
                        Audience = _jwtSettings.Value.Audience,
                        AccessTokenExpirationMinutes = _jwtSettings.Value.AccessTokenExpirationMinutes,
                        RefreshTokenExpirationDays = _jwtSettings.Value.RefreshTokenExpirationDays,
                        HasSecretKey = !string.IsNullOrEmpty(_jwtSettings.Value.SecretKey)
                    },
                    FileStorage = new
                    {
                        UploadPath = _fileStorageSettings.Value.UploadPath,
                        MaxFileSizeMB = _fileStorageSettings.Value.MaxFileSizeMB,
                        AllowedExtensions = _fileStorageSettings.Value.AllowedExtensions
                    },
                    Cache = _cacheSettings.Value,
                    Api = _apiSettings.Value
                },
                Environment = _environment.EnvironmentName,
                Timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }

        /// <summary>
        /// Get connection strings (Admin only)
        /// </summary>
        [HttpGet("connections")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetConnectionStrings()
        {
            var result = new
            {
                Connections = new
                {
                    Database = new
                    {
                        HasConnectionString = !string.IsNullOrEmpty(_databaseSettings.Value.ConnectionString),
                        Provider = _context.Database.ProviderName,
                        DatabaseName = _context.Database.GetDbConnection().Database
                    },
                    Cache = new
                    {
                        EnableDistributedCache = _cacheSettings.Value.EnableDistributedCache,
                        CacheProvider = _cacheSettings.Value.EnableDistributedCache ? "Redis" : "InMemory"
                    }
                }
            };

            return Ok(result);
        }

        /// <summary>
        /// Get feature flags
        /// </summary>
        [HttpGet("features")]
        public IActionResult GetFeatureFlags()
        {
            var result = new
            {
                Features = new
                {
                    SwaggerEnabled = _apiSettings.Value.EnableSwagger,
                    CachingEnabled = _cacheSettings.Value.EnableCaching,
                    DistributedCacheEnabled = _cacheSettings.Value.EnableDistributedCache,
                    DatabaseLoggingEnabled = _databaseSettings.Value.EnableSensitiveDataLogging,
                    FileStorageEnabled = !string.IsNullOrEmpty(_fileStorageSettings.Value.UploadPath),
                    ImageOptimizationEnabled = _fileStorageSettings.Value.EnableImageOptimization
                },
                Environment = _environment.EnvironmentName,
                Timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }

        /// <summary>
        /// Legacy test configuration endpoint
        /// </summary>
        [HttpGet("test-configuration")]
        public IActionResult TestConfigurationLegacy()
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

        /// <summary>
        /// Legacy test database endpoint
        /// </summary>
        [HttpGet("test-database")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var databaseName = _context.Database.GetDbConnection().Database;
                var providerName = _context.Database.ProviderName;
                
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
