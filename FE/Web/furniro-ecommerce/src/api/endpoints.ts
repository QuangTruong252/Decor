// API Endpoints Constants
export const API_ENDPOINTS = {
  // Authentication
  AUTH: {
    REGISTER: '/Auth/register',
    LOGIN: '/Auth/login',
    USER: '/Auth/user',
    CHECK_CLAIMS: '/Auth/check-claims',
    MAKE_ADMIN: '/Auth/make-admin',
  },

  // Products
  PRODUCTS: {
    BASE: '/Products',
    ALL: '/Products/all',
    BY_ID: (id: number) => `/Products/${id}`,
    BY_CATEGORY: (categoryId: number) => `/Products/category/${categoryId}`,
    FEATURED: '/Products/featured',
    TOP_RATED: '/Products/top-rated',
    LOW_STOCK: '/Products/low-stock',
    RELATED: (id: number) => `/Products/${id}/related`,
    BULK_DELETE: '/Products/bulk',
    IMAGES: (id: number) => `/Products/${id}/images`,
    DELETE_IMAGE: (productId: number, imageId: number) => `/Products/${productId}/images/${imageId}`,
  },

  // Categories
  CATEGORIES: {
    BASE: '/Category',
    ALL: '/Category/all',
    HIERARCHICAL: '/Category/hierarchical',
    BY_ID: (id: number) => `/Category/${id}`,
    BY_SLUG: (slug: string) => `/Category/slug/${slug}`,
    WITH_PRODUCT_COUNT: '/Category/with-product-count',
    SUBCATEGORIES: (parentId: number) => `/Category/${parentId}/subcategories`,
    PRODUCT_COUNT: (categoryId: number) => `/Category/${categoryId}/product-count`,
    POPULAR: '/Category/popular',
    ROOT: '/Category/root',
  },

  // Orders
  ORDERS: {
    BASE: '/Order',
    ALL: '/Order/all',
    BY_ID: (id: number) => `/Order/${id}`,
    BY_USER: (userId: number) => `/Order/user/${userId}`,
    UPDATE_STATUS: (id: number) => `/Order/${id}/status`,
    BULK_DELETE: '/Order/bulk',
    RECENT: '/Order/recent',
    BY_STATUS: (status: string) => `/Order/status/${status}`,
    DATE_RANGE: '/Order/date-range',
    REVENUE: '/Order/revenue',
    STATUS_COUNTS: '/Order/status-counts',
  },

  // Cart
  CART: {
    BASE: '/Cart',
    ITEMS: (id: number) => `/Cart/items/${id}`,
    MERGE: '/Cart/merge',
  },

  // Customer
  CUSTOMER: {
    BASE: '/Customer',
    ALL: '/Customer/all',
    BY_ID: (id: number) => `/Customer/${id}`,
    BY_EMAIL: (email: string) => `/Customer/email/${email}`,
    WITH_ORDERS: '/Customer/with-orders',
    TOP_BY_ORDER_COUNT: '/Customer/top-by-order-count',
    TOP_BY_SPENDING: '/Customer/top-by-spending',
    BY_LOCATION: '/Customer/by-location',
    ORDER_COUNT: (customerId: number) => `/Customer/${customerId}/order-count`,
    TOTAL_SPENT: (customerId: number) => `/Customer/${customerId}/total-spent`,
  },

  // Reviews
  REVIEWS: {
    BASE: '/Review',
    BY_ID: (id: number) => `/Review/${id}`,
    BY_PRODUCT: (productId: number) => `/Review/product/${productId}`,
    RATING_BY_PRODUCT: (productId: number) => `/Review/product/${productId}/rating`,
  },

  // Banner
  BANNER: {
    BASE: '/Banner',
    ACTIVE: '/Banner/active',
    BY_ID: (id: number) => `/Banner/${id}`,
  },

  // Dashboard
  DASHBOARD: {
    SUMMARY: '/Dashboard/summary',
    SALES_TREND: '/Dashboard/sales-trend',
    POPULAR_PRODUCTS: '/Dashboard/popular-products',
    SALES_BY_CATEGORY: '/Dashboard/sales-by-category',
    ORDER_STATUS_DISTRIBUTION: '/Dashboard/order-status-distribution',
  },

  // Health Check
  HEALTH_CHECK: '/HealthCheck',
} as const;

// Export individual endpoint groups for convenience
export const {
  AUTH,
  PRODUCTS,
  CATEGORIES,
  ORDERS,
  CART,
  CUSTOMER,
  REVIEWS,
  BANNER,
  DASHBOARD,
  HEALTH_CHECK,
} = API_ENDPOINTS;
