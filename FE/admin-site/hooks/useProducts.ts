"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getProducts,
  getAllProducts,
  getProductById,
  createProduct,
  updateProduct,
  deleteProduct,
  bulkDeleteProducts,
  getProductsByCategory,
  getFeaturedProducts,
  getTopRatedProducts,
  getLowStockProducts,
  getRelatedProducts,
  addProductImages,
  deleteProductImage,
  type CreateProductPayload,
  type UpdateProductPayload
} from "@/services/products";
import { ProductFilters } from "@/types/api";
import { useToast } from "@/hooks/use-toast";

export function useGetProducts(filters?: ProductFilters) {
  return useQuery({
    queryKey: ["products", filters],
    queryFn: () => getProducts(filters),
  });
}

export function useGetAllProducts() {
  return useQuery({
    queryKey: ["products", "all"],
    queryFn: getAllProducts,
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

export function useBulkDeleteProducts() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (ids: number[]) => bulkDeleteProducts(ids),
    onSuccess: (_, ids) => {
      success({
        title: "Success",
        description: `${ids.length} product${ids.length > 1 ? 's' : ''} deleted successfully!`
      });
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error deleting products: ${err.message}`
      });
    },
  });
}

// Additional hooks for enhanced API functionality

export function useGetProductsByCategory(categoryId: number, count?: number) {
  return useQuery({
    queryKey: ["products", "category", categoryId, count],
    queryFn: () => getProductsByCategory(categoryId, count),
    enabled: !!categoryId,
  });
}

export function useGetFeaturedProducts(count?: number) {
  return useQuery({
    queryKey: ["products", "featured", count],
    queryFn: () => getFeaturedProducts(count),
  });
}

export function useGetTopRatedProducts(count?: number) {
  return useQuery({
    queryKey: ["products", "top-rated", count],
    queryFn: () => getTopRatedProducts(count),
  });
}

export function useGetLowStockProducts(threshold?: number) {
  return useQuery({
    queryKey: ["products", "low-stock", threshold],
    queryFn: () => getLowStockProducts(threshold),
  });
}

export function useGetRelatedProducts(id: number, count?: number) {
  return useQuery({
    queryKey: ["products", "related", id, count],
    queryFn: () => getRelatedProducts(id, count),
    enabled: !!id,
  });
}

export function useAddProductImages() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: ({ productId, images }: { productId: number; images: File[] }) => 
      addProductImages(productId, images),
    onSuccess: (_, variables) => {
      success({
        title: "Success",
        description: "Images added successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["products"] });
      queryClient.invalidateQueries({ queryKey: ["products", variables.productId] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error adding images: ${err.message}`
      });
    },
  });
}

export function useDeleteProductImage() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: ({ productId, imageId }: { productId: number; imageId: number }) => 
      deleteProductImage(productId, imageId),
    onSuccess: (_, variables) => {
      success({
        title: "Success",
        description: "Image deleted successfully!"
      });
      queryClient.invalidateQueries({ queryKey: ["products"] });
      queryClient.invalidateQueries({ queryKey: ["products", variables.productId] });
    },
    onError: (err: Error) => {
      error({
        title: "Error",
        description: `Error deleting image: ${err.message}`
      });
    },
  });
}
