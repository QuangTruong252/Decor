"use client";

import { API_URL, fetchWithAuth } from "@/lib/api-utils";
import { buildApiUrl, cleanFilters } from "@/lib/query-utils";
import { CustomerFilters, PagedResult } from "@/types/api";

/**
 * Customer response DTO from API
 */
export interface CustomerDTO {
  id: number;
  email: string | null;
  firstName: string | null;
  lastName: string | null;
  phone: string | null;
  address: string | null;
  city: string | null;
  state: string | null;
  country: string | null;
  postalCode: string | null;
  createdAt: string;
  updatedAt: string;
}

/**
 * Extended Customer DTO with order information
 */
export interface CustomerWithOrdersDTO extends CustomerDTO {
  orders?: {
    id: number;
    totalAmount: number;
    orderStatus: string;
    orderDate: string;
  }[];
}

/**
 * Create customer payload
 */
export interface CreateCustomerPayload {
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  address?: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

/**
 * Update customer payload
 */
export interface UpdateCustomerPayload {
  email?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
}

/**
 * Get customers with pagination and filtering
 * @param filters Customer filters
 * @returns Paged result of customers
 * @endpoint GET /api/Customer
 */
export async function getCustomers(filters?: CustomerFilters): Promise<PagedResult<CustomerDTO>> {
  try {
    const cleanedFilters = filters ? cleanFilters(filters) : {};
    const url = buildApiUrl(`${API_URL}/api/Customer`, cleanedFilters);
    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch customers");
    }

    return response.json();
  } catch (error) {
    console.error("Get customers error:", error);
    throw new Error("Unable to fetch customers. Please try again later.");
  }
}

/**
 * Get all customers without pagination
 * @returns List of all customers
 * @endpoint GET /api/Customer/all
 */
export async function getAllCustomers(): Promise<CustomerDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer/all`);

    if (!response.ok) {
      throw new Error("Unable to fetch customers");
    }

    return response.json();
  } catch (error) {
    console.error("Get all customers error:", error);
    throw new Error("Unable to fetch customers. Please try again later.");
  }
}

/**
 * Get customer by ID
 * @param id Customer ID
 * @returns Customer details
 * @endpoint GET /api/customer/{id}
 */
export async function getCustomerById(id: number): Promise<CustomerWithOrdersDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/customer/${id}`);

    if (!response.ok) {
      throw new Error("Unable to fetch customer details");
    }

    return response.json();
  } catch (error) {
    console.error(`Get customer by id ${id} error:`, error);
    throw new Error("Unable to fetch customer details. Please try again later.");
  }
}

/**
 * Get customer by email
 * @param email Customer email
 * @returns Customer details
 * @endpoint GET /api/Customer/email/{email}
 */
export async function getCustomerByEmail(email: string): Promise<CustomerDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer/email/${encodeURIComponent(email)}`);

    if (!response.ok) {
      throw new Error("Unable to fetch customer by email");
    }

    return response.json();
  } catch (error) {
    console.error(`Get customer by email ${email} error:`, error);
    throw new Error("Unable to fetch customer by email. Please try again later.");
  }
}

/**
 * Create a new customer
 * @param customer Customer data
 * @returns Created customer
 * @endpoint POST /api/Customer
 */
export async function createCustomer(customer: CreateCustomerPayload): Promise<CustomerDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer`, {
      method: "POST",
      body: JSON.stringify(customer),
    });

    if (!response.ok) {
      throw new Error("Unable to create customer");
    }

    return response.json();
  } catch (error) {
    console.error("Create customer error:", error);
    throw new Error("Unable to create customer. Please try again later.");
  }
}

/**
 * Update a customer
 * @param id Customer ID
 * @param customer Customer data
 * @returns void
 * @endpoint PUT /api/Customer/{id}
 */
export async function updateCustomer(id: number, customer: UpdateCustomerPayload): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer/${id}`, {
      method: "PUT",
      body: JSON.stringify(customer),
    });

    if (!response.ok) {
      throw new Error("Unable to update customer");
    }
  } catch (error) {
    console.error(`Update customer by id ${id} error:`, error);
    throw new Error("Unable to update customer. Please try again later.");
  }
}

/**
 * Delete a customer
 * @param id Customer ID
 * @returns void
 * @endpoint DELETE /api/Customer/{id}
 */
export async function deleteCustomer(id: number): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer/${id}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to delete customer");
    }
  } catch (error) {
    console.error(`Delete customer by id ${id} error:`, error);
    throw new Error("Unable to delete customer. Please try again later.");
  }
}

/**
 * Get customers with orders
 * @returns List of customers with their orders
 * @endpoint GET /api/Customer/with-orders
 */
export async function getCustomersWithOrders(): Promise<CustomerWithOrdersDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer/with-orders`);

    if (!response.ok) {
      throw new Error("Unable to fetch customers with orders");
    }

    return response.json();
  } catch (error) {
    console.error("Get customers with orders error:", error);
    throw new Error("Unable to fetch customers with orders. Please try again later.");
  }
}

/**
 * Get top customers by order count
 * @param limit Number of top customers to return
 * @returns List of top customers by order count
 * @endpoint GET /api/Customer/top-by-order-count
 */
export async function getTopCustomersByOrderCount(limit?: number): Promise<CustomerDTO[]> {
  try {
    let url = `${API_URL}/api/Customer/top-by-order-count`;
    if (limit) {
      url += `?limit=${limit}`;
    }

    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch top customers by order count");
    }

    return response.json();
  } catch (error) {
    console.error("Get top customers by order count error:", error);
    throw new Error("Unable to fetch top customers by order count. Please try again later.");
  }
}

/**
 * Get top customers by spending
 * @param limit Number of top customers to return
 * @returns List of top customers by spending
 * @endpoint GET /api/Customer/top-by-spending
 */
export async function getTopCustomersBySpending(limit?: number): Promise<CustomerDTO[]> {
  try {
    let url = `${API_URL}/api/Customer/top-by-spending`;
    if (limit) {
      url += `?limit=${limit}`;
    }

    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch top customers by spending");
    }

    return response.json();
  } catch (error) {
    console.error("Get top customers by spending error:", error);
    throw new Error("Unable to fetch top customers by spending. Please try again later.");
  }
}

/**
 * Get customer order count
 * @param customerId Customer ID
 * @returns Number of orders for the customer
 * @endpoint GET /api/Customer/{customerId}/order-count
 */
export async function getCustomerOrderCount(customerId: number): Promise<number> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer/${customerId}/order-count`);

    if (!response.ok) {
      throw new Error("Unable to fetch customer order count");
    }

    return response.json();
  } catch (error) {
    console.error(`Get customer order count for ${customerId} error:`, error);
    throw new Error("Unable to fetch customer order count. Please try again later.");
  }
}

/**
 * Get customer total spent
 * @param customerId Customer ID
 * @returns Total amount spent by the customer
 * @endpoint GET /api/Customer/{customerId}/total-spent
 */
export async function getCustomerTotalSpent(customerId: number): Promise<number> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Customer/${customerId}/total-spent`);

    if (!response.ok) {
      throw new Error("Unable to fetch customer total spent");
    }

    return response.json();
  } catch (error) {
    console.error(`Get customer total spent for ${customerId} error:`, error);
    throw new Error("Unable to fetch customer total spent. Please try again later.");
  }
}

/**
 * Get customers by location
 * @param city Optional city filter
 * @param state Optional state filter
 * @param country Optional country filter
 * @returns List of customers filtered by location
 * @endpoint GET /api/Customer/by-location
 */
export async function getCustomersByLocation(city?: string, state?: string, country?: string): Promise<CustomerDTO[]> {
  try {
    let url = `${API_URL}/api/Customer/by-location`;
    const params = [];
    
    if (city) {
      params.push(`city=${encodeURIComponent(city)}`);
    }
    
    if (state) {
      params.push(`state=${encodeURIComponent(state)}`);
    }
    
    if (country) {
      params.push(`country=${encodeURIComponent(country)}`);
    }
    
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }

    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch customers by location");
    }

    return response.json();
  } catch (error) {
    console.error("Get customers by location error:", error);
    throw new Error("Unable to fetch customers by location. Please try again later.");
  }
}
