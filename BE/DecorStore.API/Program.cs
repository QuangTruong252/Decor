using System.Text.Json.Serialization;
using DecorStore.API.Extensions;
using DecorStore.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON options for MVC controllers to ensure proper model binding
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add HttpClient support
builder.Services.AddHttpClient();

// Add optimized JSON serialization
builder.Services.AddOptimizedJsonSerialization();

// Add response compression
DecorStore.API.Extensions.CompressionServiceExtensions.AddResponseCompressionServices(builder.Services);

// Add service extensions
builder.Services.AddDatabaseServices(builder.Configuration);
// builder.Services.AddDatabaseOptimizationServices(builder.Configuration);
builder.Services.AddDistributedCacheServices(builder.Configuration);
builder.Services.AddPerformanceServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddCorsServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddFileStorageServices(builder.Configuration);
builder.Services.AddLoggingServices(builder.Configuration);
builder.Services.AddHealthCheckServices(builder.Configuration);
builder.Services.AddSecurityServices(builder.Configuration);
builder.Services.AddValidationServices(builder.Configuration);
builder.Services.AddSwaggerServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Check if running in test environment for simplified pipeline
var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Test", StringComparison.OrdinalIgnoreCase) == true ||
                       app.Configuration["UseInMemoryDatabase"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

Console.WriteLine($"[MIDDLEWARE-PIPELINE] Test Environment Detected: {isTestEnvironment}");

if (isTestEnvironment)
{
    Console.WriteLine($"[MIDDLEWARE-PIPELINE] Configuring simplified middleware pipeline for test environment");

    // Simplified middleware pipeline for test environment
    // IMPORTANT: Exception handling must be FIRST to catch all exceptions
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Use response caching (needed for [ResponseCache] attributes in controllers)
    app.UseResponseCaching();

    // Use CORS - MUST be before authentication
    app.UseCors("AllowAll");

    // Use Authentication and Authorization - CRITICAL: Must be in correct order
    app.UseAuthentication();
    app.UseAuthorization();
}
else
{
    Console.WriteLine($"[MIDDLEWARE-PIPELINE] Configuring full middleware pipeline for production environment");
    
    // Full middleware pipeline for production environment
    // IMPORTANT: Exception handling must be FIRST to catch all exceptions
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    app.UseSwaggerMiddleware();

    // Use response compression (early in pipeline)
    app.UseResponseCompression();

    // Use response caching (before performance middleware)
    app.UseResponseCaching();

    // Use CORS - MUST be before authentication
    var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "Production";
    app.UseCors(corsPolicy);

    // Use Authentication and Authorization - CRITICAL: Must be in correct order
    app.UseAuthentication();
    app.UseAuthorization();

    // Use performance middleware (minimal logging to avoid stream conflicts)
    app.UsePerformanceMiddleware();

    // Use security middleware
    app.UseSecurityMiddleware();

    // Use logging middleware (simplified to avoid stream conflicts)
    app.UseLoggingMiddleware();

    // Use File Storage Middleware
    app.UseFileStorageMiddleware();
}

app.MapControllers();

// Map health check endpoints
app.UseHealthCheckEndpoints();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
