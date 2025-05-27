import { api } from '../client';
import { CATEGORIES } from '../endpoints';
import type {
  CategoryDTO,
  CategoryDTOPagedResult,
  CreateCategoryDTO,
  UpdateCategoryDTO,
  CategoryFilters,
} from '../types';

export class CategoryService {
  /**
   * Get categories with pagination and filtering
   */
  static async getCategories(params?: CategoryFilters): Promise<CategoryDTOPagedResult> {
    const response = await api.get<CategoryDTOPagedResult>(CATEGORIES.BASE, { params });
    return response.data;
  }

  /**
   * Get all categories without pagination
   */
  static async getAllCategories(): Promise<CategoryDTO[]> {
    const response = await api.get<CategoryDTO[]>(CATEGORIES.ALL);
    return response.data;
  }

  /**
   * Get hierarchical categories
   */
  static async getHierarchicalCategories(): Promise<CategoryDTO[]> {
    const response = await api.get<CategoryDTO[]>(CATEGORIES.HIERARCHICAL);
    return response.data;
  }

  /**
   * Get category by ID
   */
  static async getCategoryById(id: number): Promise<CategoryDTO> {
    const response = await api.get<CategoryDTO>(CATEGORIES.BY_ID(id));
    return response.data;
  }

  /**
   * Get category by slug
   */
  static async getCategoryBySlug(slug: string): Promise<CategoryDTO> {
    const response = await api.get<CategoryDTO>(CATEGORIES.BY_SLUG(slug));
    return response.data;
  }

  /**
   * Create new category
   */
  static async createCategory(categoryData: CreateCategoryDTO): Promise<CategoryDTO> {
    const formData = new FormData();

    formData.append('Name', categoryData.name);
    formData.append('Slug', categoryData.slug);

    if (categoryData.description) {
      formData.append('Description', categoryData.description);
    }

    if (categoryData.parentId) {
      formData.append('ParentId', categoryData.parentId.toString());
    }

    if (categoryData.imageFile) {
      formData.append('ImageFile', categoryData.imageFile);
    }

    const response = await api.post<CategoryDTO>(CATEGORIES.BASE, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  /**
   * Update category
   */
  static async updateCategory(id: number, categoryData: UpdateCategoryDTO): Promise<void> {
    const formData = new FormData();

    if (categoryData.name) {
      formData.append('Name', categoryData.name);
    }

    if (categoryData.slug) {
      formData.append('Slug', categoryData.slug);
    }

    if (categoryData.description) {
      formData.append('Description', categoryData.description);
    }

    if (categoryData.parentId) {
      formData.append('ParentId', categoryData.parentId.toString());
    }

    if (categoryData.imageFile) {
      formData.append('ImageFile', categoryData.imageFile);
    }

    await api.put(CATEGORIES.BY_ID(id), formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  /**
   * Delete category
   */
  static async deleteCategory(id: number): Promise<void> {
    await api.delete(CATEGORIES.BY_ID(id));
  }

  /**
   * Get root categories (categories without parent)
   */
  static async getRootCategories(): Promise<CategoryDTO[]> {
    const response = await api.get<CategoryDTO[]>(CATEGORIES.ROOT);
    return response.data;
  }

  /**
   * Get subcategories of a parent category
   */
  static async getSubcategories(parentId: number): Promise<CategoryDTO[]> {
    const response = await api.get<CategoryDTO[]>(CATEGORIES.SUBCATEGORIES(parentId));
    return response.data;
  }

  /**
   * Build category tree from flat list
   */
  static buildCategoryTree(categories: CategoryDTO[]): CategoryDTO[] {
    const categoryMap = new Map<number, CategoryDTO>();
    const rootCategories: CategoryDTO[] = [];

    // Create a map of all categories
    categories.forEach(category => {
      categoryMap.set(category.id, { ...category, subcategories: [] });
    });

    // Build the tree structure
    categories.forEach(category => {
      const categoryNode = categoryMap.get(category.id)!;

      if (category.parentId) {
        const parent = categoryMap.get(category.parentId);
        if (parent) {
          if (!parent.subcategories) {
            parent.subcategories = [];
          }
          parent.subcategories.push(categoryNode);
        }
      } else {
        rootCategories.push(categoryNode);
      }
    });

    return rootCategories;
  }

  /**
   * Get category breadcrumbs
   */
  static async getCategoryBreadcrumbs(categoryId: number): Promise<CategoryDTO[]> {
    const categories = await this.getAllCategories();
    const breadcrumbs: CategoryDTO[] = [];

    let currentCategory = categories.find(cat => cat.id === categoryId);

    while (currentCategory) {
      breadcrumbs.unshift(currentCategory);

      if (currentCategory.parentId) {
        currentCategory = categories.find(cat => cat.id === currentCategory!.parentId);
      } else {
        break;
      }
    }

    return breadcrumbs;
  }

  /**
   * Search categories by name
   */
  static async searchCategories(query: string): Promise<CategoryDTO[]> {
    const params: CategoryFilters = { searchTerm: query };
    const result = await this.getCategories(params);
    return result.items;
  }

  /**
   * Get popular categories
   */
  static async getPopularCategories(count?: number): Promise<CategoryDTO[]> {
    const params = count ? { count } : undefined;
    const response = await api.get<CategoryDTO[]>(CATEGORIES.POPULAR, { params });
    return response.data;
  }

  /**
   * Get categories with product count
   */
  static async getCategoriesWithProductCount(): Promise<CategoryDTO[]> {
    const response = await api.get<CategoryDTO[]>(CATEGORIES.WITH_PRODUCT_COUNT);
    return response.data;
  }

  /**
   * Get product count for a category
   */
  static async getCategoryProductCount(categoryId: number): Promise<number> {
    const response = await api.get<number>(CATEGORIES.PRODUCT_COUNT(categoryId));
    return response.data;
  }
}

export default CategoryService;
