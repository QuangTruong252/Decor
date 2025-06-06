using DecorStore.API.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

        public ConfigurationTestController(
            IOptions<DatabaseSettings> databaseSettings,
            IOptions<JwtSettings> jwtSettings,
            IOptions<FileStorageSettings> fileStorageSettings,
            IOptions<CacheSettings> cacheSettings,
            IOptions<ApiSettings> apiSettings)
        {
            _databaseSettings = databaseSettings;
            _jwtSettings = jwtSettings;
            _fileStorageSettings = fileStorageSettings;
            _cacheSettings = cacheSettings;
            _apiSettings = apiSettings;
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
    }
}
