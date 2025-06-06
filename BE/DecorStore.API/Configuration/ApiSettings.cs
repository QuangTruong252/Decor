using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.Configuration
{
    public class ApiSettings
    {
        [Required(ErrorMessage = "API version is required")]
        [RegularExpression(@"^v\d+(\.\d+)?$", ErrorMessage = "API version must be in format 'v1' or 'v1.0'")]
        public string DefaultVersion { get; set; } = "v1";

        [Required(ErrorMessage = "Supported versions list is required")]
        [MinLength(1, ErrorMessage = "At least one supported version must be specified")]
        public string[] SupportedVersions { get; set; } = new[] { "v1" };

        [Range(1, 10000, ErrorMessage = "Requests per minute must be between 1 and 10000")]
        public int RequestsPerMinute { get; set; } = 100;

        [Range(1, 1000, ErrorMessage = "Burst limit must be between 1 and 1000")]
        public int BurstLimit { get; set; } = 200;

        [Required(ErrorMessage = "Allowed origins list is required")]
        public string[] AllowedOrigins { get; set; } = new[] { "https://localhost", "http://localhost" };

        [Required(ErrorMessage = "Allowed headers list is required")]
        public string[] AllowedHeaders { get; set; } = new[] { "Content-Type", "Authorization" };

        [Required(ErrorMessage = "Allowed methods list is required")]
        public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

        public bool AllowCredentials { get; set; } = true;

        public bool EnableSwagger { get; set; } = true;

        [Required(ErrorMessage = "Swagger endpoint is required")]
        public string SwaggerEndpoint { get; set; } = "/swagger/v1/swagger.json";

        [Required(ErrorMessage = "Swagger title is required")]
        public string SwaggerTitle { get; set; } = "DecorStore API";

        [Required(ErrorMessage = "Default log level is required")]
        public string DefaultLogLevel { get; set; } = "Information";

        [Required(ErrorMessage = "Microsoft log level is required")]
        public string MicrosoftLogLevel { get; set; } = "Warning";

        public bool EnableSensitiveDataLogging { get; set; } = false;

        public bool EnableDetailedErrors { get; set; } = false;

        [Range(1, 300, ErrorMessage = "Request timeout must be between 1 and 300 seconds")]
        public int RequestTimeoutSeconds { get; set; } = 30;

        [Range(1, 1000, ErrorMessage = "Max request body size must be between 1 and 1000 MB")]
        public int MaxRequestBodySizeMB { get; set; } = 100;

        public bool EnableCompression { get; set; } = true;

        public bool EnableResponseCaching { get; set; } = true;

        [Range(1, 86400, ErrorMessage = "Default cache duration must be between 1 and 86400 seconds")]
        public int DefaultCacheDurationSeconds { get; set; } = 300;
    }
}
