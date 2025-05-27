import type { PaymentMethod, PaymentFormData } from '@/api/types';

export interface PaymentValidationResult {
  isValid: boolean;
  errors: Record<string, string>;
}

export class PaymentValidationService {
  /**
   * Validate payment form data based on payment method
   */
  static validatePaymentData(
    paymentMethod: PaymentMethod,
    paymentData?: PaymentFormData
  ): PaymentValidationResult {
    const errors: Record<string, string> = {};

    switch (paymentMethod) {
      case 'credit_card':
      case 'debit_card':
        return this.validateCardPayment(paymentData);
      case 'cash_on_delivery':
        return this.validateCashOnDelivery();
      case 'paypal':
        return this.validatePayPal(paymentData);
      case 'bank_transfer':
        return this.validateBankTransfer(paymentData);
      default:
        errors.paymentMethod = 'Invalid payment method selected';
        return { isValid: false, errors };
    }
  }

  /**
   * Validate credit/debit card payment
   */
  private static validateCardPayment(paymentData?: PaymentFormData): PaymentValidationResult {
    const errors: Record<string, string> = {};

    if (!paymentData) {
      errors.paymentData = 'Payment data is required for card payments';
      return { isValid: false, errors };
    }

    // Validate card number
    if (!paymentData.cardNumber) {
      errors.cardNumber = 'Card number is required';
    } else if (!this.isValidCardNumber(paymentData.cardNumber)) {
      errors.cardNumber = 'Invalid card number';
    }

    // Validate cardholder name
    if (!paymentData.cardholderName) {
      errors.cardholderName = 'Cardholder name is required';
    } else if (paymentData.cardholderName.length < 2) {
      errors.cardholderName = 'Cardholder name must be at least 2 characters';
    }

    // Validate expiry date
    if (!paymentData.expiryDate) {
      errors.expiryDate = 'Expiry date is required';
    } else if (!this.isValidExpiryDate(paymentData.expiryDate)) {
      errors.expiryDate = 'Invalid expiry date (MM/YY format)';
    } else if (this.isExpiredCard(paymentData.expiryDate)) {
      errors.expiryDate = 'Card has expired';
    }

    // Validate CVV
    if (!paymentData.cvv) {
      errors.cvv = 'CVV is required';
    } else if (!this.isValidCVV(paymentData.cvv)) {
      errors.cvv = 'Invalid CVV (3-4 digits)';
    }

    return {
      isValid: Object.keys(errors).length === 0,
      errors
    };
  }

  /**
   * Validate cash on delivery payment
   */
  private static validateCashOnDelivery(): PaymentValidationResult {
    // COD doesn't require additional validation
    return { isValid: true, errors: {} };
  }

  /**
   * Validate PayPal payment
   */
  private static validatePayPal(paymentData?: PaymentFormData): PaymentValidationResult {
    // PayPal validation would be handled by PayPal SDK
    // For now, just return valid
    return { isValid: true, errors: {} };
  }

  /**
   * Validate bank transfer payment
   */
  private static validateBankTransfer(paymentData?: PaymentFormData): PaymentValidationResult {
    // Bank transfer validation would depend on specific requirements
    return { isValid: true, errors: {} };
  }

  /**
   * Validate card number using Luhn algorithm
   */
  private static isValidCardNumber(cardNumber: string): boolean {
    // Remove spaces and non-digits
    const cleanNumber = cardNumber.replace(/\D/g, '');
    
    // Check length (13-19 digits for most cards)
    if (cleanNumber.length < 13 || cleanNumber.length > 19) {
      return false;
    }

    // Luhn algorithm
    let sum = 0;
    let isEven = false;

    for (let i = cleanNumber.length - 1; i >= 0; i--) {
      let digit = parseInt(cleanNumber[i]);

      if (isEven) {
        digit *= 2;
        if (digit > 9) {
          digit -= 9;
        }
      }

      sum += digit;
      isEven = !isEven;
    }

    return sum % 10 === 0;
  }

  /**
   * Validate expiry date format (MM/YY)
   */
  private static isValidExpiryDate(expiryDate: string): boolean {
    const regex = /^(0[1-9]|1[0-2])\/\d{2}$/;
    return regex.test(expiryDate);
  }

  /**
   * Check if card is expired
   */
  private static isExpiredCard(expiryDate: string): boolean {
    const [month, year] = expiryDate.split('/');
    const expiry = new Date(2000 + parseInt(year), parseInt(month) - 1);
    const now = new Date();
    
    return expiry < now;
  }

  /**
   * Validate CVV
   */
  private static isValidCVV(cvv: string): boolean {
    const regex = /^\d{3,4}$/;
    return regex.test(cvv);
  }

  /**
   * Get card type from card number
   */
  static getCardType(cardNumber: string): string {
    const cleanNumber = cardNumber.replace(/\D/g, '');

    if (/^4/.test(cleanNumber)) return 'visa';
    if (/^5[1-5]/.test(cleanNumber)) return 'mastercard';
    if (/^3[47]/.test(cleanNumber)) return 'amex';
    if (/^6(?:011|5)/.test(cleanNumber)) return 'discover';
    if (/^(?:2131|1800|35\d{3})/.test(cleanNumber)) return 'jcb';

    return 'unknown';
  }

  /**
   * Format card number with spaces
   */
  static formatCardNumber(cardNumber: string): string {
    const cleanNumber = cardNumber.replace(/\D/g, '');
    const cardType = this.getCardType(cleanNumber);

    if (cardType === 'amex') {
      // AMEX format: XXXX XXXXXX XXXXX
      return cleanNumber.replace(/(\d{4})(\d{6})(\d{5})/, '$1 $2 $3');
    } else {
      // Standard format: XXXX XXXX XXXX XXXX
      return cleanNumber.replace(/(\d{4})(?=\d)/g, '$1 ');
    }
  }

  /**
   * Format expiry date (MM/YY)
   */
  static formatExpiryDate(expiryDate: string): string {
    const cleanDate = expiryDate.replace(/\D/g, '');
    
    if (cleanDate.length >= 2) {
      return cleanDate.substring(0, 2) + '/' + cleanDate.substring(2, 4);
    }
    
    return cleanDate;
  }

  /**
   * Mask card number for display
   */
  static maskCardNumber(cardNumber: string): string {
    const cleanNumber = cardNumber.replace(/\D/g, '');
    
    if (cleanNumber.length < 4) return cardNumber;
    
    const lastFour = cleanNumber.slice(-4);
    const masked = '*'.repeat(cleanNumber.length - 4);
    
    return this.formatCardNumber(masked + lastFour);
  }

  /**
   * Validate payment amount
   */
  static validatePaymentAmount(amount: number, minAmount: number = 0.01, maxAmount: number = 10000): PaymentValidationResult {
    const errors: Record<string, string> = {};

    if (amount < minAmount) {
      errors.amount = `Minimum payment amount is $${minAmount}`;
    }

    if (amount > maxAmount) {
      errors.amount = `Maximum payment amount is $${maxAmount}`;
    }

    if (isNaN(amount) || amount <= 0) {
      errors.amount = 'Invalid payment amount';
    }

    return {
      isValid: Object.keys(errors).length === 0,
      errors
    };
  }

  /**
   * Check if payment method is available
   */
  static isPaymentMethodAvailable(paymentMethod: PaymentMethod): boolean {
    const availableMethods: PaymentMethod[] = [
      'cash_on_delivery',
      'credit_card',
      'debit_card'
      // 'paypal', 'bank_transfer' - not implemented yet
    ];

    return availableMethods.includes(paymentMethod);
  }

  /**
   * Get payment method display name
   */
  static getPaymentMethodDisplayName(paymentMethod: PaymentMethod): string {
    const displayNames: Record<PaymentMethod, string> = {
      cash_on_delivery: 'Cash on Delivery',
      credit_card: 'Credit Card',
      debit_card: 'Debit Card',
      paypal: 'PayPal',
      bank_transfer: 'Bank Transfer'
    };

    return displayNames[paymentMethod] || paymentMethod;
  }

  /**
   * Get payment method fees
   */
  static getPaymentMethodFees(paymentMethod: PaymentMethod, amount: number): number {
    const feeRates: Record<PaymentMethod, number> = {
      cash_on_delivery: 0.02, // 2% fee for COD
      credit_card: 0.029, // 2.9% fee for credit cards
      debit_card: 0.015, // 1.5% fee for debit cards
      paypal: 0.034, // 3.4% fee for PayPal
      bank_transfer: 0 // No fee for bank transfer
    };

    const feeRate = feeRates[paymentMethod] || 0;
    return Math.round(amount * feeRate * 100) / 100; // Round to 2 decimal places
  }

  /**
   * Validate billing address for card payments
   */
  static validateBillingAddress(billingAddress: any): PaymentValidationResult {
    const errors: Record<string, string> = {};

    if (!billingAddress) {
      errors.billingAddress = 'Billing address is required for card payments';
      return { isValid: false, errors };
    }

    if (!billingAddress.address) {
      errors.billingAddress = 'Street address is required';
    }

    if (!billingAddress.city) {
      errors.billingCity = 'City is required';
    }

    if (!billingAddress.postalCode) {
      errors.billingPostalCode = 'Postal code is required';
    }

    if (!billingAddress.country) {
      errors.billingCountry = 'Country is required';
    }

    return {
      isValid: Object.keys(errors).length === 0,
      errors
    };
  }
}

export default PaymentValidationService;
