import { BaseEntity } from './common';

// Cart related types
export interface CartDTO extends BaseEntity {
  userId: number | null;
  sessionId: string | null;
  totalAmount: number;
  totalItems: number;
  items: CartItemDTO[] | null;
}

export interface CartItemDTO {
  id: number;
  productId: number;
  productName: string | null;
  productSlug: string | null;
  productImage: string | null;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface AddToCartDTO {
  productId: number;
  quantity: number;
}

export interface UpdateCartItemDTO {
  quantity: number;
}

// Extended cart types for UI
export interface CartItem extends CartItemDTO {
  maxQuantity?: number;
  isAvailable: boolean;
  stockStatus: 'in_stock' | 'low_stock' | 'out_of_stock';
}

export interface Cart extends Omit<CartDTO, 'items'> {
  items: CartItem[];
  subtotal: number;
  tax: number;
  shipping: number;
  discount: number;
  total: number;
}

// Cart operations
export interface CartOperations {
  addItem: (productId: number, quantity: number) => Promise<void>;
  updateItem: (itemId: number, quantity: number) => Promise<void>;
  removeItem: (itemId: number) => Promise<void>;
  clearCart: () => Promise<void>;
  mergeCart: () => Promise<void>;
}

// Cart state
export interface CartState {
  cart: Cart | null;
  isLoading: boolean;
  error: string | null;
  isUpdating: boolean;
}

// Cart summary for checkout
export interface CartSummary {
  items: CartItemDTO[];
  itemCount: number;
  subtotal: number;
  tax: number;
  shipping: number;
  discount: number;
  total: number;
}

// Cart validation
export interface CartValidation {
  isValid: boolean;
  errors: CartValidationError[];
  warnings: CartValidationWarning[];
}

export interface CartValidationError {
  itemId: number;
  productId: number;
  productName: string;
  type: 'out_of_stock' | 'insufficient_stock' | 'price_changed' | 'product_unavailable';
  message: string;
  currentStock?: number;
  requestedQuantity?: number;
}

export interface CartValidationWarning {
  itemId: number;
  productId: number;
  productName: string;
  type: 'low_stock' | 'price_increase' | 'price_decrease';
  message: string;
  oldPrice?: number;
  newPrice?: number;
}

// Cart persistence
export interface CartPersistence {
  saveToLocal: (cart: Cart) => void;
  loadFromLocal: () => Cart | null;
  clearLocal: () => void;
  syncWithServer: () => Promise<void>;
}

// Shopping cart context
export interface CartContextType extends CartOperations {
  cart: Cart | null;
  isLoading: boolean;
  error: string | null;
  itemCount: number;
  subtotal: number;
  total: number;
  isEmpty: boolean;
  validateCart: () => Promise<CartValidation>;
  refreshCart: () => Promise<void>;
}

// Cart hooks return types
export interface UseCartReturn extends CartContextType {}

export interface UseCartItemReturn {
  item: CartItem;
  updateQuantity: (quantity: number) => Promise<void>;
  removeItem: () => Promise<void>;
  isUpdating: boolean;
  error: string | null;
}

// Cart analytics
export interface CartAnalytics {
  abandonmentRate: number;
  averageCartValue: number;
  conversionRate: number;
  mostAddedProducts: Array<{
    productId: number;
    productName: string;
    addCount: number;
  }>;
  mostRemovedProducts: Array<{
    productId: number;
    productName: string;
    removeCount: number;
  }>;
}
