using DecorStore.API.Configuration;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Repositories;
using DecorStore.API.Services;
using DecorStore.API.Middleware;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DecorStore.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AutoMapper
            services.AddAutoMapper(typeof(Program).Assembly);

            // Add repositories
            AddRepositories(services);

            // Add business services
            AddBusinessServices(services);

            // Add Excel services
            AddExcelServices(services);

            // Add caching services
            AddCachingServices(services, configuration);

            // Add security services
            AddSecurityServices(services, configuration);

            // Add FluentValidation
            AddValidationServices(services);

            // Add other application services
            services.AddHttpContextAccessor();

            return services;
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<DecorStore.API.Interfaces.Repositories.IImageRepository, DecorStore.API.Repositories.ImageRepository>();
            services.AddScoped<DecorStore.API.Interfaces.Repositories.IOrderItemRepository, DecorStore.API.Repositories.OrderItemRepository>();
            
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, DecorStore.API.Data.UnitOfWork>();
        }        private static void AddBusinessServices(IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IBannerService, BannerService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IImageService, DecorStore.API.Services.ImageService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<DecorStore.API.Interfaces.Services.IFileManagerService, DecorStore.API.Services.FileManagerService>();
            
            // Add correlation ID service
            services.AddScoped<ICorrelationIdService, CorrelationIdService>();
              // Add performance monitoring
            services.AddScoped<IDatabasePerformanceMonitor, DatabasePerformanceMonitor>();
            services.AddScoped<IPerformanceDashboardService, PerformanceDashboardService>();
        }

        private static void AddExcelServices(IServiceCollection services)
        {
            services.AddScoped<DecorStore.API.Services.Excel.IExcelService, DecorStore.API.Services.Excel.ExcelService>();
            services.AddScoped<DecorStore.API.Services.Excel.IProductExcelService, DecorStore.API.Services.Excel.ProductExcelService>();
            services.AddScoped<DecorStore.API.Services.Excel.ICategoryExcelService, DecorStore.API.Services.Excel.CategoryExcelService>();
            services.AddScoped<DecorStore.API.Services.Excel.ICustomerExcelService, DecorStore.API.Services.Excel.CustomerExcelService>();
            services.AddScoped<DecorStore.API.Services.Excel.IOrderExcelService, DecorStore.API.Services.Excel.OrderExcelService>();
        }

        private static void AddCachingServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure CacheSettings
            services.Configure<CacheSettings>(configuration.GetSection("Cache"));
            services.AddSingleton<IValidateOptions<CacheSettings>, CacheSettingsValidator>();

            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>() ?? new CacheSettings();

            // Add memory cache with configuration
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = cacheSettings.DefaultSizeLimit;
            });            
            // Add custom cache service
            services.AddScoped<DecorStore.API.Interfaces.Services.ICacheService, CacheService>();
            
            // Add cache invalidation service
            services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        }

        private static void AddSecurityServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure JWT Security Settings
            services.Configure<JwtSecuritySettings>(configuration.GetSection("JwtSecurity"));
            services.AddSingleton<IValidateOptions<JwtSecuritySettings>, JwtSecuritySettingsValidator>();

            // Configure Password Security Settings
            services.Configure<PasswordSecuritySettings>(configuration.GetSection("PasswordSecurity"));
            services.AddSingleton<IValidateOptions<PasswordSecuritySettings>, PasswordSecuritySettingsValidator>();

            // Configure Data Encryption Settings
            services.Configure<DataEncryptionSettings>(configuration.GetSection("DataEncryption"));
            services.AddSingleton<IValidateOptions<DataEncryptionSettings>, DataEncryptionSettingsValidator>();

            // Configure API Key Settings
            services.Configure<ApiKeySettings>(configuration.GetSection("ApiKey"));
            services.AddSingleton<IValidateOptions<ApiKeySettings>, ApiKeySettingsValidator>();

            // Configure API Key Middleware Settings
            services.Configure<ApiKeyMiddlewareSettings>(configuration.GetSection("ApiKeyMiddleware"));
            services.AddSingleton<IValidateOptions<ApiKeyMiddlewareSettings>, ApiKeyMiddlewareSettingsValidator>();

            // Add security services
            services.AddScoped<IJwtTokenService, JwtTokenService>();            services.AddScoped<Interfaces.Services.ISecurityEventLogger, Services.SecurityEventLogger>();
            services.AddScoped<Interfaces.Services.IPasswordSecurityService, Services.PasswordSecurityService>();
            services.AddScoped<Interfaces.Services.IDataEncryptionService, Services.DataEncryptionService>();
            services.AddScoped<Interfaces.Services.IApiKeyManagementService, Services.ApiKeyManagementService>();

            // Add hosted service for token cleanup
            services.AddHostedService<TokenCleanupService>();
        }

        private static void AddValidationServices(IServiceCollection services)
        {
            // Add FluentValidation for Excel DTOs
            services.AddScoped<IValidator<DecorStore.API.DTOs.Excel.ProductExcelDTO>, DecorStore.API.Validators.Excel.ProductExcelValidator>();
            services.AddScoped<IValidator<DecorStore.API.DTOs.Excel.CategoryExcelDTO>, DecorStore.API.Validators.Excel.CategoryExcelValidator>();
            services.AddScoped<IValidator<DecorStore.API.DTOs.Excel.CustomerExcelDTO>, DecorStore.API.Validators.Excel.CustomerExcelValidator>();
            services.AddScoped<IValidator<DecorStore.API.DTOs.Excel.OrderExcelDTO>, DecorStore.API.Validators.Excel.OrderExcelValidator>();

            // Add core DTO validators
            services.AddScoped<IValidator<CreateProductDTO>, DecorStore.API.Validators.ProductValidators.CreateProductValidator>();
            services.AddScoped<IValidator<UpdateProductDTO>, DecorStore.API.Validators.ProductValidators.UpdateProductValidator>();

            // Add general FluentValidation
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        }

        public static IServiceCollection AddFileStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure FileStorageSettings
            services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
            services.AddSingleton<IValidateOptions<FileStorageSettings>, FileStorageSettingsValidator>();            return services;
        }
    }

    /// <summary>
    /// Validator for CacheSettings configuration
    /// </summary>
    public class CacheSettingsValidator : IValidateOptions<CacheSettings>
    {
        public ValidateOptionsResult Validate(string? name, CacheSettings options)
        {
            var failures = new List<string>();

            if (options.DefaultExpirationMinutes <= 0)
            {
                failures.Add("Cache DefaultExpirationMinutes must be greater than 0");
            }

            if (options.DefaultSizeLimit <= 0)
            {
                failures.Add("Cache DefaultSizeLimit must be greater than 0");
            }

            if (options.SlidingExpirationMinutes < 0)
            {
                failures.Add("Cache SlidingExpirationMinutes cannot be negative");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Validator for FileStorageSettings configuration
    /// </summary>
    public class FileStorageSettingsValidator : IValidateOptions<FileStorageSettings>
    {
        public ValidateOptionsResult Validate(string? name, FileStorageSettings options)
        {
            var failures = new List<string>();

            if (string.IsNullOrEmpty(options.UploadPath))
            {
                failures.Add("FileStorage UploadPath cannot be null or empty");
            }

            if (options.MaxFileSizeMB <= 0)
            {
                failures.Add("FileStorage MaxFileSizeMB must be greater than 0");
            }

            if (options.AllowedExtensions == null || !options.AllowedExtensions.Any())
            {
                failures.Add("FileStorage AllowedExtensions cannot be null or empty");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Validator for JWT Security Settings configuration
    /// </summary>
    public class JwtSecuritySettingsValidator : IValidateOptions<JwtSecuritySettings>
    {
        public ValidateOptionsResult Validate(string? name, JwtSecuritySettings options)
        {
            var failures = new List<string>();

            if (options.AccessTokenExpiryMinutes <= 0 || options.AccessTokenExpiryMinutes > 60)
            {
                failures.Add("JWT AccessTokenExpiryMinutes must be between 1 and 60 minutes");
            }

            if (options.RefreshTokenExpiryDays <= 0 || options.RefreshTokenExpiryDays > 30)
            {
                failures.Add("JWT RefreshTokenExpiryDays must be between 1 and 30 days");
            }

            if (options.MaxRefreshTokenFamilySize <= 0 || options.MaxRefreshTokenFamilySize > 10)
            {
                failures.Add("JWT MaxRefreshTokenFamilySize must be between 1 and 10");
            }

            if (options.TokenBindingDurationMinutes <= 0 || options.TokenBindingDurationMinutes > 60)
            {
                failures.Add("JWT TokenBindingDurationMinutes must be between 1 and 60 minutes");
            }

            if (options.TokenReplayWindowMinutes <= 0 || options.TokenReplayWindowMinutes > 30)
            {
                failures.Add("JWT TokenReplayWindowMinutes must be between 1 and 30 minutes");
            }

            if (options.EnableTokenEncryption && string.IsNullOrEmpty(options.EncryptionKey))
            {
                failures.Add("JWT EncryptionKey is required when token encryption is enabled");
            }

            if (!string.IsNullOrEmpty(options.EncryptionKey) && options.EncryptionKey.Length < 32)
            {
                failures.Add("JWT EncryptionKey must be at least 32 characters long");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Validator for Password Security Settings configuration
    /// </summary>
    public class PasswordSecuritySettingsValidator : IValidateOptions<PasswordSecuritySettings>
    {
        public ValidateOptionsResult Validate(string? name, PasswordSecuritySettings options)
        {
            var failures = new List<string>();

            if (options.MinimumLength < 6 || options.MinimumLength > 50)
            {
                failures.Add("Password MinimumLength must be between 6 and 50 characters");
            }

            if (options.MaximumLength < options.MinimumLength || options.MaximumLength > 256)
            {
                failures.Add("Password MaximumLength must be greater than MinimumLength and not exceed 256 characters");
            }

            if (options.SaltRounds < 10 || options.SaltRounds > 15)
            {
                failures.Add("Password SaltRounds must be between 10 and 15 for optimal security and performance");
            }

            if (options.MaxFailedAccessAttempts <= 0 || options.MaxFailedAccessAttempts > 20)
            {
                failures.Add("Password MaxFailedAccessAttempts must be between 1 and 20");
            }

            if (options.LockoutDurationMinutes <= 0 || options.LockoutDurationMinutes > 1440)
            {
                failures.Add("Password LockoutDurationMinutes must be between 1 and 1440 (24 hours)");
            }

            if (options.PasswordHistoryCount < 0 || options.PasswordHistoryCount > 20)
            {
                failures.Add("Password PasswordHistoryCount must be between 0 and 20");
            }

            if (options.PasswordExpirationDays <= 0 || options.PasswordExpirationDays > 365)
            {
                failures.Add("Password PasswordExpirationDays must be between 1 and 365 days");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Validator for Data Encryption Settings configuration
    /// </summary>
    public class DataEncryptionSettingsValidator : IValidateOptions<DataEncryptionSettings>
    {
        public ValidateOptionsResult Validate(string? name, DataEncryptionSettings options)
        {
            var failures = new List<string>();

            if (string.IsNullOrEmpty(options.MasterKey))
            {
                failures.Add("DataEncryption MasterKey cannot be null or empty");
            }
            else if (options.MasterKey.Length < 32)
            {
                failures.Add("DataEncryption MasterKey must be at least 32 characters long");
            }

            if (string.IsNullOrEmpty(options.Salt))
            {
                failures.Add("DataEncryption Salt cannot be null or empty");
            }
            else if (options.Salt.Length < 16)
            {
                failures.Add("DataEncryption Salt must be at least 16 characters long");
            }

            if (options.KeyDerivationIterations < 10000 || options.KeyDerivationIterations > 1000000)
            {
                failures.Add("DataEncryption KeyDerivationIterations must be between 10,000 and 1,000,000");
            }

            if (options.CurrentKeyVersion < 1)
            {
                failures.Add("DataEncryption CurrentKeyVersion must be at least 1");
            }

            if (options.KeyRotationDays <= 0 || options.KeyRotationDays > 365)
            {
                failures.Add("DataEncryption KeyRotationDays must be between 1 and 365 days");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Validator for API Key Settings configuration
    /// </summary>
    public class ApiKeySettingsValidator : IValidateOptions<ApiKeySettings>
    {
        public ValidateOptionsResult Validate(string? name, ApiKeySettings options)
        {
            var failures = new List<string>();

            if (options.DefaultExpirationDays <= 0 || options.DefaultExpirationDays > 3650)
            {
                failures.Add("ApiKey DefaultExpirationDays must be between 1 and 3650 days (10 years)");
            }

            if (options.DefaultRateLimitPerHour <= 0 || options.DefaultRateLimitPerHour > 100000)
            {
                failures.Add("ApiKey DefaultRateLimitPerHour must be between 1 and 100,000");
            }

            if (options.DefaultRateLimitPerDay <= 0 || options.DefaultRateLimitPerDay > 1000000)
            {
                failures.Add("ApiKey DefaultRateLimitPerDay must be between 1 and 1,000,000");
            }

            if (options.CleanupExpiredKeysAfterDays <= 0 || options.CleanupExpiredKeysAfterDays > 365)
            {
                failures.Add("ApiKey CleanupExpiredKeysAfterDays must be between 1 and 365 days");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Validator for API Key Middleware Settings configuration
    /// </summary>
    public class ApiKeyMiddlewareSettingsValidator : IValidateOptions<ApiKeyMiddlewareSettings>
    {
        public ValidateOptionsResult Validate(string? name, ApiKeyMiddlewareSettings options)
        {
            var failures = new List<string>();

            if (options.MaxSuspiciousRequestsPerHour <= 0 || options.MaxSuspiciousRequestsPerHour > 10000)
            {
                failures.Add("ApiKeyMiddleware MaxSuspiciousRequestsPerHour must be between 1 and 10,000");
            }

            if (options.ExemptPaths == null)
            {
                failures.Add("ApiKeyMiddleware ExemptPaths cannot be null");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}
