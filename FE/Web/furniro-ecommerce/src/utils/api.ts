import type { ApiError } from '@/api/types';

/**
 * API utility functions
 */
export class ApiUtils {
  /**
   * Handle API errors consistently
   */
  static handleApiError(error: any): ApiError {
    // If it's already an ApiError, return as is
    if (error.message && error.status) {
      return error as ApiError;
    }

    // Handle axios errors
    if (error.response) {
      return {
        message: error.response.data?.message || error.message || 'An error occurred',
        status: error.response.status,
        errors: error.response.data?.errors,
      };
    }

    // Handle network errors
    if (error.request) {
      return {
        message: 'Network error. Please check your connection.',
        status: 0,
      };
    }

    // Handle other errors
    return {
      message: error.message || 'An unexpected error occurred',
      status: 500,
    };
  }

  /**
   * Format error message for display
   */
  static formatErrorMessage(error: ApiError): string {
    if (error.errors && Object.keys(error.errors).length > 0) {
      // Return first validation error
      const firstField = Object.keys(error.errors)[0];
      const firstError = error.errors[firstField][0];
      return firstError || error.message;
    }

    return error.message;
  }

  /**
   * Get all validation errors as array
   */
  static getValidationErrors(error: ApiError): string[] {
    if (!error.errors) return [error.message];

    const errors: string[] = [];
    Object.values(error.errors).forEach(fieldErrors => {
      if (Array.isArray(fieldErrors)) {
        errors.push(...fieldErrors);
      }
    });

    return errors.length > 0 ? errors : [error.message];
  }

  /**
   * Check if error is a specific HTTP status
   */
  static isErrorStatus(error: ApiError, status: number): boolean {
    return error.status === status;
  }

  /**
   * Check if error is unauthorized
   */
  static isUnauthorizedError(error: ApiError): boolean {
    return this.isErrorStatus(error, 401);
  }

  /**
   * Check if error is forbidden
   */
  static isForbiddenError(error: ApiError): boolean {
    return this.isErrorStatus(error, 403);
  }

  /**
   * Check if error is not found
   */
  static isNotFoundError(error: ApiError): boolean {
    return this.isErrorStatus(error, 404);
  }

  /**
   * Check if error is validation error
   */
  static isValidationError(error: ApiError): boolean {
    return this.isErrorStatus(error, 400) && !!error.errors;
  }

  /**
   * Check if error is server error
   */
  static isServerError(error: ApiError): boolean {
    return error.status >= 500;
  }

  /**
   * Check if error is network error
   */
  static isNetworkError(error: ApiError): boolean {
    return error.status === 0;
  }

  /**
   * Build query string from object
   */
  static buildQueryString(params: Record<string, any>): string {
    const searchParams = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        if (Array.isArray(value)) {
          value.forEach(item => searchParams.append(key, String(item)));
        } else {
          searchParams.append(key, String(value));
        }
      }
    });

    const queryString = searchParams.toString();
    return queryString ? `?${queryString}` : '';
  }

  /**
   * Parse query string to object
   */
  static parseQueryString(queryString: string): Record<string, string | string[]> {
    const params: Record<string, string | string[]> = {};
    const searchParams = new URLSearchParams(queryString);

    for (const [key, value] of searchParams.entries()) {
      if (params[key]) {
        // Convert to array if multiple values
        if (Array.isArray(params[key])) {
          (params[key] as string[]).push(value);
        } else {
          params[key] = [params[key] as string, value];
        }
      } else {
        params[key] = value;
      }
    }

    return params;
  }

  /**
   * Debounce function for API calls
   */
  static debounce<T extends (...args: any[]) => any>(
    func: T,
    wait: number
  ): (...args: Parameters<T>) => void {
    let timeout: NodeJS.Timeout;

    return (...args: Parameters<T>) => {
      clearTimeout(timeout);
      timeout = setTimeout(() => func(...args), wait);
    };
  }

  /**
   * Throttle function for API calls
   */
  static throttle<T extends (...args: any[]) => any>(
    func: T,
    limit: number
  ): (...args: Parameters<T>) => void {
    let inThrottle: boolean;

    return (...args: Parameters<T>) => {
      if (!inThrottle) {
        func(...args);
        inThrottle = true;
        setTimeout(() => inThrottle = false, limit);
      }
    };
  }

  /**
   * Retry API call with exponential backoff
   */
  static async retry<T>(
    fn: () => Promise<T>,
    maxRetries: number = 3,
    baseDelay: number = 1000
  ): Promise<T> {
    let lastError: any;

    for (let attempt = 0; attempt <= maxRetries; attempt++) {
      try {
        return await fn();
      } catch (error) {
        lastError = error;

        if (attempt === maxRetries) {
          throw this.handleApiError(error);
        }

        // Don't retry on client errors (4xx)
        const apiError = this.handleApiError(error);
        if (apiError.status >= 400 && apiError.status < 500) {
          throw apiError;
        }

        // Exponential backoff
        const delay = baseDelay * Math.pow(2, attempt);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }

    throw this.handleApiError(lastError);
  }

  /**
   * Create FormData from object
   */
  static createFormData(data: Record<string, any>): FormData {
    const formData = new FormData();

    Object.entries(data).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        if (value instanceof File) {
          formData.append(key, value);
        } else if (Array.isArray(value)) {
          value.forEach((item, index) => {
            if (item instanceof File) {
              formData.append(key, item);
            } else {
              formData.append(`${key}[${index}]`, String(item));
            }
          });
        } else {
          formData.append(key, String(value));
        }
      }
    });

    return formData;
  }

  /**
   * Format file size for display
   */
  static formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Generate unique request ID
   */
  static generateRequestId(): string {
    return `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Check if response is successful
   */
  static isSuccessResponse(status: number): boolean {
    return status >= 200 && status < 300;
  }

  /**
   * Get error message based on status code
   */
  static getStatusMessage(status: number): string {
    const statusMessages: Record<number, string> = {
      400: 'Bad Request',
      401: 'Unauthorized',
      403: 'Forbidden',
      404: 'Not Found',
      409: 'Conflict',
      422: 'Validation Error',
      429: 'Too Many Requests',
      500: 'Internal Server Error',
      502: 'Bad Gateway',
      503: 'Service Unavailable',
      504: 'Gateway Timeout',
    };

    return statusMessages[status] || 'Unknown Error';
  }

  /**
   * Log API call for debugging
   */
  static logApiCall(method: string, url: string, data?: any, response?: any): void {
    if (process.env.NODE_ENV === 'development') {
      console.group(`🌐 API ${method.toUpperCase()} ${url}`);
      if (data) console.log('📤 Request:', data);
      if (response) console.log('📥 Response:', response);
      console.groupEnd();
    }
  }
}

export default ApiUtils;
