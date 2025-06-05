# Phase 2: Core Services & Controllers Refactoring - Task Tracking

**Duration**: Days 4-6  
**Goal**: Refactor all core services and controllers to use Result<T> pattern and BaseController

## Progress Overview
- **Total Services**: 10 services + 10 controllers
- **Completed**: 0
- **In Progress**: 0
- **Remaining**: 20

---

## Task 1: Complete Foundation (Finish Phase 1)

### 1.1 Complete Remaining Repositories
- [ ] Refactor CategoryRepository to inherit from BaseRepository
- [ ] Refactor OrderRepository to inherit from BaseRepository
- [ ] Update ICategoryRepository to extend IRepository<T>
- [ ] Update IOrderRepository to extend IRepository<T>
- [ ] Test all repository operations

### 1.2 Validate Phase 1 Implementation
- [ ] Verify ProductRepository operations work correctly
- [ ] Test ProductService Result<T> patterns
- [ ] Validate ProductController BaseController usage
- [ ] Check pagination functionality
- [ ] Confirm soft delete behavior

---

## Task 2: Core Business Services Refactoring (Priority Order)

### 2.1 CategoryService + CategoryController (Day 4)
**Priority**: 🔥 CRITICAL (Product dependency)

#### CategoryService Refactoring
- [ ] Update ICategoryService interface to return Result<T>
- [ ] Refactor CategoryService.GetAllAsync() → Result<IEnumerable<CategoryDTO>>
- [ ] Refactor CategoryService.GetByIdAsync() → Result<CategoryDTO>
- [ ] Refactor CategoryService.CreateAsync() → Result<CategoryDTO>
- [ ] Refactor CategoryService.UpdateAsync() → Result
- [ ] Refactor CategoryService.DeleteAsync() → Result
- [ ] Add input validation (null checks, ID validation)
- [ ] Implement business rule validation
- [ ] Add error codes (INVALID_INPUT, NOT_FOUND, DUPLICATE_NAME)
- [ ] Remove direct exception throwing

#### CategoryController Refactoring
- [ ] Inherit from BaseController
- [ ] Add ILogger<CategoryController> injection
- [ ] Update GetCategories() to use HandleResult()
- [ ] Update GetCategory(id) to use HandleResult()
- [ ] Update CreateCategory() to use HandleCreateResult()
- [ ] Update UpdateCategory() to use HandleResult()
- [ ] Update DeleteCategory() to use HandleResult()
- [ ] Add model state validation using ValidateModelState()
- [ ] Remove manual try-catch blocks

#### Quality Gates for Category
- [ ] **Code Quality**: Single responsibility, max 20 lines per method
- [ ] **Performance**: All database calls async, AsNoTracking for reads
- [ ] **Error Handling**: Consistent Result<T> responses
- [ ] **Naming**: PascalCase classes, camelCase variables
- [ ] **No Code Duplication**: Extract common patterns

---

### 2.2 OrderService + OrderController (Day 4-5)
**Priority**: 🔥 CRITICAL (Business critical)

#### OrderService Refactoring
- [ ] Update IOrderService interface to return Result<T>
- [ ] Refactor GetOrdersAsync() → Result<PagedResult<OrderDTO>>
- [ ] Refactor GetOrderByIdAsync() → Result<OrderDTO>
- [ ] Refactor CreateOrderAsync() → Result<OrderDTO>
- [ ] Refactor UpdateOrderStatusAsync() → Result
- [ ] Refactor CancelOrderAsync() → Result
- [ ] Refactor GetOrdersByCustomerAsync() → Result<IEnumerable<OrderDTO>>
- [ ] Add order status validation
- [ ] Add inventory check validation
- [ ] Add business rule validation (order total, customer exists)
- [ ] Implement transaction management for order creation
- [ ] Add error codes (INSUFFICIENT_INVENTORY, INVALID_STATUS, etc.)

#### OrderController Refactoring
- [ ] Inherit from BaseController
- [ ] Add ILogger<OrderController> injection
- [ ] Update GetOrders() to use HandlePagedResult()
- [ ] Update GetOrder(id) to use HandleResult()
- [ ] Update CreateOrder() to use HandleCreateResult()
- [ ] Update UpdateOrderStatus() to use HandleResult()
- [ ] Update CancelOrder() to use HandleResult()
- [ ] Update GetCustomerOrders() to use HandleResult()
- [ ] Add authorization checks
- [ ] Remove manual error handling

#### Quality Gates for Order
- [ ] **Code Quality**: Complex business logic properly separated
- [ ] **Performance**: Efficient order queries with includes
- [ ] **Error Handling**: Transaction rollback on failures
- [ ] **Business Rules**: Order validation, status transitions
- [ ] **Security**: Customer can only access own orders

---

### 2.3 CartService + CartController (Day 5)
**Priority**: 🔥 CRITICAL (User experience critical)

#### CartService Refactoring
- [ ] Update ICartService interface to return Result<T>
- [ ] Refactor GetCartAsync() → Result<CartDTO>
- [ ] Refactor AddToCartAsync() → Result<CartDTO>
- [ ] Refactor UpdateCartItemAsync() → Result<CartDTO>
- [ ] Refactor RemoveFromCartAsync() → Result<CartDTO>
- [ ] Refactor ClearCartAsync() → Result
- [ ] Add product existence validation
- [ ] Add inventory availability check
- [ ] Add quantity validation (positive numbers)
- [ ] Implement cart session management
- [ ] Add error codes (PRODUCT_NOT_FOUND, INSUFFICIENT_STOCK, etc.)

#### CartController Refactoring
- [ ] Inherit from BaseController
- [ ] Add ILogger<CartController> injection
- [ ] Update GetCart() to use HandleResult()
- [ ] Update AddToCart() to use HandleResult()
- [ ] Update UpdateCartItem() to use HandleResult()
- [ ] Update RemoveFromCart() to use HandleResult()
- [ ] Update ClearCart() to use HandleResult()
- [ ] Add user context validation
- [ ] Remove manual error handling

#### Quality Gates for Cart
- [ ] **Code Quality**: Clear separation of cart operations
- [ ] **Performance**: Efficient cart data loading
- [ ] **Error Handling**: Graceful handling of product changes
- [ ] **User Experience**: Real-time inventory validation
- [ ] **Session Management**: Proper cart persistence

---

### 2.4 CustomerService + CustomerController (Day 5-6)
**Priority**: 🔥 CRITICAL (Authentication critical)

#### CustomerService Refactoring
- [ ] Update ICustomerService interface to return Result<T>
- [ ] Refactor GetCustomersAsync() → Result<PagedResult<CustomerDTO>>
- [ ] Refactor GetCustomerByIdAsync() → Result<CustomerDTO>
- [ ] Refactor CreateCustomerAsync() → Result<CustomerDTO>
- [ ] Refactor UpdateCustomerAsync() → Result<CustomerDTO>
- [ ] Refactor DeleteCustomerAsync() → Result
- [ ] Refactor GetCustomerOrdersAsync() → Result<IEnumerable<OrderDTO>>
- [ ] Add email uniqueness validation
- [ ] Add phone number format validation
- [ ] Add password strength validation
- [ ] Implement soft delete for customers
- [ ] Add error codes (DUPLICATE_EMAIL, INVALID_PHONE, etc.)

#### CustomerController Refactoring
- [ ] Inherit from BaseController
- [ ] Add ILogger<CustomerController> injection
- [ ] Update GetCustomers() to use HandlePagedResult()
- [ ] Update GetCustomer(id) to use HandleResult()
- [ ] Update CreateCustomer() to use HandleCreateResult()
- [ ] Update UpdateCustomer() to use HandleResult()
- [ ] Update DeleteCustomer() to use HandleResult()
- [ ] Update GetCustomerOrders() to use HandleResult()
- [ ] Add admin authorization for customer management
- [ ] Add customer self-access validation

#### Quality Gates for Customer
- [ ] **Code Quality**: Secure customer data handling
- [ ] **Performance**: Efficient customer queries
- [ ] **Error Handling**: Privacy-conscious error messages
- [ ] **Security**: Proper authorization checks
- [ ] **Data Protection**: Sensitive data handling

---

### 2.5 ImageService + ImageController (Day 6)
**Priority**: ⚡ HIGH (Product dependency)

#### ImageService Refactoring
- [ ] Update IImageService interface to return Result<T>
- [ ] Refactor UploadImageAsync() → Result<ImageDTO>
- [ ] Refactor GetImageAsync() → Result<ImageDTO>
- [ ] Refactor DeleteImageAsync() → Result
- [ ] Refactor GetImagesByIdsAsync() → Result<IEnumerable<ImageDTO>>
- [ ] Add file type validation (jpg, png, gif)
- [ ] Add file size validation (max 5MB)
- [ ] Add image dimension validation
- [ ] Implement secure file storage
- [ ] Add error codes (INVALID_FILE_TYPE, FILE_TOO_LARGE, etc.)

#### ImageController Refactoring
- [ ] Inherit from BaseController
- [ ] Add ILogger<ImageController> injection
- [ ] Update UploadImage() to use HandleCreateResult()
- [ ] Update GetImage(id) to use HandleResult()
- [ ] Update DeleteImage(id) to use HandleResult()
- [ ] Add file upload validation
- [ ] Add authorization checks
- [ ] Remove manual error handling

#### Quality Gates for Image
- [ ] **Code Quality**: Clean file handling logic
- [ ] **Performance**: Efficient image processing
- [ ] **Error Handling**: Proper file operation errors
- [ ] **Security**: Secure file upload validation
- [ ] **Storage**: Optimized file storage management

---

## Task 3: Supporting Services Refactoring

### 3.1 BannerService + BannerController
#### Service Refactoring
- [ ] Update IBannerService interface to return Result<T>
- [ ] Refactor GetActiveBannersAsync() → Result<IEnumerable<BannerDTO>>
- [ ] Refactor GetBannerByIdAsync() → Result<BannerDTO>
- [ ] Refactor CreateBannerAsync() → Result<BannerDTO>
- [ ] Refactor UpdateBannerAsync() → Result<BannerDTO>
- [ ] Refactor DeleteBannerAsync() → Result
- [ ] Add banner validation (dates, URLs, images)
- [ ] Add error codes (INVALID_DATE_RANGE, INVALID_URL, etc.)

#### Controller Refactoring
- [ ] Inherit from BaseController and implement Result<T> pattern
- [ ] Update all action methods to use HandleResult()
- [ ] Add admin authorization
- [ ] Remove manual error handling

### 3.2 ReviewService + ReviewController
#### Service Refactoring
- [ ] Update IReviewService interface to return Result<T>
- [ ] Refactor GetReviewsAsync() → Result<PagedResult<ReviewDTO>>
- [ ] Refactor GetReviewByIdAsync() → Result<ReviewDTO>
- [ ] Refactor CreateReviewAsync() → Result<ReviewDTO>
- [ ] Refactor UpdateReviewAsync() → Result<ReviewDTO>
- [ ] Refactor DeleteReviewAsync() → Result
- [ ] Add review validation (rating 1-5, customer purchased product)
- [ ] Add duplicate review prevention
- [ ] Add error codes (DUPLICATE_REVIEW, INVALID_RATING, etc.)

#### Controller Refactoring
- [ ] Inherit from BaseController and implement Result<T> pattern
- [ ] Update all action methods to use HandleResult()
- [ ] Add customer authentication checks
- [ ] Remove manual error handling

### 3.3 DashboardService + DashboardController
#### Service Refactoring
- [ ] Update IDashboardService interface to return Result<T>
- [ ] Refactor GetDashboardDataAsync() → Result<DashboardDTO>
- [ ] Refactor GetSalesStatisticsAsync() → Result<SalesStatisticsDTO>
- [ ] Refactor GetTopProductsAsync() → Result<IEnumerable<ProductDTO>>
- [ ] Add performance optimization for dashboard queries
- [ ] Add caching for dashboard data
- [ ] Add error codes for dashboard operations

#### Controller Refactoring
- [ ] Inherit from BaseController and implement Result<T> pattern
- [ ] Update all action methods to use HandleResult()
- [ ] Add admin authorization
- [ ] Add response caching

### 3.4 FileManagerService + FileManagerController
#### Service Refactoring
- [ ] Update IFileManagerService interface to return Result<T>
- [ ] Refactor BrowseFilesAsync() → Result<IEnumerable<FileItemDTO>>
- [ ] Refactor CreateFolderAsync() → Result<FolderDTO>
- [ ] Refactor DeleteFileAsync() → Result
- [ ] Refactor MoveFileAsync() → Result<FileItemDTO>
- [ ] Add file system security validation
- [ ] Add path traversal protection
- [ ] Add error codes (INVALID_PATH, ACCESS_DENIED, etc.)

#### Controller Refactoring
- [ ] Inherit from BaseController and implement Result<T> pattern
- [ ] Update all action methods to use HandleResult()
- [ ] Add admin authorization
- [ ] Add file operation validation

### 3.5 AuthService + AuthController
#### Service Refactoring
- [ ] Update IAuthService interface to return Result<T>
- [ ] Refactor LoginAsync() → Result<LoginResultDTO>
- [ ] Refactor RegisterAsync() → Result<RegisterResultDTO>
- [ ] Refactor RefreshTokenAsync() → Result<TokenDTO>
- [ ] Refactor ChangePasswordAsync() → Result
- [ ] Add password validation
- [ ] Add account lockout logic
- [ ] Add error codes (INVALID_CREDENTIALS, ACCOUNT_LOCKED, etc.)

#### Controller Refactoring
- [ ] Inherit from BaseController and implement Result<T> pattern
- [ ] Update all action methods to use HandleResult()
- [ ] Add rate limiting for login attempts
- [ ] Remove manual error handling

---

## Quality Gates (All Services & Controllers)

### ✅ Code Quality Requirements
- [ ] **Single Responsibility**: Each method max 20 lines, single purpose
- [ ] **Naming Conventions**: PascalCase classes, camelCase variables, descriptive names
- [ ] **Code Duplication**: Max 3 lines duplicate code allowed, extract to methods/classes
- [ ] **SOLID Principles**: Dependency injection, interface segregation
- [ ] **Clean Code**: Meaningful variable names, clear method signatures

### ✅ Performance Requirements
- [ ] **Async/Await**: All database calls must be async, no .Result or .Wait()
- [ ] **Database Queries**: Include related data efficiently, use AsNoTracking for read-only
- [ ] **Caching**: Response caching for GET operations, memory caching for reference data
- [ ] **Query Optimization**: Proper indexing, avoid N+1 queries
- [ ] **Resource Management**: Proper disposal of resources

### ✅ Error Handling Requirements
- [ ] **Global Exception Handler**: Catch all unhandled exceptions
- [ ] **Structured Logging**: Use ILogger with correlation IDs
- [ ] **Error Messages**: User-friendly messages, detailed logs for debugging
- [ ] **Result<T> Pattern**: Consistent error responses across all endpoints
- [ ] **Validation**: Input validation with meaningful error messages

---

## Completion Criteria

### Phase 2 Success Metrics:
- [ ] All 10 services implement Result<T> pattern
- [ ] All 10 controllers inherit from BaseController
- [ ] 100% consistent error handling across all endpoints
- [ ] No manual try-catch blocks in controllers
- [ ] All database operations are async
- [ ] Proper input validation for all endpoints
- [ ] Business rule validation implemented
- [ ] No code compilation errors
- [ ] All existing API functionality preserved

### Dependencies for Phase 3:
- [ ] All services return Result<T>
- [ ] All controllers use BaseController
- [ ] Error handling patterns established
- [ ] Ready for configuration pattern implementation

---

## Notes & Issues

### Implementation Strategy:
- Start with CategoryService (Product dependency)
- Test each service thoroughly before moving to next
- Maintain backward compatibility
- Document any breaking changes

### Risk Mitigation:
- One service at a time to minimize risk
- Comprehensive testing after each service
- Keep rollback options available
- Monitor performance during refactoring

---

**Last Updated**: 2025-06-05  
**Phase Status**: 🚧 Ready to Start  
**Next Task**: Complete remaining repositories from Phase 1
