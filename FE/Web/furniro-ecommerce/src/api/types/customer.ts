import { BaseEntity } from './common';
import { PagedResult } from './api';

// Customer related types
export interface CustomerDTO extends BaseEntity {
  firstName: string;
  lastName: string;
  email: string;
  address: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  phone: string | null;
  fullName: string;
}

// Paged result for customers
export interface CustomerDTOPagedResult extends PagedResult<CustomerDTO> {}

export interface CreateCustomerDTO {
  firstName: string;
  lastName: string;
  email: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  phone?: string;
}

export interface UpdateCustomerDTO {
  firstName: string;
  lastName: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  phone?: string;
}

// Extended customer types
export interface Customer extends CustomerDTO {
  orderHistory: CustomerOrderSummary[];
  totalOrders: number;
  totalSpent: number;
  averageOrderValue: number;
  lastOrderDate?: string;
  loyaltyPoints?: number;
  preferredCategories: string[];
}

export interface CustomerOrderSummary {
  id: number;
  orderDate: string;
  totalAmount: number;
  status: string;
  itemCount: number;
}

// Customer profile form
export interface CustomerProfileForm {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
}

export interface CustomerAddressForm {
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault?: boolean;
}

// Customer address management
export interface CustomerAddress extends BaseEntity {
  customerId: number;
  type: 'shipping' | 'billing';
  firstName: string;
  lastName: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string;
  isDefault: boolean;
}

export interface CreateAddressDTO {
  type: 'shipping' | 'billing';
  firstName: string;
  lastName: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone?: string;
  isDefault?: boolean;
}

export interface UpdateAddressDTO extends Partial<CreateAddressDTO> {}

// Customer preferences
export interface CustomerPreferences {
  customerId: number;
  emailNotifications: boolean;
  smsNotifications: boolean;
  marketingEmails: boolean;
  orderUpdates: boolean;
  newsletter: boolean;
  language: string;
  currency: string;
  timezone: string;
}

// Customer search and filter
export interface CustomerSearchParams {
  query?: string;
  city?: string;
  state?: string;
  country?: string;
  hasOrders?: boolean;
  registeredFrom?: string;
  registeredTo?: string;
  sortBy?: 'firstName' | 'lastName' | 'email' | 'createdAt' | 'totalSpent';
  sortOrder?: 'asc' | 'desc';
  page?: number;
  limit?: number;
}

// Customer analytics
export interface CustomerAnalytics {
  totalCustomers: number;
  newCustomersThisMonth: number;
  activeCustomers: number;
  averageLifetimeValue: number;
  topCustomers: Array<{
    id: number;
    fullName: string;
    email: string;
    totalSpent: number;
    orderCount: number;
  }>;
  customersByLocation: Array<{
    country: string;
    count: number;
  }>;
}

// Customer loyalty
export interface CustomerLoyalty {
  customerId: number;
  points: number;
  tier: 'bronze' | 'silver' | 'gold' | 'platinum';
  nextTierPoints: number;
  expiringPoints: number;
  expirationDate?: string;
}

export interface LoyaltyTransaction {
  id: number;
  customerId: number;
  type: 'earned' | 'redeemed' | 'expired';
  points: number;
  description: string;
  orderId?: number;
  createdAt: string;
}

// Hooks return types
export interface UseCustomerReturn {
  customer: Customer | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  updateProfile: (data: UpdateCustomerDTO) => Promise<void>;
  updatePreferences: (preferences: Partial<CustomerPreferences>) => Promise<void>;
}

export interface UseCustomerAddressesReturn {
  addresses: CustomerAddress[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  addAddress: (address: CreateAddressDTO) => Promise<void>;
  updateAddress: (id: number, address: UpdateAddressDTO) => Promise<void>;
  deleteAddress: (id: number) => Promise<void>;
  setDefaultAddress: (id: number) => Promise<void>;
}

export interface UseCustomerOrdersReturn {
  orders: CustomerOrderSummary[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  hasMore: boolean;
  loadMore: () => void;
}
