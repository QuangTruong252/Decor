'use client';

import React from 'react';
import Link from 'next/link';
import { useCart } from '@/context/CartContext';
import { Button } from '@/components/ui/Button';
import { ShoppingCart, Truck, Shield, RotateCcw } from 'lucide-react';

interface CartSummaryProps {
  className?: string;
  showCheckoutButton?: boolean;
  showContinueShoppingButton?: boolean;
  showShippingInfo?: boolean;
  showPolicies?: boolean;
}

export function CartSummary({
  className = '',
  showCheckoutButton = true,
  showContinueShoppingButton = true,
  showShippingInfo = true,
  showPolicies = true,
}: CartSummaryProps) {
  const { cart, itemCount, subtotal, total, isEmpty, isLoading } = useCart();

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  };

  const tax = cart?.tax || 0;
  const shipping = cart?.shipping || 0;
  const discount = cart?.discount || 0;
  const freeShippingThreshold = 100;
  const remainingForFreeShipping = Math.max(0, freeShippingThreshold - subtotal);

  if (isEmpty) {
    return (
      <div className={`bg-white border border-gray-200 rounded-lg p-6 ${className}`}>
        <div className="text-center">
          <ShoppingCart className="w-12 h-12 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            Your cart is empty
          </h3>
          <p className="text-gray-500 mb-6">
            Add some items to your cart to see the summary here.
          </p>
          <Link href="/shop">
            <Button className="w-full">
              Continue Shopping
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className={`bg-white border border-gray-200 rounded-lg p-6 ${className}`}>
      {/* Header */}
      <h3 className="text-lg font-semibold text-gray-900 mb-4">
        Order Summary
      </h3>

      {/* Items Count */}
      <div className="flex items-center justify-between mb-4 pb-4 border-b border-gray-200">
        <span className="text-gray-600">
          Items ({itemCount})
        </span>
        <span className="font-medium">
          {formatPrice(subtotal)}
        </span>
      </div>

      {/* Price Breakdown */}
      <div className="space-y-3 mb-4">
        <div className="flex items-center justify-between">
          <span className="text-gray-600">Subtotal</span>
          <span className="font-medium">{formatPrice(subtotal)}</span>
        </div>

        {discount > 0 && (
          <div className="flex items-center justify-between text-green-600">
            <span>Discount</span>
            <span>-{formatPrice(discount)}</span>
          </div>
        )}

        <div className="flex items-center justify-between">
          <span className="text-gray-600">Tax</span>
          <span className="font-medium">{formatPrice(tax)}</span>
        </div>

        <div className="flex items-center justify-between">
          <span className="text-gray-600">Shipping</span>
          <span className="font-medium">
            {shipping === 0 ? 'Free' : formatPrice(shipping)}
          </span>
        </div>
      </div>

      {/* Free Shipping Progress */}
      {showShippingInfo && remainingForFreeShipping > 0 && (
        <div className="mb-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
          <div className="flex items-center gap-2 mb-2">
            <Truck className="w-4 h-4 text-blue-600" />
            <span className="text-sm font-medium text-blue-900">
              Free Shipping Available
            </span>
          </div>
          <p className="text-sm text-blue-700">
            Add {formatPrice(remainingForFreeShipping)} more to qualify for free shipping!
          </p>
          <div className="mt-2 w-full bg-blue-200 rounded-full h-2">
            <div
              className="bg-blue-600 h-2 rounded-full transition-all duration-300"
              style={{
                width: `${Math.min(100, (subtotal / freeShippingThreshold) * 100)}%`,
              }}
            />
          </div>
        </div>
      )}

      {/* Total */}
      <div className="flex items-center justify-between py-4 border-t border-gray-200 mb-6">
        <span className="text-lg font-semibold text-gray-900">Total</span>
        <span className="text-lg font-bold text-blue-600">
          {formatPrice(total)}
        </span>
      </div>

      {/* Action Buttons */}
      <div className="space-y-3">
        {showCheckoutButton && (
          <Link href="/checkout">
            <Button
              className="w-full"
              disabled={isLoading || isEmpty}
            >
              {isLoading ? (
                <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
              ) : null}
              Proceed to Checkout
            </Button>
          </Link>
        )}

        {showContinueShoppingButton && (
          <Link href="/shop">
            <Button variant="outline" className="w-full">
              Continue Shopping
            </Button>
          </Link>
        )}
      </div>

      {/* Policies */}
      {showPolicies && (
        <div className="mt-6 pt-6 border-t border-gray-200">
          <div className="space-y-3 text-sm text-gray-600">
            <div className="flex items-center gap-2">
              <Shield className="w-4 h-4 text-green-600" />
              <span>Secure checkout with SSL encryption</span>
            </div>
            <div className="flex items-center gap-2">
              <RotateCcw className="w-4 h-4 text-blue-600" />
              <span>30-day return policy</span>
            </div>
            <div className="flex items-center gap-2">
              <Truck className="w-4 h-4 text-purple-600" />
              <span>Free shipping on orders over {formatPrice(freeShippingThreshold)}</span>
            </div>
          </div>
        </div>
      )}

      {/* Promo Code Section */}
      <div className="mt-6 pt-6 border-t border-gray-200">
        <details className="group">
          <summary className="flex items-center justify-between cursor-pointer text-sm font-medium text-gray-900">
            <span>Have a promo code?</span>
            <span className="group-open:rotate-180 transition-transform">
              ▼
            </span>
          </summary>
          <div className="mt-3">
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Enter promo code"
                className="flex-1 px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <Button variant="outline" size="sm">
                Apply
              </Button>
            </div>
          </div>
        </details>
      </div>
    </div>
  );
}

export default CartSummary;
