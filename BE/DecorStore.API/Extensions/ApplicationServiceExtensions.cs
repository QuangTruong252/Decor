using DecorStore.API.Configuration;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Repositories;
using DecorStore.API.Services;
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
            });            // Add custom cache service
            services.AddScoped<DecorStore.API.Interfaces.Services.ICacheService, CacheService>();
            
            // Add cache invalidation service
            services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
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
}
