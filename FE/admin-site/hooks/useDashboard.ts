"use client";

import { useQuery } from "@tanstack/react-query";
import {
  getDashboardSummary,
  getSalesTrend,
  getPopularProducts,
  getSalesByCategory,
  getOrderStatusDistribution,
  type SalesTrendItem,
  type PopularProduct,
  type CategorySales,
  type OrderStatusCount,
  type OrderStatusDistribution,
  type RecentOrder
} from "@/services/dashboard";
import { getProducts } from "@/services/products";
import { getOrders } from "@/services/orders";
import { getCustomers } from "@/services/customers";

/**
 * Fallback dashboard data implementation using existing endpoints
 * This is used if the dedicated dashboard endpoints are not available
 */
function useFallbackDashboardData() {
  const productsQuery = useQuery({
    queryKey: ["products"],
    queryFn: getProducts,
  });

  const ordersQuery = useQuery({
    queryKey: ["orders"],
    queryFn: getOrders,
  });

  const customersQuery = useQuery({
    queryKey: ["customers"],
    queryFn: getCustomers,
  });

  // Calculate total revenue from orders
  const totalRevenue = ordersQuery.data?.reduce((sum, order) => sum + order.totalAmount, 0) || 0;

  // Calculate order status distribution
  const orderStatusDistribution = ordersQuery.data ? calculateOrderStatusDistribution(ordersQuery.data) : [];

  // Calculate popular products based on order items
  const popularProducts = ordersQuery.data && productsQuery.data
    ? calculatePopularProducts(ordersQuery.data, productsQuery.data)
    : [];

  // Generate mock sales trend data if real data is not available
  const salesTrend = generateMockSalesTrend();

  // Convert order status distribution to the new format
  const orderStatusObj: OrderStatusDistribution = {
    pending: 0,
    processing: 0,
    shipped: 0,
    delivered: 0,
    cancelled: 0,
    total: 0
  };

  orderStatusDistribution.forEach(item => {
    const status = item.status.toLowerCase();
    if (status in orderStatusObj) {
      orderStatusObj[status as keyof OrderStatusDistribution] = item.count;
      orderStatusObj.total += item.count;
    }
  });

  // We don't need to convert popular products since we've updated the calculatePopularProducts function

  // Convert recent orders to the new format
  const recentOrders = ordersQuery.data?.slice(0, 5).map(order => ({
    orderId: order.id,
    orderDate: order.orderDate,
    customerName: order.userFullName || "Guest",
    totalAmount: order.totalAmount,
    orderStatus: order.orderStatus || "Unknown"
  })) || [];

  return {
    isLoading: productsQuery.isLoading || ordersQuery.isLoading || customersQuery.isLoading,
    error: productsQuery.error || ordersQuery.error || customersQuery.error,
    data: {
      totalProducts: productsQuery.data?.length || 0,
      totalOrders: ordersQuery.data?.length || 0,
      totalCustomers: customersQuery.data?.length || 0,
      totalRevenue,
      orderStatusDistribution: orderStatusObj,
      popularProducts,
      salesTrend,
      recentSalesTrend: salesTrend, // Use the same data for both
      recentOrders,
      salesByCategory: []
    }
  };
}

/**
 * Hook to fetch dashboard summary data
 */
export function useGetDashboardSummary() {
  return useQuery({
    queryKey: ["dashboard", "summary"],
    queryFn: getDashboardSummary,
    retry: 1,
  });
}

/**
 * Hook to fetch sales trend data
 * @param period Period for sales trend (daily, weekly, monthly)
 * @param startDate Optional start date for custom period
 * @param endDate Optional end date for custom period
 */
export function useGetSalesTrend(
  period: "daily" | "weekly" | "monthly" = "daily",
  startDate?: string,
  endDate?: string
) {
  return useQuery({
    queryKey: ["dashboard", "sales-trend", period, startDate, endDate],
    queryFn: () => getSalesTrend(period, startDate, endDate),
    retry: 1,
  });
}

/**
 * Hook to fetch popular products
 * @param limit Number of products to return
 */
export function useGetPopularProducts(limit: number = 5) {
  return useQuery({
    queryKey: ["dashboard", "popular-products", limit],
    queryFn: () => getPopularProducts(limit),
    retry: 1,
  });
}

/**
 * Hook to fetch sales by category
 */
export function useGetSalesByCategory() {
  return useQuery({
    queryKey: ["dashboard", "sales-by-category"],
    queryFn: getSalesByCategory,
    retry: 1,
  });
}

/**
 * Hook to fetch order status distribution
 */
export function useGetOrderStatusDistribution() {
  return useQuery({
    queryKey: ["dashboard", "order-status-distribution"],
    queryFn: getOrderStatusDistribution,
    retry: 1,
  });
}

/**
 * Hook to fetch dashboard data from dedicated dashboard endpoints
 */
export function useGetBasicDashboardData() {
  // Use the dashboard summary endpoint to get all dashboard data at once
  const dashboardQuery = useQuery({
    queryKey: ["dashboard", "summary"],
    queryFn: getDashboardSummary,
    retry: 1,
  });

  // Fallback to individual queries if the summary endpoint fails
  const salesTrendQuery = useQuery({
    queryKey: ["dashboard", "sales-trend", "daily"],
    queryFn: () => getSalesTrend("daily"),
    enabled: !!dashboardQuery.error,
    retry: 1,
  });

  const popularProductsQuery = useQuery({
    queryKey: ["dashboard", "popular-products", 5],
    queryFn: () => getPopularProducts(5),
    enabled: !!dashboardQuery.error,
    retry: 1,
  });

  const orderStatusQuery = useQuery({
    queryKey: ["dashboard", "order-status-distribution"],
    queryFn: getOrderStatusDistribution,
    enabled: !!dashboardQuery.error,
    retry: 1,
  });

  const salesByCategoryQuery = useQuery({
    queryKey: ["dashboard", "sales-by-category"],
    queryFn: getSalesByCategory,
    enabled: !!dashboardQuery.error,
    retry: 1,
  });

  // If the dashboard summary endpoint succeeds, use its data
  if (!dashboardQuery.error && dashboardQuery.data) {
    return {
      isLoading: dashboardQuery.isLoading,
      error: dashboardQuery.error,
      data: dashboardQuery.data
    };
  }

  // If the dashboard summary endpoint fails, use the individual queries
  const isLoading =
    salesTrendQuery.isLoading ||
    popularProductsQuery.isLoading ||
    orderStatusQuery.isLoading ||
    salesByCategoryQuery.isLoading;

  const error =
    salesTrendQuery.error ||
    popularProductsQuery.error ||
    orderStatusQuery.error ||
    salesByCategoryQuery.error;

  // Fallback to the original implementation if all dashboard endpoints fail
  if (error) {
    // Use the fallback implementation with existing endpoints
    const fallbackData = useFallbackDashboardData();
    return fallbackData;
  }

  return {
    isLoading,
    error,
    data: {
      totalProducts: 0, // Will be filled by the dashboard page
      totalOrders: 0,   // Will be filled by the dashboard page
      totalCustomers: 0, // Will be filled by the dashboard page
      totalRevenue: 0,   // Will be filled by the dashboard page
      salesTrend: salesTrendQuery.data || [],
      recentSalesTrend: [], // For compatibility with new API
      popularProducts: popularProductsQuery.data || [],
      orderStatusDistribution: orderStatusQuery.data || [],
      salesByCategory: salesByCategoryQuery.data || [],
      recentOrders: [] // Empty array as fallback
    }
  };
}

/**
 * Helper function to calculate order status distribution
 */
function calculateOrderStatusDistribution(orders: any[]): OrderStatusCount[] {
  const statusMap = new Map<string, number>();

  orders.forEach(order => {
    const status = order.orderStatus || 'Unknown';
    statusMap.set(status, (statusMap.get(status) || 0) + 1);
  });

  return Array.from(statusMap.entries()).map(([status, count]) => ({
    status,
    count
  }));
}

/**
 * Helper function to calculate popular products based on order items
 */
function calculatePopularProducts(orders: any[], products: any[]): PopularProduct[] {
  const productSalesMap = new Map<number, { totalSold: number, totalRevenue: number }>();

  // Count sales for each product
  orders.forEach(order => {
    if (order.orderItems) {
      order.orderItems.forEach((item: any) => {
        const currentStats = productSalesMap.get(item.productId) || { totalSold: 0, totalRevenue: 0 };
        productSalesMap.set(item.productId, {
          totalSold: currentStats.totalSold + item.quantity,
          totalRevenue: currentStats.totalRevenue + item.subtotal
        });
      });
    }
  });

  // Map to product details and sort by sales
  const popularProducts = Array.from(productSalesMap.entries())
    .map(([productId, stats]) => {
      const product = products.find(p => p.id === productId);
      return {
        productId: productId,
        name: product?.name || `Product #${productId}`,
        totalSold: stats.totalSold,
        totalRevenue: stats.totalRevenue,
        price: product?.price || 0,
        imageUrl: product?.images?.[0] || undefined
      };
    })
    .sort((a, b) => b.totalSold - a.totalSold)
    .slice(0, 5);

  return popularProducts;
}

/**
 * Helper function to generate mock sales trend data
 */
function generateMockSalesTrend(): SalesTrendItem[] {
  const today = new Date();
  const salesTrend: SalesTrendItem[] = [];

  for (let i = 30; i >= 0; i--) {
    const date = new Date(today);
    date.setDate(date.getDate() - i);

    // Generate some random data
    const revenue = Math.floor(Math.random() * 10000) + 1000;
    const orderCount = Math.floor(Math.random() * 20) + 5;

    salesTrend.push({
      date: date.toISOString().split('T')[0],
      revenue,
      orderCount
    });
  }

  return salesTrend;
}
