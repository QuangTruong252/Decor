using System.Text;
using System.Text.Json.Serialization;
using DecorStore.API.Data;
using DecorStore.API.Extensions;
using DecorStore.API.Interfaces;
using DecorStore.API.Repositories;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Read environment variable DATABASE_URL (if any) and prioritize using it
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

// If DATABASE_URL is not set and running in Development environment, use hardcoded Railway connection string
if (string.IsNullOrEmpty(databaseUrl) && builder.Environment.IsDevelopment())
{
    // Use Railway connection string for Development environment
    databaseUrl = "postgresql://postgres:aDXFZErwvtuSNUMPQNgIFRqhbPkhCrKb@crossover.proxy.rlwy.net:18693/railway";
    Console.WriteLine("Using hardcoded Railway connection string for Development environment");
}

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        // Convert Railway PostgreSQL URL to Npgsql connection string
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        var npgsqlBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = userInfo[0],
            Password = userInfo[1],
            SslMode = SslMode.Require,
            TrustServerCertificate = true,
            Pooling = true,
            MinPoolSize = 0,
            MaxPoolSize = 100,
            ConnectionIdleLifetime = 300,
            Timeout = 60, // Increase timeout to 60 seconds
            CommandTimeout = 60 // Increase command timeout to 60 seconds
        };

        connectionString = npgsqlBuilder.ToString();
        Console.WriteLine($"Successfully parsed DATABASE_URL to connection string");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        // Keep connection string from appsettings.json if there's an error
    }
}

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Configure JSON serialization to handle circular references
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Configure DbContext with PostgreSQL with resilience
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Configure retry policy with more retries and longer delays
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(60),
            errorCodesToAdd: null);

        // Increase command timeout to 120 seconds
        npgsqlOptions.CommandTimeout(120);

        // Configure migration history table
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
    });

    // Ignore PendingModelChangesWarning to avoid migration issues
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IBannerRepository, BannerRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Add Excel services
builder.Services.AddScoped<DecorStore.API.Services.Excel.IExcelService, DecorStore.API.Services.Excel.ExcelService>();
builder.Services.AddScoped<DecorStore.API.Services.Excel.IProductExcelService, DecorStore.API.Services.Excel.ProductExcelService>();
builder.Services.AddScoped<DecorStore.API.Services.Excel.ICategoryExcelService, DecorStore.API.Services.Excel.CategoryExcelService>();
builder.Services.AddScoped<DecorStore.API.Services.Excel.ICustomerExcelService, DecorStore.API.Services.Excel.CustomerExcelService>();
builder.Services.AddScoped<DecorStore.API.Services.Excel.IOrderExcelService, DecorStore.API.Services.Excel.OrderExcelService>();

// Add FluentValidation for Excel DTOs
builder.Services.AddScoped<FluentValidation.IValidator<DecorStore.API.DTOs.Excel.ProductExcelDTO>, DecorStore.API.Validators.Excel.ProductExcelValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<DecorStore.API.DTOs.Excel.CategoryExcelDTO>, DecorStore.API.Validators.Excel.CategoryExcelValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<DecorStore.API.DTOs.Excel.CustomerExcelDTO>, DecorStore.API.Validators.Excel.CustomerExcelValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<DecorStore.API.DTOs.Excel.OrderExcelDTO>, DecorStore.API.Validators.Excel.OrderExcelValidator>();

// Add memory cache for dashboard data
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();
// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    // Add event handlers for debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            var claims = context.Principal.Claims.Select(c => new { c.Type, c.Value });
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DecorStore API", Version = "v1" });

    // Configure Swagger to use JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
var uploadPath = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImageSettings:BasePath"] ?? "Uploads");
if(!Directory.Exists(uploadPath)) {
    Directory.CreateDirectory(uploadPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/Resources"
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("AllowAll");

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed furniture data if needed
// if (app.Environment.IsDevelopment())
// {
//     // Check if we should skip database seeding
//     var skipSeeding = Environment.GetEnvironmentVariable("SKIP_DB_SEEDING");
//     if (string.Equals(skipSeeding, "true", StringComparison.OrdinalIgnoreCase))
//     {
//         Console.WriteLine("Skipping database seeding as per SKIP_DB_SEEDING environment variable");
//     }
//     else
//     {
//         // Only seed data in development environment
//         Console.WriteLine("Starting database seeding process...");
//         try
//         {
//             await app.SeedFurnitureDataAsync();
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error during database seeding: {ex.Message}");
//             // Continue running the application even if seeding fails
//         }
//     }
// }

app.Run();
