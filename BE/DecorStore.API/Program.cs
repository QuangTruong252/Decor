using System.Text;
using System.Text.Json.Serialization;
using DecorStore.API.Data;
using DecorStore.API.Interfaces;
using DecorStore.API.Repositories;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Configure JSON serialization to handle circular references
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        // Configure retry policy
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);

        // Configure migration history table
        sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory");
    });
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
builder.Services.AddScoped<DecorStore.API.Interfaces.Repositories.IImageRepository, DecorStore.API.Repositories.ImageRepository>();

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
builder.Services.AddScoped<DecorStore.API.Interfaces.Services.IFileManagerService, DecorStore.API.Services.FileManagerService>();

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

// Setup upload directories
var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if(!Directory.Exists(uploadPath)) {
    Directory.CreateDirectory(uploadPath);
}

var thumbnailPath = Path.Combine(uploadPath, ".thumbnails");
if(!Directory.Exists(thumbnailPath)) {
    Directory.CreateDirectory(thumbnailPath);
}

// Configure static files for uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

// Configure static files for thumbnails
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(thumbnailPath),
    RequestPath = "/.thumbnails"
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

app.Run();
