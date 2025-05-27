/**
 * Shared types for API pagination, filtering, and search functionality
 */

// Pagination metadata from API
export interface PaginationMetadata {
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
  nextPage: number | null;
  previousPage: number | null;
}

// Generic paged result structure
export interface PagedResult<T> {
  items: T[];
  pagination: PaginationMetadata;
}

// Base pagination parameters
export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
  isDescending?: boolean;
  skip?: number;
}

// Base search and filter parameters
export interface BaseSearchParams {
  searchTerm?: string;
}

// Date range filter
export interface DateRangeFilter {
  from?: string;
  to?: string;
}

// Products specific filters
export interface ProductFilters extends BaseSearchParams, PaginationParams {
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  isActive?: boolean;
  createdAfter?: string;
  createdBefore?: string;
  stockQuantityMin?: number;
  stockQuantityMax?: number;
  minRating?: number;
  sku?: string;
}

// Categories specific filters
export interface CategoryFilters extends BaseSearchParams, PaginationParams {
  parentId?: number;
  isRootCategory?: boolean;
  includeSubcategories?: boolean;
  includeProductCount?: boolean;
  createdAfter?: string;
  createdBefore?: string;
  includeDeleted?: boolean;
}

// Customers specific filters
export interface CustomerFilters extends BaseSearchParams, PaginationParams {
  email?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  registeredAfter?: string;
  registeredBefore?: string;
  hasOrders?: boolean;
  includeOrderCount?: boolean;
  includeTotalSpent?: boolean;
  includeDeleted?: boolean;
}

// Orders specific filters
export interface OrderFilters extends BaseSearchParams, PaginationParams {
  userId?: number;
  customerId?: number;
  orderStatus?: string;
  paymentMethod?: string;
  minAmount?: number;
  maxAmount?: number;
  orderDateFrom?: string;
  orderDateTo?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingCountry?: string;
  includeDeleted?: boolean;
}

// Sort options
export interface SortOption {
  value: string;
  label: string;
}

// Filter state for UI components
export interface FilterState<T> {
  filters: T;
  isOpen: boolean;
  hasActiveFilters: boolean;
}

// Search state for UI components
export interface SearchState {
  query: string;
  isSearching: boolean;
}

// Pagination state for UI components
export interface PaginationState {
  currentPage: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

// Common filter options
export const PAGE_SIZE_OPTIONS = [
  { value: 10, label: "10 per page" },
  { value: 25, label: "25 per page" },
  { value: 50, label: "50 per page" },
  { value: 100, label: "100 per page" },
];

export const SORT_DIRECTIONS = [
  { value: "asc", label: "Ascending" },
  { value: "desc", label: "Descending" },
];

// Product specific options
export const PRODUCT_SORT_OPTIONS: SortOption[] = [
  { value: "name", label: "Name" },
  { value: "price", label: "Price" },
  { value: "stockQuantity", label: "Stock" },
  { value: "createdAt", label: "Created Date" },
  { value: "averageRating", label: "Rating" },
];

// Category specific options
export const CATEGORY_SORT_OPTIONS: SortOption[] = [
  { value: "name", label: "Name" },
  { value: "createdAt", label: "Created Date" },
];

// Customer specific options
export const CUSTOMER_SORT_OPTIONS: SortOption[] = [
  { value: "firstName", label: "First Name" },
  { value: "lastName", label: "Last Name" },
  { value: "email", label: "Email" },
  { value: "createdAt", label: "Registration Date" },
];

// Order specific options
export const ORDER_SORT_OPTIONS: SortOption[] = [
  { value: "orderDate", label: "Order Date" },
  { value: "totalAmount", label: "Total Amount" },
  { value: "orderStatus", label: "Status" },
];

export const ORDER_STATUS_OPTIONS = [
  { value: "pending", label: "Pending" },
  { value: "processing", label: "Processing" },
  { value: "shipped", label: "Shipped" },
  { value: "delivered", label: "Delivered" },
  { value: "cancelled", label: "Cancelled" },
];

export const PAYMENT_METHOD_OPTIONS = [
  { value: "credit_card", label: "Credit Card" },
  { value: "debit_card", label: "Debit Card" },
  { value: "paypal", label: "PayPal" },
  { value: "bank_transfer", label: "Bank Transfer" },
  { value: "cash_on_delivery", label: "Cash on Delivery" },
];
