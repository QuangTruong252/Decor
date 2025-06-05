"use client";

import { API_URL, fetchWithAuth } from "@/lib/api-utils";

/**
 * Dashboard summary metrics
 */
export interface DashboardSummary {
  totalProducts: number;
  totalOrders: number;
  totalCustomers: number;
  totalRevenue: number;
  recentOrders: RecentOrder[];
  popularProducts: PopularProduct[];
  salesByCategory: CategorySales[];
  orderStatusDistribution: OrderStatusDistribution;
  recentSalesTrend: SalesTrendItem[];
}

/**
 * Recent order item
 */
export interface RecentOrder {
  orderId: number;
  orderDate: string;
  customerName: string;
  totalAmount: number;
  orderStatus: string;
}

/**
 * Popular product item
 */
export interface PopularProduct {
  productId: number;
  name: string;
  imageUrl?: string;
  price: number;
  totalSold: number;
  totalRevenue: number;
}

/**
 * Category sales data
 */
export interface CategorySales {
  categoryId: number;
  categoryName: string;
  totalSales: number;
  totalRevenue: number;
  percentage: number;
}

/**
 * Order status distribution
 */
export interface OrderStatusDistribution {
  pending: number;
  processing: number;
  shipped: number;
  delivered: number;
  cancelled: number;
  total: number;
}

/**
 * Order status count (for backward compatibility)
 */
export interface OrderStatusCount {
  status: string;
  count: number;
}

/**
 * Sales trend data point
 */
export interface SalesTrendItem {
  date: string;
  revenue: number;
  orderCount: number;
}

/**
 * Get dashboard summary data
 * @returns Dashboard summary data
 * @endpoint GET /api/Dashboard/summary
 */
export async function getDashboardSummary(): Promise<DashboardSummary> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Dashboard/summary`);

    if (!response.ok) {
      throw new Error("Unable to fetch dashboard summary");
    }

    return response.json();
  } catch (error) {
    console.error("Get dashboard summary error:", error);
    throw new Error("Unable to fetch dashboard summary. Please try again later.");
  }
}

/**
 * Get sales trend data for a specific period
 * @param period Period for sales trend (daily, weekly, monthly)
 * @param startDate Optional start date for custom period
 * @param endDate Optional end date for custom period
 * @returns Sales trend data
 * @endpoint GET /api/Dashboard/sales-trend
 */
export async function getSalesTrend(
  period: "daily" | "weekly" | "monthly" = "daily",
  startDate?: string,
  endDate?: string
): Promise<SalesTrendItem[]> {
  try {
    let url = `${API_URL}/api/Dashboard/sales-trend?period=${period}`;

    if (startDate) {
      url += `&startDate=${startDate}`;
    }

    if (endDate) {
      url += `&endDate=${endDate}`;
    }

    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch sales trend data");
    }

    return response.json();
  } catch (error) {
    console.error("Get sales trend error:", error);
    throw new Error("Unable to fetch sales trend data. Please try again later.");
  }
}

/**
 * Get popular products
 * @param limit Number of products to return
 * @returns List of popular products
 * @endpoint GET /api/Dashboard/popular-products
 */
export async function getPopularProducts(limit: number = 5): Promise<PopularProduct[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Dashboard/popular-products?limit=${limit}`);

    if (!response.ok) {
      throw new Error("Unable to fetch popular products");
    }

    return response.json();
  } catch (error) {
    console.error("Get popular products error:", error);
    throw new Error("Unable to fetch popular products. Please try again later.");
  }
}

/**
 * Get sales by category
 * @returns List of category sales data
 * @endpoint GET /api/Dashboard/sales-by-category
 */
export async function getSalesByCategory(): Promise<CategorySales[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Dashboard/sales-by-category`);

    if (!response.ok) {
      throw new Error("Unable to fetch sales by category");
    }

    return response.json();
  } catch (error) {
    console.error("Get sales by category error:", error);
    throw new Error("Unable to fetch sales by category. Please try again later.");
  }
}

/**
 * Get order status distribution
 * @returns Order status distribution object
 * @endpoint GET /api/Dashboard/order-status-distribution
 */
export async function getOrderStatusDistribution(): Promise<OrderStatusDistribution> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Dashboard/order-status-distribution`);

    if (!response.ok) {
      throw new Error("Unable to fetch order status distribution");
    }

    return response.json();
  } catch (error) {
    console.error("Get order status distribution error:", error);
    throw new Error("Unable to fetch order status distribution. Please try again later.");
  }
}
