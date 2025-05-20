"use client";

import { API_URL, fetchWithAuth, fetchWithAuthFormData } from "@/lib/api-utils";

/**
 * Category response DTO from API
 */
export interface Category {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  parentId: number | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  // Additional fields from MCP
  imageUrl: string | null;
  displayOrder: number;
  products?: Product[];
  childCategories?: Category[];
  parentCategory?: Category | null;
}

/**
 * Product summary DTO for category relationships
 */
export interface Product {
  id: number;
  name: string;
  slug: string;
  price: number;
  imageUrl: string | null;
}

/**
 * Create category request DTO
 */
export interface CreateCategoryPayload {
  name: string;
  slug: string;
  description?: string;
  parentId?: number | null;
  isActive?: boolean;
  displayOrder?: number;
  image?: File;
}

/**
 * Update category request DTO
 */
export interface UpdateCategoryPayload extends Partial<CreateCategoryPayload> {
  id: number;
}

/**
 * Get all categories
 * @returns List of categories
 * @endpoint GET /api/Category
 */
export async function getCategories(): Promise<Category[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category`);

    if (!response.ok) {
      throw new Error("Unable to fetch categories");
    }

    return response.json();
  } catch (error) {
    console.error("Get categories error:", error);
    throw new Error("Unable to fetch categories. Please try again later.");
  }
}

/**
 * Get category by ID
 * @param id Category ID
 * @returns Category details
 * @endpoint GET /api/Category/{id}
 */
export async function getCategoryById(id: number): Promise<Category> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/${id}`);

    if (!response.ok) {
      throw new Error("Unable to fetch category");
    }

    return response.json();
  } catch (error) {
    console.error(`Get category by id ${id} error:`, error);
    throw new Error("Unable to fetch category. Please try again later.");
  }
}

/**
 * Create a new category
 * @param category Category data
 * @returns Created category
 * @endpoint POST /api/Category
 */
export async function createCategory(category: CreateCategoryPayload): Promise<Category> {
  try {
    // Create FormData to send both data and image
    const formData = new FormData();

    // Add image if provided
    if (category.image) {
      formData.append("Image", category.image);
    }

    // Build URL with query params
    let url = `${API_URL}/api/Category?`;
    url += `Name=${encodeURIComponent(category.name)}`;
    url += `&Slug=${encodeURIComponent(category.slug)}`;

    if (category.description) {
      url += `&Description=${encodeURIComponent(category.description)}`;
    }

    if (category.parentId !== undefined) {
      url += `&ParentId=${category.parentId}`;
    }

    if (category.isActive !== undefined) {
      url += `&IsActive=${category.isActive}`;
    }

    if (category.displayOrder !== undefined) {
      url += `&DisplayOrder=${category.displayOrder}`;
    }

    const response = await fetchWithAuthFormData(url, formData);

    if (!response.ok) {
      throw new Error("Unable to create category");
    }

    return response.json();
  } catch (error) {
    console.error("Create category error:", error);
    throw new Error("Unable to create category. Please try again later.");
  }
}

/**
 * Update an existing category
 * @param category Category data with ID
 * @returns void
 * @endpoint PUT /api/Category/{id}
 */
export async function updateCategory(category: UpdateCategoryPayload): Promise<void> {
  try {
    // Create FormData to send both data and image
    const formData = new FormData();

    // Add image if provided
    if (category.image) {
      formData.append("Image", category.image);
    }

    // Build URL with query params
    let url = `${API_URL}/api/Category/${category.id}?`;

    if (category.name) {
      url += `Name=${encodeURIComponent(category.name)}`;
    }

    if (category.slug) {
      url += `&Slug=${encodeURIComponent(category.slug)}`;
    }

    if (category.description !== undefined) {
      url += `&Description=${encodeURIComponent(category.description || '')}`;
    }

    if (category.parentId !== undefined) {
      url += `&ParentId=${category.parentId}`;
    }

    if (category.isActive !== undefined) {
      url += `&IsActive=${category.isActive}`;
    }

    if (category.displayOrder !== undefined) {
      url += `&DisplayOrder=${category.displayOrder}`;
    }

    const response = await fetchWithAuthFormData(url, formData, "PUT");

    if (!response.ok) {
      throw new Error("Unable to update category");
    }
  } catch (error) {
    console.error(`Update category error:`, error);
    throw new Error("Unable to update category. Please try again later.");
  }
}

/**
 * Delete a category
 * @param id Category ID
 * @returns void
 * @endpoint DELETE /api/Category/{id}
 */
export async function deleteCategory(id: number): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/${id}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to delete category");
    }
  } catch (error) {
    console.error(`Delete category by id ${id} error:`, error);
    throw new Error("Unable to delete category. Please try again later.");
  }
}

/**
 * Get products by category ID
 * @param categoryId Category ID
 * @returns List of products in the category
 * @endpoint GET /api/Category/{id}/Products
 */
export async function getProductsByCategory(categoryId: number): Promise<Product[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/${categoryId}/Products`);

    if (!response.ok) {
      throw new Error("Unable to fetch products for this category");
    }

    return response.json();
  } catch (error) {
    console.error(`Get products by category ${categoryId} error:`, error);
    throw new Error("Unable to fetch products for this category. Please try again later.");
  }
}
