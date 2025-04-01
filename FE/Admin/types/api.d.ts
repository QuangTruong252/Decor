declare module '~/api-services' {
  import { 
    ProductsApi as _ProductsApi,
    CategoryApi as _CategoryApi,
    OrderApi as _OrderApi,
    AuthApi as _AuthApi,
    BannerApi as _BannerApi,
    ReviewApi as _ReviewApi,
    HealthCheckApi as _HealthCheckApi
  } from '/api-services/api';

  // Re-export thực thể
  export const ProductsApi: typeof _ProductsApi;
  export const CategoryApi: typeof _CategoryApi;
  export const OrderApi: typeof _OrderApi;
  export const AuthApi: typeof _AuthApi;
  export const BannerApi: typeof _BannerApi;
  export const ReviewApi: typeof _ReviewApi;
  export const HealthCheckApi: typeof _HealthCheckApi;

  // Re-export các DTO interfaces
  export interface ProductDTO {
    id?: number;
    name?: string | null;
    slug?: string | null;
    description?: string | null;
    price?: number;
    originalPrice?: number;
    stockQuantity?: number;
    sku?: string | null;
    categoryId?: number;
    categoryName?: string | null;
    isFeatured?: boolean;
    isActive?: boolean;
    averageRating?: number;
    createdAt?: string;
    updatedAt?: string;
    imageUrl?: string | null;
    images?: Array<ProductImageDTO> | null;
  }

  export interface ProductImageDTO {
    id?: number;
    productId?: number;
    imageUrl?: string | null;
    isDefault?: boolean;
  }

  export interface CategoryDTO {
    id?: number;
    name?: string | null;
    slug?: string | null;
    description?: string | null;
    parentId?: number | null;
    parentName?: string | null;
    imageUrl?: string | null;
    createdAt?: string;
    subcategories?: Array<CategoryDTO> | null;
  }

  export interface OrderDTO {
    id?: number;
    userId?: number;
    userFullName?: string | null;
    totalAmount?: number;
    orderStatus?: string | null;
    paymentMethod?: string | null;
    shippingAddress?: string | null;
    orderDate?: string;
    updatedAt?: string;
    orderItems?: Array<OrderItemDTO> | null;
  }

  export interface OrderItemDTO {
    id?: number;
    orderId?: number;
    productId?: number;
    productName?: string | null;
    productImageUrl?: string | null;
    quantity?: number;
    unitPrice?: number;
    subtotal?: number;
  }

  export interface UserDTO {
    id?: number;
    username?: string | null;
    email?: string | null;
    role?: string | null;
  }

  export interface BannerDTO {
    id?: number;
    title?: string | null;
    imageUrl?: string | null;
    link?: string | null;
    isActive?: boolean;
    displayOrder?: number;
    createdAt?: string;
  }

  export interface ReviewDTO {
    id?: number;
    userId?: number;
    userName?: string | null;
    productId?: number;
    rating?: number;
    comment?: string | null;
    createdAt?: string;
  }
}

declare module '~/api-services/api-service' {
  export const apiService: {
    productsApi: import('~/api-services').ProductsApi;
    categoryApi: import('~/api-services').CategoryApi;
    orderApi: import('~/api-services').OrderApi;
    authApi: import('~/api-services').AuthApi;
    bannerApi: import('~/api-services').BannerApi;
    reviewApi: import('~/api-services').ReviewApi;
    healthCheckApi: import('~/api-services').HealthCheckApi;
  };

  export function handleApiError(error: unknown, context?: string): void;
  export function showSuccessToast(message: string): void;
  export function showErrorToast(message: string): void;
} 