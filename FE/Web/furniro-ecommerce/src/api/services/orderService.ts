import { api } from '../client';
import { ORDERS } from '../endpoints';
import type {
  OrderDTO,
  OrderDTOPagedResult,
  CreateOrderDTO,
  UpdateOrderStatusDTO,
  OrderFilters,
  BulkDeleteDTO,
} from '../types';

export class OrderService {
  /**
   * Get orders with pagination and filtering
   */
  static async getOrders(params?: OrderFilters): Promise<OrderDTOPagedResult> {
    const response = await api.get<OrderDTOPagedResult>(ORDERS.BASE, { params });
    return response.data;
  }

  /**
   * Get all orders without pagination (admin only)
   */
  static async getAllOrders(): Promise<OrderDTO[]> {
    const response = await api.get<OrderDTO[]>(ORDERS.ALL);
    return response.data;
  }

  /**
   * Get order by ID
   */
  static async getOrderById(id: number): Promise<OrderDTO> {
    const response = await api.get<OrderDTO>(ORDERS.BY_ID(id));
    return response.data;
  }

  /**
   * Get orders by user ID
   */
  static async getOrdersByUserId(userId: number): Promise<OrderDTO[]> {
    const response = await api.get<OrderDTO[]>(ORDERS.BY_USER(userId));
    return response.data;
  }

  /**
   * Create new order
   */
  static async createOrder(orderData: CreateOrderDTO): Promise<OrderDTO> {
    const response = await api.post<OrderDTO>(ORDERS.BASE, orderData);
    return response.data;
  }

  /**
   * Update order status
   */
  static async updateOrderStatus(id: number, statusData: UpdateOrderStatusDTO): Promise<void> {
    await api.put(ORDERS.UPDATE_STATUS(id), statusData);
  }

  /**
   * Delete order
   */
  static async deleteOrder(id: number): Promise<void> {
    await api.delete(ORDERS.BY_ID(id));
  }

  /**
   * Bulk delete orders
   */
  static async bulkDeleteOrders(ids: number[]): Promise<void> {
    const data: BulkDeleteDTO = { ids };
    await api.delete(ORDERS.BULK_DELETE, { data });
  }

  /**
   * Get current user's orders
   */
  static async getCurrentUserOrders(): Promise<OrderDTO[]> {
    // This assumes the API will return orders for the authenticated user
    // when no specific user ID is provided
    const response = await api.get<OrderDTO[]>(ORDERS.BASE);
    return response.data;
  }

  /**
   * Cancel order
   */
  static async cancelOrder(id: number): Promise<void> {
    await this.updateOrderStatus(id, { orderStatus: 'cancelled' });
  }

  /**
   * Get order statistics
   */
  static async getOrderStats(userId?: number): Promise<{
    totalOrders: number;
    totalSpent: number;
    averageOrderValue: number;
    ordersByStatus: Record<string, number>;
  }> {
    const orders = userId
      ? await this.getOrdersByUserId(userId)
      : await this.getCurrentUserOrders();

    const totalOrders = orders.length;
    const totalSpent = orders.reduce((sum, order) => sum + order.totalAmount, 0);
    const averageOrderValue = totalOrders > 0 ? totalSpent / totalOrders : 0;

    const ordersByStatus = orders.reduce((acc, order) => {
      const status = order.orderStatus || 'unknown';
      acc[status] = (acc[status] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    return {
      totalOrders,
      totalSpent,
      averageOrderValue,
      ordersByStatus,
    };
  }

  /**
   * Get recent orders
   */
  static async getRecentOrders(count?: number): Promise<OrderDTO[]> {
    const params = count ? { count } : undefined;
    const response = await api.get<OrderDTO[]>(ORDERS.RECENT, { params });
    return response.data;
  }

  /**
   * Search orders
   */
  static async searchOrders(query: string): Promise<OrderDTO[]> {
    const params: OrderFilters = { searchTerm: query };
    const result = await this.getOrders(params);
    return result.items;
  }

  /**
   * Get orders by status
   */
  static async getOrdersByStatus(status: string): Promise<OrderDTO[]> {
    const response = await api.get<OrderDTO[]>(ORDERS.BY_STATUS(status));
    return response.data;
  }

  /**
   * Get orders by date range
   */
  static async getOrdersByDateRange(startDate?: string, endDate?: string): Promise<OrderDTO[]> {
    const params = { startDate, endDate };
    const response = await api.get<OrderDTO[]>(ORDERS.DATE_RANGE, { params });
    return response.data;
  }

  /**
   * Get revenue for date range
   */
  static async getRevenue(startDate?: string, endDate?: string): Promise<number> {
    const params = { startDate, endDate };
    const response = await api.get<number>(ORDERS.REVENUE, { params });
    return response.data;
  }

  /**
   * Get order status counts
   */
  static async getOrderStatusCounts(): Promise<Record<string, number>> {
    const response = await api.get<Record<string, number>>(ORDERS.STATUS_COUNTS);
    return response.data;
  }

  /**
   * Check if order can be cancelled
   */
  static canCancelOrder(order: OrderDTO): boolean {
    const cancellableStatuses = ['pending', 'confirmed'];
    return order.orderStatus ? cancellableStatuses.includes(order.orderStatus.toLowerCase()) : false;
  }

  /**
   * Check if order can be returned
   */
  static canReturnOrder(order: OrderDTO): boolean {
    const returnableStatuses = ['delivered'];
    const orderDate = new Date(order.orderDate);
    const daysSinceOrder = (Date.now() - orderDate.getTime()) / (1000 * 60 * 60 * 24);

    return order.orderStatus ? returnableStatuses.includes(order.orderStatus.toLowerCase()) && daysSinceOrder <= 30 : false;
  }

  /**
   * Format order status for display
   */
  static formatOrderStatus(status: string): string {
    return status.charAt(0).toUpperCase() + status.slice(1).replace('_', ' ');
  }

  /**
   * Get order status color for UI
   */
  static getOrderStatusColor(status: string): string {
    const statusColors: Record<string, string> = {
      pending: 'yellow',
      confirmed: 'blue',
      processing: 'purple',
      shipped: 'indigo',
      delivered: 'green',
      cancelled: 'red',
      refunded: 'gray',
    };

    return statusColors[status.toLowerCase()] || 'gray';
  }
}

export default OrderService;
