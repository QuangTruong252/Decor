import { ref } from 'vue';
import type { Ref } from 'vue';
import axios from 'axios';
import type { 
  AxiosPromise, 
  AxiosRequestConfig, 
  AxiosResponse, 
  AxiosError, 
  CancelTokenSource 
} from 'axios';
import { useToast } from '#imports';
import { useAuthStore } from '~/stores/auth'

interface ApiState<T> {
  data: Ref<T | null>;
  isLoading: Ref<boolean>;
  error: Ref<Error | null>;
  execute: () => Promise<AxiosResponse<T> | undefined>;
  cancel: () => void;
}

interface UseApiOptions {
  retries?: number;
  retryDelay?: number;
  cacheTime?: number;
  immediate?: boolean;
}

const DEFAULT_OPTIONS: UseApiOptions = {
  retries: 1,
  retryDelay: 1000,
  cacheTime: 0, // No cache by default
  immediate: false,
};

// Simple cache implementation
const cache = new Map<string, { data: any; timestamp: number }>();

/**
 * Composable for handling API requests with advanced features
 * @param apiCallFn - Function that returns an AxiosPromise
 * @param options - Configuration options
 * @returns ApiState object with data, loading state, error and execute function
 */
export function useApiAdvanced<T>(
  apiCallFn: (config?: AxiosRequestConfig) => AxiosPromise<T>,
  options: UseApiOptions = {}
): ApiState<T> {
  const mergedOptions = { ...DEFAULT_OPTIONS, ...options };
  const data = ref<T | null>(null) as Ref<T | null>;
  const isLoading = ref<boolean>(false);
  const error = ref<Error | null>(null);
  let cancelTokenSource: CancelTokenSource | null = null;
  const toast = useToast();

  // Generate a cache key based on the function and its arguments
  const getCacheKey = (): string => {
    // This is a simple implementation, in a real app you might want to
    // include more details for a unique key based on parameters
    return apiCallFn.toString();
  };

  // Check and retrieve from cache
  const getFromCache = (): T | null => {
    if (!mergedOptions.cacheTime) return null;
    
    const cacheKey = getCacheKey();
    const cachedItem = cache.get(cacheKey);
    
    if (!cachedItem) return null;
    
    const now = Date.now();
    if (now - cachedItem.timestamp < mergedOptions.cacheTime) {
      return cachedItem.data;
    }
    
    // Cache expired
    cache.delete(cacheKey);
    return null;
  };

  // Set cache
  const setCache = (responseData: T): void => {
    if (!mergedOptions.cacheTime) return;
    
    const cacheKey = getCacheKey();
    cache.set(cacheKey, {
      data: responseData,
      timestamp: Date.now(),
    });
  };

  // Execute the API call with retries
  const execute = async (): Promise<AxiosResponse<T> | undefined> => {
    // Check cache first
    const cachedData = getFromCache();
    if (cachedData) {
      data.value = cachedData;
      return { data: cachedData } as AxiosResponse<T>;
    }

    error.value = null;
    isLoading.value = true;
    
    // Create new cancel token
    cancelTokenSource = axios.CancelToken.source();
    
    let retries = 0;
    let success = false;
    let lastResponse: AxiosResponse<T> | undefined;
    
    while (!success && retries <= mergedOptions.retries!) {
      try {
        const response = await apiCallFn({
          cancelToken: cancelTokenSource.token,
        });
        
        lastResponse = response;
        data.value = response.data;
        success = true;
        
        // Cache the response if caching is enabled
        setCache(response.data);
        
        return response;
      } catch (err) {
        if (axios.isCancel(err)) {
          // Request was cancelled, don't retry
          break;
        }
        
        const axiosError = err as AxiosError;
        
        // Don't retry for certain status codes
        if (axiosError.response && 
            (axiosError.response.status === 401 || 
             axiosError.response.status === 403 || 
             axiosError.response.status === 404)) {
          const errorData = axiosError.response.data as Record<string, unknown>;
          error.value = new Error(
            typeof errorData.message === 'string' ? errorData.message : 
            `Error ${axiosError.response.status}: ${axiosError.response.statusText}`
          );
          break;
        }
        
        if (retries < mergedOptions.retries!) {
          // Wait before retrying
          await new Promise(resolve => 
            setTimeout(resolve, mergedOptions.retryDelay)
          );
          retries++;
        } else {
          // Max retries reached
          if (err instanceof Error) {
            error.value = err;
          } else {
            error.value = new Error('Unknown API error occurred');
          }
        }
      }
    }
    
    isLoading.value = false;
    return lastResponse;
  };

  // Cancel the API request
  const cancel = (): void => {
    if (cancelTokenSource) {
      cancelTokenSource.cancel('Request cancelled by user');
    }
  };

  // Execute immediately if specified
  if (mergedOptions.immediate) {
    execute();
  }

  return {
    data,
    isLoading,
    error,
    execute,
    cancel
  };
}

/**
 * Enhanced API module with common methods
 * This is for backward compatibility with direct API calls
 */
export const useApiCompat = () => {
  const toast = useToast();

  return {
    get: async <T>(url: string, config?: AxiosRequestConfig) => {
      try {
        const response = await axios.get<T>(url, config);
        return { data: response.data, error: null };
      } catch (err) {
        console.error('GET request error:', err);
        const error = err instanceof Error ? err : new Error('Unknown error during GET request');
        toast.add({
          title: 'Error',
          description: error.message,
          color: 'red'
        });
        return { data: null, error };
      }
    },
    
    post: async <T>(url: string, data: any, config?: AxiosRequestConfig) => {
      try {
        const response = await axios.post<T>(url, data, config);
        return { data: response.data, error: null };
      } catch (err) {
        console.error('POST request error:', err);
        const error = err instanceof Error ? err : new Error('Unknown error during POST request');
        toast.add({
          title: 'Error',
          description: error.message,
          color: 'red'
        });
        return { data: null, error };
      }
    },
    
    put: async <T>(url: string, data: any, config?: AxiosRequestConfig) => {
      try {
        const response = await axios.put<T>(url, data, config);
        return { data: response.data, error: null };
      } catch (err) {
        console.error('PUT request error:', err);
        const error = err instanceof Error ? err : new Error('Unknown error during PUT request');
        toast.add({
          title: 'Error',
          description: error.message,
          color: 'red'
        });
        return { data: null, error };
      }
    },
    
    delete: async <T>(url: string, config?: AxiosRequestConfig) => {
      try {
        const response = await axios.delete<T>(url, config);
        return { data: response.data, error: null };
      } catch (err) {
        console.error('DELETE request error:', err);
        const error = err instanceof Error ? err : new Error('Unknown error during DELETE request');
        toast.add({
          title: 'Error',
          description: error.message,
          color: 'red'
        });
        return { data: null, error };
      }
    }
  };
};

export const useApi = () => {
  const config = useRuntimeConfig()
  const auth = useAuthStore()
  
  const baseURL = config.public.apiBaseUrl
  
  const fetchWithAuth = async (endpoint: string, options: any = {}) => {
    const headers = {
      ...options.headers || {},
    }
    
    if (auth.token) {
      headers.Authorization = `Bearer ${auth.token}`
    }
    
    try {
      console.log(baseURL)
      const { data, error } = await useFetch(endpoint, {
        ...options,
        baseURL,
        headers
      })
      
      if (error.value) {
        // Handle 401 Unauthorized
        if (error.value.statusCode === 401) {
          auth.logout()
          return { data: null, error: error.value }
        }
        
        return { data: null, error: error.value }
      }
      
      return { data: data.value, error: null }
    } catch (err) {
      return { data: null, error: err }
    }
  }
  
  return {
    get: (endpoint: string, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'GET' }),
      
    post: (endpoint: string, data: any, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'POST', body: data }),
      
    put: (endpoint: string, data: any, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'PUT', body: data }),
      
    delete: (endpoint: string, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'DELETE' })
  }
} 