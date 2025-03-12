## **1. Tổng quan dự án (Project Overview)**

Decor Store API là một backend API hiện đại được xây dựng trên nền tảng ASP.NET Core 8, phục vụ cho ứng dụng web bán đồ trang trí nội thất. API này cung cấp đầy đủ các chức năng thiết yếu như quản lý sản phẩm, danh mục, đơn hàng, đánh giá, banner quảng cáo, xác thực người dùng, và xử lý đơn hàng.

**Mục tiêu dự án:**
- Xây dựng API RESTful chuẩn, dễ mở rộng và bảo trì
- Cung cấp backend mạnh mẽ cho ứng dụng web bán đồ trang trí
- Triển khai dễ dàng trên nền tảng đám mây (Railway)
- Đảm bảo bảo mật với JWT Authentication
- Sử dụng PostgreSQL làm cơ sở dữ liệu chính

## **2. Tech Stack (Công nghệ sử dụng)**

### Backend:
- **ASP.NET Core 8** - Framework mới nhất của Microsoft
- **Entity Framework Core** - ORM để làm việc với cơ sở dữ liệu
- **PostgreSQL** - Hệ quản trị cơ sở dữ liệu
- **JWT Authentication** - Xác thực người dùng
- **Swagger/OpenAPI** - Tài liệu API tự động
- **BCrypt.Net** - Mã hóa mật khẩu
- **Railway** - Nền tảng triển khai

## **3. Cấu trúc dự án (Project Structure)**

```
DecorStore.API/
├── Controllers/                  # API controllers xử lý các requests
│   ├── AuthController.cs         # Xử lý đăng nhập, đăng ký
│   ├── ProductsController.cs     # Quản lý sản phẩm
│   ├── CategoryController.cs     # Quản lý danh mục sản phẩm
│   ├── OrderController.cs        # Quản lý đơn hàng
│   ├── ReviewController.cs       # Quản lý đánh giá sản phẩm
│   ├── BannerController.cs       # Quản lý banner quảng cáo
│   └── HealthCheckController.cs  # Kiểm tra trạng thái API
├── Data/
│   └── ApplicationDbContext.cs   # DbContext kết nối với database
├── DTOs/                         # Data Transfer Objects
│   ├── AuthResponseDTO.cs        # Phản hồi xác thực
│   ├── LoginDTO.cs               # Dữ liệu đăng nhập
│   ├── RegisterDTO.cs            # Dữ liệu đăng ký
│   ├── UserDTO.cs                # Dữ liệu người dùng
│   ├── ProductDTO.cs             # Dữ liệu sản phẩm
│   ├── CategoryDTO.cs            # Dữ liệu danh mục
│   ├── OrderDTO.cs               # Dữ liệu đơn hàng
│   ├── ReviewDTO.cs              # Dữ liệu đánh giá
│   └── BannerDTO.cs              # Dữ liệu banner
├── Migrations/                   # EF Core migrations
├── Models/                       # Entity models
│   ├── Product.cs                # Model sản phẩm
│   ├── User.cs                   # Model người dùng
│   ├── Category.cs               # Model danh mục
│   ├── Order.cs                  # Model đơn hàng
│   ├── OrderItem.cs              # Model mặt hàng trong đơn hàng
│   ├── Review.cs                 # Model đánh giá
│   ├── Banner.cs                 # Model banner
│   └── ProductImage.cs           # Model hình ảnh sản phẩm
├── Repositories/                 # Repository pattern
│   ├── IProductRepository.cs     # Interface cho repository sản phẩm
│   ├── ProductRepository.cs      # Repository sản phẩm
│   ├── ICategoryRepository.cs    # Interface cho repository danh mục
│   ├── CategoryRepository.cs     # Repository danh mục
│   ├── IOrderRepository.cs       # Interface cho repository đơn hàng
│   ├── OrderRepository.cs        # Repository đơn hàng
│   ├── IReviewRepository.cs      # Interface cho repository đánh giá
│   ├── ReviewRepository.cs       # Repository đánh giá
│   ├── IBannerRepository.cs      # Interface cho repository banner
│   └── BannerRepository.cs       # Repository banner
├── Services/                     # Business logic
│   ├── IAuthService.cs           # Interface service xác thực
│   ├── AuthService.cs            # Service xác thực
│   ├── IProductService.cs        # Interface service sản phẩm
│   ├── ProductService.cs         # Service sản phẩm
│   ├── ICategoryService.cs       # Interface service danh mục
│   ├── CategoryService.cs        # Service danh mục
│   ├── IOrderService.cs          # Interface service đơn hàng
│   ├── OrderService.cs           # Service đơn hàng
│   ├── IReviewService.cs         # Interface service đánh giá
│   ├── ReviewService.cs          # Service đánh giá
│   ├── IBannerService.cs         # Interface service banner
│   └── BannerService.cs          # Service banner
├── Program.cs                    # Điểm khởi đầu ứng dụng
├── appsettings.json              # Cấu hình ứng dụng
└── railway.toml                  # Cấu hình triển khai Railway
```

## **4. Hướng dẫn cài đặt (Setup Guide)**

### Yêu cầu hệ thống:
- .NET 8 SDK
- PostgreSQL
- Git

### Các bước cài đặt:

1. **Clone repository:**
   ```bash
   git clone <repository-url>
   cd DecorStore.API
   ```

2. **Khôi phục các packages:**
   ```bash
   dotnet restore
   ```

3. **Cấu hình kết nối cơ sở dữ liệu:**
   - Mở file `appsettings.json`
   - Thay đổi chuỗi kết nối trong `"DefaultConnection"` phù hợp với PostgreSQL của bạn

4. **Chạy migrations để tạo cơ sở dữ liệu:**
   ```bash
   dotnet ef database update
   ```

5. **Khởi chạy ứng dụng:**
   ```bash
   dotnet run
   ```

6. **Truy cập API:**
   - Swagger UI: `https://localhost:5001/swagger`
   - API Endpoint: `https://localhost:5001/api`

## **5. Cách hoạt động (How It Works)**

### Luồng xử lý request:

```
┌─────────┐      ┌─────────────┐      ┌────────────┐      ┌──────────────┐      ┌────────────┐
│ Client  │───►  │ Controllers │───►  │ Services   │───►  │ Repositories │───►  │ Database   │
└─────────┘      └─────────────┘      └────────────┘      └──────────────┘      └────────────┘
                        │                   │                    │
                        │                   │                    │
                        ▼                   ▼                    ▼
                 ┌─────────────┐    ┌──────────────┐    ┌────────────────┐
                 │ DTOs        │    │ Models       │    │ DbContext      │
                 └─────────────┘    └──────────────┘    └────────────────┘
```

### Các khái niệm cốt lõi:

#### **Dependency Injection:**
Trong ASP.NET Core, Dependency Injection (DI) được tích hợp sẵn và được sử dụng để quản lý các dependency giữa các components. Ví dụ:

```csharp
// Đăng ký service trong Program.cs
builder.Services.AddScoped<IAuthService, AuthService>();

// Sử dụng service trong controller
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
}
```

#### **Middleware:**
Middleware là các thành phần xử lý request/response trong pipeline của ASP.NET Core. Middleware được cấu hình trong `Program.cs`:

```csharp
// Cấu hình middleware
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

#### **JWT Authentication:**
Dự án sử dụng JWT (JSON Web Tokens) để xác thực người dùng:

```csharp
// Cấu hình JWT trong Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});
```

## **6. Khái niệm cốt lõi (Core Concepts)**

### **ASP.NET Core 8 hoạt động ra sao?**

ASP.NET Core 8 là một framework web cross-platform, được thiết kế để xây dựng các ứng dụng web hiện đại. Nó sử dụng mô hình Middleware để xử lý HTTP request/response và MVC pattern để tổ chức code.

#### **Vai trò của Program.cs:**
Trong ASP.NET Core 8, `Program.cs` là điểm khởi đầu của ứng dụng, nơi cấu hình tất cả các dịch vụ và middleware:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Đăng ký services
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Cấu hình middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### **Vai trò của DbContext:**
`ApplicationDbContext` là bridge giữa model và database:

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    
    // Cấu hình model và relationship
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ...
    }
}
```

### **Kiến trúc API của DecorStore**

Dự án DecorStore API được xây dựng trên kiến trúc nhiều tầng, với các controllers, services, và repositories thực thi các chức năng cụ thể:

#### **Controllers:**

1. **AuthController**: 
   - Xử lý xác thực người dùng: đăng ký, đăng nhập, lấy thông tin user

2. **ProductsController**: 
   - Quản lý CRUD cho sản phẩm
   - Hỗ trợ tìm kiếm, lọc và phân trang

3. **CategoryController**:
   - Quản lý danh mục sản phẩm
   - Hỗ trợ hiển thị danh mục dạng phân cấp cha-con
   - Cho phép tạo, cập nhật, xóa danh mục (chỉ Admin)

4. **OrderController**:
   - Quản lý đơn hàng người dùng
   - Hỗ trợ tạo đơn hàng mới, cập nhật trạng thái
   - Kiểm tra quyền truy cập: người dùng chỉ xem được đơn hàng của mình, Admin xem được tất cả

5. **ReviewController**:
   - Quản lý đánh giá sản phẩm
   - Cho phép người dùng thêm, sửa, xóa đánh giá của mình
   - Tính toán điểm đánh giá trung bình cho sản phẩm

6. **BannerController**:
   - Quản lý banner quảng cáo
   - Lấy các banner đang hoạt động để hiển thị
   - Thêm, sửa, xóa banner (chỉ Admin)

7. **HealthCheckController**:
   - Kiểm tra trạng thái hoạt động của API
   - Hữu ích cho monitoring và kiểm tra kết nối database

#### **Repository Pattern:**

Mỗi entity đều có một repository riêng, tuân theo mẫu thiết kế Repository Pattern:

```csharp
// Interface định nghĩa các phương thức
public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> GetByIdAsync(int id);
    // ...các phương thức khác
}

// Lớp cài đặt cụ thể
public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    
    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }
    
    // ...các phương thức khác
}
```

#### **Service Layer:**

Các services chứa business logic, kết nối giữa controllers và repositories:

```csharp
// Interface định nghĩa các phương thức
public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
    Task<CategoryDTO> GetCategoryByIdAsync(int id);
    // ...các phương thức khác
}

// Lớp cài đặt cụ thể
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    
    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    
    public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => MapCategoryToDto(c));
    }
    
    // ...các phương thức khác
}
```

Mô hình này đảm bảo:
- **Separation of Concerns**: Mỗi lớp có trách nhiệm riêng biệt
- **Dependency Injection**: Các dependencies được inject vào constructor
- **Testability**: Dễ dàng viết unit tests bằng cách mock các dependencies
- **Maintainability**: Dễ dàng bảo trì và mở rộng code

## **7. Tài liệu API (API Documentation)**

### **Swagger UI:**
- **Local**: https://localhost:5001/swagger
- **Deployed**: https://decorstore-api.up.railway.app/swagger

Swagger UI cung cấp tài liệu tương tác cho API, cho phép bạn:
- Xem tất cả endpoints
- Thử nghiệm các API calls
- Xem các schemas và responses

### **API Endpoints chính:**

#### Authentication:
- `POST /api/auth/register` - Đăng ký người dùng mới
- `POST /api/auth/login` - Đăng nhập
- `GET /api/auth/user/{id}` - Lấy thông tin người dùng

#### Products:
- `GET /api/products` - Lấy tất cả sản phẩm
- `GET /api/products/{id}` - Lấy sản phẩm theo ID
- `POST /api/products` - Thêm sản phẩm mới (Admin)
- `PUT /api/products/{id}` - Cập nhật sản phẩm (Admin)
- `DELETE /api/products/{id}` - Xóa sản phẩm (Admin)

#### Categories:
- `GET /api/category` - Lấy tất cả danh mục
- `GET /api/category/hierarchical` - Lấy danh mục dạng phân cấp
- `GET /api/category/{id}` - Lấy danh mục theo ID
- `GET /api/category/slug/{slug}` - Lấy danh mục theo slug
- `POST /api/category` - Tạo danh mục mới (Admin)
- `PUT /api/category/{id}` - Cập nhật danh mục (Admin)
- `DELETE /api/category/{id}` - Xóa danh mục (Admin)

#### Orders:
- `GET /api/order` - Lấy tất cả đơn hàng (Admin)
- `GET /api/order/user/{userId}` - Lấy đơn hàng theo user
- `GET /api/order/{id}` - Lấy chi tiết đơn hàng
- `POST /api/order` - Tạo đơn hàng mới
- `PUT /api/order/{id}/status` - Cập nhật trạng thái đơn hàng (Admin)
- `DELETE /api/order/{id}` - Xóa đơn hàng

#### Reviews:
- `GET /api/review/product/{productId}` - Lấy đánh giá theo sản phẩm
- `GET /api/review/{id}` - Lấy chi tiết đánh giá
- `GET /api/review/product/{productId}/rating` - Lấy điểm đánh giá trung bình của sản phẩm
- `POST /api/review` - Tạo đánh giá mới
- `PUT /api/review/{id}` - Cập nhật đánh giá
- `DELETE /api/review/{id}` - Xóa đánh giá

#### Banners:
- `GET /api/banner` - Lấy tất cả banner
- `GET /api/banner/active` - Lấy các banner đang hoạt động
- `GET /api/banner/{id}` - Lấy chi tiết banner
- `POST /api/banner` - Tạo banner mới (Admin)
- `PUT /api/banner/{id}` - Cập nhật banner (Admin)
- `DELETE /api/banner/{id}` - Xóa banner (Admin)

## **8. Triển khai (Deployment)**

### **Triển khai lên Railway:**

1. **Tạo tài khoản Railway:**
   - Đăng ký tại [railway.app](https://railway.app)

2. **Cài đặt Railway CLI:**
   ```bash
   npm install -g @railway/cli
   railway login
   ```

3. **Khởi tạo project Railway:**
   ```bash
   railway init
   ```

4. **Cấu hình Railway:**
   - Dự án đã có sẵn file `railway.toml` với cấu hình:
   ```toml
   [build]
   builder = "nixpacks"
   buildCommand = "dotnet restore && dotnet publish -c Release -o out"

   [deploy]
   startCommand = "cd out && dotnet DecorStore.API.dll"
   healthcheckPath = "/api/products"
   healthcheckTimeout = 300
   restartPolicyType = "on-failure"
   restartPolicyMaxRetries = 5

   [env]
   ASPNETCORE_ENVIRONMENT = "Production"
   DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "true"
   ASPNETCORE_URLS = "http://0.0.0.0:${PORT:-8080}"
   ```

5. **Tạo PostgreSQL database trên Railway:**
   - Vào Dashboard Railway → New → Database → PostgreSQL
   - Kết nối database với project

6. **Deploy ứng dụng:**
   ```bash
   railway up
   ```

7. **Biến môi trường:**
   - `DATABASE_URL` - Tự động được cung cấp bởi Railway khi liên kết với PostgreSQL
   - `JWT:SecretKey` - Key bí mật cho JWT
   - `JWT:ExpiryInMinutes` - Thời gian hết hạn JWT

## **9. Tài nguyên học tập (Learning Resources)**

### **ASP.NET Core:**
- [Tài liệu chính thức ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication trong ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication)

### **Video Tutorials:**
- [ASP.NET Core Tutorial cho người mới - Microsoft Learn](https://learn.microsoft.com/en-us/training/modules/build-web-api-aspnet-core/)
- [Entity Framework Core Tutorial - Programming with Mosh](https://www.youtube.com/watch?v=C5cnZ-gZy2I)
- [REST API với ASP.NET Core - Les Jackson](https://www.youtube.com/watch?v=fmvcAzHpsk8)

### **Blogs và Cộng đồng:**
- [.NET Blog](https://devblogs.microsoft.com/dotnet/)
- [Stack Overflow - ASP.NET Core](https://stackoverflow.com/questions/tagged/asp.net-core)

## **10. Tips và Troubleshooting**

### **Các lỗi phổ biến và cách sửa:**

#### **Lỗi Migration:**
```
Unable to create an object of type 'ApplicationDbContext'...
```

**Cách sửa:**
- Kiểm tra chuỗi kết nối trong `appsettings.json`
- Đảm bảo PostgreSQL đang chạy
- Thử chạy lệnh với connection string cụ thể:
  ```bash
  dotnet ef database update --connection "Your connection string"
  ```

#### **Lỗi CORS:**
```
Access to fetch at 'https://api.example.com' from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Cách sửa:**
- Đảm bảo CORS được cấu hình đúng trong Program.cs:
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowAll", builder =>
      {
          builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
      });
  });
  
  // Và không quên sử dụng middleware:
  app.UseCors("AllowAll");
  ```

#### **Lỗi JWT Authentication:**
```
401 Unauthorized
```

**Cách sửa:**
- Kiểm tra token đã được gửi đúng format trong header: `Authorization: Bearer <token>`
- Đảm bảo SecretKey giống nhau ở cả phía tạo token và xác thực
- Kiểm tra thời gian hết hạn của token

### **Cách EF Core mapping model vào database:**

Entity Framework Core sử dụng Code-First approach để mapping:

1. **Định nghĩa model:**
   ```csharp
   public class Product
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public decimal Price { get; set; }
       public string Category { get; set; }
   }
   ```

2. **Cấu hình trong DbContext:**
   ```csharp
   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       // Cấu hình decimal precision
       modelBuilder.Entity<Product>()
           .Property(p => p.Price)
           .HasColumnType("decimal(18,2)");
           
       // Cấu hình index
       modelBuilder.Entity<User>()
           .HasIndex(u => u.Email)
           .IsUnique();
   }
   ```

3. **Tạo migration:**
   ```bash
   dotnet ef migrations add InitialCreate
   ```

4. **Cập nhật database:**
   ```bash
   dotnet ef database update
   ```

## **11. Bảng lệnh CLI cheatsheet**

### **Dotnet CLI:**
| Lệnh | Mô tả |
|------|-------|
| `dotnet new webapi -n ProjectName` | Tạo project API mới |
| `dotnet restore` | Khôi phục packages |
| `dotnet build` | Build project |
| `dotnet run` | Chạy ứng dụng |
| `dotnet watch run` | Chạy với hot reload |
| `dotnet publish -c Release` | Publish ứng dụng |
| `dotnet ef migrations add <Name>` | Tạo migration mới |
| `dotnet ef database update` | Cập nhật database |
| `dotnet ef migrations script` | Tạo script SQL |

### **Railway CLI:**
| Lệnh | Mô tả |
|------|-------|
| `railway login` | Đăng nhập |
| `railway init` | Khởi tạo project |
| `railway link` | Liên kết với project |
| `railway up` | Deploy ứng dụng |
| `railway logs` | Xem logs |
| `railway variables` | Xem biến môi trường |

## **12. Roadmap (Lộ trình phát triển)**

### **Tính năng sắp tới:**
- [ ] Tích hợp thanh toán với Stripe/Paypal
- [ ] Quản lý đơn hàng và giỏ hàng
- [ ] Hệ thống đánh giá sản phẩm
- [ ] Tìm kiếm và lọc sản phẩm nâng cao
- [ ] Phân quyền chi tiết (RBAC)
- [ ] Đa ngôn ngữ (i18n)
- [ ] Tích hợp email notification
- [ ] Dashboard quản lý cho admin
- [ ] API rate limiting và caching
- [ ] Theo dõi hiệu suất với Application Insights

---

*Dự án được phát triển bởi Truong.Tran © 2025*
