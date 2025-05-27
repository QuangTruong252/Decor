import type { OrderDTO, OrderStatus } from '@/api/types';

export interface OrderMetrics {
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  conversionRate: number;
  ordersByStatus: Record<OrderStatus, number>;
  revenueByMonth: Array<{ month: string; revenue: number; orders: number }>;
  topProducts: Array<{ productId: number; productName: string; quantity: number; revenue: number }>;
  customerMetrics: {
    newCustomers: number;
    returningCustomers: number;
    averageOrdersPerCustomer: number;
  };
}

export interface OrderAnalyticsFilter {
  startDate?: string;
  endDate?: string;
  status?: OrderStatus[];
  customerId?: number;
  productId?: number;
  minAmount?: number;
  maxAmount?: number;
}

export interface DashboardStats {
  todayOrders: number;
  todayRevenue: number;
  weeklyGrowth: number;
  monthlyGrowth: number;
  pendingOrders: number;
  processingOrders: number;
  shippedOrders: number;
  deliveredOrders: number;
}

export interface RevenueAnalytics {
  daily: Array<{ date: string; revenue: number; orders: number }>;
  weekly: Array<{ week: string; revenue: number; orders: number }>;
  monthly: Array<{ month: string; revenue: number; orders: number }>;
  yearly: Array<{ year: string; revenue: number; orders: number }>;
}

export interface CustomerAnalytics {
  totalCustomers: number;
  newCustomersThisMonth: number;
  customerLifetimeValue: number;
  customerRetentionRate: number;
  topCustomers: Array<{
    customerId: number;
    customerName: string;
    totalOrders: number;
    totalSpent: number;
    lastOrderDate: string;
  }>;
}

export class OrderAnalyticsService {
  private static orders: OrderDTO[] = [];

  /**
   * Set orders data for analytics
   */
  static setOrdersData(orders: OrderDTO[]): void {
    this.orders = orders;
  }

  /**
   * Get comprehensive order metrics
   */
  static getOrderMetrics(filter?: OrderAnalyticsFilter): OrderMetrics {
    const filteredOrders = this.filterOrders(filter);

    const totalOrders = filteredOrders.length;
    const totalRevenue = filteredOrders.reduce((sum, order) => sum + order.totalAmount, 0);
    const averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

    // Calculate conversion rate (mock data - would need visitor data in real app)
    const conversionRate = 0.025; // 2.5% mock conversion rate

    // Orders by status
    const ordersByStatus = filteredOrders.reduce((acc, order) => {
      const status = order.orderStatus as OrderStatus;
      acc[status] = (acc[status] || 0) + 1;
      return acc;
    }, {} as Record<OrderStatus, number>);

    // Revenue by month
    const revenueByMonth = this.getRevenueByMonth(filteredOrders);

    // Top products
    const topProducts = this.getTopProducts(filteredOrders);

    // Customer metrics
    const customerMetrics = this.getCustomerMetrics(filteredOrders);

    return {
      totalOrders,
      totalRevenue,
      averageOrderValue,
      conversionRate,
      ordersByStatus,
      revenueByMonth,
      topProducts,
      customerMetrics
    };
  }

  /**
   * Get dashboard statistics
   */
  static getDashboardStats(): DashboardStats {
    const today = new Date();
    const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const weekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
    const monthAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);

    // Today's orders
    const todayOrders = this.orders.filter(order =>
      new Date(order.orderDate) >= todayStart
    );

    // Last week's orders
    const lastWeekOrders = this.orders.filter(order => {
      const orderDate = new Date(order.orderDate);
      return orderDate >= weekAgo && orderDate < todayStart;
    });

    // Last month's orders
    const lastMonthOrders = this.orders.filter(order => {
      const orderDate = new Date(order.orderDate);
      return orderDate >= monthAgo && orderDate < weekAgo;
    });

    const todayRevenue = todayOrders.reduce((sum, order) => sum + order.totalAmount, 0);
    const lastWeekRevenue = lastWeekOrders.reduce((sum, order) => sum + order.totalAmount, 0);
    const lastMonthRevenue = lastMonthOrders.reduce((sum, order) => sum + order.totalAmount, 0);

    // Calculate growth rates
    const weeklyGrowth = lastWeekRevenue > 0
      ? ((todayRevenue - lastWeekRevenue) / lastWeekRevenue) * 100
      : 0;

    const monthlyGrowth = lastMonthRevenue > 0
      ? ((todayRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
      : 0;

    // Orders by status
    const pendingOrders = this.orders.filter(order => order.orderStatus === 'pending').length;
    const processingOrders = this.orders.filter(order => order.orderStatus === 'processing').length;
    const shippedOrders = this.orders.filter(order => order.orderStatus === 'shipped').length;
    const deliveredOrders = this.orders.filter(order => order.orderStatus === 'delivered').length;

    return {
      todayOrders: todayOrders.length,
      todayRevenue,
      weeklyGrowth,
      monthlyGrowth,
      pendingOrders,
      processingOrders,
      shippedOrders,
      deliveredOrders
    };
  }

  /**
   * Get revenue analytics
   */
  static getRevenueAnalytics(period: 'daily' | 'weekly' | 'monthly' | 'yearly' = 'monthly'): RevenueAnalytics[keyof RevenueAnalytics] {
    switch (period) {
      case 'daily':
        return this.getDailyRevenue();
      case 'weekly':
        return this.getWeeklyRevenue();
      case 'monthly':
        return this.getMonthlyRevenue();
      case 'yearly':
        return this.getYearlyRevenue();
      default:
        return this.getMonthlyRevenue();
    }
  }

  /**
   * Get customer analytics
   */
  static getCustomerAnalytics(): CustomerAnalytics {
    const uniqueCustomers = new Set(this.orders.map(order => order.userId));
    const totalCustomers = uniqueCustomers.size;

    // New customers this month
    const thisMonth = new Date();
    thisMonth.setDate(1);
    const newCustomersThisMonth = this.orders.filter(order =>
      new Date(order.orderDate) >= thisMonth
    ).length;

    // Customer lifetime value
    const customerLifetimeValue = totalCustomers > 0
      ? this.orders.reduce((sum, order) => sum + order.totalAmount, 0) / totalCustomers
      : 0;

    // Customer retention rate (simplified calculation)
    const customerRetentionRate = 0.75; // 75% mock retention rate

    // Top customers
    const customerOrderMap = new Map<number, { orders: OrderDTO[]; totalSpent: number }>();

    this.orders.forEach(order => {
      if (!customerOrderMap.has(order.userId)) {
        customerOrderMap.set(order.userId, { orders: [], totalSpent: 0 });
      }
      const customerData = customerOrderMap.get(order.userId)!;
      customerData.orders.push(order);
      customerData.totalSpent += order.totalAmount;
    });

    const topCustomers = Array.from(customerOrderMap.entries())
      .map(([customerId, data]) => ({
        customerId,
        customerName: data.orders[0].userFullName || 'Unknown Customer',
        totalOrders: data.orders.length,
        totalSpent: data.totalSpent,
        lastOrderDate: data.orders.sort((a, b) =>
          new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()
        )[0].orderDate
      }))
      .sort((a, b) => b.totalSpent - a.totalSpent)
      .slice(0, 10);

    return {
      totalCustomers,
      newCustomersThisMonth,
      customerLifetimeValue,
      customerRetentionRate,
      topCustomers
    };
  }

  /**
   * Export order data
   */
  static exportOrderData(format: 'csv' | 'json' | 'excel', filter?: OrderAnalyticsFilter): string {
    const filteredOrders = this.filterOrders(filter);

    switch (format) {
      case 'csv':
        return this.exportToCSV(filteredOrders);
      case 'json':
        return JSON.stringify(filteredOrders, null, 2);
      case 'excel':
        // In a real app, you would use a library like xlsx
        return this.exportToCSV(filteredOrders);
      default:
        return JSON.stringify(filteredOrders, null, 2);
    }
  }

  /**
   * Get order trends
   */
  static getOrderTrends(days: number = 30): Array<{ date: string; orders: number; revenue: number }> {
    const endDate = new Date();
    const startDate = new Date(endDate.getTime() - days * 24 * 60 * 60 * 1000);

    const trends: Array<{ date: string; orders: number; revenue: number }> = [];

    for (let d = new Date(startDate); d <= endDate; d.setDate(d.getDate() + 1)) {
      const dateStr = d.toISOString().split('T')[0];
      const dayOrders = this.orders.filter(order =>
        order.orderDate.startsWith(dateStr)
      );

      trends.push({
        date: dateStr,
        orders: dayOrders.length,
        revenue: dayOrders.reduce((sum, order) => sum + order.totalAmount, 0)
      });
    }

    return trends;
  }

  /**
   * Filter orders based on criteria
   */
  private static filterOrders(filter?: OrderAnalyticsFilter): OrderDTO[] {
    if (!filter) return this.orders;

    return this.orders.filter(order => {
      if (filter.startDate && order.orderDate < filter.startDate) return false;
      if (filter.endDate && order.orderDate > filter.endDate) return false;
      if (filter.status && order.orderStatus && !filter.status.includes(order.orderStatus as any)) return false;
      if (filter.customerId && order.userId !== filter.customerId) return false;
      if (filter.minAmount && order.totalAmount < filter.minAmount) return false;
      if (filter.maxAmount && order.totalAmount > filter.maxAmount) return false;

      return true;
    });
  }

  /**
   * Get revenue by month
   */
  private static getRevenueByMonth(orders: OrderDTO[]): Array<{ month: string; revenue: number; orders: number }> {
    const monthlyData = new Map<string, { revenue: number; orders: number }>();

    orders.forEach(order => {
      const month = new Date(order.orderDate).toISOString().slice(0, 7); // YYYY-MM
      if (!monthlyData.has(month)) {
        monthlyData.set(month, { revenue: 0, orders: 0 });
      }
      const data = monthlyData.get(month)!;
      data.revenue += order.totalAmount;
      data.orders += 1;
    });

    return Array.from(monthlyData.entries())
      .map(([month, data]) => ({ month, ...data }))
      .sort((a, b) => a.month.localeCompare(b.month));
  }

  /**
   * Get top products
   */
  private static getTopProducts(orders: OrderDTO[]): Array<{ productId: number; productName: string; quantity: number; revenue: number }> {
    const productMap = new Map<number, { productName: string; quantity: number; revenue: number }>();

    orders.forEach(order => {
      order.orderItems?.forEach(item => {
        if (!productMap.has(item.productId)) {
          productMap.set(item.productId, {
            productName: item.productName || 'Unknown Product',
            quantity: 0,
            revenue: 0
          });
        }
        const product = productMap.get(item.productId)!;
        product.quantity += item.quantity;
        product.revenue += item.subtotal;
      });
    });

    return Array.from(productMap.entries())
      .map(([productId, data]) => ({ productId, ...data }))
      .sort((a, b) => b.revenue - a.revenue)
      .slice(0, 10);
  }

  /**
   * Get customer metrics
   */
  private static getCustomerMetrics(orders: OrderDTO[]): OrderMetrics['customerMetrics'] {
    const uniqueCustomers = new Set(orders.map(order => order.userId));
    const totalCustomers = uniqueCustomers.size;

    // Simple calculation for new vs returning customers
    const newCustomers = Math.floor(totalCustomers * 0.3); // 30% new customers
    const returningCustomers = totalCustomers - newCustomers;

    const averageOrdersPerCustomer = totalCustomers > 0 ? orders.length / totalCustomers : 0;

    return {
      newCustomers,
      returningCustomers,
      averageOrdersPerCustomer
    };
  }

  /**
   * Get daily revenue
   */
  private static getDailyRevenue(): Array<{ date: string; revenue: number; orders: number }> {
    const last30Days = Array.from({ length: 30 }, (_, i) => {
      const date = new Date();
      date.setDate(date.getDate() - i);
      return date.toISOString().split('T')[0];
    }).reverse();

    return last30Days.map(date => {
      const dayOrders = this.orders.filter(order => order.orderDate.startsWith(date));
      return {
        date,
        revenue: dayOrders.reduce((sum, order) => sum + order.totalAmount, 0),
        orders: dayOrders.length
      };
    });
  }

  /**
   * Get weekly revenue
   */
  private static getWeeklyRevenue(): Array<{ week: string; revenue: number; orders: number }> {
    // Implementation for weekly revenue
    return [];
  }

  /**
   * Get monthly revenue
   */
  private static getMonthlyRevenue(): Array<{ month: string; revenue: number; orders: number }> {
    return this.getRevenueByMonth(this.orders);
  }

  /**
   * Get yearly revenue
   */
  private static getYearlyRevenue(): Array<{ year: string; revenue: number; orders: number }> {
    // Implementation for yearly revenue
    return [];
  }

  /**
   * Export to CSV
   */
  private static exportToCSV(orders: OrderDTO[]): string {
    const headers = ['Order ID', 'Customer', 'Date', 'Status', 'Total Amount', 'Items'];
    const rows = orders.map(order => [
      order.id,
      order.userFullName || 'Unknown Customer',
      order.orderDate,
      order.orderStatus || 'Unknown',
      order.totalAmount,
      order.orderItems?.length || 0
    ]);

    return [headers, ...rows].map(row => row.join(',')).join('\n');
  }
}

export default OrderAnalyticsService;
