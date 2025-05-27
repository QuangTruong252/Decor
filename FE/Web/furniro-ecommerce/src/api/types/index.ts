// Export all API types from a single entry point

// Common types
export * from './common';

// API pagination and filtering types
export * from './api';

// Authentication types
export * from './auth';

// Product types
export * from './product';

// Category types
export * from './category';

// Cart types
export * from './cart';

// Order types
export * from './order';

// Customer types
export * from './customer';

// Review types
export * from './review';

// Dashboard types
export * from './dashboard';

// Banner types
export * from './banner';

// Re-export commonly used types for convenience
export type {
  // Common
  ApiResponse,
  ApiError,
  BaseEntity,
  BulkDeleteDTO,
  ImageDTO,
} from './common';

export type {
  // API Pagination and Filtering
  PaginationMetadata,
  PagedResult,
  PaginationParams,
  ProductFilters,
  CategoryFilters,
  CustomerFilters,
  OrderFilters,
} from './api';

export type {
  // Auth
  UserDTO,
  LoginDTO,
  RegisterDTO,
  AuthResponseDTO,
  AuthState,
} from './auth';

export type {
  // Product
  ProductDTO,
  ProductDTOPagedResult,
  CreateProductDTO,
  UpdateProductDTO,
  ProductListItem,
  ProductDetail,
  ProductSearchParams
} from './product';

export type {
  // Category
  CategoryDTO,
  CategoryDTOPagedResult,
  CreateCategoryDTO,
  UpdateCategoryDTO,
  CategoryTreeNode,
} from './category';

export type {
  // Cart
  CartDTO,
  CartItemDTO,
  AddToCartDTO,
  UpdateCartItemDTO,
  Cart,
  CartItem,
} from './cart';

export type {
  // Order
  OrderDTO,
  OrderDTOPagedResult,
  OrderItemDTO,
  CreateOrderDTO,
  CreateOrderItemDTO,
  UpdateOrderStatusDTO,
  Order,
  OrderStatus,
  PaymentMethod,
} from './order';

export type {
  // Customer
  CustomerDTO,
  CustomerDTOPagedResult,
  CreateCustomerDTO,
  UpdateCustomerDTO,
  Customer,
  CustomerAddress,
} from './customer';

export type {
  // Review
  ReviewDTO,
  CreateReviewDTO,
  UpdateReviewDTO,
  Review,
  ReviewStats,
} from './review';

export type {
  // Dashboard
  DashboardSummaryDTO,
  RecentOrderDTO,
  PopularProductDTO,
  CategorySalesDTO,
  OrderStatusDistributionDTO,
  SalesTrendDTO,
  SalesTrendPointDTO,
  SalesTrendParams,
  PopularProductsParams,
} from './dashboard';

export type {
  // Banner
  BannerDTO,
  CreateBannerDTO,
  UpdateBannerDTO,
  Banner,
  BannerFormData,
} from './banner';
