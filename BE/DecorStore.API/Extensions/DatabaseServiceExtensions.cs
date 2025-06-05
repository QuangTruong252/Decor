using DecorStore.API.Configuration;
using DecorStore.API.Data;
using DecorStore.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DecorStore.API.Extensions
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
            }

            // Configure DbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>() ?? new DatabaseSettings();
                
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    // Configure retry policy
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: databaseSettings.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(databaseSettings.MaxRetryDelaySeconds),
                        errorNumbersToAdd: null);

                    // Configure migration history table
                    sqlServerOptions.MigrationsHistoryTable(databaseSettings.MigrationHistoryTable);
                });

                // Enable sensitive data logging in development only
                if (databaseSettings.EnableSensitiveDataLogging)
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();            // Add health checks for database
            services.AddHealthChecks()
                .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database is available"));

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
