'use client';

import React, { useState } from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Link from 'next/link';
import { useCart } from '@/context/CartContext';

export default function CheckoutPage() {
  const { cartItems, subtotal } = useCart();
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    companyName: '',
    country: '',
    streetAddress: '',
    apartment: '',
    city: '',
    state: '',
    zipCode: '',
    phone: '',
    email: '',
    createAccount: false,
    notes: '',
    paymentMethod: 'directBankTransfer',
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target as HTMLInputElement;
    const checked = type === 'checkbox' ? (e.target as HTMLInputElement).checked : undefined;

    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value,
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // In a real app, this would submit the order to an API
    console.log('Order submitted:', formData);
    // Redirect to order confirmation page
  };

  const shipping = 50; // Fixed shipping cost
  const total = subtotal + shipping;

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
          <form onSubmit={handleSubmit}>
            <div className="flex flex-col lg:flex-row gap-12">
              {/* Billing Details */}
              <div className="w-full lg:w-2/3">
                <h2 className="text-2xl font-bold mb-6">Billing Details</h2>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                  <div>
                    <label htmlFor="firstName" className="block text-dark mb-2">First Name *</label>
                    <input
                      type="text"
                      id="firstName"
                      name="firstName"
                      required
                      className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                      value={formData.firstName}
                      onChange={handleInputChange}
                    />
                  </div>

                  <div>
                    <label htmlFor="lastName" className="block text-dark mb-2">Last Name *</label>
                    <input
                      type="text"
                      id="lastName"
                      name="lastName"
                      required
                      className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                      value={formData.lastName}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>

                <div className="mb-6">
                  <label htmlFor="companyName" className="block text-dark mb-2">Company Name (Optional)</label>
                  <input
                    type="text"
                    id="companyName"
                    name="companyName"
                    className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                    value={formData.companyName}
                    onChange={handleInputChange}
                  />
                </div>

                <div className="mb-6">
                  <label htmlFor="country" className="block text-dark mb-2">Country / Region *</label>
                  <select
                    id="country"
                    name="country"
                    required
                    className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                    value={formData.country}
                    onChange={handleInputChange}
                  >
                    <option value="">Select a country</option>
                    <option value="US">United States</option>
                    <option value="CA">Canada</option>
                    <option value="UK">United Kingdom</option>
                    <option value="AU">Australia</option>
                  </select>
                </div>

                <div className="mb-6">
                  <label htmlFor="streetAddress" className="block text-dark mb-2">Street Address *</label>
                  <input
                    type="text"
                    id="streetAddress"
                    name="streetAddress"
                    required
                    placeholder="House number and street name"
                    className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary mb-4"
                    value={formData.streetAddress}
                    onChange={handleInputChange}
                  />
                  <input
                    type="text"
                    id="apartment"
                    name="apartment"
                    placeholder="Apartment, suite, unit, etc. (optional)"
                    className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                    value={formData.apartment}
                    onChange={handleInputChange}
                  />
                </div>

                <div className="mb-6">
                  <label htmlFor="city" className="block text-dark mb-2">Town / City *</label>
                  <input
                    type="text"
                    id="city"
                    name="city"
                    required
                    className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                    value={formData.city}
                    onChange={handleInputChange}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                  <div>
                    <label htmlFor="state" className="block text-dark mb-2">State *</label>
                    <input
                      type="text"
                      id="state"
                      name="state"
                      required
                      className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                      value={formData.state}
                      onChange={handleInputChange}
                    />
                  </div>

                  <div>
                    <label htmlFor="zipCode" className="block text-dark mb-2">ZIP Code *</label>
                    <input
                      type="text"
                      id="zipCode"
                      name="zipCode"
                      required
                      className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                      value={formData.zipCode}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                  <div>
                    <label htmlFor="phone" className="block text-dark mb-2">Phone *</label>
                    <input
                      type="tel"
                      id="phone"
                      name="phone"
                      required
                      className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                      value={formData.phone}
                      onChange={handleInputChange}
                    />
                  </div>

                  <div>
                    <label htmlFor="email" className="block text-dark mb-2">Email Address *</label>
                    <input
                      type="email"
                      id="email"
                      name="email"
                      required
                      className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary"
                      value={formData.email}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>

                <div className="mb-6">
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="createAccount"
                      name="createAccount"
                      className="mr-2"
                      checked={formData.createAccount}
                      onChange={handleInputChange}
                    />
                    <label htmlFor="createAccount" className="text-dark">Create an account?</label>
                  </div>
                </div>

                <div className="mb-6">
                  <label htmlFor="notes" className="block text-dark mb-2">Order Notes (Optional)</label>
                  <textarea
                    id="notes"
                    name="notes"
                    placeholder="Notes about your order, e.g. special notes for delivery"
                    className="w-full border border-border-color p-3 rounded focus:outline-none focus:border-primary h-32"
                    value={formData.notes}
                    onChange={handleInputChange}
                  ></textarea>
                </div>
              </div>

              {/* Order Summary */}
              <div className="w-full lg:w-1/3">
                <div className="bg-light p-6 rounded">
                  <h3 className="text-xl font-medium mb-6">Your Order</h3>

                  <div className="space-y-4">
                    <div className="flex justify-between pb-4 border-b border-border-color font-medium">
                      <span>Product</span>
                      <span>Subtotal</span>
                    </div>

                    {cartItems.map((item) => (
                      <div key={item.id} className="flex justify-between pb-4 border-b border-border-color">
                        <span>{item.name} × {item.quantity}</span>
                        <span>${(item.price * item.quantity).toFixed(2)}</span>
                      </div>
                    ))}

                    <div className="flex justify-between pb-4 border-b border-border-color">
                      <span>Subtotal</span>
                      <span>${subtotal.toFixed(2)}</span>
                    </div>

                    <div className="flex justify-between pb-4 border-b border-border-color">
                      <span>Shipping</span>
                      <span>${shipping.toFixed(2)}</span>
                    </div>

                    <div className="flex justify-between font-medium">
                      <span>Total</span>
                      <span className="text-primary">${total.toFixed(2)}</span>
                    </div>
                  </div>

                  {/* Payment Methods */}
                  <div className="mt-8 space-y-4">
                    <div className="border-b border-border-color pb-4">
                      <div className="flex items-center mb-2">
                        <input
                          type="radio"
                          id="directBankTransfer"
                          name="paymentMethod"
                          value="directBankTransfer"
                          checked={formData.paymentMethod === 'directBankTransfer'}
                          onChange={handleInputChange}
                          className="mr-2"
                        />
                        <label htmlFor="directBankTransfer" className="font-medium">Direct Bank Transfer</label>
                      </div>
                      <p className="text-text-secondary text-sm pl-6">
                        Make your payment directly into our bank account. Please use your Order ID as the payment reference.
                      </p>
                    </div>

                    <div className="border-b border-border-color pb-4">
                      <div className="flex items-center">
                        <input
                          type="radio"
                          id="checkPayment"
                          name="paymentMethod"
                          value="checkPayment"
                          checked={formData.paymentMethod === 'checkPayment'}
                          onChange={handleInputChange}
                          className="mr-2"
                        />
                        <label htmlFor="checkPayment" className="font-medium">Check Payment</label>
                      </div>
                    </div>

                    <div className="border-b border-border-color pb-4">
                      <div className="flex items-center">
                        <input
                          type="radio"
                          id="cashOnDelivery"
                          name="paymentMethod"
                          value="cashOnDelivery"
                          checked={formData.paymentMethod === 'cashOnDelivery'}
                          onChange={handleInputChange}
                          className="mr-2"
                        />
                        <label htmlFor="cashOnDelivery" className="font-medium">Cash On Delivery</label>
                      </div>
                    </div>

                    <div>
                      <div className="flex items-center">
                        <input
                          type="radio"
                          id="paypal"
                          name="paymentMethod"
                          value="paypal"
                          checked={formData.paymentMethod === 'paypal'}
                          onChange={handleInputChange}
                          className="mr-2"
                        />
                        <label htmlFor="paypal" className="font-medium">PayPal</label>
                      </div>
                    </div>
                  </div>

                  <button type="submit" className="btn-primary w-full text-center block mt-8">
                    Place Order
                  </button>
                </div>
              </div>
            </div>
          </form>
        </div>
      </section>
    </MainLayout>
  );
}
