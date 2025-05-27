'use client';

import { useEffect } from 'react';
import { useCartStore } from '@/store/cartStore';
import type { Cart } from '@/api/types';

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

/**
 * Custom hook to ensure cart computed values are synced correctly
 * Useful for page navigation and component mounting
 */
export function useCartSync() {
  const { cart, itemCount, subtotal, total, isEmpty } = useCartStore();

  useEffect(() => {
    if (cart) {
      const computedValues = updateComputedValues(cart);
      
      // Check if computed values are out of sync
      if (itemCount !== computedValues.itemCount || 
          subtotal !== computedValues.subtotal || 
          total !== computedValues.total ||
          isEmpty !== computedValues.isEmpty) {
        
        console.log('useCartSync: Computed values out of sync, updating...', {
          current: { itemCount, subtotal, total, isEmpty },
          computed: computedValues
        });
        
        // Update the store with correct computed values
        useCartStore.setState(computedValues);
      }
    }
  }, [cart, itemCount, subtotal, total, isEmpty]);

  return { cart, itemCount, subtotal, total, isEmpty };
}

export default useCartSync;
