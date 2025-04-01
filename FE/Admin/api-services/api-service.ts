/**
 * Centralized API Service
 * This file provides a centralized access point to all API clients
 */

import { 
  AuthApi, 
  BannerApi, 
  CategoryApi, 
  HealthCheckApi, 
  OrderApi, 
  ProductsApi, 
  ReviewApi 
} from './api';

import { useToast } from '#imports';

/**
 * Centralized API service with all available API clients
 */
export const apiService = {
  authApi: new AuthApi(),
  bannerApi: new BannerApi(),
  categoryApi: new CategoryApi(),
  healthCheckApi: new HealthCheckApi(),
  orderApi: new OrderApi(),
  productsApi: new ProductsApi(),
  reviewApi: new ReviewApi(),
};

/**
 * Generic error handler for API calls
 * @param error - The error object
 * @param context - Optional context message
 */
export const handleApiError = (error: unknown, context?: string): void => {
  const toast = useToast();
  console.error(`API Error${context ? ` (${context})` : ''}:`, error);
  
  let errorMessage: string;
  
  if (error instanceof Error) {
    errorMessage = error.message;
  } else if (typeof error === 'object' && error !== null && 'message' in error) {
    // Handle axios error or similar object structure
    errorMessage = String((error as { message: unknown }).message);
  } else {
    errorMessage = context || 'Unknown API error occurred';
  }
  
  toast.add({
    title: 'Error',
    description: errorMessage,
    color: 'red'
  });
};

/**
 * Show success toast message
 * @param message - Success message to display
 */
export const showSuccessToast = (message: string): void => {
  const toast = useToast();
  toast.add({
    title: 'Success',
    description: message,
    color: 'green'
  });
};

/**
 * Show error toast message
 * @param message - Error message to display
 */
export const showErrorToast = (message: string): void => {
  const toast = useToast();
  toast.add({
    title: 'Error',
    description: message,
    color: 'red'
  });
}; 