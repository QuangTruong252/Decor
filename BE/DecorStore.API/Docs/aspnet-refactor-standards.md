# ASP.NET Core Refactor & Clean Code Standards

## Hướng dẫn cho AI Agent

**Vai trò:** Bạn là một senior .NET developer với nhiều năm kinh nghiệm refactor và clean code ASP.NET Core.

**Mục tiêu:** Áp dụng các quy chuẩn sau để refactor code ASP.NET Core, cải thiện chất lượng, khả năng bảo trì và hiệu suất.

**Quy trình làm việc:**
1. Phân tích code hiện tại
2. Xác định các vấn đề cần refactor
3. Áp dụng các quy chuẩn theo thứ tự ưu tiên
4. Giải thích thay đổi và lợi ích

---

## 1. KIẾN TRÚC VÀ TỔ CHỨC CODE

### 1.1 Clean Architecture
- **PHẢI LÀM:** Tách biệt rõ ràng các layer: Presentation, Application, Domain, Infrastructure
- **PHẢI LÀM:** Domain layer không được phụ thuộc vào layer nào khác
- **PHẢI LÀM:** Sử dụng Dependency Injection để quản lý dependencies
- **VÍ DỤ REFACTOR:**
```csharp
// ❌ BAD: Controller trực tiếp gọi DbContext
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;
    public ProductController(AppDbContext context) => _context = context;
    
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }
}

// ✅ GOOD: Sử dụng Repository/Service pattern
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductController(IProductService productService) => _productService = productService;
    
    public async Task<IActionResult> GetProducts()
    {
        var result = await _productService.GetAllProductsAsync();
        return Ok(result);
    }
}
```

### 1.2 Folder Structure
- **PHẢI LÀM:** Tổ chức folder theo feature, không theo technical concern
```
📁 Features/
  📁 Products/
    📁 Controllers/
    📁 Services/
    📁 Models/
    📁 Validators/
  📁 Orders/
    📁 Controllers/
    📁 Services/
    📁 Models/
```

---

## 2. CONTROLLERS

### 2.1 Slim Controllers
- **PHẢI LÀM:** Controller chỉ chứa logic routing và response formatting
- **KHÔNG ĐƯỢC:** Business logic trong controller
- **PHẢI LÀM:** Sử dụng ActionResult<T> thay vì IActionResult khi có thể

```csharp
// ❌ BAD: Business logic trong controller
[HttpPost]
public async Task<IActionResult> CreateProduct(CreateProductRequest request)
{
    if (string.IsNullOrEmpty(request.Name) || request.Price <= 0)
        return BadRequest("Invalid product data");
        
    var product = new Product 
    { 
        Name = request.Name, 
        Price = request.Price,
        CreatedAt = DateTime.UtcNow
    };
    
    _context.Products.Add(product);
    await _context.SaveChangesAsync();
    
    return Ok(product);
}

// ✅ GOOD: Delegate to service
[HttpPost]
public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request)
{
    var result = await _productService.CreateProductAsync(request);
    return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
}
```

### 2.2 API Standards
- **PHẢI LÀM:** Sử dụng HTTP status codes chính xác
- **PHẢI LÀM:** Consistent response format
- **PHẢI LÀM:** API versioning khi cần thiết

```csharp
// ✅ GOOD: Consistent response handling
public class BaseController : ControllerBase
{
    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        return result.IsSuccess 
            ? Ok(result.Data)
            : BadRequest(new { Error = result.Error, Timestamp = DateTime.UtcNow });
    }
}
```

---

## 3. SERVICES VÀ BUSINESS LOGIC

### 3.1 Service Design
- **PHẢI LÀM:** Một service chỉ có một trách nhiệm chính
- **PHẢI LÀM:** Sử dụng interfaces cho tất cả services
- **PHẢI LÀM:** Return Result<T> pattern thay vì throw exceptions cho business logic

```csharp
// ✅ GOOD: Service with single responsibility
public interface IProductService
{
    Task<Result<ProductDto>> GetProductByIdAsync(int id);
    Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request);
    Task<Result<IEnumerable<ProductDto>>> GetProductsAsync(ProductFilter filter);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IValidator<CreateProductRequest> _validator;
    
    public async Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Result<ProductDto>.Failure(validationResult.Errors.First().ErrorMessage);
            
        var product = request.ToEntity();
        await _repository.AddAsync(product);
        
        return Result<ProductDto>.Success(product.ToDto());
    }
}
```

### 3.2 Error Handling
- **PHẢI LÀM:** Global exception handling middleware
- **PHẢI LÀM:** Structured logging với Serilog
- **KHÔNG ĐƯỢC:** Catch và ignore exceptions

```csharp
// ✅ GOOD: Global exception middleware
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", 
                context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

---

## 4. DATA ACCESS

### 4.1 Repository Pattern
- **PHẢI LÀM:** Generic repository cho CRUD operations
- **PHẢI LÀM:** Specific repository cho complex queries
- **PHẢI LÀM:** Unit of Work pattern khi cần transaction

```csharp
// ✅ GOOD: Repository implementation
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
    Task<Product?> GetProductWithCategoryAsync(int id);
}
```

### 4.2 Entity Framework Optimization
- **PHẢI LÀM:** Sử dụng AsNoTracking() cho read-only queries
- **PHẢI LÀM:** Include related data explicitly
- **PHẢI LÀM:** Sử dụng pagination cho large datasets

```csharp
// ✅ GOOD: Optimized EF queries
public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilter filter)
{
    var query = _context.Products.AsNoTracking();
    
    if (!string.IsNullOrEmpty(filter.Name))
        query = query.Where(p => p.Name.Contains(filter.Name));
        
    var totalCount = await query.CountAsync();
    
    var products = await query
        .OrderBy(p => p.Name)
        .Skip((filter.Page - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .Select(p => new ProductDto 
        { 
            Id = p.Id, 
            Name = p.Name, 
            Price = p.Price 
        })
        .ToListAsync();
        
    return new PagedResult<ProductDto>(products, totalCount, filter.Page, filter.PageSize);
}
```

---

## 5. VALIDATION VÀ MAPPING

### 5.1 FluentValidation
- **PHẢI LÀM:** Validation rules trong separate classes
- **PHẢI LÀM:** Async validation khi cần thiết
- **PHẢI LÀM:** Custom validation messages

```csharp
// ✅ GOOD: FluentValidation
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    private readonly IProductRepository _repository;
    
    public CreateProductRequestValidator(IProductRepository repository)
    {
        _repository = repository;
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters")
            .MustAsync(BeUniqueName).WithMessage("Product name already exists");
            
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");
    }
    
    private async Task<bool> BeUniqueName(string name, CancellationToken token)
    {
        return !await _repository.ExistsAsync(p => p.Name == name);
    }
}
```

### 5.2 AutoMapper
- **PHẢI LÀM:** Mapping profiles trong separate classes
- **PHẢI LÀM:** Explicit mapping configuration
- **PHẢI LÀM:** Validation mapping configuration

```csharp
// ✅ GOOD: AutoMapper profile
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, 
                opt => opt.MapFrom(src => src.Category.Name));
                
        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
```

---

## 6. CONFIGURATION VÀ DEPENDENCY INJECTION

### 6.1 Service Registration
- **PHẢI LÀM:** Extension methods cho service registration
- **PHẢI LÀM:** Appropriate service lifetimes
- **PHẢI LÀM:** Configuration validation

```csharp
// ✅ GOOD: Service registration extensions
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
        
        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        
        // Validators
        services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
        
        // AutoMapper
        services.AddAutoMapper(typeof(ProductMappingProfile));
        
        return services;
    }
}
```

### 6.2 Configuration Pattern
- **PHẢI LÀM:** Strongly-typed configuration
- **PHẢI LÀM:** Options validation
- **PHẢI LÀM:** Environment-specific settings

```csharp
// ✅ GOOD: Strongly-typed configuration
public class DatabaseSettings
{
    public const string SectionName = "Database";
    
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
    
    [Range(1, 300)]
    public int CommandTimeout { get; set; } = 30;
    
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

// Registration with validation
services.AddOptions<DatabaseSettings>()
    .Bind(configuration.GetSection(DatabaseSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

---

## 7. TESTING

### 7.1 Unit Testing Standards
- **PHẢI LÀM:** Test naming convention: MethodName_Scenario_ExpectedResult
- **PHẢI LÀM:** Arrange-Act-Assert pattern
- **PHẢI LÀM:** Mock external dependencies

```csharp
// ✅ GOOD: Unit test example
[Test]
public async Task CreateProductAsync_WithValidData_ReturnsSuccessResult()
{
    // Arrange
    var request = new CreateProductRequest { Name = "Test Product", Price = 10.99m };
    var mockRepository = new Mock<IProductRepository>();
    var mockValidator = new Mock<IValidator<CreateProductRequest>>();
    
    mockValidator.Setup(v => v.ValidateAsync(request, default))
        .ReturnsAsync(new ValidationResult());
    
    var service = new ProductService(mockRepository.Object, mockValidator.Object);
    
    // Act
    var result = await service.CreateProductAsync(request);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Data.Name, Is.EqualTo(request.Name));
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
}
```

---

## 8. PERFORMANCE VÀ SECURITY

### 8.1 Performance Optimization
- **PHẢI LÀM:** Async/await cho I/O operations
- **PHẢI LÀM:** Caching cho frequently accessed data
- **PHẢI LÀM:** Response compression

```csharp
// ✅ GOOD: Caching implementation
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> GetProduct(int id)
{
    var cacheKey = $"product:{id}";
    var cachedProduct = await _cache.GetStringAsync(cacheKey);
    
    if (cachedProduct != null)
    {
        var product = JsonSerializer.Deserialize<ProductDto>(cachedProduct);
        return Ok(product);
    }
    
    var result = await _productService.GetProductByIdAsync(id);
    if (result.IsSuccess)
    {
        var serializedProduct = JsonSerializer.Serialize(result.Data);
        await _cache.SetStringAsync(cacheKey, serializedProduct, TimeSpan.FromMinutes(15));
    }
    
    return HandleResult(result);
}
```

### 8.2 Security Best Practices
- **PHẢI LÀM:** Input validation và sanitization
- **PHẢI LÀM:** Authentication và Authorization
- **PHẢI LÀM:** Rate limiting

```csharp
// ✅ GOOD: Authorization implementation
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteProduct(int id)
{
    var result = await _productService.DeleteProductAsync(id);
    return HandleResult(result);
}
```

---

## 9. LOGGING VÀ MONITORING

### 9.1 Structured Logging
- **PHẢI LÀM:** Sử dụng structured logging với Serilog
- **PHẢI LÀM:** Log levels appropriate
- **PHẢI LÀM:** Include correlation IDs

```csharp
// ✅ GOOD: Structured logging
public async Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request)
{
    using var activity = _logger.BeginScope(new Dictionary<string, object>
    {
        ["ProductName"] = request.Name,
        ["CorrelationId"] = Activity.Current?.Id ?? Guid.NewGuid().ToString()
    });
    
    _logger.LogInformation("Creating product with name {ProductName}", request.Name);
    
    try
    {
        var result = await _repository.AddAsync(product);
        _logger.LogInformation("Successfully created product with ID {ProductId}", result.Id);
        return Result<ProductDto>.Success(result.ToDto());
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create product with name {ProductName}", request.Name);
        throw;
    }
}
```

---

## 10. QUY TRÌNH REFACTOR

### Bước 1: Phân tích Code
- Xác định code smells
- Đánh giá complexity
- Tìm duplicate code
- Kiểm tra test coverage

### Bước 2: Lập kế hoạch
- Ưu tiên theo impact và effort
- Tạo branch cho refactor
- Backup code hiện tại

### Bước 3: Thực hiện Refactor
- Refactor từng small chunks
- Chạy tests sau mỗi thay đổi
- Commit frequently với meaningful messages

### Bước 4: Review và Validate
- Code review với team
- Performance testing
- Integration testing

---

## CHECKLIST CHO AI AGENT

Khi refactor code ASP.NET Core, hãy check các điểm sau:

### ✅ Architecture
- [ ] Separation of concerns được đảm bảo
- [ ] Dependencies được inject properly
- [ ] Layer dependencies đúng hướng

### ✅ Code Quality
- [ ] Methods và classes có single responsibility
- [ ] Naming conventions consistent
- [ ] Code duplications được eliminate

### ✅ Performance
- [ ] Async/await được sử dụng đúng cách
- [ ] Database queries được optimize
- [ ] Caching được implement hợp lý

### ✅ Error Handling
- [ ] Global exception handling
- [ ] Proper logging
- [ ] Meaningful error messages

### ✅ Testing
- [ ] Unit tests coverage > 80%
- [ ] Integration tests cho critical paths
- [ ] Test cases cover edge cases

---

## KẾT LUẬN

Khi refactor, hãy luôn nhớ:
1. **Safety first**: Đảm bảo functionality không bị break
2. **Incremental changes**: Refactor từng bước nhỏ
3. **Test coverage**: Maintain hoặc improve test coverage
4. **Documentation**: Update documentation sau khi refactor
5. **Team communication**: Inform team về những thay đổi lớn

**Mục tiêu cuối cùng**: Code dễ đọc, dễ maintain, scalable và performant.