# DecorStore API Testing Guide with Postman

## Overview
This guide contains comprehensive Postman collections for testing the DecorStore API with realistic test data. The collections cover all major endpoints including authentication, categories, products, orders, and more.

## Files Included
- `DecorStore_API_Tests.postman_collection.json` - Main collection with all API endpoints
- `DecorStore_API_Environment.postman_environment.json` - Environment variables for the collection

## Getting Started

### 1. Prerequisites
- Postman application installed
- DecorStore API server running on `http://localhost:5000`
- Database setup and migrations applied

### 2. Import Collections

#### Import Collection:
1. Open Postman
2. Click "Import" button
3. Select `DecorStore_API_Tests.postman_collection.json`
4. Collection will appear in your sidebar

#### Import Environment:
1. Click "Import" button again
2. Select `DecorStore_API_Environment.postman_environment.json`
3. Select "DecorStore API Environment" from environment dropdown

### 3. Running the Tests

#### Option A: Run Full Collection (Recommended for first time)
1. Right-click on "DecorStore API Tests" collection
2. Select "Run collection"
3. Click "Run DecorStore API Tests"
4. This will execute all requests in sequence

#### Option B: Run Individual Folders
Execute folders in this order for best results:
1. 🔐 Authentication
2. 📂 Categories
3. 🛍️ Products
4. 📦 Orders
5. ❤️ Health Check

#### Option C: Run Individual Requests
You can run individual requests, but ensure you run authentication first to get valid tokens.

## Collection Structure

### 🔐 Authentication
- **Register User** - Creates a regular user account
- **Register Admin User** - Creates an admin user account
- **Make User Admin** - Converts regular user to admin
- **Login Admin** - Gets admin authentication token
- **Login User** - Gets user authentication token
- **Get Current User** - Retrieves current user information

### 📂 Categories
- **Create Root Category [Admin]** - Creates "Living Room" category
- **Create Bedroom Category [Admin]** - Creates "Bedroom" category
- **Create Subcategory - Sofas [Admin]** - Creates "Sofas" under Living Room
- **Get Categories (Paginated)** - Retrieves categories with pagination
- **Get All Categories** - Gets all categories without pagination
- **Get Category by ID** - Retrieves specific category
- **Get Category by Slug** - Finds category by slug name
- **Get Hierarchical Categories** - Gets nested category structure

### 🛍️ Products
- **Create Product - Modern Sofa [Admin]** - Creates a sofa product
- **Create Product - Coffee Table [Admin]** - Creates a coffee table
- **Create Product - Bed Frame [Admin]** - Creates a bed frame
- **Get Products (Paginated)** - Retrieves products with pagination
- **Get Product by ID** - Gets specific product details
- **Get Featured Products** - Retrieves featured products only
- **Search Products with Filters** - Demonstrates filtering capabilities
- **Update Product [Admin]** - Updates product information

### 📦 Orders
- **Create Order** - Places a new order with multiple items
- **Get Order by ID** - Retrieves specific order details
- **Update Order Status [Admin]** - Changes order status
- **Get All Orders [Admin]** - Admin view of all orders

### ❤️ Health Check
- **Health Check** - Verifies API server is running

## Sample Test Data

### Users
- **Regular User:**
  - Username: `johndoe`
  - Email: `john.doe@example.com`
  - Password: `Password123!`

- **Admin User:**
  - Username: `admin`
  - Email: `admin@decorstore.com`
  - Password: `AdminPass123!`

### Categories
- **Living Room** (root category)
  - **Sofas** (subcategory)
- **Bedroom** (root category)

### Products
1. **Modern Sofa**
   - Price: $899.99 (was $1200.00)
   - Category: Sofas
   - Stock: 15 units
   - Featured: Yes

2. **Coffee Table**
   - Price: $299.99 (was $399.99)
   - Category: Living Room
   - Stock: 25 units
   - Featured: No

3. **King Size Bed Frame**
   - Price: $1299.99 (was $1599.99)
   - Category: Bedroom
   - Stock: 8 units
   - Featured: Yes

### Orders
- Sample order with multiple items
- Customer information included
- Shipping address provided

## Environment Variables

The environment automatically manages these variables:

### API Configuration
- `baseUrl` - API server URL (http://localhost:5000)

### Authentication
- `authToken` - User authentication token
- `adminToken` - Admin authentication token
- `userId` - Regular user ID
- `adminUserId` - Admin user ID
- `userEmail` - Regular user email
- `adminEmail` - Admin user email

### Entity IDs (Auto-populated)
- `rootCategoryId` - Living Room category ID
- `bedroomCategoryId` - Bedroom category ID
- `subcategoryId` - Sofas category ID
- `productId` - Modern Sofa product ID
- `productId2` - Coffee Table product ID
- `productId3` - Bed Frame product ID
- `orderId` - Sample order ID
- `imageId` - Image ID (when testing image endpoints)

## Key Features

### ✅ Automated Testing
- Status code validation
- Response structure verification
- Data validation
- Token extraction and storage
- ID chaining between requests

### 🔄 Request Chaining
- IDs from creation responses automatically used in subsequent requests
- Authentication tokens automatically applied to protected endpoints
- Dependencies properly managed

### 🛡️ Permission Testing
- Tests both user and admin access levels
- Validates authorization requirements
- Proper error handling for unauthorized access

### 📊 Comprehensive Coverage
- CRUD operations for all entities
- Pagination testing
- Search and filtering
- Relationship testing (categories → products → orders)

## Testing Scenarios

### Basic Workflow Test
1. Run Authentication folder to setup users
2. Run Categories folder to create category structure
3. Run Products folder to create sample products
4. Run Orders folder to test order creation and management

### Permission Testing
- Try running admin-only requests with user token
- Verify proper 403 Forbidden responses
- Test unauthorized access without tokens

### Data Validation
- All responses include proper data structure validation
- Required fields verification
- Data type checking

### Error Handling
- Invalid ID testing (404 responses)
- Malformed request testing
- Authentication failure scenarios

## Troubleshooting

### Common Issues

#### 1. Server Not Running
- **Error:** Connection refused
- **Solution:** Ensure API server is running on localhost:5000

#### 2. Database Errors
- **Error:** Database connection issues
- **Solution:** Run database migrations: `dotnet ef database update`

#### 3. Authentication Failures
- **Error:** 401 Unauthorized
- **Solution:** Run authentication requests first to get valid tokens

#### 4. Missing Dependencies
- **Error:** 404 Not Found on related entities
- **Solution:** Run requests in proper order (categories before products, etc.)

### Request Order Dependencies

```
Authentication (Users/Tokens)
    ↓
Categories (Root categories)
    ↓
Categories (Subcategories)
    ↓
Products (Using category IDs)
    ↓
Orders (Using product IDs)
```

## Advanced Usage

### Custom Test Data
Modify the request bodies to test with your own data:
- Change user credentials
- Update product information
- Modify category structures

### Environment Switching
Create multiple environments for:
- Development (localhost:5000)
- Staging (your-staging-url)
- Production (your-production-url)

### Automated Testing
Use Postman's Collection Runner for:
- Continuous integration testing
- Performance testing
- Data setup for development

## API Documentation Reference

For detailed API documentation, refer to:
- Controllers in the `/Controllers` directory
- DTOs in the `/DTOs` directory
- Swagger documentation (if available) at `http://localhost:5000/swagger`

## Support

If you encounter issues:
1. Check that the API server is running
2. Verify database connection
3. Ensure proper request execution order
4. Check Postman console for detailed error messages

Happy testing! 🚀
