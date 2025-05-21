"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getCategories,
  getCategoryById,
  createCategory,
  updateCategory,
  deleteCategory,
  getProductsByCategory,
  type CreateCategoryPayload,
  type UpdateCategoryPayload
} from "@/services/categories";
import { useToast } from "@/hooks/use-toast";

/**
 * Hook to fetch all categories
 */
export function useGetCategories() {
  return useQuery({
    queryKey: ["categories"],
    queryFn: getCategories,
  });
}

/**
 * Hook to fetch a category by ID
 * @param id Category ID
 */
export function useGetCategoryById(id: number) {
  return useQuery({
    queryKey: ["categories", id],
    queryFn: () => getCategoryById(id),
    enabled: !!id,
  });
}

/**
 * Hook to create a new category
 */
export function useCreateCategory() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (category: CreateCategoryPayload) => createCategory(category),
    onSuccess: () => {
      success({
        title: "Success",
        description: "Category created successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error creating category: ${err.message}`
      });
    },
  });
}

/**
 * Hook to update an existing category
 */
export function useUpdateCategory() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (category: UpdateCategoryPayload) => updateCategory(category),
    onSuccess: (_, variables) => {
      success({
        title: "Success",
        description: "Category updated successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      queryClient.invalidateQueries({ queryKey: ["categories", variables.id] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error updating category: ${err.message}`
      });
    },
  });
}

/**
 * Hook to delete a category
 */
export function useDeleteCategory() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (id: number) => deleteCategory(id),
    onSuccess: () => {
      success({
        title: "Success",
        description: "Category deleted successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error deleting category: ${err.message}`
      });
    },
  });
}

/**
 * Hook to fetch products by category ID
 * @param categoryId Category ID
 */
export function useGetProductsByCategory(categoryId: number) {
  return useQuery({
    queryKey: ["categories", categoryId, "products"],
    queryFn: () => getProductsByCategory(categoryId),
    enabled: !!categoryId,
  });
}
