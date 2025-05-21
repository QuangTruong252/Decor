# Dashboard API Specification

This document outlines the API endpoints required to support the dashboard functionality in the DecorStore Admin Dashboard.

## Overview

The dashboard requires several API endpoints to provide metrics and visualizations for business data. These endpoints will aggregate data from existing entities (products, orders, customers) to provide insights for the admin dashboard.

## Required Endpoints

### 1. Dashboard Summary

**Endpoint:** `GET /api/Dashboard/summary`

**Description:** Provides a comprehensive summary of key metrics and data for the dashboard.

**Response:**
```json
{
  "totalProducts": 150,
  "totalOrders": 75,
  "totalCustomers": 50,
  "totalRevenue": 12500.50,
  "recentOrders": [
    {
      "id": 1,
      "userId": 5,
      "userFullName": "John Doe",
      "totalAmount": 250.00,
      "orderStatus": "Delivered",
      "paymentMethod": "Credit Card",
      "shippingAddress": "123 Main St",
      "orderDate": "2023-05-15T14:30:00Z",
      "updatedAt": "2023-05-16T10:15:00Z"
    },
    // Additional recent orders...
  ],
  "popularProducts": [
    {
      "id": 12,
      "name": "Modern Coffee Table",
      "totalSales": 25,
      "revenue": 3750.00,
      "imageUrl": "https://example.com/images/coffee-table.jpg"
    },
    // Additional popular products...
  ],
  "salesByCategory": [
    {
      "categoryId": 3,
      "categoryName": "Furniture",
      "totalSales": 120,
      "revenue": 18000.00
    },
    // Additional category sales data...
  ],
  "orderStatusDistribution": [
    {
      "status": "Pending",
      "count": 15
    },
    {
      "status": "Processing",
      "count": 10
    },
    {
      "status": "Shipped",
      "count": 25
    },
    {
      "status": "Delivered",
      "count": 45
    },
    {
      "status": "Cancelled",
      "count": 5
    }
  ],
  "salesTrend": [
    {
      "date": "2023-05-01",
      "revenue": 450.00,
      "orderCount": 3
    },
    // Additional sales trend data points...
  ]
}
```

### 2. Sales Trend

**Endpoint:** `GET /api/Dashboard/sales-trend`

**Description:** Provides sales trend data for a specific period.

**Query Parameters:**
- `period` (string, required): The period for which to retrieve sales trend data. Possible values: "daily", "weekly", "monthly".
- `startDate` (string, optional): The start date for a custom period (format: YYYY-MM-DD).
- `endDate` (string, optional): The end date for a custom period (format: YYYY-MM-DD).

**Response:**
```json
[
  {
    "date": "2023-05-01",
    "revenue": 450.00,
    "orderCount": 3
  },
  {
    "date": "2023-05-02",
    "revenue": 750.00,
    "orderCount": 5
  },
  // Additional data points...
]
```

### 3. Popular Products

**Endpoint:** `GET /api/Dashboard/popular-products`

**Description:** Provides a list of popular products based on sales.

**Query Parameters:**
- `limit` (integer, optional): The number of products to return. Default: 5.

**Response:**
```json
[
  {
    "id": 12,
    "name": "Modern Coffee Table",
    "totalSales": 25,
    "revenue": 3750.00,
    "imageUrl": "https://example.com/images/coffee-table.jpg"
  },
  {
    "id": 8,
    "name": "Decorative Pillow Set",
    "totalSales": 18,
    "revenue": 540.00,
    "imageUrl": "https://example.com/images/pillow-set.jpg"
  },
  // Additional products...
]
```

### 4. Sales by Category

**Endpoint:** `GET /api/Dashboard/sales-by-category`

**Description:** Provides sales data grouped by product category.

**Response:**
```json
[
  {
    "categoryId": 3,
    "categoryName": "Furniture",
    "totalSales": 120,
    "revenue": 18000.00
  },
  {
    "categoryId": 5,
    "categoryName": "Decor",
    "totalSales": 85,
    "revenue": 2550.00
  },
  // Additional categories...
]
```

### 5. Order Status Distribution

**Endpoint:** `GET /api/Dashboard/order-status-distribution`

**Description:** Provides the distribution of orders by status.

**Response:**
```json
[
  {
    "status": "Pending",
    "count": 15
  },
  {
    "status": "Processing",
    "count": 10
  },
  {
    "status": "Shipped",
    "count": 25
  },
  {
    "status": "Delivered",
    "count": 45
  },
  {
    "status": "Cancelled",
    "count": 5
  }
]
```

## Implementation Notes

1. **Data Aggregation**: These endpoints require aggregating data from multiple existing entities (orders, products, customers).

2. **Performance Considerations**: 
   - Consider caching frequently accessed dashboard data
   - Implement pagination for large datasets
   - Use database aggregation functions for efficient calculations

3. **Security**: 
   - These endpoints should be protected and only accessible to authenticated admin users
   - Implement proper authorization checks

4. **Error Handling**:
   - Return appropriate HTTP status codes for different error scenarios
   - Provide meaningful error messages

5. **Date Handling**:
   - All dates should be in ISO 8601 format (YYYY-MM-DDTHH:MM:SSZ)
   - Support timezone handling for accurate reporting

## Integration with Frontend

The frontend dashboard components are designed to work with these API endpoints. The data structures returned by these endpoints should match the expected formats to ensure proper rendering of charts and metrics.

If any changes are made to the API response structures, corresponding updates will be needed in the frontend components.
