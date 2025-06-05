"use client";

import { API_URL, fetchWithAuth, fetchWithAuthFormData } from "@/lib/api-utils";
import { buildApiUrl, cleanFilters } from "@/lib/query-utils";
import { ProductFilters, PagedResult } from "@/types/api";

// Image DTO from API spec
export interface ImageDTO {
  id: number;
  fileName: string;
  filePath: string;
  altText?: string;
  createdAt: string;
}

// Product interface matching API spec
export interface Product {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  price: number;
  originalPrice: number;
  stockQuantity: number;
  sku: string;
  categoryId: number;
  categoryName: string;
  isFeatured: boolean;
  isActive: boolean;
  averageRating: number;
  createdAt: string;
  updatedAt: string;
  images: string[] | null;
}

// ProductDTO with enhanced image details matching API spec
export interface ProductDTO extends Product {
  imageDetails?: ImageDTO[];
}

// CreateProductDTO matching API spec exactly
export interface CreateProductDTO {
  name: string; // 3-255 chars, required
  slug: string; // 0-255 chars, required
  description?: string; // nullable
  price: number; // min 0.01, required
  originalPrice?: number;
  stockQuantity: number; // required
  sku: string; // 0-50 chars, required
  categoryId: number; // required
  isFeatured?: boolean;
  isActive?: boolean;
  imageIds?: number[]; // nullable - for linking existing images
}

// UpdateProductDTO matching API spec exactly
export interface UpdateProductDTO {
  name: string; // 3-255 chars, required
  slug?: string; // 0-255 chars, nullable
  description?: string; // nullable
  price: number; // min 0.01, required
  originalPrice?: number;
  stockQuantity?: number;
  sku?: string; // 0-50 chars, nullable
  categoryId?: number;
  isFeatured?: boolean;
  isActive?: boolean;
  imageIds?: number[]; // nullable - for linking existing images
}

// Form payload that includes file uploads
export interface CreateProductPayload extends CreateProductDTO {
  images?: File[];
}

export interface UpdateProductPayload extends UpdateProductDTO {
  id: number;
  images?: File[];
}

export async function getProducts(filters?: ProductFilters): Promise<PagedResult<ProductDTO>> {
  try {
    const cleanedFilters = filters ? cleanFilters(filters) : {};
    const url = buildApiUrl(`${API_URL}/api/Products`, cleanedFilters);
    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch product list");
    }

    return response.json();
  } catch (error) {
    console.error("Get products error:", error);
    throw new Error("Unable to fetch product list. Please try again later.");
  }
}

export async function getAllProducts(): Promise<ProductDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/all`);

    if (!response.ok) {
      throw new Error("Unable to fetch product list");
    }

    return response.json();
  } catch (error) {
    console.error("Get all products error:", error);
    throw new Error("Unable to fetch product list. Please try again later.");
  }
}

export async function getProductById(id: number): Promise<Product> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/${id}`);

    if (!response.ok) {
      throw new Error("Unable to fetch product details");
    }

    return response.json();
  } catch (error) {
    console.error(`Get product by id ${id} error:`, error);
    throw new Error("Unable to fetch product details. Please try again later.");
  }
}

export async function createProduct(product: CreateProductPayload): Promise<Product> {
  try {
    const productData: CreateProductDTO = {
      name: product.name,
      slug: product.slug,
      description: product.description,
      price: product.price,
      originalPrice: product.originalPrice,
      stockQuantity: product.stockQuantity,
      sku: product.sku,
      categoryId: product.categoryId,
      isFeatured: product.isFeatured ?? false,
      isActive: product.isActive ?? true,
      imageIds: product.imageIds,
    };

    const response = await fetchWithAuth(`${API_URL}/api/Products`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(productData),
    });

    if (!response.ok) {
      throw new Error("Unable to create product");
    }
    return response.json();
  } catch (error) {
    console.error("Create product error:", error);
    throw new Error("Unable to create product. Please try again later.");
  }
}

export async function updateProduct(product: UpdateProductPayload): Promise<void> {
  try {
    const { id, ...updateData } = product;
    const productData: UpdateProductDTO = updateData;

    const response = await fetchWithAuth(`${API_URL}/api/Products/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(productData),
    });

    if (!response.ok) {
      throw new Error("Unable to update product");
    }
  } catch (error) {
    console.error(`Update product error:`, error);
    throw new Error("Unable to update product. Please try again later.");
  }
}

export async function deleteProduct(id: number): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/${id}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to delete product");
    }
  } catch (error) {
    console.error(`Delete product by id ${id} error:`, error);
    throw new Error("Unable to delete product. Please try again later.");
  }
}

export async function bulkDeleteProducts(ids: number[]): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/bulk`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ ids }),
    });

    if (!response.ok) {
      throw new Error("Unable to delete products");
    }
  } catch (error) {
    console.error(`Bulk delete products error:`, error);
    throw new Error("Unable to delete products. Please try again later.");
  }
}


export async function addProductImages(productId: number, images: File[]): Promise<void> {
  try {
    const formData = new FormData();
    images.forEach((image) => {
      formData.append("Images", image);
    });

    const response = await fetchWithAuthFormData(`${API_URL}/api/Products/${productId}/images`, formData);

    if (!response.ok) {
      throw new Error("Unable to add product images");
    }
  } catch (error) {
    console.error(`Add product images error:`, error);
    throw new Error("Unable to add product images. Please try again later.");
  }
}

export async function deleteProductImage(productId: number, imageId: number): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/${productId}/images/${imageId}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to delete product image");
    }
  } catch (error) {
    console.error(`Delete product image error:`, error);
    throw new Error("Unable to delete product image. Please try again later.");
  }
}

// Additional API endpoints from specification

export async function getProductsByCategory(categoryId: number, count: number = 20): Promise<ProductDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/category/${categoryId}?count=${count}`);

    if (!response.ok) {
      throw new Error("Unable to fetch products by category");
    }

    return response.json();
  } catch (error) {
    console.error(`Get products by category ${categoryId} error:`, error);
    throw new Error("Unable to fetch products by category. Please try again later.");
  }
}

export async function getFeaturedProducts(count: number = 10): Promise<ProductDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/featured?count=${count}`);

    if (!response.ok) {
      throw new Error("Unable to fetch featured products");
    }

    return response.json();
  } catch (error) {
    console.error("Get featured products error:", error);
    throw new Error("Unable to fetch featured products. Please try again later.");
  }
}

export async function getTopRatedProducts(count: number = 10): Promise<ProductDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/top-rated?count=${count}`);

    if (!response.ok) {
      throw new Error("Unable to fetch top rated products");
    }

    return response.json();
  } catch (error) {
    console.error("Get top rated products error:", error);
    throw new Error("Unable to fetch top rated products. Please try again later.");
  }
}

export async function getLowStockProducts(threshold: number = 10): Promise<ProductDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/low-stock?threshold=${threshold}`);

    if (!response.ok) {
      throw new Error("Unable to fetch low stock products");
    }

    return response.json();
  } catch (error) {
    console.error("Get low stock products error:", error);
    throw new Error("Unable to fetch low stock products. Please try again later.");
  }
}

export async function getRelatedProducts(id: number, count: number = 5): Promise<ProductDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products/${id}/related?count=${count}`);

    if (!response.ok) {
      throw new Error("Unable to fetch related products");
    }

    return response.json();
  } catch (error) {
    console.error(`Get related products for ${id} error:`, error);
    throw new Error("Unable to fetch related products. Please try again later.");
  }
}
