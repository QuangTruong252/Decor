// Export all API services from a single entry point

export { AuthService } from './authService';
export { ProductService } from './productService';
export { CategoryService } from './categoryService';
export { CartService } from './cartService';
export { OrderService } from './orderService';
export { CustomerService } from './customerService';
export { ReviewService } from './reviewService';
export { DashboardService } from './dashboardService';
export { BannerService } from './bannerService';

// Re-export default exports for convenience
export { default as authService } from './authService';
export { default as productService } from './productService';
export { default as categoryService } from './categoryService';
export { default as cartService } from './cartService';
export { default as orderService } from './orderService';
export { default as customerService } from './customerService';
export { default as reviewService } from './reviewService';
export { default as dashboardService } from './dashboardService';
export { default as bannerService } from './bannerService';

// Import services for the services object
import { AuthService } from './authService';
import { ProductService } from './productService';
import { CategoryService } from './categoryService';
import { CartService } from './cartService';
import { OrderService } from './orderService';
import { CustomerService } from './customerService';
import { ReviewService } from './reviewService';
import { DashboardService } from './dashboardService';
import { BannerService } from './bannerService';

// Create a services object for easy access
export const services = {
  auth: AuthService,
  product: ProductService,
  category: CategoryService,
  cart: CartService,
  order: OrderService,
  customer: CustomerService,
  review: ReviewService,
  dashboard: DashboardService,
  banner: BannerService,
};

// Export service types for TypeScript
export type Services = typeof services;
