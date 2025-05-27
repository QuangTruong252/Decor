import { z } from 'zod';

/**
 * Common validation schemas using Zod
 */

// Auth validation schemas
export const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
  rememberMe: z.boolean().optional(),
});

export const registerSchema = z.object({
  username: z.string()
    .min(3, 'Username must be at least 3 characters')
    .max(50, 'Username must be less than 50 characters')
    .regex(/^[a-zA-Z0-9_]+$/, 'Username can only contain letters, numbers, and underscores'),
  email: z.string().email('Invalid email address'),
  password: z.string()
    .min(6, 'Password must be at least 6 characters')
    .max(100, 'Password must be less than 100 characters'),
  confirmPassword: z.string(),
  acceptTerms: z.boolean().refine(val => val === true, 'You must accept the terms and conditions'),
}).refine(data => data.password === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

export const changePasswordSchema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string()
    .min(6, 'New password must be at least 6 characters')
    .max(100, 'New password must be less than 100 characters'),
  confirmNewPassword: z.string(),
}).refine(data => data.newPassword === data.confirmNewPassword, {
  message: 'New passwords do not match',
  path: ['confirmNewPassword'],
});

// Product validation schemas
export const productSchema = z.object({
  name: z.string()
    .min(3, 'Product name must be at least 3 characters')
    .max(255, 'Product name must be less than 255 characters'),
  slug: z.string()
    .min(1, 'Slug is required')
    .max(255, 'Slug must be less than 255 characters')
    .regex(/^[a-z0-9-]+$/, 'Slug can only contain lowercase letters, numbers, and hyphens'),
  description: z.string().optional(),
  price: z.number()
    .min(0.01, 'Price must be greater than 0')
    .max(999999.99, 'Price is too high'),
  originalPrice: z.number()
    .min(0, 'Original price cannot be negative')
    .optional(),
  stockQuantity: z.number()
    .int('Stock quantity must be a whole number')
    .min(0, 'Stock quantity cannot be negative'),
  sku: z.string()
    .min(1, 'SKU is required')
    .max(50, 'SKU must be less than 50 characters'),
  categoryId: z.number().int().positive('Category is required'),
  isFeatured: z.boolean().optional(),
  isActive: z.boolean().optional(),
});

// Category validation schemas
export const categorySchema = z.object({
  name: z.string()
    .min(2, 'Category name must be at least 2 characters')
    .max(100, 'Category name must be less than 100 characters'),
  slug: z.string()
    .min(1, 'Slug is required')
    .max(100, 'Slug must be less than 100 characters')
    .regex(/^[a-z0-9-]+$/, 'Slug can only contain lowercase letters, numbers, and hyphens'),
  description: z.string()
    .max(255, 'Description must be less than 255 characters')
    .optional(),
  parentId: z.number().int().positive().optional(),
});

// Customer validation schemas
export const customerSchema = z.object({
  firstName: z.string()
    .min(2, 'First name must be at least 2 characters')
    .max(100, 'First name must be less than 100 characters'),
  lastName: z.string()
    .min(2, 'Last name must be at least 2 characters')
    .max(100, 'Last name must be less than 100 characters'),
  email: z.string().email('Invalid email address'),
  phone: z.string()
    .regex(/^[\+]?[1-9][\d]{0,15}$/, 'Invalid phone number')
    .optional()
    .or(z.literal('')),
  address: z.string()
    .max(255, 'Address must be less than 255 characters')
    .optional(),
  city: z.string()
    .max(100, 'City must be less than 100 characters')
    .optional(),
  state: z.string()
    .max(50, 'State must be less than 50 characters')
    .optional(),
  postalCode: z.string()
    .max(20, 'Postal code must be less than 20 characters')
    .optional(),
  country: z.string()
    .max(50, 'Country must be less than 50 characters')
    .optional(),
});

// Order validation schemas
export const orderItemSchema = z.object({
  productId: z.number().int().positive('Product is required'),
  quantity: z.number()
    .int('Quantity must be a whole number')
    .min(1, 'Quantity must be at least 1'),
});

export const orderSchema = z.object({
  userId: z.number().int().positive('User is required'),
  paymentMethod: z.string()
    .min(1, 'Payment method is required')
    .max(50, 'Payment method must be less than 50 characters'),
  shippingAddress: z.string()
    .min(1, 'Shipping address is required')
    .max(255, 'Shipping address must be less than 255 characters'),
  orderItems: z.array(orderItemSchema)
    .min(1, 'At least one item is required'),
});

// Review validation schemas
export const reviewSchema = z.object({
  userId: z.number().int().positive('User is required'),
  productId: z.number().int().positive('Product is required'),
  rating: z.number()
    .int('Rating must be a whole number')
    .min(1, 'Rating must be at least 1')
    .max(5, 'Rating must be at most 5'),
  comment: z.string()
    .max(500, 'Comment must be less than 500 characters')
    .optional(),
});

// Cart validation schemas
export const addToCartSchema = z.object({
  productId: z.number().int().positive('Product is required'),
  quantity: z.number()
    .int('Quantity must be a whole number')
    .min(1, 'Quantity must be at least 1'),
});

export const updateCartItemSchema = z.object({
  quantity: z.number()
    .int('Quantity must be a whole number')
    .min(1, 'Quantity must be at least 1'),
});

// Contact form validation schema
export const contactSchema = z.object({
  name: z.string()
    .min(2, 'Name must be at least 2 characters')
    .max(100, 'Name must be less than 100 characters'),
  email: z.string().email('Invalid email address'),
  subject: z.string()
    .min(5, 'Subject must be at least 5 characters')
    .max(200, 'Subject must be less than 200 characters'),
  message: z.string()
    .min(10, 'Message must be at least 10 characters')
    .max(1000, 'Message must be less than 1000 characters'),
});

// Search validation schema
export const searchSchema = z.object({
  query: z.string()
    .min(1, 'Search query is required')
    .max(100, 'Search query must be less than 100 characters'),
  category: z.string().optional(),
  minPrice: z.number().min(0).optional(),
  maxPrice: z.number().min(0).optional(),
  sortBy: z.enum(['name', 'price', 'rating', 'date']).optional(),
  sortOrder: z.enum(['asc', 'desc']).optional(),
});

// File upload validation
export const imageFileSchema = z.object({
  file: z.instanceof(File)
    .refine(file => file.size <= 5 * 1024 * 1024, 'File size must be less than 5MB')
    .refine(
      file => ['image/jpeg', 'image/png', 'image/webp'].includes(file.type),
      'File must be a JPEG, PNG, or WebP image'
    ),
});

// Utility functions for validation
export class ValidationUtils {
  /**
   * Validate email format
   */
  static isValidEmail(email: string): boolean {
    return z.string().email().safeParse(email).success;
  }

  /**
   * Validate phone number
   */
  static isValidPhone(phone: string): boolean {
    return z.string().regex(/^[\+]?[1-9][\d]{0,15}$/).safeParse(phone).success;
  }

  /**
   * Validate URL format
   */
  static isValidUrl(url: string): boolean {
    return z.string().url().safeParse(url).success;
  }

  /**
   * Validate slug format
   */
  static isValidSlug(slug: string): boolean {
    return z.string().regex(/^[a-z0-9-]+$/).safeParse(slug).success;
  }

  /**
   * Generate slug from string
   */
  static generateSlug(text: string): string {
    return text
      .toLowerCase()
      .trim()
      .replace(/[^\w\s-]/g, '')
      .replace(/[\s_-]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  /**
   * Validate file size
   */
  static isValidFileSize(file: File, maxSizeInMB: number): boolean {
    return file.size <= maxSizeInMB * 1024 * 1024;
  }

  /**
   * Validate image file type
   */
  static isValidImageType(file: File): boolean {
    const validTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    return validTypes.includes(file.type);
  }

  /**
   * Format validation errors
   */
  static formatZodErrors(error: z.ZodError): Record<string, string> {
    const formattedErrors: Record<string, string> = {};
    
    error.errors.forEach(err => {
      const path = err.path.join('.');
      formattedErrors[path] = err.message;
    });
    
    return formattedErrors;
  }
}

// Export all schemas
export const schemas = {
  login: loginSchema,
  register: registerSchema,
  changePassword: changePasswordSchema,
  product: productSchema,
  category: categorySchema,
  customer: customerSchema,
  order: orderSchema,
  orderItem: orderItemSchema,
  review: reviewSchema,
  addToCart: addToCartSchema,
  updateCartItem: updateCartItemSchema,
  contact: contactSchema,
  search: searchSchema,
  imageFile: imageFileSchema,
};

export type ValidationSchemas = typeof schemas;
