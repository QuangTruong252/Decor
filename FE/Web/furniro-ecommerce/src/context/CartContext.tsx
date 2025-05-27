'use client';

import React, { createContext, useContext, useEffect, ReactNode } from 'react';
import { useCartStore } from '@/store/cartStore';
import { useAuth } from './AuthContext';
import type { CartContextType, CartItem, Cart } from '@/api/types';

// Helper function to update computed values (same as in cartStore)
const updateComputedValues = (cart: Cart | null) => {
  if (!cart || !cart.items) {
    return {
      itemCount: 0,
      subtotal: 0,
      total: 0,
      isEmpty: true,
    };
  }

  const itemCount = cart.totalItems || cart.items.reduce((sum, item) => sum + item.quantity, 0);
  const subtotal = cart.subtotal || 0;
  const total = cart.total || 0;
  const isEmpty = itemCount === 0;

  return { itemCount, subtotal, total, isEmpty };
};

const CartContext = createContext<CartContextType | undefined>(undefined);

interface CartProviderProps {
  children: ReactNode;
}

export function CartProvider({ children }: CartProviderProps) {
  const { user, isAuthenticated } = useAuth();
  const {
    cart,
    isLoading,
    error,
    itemCount,
    subtotal,
    total,
    isEmpty,
    initializeCart,
    addToCart: addToServerCart,
    updateCartItem: updateServerCartItem,
    removeCartItem: removeServerCartItem,
    clearCart: clearServerCart,
    mergeCart,
    refreshCart,
    validateCart,
    addToLocalCart,
    updateLocalCartItem,
    removeLocalCartItem,
    clearLocalCart,
    getItemById,
    getItemByProductId,
    isProductInCart,
    getProductQuantity,
  } = useCartStore();

  // Initialize cart when component mounts or auth state changes
  useEffect(() => {
    const initCart = async () => {
      console.log('CartContext: Initializing cart', { isAuthenticated, userId: user?.id });

      if (isAuthenticated && user) {
        // User is logged in - load cart from server
        try {
          console.log('CartContext: Loading cart from server');
          await initializeCart();

          // If there's a local cart, merge it
          const localCart = cart;
          if (localCart && localCart.items && localCart.items.length > 0) {
            console.log('CartContext: Merging local cart with server cart');
            await mergeCart();
            clearLocalCart();
          }
        } catch (error) {
          console.error('CartContext: Failed to initialize user cart:', error);
        }
      } else {
        console.log('CartContext: Guest user - using local cart');
        // Guest user - cart is managed locally via Zustand persistence
        // Don't initialize empty cart, let Zustand handle persistence
        if (!cart) {
          console.log('CartContext: No local cart found, will use persisted state');
        } else {
          console.log('CartContext: Local cart found with', cart.items?.length || 0, 'items');
          // Ensure computed values are correct after page load
          const computedValues = updateComputedValues(cart);
          console.log('CartContext: Computed values updated', computedValues);
        }
      }
    };

    initCart();
  }, [isAuthenticated, user?.id]);

  // Force update computed values when cart changes (for page navigation)
  useEffect(() => {
    if (cart) {
      console.log('CartContext: Cart changed, updating computed values');
      const computedValues = updateComputedValues(cart);
      console.log('CartContext: New computed values', computedValues);

      // Update the store with correct computed values
      const store = useCartStore.getState();
      if (store.itemCount !== computedValues.itemCount ||
          store.subtotal !== computedValues.subtotal ||
          store.total !== computedValues.total) {
        console.log('CartContext: Syncing computed values to store');
        useCartStore.setState(computedValues);
      }
    }
  }, [cart]);

  // Cart operations that handle both authenticated and guest users
  const addItem = async (productId: number, quantity: number) => {
    console.log('CartContext: Adding item', { productId, quantity, isAuthenticated });

    if (isAuthenticated) {
      console.log('CartContext: Adding to server cart');
      await addToServerCart(productId, quantity);
    } else {
      console.log('CartContext: Adding to local cart');
      // For guest users, we need product data to add to local cart
      try {
        // Fetch product data from API for guest users
        const { ProductService } = await import('@/api/services');
        const product = await ProductService.getProductById(productId);

        const productData = {
          name: product.name || `Product ${productId}`,
          slug: product.slug || `product-${productId}`,
          price: product.price || 0,
          images: product.images || [],
        };
        console.log('CartContext: Product data fetched', productData);
        addToLocalCart(productId, quantity, productData);
      } catch (error) {
        console.error('CartContext: Failed to fetch product data for local cart:', error);
        // Fallback to basic product data if API call fails
        const fallbackProductData = {
          name: `Product ${productId}`,
          slug: `product-${productId}`,
          price: 0,
          images: [],
        };
        console.log('CartContext: Using fallback product data', fallbackProductData);
        addToLocalCart(productId, quantity, fallbackProductData);
      }
    }
  };

  const updateItem = async (itemId: number, quantity: number) => {
    if (isAuthenticated) {
      await updateServerCartItem(itemId, quantity);
    } else {
      updateLocalCartItem(itemId, quantity);
    }
  };

  const removeItem = async (itemId: number) => {
    if (isAuthenticated) {
      await removeServerCartItem(itemId);
    } else {
      removeLocalCartItem(itemId);
    }
  };

  const clearCart = async () => {
    if (isAuthenticated) {
      await clearServerCart();
    } else {
      clearLocalCart();
    }
  };

  const contextValue: CartContextType = {
    // State
    cart,
    isLoading,
    error,
    itemCount,
    subtotal,
    total,
    isEmpty,

    // Operations
    addItem,
    updateItem,
    removeItem,
    clearCart,
    mergeCart,
    validateCart,
    refreshCart,
  };

  return (
    <CartContext.Provider value={contextValue}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart(): CartContextType {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
}

// Custom hook for cart item operations
export function useCartItem(itemId: number) {
  const cart = useCart();
  const { getItemById } = useCartStore();
  const item = getItemById(itemId);

  const updateQuantity = async (quantity: number) => {
    if (item) {
      await cart.updateItem(item.id, quantity);
    }
  };

  const removeItem = async () => {
    if (item) {
      await cart.removeItem(item.id);
    }
  };

  return {
    item,
    updateQuantity,
    removeItem,
    isUpdating: cart.isLoading,
    error: cart.error,
  };
}

// Custom hook for product cart operations
export function useProductCart(productId: number) {
  const cart = useCart();
  const { getItemByProductId, isProductInCart, getProductQuantity } = useCartStore();
  const item = getItemByProductId(productId);
  const isInCart = isProductInCart(productId);
  const quantity = getProductQuantity(productId);

  const addToCart = async (qty: number = 1) => {
    await cart.addItem(productId, qty);
  };

  const updateQuantity = async (newQuantity: number) => {
    if (item) {
      await cart.updateItem(item.id, newQuantity);
    }
  };

  const removeFromCart = async () => {
    if (item) {
      await cart.removeItem(item.id);
    }
  };

  return {
    item,
    isInCart,
    quantity,
    addToCart,
    updateQuantity,
    removeFromCart,
    isUpdating: cart.isLoading,
    error: cart.error,
  };
}

export default CartContext;
