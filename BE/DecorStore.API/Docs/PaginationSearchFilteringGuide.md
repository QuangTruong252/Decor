# Comprehensive Pagination, Search, and Filtering Guide

This guide demonstrates how to use the comprehensive pagination, search, and filtering functionality implemented in the DecorStore API for Product, Order, Category, and Customer entities.

## Overview

The API now supports:
- **Pagination**: Page-based pagination with configurable page size and navigation metadata
- **Search**: Text-based search across relevant fields for each entity
- **Filtering**: Dynamic filtering based on entity-specific properties
- **Sorting**: Flexible sorting by various fields in ascending or descending order

## Core Pagination Parameters

All paginated endpoints support these base parameters:

```json
{
  "pageNumber": 1,        // Page number (1-based, default: 1)
  "pageSize": 10,         // Items per page (1-100, default: 10)
  "sortBy": "createdAt",  // Field to sort by
  "sortDirection": "desc" // Sort direction: "asc" or "desc"
}
```

## Response Format

All paginated responses follow this standardized format:

```json
{
  "items": [...],           // Array of items for current page
  "pagination": {
    "currentPage": 1,       // Current page number
    "pageSize": 10,         // Items per page
    "totalCount": 150,      // Total number of items
    "totalPages": 15,       // Total number of pages
    "hasNext": true,        // Whether there's a next page
    "hasPrevious": false,   // Whether there's a previous page
    "nextPage": 2,          // Next page number (null if no next page)
    "previousPage": null    // Previous page number (null if no previous page)
  }
}
```

## Product Endpoints

### 1. Get Paginated Products
**GET** `/api/Products`

**Query Parameters:**
```
pageNumber=1
pageSize=20
sortBy=name
sortDirection=asc
searchTerm=lamp
categoryId=1
minPrice=50.00
maxPrice=500.00
isFeatured=true
isActive=true
createdAfter=2024-01-01
createdBefore=2024-12-31
stockQuantityMin=5
stockQuantityMax=100
minRating=4.0
sku=LAM001
```

**Example Request:**
```
GET /api/Products?pageNumber=1&pageSize=20&searchTerm=lamp&minPrice=50&maxPrice=200&sortBy=price&sortDirection=asc
```

**Example Response:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Modern Table Lamp",
      "price": 89.99,
      "categoryName": "Lamps",
      "stockQuantity": 25,
      "averageRating": 4.5,
      "isFeatured": true,
      "isActive": true,
      "createdAt": "2024-03-15T10:30:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 45,
    "totalPages": 3,
    "hasNext": true,
    "hasPrevious": false,
    "nextPage": 2,
    "previousPage": null
  }
}
```

### 2. Advanced Product Queries

**Featured Products:**
```
GET /api/Products/featured?count=10
```

**Products by Category:**
```
GET /api/Products/category/1?count=20
```

**Related Products:**
```
GET /api/Products/123/related?count=5
```

**Top Rated Products:**
```
GET /api/Products/top-rated?count=10
```

**Low Stock Products (Admin only):**
```
GET /api/Products/low-stock?threshold=10
```

## Order Endpoints

### Get Paginated Orders
**GET** `/api/Orders` (Implementation needed)

**Query Parameters:**
```
pageNumber=1
pageSize=20
sortBy=orderDate
sortDirection=desc
searchTerm=john
userId=123
customerId=456
orderStatus=pending
paymentMethod=credit_card
minAmount=100.00
maxAmount=1000.00
orderDateFrom=2024-01-01
orderDateTo=2024-12-31
shippingCity=New York
shippingState=NY
shippingCountry=USA
includeDeleted=false
```

**Example Request:**
```
GET /api/Orders?pageNumber=1&pageSize=10&orderStatus=pending&minAmount=100&sortBy=orderDate&sortDirection=desc
```

## Category Endpoints

### Get Paginated Categories
**GET** `/api/Categories` (Implementation needed)

**Query Parameters:**
```
pageNumber=1
pageSize=20
sortBy=name
sortDirection=asc
searchTerm=furniture
parentId=1
isRootCategory=true
includeSubcategories=true
includeProductCount=true
createdAfter=2024-01-01
createdBefore=2024-12-31
includeDeleted=false
```

**Example Request:**
```
GET /api/Categories?pageNumber=1&pageSize=10&isRootCategory=true&includeProductCount=true&sortBy=name
```

## Customer Endpoints

### Get Paginated Customers
**GET** `/api/Customers` (Implementation needed)

**Query Parameters:**
```
pageNumber=1
pageSize=20
sortBy=lastName
sortDirection=asc
searchTerm=john
email=john@example.com
city=New York
state=NY
country=USA
postalCode=10001
registeredAfter=2024-01-01
registeredBefore=2024-12-31
hasOrders=true
includeOrderCount=true
includeTotalSpent=true
includeDeleted=false
```

**Example Request:**
```
GET /api/Customers?pageNumber=1&pageSize=15&city=New York&hasOrders=true&includeOrderCount=true&sortBy=lastName
```

## Sorting Options

### Product Sorting Fields:
- `name` - Product name
- `price` - Product price
- `createdAt` - Creation date
- `updatedAt` - Last update date
- `stockQuantity` - Stock quantity
- `averageRating` - Average rating
- `category` - Category name

### Order Sorting Fields:
- `orderDate` - Order date
- `totalAmount` - Total amount
- `orderStatus` - Order status
- `paymentMethod` - Payment method
- `customer` - Customer name
- `updatedAt` - Last update date

### Category Sorting Fields:
- `name` - Category name
- `createdAt` - Creation date
- `productCount` - Number of products (when included)

### Customer Sorting Fields:
- `firstName` - First name
- `lastName` - Last name
- `email` - Email address
- `createdAt` - Registration date
- `orderCount` - Number of orders (when included)
- `totalSpent` - Total amount spent (when included)

## Performance Considerations

1. **Indexing**: Ensure database indexes on frequently filtered/sorted fields
2. **Page Size Limits**: Maximum page size is limited to 100 items
3. **Search Performance**: Text searches use case-insensitive LIKE queries
4. **Eager Loading**: Related entities are loaded efficiently using Include()
5. **Query Optimization**: Filters are applied before pagination to minimize data transfer

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK` - Successful request
- `400 Bad Request` - Invalid pagination parameters
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

Invalid pagination parameters will return validation errors:
```json
{
  "errors": {
    "PageNumber": ["Page number must be greater than 0"],
    "PageSize": ["Page size must be between 1 and 100"]
  }
}
```
