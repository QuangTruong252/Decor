"use client";

import { API_URL, fetchWithAuth } from "@/lib/api-utils";

/**
 * Cart item DTO from API
 */
export interface CartItemDTO {
  id: number;
  cartId: number;
  productId: number;
  productName: string;
  productImageUrl?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

/**
 * Cart DTO from API
 */
export interface CartDTO {
  id: number;
  userId: number;
  items: CartItemDTO[];
  totalAmount: number;
  itemCount: number;
  createdAt: string;
  updatedAt: string;
}

/**
 * Add to cart payload
 */
export interface AddToCartPayload {
  productId: number;
  quantity: number;
}

/**
 * Update cart item payload
 */
export interface UpdateCartItemPayload {
  quantity: number;
}

/**
 * Merge cart payload
 */
export interface MergeCartPayload {
  guestCartItems: {
    productId: number;
    quantity: number;
  }[];
}

/**
 * Get cart
 * @returns Cart details
 * @endpoint GET /api/Cart
 */
export async function getCart(): Promise<CartDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Cart`);

    if (!response.ok) {
      throw new Error("Unable to fetch cart");
    }

    return response.json();
  } catch (error) {
    console.error("Get cart error:", error);
    throw new Error("Unable to fetch cart. Please try again later.");
  }
}

/**
 * Add item to cart
 * @param item Item to add
 * @returns Updated cart
 * @endpoint POST /api/Cart
 */
export async function addToCart(item: AddToCartPayload): Promise<CartDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Cart`, {
      method: "POST",
      body: JSON.stringify(item),
    });

    if (!response.ok) {
      throw new Error("Unable to add item to cart");
    }

    return response.json();
  } catch (error) {
    console.error("Add to cart error:", error);
    throw new Error("Unable to add item to cart. Please try again later.");
  }
}

/**
 * Clear cart
 * @returns void
 * @endpoint DELETE /api/Cart
 */
export async function clearCart(): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Cart`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to clear cart");
    }
  } catch (error) {
    console.error("Clear cart error:", error);
    throw new Error("Unable to clear cart. Please try again later.");
  }
}

/**
 * Update cart item
 * @param itemId Cart item ID
 * @param update Update data
 * @returns Updated cart
 * @endpoint PUT /api/Cart/items/{id}
 */
export async function updateCartItem(itemId: number, update: UpdateCartItemPayload): Promise<CartDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Cart/items/${itemId}`, {
      method: "PUT",
      body: JSON.stringify(update),
    });

    if (!response.ok) {
      throw new Error("Unable to update cart item");
    }

    return response.json();
  } catch (error) {
    console.error(`Update cart item ${itemId} error:`, error);
    throw new Error("Unable to update cart item. Please try again later.");
  }
}

/**
 * Remove cart item
 * @param itemId Cart item ID
 * @returns Updated cart
 * @endpoint DELETE /api/Cart/items/{id}
 */
export async function removeCartItem(itemId: number): Promise<CartDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Cart/items/${itemId}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to remove cart item");
    }

    return response.json();
  } catch (error) {
    console.error(`Remove cart item ${itemId} error:`, error);
    throw new Error("Unable to remove cart item. Please try again later.");
  }
}

/**
 * Merge guest cart with user cart
 * @param mergeData Guest cart items to merge
 * @returns Updated cart
 * @endpoint POST /api/Cart/merge
 */
export async function mergeCart(mergeData: MergeCartPayload): Promise<CartDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Cart/merge`, {
      method: "POST",
      body: JSON.stringify(mergeData),
    });

    if (!response.ok) {
      throw new Error("Unable to merge cart");
    }

    return response.json();
  } catch (error) {
    console.error("Merge cart error:", error);
    throw new Error("Unable to merge cart. Please try again later.");
  }
}
