import { env } from './env';

/**
 * API Configuration
 */
export const apiConfig = {
  // Base URL for API requests
  baseURL: env.NEXT_PUBLIC_API_BASE_URL,
  
  // Request timeout in milliseconds
  timeout: 10000,
  
  // Default headers
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
  
  // Retry configuration
  retry: {
    attempts: 3,
    delay: 1000,
    backoff: 2,
  },
  
  // Cache configuration
  cache: {
    enabled: true,
    ttl: 5 * 60 * 1000, // 5 minutes
  },
  
  // Rate limiting
  rateLimit: {
    enabled: true,
    maxRequests: 100,
    windowMs: 60 * 1000, // 1 minute
  },
} as const;

/**
 * API Endpoints Configuration
 */
export const endpointConfig = {
  // Authentication endpoints
  auth: {
    login: '/Auth/login',
    register: '/Auth/register',
    user: '/Auth/user',
    checkClaims: '/Auth/check-claims',
    makeAdmin: '/Auth/make-admin',
  },
  
  // Product endpoints
  products: {
    base: '/Products',
    byId: (id: number) => `/Products/${id}`,
    byCategory: (categoryId: number) => `/Products/category/${categoryId}`,
    bulkDelete: '/Products/bulk',
    images: (id: number) => `/Products/${id}/images`,
    deleteImage: (productId: number, imageId: number) => `/Products/${productId}/images/${imageId}`,
  },
  
  // Category endpoints
  categories: {
    base: '/Category',
    hierarchical: '/Category/hierarchical',
    byId: (id: number) => `/Category/${id}`,
    bySlug: (slug: string) => `/Category/slug/${slug}`,
  },
  
  // Order endpoints
  orders: {
    base: '/Order',
    byId: (id: number) => `/Order/${id}`,
    byUser: (userId: number) => `/Order/user/${userId}`,
    updateStatus: (id: number) => `/Order/${id}/status`,
    bulkDelete: '/Order/bulk',
  },
  
  // Cart endpoints
  cart: {
    base: '/Cart',
    items: (id: number) => `/Cart/items/${id}`,
    merge: '/Cart/merge',
  },
  
  // Customer endpoints
  customer: {
    base: '/Customer',
    byId: (id: number) => `/Customer/${id}`,
    byEmail: (email: string) => `/Customer/email/${email}`,
  },
  
  // Review endpoints
  reviews: {
    base: '/Review',
    byId: (id: number) => `/Review/${id}`,
    byProduct: (productId: number) => `/Review/product/${productId}`,
    ratingByProduct: (productId: number) => `/Review/product/${productId}/rating`,
  },
  
  // Banner endpoints
  banner: {
    base: '/Banner',
    active: '/Banner/active',
    byId: (id: number) => `/Banner/${id}`,
  },
  
  // Dashboard endpoints
  dashboard: {
    summary: '/Dashboard/summary',
    salesTrend: '/Dashboard/sales-trend',
    popularProducts: '/Dashboard/popular-products',
    salesByCategory: '/Dashboard/sales-by-category',
    orderStatusDistribution: '/Dashboard/order-status-distribution',
  },
  
  // Health check
  healthCheck: '/HealthCheck',
} as const;

/**
 * Request configuration for different types of requests
 */
export const requestConfig = {
  // Standard JSON request
  json: {
    headers: {
      'Content-Type': 'application/json',
    },
  },
  
  // Multipart form data request (for file uploads)
  multipart: {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  },
  
  // URL encoded form request
  form: {
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  },
  
  // Request with authentication
  authenticated: (token: string) => ({
    headers: {
      'Authorization': `Bearer ${token}`,
    },
  }),
  
  // Request with custom timeout
  withTimeout: (timeout: number) => ({
    timeout,
  }),
  
  // Request with no cache
  noCache: {
    headers: {
      'Cache-Control': 'no-cache',
      'Pragma': 'no-cache',
    },
  },
} as const;

/**
 * Error handling configuration
 */
export const errorConfig = {
  // Status codes that should trigger a retry
  retryableStatusCodes: [408, 429, 500, 502, 503, 504],
  
  // Status codes that should not trigger a retry
  nonRetryableStatusCodes: [400, 401, 403, 404, 422],
  
  // Default error messages for status codes
  defaultMessages: {
    400: 'Bad Request',
    401: 'Unauthorized',
    403: 'Forbidden',
    404: 'Not Found',
    408: 'Request Timeout',
    409: 'Conflict',
    422: 'Validation Error',
    429: 'Too Many Requests',
    500: 'Internal Server Error',
    502: 'Bad Gateway',
    503: 'Service Unavailable',
    504: 'Gateway Timeout',
  },
} as const;

/**
 * Development configuration
 */
export const devConfig = {
  // Enable request/response logging in development
  enableLogging: env.NODE_ENV === 'development',
  
  // Enable mock data in development
  enableMocks: false,
  
  // API delay simulation in development (ms)
  simulateDelay: 0,
  
  // Enable detailed error messages in development
  detailedErrors: env.NODE_ENV === 'development',
} as const;

/**
 * Production configuration
 */
export const prodConfig = {
  // Enable request compression
  enableCompression: true,
  
  // Enable request caching
  enableCaching: true,
  
  // Enable error reporting
  enableErrorReporting: true,
  
  // Enable performance monitoring
  enablePerformanceMonitoring: true,
} as const;

/**
 * Get configuration based on environment
 */
export const getConfig = () => {
  const baseConfig = {
    api: apiConfig,
    endpoints: endpointConfig,
    requests: requestConfig,
    errors: errorConfig,
  };

  if (env.NODE_ENV === 'development') {
    return {
      ...baseConfig,
      dev: devConfig,
    };
  }

  return {
    ...baseConfig,
    prod: prodConfig,
  };
};

export default getConfig();
