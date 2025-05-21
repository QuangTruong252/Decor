"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getOrders,
  getOrderById,
  getOrdersByUserId,
  createOrder,
  updateOrder,
  updateOrderStatus,
  deleteOrder,
  type OrderDTO,
  type CreateOrderPayload,
  type UpdateOrderPayload,
  type UpdateOrderStatusPayload
} from "@/services/orders";
import { useToast } from "@/hooks/use-toast";

/**
 * Hook to fetch all orders
 */
export function useGetOrders() {
  return useQuery({
    queryKey: ["orders"],
    queryFn: getOrders,
  });
}

/**
 * Hook to fetch an order by ID
 * @param id Order ID
 */
export function useGetOrderById(id: number) {
  return useQuery({
    queryKey: ["orders", id],
    queryFn: () => getOrderById(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch orders by user ID
 * @param userId User ID
 */
export function useGetOrdersByUserId(userId: number) {
  return useQuery({
    queryKey: ["orders", "user", userId],
    queryFn: () => getOrdersByUserId(userId),
    enabled: !!userId,
  });
}

/**
 * Hook to create a new order
 */
export function useCreateOrder() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (order: CreateOrderPayload) => createOrder(order),
    onSuccess: () => {
      success({
        title: "Order Created",
        description: "Order has been created successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
    onError: (err: Error) => {
      error({
        title: "Create Failed",
        description: err.message || "Failed to create order",
      });
    },
  });
}

/**
 * Hook to update an order
 */
export function useUpdateOrder() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: ({ id, order }: { id: number; order: UpdateOrderPayload }) =>
      updateOrder(id, order),
    onSuccess: () => {
      success({
        title: "Order Updated",
        description: "Order has been updated successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
    onError: (err: Error) => {
      error({
        title: "Update Failed",
        description: err.message || "Failed to update order",
      });
    },
  });
}

/**
 * Hook to update order status
 */
export function useUpdateOrderStatus() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: ({ id, status }: { id: number; status: UpdateOrderStatusPayload }) =>
      updateOrderStatus(id, status),
    onSuccess: () => {
      success({
        title: "Order Status Updated",
        description: "Order status has been updated successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
    onError: (err: Error) => {
      error({
        title: "Update Failed",
        description: err.message || "Failed to update order status",
      });
    },
  });
}

/**
 * Hook to delete an order
 */
export function useDeleteOrder() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (id: number) => deleteOrder(id),
    onSuccess: () => {
      success({
        title: "Order Deleted",
        description: "Order has been deleted successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
    onError: (err: Error) => {
      error({
        title: "Delete Failed",
        description: err.message || "Failed to delete order",
      });
    },
  });
}
