'use client';

import React, { useState } from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Image from 'next/image';
import Link from 'next/link';
import { useCart } from '@/context/CartContext';

export default function CartPage() {
  const { cartItems, updateQuantity, removeFromCart, subtotal } = useCart();
  const [couponCode, setCouponCode] = useState('');

  const shipping = 50; // Fixed shipping cost
  const total = subtotal + shipping;

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
          {cartItems.length === 0 ? (
            <div className="text-center py-16">
              <h2 className="text-2xl font-medium text-dark mb-4">Your cart is empty</h2>
              <p className="text-text-secondary mb-8">Looks like you haven't added any products to your cart yet.</p>
              <Link href="/shop" className="btn-primary">
                Continue Shopping
              </Link>
            </div>
          ) : (
            <>
              {/* Cart Table */}
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-light">
                    <tr>
                      <th className="py-4 px-6 text-left">Product</th>
                      <th className="py-4 px-6 text-left">Price</th>
                      <th className="py-4 px-6 text-left">Quantity</th>
                      <th className="py-4 px-6 text-left">Subtotal</th>
                      <th className="py-4 px-6 text-left"></th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border-color">
                    {cartItems.map((item) => (
                      <tr key={item.id}>
                        <td className="py-4 px-6">
                          <div className="flex items-center">
                            <div className="relative w-16 h-16 mr-4 bg-light rounded">
                              <Image
                                src={item.image}
                                alt={item.name}
                                fill
                                className="object-contain"
                              />
                            </div>
                            <span className="font-medium">{item.name}</span>
                          </div>
                        </td>
                        <td className="py-4 px-6">${item.price.toFixed(2)}</td>
                        <td className="py-4 px-6">
                          <div className="flex border border-border-color rounded-lg w-max">
                            <button
                              className="px-3 py-1 text-dark hover:text-primary transition-colors"
                              onClick={() => updateQuantity(item.id, item.quantity - 1)}
                              aria-label="Decrease quantity"
                            >
                              -
                            </button>
                            <span className="px-3 py-1 border-x border-border-color">{item.quantity}</span>
                            <button
                              className="px-3 py-1 text-dark hover:text-primary transition-colors"
                              onClick={() => updateQuantity(item.id, item.quantity + 1)}
                              aria-label="Increase quantity"
                            >
                              +
                            </button>
                          </div>
                        </td>
                        <td className="py-4 px-6">${(item.price * item.quantity).toFixed(2)}</td>
                        <td className="py-4 px-6">
                          <button
                            className="text-text-secondary hover:text-primary transition-colors"
                            onClick={() => removeFromCart(item.id)}
                            aria-label="Remove item"
                          >
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Cart Actions */}
              <div className="flex flex-col lg:flex-row justify-between mt-12 gap-8">
                <div className="w-full lg:w-1/2">
                  <div className="flex space-x-4">
                    <input
                      type="text"
                      placeholder="Coupon Code"
                      className="flex-1 border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                      value={couponCode}
                      onChange={(e) => setCouponCode(e.target.value)}
                    />
                    <button className="btn-primary">
                      Apply Coupon
                    </button>
                  </div>
                </div>

                <div className="w-full lg:w-1/2 bg-light p-6 rounded">
                  <h3 className="text-xl font-medium mb-6">Cart Totals</h3>

                  <div className="space-y-4">
                    <div className="flex justify-between pb-4 border-b border-border-color">
                      <span>Subtotal</span>
                      <span className="font-medium">${subtotal.toFixed(2)}</span>
                    </div>

                    <div className="flex justify-between pb-4 border-b border-border-color">
                      <span>Shipping</span>
                      <span className="font-medium">${shipping.toFixed(2)}</span>
                    </div>

                    <div className="flex justify-between">
                      <span className="font-medium">Total</span>
                      <span className="font-medium text-primary">${total.toFixed(2)}</span>
                    </div>

                    <Link href="/checkout" className="btn-primary w-full text-center block mt-6">
                      Proceed To Checkout
                    </Link>
                  </div>
                </div>
              </div>
            </>
          )}
        </div>
      </section>
    </MainLayout>
  );
}
