using System.Text.Json.Serialization;
using DecorStore.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add optimized JSON serialization
builder.Services.AddOptimizedJsonSerialization();

// Add response compression
builder.Services.AddResponseCompressionServices();

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
app.UseSwaggerMiddleware();

// Use response compression (early in pipeline)
app.UseResponseCompression();

// Use performance middleware (early in pipeline for compression)
app.UsePerformanceMiddleware();

// Use validation middleware (early in pipeline)
app.UseValidationMiddleware();

// Use security middleware
app.UseSecurityMiddleware();

// Use logging middleware
app.UseLoggingMiddleware();

// Use CORS - determine policy based on environment
var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "Production";
app.UseCors(corsPolicy);

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Use File Storage Middleware
app.UseFileStorageMiddleware();

app.MapControllers();

// Map health check endpoints
app.UseHealthCheckEndpoints();

app.Run();
