import { api } from '../client';
import { PRODUCTS } from '../endpoints';
import type {
  ProductDTO,
  ProductDTOPagedResult,
  CreateProductDTO,
  UpdateProductDTO,
  ProductFilters,
  BulkDeleteDTO,
} from '../types';

export class ProductService {
  /**
   * Get products with pagination and filtering
   */
  static async getProducts(params?: ProductFilters): Promise<ProductDTOPagedResult> {
    const response = await api.get<ProductDTOPagedResult>(PRODUCTS.BASE, { params });
    return response.data;
  }

  /**
   * Get all products without pagination
   */
  static async getAllProducts(): Promise<ProductDTO[]> {
    const response = await api.get<ProductDTO[]>(PRODUCTS.ALL);
    return response.data;
  }

  /**
   * Get product by ID
   */
  static async getProductById(id: number): Promise<ProductDTO> {
    const response = await api.get<ProductDTO>(PRODUCTS.BY_ID(id));
    return response.data;
  }

  /**
   * Get products by category
   */
  static async getProductsByCategory(categoryId: number, params?: ProductFilters): Promise<ProductDTOPagedResult> {
    const response = await api.get<ProductDTOPagedResult>(PRODUCTS.BY_CATEGORY(categoryId), { params });
    return response.data;
  }

  /**
   * Get featured products
   */
  static async getFeaturedProducts(): Promise<ProductDTO[]> {
    const response = await api.get<ProductDTO[]>(PRODUCTS.FEATURED);
    return response.data;
  }

  /**
   * Get top-rated products
   */
  static async getTopRatedProducts(count?: number): Promise<ProductDTO[]> {
    const params = count ? { count } : undefined;
    const response = await api.get<ProductDTO[]>(PRODUCTS.TOP_RATED, { params });
    return response.data;
  }

  /**
   * Get low stock products
   */
  static async getLowStockProducts(threshold?: number): Promise<ProductDTO[]> {
    const params = threshold ? { threshold } : undefined;
    const response = await api.get<ProductDTO[]>(PRODUCTS.LOW_STOCK, { params });
    return response.data;
  }

  /**
   * Get related products
   */
  static async getRelatedProducts(id: number): Promise<ProductDTO[]> {
    const response = await api.get<ProductDTO[]>(PRODUCTS.RELATED(id));
    return response.data;
  }

  /**
   * Create new product
   */
  static async createProduct(productData: CreateProductDTO): Promise<ProductDTO> {
    const formData = new FormData();

    // Add product data as query parameters
    const params = new URLSearchParams();
    params.append('Name', productData.name);
    params.append('Slug', productData.slug);
    params.append('Price', productData.price.toString());
    params.append('StockQuantity', productData.stockQuantity.toString());
    params.append('SKU', productData.sku);
    params.append('CategoryId', productData.categoryId.toString());

    if (productData.description) params.append('Description', productData.description);
    if (productData.originalPrice) params.append('OriginalPrice', productData.originalPrice.toString());
    if (productData.isFeatured !== undefined) params.append('IsFeatured', productData.isFeatured.toString());
    if (productData.isActive !== undefined) params.append('IsActive', productData.isActive.toString());

    // Add images to form data
    if (productData.images) {
      productData.images.forEach((image: File) => {
        formData.append('Images', image);
      });
    }

    const response = await api.post<ProductDTO>(
      `${PRODUCTS.BASE}?${params.toString()}`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  }

  /**
   * Update product
   */
  static async updateProduct(id: number, productData: UpdateProductDTO): Promise<void> {
    const formData = new FormData();

    // Add product data as query parameters
    const params = new URLSearchParams();
    if (productData.name) params.append('Name', productData.name);
    if (productData.slug) params.append('Slug', productData.slug);
    if (productData.price) params.append('Price', productData.price.toString());
    if (productData.stockQuantity) params.append('StockQuantity', productData.stockQuantity.toString());
    if (productData.sku) params.append('SKU', productData.sku);
    if (productData.categoryId) params.append('CategoryId', productData.categoryId.toString());
    if (productData.description) params.append('Description', productData.description);
    if (productData.originalPrice) params.append('OriginalPrice', productData.originalPrice.toString());
    if (productData.isFeatured !== undefined) params.append('IsFeatured', productData.isFeatured.toString());
    if (productData.isActive !== undefined) params.append('IsActive', productData.isActive.toString());

    // Add images to form data
    if (productData.images) {
      productData.images.forEach((image: File) => {
        formData.append('Images', image);
      });
    }

    await api.put(
      `${PRODUCTS.BY_ID(id)}?${params.toString()}`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
  }

  /**
   * Delete product
   */
  static async deleteProduct(id: number): Promise<void> {
    await api.delete(PRODUCTS.BY_ID(id));
  }

  /**
   * Bulk delete products
   */
  static async bulkDeleteProducts(ids: number[]): Promise<void> {
    const data: BulkDeleteDTO = { ids };
    await api.delete(PRODUCTS.BULK_DELETE, { data });
  }

  /**
   * Add images to product
   */
  static async addProductImages(id: number, images: File[]): Promise<void> {
    const formData = new FormData();
    images.forEach((image) => {
      formData.append('Images', image);
    });

    await api.post(PRODUCTS.IMAGES(id), formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  /**
   * Delete product image
   */
  static async deleteProductImage(productId: number, imageId: number): Promise<void> {
    await api.delete(PRODUCTS.DELETE_IMAGE(productId, imageId));
  }

  /**
   * Search products with filters
   */
  static async searchProducts(searchTerm: string, filters?: ProductFilters): Promise<ProductDTOPagedResult> {
    const params = {
      searchTerm,
      ...filters,
    };
    return this.getProducts(params);
  }

  /**
   * Get products in stock
   */
  static async getInStockProducts(params?: ProductFilters): Promise<ProductDTOPagedResult> {
    return this.getProducts({
      ...params,
      stockQuantityMin: 1
    });
  }
}

export default ProductService;
