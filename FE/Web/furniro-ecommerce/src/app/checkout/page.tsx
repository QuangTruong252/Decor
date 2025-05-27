'use client';

import React from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Link from 'next/link';
import { CheckoutForm } from '@/components/checkout';

export default function CheckoutPage() {
  const handleOrderComplete = (orderId: number) => {
    console.log('Order completed:', orderId);
    // Additional logic after order completion
  };

  return (
    <MainLayout>
      {/* Checkout Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">Checkout</h1>
          <div className="flex items-center justify-center mt-4">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <Link href="/cart" className="text-dark hover:text-primary transition-colors">
              Cart
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">Checkout</span>
          </div>
        </div>
      </div>

      {/* Checkout Content */}
      <section className="py-16">
        <div className="container-custom">
          <CheckoutForm onOrderComplete={handleOrderComplete} />
        </div>
      </section>
    </MainLayout>
  );
}
