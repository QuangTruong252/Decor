"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getCustomers,
  getAllCustomers,
  getCustomerById,
  getCustomerByEmail,
  createCustomer,
  updateCustomer,
  deleteCustomer,
  type CustomerDTO,
  type CustomerWithOrdersDTO,
  type CreateCustomerPayload,
  type UpdateCustomerPayload
} from "@/services/customers";
import { CustomerFilters } from "@/types/api";
import { useToast } from "@/hooks/use-toast";

/**
 * Hook to fetch customers with pagination and filtering
 */
export function useGetCustomers(filters?: CustomerFilters) {
  return useQuery({
    queryKey: ["customers", filters],
    queryFn: () => getCustomers(filters),
  });
}

/**
 * Hook to fetch all customers without pagination
 */
export function useGetAllCustomers() {
  return useQuery({
    queryKey: ["customers", "all"],
    queryFn: getAllCustomers,
  });
}

/**
 * Hook to fetch a user/customer by ID
 * @param id Customer ID
 */
export function useGetCustomerById(id: number) {
  return useQuery({
    queryKey: ["customers", id],
    queryFn: () => getCustomerById(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch a customer by email
 * @param email Customer email
 */
export function useGetCustomerByEmail(email: string) {
  return useQuery({
    queryKey: ["customers", "email", email],
    queryFn: () => getCustomerByEmail(email),
    enabled: !!email,
  });
}

/**
 * Hook to create a new customer
 */
export function useCreateCustomer() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (customer: CreateCustomerPayload) => createCustomer(customer),
    onSuccess: () => {
      success({
        title: "Customer Created",
        description: "Customer has been created successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["customers"] });
    },
    onError: (err: Error) => {
      error({
        title: "Create Failed",
        description: err.message || "Failed to create customer",
      });
    },
  });
}

/**
 * Hook to update a customer
 */
export function useUpdateCustomer() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: ({ id, customer }: { id: number; customer: UpdateCustomerPayload }) =>
      updateCustomer(id, customer),
    onSuccess: () => {
      success({
        title: "Customer Updated",
        description: "Customer has been updated successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["customers"] });
    },
    onError: (err: Error) => {
      error({
        title: "Update Failed",
        description: err.message || "Failed to update customer",
      });
    },
  });
}

/**
 * Hook to delete a customer
 */
export function useDeleteCustomer() {
  const queryClient = useQueryClient();
  const { success, error } = useToast();

  return useMutation({
    mutationFn: (id: number) => deleteCustomer(id),
    onSuccess: () => {
      success({
        title: "Customer Deleted",
        description: "Customer has been deleted successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["customers"] });
    },
    onError: (err: Error) => {
      error({
        title: "Delete Failed",
        description: err.message || "Failed to delete customer",
      });
    },
  });
}
