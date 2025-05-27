'use client';

import React, { useState } from 'react';
import Image from 'next/image';
import { useCart } from '@/context/CartContext';
import { useOrderStore } from '@/store/orderStore';
import { PaymentValidationService } from '@/services/paymentValidation';
import { PaymentTrackingService } from '@/services/paymentTracking';
import { getImageUrl } from '@/lib/utils';
import type { OrderFormData, PaymentMethod } from '@/api/types';
import ShippingAddressForm from './ShippingAddressForm';
import PaymentMethodSelector from './PaymentMethodSelector';
import OrderSummary from './OrderSummary';
import CheckoutSteps from './CheckoutSteps';
import CheckoutProgress from './CheckoutProgress';

interface CheckoutFormProps {
  onOrderComplete?: (orderId: number) => void;
}

export default function CheckoutForm({ onOrderComplete }: CheckoutFormProps) {
  const { cart, subtotal, clearCart } = useCart();
  const { createOrder, isCreating, error } = useOrderStore();

  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState<OrderFormData>({
    shippingAddress: {
      firstName: '',
      lastName: '',
      address: '',
      city: '',
      state: '',
      postalCode: '',
      country: '',
      phone: ''
    },
    paymentMethod: 'cash_on_delivery' as PaymentMethod,
    notes: ''
  });

  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  const steps = [
    { id: 1, title: 'Shipping Address', description: 'Enter your delivery address' },
    { id: 2, title: 'Payment Method', description: 'Choose your payment method' },
    { id: 3, title: 'Review Order', description: 'Review your order details' }
  ];

  const validateStep = (step: number): boolean => {
    const errors: Record<string, string> = {};

    if (step === 1) {
      // Validate shipping address
      const { shippingAddress } = formData;
      if (!shippingAddress.firstName.trim()) {
        errors.firstName = 'First name is required';
      }
      if (!shippingAddress.lastName.trim()) {
        errors.lastName = 'Last name is required';
      }
      if (!shippingAddress.address.trim()) {
        errors.address = 'Address is required';
      }
      if (!shippingAddress.city.trim()) {
        errors.city = 'City is required';
      }
      if (!shippingAddress.state.trim()) {
        errors.state = 'State is required';
      }
      if (!shippingAddress.postalCode.trim()) {
        errors.postalCode = 'Postal code is required';
      }
      if (!shippingAddress.country.trim()) {
        errors.country = 'Country is required';
      }
    }

    if (step === 2) {
      // Validate payment method
      if (!formData.paymentMethod) {
        errors.paymentMethod = 'Payment method is required';
      } else {
        // Validate payment data using PaymentValidationService
        const paymentValidation = PaymentValidationService.validatePaymentData(
          formData.paymentMethod,
          formData.paymentDetails
        );

        if (!paymentValidation.isValid) {
          Object.assign(errors, paymentValidation.errors);
        }
      }
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleNextStep = () => {
    if (validateStep(currentStep)) {
      setCurrentStep(prev => Math.min(prev + 1, steps.length));
    }
  };

  const handlePrevStep = () => {
    setCurrentStep(prev => Math.max(prev - 1, 1));
  };

  const handleFormDataChange = (updates: Partial<OrderFormData>) => {
    setFormData(prev => ({ ...prev, ...updates }));
    // Clear related errors
    setFormErrors(prev => {
      const newErrors = { ...prev };
      Object.keys(updates).forEach(key => {
        delete newErrors[key];
      });
      return newErrors;
    });
  };

  const handleSubmit = async () => {
    if (!validateStep(currentStep)) {
      return;
    }

    try {
      // Calculate totals for payment
      const totals = calculateTotals();

      // Create payment transaction
      const transaction = PaymentTrackingService.createTransaction(
        0, // Will be updated with actual order ID
        totals.total,
        formData.paymentMethod
      );

      // Create order
      const order = await createOrder(formData);

      // Update transaction with order ID
      transaction.orderId = order.id;

      // Process payment
      await PaymentTrackingService.processPayment(transaction.id, formData.paymentDetails);

      // Clear cart after successful order
      clearCart();

      // Notify parent component
      onOrderComplete?.(order.id);

      // Redirect to order confirmation
      window.location.href = `/orders/${order.id}?success=true`;
    } catch (error) {
      console.error('Failed to create order:', error);
    }
  };

  const calculateTotals = () => {
    const tax = subtotal * 0.1; // 10% tax
    const shipping = subtotal > 100 ? 0 : 15; // Free shipping over $100
    const total = subtotal + tax + shipping;

    return { subtotal, tax, shipping, total };
  };

  if (!cart || cart.items.length === 0) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">Your cart is empty</h2>
        <p className="text-gray-600 mb-6">Add some items to your cart before checkout</p>
        <a
          href="/shop"
          className="inline-block bg-primary text-white px-6 py-3 rounded-lg hover:bg-primary-dark transition-colors"
        >
          Continue Shopping
        </a>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto">
      {/* Progress indicator */}
      <CheckoutProgress currentStep={currentStep} steps={steps} />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 mt-8">
        {/* Main checkout form */}
        <div className="lg:col-span-2">
          <div className="bg-white rounded-lg shadow-sm border p-6">
            {/* Step content */}
            {currentStep === 1 && (
              <ShippingAddressForm
                data={formData.shippingAddress}
                errors={formErrors}
                onChange={(shippingAddress) =>
                  handleFormDataChange({ shippingAddress })
                }
              />
            )}

            {currentStep === 2 && (
              <PaymentMethodSelector
                selectedMethod={formData.paymentMethod}
                paymentDetails={formData.paymentDetails}
                errors={formErrors}
                onChange={(paymentMethod, paymentDetails) =>
                  handleFormDataChange({ paymentMethod, paymentDetails })
                }
              />
            )}

            {currentStep === 3 && (
              <div>
                <h3 className="text-xl font-semibold mb-6">Review Your Order</h3>

                {/* Order items */}
                <div className="space-y-4 mb-6">
                  {cart.items?.map((item) => {
                    return (
                      <div key={item.id} className="flex items-center space-x-4 p-4 border rounded-lg">
                        <div className="relative w-16 h-16 flex-shrink-0">
                          <Image
                            src={getImageUrl(item.productImage)}
                            alt={item.productName || 'Product'}
                            fill
                            className="object-cover rounded"
                            sizes="64px"
                            onError={(e) => {
                              const target = e.target as HTMLImageElement;
                              target.src = '/images/placeholder-product.jpg';
                            }}
                          />
                        </div>
                        <div className="flex-1">
                          <h4 className="font-medium">{item.productName || 'Unknown Product'}</h4>
                          <p className="text-gray-600">Quantity: {item.quantity}</p>
                        </div>
                        <div className="text-right">
                          <p className="font-medium">${item.subtotal.toFixed(2)}</p>
                        </div>
                      </div>
                    );
                  })}
                </div>

                {/* Shipping address summary */}
                <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                  <h4 className="font-medium mb-2">Shipping Address</h4>
                  <p className="text-sm text-gray-600">
                    {formData.shippingAddress.firstName} {formData.shippingAddress.lastName}<br />
                    {formData.shippingAddress.address}<br />
                    {formData.shippingAddress.city}, {formData.shippingAddress.state} {formData.shippingAddress.postalCode}<br />
                    {formData.shippingAddress.country}
                  </p>
                </div>

                {/* Payment method summary */}
                <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                  <h4 className="font-medium mb-2">Payment Method</h4>
                  <p className="text-sm text-gray-600 capitalize">
                    {formData.paymentMethod.replace('_', ' ')}
                  </p>
                </div>

                {/* Order notes */}
                <div className="mb-6">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Order Notes (Optional)
                  </label>
                  <textarea
                    value={formData.notes || ''}
                    onChange={(e) => handleFormDataChange({ notes: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                    placeholder="Any special instructions for your order..."
                  />
                </div>
              </div>
            )}

            {/* Error display */}
            {error && (
              <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
                <p className="text-red-600">{error}</p>
              </div>
            )}

            {/* Navigation buttons */}
            <div className="flex justify-between pt-6 border-t">
              <button
                type="button"
                onClick={handlePrevStep}
                disabled={currentStep === 1}
                className="px-6 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>

              {currentStep < steps.length ? (
                <button
                  type="button"
                  onClick={handleNextStep}
                  className="px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark"
                >
                  Next
                </button>
              ) : (
                <button
                  type="button"
                  onClick={handleSubmit}
                  disabled={isCreating}
                  className="px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isCreating ? 'Processing...' : 'Place Order'}
                </button>
              )}
            </div>
          </div>
        </div>

        {/* Order summary sidebar */}
        <div className="lg:col-span-1">
          <OrderSummary
            items={cart.items || []}
            totals={calculateTotals()}
            isLoading={isCreating}
          />
        </div>
      </div>
    </div>
  );
}
