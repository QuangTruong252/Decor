/**
 * Application constants
 */

// Storage keys
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'auth_token',
  REFRESH_TOKEN: 'refresh_token',
  USER: 'user',
  CART: 'cart',
  GUEST_CART: 'guest_cart',
  PREFERENCES: 'user_preferences',
  THEME: 'theme',
  LANGUAGE: 'language',
  RECENT_SEARCHES: 'recent_searches',
  WISHLIST: 'wishlist',
} as const;

// API constants
export const API_CONSTANTS = {
  DEFAULT_PAGE_SIZE: 20,
  MAX_PAGE_SIZE: 100,
  REQUEST_TIMEOUT: 10000,
  RETRY_ATTEMPTS: 3,
  RETRY_DELAY: 1000,
} as const;

// Order status constants
export const ORDER_STATUS = {
  PENDING: 'pending',
  CONFIRMED: 'confirmed',
  PROCESSING: 'processing',
  SHIPPED: 'shipped',
  DELIVERED: 'delivered',
  CANCELLED: 'cancelled',
  REFUNDED: 'refunded',
} as const;

export const ORDER_STATUS_LABELS = {
  [ORDER_STATUS.PENDING]: 'Pending',
  [ORDER_STATUS.CONFIRMED]: 'Confirmed',
  [ORDER_STATUS.PROCESSING]: 'Processing',
  [ORDER_STATUS.SHIPPED]: 'Shipped',
  [ORDER_STATUS.DELIVERED]: 'Delivered',
  [ORDER_STATUS.CANCELLED]: 'Cancelled',
  [ORDER_STATUS.REFUNDED]: 'Refunded',
} as const;

export const ORDER_STATUS_COLORS = {
  [ORDER_STATUS.PENDING]: 'yellow',
  [ORDER_STATUS.CONFIRMED]: 'blue',
  [ORDER_STATUS.PROCESSING]: 'purple',
  [ORDER_STATUS.SHIPPED]: 'indigo',
  [ORDER_STATUS.DELIVERED]: 'green',
  [ORDER_STATUS.CANCELLED]: 'red',
  [ORDER_STATUS.REFUNDED]: 'gray',
} as const;

// Payment method constants
export const PAYMENT_METHODS = {
  CREDIT_CARD: 'credit_card',
  DEBIT_CARD: 'debit_card',
  PAYPAL: 'paypal',
  BANK_TRANSFER: 'bank_transfer',
  CASH_ON_DELIVERY: 'cash_on_delivery',
} as const;

export const PAYMENT_METHOD_LABELS = {
  [PAYMENT_METHODS.CREDIT_CARD]: 'Credit Card',
  [PAYMENT_METHODS.DEBIT_CARD]: 'Debit Card',
  [PAYMENT_METHODS.PAYPAL]: 'PayPal',
  [PAYMENT_METHODS.BANK_TRANSFER]: 'Bank Transfer',
  [PAYMENT_METHODS.CASH_ON_DELIVERY]: 'Cash on Delivery',
} as const;

// User roles
export const USER_ROLES = {
  ADMIN: 'admin',
  CUSTOMER: 'customer',
  MODERATOR: 'moderator',
} as const;

export const USER_ROLE_LABELS = {
  [USER_ROLES.ADMIN]: 'Administrator',
  [USER_ROLES.CUSTOMER]: 'Customer',
  [USER_ROLES.MODERATOR]: 'Moderator',
} as const;

// Product sort options
export const PRODUCT_SORT_OPTIONS = {
  NAME_ASC: 'name_asc',
  NAME_DESC: 'name_desc',
  PRICE_ASC: 'price_asc',
  PRICE_DESC: 'price_desc',
  RATING_ASC: 'rating_asc',
  RATING_DESC: 'rating_desc',
  DATE_ASC: 'date_asc',
  DATE_DESC: 'date_desc',
} as const;

export const PRODUCT_SORT_LABELS = {
  [PRODUCT_SORT_OPTIONS.NAME_ASC]: 'Name (A-Z)',
  [PRODUCT_SORT_OPTIONS.NAME_DESC]: 'Name (Z-A)',
  [PRODUCT_SORT_OPTIONS.PRICE_ASC]: 'Price (Low to High)',
  [PRODUCT_SORT_OPTIONS.PRICE_DESC]: 'Price (High to Low)',
  [PRODUCT_SORT_OPTIONS.RATING_ASC]: 'Rating (Low to High)',
  [PRODUCT_SORT_OPTIONS.RATING_DESC]: 'Rating (High to Low)',
  [PRODUCT_SORT_OPTIONS.DATE_ASC]: 'Oldest First',
  [PRODUCT_SORT_OPTIONS.DATE_DESC]: 'Newest First',
} as const;

// File upload constants
export const FILE_UPLOAD = {
  MAX_SIZE_MB: 5,
  MAX_SIZE_BYTES: 5 * 1024 * 1024,
  ALLOWED_IMAGE_TYPES: ['image/jpeg', 'image/png', 'image/webp', 'image/gif'],
  ALLOWED_DOCUMENT_TYPES: ['application/pdf', 'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'],
} as const;

// Validation constants
export const VALIDATION = {
  MIN_PASSWORD_LENGTH: 6,
  MAX_PASSWORD_LENGTH: 100,
  MIN_USERNAME_LENGTH: 3,
  MAX_USERNAME_LENGTH: 50,
  MAX_COMMENT_LENGTH: 500,
  MAX_DESCRIPTION_LENGTH: 1000,
  MIN_PRODUCT_NAME_LENGTH: 3,
  MAX_PRODUCT_NAME_LENGTH: 255,
} as const;

// UI constants
export const UI_CONSTANTS = {
  DEBOUNCE_DELAY: 300,
  THROTTLE_DELAY: 1000,
  TOAST_DURATION: 5000,
  MODAL_ANIMATION_DURATION: 200,
  SKELETON_ANIMATION_DURATION: 1500,
} as const;

// Pagination constants
export const PAGINATION = {
  DEFAULT_PAGE: 1,
  DEFAULT_LIMIT: 20,
  MAX_LIMIT: 100,
  VISIBLE_PAGES: 5,
} as const;

// Currency constants
export const CURRENCY = {
  DEFAULT: 'USD',
  SYMBOL: '$',
  DECIMAL_PLACES: 2,
} as const;

// Date format constants
export const DATE_FORMATS = {
  DISPLAY: 'MMM dd, yyyy',
  DISPLAY_WITH_TIME: 'MMM dd, yyyy HH:mm',
  ISO: 'yyyy-MM-dd',
  ISO_WITH_TIME: 'yyyy-MM-dd HH:mm:ss',
  RELATIVE: 'relative',
} as const;

// Error messages
export const ERROR_MESSAGES = {
  NETWORK_ERROR: 'Network error. Please check your connection.',
  UNAUTHORIZED: 'You are not authorized to perform this action.',
  FORBIDDEN: 'Access denied.',
  NOT_FOUND: 'The requested resource was not found.',
  VALIDATION_ERROR: 'Please check your input and try again.',
  SERVER_ERROR: 'An internal server error occurred.',
  UNKNOWN_ERROR: 'An unexpected error occurred.',
  SESSION_EXPIRED: 'Your session has expired. Please log in again.',
  CART_EMPTY: 'Your cart is empty.',
  PRODUCT_OUT_OF_STOCK: 'This product is out of stock.',
  INSUFFICIENT_STOCK: 'Insufficient stock available.',
} as const;

// Success messages
export const SUCCESS_MESSAGES = {
  LOGIN_SUCCESS: 'Successfully logged in.',
  LOGOUT_SUCCESS: 'Successfully logged out.',
  REGISTER_SUCCESS: 'Account created successfully.',
  PROFILE_UPDATED: 'Profile updated successfully.',
  PASSWORD_CHANGED: 'Password changed successfully.',
  PRODUCT_ADDED_TO_CART: 'Product added to cart.',
  PRODUCT_REMOVED_FROM_CART: 'Product removed from cart.',
  CART_UPDATED: 'Cart updated successfully.',
  ORDER_PLACED: 'Order placed successfully.',
  ORDER_CANCELLED: 'Order cancelled successfully.',
  REVIEW_SUBMITTED: 'Review submitted successfully.',
  REVIEW_UPDATED: 'Review updated successfully.',
  REVIEW_DELETED: 'Review deleted successfully.',
} as const;

// Routes
export const ROUTES = {
  HOME: '/',
  SHOP: '/shop',
  PRODUCT: '/product',
  CART: '/cart',
  CHECKOUT: '/checkout',
  ORDERS: '/orders',
  PROFILE: '/profile',
  LOGIN: '/login',
  REGISTER: '/register',
  ABOUT: '/about',
  CONTACT: '/contact',
  ADMIN: '/admin',
  ADMIN_PRODUCTS: '/admin/products',
  ADMIN_ORDERS: '/admin/orders',
  ADMIN_CUSTOMERS: '/admin/customers',
  ADMIN_CATEGORIES: '/admin/categories',
  ADMIN_REVIEWS: '/admin/reviews',
} as const;

// Regex patterns
export const REGEX_PATTERNS = {
  EMAIL: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  PHONE: /^[\+]?[1-9][\d]{0,15}$/,
  SLUG: /^[a-z0-9-]+$/,
  USERNAME: /^[a-zA-Z0-9_]+$/,
  PASSWORD: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{6,}$/,
  POSTAL_CODE: /^[A-Za-z0-9\s-]{3,10}$/,
} as const;

// Feature flags
export const FEATURES = {
  WISHLIST: true,
  REVIEWS: true,
  GUEST_CHECKOUT: true,
  SOCIAL_LOGIN: false,
  MULTI_CURRENCY: false,
  LIVE_CHAT: false,
  PUSH_NOTIFICATIONS: false,
} as const;

// Export all constants as a single object
export const CONSTANTS = {
  STORAGE_KEYS,
  API_CONSTANTS,
  ORDER_STATUS,
  ORDER_STATUS_LABELS,
  ORDER_STATUS_COLORS,
  PAYMENT_METHODS,
  PAYMENT_METHOD_LABELS,
  USER_ROLES,
  USER_ROLE_LABELS,
  PRODUCT_SORT_OPTIONS,
  PRODUCT_SORT_LABELS,
  FILE_UPLOAD,
  VALIDATION,
  UI_CONSTANTS,
  PAGINATION,
  CURRENCY,
  DATE_FORMATS,
  ERROR_MESSAGES,
  SUCCESS_MESSAGES,
  ROUTES,
  REGEX_PATTERNS,
  FEATURES,
} as const;
