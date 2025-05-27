'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import type { OrderDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface OrderConfirmationProps {
  order: OrderDTO;
  onContinueShopping?: () => void;
}

export default function OrderConfirmation({ order, onContinueShopping }: OrderConfirmationProps) {
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getEstimatedDelivery = () => {
    const orderDate = new Date(order.orderDate);
    const estimatedDate = new Date(orderDate);
    estimatedDate.setDate(orderDate.getDate() + 7); // Add 7 days for delivery

    return estimatedDate.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  return (
    <div className="max-w-2xl mx-auto text-center">
      {/* Success icon */}
      <div className="mb-8">
        <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-green-100">
          <svg className="h-8 w-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
      </div>

      {/* Success message */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Order Confirmed!
        </h1>
        <p className="text-lg text-gray-600">
          Thank you for your purchase. Your order has been successfully placed.
        </p>
      </div>

      {/* Order details card */}
      <div className="bg-white border border-gray-200 rounded-lg p-6 mb-8 text-left">
        <div className="flex justify-between items-start mb-6">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">Order Details</h2>
            <p className="text-sm text-gray-600">Order #{order.id}</p>
          </div>
          <div className="text-right">
            <p className="text-sm text-gray-600">Order Date</p>
            <p className="font-medium">{formatDate(order.orderDate)}</p>
          </div>
        </div>

        {/* Order items */}
        <div className="space-y-4 mb-6">
          {order.orderItems?.map((item) => {
            // Get the image URL or fallback to placeholder
            const imageUrl = item.productImageUrl || '/images/placeholder-product.jpg';

            return (
              <div key={item.id} className="flex items-center space-x-4 py-3 border-b border-gray-100 last:border-b-0">
                <div className="relative w-12 h-12 flex-shrink-0">
                  <Image
                    src={imageUrl}
                    alt={item.productName || 'Product'}
                    fill
                    className="object-cover rounded"
                    sizes="48px"
                    onError={(e) => {
                      const target = e.target as HTMLImageElement;
                      target.src = '/images/placeholder-product.jpg';
                    }}
                  />
                </div>
                <div className="flex-1">
                  <h4 className="font-medium text-gray-900">{item.productName || 'Unknown Product'}</h4>
                  <p className="text-sm text-gray-600">Quantity: {item.quantity}</p>
                </div>
                <div className="text-right">
                  <p className="font-medium">{formatPrice(item.subtotal)}</p>
                  <p className="text-sm text-gray-600">{formatPrice(item.unitPrice)} each</p>
                </div>
              </div>
            );
          }) || <p className="text-gray-500">No items found</p>}
        </div>

        {/* Order summary */}
        <div className="border-t border-gray-200 pt-4">
          <div className="flex justify-between text-lg font-semibold">
            <span>Total</span>
            <span className="text-primary">{formatPrice(order.totalAmount)}</span>
          </div>
        </div>
      </div>

      {/* Delivery information */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 mb-8 text-left">
        <h3 className="font-semibold text-blue-900 mb-3">Delivery Information</h3>
        <div className="space-y-2 text-sm text-blue-800">
          <div className="flex justify-between">
            <span>Payment Method:</span>
            <span className="capitalize">{order.paymentMethod?.replace('_', ' ') || 'Not specified'}</span>
          </div>
          <div className="flex justify-between">
            <span>Estimated Delivery:</span>
            <span>{getEstimatedDelivery()}</span>
          </div>
          <div className="flex justify-between">
            <span>Shipping Address:</span>
            <span className="text-right max-w-xs">{order.shippingAddress || 'No address provided'}</span>
          </div>
        </div>
      </div>

      {/* Next steps */}
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-6 mb-8">
        <h3 className="font-semibold text-gray-900 mb-3">What's Next?</h3>
        <div className="space-y-3 text-sm text-gray-600 text-left">
          <div className="flex items-start space-x-3">
            <div className="flex-shrink-0 w-6 h-6 bg-primary text-white rounded-full flex items-center justify-center text-xs font-medium">
              1
            </div>
            <div>
              <p className="font-medium text-gray-900">Order Confirmation</p>
              <p>You'll receive an email confirmation shortly with your order details.</p>
            </div>
          </div>
          <div className="flex items-start space-x-3">
            <div className="flex-shrink-0 w-6 h-6 bg-gray-300 text-gray-600 rounded-full flex items-center justify-center text-xs font-medium">
              2
            </div>
            <div>
              <p className="font-medium text-gray-900">Order Processing</p>
              <p>We'll prepare your order and send you tracking information.</p>
            </div>
          </div>
          <div className="flex items-start space-x-3">
            <div className="flex-shrink-0 w-6 h-6 bg-gray-300 text-gray-600 rounded-full flex items-center justify-center text-xs font-medium">
              3
            </div>
            <div>
              <p className="font-medium text-gray-900">Delivery</p>
              <p>Your order will be delivered to your specified address.</p>
            </div>
          </div>
        </div>
      </div>

      {/* Action buttons */}
      <div className="flex flex-col sm:flex-row gap-4 justify-center">
        <Link
          href={`/orders/${order.id}`}
          className="inline-flex items-center justify-center px-6 py-3 border border-primary text-primary bg-white rounded-lg hover:bg-primary hover:text-white transition-colors"
        >
          View Order Details
        </Link>

        <button
          onClick={onContinueShopping}
          className="inline-flex items-center justify-center px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors"
        >
          Continue Shopping
        </button>
      </div>

      {/* Support information */}
      <div className="mt-8 text-center">
        <p className="text-sm text-gray-600">
          Need help with your order?{' '}
          <Link href="/contact" className="text-primary hover:text-primary-dark">
            Contact our support team
          </Link>
        </p>
      </div>
    </div>
  );
}
