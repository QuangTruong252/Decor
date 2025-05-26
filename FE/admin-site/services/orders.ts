"use client";

import { API_URL, fetchWithAuth } from "@/lib/api-utils";
import { buildApiUrl, cleanFilters } from "@/lib/query-utils";
import { OrderFilters, PagedResult } from "@/types/api";

/**
 * Order response DTO from API
 */
export interface OrderDTO {
  id: number;
  userId: number;
  userFullName: string | null;
  customerFullName: string | null;
  totalAmount: number;
  orderStatus: string | null;
  paymentMethod: string | null;
  shippingAddress: string | null;
  notes: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  shippingCity: string | null;
  shippingState: string | null;
  shippingCountry: string | null;
  shippingPostalCode: string | null;
  orderDate: string;
  updatedAt: string;
  orderItems: OrderItemDTO[] | null;
}

/**
 * Order item DTO from API
 */
export interface OrderItemDTO {
  id: number;
  orderId: number;
  productId: number;
  productName: string | null;
  productImageUrl: string | null;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

/**
 * Create order item payload
 */
export interface CreateOrderItemPayload {
  productId: number;
  quantity: number;
}

/**
 * Create order payload
 */
export interface CreateOrderPayload {
  userId: number;
  paymentMethod: string;
  shippingAddress: string;
  notes: string;
  contactEmail: string;
  contactPhone: string;
  shippingCity: string;
  shippingState: string;
  shippingCountry: string;
  shippingPostalCode: string;
  orderItems: CreateOrderItemPayload[];
}

/**
 * Update order payload
 */
export interface UpdateOrderPayload {
  paymentMethod?: string;
  shippingAddress?: string;
  notes?: string;
  contactEmail?: string;
  contactPhone?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingCountry?: string;
  shippingPostalCode?: string;
}

/**
 * Update order status payload
 */
export interface UpdateOrderStatusPayload {
  orderStatus: string;
}

/**
 * Get orders with pagination and filtering
 * @param filters Order filters
 * @returns Paged result of orders
 * @endpoint GET /api/Order
 */
export async function getOrders(filters?: OrderFilters): Promise<PagedResult<OrderDTO>> {
  try {
    const cleanedFilters = filters ? cleanFilters(filters) : {};
    const url = buildApiUrl(`${API_URL}/api/Order`, cleanedFilters);
    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch orders");
    }

    return response.json();
  } catch (error) {
    console.error("Get orders error:", error);
    throw new Error("Unable to fetch orders. Please try again later.");
  }
}

/**
 * Get all orders without pagination
 * @returns List of all orders
 * @endpoint GET /api/Order/all
 */
export async function getAllOrders(): Promise<OrderDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Order/all`);

    if (!response.ok) {
      throw new Error("Unable to fetch orders");
    }

    return response.json();
  } catch (error) {
    console.error("Get all orders error:", error);
    throw new Error("Unable to fetch orders. Please try again later.");
  }
}

/**
 * Get order by ID
 * @param id Order ID
 * @returns Order details
 * @endpoint GET /api/Order/{id}
 */
export async function getOrderById(id: number): Promise<OrderDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Order/${id}`);

    if (!response.ok) {
      throw new Error("Unable to fetch order details");
    }

    return response.json();
  } catch (error) {
    console.error(`Get order by id ${id} error:`, error);
    throw new Error("Unable to fetch order details. Please try again later.");
  }
}

/**
 * Get orders by user ID
 * @param userId User ID
 * @returns List of orders for the user
 * @endpoint GET /api/Order/user/{userId}
 */
export async function getOrdersByUserId(userId: number): Promise<OrderDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Order/user/${userId}`);

    if (!response.ok) {
      throw new Error("Unable to fetch user orders");
    }

    return response.json();
  } catch (error) {
    console.error(`Get orders by user id ${userId} error:`, error);
    throw new Error("Unable to fetch user orders. Please try again later.");
  }
}

/**
 * Update order status
 * @param id Order ID
 * @param status New order status
 * @returns void
 * @endpoint PUT /api/Order/{id}/status
 */
export async function updateOrderStatus(id: number, status: UpdateOrderStatusPayload): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Order/${id}/status`, {
      method: "PUT",
      body: JSON.stringify(status),
    });

    if (!response.ok) {
      throw new Error("Unable to update order status");
    }
  } catch (error) {
    console.error(`Update order status error:`, error);
    throw new Error("Unable to update order status. Please try again later.");
  }
}

/**
 * Create a new order
 * @param order Order data
 * @returns Created order
 * @endpoint POST /api/Order
 */
export async function createOrder(order: CreateOrderPayload): Promise<OrderDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Order`, {
      method: "POST",
      body: JSON.stringify(order),
    });

    if (!response.ok) {
      throw new Error("Unable to create order");
    }

    return response.json();
  } catch (error) {
    console.error("Create order error:", error);
    throw new Error("Unable to create order. Please try again later.");
  }
}

/**
 * Update an order
 * @param id Order ID
 * @param order Order data
 * @returns void
 * @endpoint PUT /api/Order/{id}
 */
export async function updateOrder(id: number, order: UpdateOrderPayload): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Order/${id}`, {
      method: "PUT",
      body: JSON.stringify(order),
    });

    if (!response.ok) {
      throw new Error("Unable to update order");
    }
  } catch (error) {
    console.error(`Update order by id ${id} error:`, error);
    throw new Error("Unable to update order. Please try again later.");
  }
}

/**
 * Delete an order
 * @param id Order ID
 * @returns void
 * @endpoint DELETE /api/Order/{id}
 */
export async function deleteOrder(id: number): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Order/${id}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to delete order");
    }
  } catch (error) {
    console.error(`Delete order by id ${id} error:`, error);
    throw new Error("Unable to delete order. Please try again later.");
  }
}
