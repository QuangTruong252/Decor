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
  return useMutation({
    mutationFn: (category: CreateCategoryPayload) => createCategory(category),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
  });
}

/**
 * Hook to update an existing category
 */
export function useUpdateCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (category: UpdateCategoryPayload) => updateCategory(category),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      queryClient.invalidateQueries({ queryKey: ["categories", variables.id] });
    },
  });
}

/**
 * Hook to delete a category
 */
export function useDeleteCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
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
