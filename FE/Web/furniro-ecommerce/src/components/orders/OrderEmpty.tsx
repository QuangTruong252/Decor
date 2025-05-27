'use client';

import React from 'react';
import Link from 'next/link';

interface OrderEmptyProps {
  title?: string;
  description?: string;
  showShopButton?: boolean;
}

export default function OrderEmpty({ 
  title = "No orders yet",
  description = "You haven't placed any orders yet. Start shopping to see your orders here.",
  showShopButton = true
}: OrderEmptyProps) {
  return (
    <div className="text-center py-16">
      {/* Empty state illustration */}
      <div className="mb-8">
        <div className="mx-auto w-24 h-24 bg-gray-100 rounded-full flex items-center justify-center">
          <svg className="w-12 h-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
          </svg>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-md mx-auto">
        <h3 className="text-xl font-semibold text-gray-900 mb-3">
          {title}
        </h3>
        <p className="text-gray-600 mb-8">
          {description}
        </p>

        {/* Action buttons */}
        {showShopButton && (
          <div className="space-y-3">
            <Link
              href="/shop"
              className="inline-flex items-center justify-center w-full px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors"
            >
              Start Shopping
            </Link>
            
            <Link
              href="/"
              className="inline-flex items-center justify-center w-full px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Browse Featured Products
            </Link>
          </div>
        )}
      </div>

      {/* Additional help */}
      <div className="mt-12 pt-8 border-t border-gray-200">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-3xl mx-auto">
          <div className="text-center">
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center mx-auto mb-3">
              <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h4 className="font-medium text-gray-900 mb-1">Fast Delivery</h4>
            <p className="text-sm text-gray-600">Get your orders delivered within 3-7 business days</p>
          </div>

          <div className="text-center">
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mx-auto mb-3">
              <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h4 className="font-medium text-gray-900 mb-1">Quality Guarantee</h4>
            <p className="text-sm text-gray-600">All products come with our quality guarantee</p>
          </div>

          <div className="text-center">
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center mx-auto mb-3">
              <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
              </svg>
            </div>
            <h4 className="font-medium text-gray-900 mb-1">Secure Payment</h4>
            <p className="text-sm text-gray-600">Your payment information is always secure</p>
          </div>
        </div>
      </div>

      {/* Contact support */}
      <div className="mt-8">
        <p className="text-sm text-gray-600">
          Need help getting started?{' '}
          <Link href="/contact" className="text-primary hover:text-primary-dark font-medium">
            Contact our support team
          </Link>
        </p>
      </div>
    </div>
  );
}
