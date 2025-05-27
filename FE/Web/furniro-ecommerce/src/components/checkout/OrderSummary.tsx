'use client';

import React from 'react';
import Image from 'next/image';
import type { CartItem, ShippingAddressForm } from '@/api/types';
import { ShippingService } from '@/services/shippingService';
import { getImageUrl } from '@/lib/utils';

interface OrderTotals {
  subtotal: number;
  tax: number;
  shipping: number;
  total: number;
  discount?: number;
}

interface OrderSummaryProps {
  items: CartItem[];
  totals: OrderTotals;
  isLoading?: boolean;
  showItems?: boolean;
  shippingAddress?: ShippingAddressForm;
  shippingMethod?: string;
}

export default function OrderSummary({
  items,
  totals,
  isLoading = false,
  showItems = true,
  shippingAddress,
  shippingMethod = 'standard'
}: OrderSummaryProps) {
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  };

  // Calculate shipping if address is provided
  const shippingCalculation = React.useMemo(() => {
    if (!shippingAddress || !shippingAddress.country) {
      return null;
    }

    try {
      return ShippingService.calculateShipping(
        shippingAddress,
        totals.subtotal,
        1, // Default weight
        shippingMethod
      );
    } catch (error) {
      console.error('Shipping calculation failed:', error);
      return null;
    }
  }, [shippingAddress, totals.subtotal, shippingMethod]);

  // Get available shipping methods
  const availableShippingMethods = React.useMemo(() => {
    if (!shippingAddress || !shippingAddress.country) {
      return [];
    }

    try {
      return ShippingService.getAvailableShippingMethods(
        shippingAddress,
        totals.subtotal
      );
    } catch (error) {
      console.error('Failed to get shipping methods:', error);
      return [];
    }
  }, [shippingAddress, totals.subtotal]);

  return (
    <div className="bg-white rounded-lg shadow-sm border p-6 sticky top-6">
      <h3 className="text-lg font-semibold mb-6">Order Summary</h3>

      {/* Order items */}
      {showItems && (
        <div className="space-y-4 mb-6">
          {items.map((item) => {
            return (
              <div key={item.id} className="flex items-center space-x-3">
                <div className="relative">
                  <div className="relative w-12 h-12">
                    <Image
                      src={getImageUrl(item.productImage)}
                      alt={item.productName || 'Product'}
                      fill
                      className="object-cover rounded"
                      sizes="48px"
                      onError={(e) => {
                        console.log('Image error:', e);
                        const target = e.target as HTMLImageElement;
                        target.src = '/images/placeholder-product.jpg';
                      }}
                    />
                  </div>
                  <span className="absolute -top-2 -right-2 bg-primary text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                    {item.quantity}
                  </span>
                </div>
                <div className="flex-1 min-w-0">
                  <h4 className="text-sm font-medium text-gray-900 truncate">
                    {item.productName || 'Unknown Product'}
                  </h4>
                  <p className="text-sm text-gray-600">
                    {formatPrice(item.unitPrice)} × {item.quantity}
                  </p>
                </div>
                <div className="text-sm font-medium text-gray-900">
                  {formatPrice(item.subtotal)}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Divider */}
      <div className="border-t border-gray-200 my-6"></div>

      {/* Order totals */}
      <div className="space-y-3">
        {/* Subtotal */}
        <div className="flex justify-between text-sm">
          <span className="text-gray-600">Subtotal ({items.length} items)</span>
          <span className="text-gray-900">{formatPrice(totals.subtotal)}</span>
        </div>

        {/* Discount */}
        {totals.discount && totals.discount > 0 && (
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Discount</span>
            <span className="text-green-600">-{formatPrice(totals.discount)}</span>
          </div>
        )}

        {/* Shipping */}
        <div className="flex justify-between text-sm">
          <span className="text-gray-600">Shipping</span>
          <span className="text-gray-900">
            {shippingCalculation ? (
              shippingCalculation.shippingCost === 0 ? (
                <span className="text-green-600">Free</span>
              ) : (
                formatPrice(shippingCalculation.shippingCost)
              )
            ) : totals.shipping === 0 ? (
              <span className="text-green-600">Free</span>
            ) : (
              formatPrice(totals.shipping)
            )}
          </span>
        </div>

        {/* Shipping method info */}
        {shippingCalculation && shippingMethod && (
          <div className="text-xs text-gray-500">
            {ShippingService.getShippingMethod(shippingMethod)?.name} -
            Estimated delivery: {shippingCalculation.estimatedDelivery.toLocaleDateString()}
          </div>
        )}

        {/* Tax */}
        <div className="flex justify-between text-sm">
          <span className="text-gray-600">Tax</span>
          <span className="text-gray-900">{formatPrice(totals.tax)}</span>
        </div>

        {/* Free shipping notice */}
        {totals.shipping === 0 && totals.subtotal >= 100 && (
          <div className="text-xs text-green-600 bg-green-50 p-2 rounded">
            🎉 You've qualified for free shipping!
          </div>
        )}

        {/* Free shipping progress */}
        {totals.shipping > 0 && totals.subtotal < 100 && (
          <div className="text-xs text-gray-600 bg-gray-50 p-2 rounded">
            Add {formatPrice(100 - totals.subtotal)} more for free shipping
          </div>
        )}
      </div>

      {/* Divider */}
      <div className="border-t border-gray-200 my-4"></div>

      {/* Total */}
      <div className="flex justify-between text-lg font-semibold">
        <span>Total</span>
        <span className="text-primary">{formatPrice(totals.total)}</span>
      </div>

      {/* Security badges */}
      <div className="mt-6 pt-6 border-t border-gray-200">
        <div className="flex items-center justify-center space-x-4 text-xs text-gray-500">
          <div className="flex items-center">
            <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
            </svg>
            Secure Checkout
          </div>
          <div className="flex items-center">
            <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
            </svg>
            SSL Protected
          </div>
        </div>
      </div>

      {/* Money back guarantee */}
      <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
        <div className="flex items-center text-sm text-blue-800">
          <svg className="w-4 h-4 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
          </svg>
          <span>30-day money back guarantee</span>
        </div>
      </div>

      {/* Loading overlay */}
      {isLoading && (
        <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center rounded-lg">
          <div className="flex items-center space-x-2">
            <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
            <span className="text-sm text-gray-600">Processing...</span>
          </div>
        </div>
      )}
    </div>
  );
}

