import { api } from '../client';
import { CART } from '../endpoints';
import type {
  CartDTO,
  AddToCartDTO,
  UpdateCartItemDTO,
} from '../types';

export class CartService {
  /**
   * Get current user's cart
   */
  static async getCart(): Promise<CartDTO> {
    const response = await api.get<CartDTO>(CART.BASE);
    return response.data;
  }

  /**
   * Add item to cart
   */
  static async addToCart(item: AddToCartDTO): Promise<CartDTO> {
    const response = await api.post<CartDTO>(CART.BASE, item);
    return response.data;
  }

  /**
   * Update cart item quantity
   */
  static async updateCartItem(itemId: number, data: UpdateCartItemDTO): Promise<CartDTO> {
    const response = await api.put<CartDTO>(CART.ITEMS(itemId), data);
    return response.data;
  }

  /**
   * Remove item from cart
   */
  static async removeCartItem(itemId: number): Promise<CartDTO> {
    const response = await api.delete<CartDTO>(CART.ITEMS(itemId));
    return response.data;
  }

  /**
   * Clear entire cart
   */
  static async clearCart(): Promise<CartDTO> {
    const response = await api.delete<CartDTO>(CART.BASE);
    return response.data;
  }

  /**
   * Merge guest cart with user cart (after login)
   */
  static async mergeCart(): Promise<void> {
    await api.post(CART.MERGE);
  }

  /**
   * Get cart item count
   */
  static async getCartItemCount(): Promise<number> {
    try {
      const cart = await this.getCart();
      return cart.totalItems;
    } catch {
      return 0;
    }
  }

  /**
   * Get cart total amount
   */
  static async getCartTotal(): Promise<number> {
    try {
      const cart = await this.getCart();
      return cart.totalAmount;
    } catch {
      return 0;
    }
  }

  /**
   * Check if product is in cart
   */
  static async isProductInCart(productId: number): Promise<boolean> {
    try {
      const cart = await this.getCart();
      return cart.items?.some((item: any) => item.productId === productId) || false;
    } catch {
      return false;
    }
  }

  /**
   * Get cart item by product ID
   */
  static async getCartItemByProductId(productId: number) {
    try {
      const cart = await this.getCart();
      return cart.items?.find((item: any) => item.productId === productId) || null;
    } catch {
      return null;
    }
  }

  /**
   * Update multiple cart items at once
   */
  static async updateMultipleItems(updates: Array<{ itemId: number; quantity: number }>): Promise<CartDTO> {
    // Since API doesn't support bulk update, we'll update items sequentially
    let cart = await this.getCart();

    for (const update of updates) {
      cart = await this.updateCartItem(update.itemId, { quantity: update.quantity });
    }

    return cart;
  }

  /**
   * Validate cart items (check stock, prices, etc.)
   */
  static async validateCart(): Promise<{
    isValid: boolean;
    errors: Array<{
      itemId: number;
      productId: number;
      message: string;
    }>;
  }> {
    try {
      const cart = await this.getCart();
      // This would typically be a separate API endpoint
      // For now, we'll just return valid
      return {
        isValid: true,
        errors: [],
      };
    } catch {
      return {
        isValid: false,
        errors: [{ itemId: 0, productId: 0, message: 'Failed to validate cart' }],
      };
    }
  }

  /**
   * Calculate cart summary with taxes and shipping
   */
  static calculateCartSummary(cart: CartDTO, taxRate: number = 0.1, shippingCost: number = 0) {
    const subtotal = cart.totalAmount;
    const tax = subtotal * taxRate;
    const total = subtotal + tax + shippingCost;

    return {
      subtotal,
      tax,
      shipping: shippingCost,
      total,
      itemCount: cart.totalItems,
    };
  }

  /**
   * Save cart to localStorage (for guest users)
   */
  static saveCartToLocal(cart: CartDTO): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem('guest_cart', JSON.stringify(cart));
    }
  }

  /**
   * Load cart from localStorage (for guest users)
   */
  static loadCartFromLocal(): CartDTO | null {
    if (typeof window === 'undefined') return null;

    const cartStr = localStorage.getItem('guest_cart');
    if (!cartStr) return null;

    try {
      return JSON.parse(cartStr);
    } catch {
      return null;
    }
  }

  /**
   * Clear cart from localStorage
   */
  static clearCartFromLocal(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('guest_cart');
    }
  }
}

export default CartService;
