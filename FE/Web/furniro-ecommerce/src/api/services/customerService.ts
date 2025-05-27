import { api } from '../client';
import { CUSTOMER } from '../endpoints';
import type {
  CustomerDTO,
  CustomerDTOPagedResult,
  CreateCustomerDTO,
  UpdateCustomerDTO,
  CustomerFilters,
} from '../types';

export class CustomerService {
  /**
   * Get customers with pagination and filtering
   */
  static async getCustomers(params?: CustomerFilters): Promise<CustomerDTOPagedResult> {
    const response = await api.get<CustomerDTOPagedResult>(CUSTOMER.BASE, { params });
    return response.data;
  }

  /**
   * Get all customers without pagination (admin only)
   */
  static async getAllCustomers(): Promise<CustomerDTO[]> {
    const response = await api.get<CustomerDTO[]>(CUSTOMER.ALL);
    return response.data;
  }

  /**
   * Get customer by ID
   */
  static async getCustomerById(id: number): Promise<CustomerDTO> {
    const response = await api.get<CustomerDTO>(CUSTOMER.BY_ID(id));
    return response.data;
  }

  /**
   * Get customer by email
   */
  static async getCustomerByEmail(email: string): Promise<CustomerDTO> {
    const response = await api.get<CustomerDTO>(CUSTOMER.BY_EMAIL(email));
    return response.data;
  }

  /**
   * Create new customer
   */
  static async createCustomer(customerData: CreateCustomerDTO): Promise<CustomerDTO> {
    const response = await api.post<CustomerDTO>(CUSTOMER.BASE, customerData);
    return response.data;
  }

  /**
   * Update customer
   */
  static async updateCustomer(id: number, customerData: UpdateCustomerDTO): Promise<void> {
    await api.put(CUSTOMER.BY_ID(id), customerData);
  }

  /**
   * Delete customer
   */
  static async deleteCustomer(id: number): Promise<void> {
    await api.delete(CUSTOMER.BY_ID(id));
  }

  /**
   * Get current customer profile (based on authenticated user)
   */
  static async getCurrentCustomerProfile(): Promise<CustomerDTO | null> {
    try {
      // This would typically be a separate endpoint like /Customer/profile
      // For now, we'll assume the API returns the current user's customer data
      const response = await api.get<CustomerDTO>('/Customer/profile');
      return response.data;
    } catch (error) {
      // If no customer profile exists, return null
      return null;
    }
  }

  /**
   * Update current customer profile
   */
  static async updateCurrentCustomerProfile(customerData: UpdateCustomerDTO): Promise<CustomerDTO> {
    const response = await api.put<CustomerDTO>('/Customer/profile', customerData);
    return response.data;
  }

  /**
   * Search customers by name or email
   */
  static async searchCustomers(query: string): Promise<CustomerDTO[]> {
    const params: CustomerFilters = { searchTerm: query };
    const result = await this.getCustomers(params);
    return result.items;
  }

  /**
   * Get customers by location
   */
  static async getCustomersByLocation(city?: string, state?: string, country?: string): Promise<CustomerDTO[]> {
    const response = await api.get<CustomerDTO[]>(CUSTOMER.BY_LOCATION, {
      params: { city, state, country }
    });
    return response.data;
  }

  /**
   * Get customers with orders
   */
  static async getCustomersWithOrders(): Promise<CustomerDTO[]> {
    const response = await api.get<CustomerDTO[]>(CUSTOMER.WITH_ORDERS);
    return response.data;
  }

  /**
   * Get top customers by order count
   */
  static async getTopCustomersByOrderCount(count?: number): Promise<CustomerDTO[]> {
    const params = count ? { count } : undefined;
    const response = await api.get<CustomerDTO[]>(CUSTOMER.TOP_BY_ORDER_COUNT, { params });
    return response.data;
  }

  /**
   * Get top customers by spending
   */
  static async getTopCustomersBySpending(count?: number): Promise<CustomerDTO[]> {
    const params = count ? { count } : undefined;
    const response = await api.get<CustomerDTO[]>(CUSTOMER.TOP_BY_SPENDING, { params });
    return response.data;
  }

  /**
   * Get customer order count
   */
  static async getCustomerOrderCount(customerId: number): Promise<number> {
    const response = await api.get<number>(CUSTOMER.ORDER_COUNT(customerId));
    return response.data;
  }

  /**
   * Get customer total spent
   */
  static async getCustomerTotalSpent(customerId: number): Promise<number> {
    const response = await api.get<number>(CUSTOMER.TOTAL_SPENT(customerId));
    return response.data;
  }

  /**
   * Validate customer email
   */
  static async validateCustomerEmail(email: string): Promise<{ isValid: boolean; exists: boolean }> {
    try {
      await this.getCustomerByEmail(email);
      return { isValid: true, exists: true };
    } catch (error: any) {
      if (error.status === 404) {
        return { isValid: true, exists: false };
      }
      return { isValid: false, exists: false };
    }
  }

  /**
   * Format customer full name
   */
  static formatCustomerName(customer: CustomerDTO): string {
    return `${customer.firstName} ${customer.lastName}`.trim();
  }

  /**
   * Format customer address
   */
  static formatCustomerAddress(customer: CustomerDTO): string {
    const addressParts = [
      customer.address,
      customer.city,
      customer.state,
      customer.postalCode,
      customer.country,
    ].filter(Boolean);

    return addressParts.join(', ');
  }

  /**
   * Check if customer has complete profile
   */
  static hasCompleteProfile(customer: CustomerDTO): boolean {
    const requiredFields = [
      customer.firstName,
      customer.lastName,
      customer.email,
    ];

    return requiredFields.every(field => field && field.trim().length > 0);
  }

  /**
   * Check if customer has complete address
   */
  static hasCompleteAddress(customer: CustomerDTO): boolean {
    const addressFields = [
      customer.address,
      customer.city,
      customer.state,
      customer.postalCode,
      customer.country,
    ];

    return addressFields.every(field => field && field.trim().length > 0);
  }

  /**
   * Get customer statistics
   */
  static async getCustomerStats(): Promise<{
    totalCustomers: number;
    customersWithOrders: number;
    customersWithCompleteProfiles: number;
    customersByCountry: Record<string, number>;
  }> {
    const customers = await this.getAllCustomers();

    const totalCustomers = customers.length;
    const customersWithCompleteProfiles = customers.filter(this.hasCompleteProfile).length;

    const customersByCountry = customers.reduce((acc, customer) => {
      if (customer.country) {
        acc[customer.country] = (acc[customer.country] || 0) + 1;
      }
      return acc;
    }, {} as Record<string, number>);

    return {
      totalCustomers,
      customersWithOrders: 0, // This would need order data
      customersWithCompleteProfiles,
      customersByCountry,
    };
  }

  /**
   * Export customers to CSV format
   */
  static async exportCustomersToCSV(): Promise<string> {
    const customers = await this.getAllCustomers();

    const headers = [
      'ID',
      'First Name',
      'Last Name',
      'Email',
      'Phone',
      'Address',
      'City',
      'State',
      'Postal Code',
      'Country',
      'Created At',
    ];

    const rows = customers.map(customer => [
      customer.id,
      customer.firstName,
      customer.lastName,
      customer.email,
      customer.phone || '',
      customer.address || '',
      customer.city || '',
      customer.state || '',
      customer.postalCode || '',
      customer.country || '',
      customer.createdAt,
    ]);

    const csvContent = [headers, ...rows]
      .map(row => row.map(field => `"${field}"`).join(','))
      .join('\n');

    return csvContent;
  }
}

export default CustomerService;
