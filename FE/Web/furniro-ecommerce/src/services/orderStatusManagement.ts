import type { OrderStatus, OrderDTO } from '@/api/types';

export interface OrderStatusTransition {
  from: OrderStatus;
  to: OrderStatus;
  allowedBy: 'customer' | 'admin' | 'system';
  conditions?: (order: OrderDTO) => boolean;
  description: string;
}

export interface OrderStatusInfo {
  status: OrderStatus;
  label: string;
  description: string;
  color: string;
  icon: string;
  isActive: boolean;
  isFinal: boolean;
  estimatedDuration?: number; // in hours
}

export interface OrderTimeline {
  status: OrderStatus;
  timestamp: string;
  description: string;
  updatedBy?: string;
  note?: string;
}

export class OrderStatusManagementService {
  // Define all possible order statuses with their information
  private static statusInfo: Record<OrderStatus, OrderStatusInfo> = {
    pending: {
      status: 'pending',
      label: 'Pending',
      description: 'Order is waiting for confirmation',
      color: 'yellow',
      icon: '⏳',
      isActive: true,
      isFinal: false,
      estimatedDuration: 2
    },
    confirmed: {
      status: 'confirmed',
      label: 'Confirmed',
      description: 'Order has been confirmed and will be processed',
      color: 'blue',
      icon: '✅',
      isActive: true,
      isFinal: false,
      estimatedDuration: 4
    },
    processing: {
      status: 'processing',
      label: 'Processing',
      description: 'Order is being prepared for shipment',
      color: 'purple',
      icon: '⚡',
      isActive: true,
      isFinal: false,
      estimatedDuration: 24
    },
    shipped: {
      status: 'shipped',
      label: 'Shipped',
      description: 'Order has been shipped and is on its way',
      color: 'indigo',
      icon: '🚚',
      isActive: true,
      isFinal: false,
      estimatedDuration: 72
    },
    delivered: {
      status: 'delivered',
      label: 'Delivered',
      description: 'Order has been successfully delivered',
      color: 'green',
      icon: '📦',
      isActive: false,
      isFinal: true
    },
    cancelled: {
      status: 'cancelled',
      label: 'Cancelled',
      description: 'Order has been cancelled',
      color: 'red',
      icon: '❌',
      isActive: false,
      isFinal: true
    },
    refunded: {
      status: 'refunded',
      label: 'Refunded',
      description: 'Order has been refunded',
      color: 'gray',
      icon: '↩️',
      isActive: false,
      isFinal: true
    }
  };

  // Define allowed status transitions
  private static transitions: OrderStatusTransition[] = [
    // From pending
    {
      from: 'pending',
      to: 'confirmed',
      allowedBy: 'admin',
      description: 'Confirm the order'
    },
    {
      from: 'pending',
      to: 'cancelled',
      allowedBy: 'customer',
      description: 'Cancel the order'
    },

    // From confirmed
    {
      from: 'confirmed',
      to: 'processing',
      allowedBy: 'admin',
      description: 'Start processing the order'
    },
    {
      from: 'confirmed',
      to: 'cancelled',
      allowedBy: 'customer',
      conditions: (order) => {
        // Allow cancellation within 1 hour of confirmation
        const confirmTime = new Date(order.updatedAt).getTime();
        const now = Date.now();
        return (now - confirmTime) < (60 * 60 * 1000);
      },
      description: 'Cancel the order (within 1 hour)'
    },

    // From processing
    {
      from: 'processing',
      to: 'shipped',
      allowedBy: 'admin',
      description: 'Ship the order'
    },
    {
      from: 'processing',
      to: 'cancelled',
      allowedBy: 'admin',
      description: 'Cancel the order (admin only)'
    },

    // From shipped
    {
      from: 'shipped',
      to: 'delivered',
      allowedBy: 'system',
      description: 'Mark as delivered'
    },

    // From delivered
    {
      from: 'delivered',
      to: 'refunded',
      allowedBy: 'admin',
      conditions: (order) => {
        // Allow refund within 30 days of delivery
        const deliveryTime = new Date(order.updatedAt).getTime();
        const now = Date.now();
        return (now - deliveryTime) < (30 * 24 * 60 * 60 * 1000);
      },
      description: 'Process refund (within 30 days)'
    }
  ];

  /**
   * Get status information
   */
  static getStatusInfo(status: OrderStatus): OrderStatusInfo {
    return this.statusInfo[status];
  }

  /**
   * Get all available statuses
   */
  static getAllStatuses(): OrderStatusInfo[] {
    return Object.values(this.statusInfo);
  }

  /**
   * Get active statuses (non-final)
   */
  static getActiveStatuses(): OrderStatusInfo[] {
    return Object.values(this.statusInfo).filter(info => info.isActive);
  }

  /**
   * Check if status transition is allowed
   */
  static canTransitionTo(
    order: OrderDTO,
    newStatus: OrderStatus,
    userRole: 'customer' | 'admin' | 'system' = 'customer'
  ): boolean {
    const transition = this.transitions.find(
      t => t.from === order.orderStatus && t.to === newStatus
    );

    if (!transition) {
      return false;
    }

    // Check if user role is allowed
    if (transition.allowedBy !== userRole && transition.allowedBy !== 'system') {
      return false;
    }

    // Check additional conditions
    if (transition.conditions && !transition.conditions(order)) {
      return false;
    }

    return true;
  }

  /**
   * Get allowed transitions for an order
   */
  static getAllowedTransitions(
    order: OrderDTO,
    userRole: 'customer' | 'admin' | 'system' = 'customer'
  ): OrderStatusTransition[] {
    return this.transitions.filter(transition =>
      transition.from === order.orderStatus &&
      (transition.allowedBy === userRole || transition.allowedBy === 'system') &&
      (!transition.conditions || transition.conditions(order))
    );
  }

  /**
   * Get next possible statuses for an order
   */
  static getNextStatuses(
    order: OrderDTO,
    userRole: 'customer' | 'admin' | 'system' = 'customer'
  ): OrderStatus[] {
    return this.getAllowedTransitions(order, userRole).map(t => t.to);
  }

  /**
   * Validate status transition
   */
  static validateTransition(
    order: OrderDTO,
    newStatus: OrderStatus,
    userRole: 'customer' | 'admin' | 'system' = 'customer'
  ): { isValid: boolean; reason?: string } {
    if (order.orderStatus === newStatus) {
      return { isValid: false, reason: 'Order is already in this status' };
    }

    const transition = this.transitions.find(
      t => t.from === order.orderStatus && t.to === newStatus
    );

    if (!transition) {
      return {
        isValid: false,
        reason: `Cannot transition from ${order.orderStatus} to ${newStatus}`
      };
    }

    if (transition.allowedBy !== userRole && transition.allowedBy !== 'system') {
      return {
        isValid: false,
        reason: `Only ${transition.allowedBy} can perform this transition`
      };
    }

    if (transition.conditions && !transition.conditions(order)) {
      return {
        isValid: false,
        reason: 'Transition conditions not met'
      };
    }

    return { isValid: true };
  }

  /**
   * Get estimated delivery date
   */
  static getEstimatedDeliveryDate(order: OrderDTO): Date | null {
    const status = order.orderStatus || 'pending';
    const statusInfo = this.getStatusInfo(status as OrderStatus);

    if (!statusInfo.estimatedDuration || statusInfo.isFinal) {
      return null;
    }

    const orderDate = new Date(order.orderDate);
    const totalHours = this.getTotalEstimatedHours(status as OrderStatus);

    return new Date(orderDate.getTime() + (totalHours * 60 * 60 * 1000));
  }

  /**
   * Get total estimated hours from order date to current status
   */
  private static getTotalEstimatedHours(currentStatus: OrderStatus): number {
    const statusOrder: OrderStatus[] = ['pending', 'confirmed', 'processing', 'shipped', 'delivered'];
    const currentIndex = statusOrder.indexOf(currentStatus);

    let totalHours = 0;
    for (let i = 0; i <= currentIndex; i++) {
      const status = statusOrder[i];
      const info = this.statusInfo[status];
      if (info.estimatedDuration) {
        totalHours += info.estimatedDuration;
      }
    }

    return totalHours;
  }

  /**
   * Generate order timeline
   */
  static generateTimeline(order: OrderDTO): OrderTimeline[] {
    const timeline: OrderTimeline[] = [];

    // Add order placed
    timeline.push({
      status: 'pending',
      timestamp: order.orderDate,
      description: 'Order placed',
      updatedBy: order.userFullName || 'Customer'
    });

    // Add current status if different from pending
    const currentStatus = order.orderStatus || 'pending';
    if (currentStatus !== 'pending') {
      timeline.push({
        status: currentStatus as OrderStatus,
        timestamp: order.updatedAt,
        description: this.getStatusInfo(currentStatus as OrderStatus).description,
        updatedBy: 'System'
      });
    }

    return timeline.sort((a, b) =>
      new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
    );
  }

  /**
   * Check if order can be cancelled by customer
   */
  static canCustomerCancel(order: OrderDTO): boolean {
    return this.canTransitionTo(order, 'cancelled', 'customer');
  }

  /**
   * Check if order can be returned/refunded
   */
  static canReturn(order: OrderDTO): boolean {
    return this.canTransitionTo(order, 'refunded', 'customer');
  }

  /**
   * Get status badge class for UI
   */
  static getStatusBadgeClass(status: OrderStatus): string {
    const info = this.getStatusInfo(status);
    const baseClass = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium';

    const colorClasses = {
      yellow: 'bg-yellow-100 text-yellow-800',
      blue: 'bg-blue-100 text-blue-800',
      purple: 'bg-purple-100 text-purple-800',
      indigo: 'bg-indigo-100 text-indigo-800',
      green: 'bg-green-100 text-green-800',
      red: 'bg-red-100 text-red-800',
      gray: 'bg-gray-100 text-gray-800',
    };

    return `${baseClass} ${colorClasses[info.color as keyof typeof colorClasses] || colorClasses.gray}`;
  }

  /**
   * Get progress percentage for order
   */
  static getOrderProgress(status: OrderStatus): number {
    const statusOrder: OrderStatus[] = ['pending', 'confirmed', 'processing', 'shipped', 'delivered'];
    const currentIndex = statusOrder.indexOf(status);

    if (currentIndex === -1) {
      return 0; // For cancelled/refunded orders
    }

    return Math.round((currentIndex / (statusOrder.length - 1)) * 100);
  }

  /**
   * Get order status color for charts/analytics
   */
  static getStatusColor(status: OrderStatus): string {
    return this.getStatusInfo(status).color;
  }

  /**
   * Format status for display
   */
  static formatStatus(status: OrderStatus): string {
    return this.getStatusInfo(status).label;
  }
}

export default OrderStatusManagementService;
