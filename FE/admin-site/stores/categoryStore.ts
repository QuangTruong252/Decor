"use client";

import { create } from "zustand";
import { devtools } from "zustand/middleware";
import { CategoryDTO } from "@/services/categories";
import { getHierarchicalCategories } from "@/services/categories";

/**
 * Category store state interface
 */
interface CategoryState {
  // Data
  categories: CategoryDTO[];
  flatCategories: CategoryDTO[];
  
  // Loading states
  isLoading: boolean;
  isInitialized: boolean;
  
  // Error state
  error: string | null;
  
  // Actions
  initializeCategories: () => Promise<void>;
  addCategory: (category: CategoryDTO) => void;
  updateCategory: (category: CategoryDTO) => void;
  removeCategory: (categoryId: number) => void;
  clearError: () => void;
  reset: () => void;
  
  // Selectors
  getCategoryById: (id: number) => CategoryDTO | undefined;
  getRootCategories: () => CategoryDTO[];
  getSubcategories: (parentId: number) => CategoryDTO[];
  getCategoryPath: (categoryId: number) => CategoryDTO[];
}

/**
 * Utility function to flatten hierarchical categories
 */
const flattenCategories = (categories: CategoryDTO[]): CategoryDTO[] => {
  const flattened: CategoryDTO[] = [];
  
  const flatten = (cats: CategoryDTO[]) => {
    cats.forEach(category => {
      flattened.push(category);
      if (category.subcategories && category.subcategories.length > 0) {
        flatten(category.subcategories);
      }
    });
  };
  
  flatten(categories);
  return flattened;
};

/**
 * Utility function to find category by ID in hierarchical structure
 */
const findCategoryById = (categories: CategoryDTO[], id: number): CategoryDTO | undefined => {
  for (const category of categories) {
    if (category.id === id) {
      return category;
    }
    if (category.subcategories && category.subcategories.length > 0) {
      const found = findCategoryById(category.subcategories, id);
      if (found) return found;
    }
  }
  return undefined;
};

/**
 * Utility function to update category in hierarchical structure
 */
const updateCategoryInHierarchy = (categories: CategoryDTO[], updatedCategory: CategoryDTO): CategoryDTO[] => {
  return categories.map(category => {
    if (category.id === updatedCategory.id) {
      return { ...updatedCategory, subcategories: category.subcategories };
    }
    if (category.subcategories && category.subcategories.length > 0) {
      return {
        ...category,
        subcategories: updateCategoryInHierarchy(category.subcategories, updatedCategory)
      };
    }
    return category;
  });
};

/**
 * Utility function to remove category from hierarchical structure
 */
const removeCategoryFromHierarchy = (categories: CategoryDTO[], categoryId: number): CategoryDTO[] => {
  return categories
    .filter(category => category.id !== categoryId)
    .map(category => ({
      ...category,
      subcategories: category.subcategories 
        ? removeCategoryFromHierarchy(category.subcategories, categoryId)
        : undefined
    }));
};

/**
 * Utility function to add category to hierarchical structure
 */
const addCategoryToHierarchy = (categories: CategoryDTO[], newCategory: CategoryDTO): CategoryDTO[] => {
  // If it's a root category (no parentId)
  if (!newCategory.parentId) {
    return [...categories, newCategory];
  }
  
  // Find parent and add as subcategory
  return categories.map(category => {
    if (category.id === newCategory.parentId) {
      return {
        ...category,
        subcategories: [...(category.subcategories || []), newCategory]
      };
    }
    if (category.subcategories && category.subcategories.length > 0) {
      return {
        ...category,
        subcategories: addCategoryToHierarchy(category.subcategories, newCategory)
      };
    }
    return category;
  });
};

/**
 * Category store implementation using Zustand
 */
export const useCategoryStore = create<CategoryState>()(
  devtools(
    (set, get) => ({
      // Initial state
      categories: [],
      flatCategories: [],
      isLoading: false,
      isInitialized: false,
      error: null,

      // Initialize categories from API
      initializeCategories: async () => {
        const { isInitialized, isLoading } = get();
        
        // Prevent multiple initializations
        if (isInitialized || isLoading) {
          return;
        }

        set({ isLoading: true, error: null });

        try {
          const hierarchicalCategories = await getHierarchicalCategories();
          const flatCategories = flattenCategories(hierarchicalCategories);

          set({
            categories: hierarchicalCategories,
            flatCategories,
            isLoading: false,
            isInitialized: true,
            error: null,
          });
        } catch (error) {
          console.error("Failed to initialize categories:", error);
          set({
            isLoading: false,
            error: error instanceof Error ? error.message : "Failed to load categories",
          });
        }
      },

      // Add new category
      addCategory: (category: CategoryDTO) => {
        const { categories } = get();
        const updatedCategories = addCategoryToHierarchy(categories, category);
        const flatCategories = flattenCategories(updatedCategories);

        set({
          categories: updatedCategories,
          flatCategories,
        });
      },

      // Update existing category
      updateCategory: (category: CategoryDTO) => {
        const { categories } = get();
        const updatedCategories = updateCategoryInHierarchy(categories, category);
        const flatCategories = flattenCategories(updatedCategories);

        set({
          categories: updatedCategories,
          flatCategories,
        });
      },

      // Remove category
      removeCategory: (categoryId: number) => {
        const { categories } = get();
        const updatedCategories = removeCategoryFromHierarchy(categories, categoryId);
        const flatCategories = flattenCategories(updatedCategories);

        set({
          categories: updatedCategories,
          flatCategories,
        });
      },

      // Clear error
      clearError: () => {
        set({ error: null });
      },

      // Reset store
      reset: () => {
        set({
          categories: [],
          flatCategories: [],
          isLoading: false,
          isInitialized: false,
          error: null,
        });
      },

      // Selectors
      getCategoryById: (id: number) => {
        const { flatCategories } = get();
        return flatCategories.find(category => category.id === id);
      },

      getRootCategories: () => {
        const { categories } = get();
        return categories;
      },

      getSubcategories: (parentId: number) => {
        const { categories } = get();
        const parent = findCategoryById(categories, parentId);
        return parent?.subcategories || [];
      },

      getCategoryPath: (categoryId: number) => {
        const { flatCategories } = get();
        const path: CategoryDTO[] = [];
        
        const findPath = (id: number): boolean => {
          const category = flatCategories.find(cat => cat.id === id);
          if (!category) return false;
          
          path.unshift(category);
          
          if (category.parentId) {
            return findPath(category.parentId);
          }
          
          return true;
        };
        
        findPath(categoryId);
        return path;
      },
    }),
    {
      name: "category-store",
    }
  )
);
