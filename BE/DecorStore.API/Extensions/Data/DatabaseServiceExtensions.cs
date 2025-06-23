using DecorStore.API.Configuration;
using DecorStore.API.Data;
using DecorStore.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DecorStore.API.Extensions.Data
{
    public static class DatabaseServiceExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {            // Configure and validate DatabaseSettings
            services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
            services.AddSingleton<IValidateOptions<DatabaseSettings>, DatabaseSettingsValidator>();
            
            // Enable options validation on startup
            services.AddOptions<DatabaseSettings>()
                .Bind(configuration.GetSection("Database"))
                .ValidateOnStart();

            // Get connection string from configuration or DatabaseSettings
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured");
            }            // Check if we should use in-memory database (for development/testing when SQL Server is not available)
            var useInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase", false);

            // Also check environment for testing
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Test")
            {
                useInMemoryDatabase = true;
            }
            
            // Configure DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {                if (useInMemoryDatabase)
                {
                    // Use in-memory database for development/testing
                    options.UseInMemoryDatabase("DecorStoreInMemory");
                    options.EnableSensitiveDataLogging(true);
                    options.EnableDetailedErrors(true);
                      // Configure warnings to ignore transaction warnings for in-memory database
                    options.ConfigureWarnings(warnings =>
                    {
                        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning);
                    });
                }
                else
                {
                    // Use SQL Server for production/when available
                    var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>() ?? new DatabaseSettings();
                    
                    // Build optimized connection string with pooling settings
                    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
                    {
                        // Connection Pool Optimization
                        Pooling = true,
                        MinPoolSize = databaseSettings.MinPoolSize,
                        MaxPoolSize = databaseSettings.MaxPoolSize,
                        ConnectTimeout = databaseSettings.ConnectionTimeoutSeconds,
                        CommandTimeout = databaseSettings.CommandTimeoutSeconds,
                        
                        // Performance optimizations
                        ApplicationName = "DecorStoreAPI",
                        ConnectRetryCount = 3,
                        ConnectRetryInterval = 10,
                        
                        // Security settings
                        Encrypt = databaseSettings.EnableEncryption,
                        TrustServerCertificate = databaseSettings.TrustServerCertificate,
                        
                        // Additional performance settings
                        MultipleActiveResultSets = true,
                        LoadBalanceTimeout = 0
                    };
                    
                    options.UseSqlServer(builder.ConnectionString, sqlServerOptions =>
                    {
                        // Configure retry policy for transient failures
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: databaseSettings.MaxRetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(databaseSettings.MaxRetryDelaySeconds),
                            errorNumbersToAdd: null);

                        // Configure migration history table
                        sqlServerOptions.MigrationsHistoryTable(databaseSettings.MigrationHistoryTable);
                        
                        // Set command timeout for long-running queries
                        sqlServerOptions.CommandTimeout(databaseSettings.CommandTimeoutSeconds);
                    });

                    // Performance optimizations
                    options.EnableServiceProviderCaching(true);
                    options.EnableSensitiveDataLogging(databaseSettings.EnableSensitiveDataLogging);
                    options.EnableDetailedErrors(databaseSettings.EnableDetailedErrors);
                    
                    // Configure query tracking behavior for performance
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                    
                    // Query optimization
                    options.ConfigureWarnings(warnings =>
                    {
                        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.DetachedLazyLoadingWarning);
                    });
                    
                    // Configure logging for slow queries
                    if (databaseSettings.EnableDetailedErrors)
                    {
                        options.LogTo(message => System.Diagnostics.Debug.WriteLine(message), 
                            new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted },
                            LogLevel.Information,
                            Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.SingleLine);
                    }
                }
            }, ServiceLifetime.Scoped); // Use Scoped lifetime for better connection pooling

            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();            // Note: Database health checks are configured in HealthCheckExtensions.cs to avoid duplicates

            return services;
        }
    }

    /// <summary>
    /// Validator for DatabaseSettings configuration
    /// </summary>
    public class DatabaseSettingsValidator : IValidateOptions<DatabaseSettings>
    {
        public ValidateOptionsResult Validate(string? name, DatabaseSettings options)
        {
            var failures = new List<string>();

            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                failures.Add("Database connection string cannot be null or empty");
            }

            if (options.MaxRetryCount < 1 || options.MaxRetryCount > 10)
            {
                failures.Add("Database MaxRetryCount must be between 1 and 10");
            }

            if (options.MaxRetryDelaySeconds < 1 || options.MaxRetryDelaySeconds > 300)
            {
                failures.Add("Database MaxRetryDelaySeconds must be between 1 and 300");
            }

            if (string.IsNullOrEmpty(options.MigrationHistoryTable))
            {
                failures.Add("Database MigrationHistoryTable cannot be null or empty");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}
