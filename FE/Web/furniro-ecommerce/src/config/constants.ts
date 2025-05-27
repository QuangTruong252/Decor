import { env } from './env';

/**
 * Application configuration constants
 */
export const appConfig = {
  name: env.NEXT_PUBLIC_APP_NAME,
  version: env.NEXT_PUBLIC_APP_VERSION,
  url: env.NEXT_PUBLIC_APP_URL,
  description: 'Modern furniture e-commerce platform',
  keywords: ['furniture', 'home decor', 'interior design', 'e-commerce'],
} as const;

/**
 * Theme configuration
 */
export const themeConfig = {
  defaultTheme: 'light',
  themes: ['light', 'dark'],
  colors: {
    primary: '#B88E2F',
    secondary: '#F9F1E7',
    accent: '#FFF3E3',
    neutral: '#3A3A3A',
    success: '#10B981',
    warning: '#F59E0B',
    error: '#EF4444',
    info: '#3B82F6',
  },
} as const;

/**
 * Layout configuration
 */
export const layoutConfig = {
  header: {
    height: '80px',
    sticky: true,
  },
  footer: {
    height: '200px',
  },
  sidebar: {
    width: '280px',
    collapsedWidth: '80px',
  },
  container: {
    maxWidth: '1200px',
    padding: '0 20px',
  },
  breakpoints: {
    xs: '480px',
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1536px',
  },
} as const;

/**
 * Navigation configuration
 */
export const navigationConfig = {
  mainMenu: [
    { label: 'Home', href: '/', icon: 'home' },
    { label: 'Shop', href: '/shop', icon: 'shop' },
    { label: 'About', href: '/about', icon: 'info' },
    { label: 'Contact', href: '/contact', icon: 'contact' },
  ],
  userMenu: [
    { label: 'Profile', href: '/profile', icon: 'user' },
    { label: 'Orders', href: '/orders', icon: 'orders' },
    { label: 'Wishlist', href: '/wishlist', icon: 'heart' },
    { label: 'Settings', href: '/settings', icon: 'settings' },
  ],
  adminMenu: [
    { label: 'Dashboard', href: '/admin', icon: 'dashboard' },
    { label: 'Products', href: '/admin/products', icon: 'products' },
    { label: 'Orders', href: '/admin/orders', icon: 'orders' },
    { label: 'Customers', href: '/admin/customers', icon: 'users' },
    { label: 'Categories', href: '/admin/categories', icon: 'categories' },
    { label: 'Reviews', href: '/admin/reviews', icon: 'reviews' },
  ],
} as const;

/**
 * SEO configuration
 */
export const seoConfig = {
  defaultTitle: 'Furniro - Modern Furniture Store',
  titleTemplate: '%s | Furniro',
  defaultDescription: 'Discover modern furniture and home decor at Furniro. Quality pieces for every room.',
  siteUrl: env.NEXT_PUBLIC_APP_URL,
  defaultImage: '/images/og-image.jpg',
  twitterHandle: '@furniro',
  locale: 'en_US',
  type: 'website',
} as const;

/**
 * Social media configuration
 */
export const socialConfig = {
  facebook: 'https://facebook.com/furniro',
  twitter: 'https://twitter.com/furniro',
  instagram: 'https://instagram.com/furniro',
  linkedin: 'https://linkedin.com/company/furniro',
  youtube: 'https://youtube.com/furniro',
} as const;

/**
 * Contact information
 */
export const contactConfig = {
  email: 'info@furniro.com',
  phone: '+1 (555) 123-4567',
  address: {
    street: '123 Furniture Street',
    city: 'Design City',
    state: 'DC',
    zip: '12345',
    country: 'United States',
  },
  hours: {
    monday: '9:00 AM - 6:00 PM',
    tuesday: '9:00 AM - 6:00 PM',
    wednesday: '9:00 AM - 6:00 PM',
    thursday: '9:00 AM - 6:00 PM',
    friday: '9:00 AM - 6:00 PM',
    saturday: '10:00 AM - 4:00 PM',
    sunday: 'Closed',
  },
} as const;

/**
 * Feature flags
 */
export const featureFlags = {
  // Core features
  userRegistration: true,
  guestCheckout: true,
  productReviews: true,
  wishlist: true,
  
  // Payment features
  creditCardPayment: true,
  paypalPayment: true,
  bankTransfer: true,
  cashOnDelivery: true,
  
  // Social features
  socialLogin: false,
  socialSharing: true,
  
  // Advanced features
  multiCurrency: false,
  multiLanguage: false,
  liveChat: false,
  pushNotifications: false,
  
  // Admin features
  analytics: true,
  bulkOperations: true,
  exportData: true,
  
  // Development features
  debugMode: env.NODE_ENV === 'development',
  mockData: false,
} as const;

/**
 * Business rules configuration
 */
export const businessConfig = {
  // Order settings
  minOrderAmount: 0,
  maxOrderAmount: 10000,
  freeShippingThreshold: 100,
  
  // Inventory settings
  lowStockThreshold: 10,
  outOfStockThreshold: 0,
  
  // Review settings
  requirePurchaseForReview: true,
  maxReviewsPerProduct: 1000,
  reviewModerationEnabled: false,
  
  // Cart settings
  maxCartItems: 50,
  cartExpirationDays: 30,
  
  // User settings
  maxWishlistItems: 100,
  accountVerificationRequired: false,
  
  // Return policy
  returnWindowDays: 30,
  refundProcessingDays: 7,
} as const;

/**
 * Performance configuration
 */
export const performanceConfig = {
  // Image optimization
  imageQuality: 80,
  imageSizes: [320, 640, 768, 1024, 1280, 1920],
  
  // Lazy loading
  lazyLoadingEnabled: true,
  lazyLoadingThreshold: 100,
  
  // Caching
  staticCacheDuration: 31536000, // 1 year
  apiCacheDuration: 300, // 5 minutes
  
  // Bundle optimization
  codesplitting: true,
  treeshaking: true,
  
  // Performance monitoring
  performanceMonitoring: env.NODE_ENV === 'production',
  errorReporting: env.NODE_ENV === 'production',
} as const;

/**
 * Security configuration
 */
export const securityConfig = {
  // Authentication
  jwtExpirationTime: '24h',
  refreshTokenExpirationTime: '7d',
  maxLoginAttempts: 5,
  lockoutDuration: 15 * 60 * 1000, // 15 minutes
  
  // Password requirements
  minPasswordLength: 8,
  requireUppercase: true,
  requireLowercase: true,
  requireNumbers: true,
  requireSpecialChars: true,
  
  // Session management
  sessionTimeout: 30 * 60 * 1000, // 30 minutes
  rememberMeDuration: 30 * 24 * 60 * 60 * 1000, // 30 days
  
  // Content security
  allowedImageTypes: ['image/jpeg', 'image/png', 'image/webp'],
  maxFileSize: 5 * 1024 * 1024, // 5MB
  
  // Rate limiting
  rateLimitEnabled: true,
  rateLimitRequests: 100,
  rateLimitWindow: 60 * 1000, // 1 minute
} as const;

/**
 * Export all configuration
 */
export const config = {
  app: appConfig,
  theme: themeConfig,
  layout: layoutConfig,
  navigation: navigationConfig,
  seo: seoConfig,
  social: socialConfig,
  contact: contactConfig,
  features: featureFlags,
  business: businessConfig,
  performance: performanceConfig,
  security: securityConfig,
} as const;

export default config;
