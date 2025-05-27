import type { OrderDTO, OrderStatus } from '@/api/types';

export type NotificationType = 'email' | 'sms' | 'push' | 'in-app';
export type NotificationPriority = 'low' | 'medium' | 'high' | 'urgent';

export interface NotificationTemplate {
  id: string;
  type: NotificationType;
  subject: string;
  title: string;
  message: string;
  htmlContent?: string;
  variables: string[];
}

export interface NotificationPreferences {
  email: boolean;
  sms: boolean;
  push: boolean;
  inApp: boolean;
  orderUpdates: boolean;
  promotions: boolean;
  reminders: boolean;
}

export interface Notification {
  id: string;
  userId: number;
  type: NotificationType;
  priority: NotificationPriority;
  title: string;
  message: string;
  data?: any;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
  expiresAt?: string;
}

export class NotificationService {
  private static notifications: Map<string, Notification> = new Map();
  private static templates: Map<string, NotificationTemplate> = new Map();
  private static userPreferences: Map<number, NotificationPreferences> = new Map();

  // Initialize default templates
  static {
    this.initializeTemplates();
  }

  /**
   * Initialize notification templates
   */
  private static initializeTemplates() {
    const templates: NotificationTemplate[] = [
      {
        id: 'order_confirmation',
        type: 'email',
        subject: 'Order Confirmation - Order #{orderId}',
        title: 'Order Confirmed!',
        message: 'Thank you for your order! Your order #{orderId} has been confirmed and will be processed soon.',
        htmlContent: `
          <h2>Order Confirmed!</h2>
          <p>Thank you for your order, {customerName}!</p>
          <p>Your order #{orderId} for {totalAmount} has been confirmed and will be processed soon.</p>
          <p>Order Details:</p>
          <ul>{orderItems}</ul>
          <p>Estimated delivery: {estimatedDelivery}</p>
        `,
        variables: ['orderId', 'customerName', 'totalAmount', 'orderItems', 'estimatedDelivery']
      },
      {
        id: 'order_shipped',
        type: 'email',
        subject: 'Your Order Has Shipped - Order #{orderId}',
        title: 'Order Shipped!',
        message: 'Great news! Your order #{orderId} has been shipped and is on its way to you.',
        htmlContent: `
          <h2>Your Order Has Shipped!</h2>
          <p>Great news, {customerName}!</p>
          <p>Your order #{orderId} has been shipped and is on its way to you.</p>
          <p>Tracking Number: {trackingNumber}</p>
          <p>Estimated delivery: {estimatedDelivery}</p>
          <p>You can track your package using the tracking number above.</p>
        `,
        variables: ['orderId', 'customerName', 'trackingNumber', 'estimatedDelivery']
      },
      {
        id: 'order_delivered',
        type: 'email',
        subject: 'Order Delivered - Order #{orderId}',
        title: 'Order Delivered!',
        message: 'Your order #{orderId} has been successfully delivered. We hope you enjoy your purchase!',
        htmlContent: `
          <h2>Order Delivered!</h2>
          <p>Hi {customerName},</p>
          <p>Your order #{orderId} has been successfully delivered.</p>
          <p>We hope you enjoy your purchase! If you have any questions or concerns, please don't hesitate to contact us.</p>
          <p>Don't forget to leave a review for the items you purchased.</p>
        `,
        variables: ['orderId', 'customerName']
      },
      {
        id: 'order_cancelled',
        type: 'email',
        subject: 'Order Cancelled - Order #{orderId}',
        title: 'Order Cancelled',
        message: 'Your order #{orderId} has been cancelled. If you paid online, your refund will be processed within 3-5 business days.',
        htmlContent: `
          <h2>Order Cancelled</h2>
          <p>Hi {customerName},</p>
          <p>Your order #{orderId} has been cancelled as requested.</p>
          <p>If you paid online, your refund of {totalAmount} will be processed within 3-5 business days.</p>
          <p>If you have any questions, please contact our customer support.</p>
        `,
        variables: ['orderId', 'customerName', 'totalAmount']
      },
      {
        id: 'payment_failed',
        type: 'email',
        subject: 'Payment Failed - Order #{orderId}',
        title: 'Payment Failed',
        message: 'We were unable to process payment for your order #{orderId}. Please update your payment method.',
        htmlContent: `
          <h2>Payment Failed</h2>
          <p>Hi {customerName},</p>
          <p>We were unable to process payment for your order #{orderId}.</p>
          <p>Please log in to your account and update your payment method to complete your order.</p>
          <p>Your order will be held for 24 hours before being cancelled.</p>
        `,
        variables: ['orderId', 'customerName']
      }
    ];

    templates.forEach(template => {
      this.templates.set(template.id, template);
    });
  }

  /**
   * Send order notification
   */
  static async sendOrderNotification(
    order: OrderDTO,
    templateId: string,
    additionalData?: Record<string, any>
  ): Promise<void> {
    const template = this.templates.get(templateId);
    if (!template) {
      throw new Error(`Template ${templateId} not found`);
    }

    const preferences = this.getUserPreferences(order.userId);

    // Check if user wants order updates
    if (!preferences.orderUpdates) {
      console.log(`User ${order.userId} has disabled order notifications`);
      return;
    }

    // Prepare notification data
    const notificationData = {
      orderId: order.id,
      customerName: order.userFullName || 'Customer',
      totalAmount: `$${order.totalAmount.toFixed(2)}`,
      orderItems: order.orderItems?.map(item =>
        `${item.productName || 'Product'} (x${item.quantity})`
      ).join(', ') || 'No items',
      estimatedDelivery: this.getEstimatedDeliveryDate(order),
      trackingNumber: additionalData?.trackingNumber || 'TRK123456789',
      ...additionalData
    };

    // Send notifications based on user preferences
    if (preferences.email) {
      await this.sendEmailNotification(order.userId, template, notificationData);
    }

    if (preferences.inApp) {
      await this.sendInAppNotification(order.userId, template, notificationData);
    }

    if (preferences.push) {
      await this.sendPushNotification(order.userId, template, notificationData);
    }

    if (preferences.sms) {
      await this.sendSMSNotification(order.userId, template, notificationData);
    }
  }

  /**
   * Send email notification
   */
  private static async sendEmailNotification(
    userId: number,
    template: NotificationTemplate,
    data: Record<string, any>
  ): Promise<void> {
    const subject = this.replaceVariables(template.subject, data);
    const htmlContent = this.replaceVariables(template.htmlContent || template.message, data);

    // Mock email sending
    console.log('Sending email notification:', {
      to: `user${userId}@example.com`,
      subject,
      html: htmlContent
    });

    // In a real app, you would integrate with an email service like:
    // - SendGrid
    // - Mailgun
    // - AWS SES
    // - Nodemailer
  }

  /**
   * Send in-app notification
   */
  private static async sendInAppNotification(
    userId: number,
    template: NotificationTemplate,
    data: Record<string, any>
  ): Promise<void> {
    const notification: Notification = {
      id: this.generateNotificationId(),
      userId,
      type: 'in-app',
      priority: 'medium',
      title: this.replaceVariables(template.title, data),
      message: this.replaceVariables(template.message, data),
      data,
      isRead: false,
      createdAt: new Date().toISOString()
    };

    this.notifications.set(notification.id, notification);

    // Trigger real-time notification (WebSocket, Server-Sent Events, etc.)
    this.triggerRealTimeNotification(notification);
  }

  /**
   * Send push notification
   */
  private static async sendPushNotification(
    userId: number,
    template: NotificationTemplate,
    data: Record<string, any>
  ): Promise<void> {
    const title = this.replaceVariables(template.title, data);
    const message = this.replaceVariables(template.message, data);

    // Mock push notification
    console.log('Sending push notification:', {
      userId,
      title,
      message,
      data
    });

    // In a real app, you would integrate with:
    // - Firebase Cloud Messaging (FCM)
    // - Apple Push Notification Service (APNs)
    // - Web Push API
  }

  /**
   * Send SMS notification
   */
  private static async sendSMSNotification(
    userId: number,
    template: NotificationTemplate,
    data: Record<string, any>
  ): Promise<void> {
    const message = this.replaceVariables(template.message, data);

    // Mock SMS sending
    console.log('Sending SMS notification:', {
      to: `+1234567890`, // User's phone number
      message
    });

    // In a real app, you would integrate with:
    // - Twilio
    // - AWS SNS
    // - Nexmo/Vonage
  }

  /**
   * Replace variables in template
   */
  private static replaceVariables(text: string, data: Record<string, any>): string {
    let result = text;

    Object.entries(data).forEach(([key, value]) => {
      const regex = new RegExp(`{${key}}`, 'g');
      result = result.replace(regex, String(value));
    });

    return result;
  }

  /**
   * Get user notification preferences
   */
  static getUserPreferences(userId: number): NotificationPreferences {
    return this.userPreferences.get(userId) || {
      email: true,
      sms: false,
      push: true,
      inApp: true,
      orderUpdates: true,
      promotions: false,
      reminders: true
    };
  }

  /**
   * Update user notification preferences
   */
  static updateUserPreferences(userId: number, preferences: Partial<NotificationPreferences>): void {
    const currentPreferences = this.getUserPreferences(userId);
    const updatedPreferences = { ...currentPreferences, ...preferences };
    this.userPreferences.set(userId, updatedPreferences);
  }

  /**
   * Get user notifications
   */
  static getUserNotifications(userId: number, limit: number = 20): Notification[] {
    return Array.from(this.notifications.values())
      .filter(notification => notification.userId === userId)
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      .slice(0, limit);
  }

  /**
   * Mark notification as read
   */
  static markAsRead(notificationId: string): void {
    const notification = this.notifications.get(notificationId);
    if (notification) {
      notification.isRead = true;
      notification.readAt = new Date().toISOString();
      this.notifications.set(notificationId, notification);
    }
  }

  /**
   * Mark all notifications as read for user
   */
  static markAllAsRead(userId: number): void {
    Array.from(this.notifications.values())
      .filter(notification => notification.userId === userId && !notification.isRead)
      .forEach(notification => {
        notification.isRead = true;
        notification.readAt = new Date().toISOString();
        this.notifications.set(notification.id, notification);
      });
  }

  /**
   * Get unread notification count
   */
  static getUnreadCount(userId: number): number {
    return Array.from(this.notifications.values())
      .filter(notification => notification.userId === userId && !notification.isRead)
      .length;
  }

  /**
   * Delete notification
   */
  static deleteNotification(notificationId: string): void {
    this.notifications.delete(notificationId);
  }

  /**
   * Send order status update notification
   */
  static async sendOrderStatusUpdate(order: OrderDTO, newStatus: OrderStatus): Promise<void> {
    const templateMap: Record<OrderStatus, string> = {
      pending: 'order_confirmation',
      confirmed: 'order_confirmation',
      processing: 'order_confirmation',
      shipped: 'order_shipped',
      delivered: 'order_delivered',
      cancelled: 'order_cancelled',
      refunded: 'order_cancelled'
    };

    const templateId = templateMap[newStatus];
    if (templateId) {
      await this.sendOrderNotification(order, templateId);
    }
  }

  /**
   * Generate notification ID
   */
  private static generateNotificationId(): string {
    return `notif_${Date.now()}_${Math.random().toString(36).substring(2, 11)}`;
  }

  /**
   * Get estimated delivery date
   */
  private static getEstimatedDeliveryDate(order: OrderDTO): string {
    const orderDate = new Date(order.orderDate);
    const estimatedDate = new Date(orderDate);
    estimatedDate.setDate(orderDate.getDate() + 7); // Add 7 days

    return estimatedDate.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  /**
   * Trigger real-time notification
   */
  private static triggerRealTimeNotification(notification: Notification): void {
    // In a real app, you would emit this through WebSocket or Server-Sent Events
    console.log('Real-time notification triggered:', notification);

    // Dispatch custom event for UI to listen to
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new CustomEvent('newNotification', {
        detail: notification
      }));
    }
  }

  /**
   * Clear all notifications (for testing)
   */
  static clearAllNotifications(): void {
    this.notifications.clear();
  }
}

export default NotificationService;
