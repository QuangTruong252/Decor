# DecorStore API - Complete Documentation Summary

## API Overview

**Title:** DecorStore API  
**Version:** v1  
**Base URL:** Not specified in swagger  
**Authentication:** Bearer JWT Token  
**Security Scheme:** API Key in Authorization header  

## Table of Contents

1. [Authentication Endpoints](#authentication-endpoints)
2. [Banner Management](#banner-management)
3. [Cart Operations](#cart-operations)
4. [Category Management](#category-management)
5. [Customer Management](#customer-management)
6. [Dashboard & Analytics](#dashboard--analytics)
7. [File Manager](#file-manager)
8. [Health Check](#health-check)
9. [Image Management](#image-management)
10. [Order Management](#order-management)
11. [Product Management](#product-management)
12. [Review System](#review-system)
13. [Data Transfer Objects (DTOs)](#data-transfer-objects-dtos)
14. [Common Patterns](#common-patterns)

---

## Authentication Endpoints

### POST /api/Auth/register
**Purpose:** Register a new user account  
**Request Body:** `RegisterDTO`  
**Response:** `AuthResponseDTO`  

### POST /api/Auth/login
**Purpose:** Authenticate user and get JWT token  
**Request Body:** `LoginDTO`  
**Response:** `AuthResponseDTO`  

### GET /api/Auth/user
**Purpose:** Get current authenticated user details  
**Response:** `UserDTO`  

### GET /api/Auth/check-claims
**Purpose:** Validate user claims/permissions  
**Response:** 200 OK  

### POST /api/Auth/make-admin
**Purpose:** Elevate user to admin role  
**Query Parameters:**
- `email` (string): User email to promote  
**Response:** `UserDTO`  

---

## Banner Management

### GET /api/Banner
**Purpose:** Get all banners  
**Response:** Array of `BannerDTO`  

### GET /api/Banner/active
**Purpose:** Get only active banners  
**Response:** Array of `BannerDTO`  

### GET /api/Banner/{id}
**Purpose:** Get specific banner by ID  
**Path Parameters:**
- `id` (int32): Banner ID  
**Response:** `BannerDTO`  

### POST /api/Banner
**Purpose:** Create new banner  
**Request Body:** Multipart form data
- `Title` (string, required, max 100 chars)
- `ImageFile` (binary, required)
- `Link` (string, max 255 chars)
- `IsActive` (boolean)
- `DisplayOrder` (int32)  
**Response:** `Banner`  

### PUT /api/Banner/{id}
**Purpose:** Update existing banner  
**Path Parameters:**
- `id` (int32): Banner ID  
**Request Body:** Multipart form data (same as POST, but Title only required)  
**Response:** 200 OK  

### DELETE /api/Banner/{id}
**Purpose:** Delete banner  
**Path Parameters:**
- `id` (int32): Banner ID  
**Response:** 200 OK  

---

## Cart Operations

### GET /api/Cart
**Purpose:** Get user's cart  
**Response:** `CartDTO`  

### POST /api/Cart
**Purpose:** Add item to cart  
**Request Body:** `AddToCartDTO`  
**Response:** `CartDTO`  

### DELETE /api/Cart
**Purpose:** Clear entire cart  
**Response:** `CartDTO`  

### PUT /api/Cart/items/{id}
**Purpose:** Update cart item quantity  
**Path Parameters:**
- `id` (int32): Cart item ID  
**Request Body:** `UpdateCartItemDTO`  
**Response:** `CartDTO`  

### DELETE /api/Cart/items/{id}
**Purpose:** Remove item from cart  
**Path Parameters:**
- `id` (int32): Cart item ID  
**Response:** `CartDTO`  

### POST /api/Cart/merge
**Purpose:** Merge guest cart with user cart  
**Response:** 200 OK  

---

## Category Management

### GET /api/Category
**Purpose:** Get categories with advanced filtering and pagination  
**Query Parameters:**
- `SearchTerm` (string): Search in category names
- `ParentId` (int32): Filter by parent category
- `IsRootCategory` (boolean): Get only root categories
- `IncludeSubcategories` (boolean): Include child categories
- `IncludeProductCount` (boolean): Include product counts
- `CreatedAfter` (datetime): Filter by creation date
- `CreatedBefore` (datetime): Filter by creation date
- `IncludeDeleted` (boolean): Include soft-deleted items
- Pagination: `PageNumber`, `PageSize`, `SortBy`, `SortDirection`, `Skip`, `IsDescending`  
**Response:** `CategoryDTOPagedResult`  

### GET /api/Category/all
**Purpose:** Get all categories without pagination  
**Response:** Array of `CategoryDTO`  

### GET /api/Category/hierarchical
**Purpose:** Get categories in hierarchical structure  
**Response:** Array of `CategoryDTO`  

### GET /api/Category/{id}
**Purpose:** Get specific category  
**Path Parameters:**
- `id` (int32): Category ID  
**Response:** `CategoryDTO`  

### GET /api/Category/slug/{slug}
**Purpose:** Get category by slug  
**Path Parameters:**
- `slug` (string): Category slug  
**Response:** `CategoryDTO`  

### GET /api/Category/with-product-count
**Purpose:** Get categories with product counts  
**Response:** Array of `CategoryDTO`  

### GET /api/Category/{parentId}/subcategories
**Purpose:** Get subcategories of a parent  
**Path Parameters:**
- `parentId` (int32): Parent category ID  
**Response:** Array of `CategoryDTO`  

### GET /api/Category/{categoryId}/product-count
**Purpose:** Get product count for category  
**Path Parameters:**
- `categoryId` (int32): Category ID  
**Response:** int32  

### GET /api/Category/popular
**Purpose:** Get popular categories  
**Query Parameters:**
- `count` (int32, default: 10): Number of categories  
**Response:** Array of `CategoryDTO`  

### GET /api/Category/root
**Purpose:** Get root categories only  
**Response:** Array of `CategoryDTO`  

### POST /api/Category
**Purpose:** Create new category  
**Request Body:** `CreateCategoryDTO`  
**Response:** `Category`  

### PUT /api/Category/{id}
**Purpose:** Update category  
**Path Parameters:**
- `id` (int32): Category ID  
**Request Body:** `UpdateCategoryDTO`  
**Response:** 200 OK  

### DELETE /api/Category/{id}
**Purpose:** Delete category  
**Path Parameters:**
- `id` (int32): Category ID  
**Response:** 200 OK  

### Excel Operations
- **POST /api/Category/import** - Import categories from Excel
- **GET /api/Category/export** - Export categories to Excel
- **GET /api/Category/export-template** - Get import template
- **POST /api/Category/validate-import** - Validate import file
- **POST /api/Category/import-statistics** - Get import statistics

---

## Customer Management

### GET /api/Customer
**Purpose:** Get customers with filtering and pagination  
**Query Parameters:**
- `SearchTerm` (string): Search in customer data
- `Email` (string): Filter by email
- `City`, `State`, `Country`, `PostalCode` (string): Location filters
- `RegisteredAfter`, `RegisteredBefore` (datetime): Registration date filters
- `HasOrders` (boolean): Filter customers with orders
- `IncludeOrderCount`, `IncludeTotalSpent` (boolean): Include statistics
- `IncludeDeleted` (boolean): Include soft-deleted customers
- Pagination parameters  
**Response:** `CustomerDTOPagedResult`  

### GET /api/Customer/all
**Purpose:** Get all customers without pagination  
**Response:** Array of `CustomerDTO`  

### GET /api/Customer/{id}
**Purpose:** Get specific customer  
**Path Parameters:**
- `id` (int32): Customer ID  
**Response:** `CustomerDTO`  

### GET /api/Customer/email/{email}
**Purpose:** Get customer by email  
**Path Parameters:**
- `email` (string): Customer email  
**Response:** `CustomerDTO`  

### GET /api/Customer/with-orders
**Purpose:** Get customers who have placed orders  
**Response:** Array of `CustomerDTO`  

### GET /api/Customer/top-by-order-count
**Purpose:** Get top customers by order count  
**Query Parameters:**
- `count` (int32, default: 10): Number of customers  
**Response:** Array of `CustomerDTO`  

### GET /api/Customer/top-by-spending
**Purpose:** Get top customers by spending  
**Query Parameters:**
- `count` (int32, default: 10): Number of customers  
**Response:** Array of `CustomerDTO`  

### GET /api/Customer/{customerId}/order-count
**Purpose:** Get order count for customer  
**Path Parameters:**
- `customerId` (int32): Customer ID  
**Response:** int32  

### GET /api/Customer/{customerId}/total-spent
**Purpose:** Get total spent by customer  
**Path Parameters:**
- `customerId` (int32): Customer ID  
**Response:** double  

### GET /api/Customer/by-location
**Purpose:** Get customers by location  
**Query Parameters:**
- `city`, `state`, `country` (string): Location filters  
**Response:** Array of `CustomerDTO`  

### POST /api/Customer
**Purpose:** Create new customer  
**Request Body:** `CreateCustomerDTO`  
**Response:** `Customer`  

### PUT /api/Customer/{id}
**Purpose:** Update customer  
**Path Parameters:**
- `id` (int32): Customer ID  
**Request Body:** `UpdateCustomerDTO`  
**Response:** 200 OK  

### DELETE /api/Customer/{id}
**Purpose:** Delete customer  
**Path Parameters:**
- `id` (int32): Customer ID  
**Response:** 200 OK  

### Excel Operations
- **POST /api/Customer/import** - Import customers from Excel
- **GET /api/Customer/export** - Export customers to Excel
- **GET /api/Customer/export-template** - Get import template
- **POST /api/Customer/validate-import** - Validate import file
- **POST /api/Customer/import-statistics** - Get import statistics

---

## Dashboard & Analytics

### GET /api/Dashboard/summary
**Purpose:** Get dashboard summary with key metrics  
**Response:** `DashboardSummaryDTO`  

### GET /api/Dashboard/sales-trend
**Purpose:** Get sales trend data  
**Query Parameters:**
- `period` (string, default: "daily"): Time period for grouping
- `startDate` (datetime): Start date for trend
- `endDate` (datetime): End date for trend  
**Response:** `SalesTrendDTO`  

### GET /api/Dashboard/popular-products
**Purpose:** Get popular products for dashboard  
**Query Parameters:**
- `limit` (int32, default: 5): Number of products  
**Response:** Array of `PopularProductDTO`  

### GET /api/Dashboard/sales-by-category
**Purpose:** Get sales breakdown by category  
**Response:** Array of `CategorySalesDTO`  

### GET /api/Dashboard/order-status-distribution
**Purpose:** Get distribution of order statuses  
**Response:** `OrderStatusDistributionDTO`  

---

## File Manager

### GET /api/FileManager/browse
**Purpose:** Browse files and folders  
**Query Parameters:**
- `Path` (string): Directory path to browse
- `Search` (string): Search term
- `FileType`, `Extension` (string): File type filters
- `SortBy`, `SortOrder` (string): Sorting options
- `MinSize`, `MaxSize` (int64): File size filters
- `FromDate`, `ToDate` (datetime): Date filters
- Pagination parameters  
**Response:** `FileBrowseResponseDTO`  

### GET /api/FileManager/folders
**Purpose:** Get folder structure  
**Query Parameters:**
- `rootPath` (string): Root path for folder tree  
**Response:** `FolderStructureDTO`  

### GET /api/FileManager/info
**Purpose:** Get file information  
**Query Parameters:**
- `filePath` (string): Path to file  
**Response:** `FileItemDTO`  

### POST /api/FileManager/upload
**Purpose:** Upload files  
**Request Body:** Multipart form data
- `FolderPath` (string): Destination folder
- `CreateThumbnails` (boolean): Generate thumbnails
- `OverwriteExisting` (boolean): Overwrite existing files
- `files` (array of binary): Files to upload  
**Response:** `FileUploadResponseDTO`  

### POST /api/FileManager/create-folder
**Purpose:** Create new folder  
**Request Body:** `CreateFolderRequestDTO`  
**Response:** `FileItemDTO`  

### DELETE /api/FileManager/delete
**Purpose:** Delete files/folders  
**Request Body:** `DeleteFileRequestDTO`  
**Response:** `DeleteFileResponseDTO`  

### POST /api/FileManager/move
**Purpose:** Move files/folders  
**Request Body:** `MoveFileRequestDTO`  
**Response:** `FileOperationResponseDTO`  

### POST /api/FileManager/copy
**Purpose:** Copy files/folders  
**Request Body:** `CopyFileRequestDTO`  
**Response:** `FileOperationResponseDTO`  

### POST /api/FileManager/generate-thumbnail
**Purpose:** Generate thumbnail for image  
**Request Body:** string (image path)  
**Response:** string (thumbnail path)  

### GET /api/FileManager/metadata
**Purpose:** Get image metadata  
**Query Parameters:**
- `imagePath` (string): Path to image  
**Response:** `ImageMetadataDTO`  

### POST /api/FileManager/cleanup-orphaned
**Purpose:** Clean up orphaned files  
**Response:** int32 (count of cleaned files)  

### POST /api/FileManager/sync-database
**Purpose:** Sync files with database  
**Response:** int32 (count of synced files)  

### GET /api/FileManager/missing-files
**Purpose:** Get list of missing files  
**Response:** Array of string (file paths)  

### GET /api/FileManager/health
**Purpose:** Check file manager health  
**Response:** 200 OK  

### GET /api/FileManager/download
**Purpose:** Download file  
**Query Parameters:**
- `filePath` (string): Path to file  
**Response:** File download  

---

## Health Check

### GET /api/HealthCheck
**Purpose:** Check API health status  
**Response:** 200 OK  

---

## Image Management

### POST /api/Image/upload
**Purpose:** Upload images  
**Request Body:** Multipart form data
- `Files` (array of binary, required): Image files
- `folderName` (string): Destination folder  
**Response:** `ImageUploadResponseDTO`  

### GET /api/Image/system
**Purpose:** Get system images  
**Response:** `ImageUploadResponseDTO`  

### GET /api/Image/exists/{fileName}
**Purpose:** Check if image exists  
**Path Parameters:**
- `fileName` (string): Image file name  
**Response:** boolean  

### GET /api/Image/{ids}
**Purpose:** Get images by IDs  
**Path Parameters:**
- `ids` (string): Comma-separated image IDs  
**Response:** `ImageUploadResponseDTO`  

### GET /api/Image/api/Image/get-by-filepaths
**Purpose:** Get images by filepaths
**Path Parameters:**
- filePaths (array of string): list of filepath
**Response:** `ImageUploadResponseDTO`

---

## Order Management

### GET /api/Order
**Purpose:** Get orders with advanced filtering  
**Query Parameters:**
- `SearchTerm` (string): Search in order data
- `UserId`, `CustomerId` (int32): Filter by user/customer
- `OrderStatus` (string): Filter by status
- `PaymentMethod` (string): Filter by payment method
- `MinAmount`, `MaxAmount` (double): Amount range filters
- `OrderDateFrom`, `OrderDateTo` (datetime): Date range filters
- `ShippingCity`, `ShippingState`, `ShippingCountry` (string): Shipping filters
- `IncludeDeleted` (boolean): Include soft-deleted orders
- Pagination parameters  
**Response:** `OrderDTOPagedResult`  

### GET /api/Order/all
**Purpose:** Get all orders without pagination  
**Response:** Array of `OrderDTO`  

### GET /api/Order/user/{userId}
**Purpose:** Get orders for specific user  
**Path Parameters:**
- `userId` (int32): User ID  
**Response:** Array of `OrderDTO`  

### GET /api/Order/{id}
**Purpose:** Get specific order  
**Path Parameters:**
- `id` (int32): Order ID  
**Response:** `OrderDTO`  

### GET /api/Order/recent
**Purpose:** Get recent orders  
**Query Parameters:**
- `count` (int32, default: 10): Number of orders  
**Response:** Array of `OrderDTO`  

### GET /api/Order/status/{status}
**Purpose:** Get orders by status  
**Path Parameters:**
- `status` (string): Order status  
**Query Parameters:**
- `count` (int32, default: 50): Number of orders  
**Response:** Array of `OrderDTO`  

### GET /api/Order/date-range
**Purpose:** Get orders in date range  
**Query Parameters:**
- `startDate`, `endDate` (datetime): Date range  
**Response:** Array of `OrderDTO`  

### GET /api/Order/revenue
**Purpose:** Get revenue for date range  
**Query Parameters:**
- `startDate`, `endDate` (datetime): Date range  
**Response:** double  

### GET /api/Order/status-counts
**Purpose:** Get order counts by status  
**Response:** Object with status counts  

### POST /api/Order
**Purpose:** Create new order  
**Request Body:** `CreateOrderDTO`  
**Response:** `OrderDTO`  

### PUT /api/Order/{id}
**Purpose:** Update order  
**Path Parameters:**
- `id` (int32): Order ID  
**Request Body:** `UpdateOrderDTO`  
**Response:** 200 OK  

### PUT /api/Order/{id}/status
**Purpose:** Update order status  
**Path Parameters:**
- `id` (int32): Order ID  
**Request Body:** `UpdateOrderStatusDTO`  
**Response:** 200 OK  

### DELETE /api/Order/{id}
**Purpose:** Delete order  
**Path Parameters:**
- `id` (int32): Order ID  
**Response:** 200 OK  

### DELETE /api/Order/bulk
**Purpose:** Bulk delete orders  
**Request Body:** `BulkDeleteDTO`  
**Response:** 200 OK  

### Excel Operations
- **POST /api/Order/import** - Import orders from Excel
- **GET /api/Order/export** - Export orders to Excel
- **GET /api/Order/export-template** - Get import template
- **POST /api/Order/validate-import** - Validate import file
- **POST /api/Order/import-statistics** - Get import statistics

---

## Product Management

### GET /api/Products
**Purpose:** Get products with advanced filtering  
**Query Parameters:**
- `SearchTerm` (string): Search in product data
- `CategoryId` (int32): Filter by category
- `MinPrice`, `MaxPrice` (double): Price range filters
- `IsFeatured`, `IsActive` (boolean): Status filters
- `CreatedAfter`, `CreatedBefore` (datetime): Date filters
- `StockQuantityMin`, `StockQuantityMax` (int32): Stock filters
- `MinRating` (float): Rating filter
- `SKU` (string): Filter by SKU
- Pagination parameters  
**Response:** `ProductDTOPagedResult`  

### GET /api/Products/all
**Purpose:** Get all products without pagination  
**Response:** Array of `ProductDTO`  

### GET /api/Products/{id}
**Purpose:** Get specific product  
**Path Parameters:**
- `id` (int32): Product ID  
**Response:** `ProductDTO`  

### GET /api/Products/category/{categoryId}
**Purpose:** Get products by category  
**Path Parameters:**
- `categoryId` (int32): Category ID  
**Query Parameters:**
- `count` (int32, default: 20): Number of products  
**Response:** Array of `ProductDTO`  

### GET /api/Products/featured
**Purpose:** Get featured products  
**Query Parameters:**
- `count` (int32, default: 10): Number of products  
**Response:** Array of `ProductDTO`  

### GET /api/Products/{id}/related
**Purpose:** Get related products  
**Path Parameters:**
- `id` (int32): Product ID  
**Query Parameters:**
- `count` (int32, default: 5): Number of products  
**Response:** Array of `ProductDTO`  

### GET /api/Products/top-rated
**Purpose:** Get top-rated products  
**Query Parameters:**
- `count` (int32, default: 10): Number of products  
**Response:** Array of `ProductDTO`  

### GET /api/Products/low-stock
**Purpose:** Get low-stock products  
**Query Parameters:**
- `threshold` (int32, default: 10): Stock threshold  
**Response:** Array of `ProductDTO`  

### POST /api/Products
**Purpose:** Create new product  
**Request Body:** `CreateProductDTO`  
**Response:** `ProductDTO`  

### PUT /api/Products/{id}
**Purpose:** Update product  
**Path Parameters:**
- `id` (int32): Product ID  
**Request Body:** `UpdateProductDTO`  
**Response:** 200 OK  

### DELETE /api/Products/{id}
**Purpose:** Delete product  
**Path Parameters:**
- `id` (int32): Product ID  
**Response:** 200 OK  

### DELETE /api/Products/bulk
**Purpose:** Bulk delete products  
**Request Body:** `BulkDeleteDTO`  
**Response:** 200 OK  

### POST /api/Products/{id}/images
**Purpose:** Add image to product  
**Path Parameters:**
- `id` (int32): Product ID  
**Request Body:** Multipart form data with image  
**Response:** 200 OK  

### DELETE /api/Products/{productId}/images/{imageId}
**Purpose:** Remove image from product  
**Path Parameters:**
- `productId` (int32): Product ID
- `imageId` (int32): Image ID  
**Response:** 200 OK  

### Excel Operations
- **POST /api/Products/import** - Import products from Excel
- **GET /api/Products/export** - Export products to Excel
- **GET /api/Products/export-template** - Get import template
- **POST /api/Products/validate-import** - Validate import file
- **POST /api/Products/import-statistics** - Get import statistics

---

## Review System

### GET /api/Review/product/{productId}
**Purpose:** Get reviews for product  
**Path Parameters:**
- `productId` (int32): Product ID  
**Response:** Array of `ReviewDTO`  

### GET /api/Review/{id}
**Purpose:** Get specific review  
**Path Parameters:**
- `id` (int32): Review ID  
**Response:** `ReviewDTO`  

### GET /api/Review/product/{productId}/rating
**Purpose:** Get average rating for product  
**Path Parameters:**
- `productId` (int32): Product ID  
**Response:** float  

### POST /api/Review
**Purpose:** Create new review  
**Request Body:** `CreateReviewDTO`  
**Response:** `Review`  

### PUT /api/Review/{id}
**Purpose:** Update review  
**Path Parameters:**
- `id` (int32): Review ID  
**Request Body:** `UpdateReviewDTO`  
**Response:** 200 OK  

### DELETE /api/Review/{id}
**Purpose:** Delete review  
**Path Parameters:**
- `id` (int32): Review ID  
**Response:** 200 OK  

---

## Data Transfer Objects (DTOs)

### Authentication DTOs

#### RegisterDTO
- `username` (string, 3-50 chars, required)
- `email` (string, email format, required)
- `password` (string, 6-100 chars, required)
- `confirmPassword` (string, optional)

#### LoginDTO
- `email` (string, required)
- `password` (string, required)

#### AuthResponseDTO
- `token` (string, nullable)
- `user` (UserDTO)

#### UserDTO
- `id` (int32)
- `username` (string, nullable)
- `email` (string, nullable)
- `role` (string, nullable)

### Banner DTOs

#### BannerDTO
- `id` (int32)
- `title` (string, nullable)
- `imageUrl` (string, nullable)
- `link` (string, nullable)
- `isActive` (boolean)
- `displayOrder` (int32)
- `createdAt` (datetime)

### Cart DTOs

#### CartDTO
- `id` (int32)
- `userId` (int32, nullable)
- `sessionId` (string, nullable)
- `totalAmount` (double)
- `totalItems` (int32)
- `updatedAt` (datetime)
- `items` (array of CartItemDTO, nullable)

#### CartItemDTO
- `id` (int32)
- `productId` (int32)
- `productName` (string, nullable)
- `productSlug` (string, nullable)
- `productImage` (string, nullable)
- `quantity` (int32)
- `unitPrice` (double)
- `subtotal` (double)

#### AddToCartDTO
- `productId` (int32, required)
- `quantity` (int32, 1-max, required)

#### UpdateCartItemDTO
- `quantity` (int32, 1-max, required)

### Category DTOs

#### CategoryDTO
- `id` (int32)
- `name` (string, nullable)
- `slug` (string, nullable)
- `description` (string, nullable)
- `parentId` (int32, nullable)
- `parentName` (string, nullable)
- `createdAt` (datetime)
- `subcategories` (array of CategoryDTO, nullable)
- `imageDetails` (array of ImageDTO, nullable)

#### CreateCategoryDTO
- `name` (string, 2-100 chars, required)
- `slug` (string, 0-100 chars, required)
- `description` (string, 0-255 chars, nullable)
- `parentId` (int32, nullable)
- `imageIds` (array of int32, nullable)

#### UpdateCategoryDTO
- `name` (string, 2-100 chars, required)
- `slug` (string, 0-100 chars, nullable)
- `description` (string, 0-255 chars, nullable)
- `parentId` (int32, nullable)
- `imageIds` (array of int32, nullable)

### Customer DTOs

#### CustomerDTO
- `id` (int32)
- `firstName` (string, nullable)
- `lastName` (string, nullable)
- `email` (string, nullable)
- `address` (string, nullable)
- `city` (string, nullable)
- `state` (string, nullable)
- `postalCode` (string, nullable)
- `country` (string, nullable)
- `phone` (string, nullable)
- `fullName` (string, nullable)
- `createdAt` (datetime)
- `updatedAt` (datetime)

#### CreateCustomerDTO
- `firstName` (string, 2-100 chars, required)
- `lastName` (string, 2-100 chars, required)
- `email` (string, email format, 0-100 chars, required)
- `address` (string, 0-255 chars, nullable)
- `city` (string, 0-100 chars, nullable)
- `state` (string, 0-50 chars, nullable)
- `postalCode` (string, 0-20 chars, nullable)
- `country` (string, 0-50 chars, nullable)
- `phone` (string, tel format, 0-20 chars, nullable)

#### UpdateCustomerDTO
- `firstName` (string, 2-100 chars, required)
- `lastName` (string, 2-100 chars, required)
- `address` (string, 0-255 chars, nullable)
- `city` (string, 0-100 chars, nullable)
- `state` (string, 0-50 chars, nullable)
- `postalCode` (string, 0-20 chars, nullable)
- `country` (string, 0-50 chars, nullable)
- `phone` (string, tel format, 0-20 chars, nullable)

### Dashboard DTOs

#### DashboardSummaryDTO
- `totalProducts` (int32)
- `totalOrders` (int32)
- `totalCustomers` (int32)
- `totalRevenue` (double)
- `recentOrders` (array of RecentOrderDTO, nullable)
- `popularProducts` (array of PopularProductDTO, nullable)
- `salesByCategory` (array of CategorySalesDTO, nullable)
- `orderStatusDistribution` (OrderStatusDistributionDTO)
- `recentSalesTrend` (array of SalesTrendPointDTO, nullable)

#### SalesTrendDTO
- `period` (string, nullable)
- `startDate` (datetime)
- `endDate` (datetime)
- `data` (array of SalesTrendPointDTO, nullable)

#### SalesTrendPointDTO
- `date` (datetime)
- `revenue` (double)
- `orderCount` (int32)

#### PopularProductDTO
- `productId` (int32)
- `name` (string, nullable)
- `imageUrl` (string, nullable)
- `price` (double)
- `totalSold` (int32)
- `totalRevenue` (double)

#### CategorySalesDTO
- `categoryId` (int32)
- `categoryName` (string, nullable)
- `totalSales` (int32)
- `totalRevenue` (double)
- `percentage` (double)

#### OrderStatusDistributionDTO
- `pending` (int32)
- `processing` (int32)
- `shipped` (int32)
- `delivered` (int32)
- `cancelled` (int32)
- `total` (int32)

#### RecentOrderDTO
- `orderId` (int32)
- `orderDate` (datetime)
- `customerName` (string, nullable)
- `totalAmount` (double)
- `orderStatus` (string, nullable)

### File Manager DTOs

#### FileBrowseResponseDTO
- `currentPath` (string, nullable)
- `parentPath` (string, nullable)
- `items` (array of FileItemDTO, nullable)
- `totalItems` (int32)
- `totalFiles` (int32)
- `totalFolders` (int32)
- `totalSize` (int64)
- `formattedTotalSize` (string, nullable)
- `page` (int32)
- `pageSize` (int32)
- `totalPages` (int32)
- `hasNextPage` (boolean)
- `hasPreviousPage` (boolean)

#### FileItemDTO
- `name` (string, nullable)
- `path` (string, nullable)
- `relativePath` (string, nullable)
- `type` (string, nullable)
- `size` (int64)
- `formattedSize` (string, nullable)
- `createdAt` (datetime)
- `modifiedAt` (datetime)
- `extension` (string, nullable)
- `thumbnailUrl` (string, nullable)
- `fullUrl` (string, nullable)
- `metadata` (ImageMetadataDTO)
- `isSelected` (boolean)

#### FolderStructureDTO
- `name` (string, nullable)
- `path` (string, nullable)
- `relativePath` (string, nullable)
- `fileCount` (int32)
- `folderCount` (int32)
- `totalItems` (int32)
- `totalSize` (int64)
- `formattedSize` (string, nullable)
- `subfolders` (array of FolderStructureDTO, nullable)
- `isExpanded` (boolean)
- `hasChildren` (boolean)

#### CreateFolderRequestDTO
- `parentPath` (string, nullable)
- `folderName` (string, nullable)

#### DeleteFileRequestDTO
- `filePaths` (array of string, nullable)
- `permanent` (boolean)

#### DeleteFileResponseDTO
- `deletedFiles` (array of string, nullable)
- `errors` (array of string, nullable)
- `successCount` (int32)
- `errorCount` (int32)

#### MoveFileRequestDTO
- `sourcePaths` (array of string, nullable)
- `destinationPath` (string, nullable)
- `overwriteExisting` (boolean)

#### CopyFileRequestDTO
- `sourcePaths` (array of string, nullable)
- `destinationPath` (string, nullable)
- `overwriteExisting` (boolean)

#### FileOperationResponseDTO
- `processedFiles` (array of string, nullable)
- `errors` (array of string, nullable)
- `successCount` (int32)
- `errorCount` (int32)
- `operation` (string, nullable)

#### FileUploadResponseDTO
- `uploadedFiles` (array of FileItemDTO, nullable)
- `errors` (array of string, nullable)
- `successCount` (int32)
- `errorCount` (int32)
- `totalSize` (int64)
- `formattedTotalSize` (string, nullable)

### Image DTOs

#### ImageDTO
- `id` (int32)
- `fileName` (string, nullable)
- `filePath` (string, nullable)
- `altText` (string, nullable)
- `createdAt` (datetime)

#### ImageResponseDTO
- `id` (int32)
- `fileName` (string, nullable)
- `filePath` (string, nullable)
- `altText` (string, nullable)
- `createdAt` (datetime)

#### ImageUploadResponseDTO
- `images` (array of ImageResponseDTO, nullable)

#### ImageMetadataDTO
- `width` (int32)
- `height` (int32)
- `format` (string, nullable)
- `aspectRatio` (double)
- `colorSpace` (string, nullable)

### Order DTOs

#### OrderDTO
- `id` (int32)
- `userId` (int32)
- `userFullName` (string, nullable)
- `customerId` (int32, nullable)
- `customerFullName` (string, nullable)
- `totalAmount` (double)
- `orderStatus` (string, nullable)
- `paymentMethod` (string, nullable)
- `shippingAddress` (string, nullable)
- `shippingCity` (string, nullable)
- `shippingState` (string, nullable)
- `shippingPostalCode` (string, nullable)
- `shippingCountry` (string, nullable)
- `contactPhone` (string, nullable)
- `contactEmail` (string, nullable)
- `notes` (string, nullable)
- `orderDate` (datetime)
- `updatedAt` (datetime)
- `orderItems` (array of OrderItemDTO, nullable)

#### OrderItemDTO
- `id` (int32)
- `orderId` (int32)
- `productId` (int32)
- `productName` (string, nullable)
- `productImageUrl` (string, nullable)
- `quantity` (int32)
- `unitPrice` (double)
- `subtotal` (double)

#### CreateOrderDTO
- `userId` (int32, required)
- `customerId` (int32, nullable)
- `paymentMethod` (string, 0-50 chars, required)
- `shippingAddress` (string, 0-255 chars, required)
- `shippingCity` (string, 0-100 chars, nullable)
- `shippingState` (string, 0-50 chars, nullable)
- `shippingPostalCode` (string, 0-20 chars, nullable)
- `shippingCountry` (string, 0-50 chars, nullable)
- `contactPhone` (string, tel format, 0-100 chars, nullable)
- `contactEmail` (string, email format, 0-100 chars, nullable)
- `notes` (string, 0-255 chars, nullable)
- `orderItems` (array of CreateOrderItemDTO, required)

#### CreateOrderItemDTO
- `productId` (int32, required)
- `quantity` (int32, 1-max, required)

#### UpdateOrderDTO
- `customerId` (int32, nullable)
- `paymentMethod` (string, 0-50 chars, nullable)
- `shippingAddress` (string, 0-255 chars, nullable)
- `shippingCity` (string, 0-100 chars, nullable)
- `shippingState` (string, 0-50 chars, nullable)
- `shippingPostalCode` (string, 0-20 chars, nullable)
- `shippingCountry` (string, 0-50 chars, nullable)
- `contactPhone` (string, tel format, 0-100 chars, nullable)
- `contactEmail` (string, email format, 0-100 chars, nullable)
- `notes` (string, 0-255 chars, nullable)

#### UpdateOrderStatusDTO
- `orderStatus` (string, 0-50 chars, required)

### Product DTOs

#### ProductDTO
- `id` (int32)
- `name` (string, nullable)
- `slug` (string, nullable)
- `description` (string, nullable)
- `price` (double)
- `originalPrice` (double)
- `stockQuantity` (int32)
- `sku` (string, nullable)
- `categoryId` (int32)
- `categoryName` (string, nullable)
- `isFeatured` (boolean)
- `isActive` (boolean)
- `averageRating` (float)
- `createdAt` (datetime)
- `updatedAt` (datetime)
- `images` (array of string, nullable)
- `imageDetails` (array of ImageDTO, nullable)

#### CreateProductDTO
- `name` (string, 3-255 chars, required)
- `slug` (string, 0-255 chars, required)
- `description` (string, nullable)
- `price` (double, min 0.01, required)
- `originalPrice` (double)
- `stockQuantity` (int32, required)
- `sku` (string, 0-50 chars, required)
- `categoryId` (int32, required)
- `isFeatured` (boolean)
- `isActive` (boolean)
- `imageIds` (array of int32, nullable)

#### UpdateProductDTO
- `name` (string, 3-255 chars, required)
- `slug` (string, 0-255 chars, nullable)
- `description` (string, nullable)
- `price` (double, min 0.01, required)
- `originalPrice` (double)
- `stockQuantity` (int32)
- `sku` (string, 0-50 chars, nullable)
- `categoryId` (int32)
- `isFeatured` (boolean)
- `isActive` (boolean)
- `imageIds` (array of int32, nullable)

### Review DTOs

#### ReviewDTO
- `id` (int32)
- `userId` (int32)
- `userName` (string, nullable)
- `productId` (int32)
- `rating` (int32)
- `comment` (string, nullable)
- `createdAt` (datetime)

#### CreateReviewDTO
- `userId` (int32, required)
- `productId` (int32, required)
- `rating` (int32, 1-5, required)
- `comment` (string, 0-500 chars, nullable)

#### UpdateReviewDTO
- `rating` (int32, 1-5, required)
- `comment` (string, 0-500 chars, nullable)

### Utility DTOs

#### BulkDeleteDTO
- `ids` (array of int32, min 1 item, required)

#### PaginationMetadata
- `currentPage` (int32)
- `pageSize` (int32)
- `totalCount` (int32)
- `totalPages` (int32)
- `hasNext` (boolean)
- `hasPrevious` (boolean)
- `nextPage` (int32, nullable, readonly)
- `previousPage` (int32, nullable, readonly)

#### DateRange
- `startDate` (datetime, nullable)
- `endDate` (datetime, nullable)
- `daysInRange` (int32, nullable, readonly)

### Excel Import/Export DTOs

#### ExcelValidationResultDTO
- `isValid` (boolean)
- `errors` (array of string, nullable)
- `warnings` (array of string, nullable)
- `fileInfo` (ExcelFileInfoDTO)
- `detectedColumns` (array of string, nullable)
- `missingColumns` (array of string, nullable)
- `extraColumns` (array of string, nullable)

#### ExcelValidationErrorDTO
- `rowNumber` (int32)
- `columnName` (string, nullable)
- `invalidValue` (string, nullable)
- `errorMessage` (string, nullable)
- `errorCode` (string, nullable)
- `severity` (ExcelErrorSeverity enum)
- `propertyName` (string, nullable)
- `suggestedFix` (string, nullable)
- `context` (object, nullable)

#### ExcelFileInfoDTO
- `fileSizeBytes` (int64)
- `worksheetCount` (int32)
- `rowCount` (int32)
- `columnCount` (int32)
- `worksheetNames` (array of string, nullable)
- `fileFormat` (string, nullable)

---

## Common Patterns

### Pagination Parameters
Most list endpoints support these parameters:
- `PageNumber` (int32, 1-max): Page number (1-based)
- `PageSize` (int32, 1-100): Items per page
- `SortBy` (string): Field to sort by
- `SortDirection` (string): Sort direction
- `Skip` (int32): Number of items to skip
- `IsDescending` (boolean): Sort in descending order

### Filtering Patterns
Common filter parameters:
- `SearchTerm` (string): Global search across relevant fields
- `IncludeDeleted` (boolean): Include soft-deleted items
- Date ranges: `CreatedAfter`, `CreatedBefore`, `OrderDateFrom`, `OrderDateTo`
- Numeric ranges: `MinPrice`, `MaxPrice`, `MinAmount`, `MaxAmount`

### Excel Import/Export Pattern
Controllers supporting Excel operations typically have:
- **Import**: `POST /import?validateOnly=false` with file upload
- **Export**: `GET /export?format=xlsx` with same filters as list endpoint
- **Template**: `GET /export-template?includeExample=true`
- **Validation**: `POST /validate-import` with file upload
- **Statistics**: `POST /import-statistics` with file upload

### Response Patterns
- **Success**: Returns appropriate DTO or 200 OK
- **Paged Results**: Returns `{EntityName}DTOPagedResult` with items and pagination metadata
- **Lists**: Returns array of DTOs
- **File Operations**: Returns operation result DTOs with success/error counts
- **Bulk Operations**: Returns success status or count

### Security
- All endpoints require Bearer JWT authentication except where noted
- Authorization header format: `Authorization: Bearer {token}`
- Admin-only endpoints require appropriate role claims

### Error Handling
- Standard HTTP status codes
- Validation errors return detailed field-level messages
- Excel import errors include row numbers and specific error details
- File operation errors include per-file success/failure information

---

*This documentation was generated from the DecorStore API OpenAPI 3.0.1 specification.*
