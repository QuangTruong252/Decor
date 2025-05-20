"use client";

import { API_URL, fetchWithAuth, fetchWithAuthFormData } from "@/lib/api-utils";

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

export interface CreateProductPayload {
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

export interface UpdateProductPayload extends Partial<CreateProductPayload> {
  id: number;
}

export async function getProducts(): Promise<Product[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Products`);

    if (!response.ok) {
      throw new Error("Unable to fetch product list");
    }

    return response.json();
  } catch (error) {
    console.error("Get products error:", error);
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
    // Create FormData to send both data and images
    const formData = new FormData();

    // Add product information fields to FormData
    if (product.images && product.images.length > 0) {
      product.images.forEach((image) => {
        formData.append("Images", image);
      });
    }

    // Build URL with query params
    let url = `${API_URL}/api/Products?`;
    url += `Name=${encodeURIComponent(product.name)}`;
    url += `&Slug=${encodeURIComponent(product.slug)}`;
    url += `&Price=${product.price}`;
    url += `&StockQuantity=${product.stockQuantity}`;
    url += `&SKU=${encodeURIComponent(product.sku)}`;
    url += `&CategoryId=${product.categoryId}`;

    if (product.description) {
      url += `&Description=${encodeURIComponent(product.description)}`;
    }

    if (product.originalPrice) {
      url += `&OriginalPrice=${product.originalPrice}`;
    }

    if (product.isFeatured !== undefined) {
      url += `&IsFeatured=${product.isFeatured}`;
    }

    if (product.isActive !== undefined) {
      url += `&IsActive=${product.isActive}`;
    }

    const response = await fetchWithAuthFormData(url, formData);

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
    // Create FormData to send both data and images
    const formData = new FormData();

    // Add product information fields to FormData
    if (product.images && product.images.length > 0) {
      product.images.forEach((image) => {
        formData.append("Images", image);
      });
    }

    // Build URL with query params
    let url = `${API_URL}/api/Products/${product.id}?`;

    if (product.name) {
      url += `Name=${encodeURIComponent(product.name)}`;
    }

    if (product.slug) {
      url += `&Slug=${encodeURIComponent(product.slug)}`;
    }

    if (product.price !== undefined) {
      url += `&Price=${product.price}`;
    }

    if (product.stockQuantity !== undefined) {
      url += `&StockQuantity=${product.stockQuantity}`;
    }

    if (product.sku) {
      url += `&SKU=${encodeURIComponent(product.sku)}`;
    }

    if (product.categoryId !== undefined) {
      url += `&CategoryId=${product.categoryId}`;
    }

    if (product.description) {
      url += `&Description=${encodeURIComponent(product.description)}`;
    }

    if (product.originalPrice !== undefined) {
      url += `&OriginalPrice=${product.originalPrice}`;
    }

    if (product.isFeatured !== undefined) {
      url += `&IsFeatured=${product.isFeatured}`;
    }

    if (product.isActive !== undefined) {
      url += `&IsActive=${product.isActive}`;
    }

    const response = await fetchWithAuthFormData(url, formData, "PUT");

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