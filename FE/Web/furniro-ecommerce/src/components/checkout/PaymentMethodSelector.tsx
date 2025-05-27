'use client';

import React, { useEffect } from 'react';
import type { PaymentMethod, PaymentFormData } from '@/api/types';
import { PaymentValidationService } from '@/services/paymentValidation';

interface PaymentMethodSelectorProps {
  selectedMethod: PaymentMethod;
  paymentDetails?: PaymentFormData;
  errors: Record<string, string>;
  onChange: (method: PaymentMethod, details?: PaymentFormData) => void;
}

export default function PaymentMethodSelector({
  selectedMethod,
  paymentDetails,
  errors,
  onChange
}: PaymentMethodSelectorProps) {
  const paymentMethods = [
    {
      id: 'cash_on_delivery' as PaymentMethod,
      name: PaymentValidationService.getPaymentMethodDisplayName('cash_on_delivery'),
      description: 'Pay when your order is delivered',
      icon: '💵',
      available: PaymentValidationService.isPaymentMethodAvailable('cash_on_delivery')
    },
    {
      id: 'credit_card' as PaymentMethod,
      name: PaymentValidationService.getPaymentMethodDisplayName('credit_card'),
      description: 'Pay securely with your credit card',
      icon: '💳',
      available: PaymentValidationService.isPaymentMethodAvailable('credit_card')
    },
    {
      id: 'debit_card' as PaymentMethod,
      name: PaymentValidationService.getPaymentMethodDisplayName('debit_card'),
      description: 'Pay with your debit card',
      icon: '💳',
      available: PaymentValidationService.isPaymentMethodAvailable('debit_card')
    },
    {
      id: 'paypal' as PaymentMethod,
      name: PaymentValidationService.getPaymentMethodDisplayName('paypal'),
      description: 'Pay with your PayPal account',
      icon: '🅿️',
      available: PaymentValidationService.isPaymentMethodAvailable('paypal')
    },
    {
      id: 'bank_transfer' as PaymentMethod,
      name: PaymentValidationService.getPaymentMethodDisplayName('bank_transfer'),
      description: 'Transfer money directly from your bank',
      icon: '🏦',
      available: PaymentValidationService.isPaymentMethodAvailable('bank_transfer')
    }
  ];

  const handleMethodChange = (method: PaymentMethod) => {
    onChange(method, paymentDetails);
  };

  const handleCardDetailsChange = (field: keyof PaymentFormData, value: string) => {
    let formattedValue = value;

    // Format card number
    if (field === 'cardNumber') {
      formattedValue = PaymentValidationService.formatCardNumber(value);
    }

    // Format expiry date
    if (field === 'expiryDate') {
      formattedValue = PaymentValidationService.formatExpiryDate(value);
    }

    // Format CVV (only allow digits)
    if (field === 'cvv') {
      formattedValue = value.replace(/\D/g, '');
    }

    const updatedDetails = {
      ...paymentDetails,
      [field]: formattedValue
    };
    onChange(selectedMethod, updatedDetails);
  };

  const isCardPayment = selectedMethod === 'credit_card' || selectedMethod === 'debit_card';

  return (
    <div>
      <h3 className="text-xl font-semibold mb-6">Payment Method</h3>

      {/* Payment method options */}
      <div className="space-y-4 mb-6">
        {paymentMethods.map((method) => (
          <div
            key={method.id}
            className={`relative border rounded-lg p-4 cursor-pointer transition-all ${
              selectedMethod === method.id
                ? 'border-primary bg-primary/5'
                : 'border-gray-300 hover:border-gray-400'
            } ${!method.available ? 'opacity-50 cursor-not-allowed' : ''}`}
            onClick={() => method.available && handleMethodChange(method.id)}
          >
            <div className="flex items-center">
              <input
                type="radio"
                id={method.id}
                name="paymentMethod"
                value={method.id}
                checked={selectedMethod === method.id}
                onChange={() => method.available && handleMethodChange(method.id)}
                disabled={!method.available}
                className="h-4 w-4 text-primary focus:ring-primary border-gray-300"
              />
              <label htmlFor={method.id} className="ml-3 flex-1 cursor-pointer">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <span className="text-2xl mr-3">{method.icon}</span>
                    <div>
                      <div className="font-medium text-gray-900">{method.name}</div>
                      <div className="text-sm text-gray-600">{method.description}</div>
                    </div>
                  </div>
                  {!method.available && (
                    <span className="text-xs text-gray-500 bg-gray-100 px-2 py-1 rounded">
                      Coming Soon
                    </span>
                  )}
                </div>
              </label>
            </div>
          </div>
        ))}
      </div>

      {/* Card payment details */}
      {isCardPayment && (
        <div className="border border-gray-300 rounded-lg p-6 bg-gray-50">
          <h4 className="font-medium text-gray-900 mb-4">Card Details</h4>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Card Number */}
            <div className="md:col-span-2">
              <label htmlFor="cardNumber" className="block text-sm font-medium text-gray-700 mb-2">
                Card Number *
              </label>
              <input
                type="text"
                id="cardNumber"
                value={paymentDetails?.cardNumber || ''}
                onChange={(e) => handleCardDetailsChange('cardNumber', e.target.value)}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
                  errors.cardNumber ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="1234 5678 9012 3456"
                maxLength={19}
              />
              {errors.cardNumber && (
                <p className="mt-1 text-sm text-red-600">{errors.cardNumber}</p>
              )}
            </div>

            {/* Cardholder Name */}
            <div className="md:col-span-2">
              <label htmlFor="cardholderName" className="block text-sm font-medium text-gray-700 mb-2">
                Cardholder Name *
              </label>
              <input
                type="text"
                id="cardholderName"
                value={paymentDetails?.cardholderName || ''}
                onChange={(e) => handleCardDetailsChange('cardholderName', e.target.value)}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
                  errors.cardholderName ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="John Doe"
              />
              {errors.cardholderName && (
                <p className="mt-1 text-sm text-red-600">{errors.cardholderName}</p>
              )}
            </div>

            {/* Expiry Date */}
            <div>
              <label htmlFor="expiryDate" className="block text-sm font-medium text-gray-700 mb-2">
                Expiry Date *
              </label>
              <input
                type="text"
                id="expiryDate"
                value={paymentDetails?.expiryDate || ''}
                onChange={(e) => handleCardDetailsChange('expiryDate', e.target.value)}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
                  errors.expiryDate ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="MM/YY"
                maxLength={5}
              />
              {errors.expiryDate && (
                <p className="mt-1 text-sm text-red-600">{errors.expiryDate}</p>
              )}
            </div>

            {/* CVV */}
            <div>
              <label htmlFor="cvv" className="block text-sm font-medium text-gray-700 mb-2">
                CVV *
              </label>
              <input
                type="text"
                id="cvv"
                value={paymentDetails?.cvv || ''}
                onChange={(e) => handleCardDetailsChange('cvv', e.target.value)}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
                  errors.cvv ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="123"
                maxLength={4}
              />
              {errors.cvv && (
                <p className="mt-1 text-sm text-red-600">{errors.cvv}</p>
              )}
            </div>
          </div>

          {/* Security notice */}
          <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded-lg">
            <div className="flex items-center">
              <svg className="h-5 w-5 text-green-400 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
              </svg>
              <span className="text-sm text-green-800">
                Your payment information is encrypted and secure
              </span>
            </div>
          </div>
        </div>
      )}

      {/* Cash on Delivery info */}
      {selectedMethod === 'cash_on_delivery' && (
        <div className="border border-gray-300 rounded-lg p-6 bg-gray-50">
          <h4 className="font-medium text-gray-900 mb-2">Cash on Delivery</h4>
          <p className="text-sm text-gray-600 mb-4">
            You will pay for your order when it is delivered to your address.
            Please have the exact amount ready for the delivery person.
          </p>

          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3">
            <div className="flex items-center">
              <svg className="h-5 w-5 text-yellow-400 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
              <span className="text-sm text-yellow-800">
                Additional delivery charges may apply for cash on delivery orders
              </span>
            </div>
          </div>
        </div>
      )}

      {/* Error display */}
      {errors.paymentMethod && (
        <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-600">{errors.paymentMethod}</p>
        </div>
      )}
    </div>
  );
}
