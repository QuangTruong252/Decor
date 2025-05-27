import { BaseEntity } from './common';
import { PagedResult } from './api';

// Order related types
export interface OrderDTO extends BaseEntity {
  userId: number;
  userFullName: string | null;
  customerId: number | null;
  customerFullName: string | null;
  totalAmount: number;
  orderStatus: string | null;
  paymentMethod: string | null;
  shippingAddress: string | null;
  shippingCity: string | null;
  shippingState: string | null;
  shippingPostalCode: string | null;
  shippingCountry: string | null;
  contactPhone: string | null;
  contactEmail: string | null;
  notes: string | null;
  orderDate: string;
  orderItems: OrderItemDTO[] | null;
}

// Paged result for orders
export interface OrderDTOPagedResult extends PagedResult<OrderDTO> {}

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

export interface CreateOrderDTO {
  userId: number;
  customerId?: number | null;
  paymentMethod: string;
  shippingAddress: string;
  shippingCity?: string | null;
  shippingState?: string | null;
  shippingPostalCode?: string | null;
  shippingCountry?: string | null;
  contactPhone?: string | null;
  contactEmail?: string | null;
  notes?: string | null;
  orderItems: CreateOrderItemDTO[];
}

export interface CreateOrderItemDTO {
  productId: number;
  quantity: number;
}

export interface UpdateOrderStatusDTO {
  orderStatus: string;
}

// Extended order types
export type OrderStatus =
  | 'pending'
  | 'confirmed'
  | 'processing'
  | 'shipped'
  | 'delivered'
  | 'cancelled'
  | 'refunded';

export type PaymentMethod =
  | 'credit_card'
  | 'debit_card'
  | 'paypal'
  | 'bank_transfer'
  | 'cash_on_delivery';

export interface Order extends OrderDTO {
  statusHistory: OrderStatusHistory[];
  paymentDetails: PaymentDetails;
  shippingDetails: ShippingDetails;
  canCancel: boolean;
  canReturn: boolean;
  estimatedDelivery?: string;
}

export interface OrderStatusHistory {
  id: number;
  orderId: number;
  status: OrderStatus;
  timestamp: string;
  note?: string;
  updatedBy: string;
}

export interface PaymentDetails {
  method: PaymentMethod;
  status: 'pending' | 'completed' | 'failed' | 'refunded';
  transactionId?: string;
  paidAt?: string;
  amount: number;
  currency: string;
}

export interface ShippingDetails {
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string;
  trackingNumber?: string;
  carrier?: string;
  estimatedDelivery?: string;
  actualDelivery?: string;
}

// Order creation form
export interface OrderFormData {
  shippingAddress: ShippingAddressForm;
  paymentMethod: PaymentMethod;
  paymentDetails?: PaymentFormData;
  notes?: string;
}

// Additional types for Order Management System
export interface OrderExtended extends OrderDTO {
  statusHistory: OrderStatusHistory[];
  paymentDetails: PaymentDetailsExtended;
  shippingDetails: ShippingDetails;
  canCancel: boolean;
  canReturn: boolean;
}

export interface OrderStatusHistory {
  status: OrderStatus;
  timestamp: string;
  updatedBy: string;
  note?: string;
}

export interface PaymentDetailsExtended {
  method: PaymentMethod;
  status: 'pending' | 'processing' | 'completed' | 'failed' | 'cancelled' | 'refunded';
  amount: number;
  currency: string;
  transactionId?: string;
  gatewayResponse?: any;
}

export interface ShippingDetails {
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  trackingNumber?: string;
  estimatedDelivery?: string;
}

export interface ShippingAddressForm {
  firstName: string;
  lastName: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string;
}

export interface PaymentFormData {
  cardNumber?: string;
  expiryDate?: string;
  cvv?: string;
  cardholderName?: string;
  billingAddress?: ShippingAddressForm;
}

// Note: OrderFilters is now defined in api.ts for consistency with API specification

// Order summary
export interface OrderSummary {
  subtotal: number;
  tax: number;
  shipping: number;
  discount: number;
  total: number;
  itemCount: number;
}

// Order analytics
export interface OrderAnalytics {
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  ordersByStatus: Record<OrderStatus, number>;
  ordersByPaymentMethod: Record<PaymentMethod, number>;
  topProducts: Array<{
    productId: number;
    productName: string;
    quantity: number;
    revenue: number;
  }>;
}

// Hooks return types
export interface UseOrdersReturn {
  orders: OrderDTO[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  hasMore: boolean;
  loadMore: () => void;
}

export interface UseOrderReturn {
  order: Order | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  updateStatus: (status: OrderStatus) => Promise<void>;
  cancelOrder: () => Promise<void>;
}

export interface UseCreateOrderReturn {
  createOrder: (orderData: OrderFormData) => Promise<OrderDTO>;
  isCreating: boolean;
  error: string | null;
  clearError: () => void;
}
