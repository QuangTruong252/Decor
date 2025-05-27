import type { PaymentMethod, PaymentDetails } from '@/api/types';

export type PaymentStatus = 'pending' | 'processing' | 'completed' | 'failed' | 'cancelled' | 'refunded';

export interface PaymentTransaction {
  id: string;
  orderId: number;
  amount: number;
  currency: string;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  transactionId?: string;
  gatewayResponse?: any;
  createdAt: string;
  updatedAt: string;
  completedAt?: string;
  failureReason?: string;
  refundAmount?: number;
  refundedAt?: string;
}

export interface PaymentStatusUpdate {
  status: PaymentStatus;
  transactionId?: string;
  gatewayResponse?: any;
  failureReason?: string;
  timestamp: string;
}

export class PaymentTrackingService {
  private static transactions: Map<string, PaymentTransaction> = new Map();
  private static statusHistory: Map<string, PaymentStatusUpdate[]> = new Map();

  /**
   * Create a new payment transaction
   */
  static createTransaction(
    orderId: number,
    amount: number,
    paymentMethod: PaymentMethod,
    currency: string = 'USD'
  ): PaymentTransaction {
    const transactionId = this.generateTransactionId();
    
    const transaction: PaymentTransaction = {
      id: transactionId,
      orderId,
      amount,
      currency,
      paymentMethod,
      status: 'pending',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };

    this.transactions.set(transactionId, transaction);
    this.statusHistory.set(transactionId, [{
      status: 'pending',
      timestamp: new Date().toISOString()
    }]);

    return transaction;
  }

  /**
   * Update payment status
   */
  static updatePaymentStatus(
    transactionId: string,
    statusUpdate: Partial<PaymentStatusUpdate>
  ): PaymentTransaction | null {
    const transaction = this.transactions.get(transactionId);
    
    if (!transaction) {
      console.error(`Transaction ${transactionId} not found`);
      return null;
    }

    // Update transaction
    const updatedTransaction: PaymentTransaction = {
      ...transaction,
      status: statusUpdate.status || transaction.status,
      transactionId: statusUpdate.transactionId || transaction.transactionId,
      gatewayResponse: statusUpdate.gatewayResponse || transaction.gatewayResponse,
      failureReason: statusUpdate.failureReason || transaction.failureReason,
      updatedAt: new Date().toISOString()
    };

    if (statusUpdate.status === 'completed') {
      updatedTransaction.completedAt = new Date().toISOString();
    }

    this.transactions.set(transactionId, updatedTransaction);

    // Add to status history
    const history = this.statusHistory.get(transactionId) || [];
    history.push({
      status: statusUpdate.status || transaction.status,
      transactionId: statusUpdate.transactionId,
      gatewayResponse: statusUpdate.gatewayResponse,
      failureReason: statusUpdate.failureReason,
      timestamp: new Date().toISOString()
    });
    this.statusHistory.set(transactionId, history);

    return updatedTransaction;
  }

  /**
   * Get transaction by ID
   */
  static getTransaction(transactionId: string): PaymentTransaction | null {
    return this.transactions.get(transactionId) || null;
  }

  /**
   * Get transactions by order ID
   */
  static getTransactionsByOrderId(orderId: number): PaymentTransaction[] {
    return Array.from(this.transactions.values())
      .filter(transaction => transaction.orderId === orderId);
  }

  /**
   * Get payment status history
   */
  static getPaymentHistory(transactionId: string): PaymentStatusUpdate[] {
    return this.statusHistory.get(transactionId) || [];
  }

  /**
   * Process payment based on method
   */
  static async processPayment(
    transactionId: string,
    paymentData?: any
  ): Promise<PaymentTransaction | null> {
    const transaction = this.getTransaction(transactionId);
    
    if (!transaction) {
      throw new Error('Transaction not found');
    }

    // Update status to processing
    this.updatePaymentStatus(transactionId, {
      status: 'processing',
      timestamp: new Date().toISOString()
    });

    try {
      switch (transaction.paymentMethod) {
        case 'cash_on_delivery':
          return await this.processCashOnDelivery(transactionId);
        
        case 'credit_card':
        case 'debit_card':
          return await this.processCardPayment(transactionId, paymentData);
        
        case 'paypal':
          return await this.processPayPalPayment(transactionId, paymentData);
        
        case 'bank_transfer':
          return await this.processBankTransfer(transactionId, paymentData);
        
        default:
          throw new Error(`Unsupported payment method: ${transaction.paymentMethod}`);
      }
    } catch (error) {
      // Update status to failed
      this.updatePaymentStatus(transactionId, {
        status: 'failed',
        failureReason: error instanceof Error ? error.message : 'Unknown error',
        timestamp: new Date().toISOString()
      });
      
      throw error;
    }
  }

  /**
   * Process cash on delivery payment
   */
  private static async processCashOnDelivery(transactionId: string): Promise<PaymentTransaction> {
    // COD is automatically approved but payment is pending until delivery
    const updatedTransaction = this.updatePaymentStatus(transactionId, {
      status: 'pending',
      transactionId: `COD_${transactionId}`,
      timestamp: new Date().toISOString()
    });

    return updatedTransaction!;
  }

  /**
   * Process card payment (mock implementation)
   */
  private static async processCardPayment(
    transactionId: string,
    paymentData: any
  ): Promise<PaymentTransaction> {
    // Simulate API call delay
    await new Promise(resolve => setTimeout(resolve, 2000));

    // Mock payment gateway response
    const mockGatewayResponse = {
      gatewayTransactionId: `TXN_${Date.now()}`,
      authCode: `AUTH_${Math.random().toString(36).substr(2, 9)}`,
      responseCode: '00',
      responseMessage: 'Approved'
    };

    // Simulate 95% success rate
    const isSuccess = Math.random() > 0.05;

    if (isSuccess) {
      const updatedTransaction = this.updatePaymentStatus(transactionId, {
        status: 'completed',
        transactionId: mockGatewayResponse.gatewayTransactionId,
        gatewayResponse: mockGatewayResponse,
        timestamp: new Date().toISOString()
      });

      return updatedTransaction!;
    } else {
      throw new Error('Payment declined by bank');
    }
  }

  /**
   * Process PayPal payment (mock implementation)
   */
  private static async processPayPalPayment(
    transactionId: string,
    paymentData: any
  ): Promise<PaymentTransaction> {
    // Simulate PayPal API call
    await new Promise(resolve => setTimeout(resolve, 1500));

    const mockPayPalResponse = {
      paypalTransactionId: `PP_${Date.now()}`,
      payerEmail: 'customer@example.com',
      status: 'COMPLETED'
    };

    const updatedTransaction = this.updatePaymentStatus(transactionId, {
      status: 'completed',
      transactionId: mockPayPalResponse.paypalTransactionId,
      gatewayResponse: mockPayPalResponse,
      timestamp: new Date().toISOString()
    });

    return updatedTransaction!;
  }

  /**
   * Process bank transfer payment
   */
  private static async processBankTransfer(
    transactionId: string,
    paymentData: any
  ): Promise<PaymentTransaction> {
    // Bank transfer requires manual verification
    const updatedTransaction = this.updatePaymentStatus(transactionId, {
      status: 'pending',
      transactionId: `BT_${transactionId}`,
      timestamp: new Date().toISOString()
    });

    return updatedTransaction!;
  }

  /**
   * Cancel payment
   */
  static cancelPayment(transactionId: string, reason?: string): PaymentTransaction | null {
    const transaction = this.getTransaction(transactionId);
    
    if (!transaction) {
      return null;
    }

    if (transaction.status === 'completed') {
      throw new Error('Cannot cancel completed payment');
    }

    return this.updatePaymentStatus(transactionId, {
      status: 'cancelled',
      failureReason: reason || 'Payment cancelled by user',
      timestamp: new Date().toISOString()
    });
  }

  /**
   * Refund payment
   */
  static async refundPayment(
    transactionId: string,
    refundAmount?: number,
    reason?: string
  ): Promise<PaymentTransaction | null> {
    const transaction = this.getTransaction(transactionId);
    
    if (!transaction) {
      throw new Error('Transaction not found');
    }

    if (transaction.status !== 'completed') {
      throw new Error('Can only refund completed payments');
    }

    const refundAmountFinal = refundAmount || transaction.amount;

    if (refundAmountFinal > transaction.amount) {
      throw new Error('Refund amount cannot exceed original payment amount');
    }

    // Simulate refund processing
    await new Promise(resolve => setTimeout(resolve, 1000));

    const updatedTransaction = this.updatePaymentStatus(transactionId, {
      status: 'refunded',
      failureReason: reason,
      timestamp: new Date().toISOString()
    });

    if (updatedTransaction) {
      updatedTransaction.refundAmount = refundAmountFinal;
      updatedTransaction.refundedAt = new Date().toISOString();
      this.transactions.set(transactionId, updatedTransaction);
    }

    return updatedTransaction;
  }

  /**
   * Get payment status display info
   */
  static getPaymentStatusInfo(status: PaymentStatus): {
    label: string;
    color: string;
    icon: string;
  } {
    const statusInfo = {
      pending: { label: 'Pending', color: 'yellow', icon: '⏳' },
      processing: { label: 'Processing', color: 'blue', icon: '⚡' },
      completed: { label: 'Completed', color: 'green', icon: '✅' },
      failed: { label: 'Failed', color: 'red', icon: '❌' },
      cancelled: { label: 'Cancelled', color: 'gray', icon: '🚫' },
      refunded: { label: 'Refunded', color: 'purple', icon: '↩️' }
    };

    return statusInfo[status] || statusInfo.pending;
  }

  /**
   * Generate unique transaction ID
   */
  private static generateTransactionId(): string {
    const timestamp = Date.now().toString(36);
    const random = Math.random().toString(36).substr(2, 9);
    return `TXN_${timestamp}_${random}`.toUpperCase();
  }

  /**
   * Get payment summary for order
   */
  static getPaymentSummary(orderId: number): {
    totalAmount: number;
    paidAmount: number;
    pendingAmount: number;
    refundedAmount: number;
    transactions: PaymentTransaction[];
  } {
    const transactions = this.getTransactionsByOrderId(orderId);
    
    let totalAmount = 0;
    let paidAmount = 0;
    let pendingAmount = 0;
    let refundedAmount = 0;

    transactions.forEach(transaction => {
      totalAmount += transaction.amount;
      
      switch (transaction.status) {
        case 'completed':
          paidAmount += transaction.amount;
          break;
        case 'pending':
        case 'processing':
          pendingAmount += transaction.amount;
          break;
        case 'refunded':
          refundedAmount += transaction.refundAmount || transaction.amount;
          break;
      }
    });

    return {
      totalAmount,
      paidAmount,
      pendingAmount,
      refundedAmount,
      transactions
    };
  }

  /**
   * Clear all transactions (for testing)
   */
  static clearAllTransactions(): void {
    this.transactions.clear();
    this.statusHistory.clear();
  }
}

export default PaymentTrackingService;
