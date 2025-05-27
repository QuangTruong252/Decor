import { BaseEntity, ImageDTO } from './common';
import { PagedResult } from './api';

// Product related types
export interface ProductDTO extends BaseEntity {
  name: string | null;
  slug: string | null;
  description: string | null;
  price: number;
  originalPrice: number;
  stockQuantity: number;
  sku: string | null;
  categoryId: number;
  categoryName: string | null;
  isFeatured: boolean;
  isActive: boolean;
  averageRating: number;
  images: string[] | null;
  imageDetails: ImageDTO[] | null;
}

// Paged result for products
export interface ProductDTOPagedResult extends PagedResult<ProductDTO> {}

export interface CreateProductDTO {
  name: string;
  slug: string;
  description?: string;
  price: number;
  originalPrice?: number;
  stockQuantity: number;
  sku: string;
  categoryId: number;
  isFeatured?: boolean;
  isActive?: boolean;
  images?: File[];
}

export interface UpdateProductDTO {
  name?: string;
  slug?: string;
  description?: string;
  price?: number;
  originalPrice?: number;
  stockQuantity?: number;
  sku?: string;
  categoryId?: number;
  isFeatured?: boolean;
  isActive?: boolean;
  images?: File[];
}

// Note: ProductFilters is now defined in api.ts for consistency with API specification

export interface ProductImage {
  id: number;
  url: string;
  filename: string;
  size: number;
  mimeType: string;
  isPrimary: boolean;
  productId: number;
}

export interface ProductVariant {
  id: number;
  name: string;
  value: string;
  price?: number;
  stockQuantity?: number;
  sku?: string;
  productId: number;
}

export interface ProductReview {
  id: number;
  userId: number;
  userName: string;
  rating: number;
  comment: string;
  createdAt: string;
  productId: number;
}

export interface ProductStats {
  totalViews: number;
  totalSales: number;
  averageRating: number;
  totalReviews: number;
  stockStatus: 'in_stock' | 'low_stock' | 'out_of_stock';
}

// Product form types
export interface ProductFormData extends Omit<CreateProductDTO, 'images'> {
  images: File[];
  existingImages?: string[];
}

// Product list item for display
export interface ProductListItem {
  id: number;
  name: string;
  slug: string;
  price: number;
  originalPrice?: number;
  image: string;
  categoryName: string;
  averageRating: number;
  stockQuantity: number;
  isFeatured: boolean;
}

// Product detail for single product page
export interface ProductDetail extends ProductDTO {
  reviews: ProductReview[];
  variants?: ProductVariant[];
  relatedProducts?: ProductListItem[];
  stats: ProductStats;
}

// Hooks return types
export interface UseProductsReturn {
  products: ProductDTO[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  hasMore: boolean;
  loadMore: () => void;
}

export interface UseProductReturn {
  product: ProductDetail | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export interface ProductSearchParams {
  searchTerm?: string;
  query?: string; // For backward compatibility
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  featured?: boolean; // For backward compatibility
  isActive?: boolean;
  inStock?: boolean; // For backward compatibility
  stockQuantityMin?: number;
  pageNumber?: number;
  page?: number; // For backward compatibility
  pageSize?: number;
  limit?: number; // For backward compatibility
  sortBy?: string;
  sortDirection?: string;
  sortOrder?: string; // For backward compatibility
  isDescending?: boolean;
}
