/* eslint-disable @typescript-eslint/no-explicit-any */
/**
 * Utility functions for building API query parameters
 */

/**
 * Builds query string from filter parameters
 * @param params - Object containing filter parameters
 * @returns URLSearchParams object
 */
export function buildQueryParams(params: Record<string, any>): URLSearchParams {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      // Handle boolean values
      if (typeof value === 'boolean') {
        searchParams.append(key, value.toString());
      }
      // Handle number values
      else if (typeof value === 'number') {
        searchParams.append(key, value.toString());
      }
      // Handle string values
      else if (typeof value === 'string') {
        searchParams.append(key, value);
      }
      // Handle Date objects
      else if (value instanceof Date) {
        searchParams.append(key, value.toISOString());
      }
    }
  });

  return searchParams;
}

/**
 * Builds query string URL from base URL and parameters
 * @param baseUrl - Base API URL
 * @param params - Filter parameters
 * @returns Complete URL with query string
 */
export function buildApiUrl(baseUrl: string, params: Record<string, any>): string {
  const queryParams = buildQueryParams(params);
  const queryString = queryParams.toString();

  if (queryString) {
    const separator = baseUrl.includes('?') ? '&' : '?';
    return `${baseUrl}${separator}${queryString}`;
  }

  return baseUrl;
}

/**
 * Removes empty or undefined values from an object
 * @param obj - Object to clean
 * @returns Cleaned object
 */
export function cleanFilters<T extends Record<string, any>>(obj: T): Partial<T> {
  const cleaned: Partial<T> = {};

  Object.entries(obj).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      cleaned[key as keyof T] = value;
    }
  });

  return cleaned;
}

/**
 * Debounce function for search inputs
 * @param func - Function to debounce
 * @param wait - Wait time in milliseconds
 * @returns Debounced function
 */
export function debounce<T extends (...args: any[]) => any>(
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
 * Checks if any filters are active (non-empty)
 * @param filters - Filter object
 * @returns Boolean indicating if filters are active
 */
export function hasActiveFilters(filters: Record<string, any>): boolean {
  return Object.values(filters).some(value =>
    value !== undefined &&
    value !== null &&
    value !== '' &&
    !(Array.isArray(value) && value.length === 0)
  );
}

/**
 * Resets pagination to first page when filters change
 * @param filters - Current filters
 * @param previousFilters - Previous filters
 * @returns Boolean indicating if pagination should reset
 */
export function shouldResetPagination(
  filters: Record<string, any>,
  previousFilters: Record<string, any>
): boolean {
  // Compare all filter values except pagination-related ones
  const filterKeys = Object.keys(filters).filter(key =>
    !['pageNumber', 'pageSize', 'skip'].includes(key)
  );

  return filterKeys.some(key => filters[key] !== previousFilters[key]);
}

/**
 * Formats date for API consumption (ISO string)
 * @param date - Date object or string
 * @returns ISO string or undefined
 */
export function formatDateForApi(date: Date | string | undefined): string | undefined {
  if (!date) return undefined;

  if (typeof date === 'string') {
    return new Date(date).toISOString();
  }

  return date.toISOString();
}

/**
 * Parses date from API response
 * @param dateString - ISO date string
 * @returns Date object or undefined
 */
export function parseDateFromApi(dateString: string | undefined): Date | undefined {
  if (!dateString) return undefined;
  return new Date(dateString);
}

/**
 * Creates default pagination parameters
 * @param pageSize - Default page size
 * @returns Default pagination params
 */
export function createDefaultPagination(pageSize: number = 25) {
  return {
    pageNumber: 1,
    pageSize,
    sortBy: 'createdAt',
    sortDirection: 'desc',
    isDescending: true,
  };
}

/**
 * Validates pagination parameters
 * @param params - Pagination parameters
 * @returns Validated parameters
 */
export function validatePaginationParams(params: {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
  isDescending?: boolean;
}) {
  return {
    pageNumber: Math.max(1, params.pageNumber || 1),
    pageSize: Math.min(100, Math.max(1, params.pageSize || 25)),
    sortBy: params.sortBy || 'createdAt',
    sortDirection: params.sortDirection || 'desc',
    isDescending: params.isDescending ?? true,
  };
}

/**
 * Safely handles Select component values to avoid empty string errors
 * @param value - The current filter value
 * @param defaultValue - Default value to use when no selection (default: "all")
 * @returns Safe value for Select component
 */
export function getSelectValue(value: string | number | undefined | null, defaultValue: string = "all"): string {
  if (value === undefined || value === null || value === "") {
    return defaultValue;
  }
  return value.toString();
}

/**
 * Parses Select component value back to appropriate type
 * @param value - Value from Select component
 * @param defaultValue - Default value that represents "no selection"
 * @param type - Type to parse to ('string', 'number', 'float')
 * @returns Parsed value or undefined if default
 */
export function parseSelectValue(
  value: string,
  defaultValue: string = "all",
  type: 'string' | 'number' | 'float' = 'string'
): string | number | undefined {
  if (value === defaultValue) {
    return undefined;
  }

  switch (type) {
    case 'number':
      return parseInt(value);
    case 'float':
      return parseFloat(value);
    case 'string':
    default:
      return value;
  }
}
