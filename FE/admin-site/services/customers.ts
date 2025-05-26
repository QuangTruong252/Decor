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
