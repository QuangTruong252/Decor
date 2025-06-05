"use client";

import { API_URL, fetchWithAuth } from "@/lib/api-utils";
import { buildApiUrl, cleanFilters } from "@/lib/query-utils";
import { CategoryFilters, PagedResult } from "@/types/api";

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
 * Category DTO with additional computed fields
 */
export interface CategoryDTO extends Category {
  subcategories?: CategoryDTO[];
  parentName?: string;
  productCount?: number;
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
  imageId?: number;
}

/**
 * Update category request DTO
 */
export interface UpdateCategoryPayload extends Partial<CreateCategoryPayload> {
  id: number;
}

/**
 * Get categories with pagination and filtering
 * @param filters Category filters
 * @returns Paged result of categories
 * @endpoint GET /api/Category
 */
export async function getCategories(filters?: CategoryFilters): Promise<PagedResult<CategoryDTO>> {
  try {
    const cleanedFilters = filters ? cleanFilters(filters) : {};
    const url = buildApiUrl(`${API_URL}/api/Category`, cleanedFilters);
    const response = await fetchWithAuth(url);

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
 * Get all categories without pagination
 * @returns List of all categories
 * @endpoint GET /api/Category/all
 */
export async function getAllCategories(): Promise<CategoryDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/all`);

    if (!response.ok) {
      throw new Error("Unable to fetch categories");
    }

    return response.json();
  } catch (error) {
    console.error("Get all categories error:", error);
    throw new Error("Unable to fetch categories. Please try again later.");
  }
}

/**
 * Get hierarchical categories structure
 * @returns Hierarchical list of categories with subcategories
 * @endpoint GET /api/Category/hierarchical
 */
export async function getHierarchicalCategories(): Promise<CategoryDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/hierarchical`);

    if (!response.ok) {
      throw new Error("Unable to fetch hierarchical categories");
    }

    return response.json();
  } catch (error) {
    console.error("Get hierarchical categories error:", error);
    throw new Error("Unable to fetch hierarchical categories. Please try again later.");
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
    const categoryData = {
      name: category.name,
      slug: category.slug,
      description: category.description,
      parentId: category.parentId,
      isActive: category.isActive ?? true,
      displayOrder: category.displayOrder ?? 0,
      imageId: category.imageId,
    };

    const response = await fetchWithAuth(`${API_URL}/api/Category`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(categoryData),
    });

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
    const categoryData: any = {};

    if (category.name) {
      categoryData.name = category.name;
    }
    if (category.slug) {
      categoryData.slug = category.slug;
    }
    if (category.description !== undefined) {
      categoryData.description = category.description;
    }
    if (category.parentId !== undefined) {
      categoryData.parentId = category.parentId;
    }
    if (category.isActive !== undefined) {
      categoryData.isActive = category.isActive;
    }
    if (category.displayOrder !== undefined) {
      categoryData.displayOrder = category.displayOrder;
    }
    if (category.imageId !== undefined) {
      categoryData.imageId = category.imageId;
    }

    const response = await fetchWithAuth(`${API_URL}/api/Category/${category.id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(categoryData),
    });

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

/**
 * Get category by slug
 * @param slug Category slug
 * @returns Category details
 * @endpoint GET /api/Category/slug/{slug}
 */
export async function getCategoryBySlug(slug: string): Promise<Category> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/slug/${encodeURIComponent(slug)}`);

    if (!response.ok) {
      throw new Error("Unable to fetch category");
    }

    return response.json();
  } catch (error) {
    console.error(`Get category by slug ${slug} error:`, error);
    throw new Error("Unable to fetch category. Please try again later.");
  }
}

/**
 * Get categories with product count
 * @returns List of categories with product counts
 * @endpoint GET /api/Category/with-product-count
 */
export async function getCategoriesWithProductCount(): Promise<CategoryDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/with-product-count`);

    if (!response.ok) {
      throw new Error("Unable to fetch categories with product count");
    }

    return response.json();
  } catch (error) {
    console.error("Get categories with product count error:", error);
    throw new Error("Unable to fetch categories with product count. Please try again later.");
  }
}

/**
 * Get subcategories
 * @param parentId Parent category ID
 * @returns List of subcategories
 * @endpoint GET /api/Category/{parentId}/subcategories
 */
export async function getSubcategories(parentId: number): Promise<CategoryDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/${parentId}/subcategories`);

    if (!response.ok) {
      throw new Error("Unable to fetch subcategories");
    }

    return response.json();
  } catch (error) {
    console.error(`Get subcategories for parent ${parentId} error:`, error);
    throw new Error("Unable to fetch subcategories. Please try again later.");
  }
}

/**
 * Get category product count
 * @param categoryId Category ID
 * @returns Product count for the category
 * @endpoint GET /api/Category/{categoryId}/product-count
 */
export async function getCategoryProductCount(categoryId: number): Promise<number> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/${categoryId}/product-count`);

    if (!response.ok) {
      throw new Error("Unable to fetch category product count");
    }

    return response.json();
  } catch (error) {
    console.error(`Get category product count for ${categoryId} error:`, error);
    throw new Error("Unable to fetch category product count. Please try again later.");
  }
}

/**
 * Get popular categories
 * @returns List of popular categories
 * @endpoint GET /api/Category/popular
 */
export async function getPopularCategories(): Promise<CategoryDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/popular`);

    if (!response.ok) {
      throw new Error("Unable to fetch popular categories");
    }

    return response.json();
  } catch (error) {
    console.error("Get popular categories error:", error);
    throw new Error("Unable to fetch popular categories. Please try again later.");
  }
}

/**
 * Get root categories
 * @returns List of root categories
 * @endpoint GET /api/Category/root
 */
export async function getRootCategories(): Promise<CategoryDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Category/root`);

    if (!response.ok) {
      throw new Error("Unable to fetch root categories");
    }

    return response.json();
  } catch (error) {
    console.error("Get root categories error:", error);
    throw new Error("Unable to fetch root categories. Please try again later.");
  }
}
