'use client';

import React from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Link from 'next/link';
import { useCart } from '@/context/CartContext';
import { useCartSync } from '@/hooks/useCartSync';
import { CartItem } from '@/components/cart/CartItem';
import { CartSummary } from '@/components/cart/CartSummary';
import { CartEmpty } from '@/components/cart/CartEmpty';


export default function CartPage() {
  const { cart, isEmpty, isLoading } = useCart();

  // Ensure cart computed values are synced on page load
  useCartSync();

  return (
    <MainLayout>
      {/* Cart Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">Cart</h1>
          <div className="flex items-center justify-center mt-4">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">Cart</span>
          </div>
        </div>
      </div>

      {/* Cart Content */}
      <section className="py-16">
        <div className="container-custom">


          {isLoading ? (
            <div className="text-center py-16">
              <div className="w-8 h-8 border-4 border-gray-300 border-t-blue-600 rounded-full animate-spin mx-auto mb-4"></div>
              <p className="text-gray-600">Loading your cart...</p>
            </div>
          ) : isEmpty ? (
            <CartEmpty showRecommendations={true} />
          ) : (
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
              {/* Cart Items */}
              <div className="lg:col-span-2 space-y-4">
                <h2 className="text-2xl font-semibold text-gray-900 mb-6">
                  Shopping Cart ({cart?.totalItems || 0} items)
                </h2>

                {cart?.items.map((item) => (
                  <CartItem
                    key={item.id}
                    item={item}
                    showRemoveButton={true}
                    showWishlistButton={true}
                    layout="horizontal"
                  />
                ))}
              </div>

              {/* Cart Summary */}
              <div className="lg:col-span-1">
                <CartSummary
                  showCheckoutButton={true}
                  showContinueShoppingButton={true}
                  showShippingInfo={true}
                  showPolicies={true}
                />
              </div>
            </div>
          )}
        </div>
      </section>
    </MainLayout>
  );
}
