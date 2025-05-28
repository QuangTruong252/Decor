"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useCategoryStore } from "@/stores/categoryStore";
import {
  createCategory,
  updateCategory,
  deleteCategory,
  type CreateCategoryPayload,
  type UpdateCategoryPayload,
  type CategoryDTO,
} from "@/services/categories";
import { useToast } from "@/hooks/use-toast";

/**
 * Hook that provides category store integration with mutations
 * This replaces the individual category hooks and integrates with the centralized store
 */
export function useCategoryStoreActions() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();
  
  // Store actions
  const {
    categories,
    flatCategories,
    isLoading,
    isInitialized,
    error: storeError,
    initializeCategories,
    addCategory,
    updateCategory: updateCategoryInStore,
    removeCategory,
    clearError,
    reset,
    getCategoryById,
    getRootCategories,
    getSubcategories,
    getCategoryPath,
  } = useCategoryStore();

  // Create category mutation
  const createCategoryMutation = useMutation({
    mutationFn: (category: CreateCategoryPayload) => createCategory(category),
    onSuccess: (newCategory: CategoryDTO) => {
      success({
        title: "Success",
        description: "Category created successfully!"
      });
      
      // Add to store
      addCategory(newCategory);
      
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error creating category: ${err.message}`
      });
    },
  });

  // Update category mutation
  const updateCategoryMutation = useMutation({
    mutationFn: (category: UpdateCategoryPayload) => updateCategory(category),
    onSuccess: (updatedCategory: CategoryDTO) => {
      success({
        title: "Success",
        description: "Category updated successfully!"
      });
      
      // Update in store
      updateCategoryInStore(updatedCategory);
      
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error updating category: ${err.message}`
      });
    },
  });

  // Delete category mutation
  const deleteCategoryMutation = useMutation({
    mutationFn: (categoryId: number) => deleteCategory(categoryId),
    onSuccess: (_, categoryId) => {
      success({
        title: "Success",
        description: "Category deleted successfully!"
      });
      
      // Remove from store
      removeCategory(categoryId);
      
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error deleting category: ${err.message}`
      });
    },
  });

  return {
    // Store state
    categories,
    flatCategories,
    isLoading,
    isInitialized,
    error: storeError,
    
    // Store actions
    initializeCategories,
    clearError,
    reset,
    
    // Selectors
    getCategoryById,
    getRootCategories,
    getSubcategories,
    getCategoryPath,
    
    // Mutations
    createCategory: createCategoryMutation,
    updateCategory: updateCategoryMutation,
    deleteCategory: deleteCategoryMutation,
    
    // Mutation states
    isCreating: createCategoryMutation.isPending,
    isUpdating: updateCategoryMutation.isPending,
    isDeleting: deleteCategoryMutation.isPending,
  };
}

/**
 * Hook to get all categories from store (replaces useGetAllCategories)
 */
export function useCategories() {
  const { flatCategories, isLoading, isInitialized, error, initializeCategories } = useCategoryStore();
  
  // Auto-initialize if not already done
  if (!isInitialized && !isLoading && !error) {
    initializeCategories();
  }
  
  return {
    data: flatCategories,
    isLoading,
    error,
    isSuccess: isInitialized && !error,
  };
}

/**
 * Hook to get hierarchical categories from store
 */
export function useHierarchicalCategories() {
  const { categories, isLoading, isInitialized, error, initializeCategories } = useCategoryStore();
  
  // Auto-initialize if not already done
  if (!isInitialized && !isLoading && !error) {
    initializeCategories();
  }
  
  return {
    data: categories,
    isLoading,
    error,
    isSuccess: isInitialized && !error,
  };
}

/**
 * Hook to get a specific category by ID
 */
export function useCategoryById(id: number) {
  const { getCategoryById, isLoading, isInitialized, error, initializeCategories } = useCategoryStore();
  
  // Auto-initialize if not already done
  if (!isInitialized && !isLoading && !error) {
    initializeCategories();
  }
  
  return {
    data: getCategoryById(id),
    isLoading,
    error,
    isSuccess: isInitialized && !error,
  };
}

/**
 * Hook to get root categories
 */
export function useRootCategories() {
  const { getRootCategories, isLoading, isInitialized, error, initializeCategories } = useCategoryStore();
  
  // Auto-initialize if not already done
  if (!isInitialized && !isLoading && !error) {
    initializeCategories();
  }
  
  return {
    data: getRootCategories(),
    isLoading,
    error,
    isSuccess: isInitialized && !error,
  };
}

/**
 * Hook to get subcategories of a parent category
 */
export function useSubcategories(parentId: number) {
  const { getSubcategories, isLoading, isInitialized, error, initializeCategories } = useCategoryStore();
  
  // Auto-initialize if not already done
  if (!isInitialized && !isLoading && !error) {
    initializeCategories();
  }
  
  return {
    data: getSubcategories(parentId),
    isLoading,
    error,
    isSuccess: isInitialized && !error,
  };
}
