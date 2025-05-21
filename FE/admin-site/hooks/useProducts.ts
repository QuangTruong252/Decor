"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getProducts,
  getProductById,
  createProduct,
  updateProduct,
  deleteProduct,
  type CreateProductPayload,
  type UpdateProductPayload
} from "@/services/products";
import { useToast } from "@/hooks/use-toast";

export function useGetProducts() {
  return useQuery({
    queryKey: ["products"],
    queryFn: getProducts,
  });
}

export function useGetProductById(id: number) {
  return useQuery({
    queryKey: ["products", id],
    queryFn: () => getProductById(id),
    enabled: !!id,
  });
}

export function useCreateProduct() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (product: CreateProductPayload) => createProduct(product),
    onSuccess: () => {
      success({
        title: "Success",
        description: "Product created successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error creating product: ${err.message}`
      });
    },
  });
}

export function useUpdateProduct() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (product: UpdateProductPayload) => updateProduct(product),
    onSuccess: (_, variables) => {
      success({
        title: "Success",
        description: "Product updated successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["products"] });
      queryClient.invalidateQueries({ queryKey: ["products", variables.id] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error updating product: ${err.message}`
      });
    },
  });
}

export function useDeleteProduct() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (id: number) => deleteProduct(id),
    onSuccess: () => {
      success({
        title: "Success",
        description: "Product deleted successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error deleting product: ${err.message}`
      });
    },
  });
}