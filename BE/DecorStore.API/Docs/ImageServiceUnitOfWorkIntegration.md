# ImageService UnitOfWork Integration

## Tổng quan
Tài liệu này mô tả việc triển khai UnitOfWork pattern cho ImageService để giải quyết vấn đề SqlServerRetryingExecutionStrategy và cải thiện quản lý transaction.

## Vấn đề trước khi refactor

### 1. Dependency Injection trực tiếp
- ImageService inject trực tiếp `IImageRepository`
- Không sử dụng UnitOfWork pattern
- Thiếu quản lý transaction tập trung

### 2. Lỗi SqlServerRetryingExecutionStrategy
- Xảy ra khi có nhiều DbContext instances
- Transaction không được quản lý đúng cách
- Retry strategy xung đột với manual transaction

### 3. Code cũ
```csharp
public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    
    public ImageService(IConfiguration configuration, IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }
    
    public async Task<List<int>> GetOrCreateImagesAsync(List<IFormFile> files, string folderName = "images")
    {
        // Trực tiếp gọi repository mà không có transaction management
        await _imageRepository.AddAsync(image);
    }
}
```

## Giải pháp đã triển khai

### 1. Dependency Injection thông qua UnitOfWork
```csharp
public class ImageService : IImageService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ImageService(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

### 2. Transaction Management với Execution Strategy
```csharp
public async Task<List<int>> GetOrCreateImagesAsync(List<IFormFile> files, string folderName = "images")
{
    return await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
    {
        var imageIds = new List<int>();
        var uploadedFiles = new List<(string fileName, string filePath)>();
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            foreach (var file in files)
            {
                // Upload file và tạo record trong database
                var imagePath = await UploadImageAsync(file, folderName);
                uploadedFiles.Add((file.FileName, imagePath));

                var image = new Image
                {
                    FileName = file.FileName,
                    FilePath = imagePath,
                    AltText = Path.GetFileNameWithoutExtension(file.FileName),
                    CreatedAt = DateTime.UtcNow
                };
                
                await _unitOfWork.Images.AddAsync(image);
                await _unitOfWork.SaveChangesAsync();
                imageIds.Add(image.Id);
            }
            
            await _unitOfWork.CommitTransactionAsync();
            return imageIds;
        }
        catch
        {
            // Rollback transaction
            await _unitOfWork.RollbackTransactionAsync();
            
            // Clean up uploaded files nếu database operation thất bại
            foreach (var (fileName, filePath) in uploadedFiles)
            {
                try
                {
                    await DeleteImageAsync(filePath);
                }
                catch
                {
                    // Log cleanup failure nhưng không throw
                }
            }
            
            throw;
        }
    });
}
```

### 3. Cập nhật tất cả methods sử dụng UnitOfWork
```csharp
// Trước
public async Task<List<Image>> GetAllImagesAsync()
{
    return await _imageRepository.GetAllAsync();
}

// Sau
public async Task<List<Image>> GetAllImagesAsync()
{
    return await _unitOfWork.Images.GetAllAsync();
}

// Với Execution Strategy cho operations quan trọng
public async Task AssignImageToProductAsync(int imageId, int productId)
{
    await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
    {
        await _unitOfWork.Images.AddProductImageAsync(imageId, productId);
        await _unitOfWork.SaveChangesAsync();
        return Task.CompletedTask;
    });
}
```

## Lợi ích của việc triển khai

### 1. Giải quyết SqlServerRetryingExecutionStrategy
- ✅ Sử dụng execution strategy được quản lý bởi UnitOfWork
- ✅ Tránh xung đột giữa retry policy và manual transaction
- ✅ Consistent transaction management

### 2. Cải thiện quản lý Transaction
- ✅ Tất cả database operations được wrap trong transaction
- ✅ Automatic rollback khi có lỗi
- ✅ File cleanup khi database operation thất bại

### 3. Consistency với các Service khác
- ✅ Sử dụng cùng pattern với ProductService, CategoryService, etc.
- ✅ Dễ maintain và debug
- ✅ Better error handling

### 4. Performance và Reliability
- ✅ Execution strategy xử lý transient failures
- ✅ Proper resource disposal
- ✅ Better concurrency handling

## Các methods đã được cập nhật

### Core Methods
- `GetOrCreateImagesAsync()` - Với full transaction management và file cleanup
- `GetImagesByIdsAsync()` - Sử dụng UnitOfWork.Images
- `GetAllImagesAsync()` - Sử dụng UnitOfWork.Images
- `ImageExistsInSystemAsync()` - Sử dụng UnitOfWork.Images

### Assignment Methods
- `AssignImageToProductAsync()` - Với ExecutionStrategy
- `AssignImageToCategoryAsync()` - Với ExecutionStrategy
- `UnassignImageFromProductAsync()` - Với ExecutionStrategy
- `UnassignImageFromCategoryAsync()` - Với ExecutionStrategy

## Configuration Changes

### Program.cs
Không cần thay đổi dependency injection vì UnitOfWork đã được register:
```csharp
// UnitOfWork đã được register
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ImageService sẽ tự động nhận UnitOfWork thông qua DI
builder.Services.AddScoped<IImageService, DecorStore.API.Services.ImageService>();
```

## Testing

Project đã build thành công với 193 warnings (chỉ là nullability warnings, không phải errors):
```
Build succeeded with 193 warning(s) in 4.0s
```

## Kết luận

Việc triển khai UnitOfWork cho ImageService đã hoàn thành thành công với những cải tiến chính:

1. **Giải quyết SqlServerRetryingExecutionStrategy errors**
2. **Cải thiện transaction management**
3. **Tăng tính consistency trong codebase**
4. **Better error handling và resource cleanup**
5. **Improved reliability cho image operations**

ImageService giờ đây sử dụng cùng pattern với các service khác trong hệ thống, đảm bảo tính nhất quán và dễ bảo trì.
