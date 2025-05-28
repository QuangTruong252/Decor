# File Manager API Documentation

## Overview

File Manager API cung cấp đầy đủ tính năng quản lý files và images cho Admin site của hệ thống E-commerce. API được thiết kế để hỗ trợ UI/UX giống như file manager hiện đại với các tính năng browse, upload, search, filter, và các thao tác file cơ bản.

## Features

### ✅ Core Features
- **Browse Files & Folders**: Xem danh sách files/folders với pagination
- **Upload Multiple Files**: Upload nhiều files cùng lúc với validation
- **Create Folders**: Tạo folders mới
- **Delete Files/Folders**: Xóa files hoặc folders (có thể bulk delete)
- **Move & Copy**: Di chuyển và copy files/folders
- **Search & Filter**: Tìm kiếm và filter theo nhiều tiêu chí

### ✅ Advanced Features
- **Thumbnail Generation**: Tự động tạo thumbnails cho images
- **Image Metadata**: Trích xuất metadata từ images (kích thước, format, etc.)
- **Path Security**: Bảo vệ chống path traversal attacks
- **Database Sync**: Đồng bộ giữa file system và database
- **Orphaned Files Cleanup**: Dọn dẹp files không còn sử dụng

### ✅ Performance & Security
- **Pagination**: Hỗ trợ pagination cho large directories
- **JWT Authentication**: Bảo mật với JWT tokens
- **Admin Only Access**: Chỉ admin mới có quyền truy cập
- **File Validation**: Validate file types và sizes
- **Error Handling**: Xử lý lỗi chi tiết và logging

## API Endpoints

### Authentication
Tất cả endpoints đều yêu cầu JWT authentication với role Admin.

```
Authorization: Bearer <jwt_token>
```

### Base URL
```
/api/filemanager
```

## Endpoints Detail

### 1. Health Check
```http
GET /api/filemanager/health
```

**Response:**
```json
{
  "status": "healthy",
  "uploadsPath": "/path/to/uploads"
}
```

### 2. Browse Files & Folders
```http
GET /api/filemanager/browse
```

**Query Parameters:**
- `path` (string): Đường dẫn folder (mặc định: root)
- `page` (int): Trang hiện tại (mặc định: 1)
- `pageSize` (int): Số items per page (mặc định: 20)
- `search` (string): Từ khóa tìm kiếm
- `fileType` (string): Loại file ("all", "image", "folder")
- `extension` (string): Extension cụ thể (".jpg", ".png", etc.)
- `sortBy` (string): Sắp xếp theo ("name", "size", "date", "type")
- `sortOrder` (string): Thứ tự ("asc", "desc")
- `minSize` (long): Kích thước tối thiểu (bytes)
- `maxSize` (long): Kích thước tối đa (bytes)
- `fromDate` (datetime): Từ ngày
- `toDate` (datetime): Đến ngày

**Response:**
```json
{
  "currentPath": "categories",
  "parentPath": "",
  "items": [
    {
      "name": "bedroom.jpg",
      "path": "/full/path/to/file",
      "relativePath": "categories/bedroom.jpg",
      "type": "image",
      "size": 1024000,
      "formattedSize": "1.00 MB",
      "createdAt": "2024-01-01T00:00:00Z",
      "modifiedAt": "2024-01-01T00:00:00Z",
      "extension": ".jpg",
      "fullUrl": "/uploads/categories/bedroom.jpg",
      "thumbnailUrl": "/.thumbnails/bedroom_thumb.jpg",
      "metadata": {
        "width": 1920,
        "height": 1080,
        "format": "JPEG",
        "aspectRatio": 1.78,
        "colorSpace": "RGB"
      }
    }
  ],
  "totalItems": 50,
  "totalFiles": 45,
  "totalFolders": 5,
  "totalSize": 52428800,
  "formattedTotalSize": "50.00 MB",
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### 3. Get Folder Structure
```http
GET /api/filemanager/folders?rootPath=
```

**Response:**
```json
{
  "name": "Uploads",
  "path": "/full/path",
  "relativePath": "",
  "fileCount": 10,
  "folderCount": 3,
  "totalItems": 13,
  "totalSize": 10485760,
  "formattedSize": "10.00 MB",
  "subfolders": [
    {
      "name": "categories",
      "path": "/full/path/categories",
      "relativePath": "categories",
      "fileCount": 25,
      "folderCount": 0,
      "totalItems": 25,
      "totalSize": 5242880,
      "formattedSize": "5.00 MB",
      "subfolders": [],
      "hasChildren": false
    }
  ],
  "hasChildren": true
}
```

### 4. Get File Info
```http
GET /api/filemanager/info?filePath=categories/bedroom.jpg
```

**Response:**
```json
{
  "name": "bedroom.jpg",
  "path": "/full/path/to/file",
  "relativePath": "categories/bedroom.jpg",
  "type": "image",
  "size": 1024000,
  "formattedSize": "1.00 MB",
  "createdAt": "2024-01-01T00:00:00Z",
  "modifiedAt": "2024-01-01T00:00:00Z",
  "extension": ".jpg",
  "fullUrl": "/uploads/categories/bedroom.jpg",
  "thumbnailUrl": "/.thumbnails/bedroom_thumb.jpg",
  "metadata": {
    "width": 1920,
    "height": 1080,
    "format": "JPEG",
    "aspectRatio": 1.78,
    "colorSpace": "RGB"
  }
}
```

### 5. Upload Files
```http
POST /api/filemanager/upload
Content-Type: multipart/form-data
```

**Form Data:**
- `files`: Multiple files
- `FolderPath`: Đường dẫn folder đích
- `CreateThumbnails`: true/false
- `OverwriteExisting`: true/false

**Response:**
```json
{
  "uploadedFiles": [
    {
      "name": "image1.jpg",
      "relativePath": "uploads/image1.jpg",
      "type": "image",
      "size": 1024000,
      "formattedSize": "1.00 MB",
      "thumbnailUrl": "/.thumbnails/image1_thumb.jpg"
    }
  ],
  "errors": [],
  "successCount": 1,
  "errorCount": 0,
  "totalSize": 1024000,
  "formattedTotalSize": "1.00 MB"
}
```

### 6. Create Folder
```http
POST /api/filemanager/create-folder
Content-Type: application/json
```

**Request Body:**
```json
{
  "parentPath": "categories",
  "folderName": "new-folder"
}
```

**Response:**
```json
{
  "name": "new-folder",
  "path": "/full/path/categories/new-folder",
  "relativePath": "categories/new-folder",
  "type": "folder",
  "size": 0,
  "formattedSize": "0 B",
  "createdAt": "2024-01-01T00:00:00Z",
  "modifiedAt": "2024-01-01T00:00:00Z"
}
```

### 7. Delete Files/Folders
```http
DELETE /api/filemanager/delete
Content-Type: application/json
```

**Request Body:**
```json
{
  "filePaths": [
    "categories/old-image.jpg",
    "temp-folder"
  ],
  "permanent": false
}
```

**Response:**
```json
{
  "deletedFiles": [
    "categories/old-image.jpg",
    "temp-folder"
  ],
  "errors": [],
  "successCount": 2,
  "errorCount": 0
}
```

### 8. Move Files
```http
POST /api/filemanager/move
Content-Type: application/json
```

**Request Body:**
```json
{
  "sourcePaths": [
    "temp/image1.jpg",
    "temp/image2.jpg"
  ],
  "destinationPath": "categories",
  "overwriteExisting": false
}
```

**Response:**
```json
{
  "processedFiles": [
    "temp/image1.jpg",
    "temp/image2.jpg"
  ],
  "errors": [],
  "successCount": 2,
  "errorCount": 0,
  "operation": "Move"
}
```

### 9. Copy Files
```http
POST /api/filemanager/copy
Content-Type: application/json
```

**Request Body:**
```json
{
  "sourcePaths": [
    "categories/bedroom.jpg"
  ],
  "destinationPath": "backup",
  "overwriteExisting": false
}
```

**Response:**
```json
{
  "processedFiles": [
    "categories/bedroom.jpg"
  ],
  "errors": [],
  "successCount": 1,
  "errorCount": 0,
  "operation": "Copy"
}
```

### 10. Generate Thumbnail
```http
POST /api/filemanager/generate-thumbnail
Content-Type: application/json
```

**Request Body:**
```json
"categories/bedroom.jpg"
```

**Response:**
```json
{
  "thumbnailUrl": "/.thumbnails/bedroom_thumb.jpg"
}
```

### 11. Get Image Metadata
```http
GET /api/filemanager/metadata?imagePath=categories/bedroom.jpg
```

**Response:**
```json
{
  "width": 1920,
  "height": 1080,
  "format": "JPEG",
  "aspectRatio": 1.78,
  "colorSpace": "RGB"
}
```

### 12. Cleanup Orphaned Files
```http
POST /api/filemanager/cleanup-orphaned
```

**Response:**
```json
{
  "cleanedCount": 5,
  "message": "Cleaned up 5 orphaned file records"
}
```

### 13. Sync Database
```http
POST /api/filemanager/sync-database
```

**Response:**
```json
{
  "syncedCount": 10,
  "message": "Synced 10 files to database"
}
```

### 14. Get Missing Files
```http
GET /api/filemanager/missing-files
```

**Response:**
```json
{
  "missingFiles": [
    "categories/deleted-image.jpg",
    "products/missing-product.jpg"
  ],
  "count": 2
}
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "Invalid path",
  "details": "Path contains invalid characters"
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized",
  "details": "JWT token is required"
}
```

### 403 Forbidden
```json
{
  "error": "Forbidden",
  "details": "Admin access required"
}
```

### 404 Not Found
```json
{
  "error": "Not Found",
  "details": "Directory not found: invalid-path"
}
```

### 409 Conflict
```json
{
  "error": "Conflict",
  "details": "Folder 'new-folder' already exists"
}
```

### 500 Internal Server Error
```json
{
  "error": "Internal Server Error",
  "details": "An unexpected error occurred"
}
```

## File Types Support

### Supported Image Formats
- `.jpg`, `.jpeg`
- `.png`
- `.gif`
- `.bmp`
- `.webp`

### File Size Limits
- Maximum file size: 10MB per file
- Maximum total upload size: 100MB per request

### Security Features
- Path traversal protection
- File type validation
- Size limits enforcement
- Admin-only access control
- JWT token validation

## Database Integration

### Image Model
```csharp
public class Image
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string AltText { get; set; }
    public int? ProductId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual Product? Product { get; set; }
}
```

### Repository Pattern
- `IImageRepository`: Interface cho database operations
- `ImageRepository`: Implementation với Entity Framework
- Soft delete support
- Orphaned files tracking

## Performance Considerations

### Pagination
- Default page size: 20 items
- Maximum page size: 100 items
- Efficient database queries với Skip/Take

### Caching
- Thumbnail caching
- File metadata caching
- Static file serving optimization

### Thumbnails
- Automatic generation cho images
- 150x150 pixels (maintain aspect ratio)
- JPEG format cho thumbnails
- Lazy loading support

## Usage Examples

### Frontend Integration
```javascript
// Browse files
const response = await fetch('/api/filemanager/browse?path=categories&page=1&pageSize=20', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
const data = await response.json();

// Upload files
const formData = new FormData();
formData.append('FolderPath', 'categories');
formData.append('CreateThumbnails', 'true');
files.forEach(file => formData.append('files', file));

const uploadResponse = await fetch('/api/filemanager/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});
```

### File URLs
- **Original files**: `/uploads/{relativePath}`
- **Thumbnails**: `/.thumbnails/{filename}_thumb.{ext}`

## Testing

Sử dụng file `DecorStore.API.http` để test các endpoints:

1. Login để lấy JWT token
2. Test các endpoints với token
3. Verify responses và error handling

## Deployment Notes

### Production Considerations
- Configure proper file permissions
- Set up CDN cho static files
- Monitor disk space usage
- Regular cleanup của orphaned files
- Backup strategy cho uploaded files

### Environment Variables
- `ImageSettings:BasePath`: Đường dẫn uploads directory
- `ImageSettings:MaxFileSize`: Maximum file size
- `ImageSettings:AllowedExtensions`: Allowed file extensions
