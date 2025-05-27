import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { CartService } from '@/api/services';
import type {
  Cart,
  CartItem,
  CartDTO,
  AddToCartDTO,
  CartValidation
} from '@/api/types';

interface CartStore {
  // State
  cart: Cart | null;
  isLoading: boolean;
  error: string | null;
  isUpdating: boolean;
  lastUpdated: number | null;

  // Computed values (stored as properties for Zustand compatibility)
  itemCount: number;
  subtotal: number;
  total: number;
  isEmpty: boolean;

  // Actions
  initializeCart: () => Promise<void>;
  addToCart: (productId: number, quantity: number) => Promise<void>;
  updateCartItem: (itemId: number, quantity: number) => Promise<void>;
  removeCartItem: (itemId: number) => Promise<void>;
  clearCart: () => Promise<void>;
  mergeCart: () => Promise<void>;
  refreshCart: () => Promise<void>;
  validateCart: () => Promise<CartValidation>;

  // Local cart operations (for guests)
  addToLocalCart: (productId: number, quantity: number, productData: any) => void;
  updateLocalCartItem: (itemId: number, quantity: number) => void;
  removeLocalCartItem: (itemId: number) => void;
  clearLocalCart: () => void;

  // Utility functions
  getItemById: (itemId: number) => CartItem | undefined;
  getItemByProductId: (productId: number) => CartItem | undefined;
  isProductInCart: (productId: number) => boolean;
  getProductQuantity: (productId: number) => number;

  // Error handling
  setError: (error: string | null) => void;
  clearError: () => void;

  // Loading states
  setLoading: (loading: boolean) => void;
  setUpdating: (updating: boolean) => void;
}

const TAX_RATE = 0.1; // 10% tax
const FREE_SHIPPING_THRESHOLD = 100; // Free shipping over $100
const SHIPPING_COST = 10;
const CART_EXPIRY_HOURS = 24; // Cart expires after 24 hours

const calculateCartTotals = (items: CartItem[]) => {
  // Calculate subtotal from individual item subtotals
  const subtotal = items.reduce((sum, item) => {
    // Ensure we have valid numbers
    const itemSubtotal = item.subtotal || (item.quantity * item.unitPrice);
    return sum + itemSubtotal;
  }, 0);

  // Calculate tax on subtotal
  const tax = Math.round(subtotal * TAX_RATE * 100) / 100; // Round to 2 decimal places

  // Calculate shipping based on threshold
  const shipping = subtotal >= FREE_SHIPPING_THRESHOLD ? 0 : SHIPPING_COST;

  // Calculate final total
  const total = Math.round((subtotal + tax + shipping) * 100) / 100; // Round to 2 decimal places

  return { subtotal, tax, shipping, total };
};

// Helper function to update computed values
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

const transformCartDTO = (cartDTO: CartDTO): Cart => {
  const items: CartItem[] = (cartDTO.items || []).map(item => ({
    ...item,
    // Ensure subtotal is calculated correctly
    subtotal: item.subtotal || (item.quantity * item.unitPrice),
    isAvailable: true, // Default to available - should come from API
    stockStatus: 'in_stock' as const, // Default to in stock - should come from API
  }));

  const { subtotal, tax, shipping, total } = calculateCartTotals(items);

  return {
    ...cartDTO,
    items,
    subtotal,
    tax,
    shipping,
    discount: 0, // Default to 0 - should come from API
    total,
    // Ensure totalItems is calculated correctly
    totalItems: items.reduce((sum, item) => sum + item.quantity, 0),
    totalAmount: subtotal, // This is typically the subtotal before tax/shipping
  };
};

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      // Initial state
      cart: null,
      isLoading: false,
      error: null,
      isUpdating: false,
      lastUpdated: null,

      // Computed values as properties (not getters for Zustand compatibility)
      itemCount: 0,
      subtotal: 0,
      total: 0,
      isEmpty: true,

      // Initialize cart
      initializeCart: async () => {
        const { setLoading, setError } = get();

        try {
          setLoading(true);
          setError(null);

          // Check if cart has expired
          const { lastUpdated } = get();
          if (lastUpdated) {
            const hoursSinceUpdate = (Date.now() - lastUpdated) / (1000 * 60 * 60);
            if (hoursSinceUpdate > CART_EXPIRY_HOURS) {
              // Cart has expired, clear it
              const computedValues = updateComputedValues(null);
              set({
                cart: null,
                lastUpdated: null,
                isLoading: false,
                ...computedValues
              });
              return;
            }
          }

          const cartDTO = await CartService.getCart();
          const cart = transformCartDTO(cartDTO);
          const computedValues = updateComputedValues(cart);

          set({
            cart,
            lastUpdated: Date.now(),
            isLoading: false,
            ...computedValues
          });
        } catch (error) {
          console.error('Failed to initialize cart:', error);
          const computedValues = updateComputedValues(null);
          setError('Failed to load cart');
          setLoading(false);
          set({ ...computedValues });
        }
      },

      // Add item to cart
      addToCart: async (productId: number, quantity: number) => {
        const { setUpdating, setError } = get();

        try {
          setUpdating(true);
          setError(null);

          const addToCartData: AddToCartDTO = { productId, quantity };
          const cartDTO = await CartService.addToCart(addToCartData);

          if (!cartDTO) {
            throw new Error('No response from cart service');
          }

          const cart = transformCartDTO(cartDTO);
          const computedValues = updateComputedValues(cart);

          set({
            cart,
            lastUpdated: Date.now(),
            isUpdating: false,
            ...computedValues
          });
        } catch (error: any) {
          console.error('Failed to add to cart:', {
            error: error?.message || error,
            productId,
            quantity,
            stack: error?.stack
          });

          const errorMessage = error?.response?.data?.message ||
                              error?.message ||
                              'Failed to add item to cart';

          setError(errorMessage);
          setUpdating(false);
          throw new Error(errorMessage);
        }
      },

      // Update cart item
      updateCartItem: async (itemId: number, quantity: number) => {
        const { setUpdating, setError } = get();

        try {
          setUpdating(true);
          setError(null);

          if (quantity <= 0) {
            await get().removeCartItem(itemId);
            return;
          }

          const cartDTO = await CartService.updateCartItem(itemId, { quantity });

          if (!cartDTO) {
            throw new Error('No response from cart service');
          }

          const cart = transformCartDTO(cartDTO);
          const computedValues = updateComputedValues(cart);

          set({
            cart,
            lastUpdated: Date.now(),
            isUpdating: false,
            ...computedValues
          });
        } catch (error: any) {
          console.error('Failed to update cart item:', {
            error: error?.message || error,
            itemId,
            quantity,
            stack: error?.stack
          });

          const errorMessage = error?.response?.data?.message ||
                              error?.message ||
                              'Failed to update cart item';

          setError(errorMessage);
          setUpdating(false);
          throw new Error(errorMessage);
        }
      },

      // Remove cart item
      removeCartItem: async (itemId: number) => {
        const { setUpdating, setError } = get();

        try {
          setUpdating(true);
          setError(null);

          const cartDTO = await CartService.removeCartItem(itemId);
          const cart = transformCartDTO(cartDTO);
          const computedValues = updateComputedValues(cart);

          set({
            cart,
            lastUpdated: Date.now(),
            isUpdating: false,
            ...computedValues
          });
        } catch (error) {
          console.error('Failed to remove cart item:', error);
          setError('Failed to remove cart item');
          setUpdating(false);
          throw error;
        }
      },

      // Clear cart
      clearCart: async () => {
        const { setUpdating, setError } = get();

        try {
          setUpdating(true);
          setError(null);

          const cartDTO = await CartService.clearCart();
          const cart = transformCartDTO(cartDTO);
          const computedValues = updateComputedValues(cart);

          set({
            cart,
            lastUpdated: Date.now(),
            isUpdating: false,
            ...computedValues
          });
        } catch (error) {
          console.error('Failed to clear cart:', error);
          setError('Failed to clear cart');
          setUpdating(false);
          throw error;
        }
      },

      // Merge cart (after login)
      mergeCart: async () => {
        const { setUpdating, setError } = get();

        try {
          setUpdating(true);
          setError(null);

          await CartService.mergeCart();
          await get().refreshCart();
        } catch (error) {
          console.error('Failed to merge cart:', error);
          setError('Failed to merge cart');
          setUpdating(false);
          throw error;
        }
      },

      // Refresh cart
      refreshCart: async () => {
        await get().initializeCart();
      },

      // Validate cart
      validateCart: async () => {
        try {
          const validation = await CartService.validateCart();
          return {
            isValid: validation.isValid,
            errors: validation.errors.map(error => ({
              itemId: error.itemId,
              productId: error.productId,
              productName: '', // This should come from API
              type: 'product_unavailable' as const,
              message: error.message,
            })),
            warnings: [],
          };
        } catch (error) {
          console.error('Failed to validate cart:', error);
          return {
            isValid: false,
            errors: [{
              itemId: 0,
              productId: 0,
              productName: '',
              type: 'product_unavailable' as const,
              message: 'Failed to validate cart',
            }],
            warnings: [],
          };
        }
      },

      // Local cart operations (for guests)
      addToLocalCart: (productId: number, quantity: number, productData: any) => {
        const { cart } = get();
        const currentCart = cart || {
          id: 0,
          userId: null,
          sessionId: 'guest',
          totalAmount: 0,
          totalItems: 0,
          items: [],
          subtotal: 0,
          tax: 0,
          shipping: 0,
          discount: 0,
          total: 0,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };

        const existingItemIndex = (currentCart.items || []).findIndex(
          item => item.productId === productId
        );

        let newItems: CartItem[];

        if (existingItemIndex >= 0) {
          // Update existing item
          newItems = [...(currentCart.items || [])];
          newItems[existingItemIndex] = {
            ...newItems[existingItemIndex],
            quantity: newItems[existingItemIndex].quantity + quantity,
            subtotal: (newItems[existingItemIndex].quantity + quantity) * newItems[existingItemIndex].unitPrice,
          };
        } else {
          // Add new item
          const newItem: CartItem = {
            id: Date.now(), // Temporary ID for local cart
            productId,
            productName: productData.name,
            productSlug: productData.slug,
            productImage: productData.images?.[0] || '/images/product-placeholder.png',
            quantity,
            unitPrice: productData.price,
            subtotal: Math.round(quantity * productData.price * 100) / 100, // Round to 2 decimal places
            isAvailable: true,
            stockStatus: 'in_stock',
          };
          newItems = [...(currentCart.items || []), newItem];
        }

        const { subtotal, tax, shipping, total } = calculateCartTotals(newItems);

        const updatedCart: Cart = {
          ...currentCart,
          items: newItems,
          totalItems: newItems.reduce((sum, item) => sum + item.quantity, 0),
          totalAmount: subtotal,
          subtotal,
          tax,
          shipping,
          total,
          updatedAt: new Date().toISOString(),
        };

        const computedValues = updateComputedValues(updatedCart);
        set({ cart: updatedCart, lastUpdated: Date.now(), ...computedValues });
      },

      updateLocalCartItem: (itemId: number, quantity: number) => {
        const { cart } = get();
        if (!cart) return;

        if (quantity <= 0) {
          get().removeLocalCartItem(itemId);
          return;
        }

        const newItems = (cart.items || []).map(item =>
          item.id === itemId
            ? { ...item, quantity, subtotal: Math.round(quantity * item.unitPrice * 100) / 100 }
            : item
        );

        const { subtotal, tax, shipping, total } = calculateCartTotals(newItems);

        const updatedCart: Cart = {
          ...cart,
          items: newItems,
          totalItems: newItems.reduce((sum, item) => sum + item.quantity, 0),
          totalAmount: subtotal,
          subtotal,
          tax,
          shipping,
          total,
          updatedAt: new Date().toISOString(),
        };

        const computedValues = updateComputedValues(updatedCart);
        set({ cart: updatedCart, lastUpdated: Date.now(), ...computedValues });
      },

      removeLocalCartItem: (itemId: number) => {
        const { cart } = get();
        if (!cart) return;

        const newItems = (cart.items || []).filter(item => item.id !== itemId);
        const { subtotal, tax, shipping, total } = calculateCartTotals(newItems);

        const updatedCart: Cart = {
          ...cart,
          items: newItems,
          totalItems: newItems.reduce((sum, item) => sum + item.quantity, 0),
          totalAmount: subtotal,
          subtotal,
          tax,
          shipping,
          total,
          updatedAt: new Date().toISOString(),
        };

        const computedValues = updateComputedValues(updatedCart);
        set({ cart: updatedCart, lastUpdated: Date.now(), ...computedValues });
      },

      clearLocalCart: () => {
        const computedValues = updateComputedValues(null);
        set({
          cart: null,
          lastUpdated: Date.now(),
          ...computedValues
        });
      },

      // Utility functions
      getItemById: (itemId: number) => {
        const { cart } = get();
        return cart?.items?.find(item => item.id === itemId);
      },

      getItemByProductId: (productId: number) => {
        const { cart } = get();
        return cart?.items?.find(item => item.productId === productId);
      },

      isProductInCart: (productId: number) => {
        const { cart } = get();
        return cart?.items?.some(item => item.productId === productId) || false;
      },

      getProductQuantity: (productId: number) => {
        const { cart } = get();
        const item = cart?.items?.find(item => item.productId === productId);
        return item?.quantity || 0;
      },

      // Error handling
      setError: (error: string | null) => set({ error }),
      clearError: () => set({ error: null }),

      // Loading states
      setLoading: (isLoading: boolean) => set({ isLoading }),
      setUpdating: (isUpdating: boolean) => set({ isUpdating }),
    }),
    {
      name: 'cart-storage',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        cart: state.cart,
        lastUpdated: state.lastUpdated,
        // Also persist computed values to prevent reset on page reload
        itemCount: state.itemCount,
        subtotal: state.subtotal,
        total: state.total,
        isEmpty: state.isEmpty
      }),
      // Add onRehydrateStorage to ensure computed values are updated after hydration
      onRehydrateStorage: () => (state) => {
        if (state?.cart) {
          const computedValues = updateComputedValues(state.cart);
          Object.assign(state, computedValues);
        }
      },
    }
  )
);

export default useCartStore;
