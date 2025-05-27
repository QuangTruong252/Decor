'use client';

import React, { useEffect } from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Link from 'next/link';
import { useOrderStore } from '@/store/orderStore';
import { OrderList } from '@/components/orders';

export default function OrdersPage() {
  const {
    orders,
    isLoading,
    error,
    fetchUserOrders,
    updateOrderStatus
  } = useOrderStore();

  useEffect(() => {
    // Fetch user orders on component mount
    fetchUserOrders();
  }, [fetchUserOrders]);

  const handleStatusUpdate = (orderId: number, status: string) => {
    updateOrderStatus(orderId, status as any);
  };

  const handleLoadMore = () => {
    // loadMore functionality would be implemented here
    console.log('Load more orders');
  };

  return (
    <MainLayout>
      {/* Orders Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">My Orders</h1>
          <div className="flex items-center justify-center mt-4">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <Link href="/profile" className="text-dark hover:text-primary transition-colors">
              Profile
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">Orders</span>
          </div>
        </div>
      </div>

      {/* Orders Content */}
      <section className="py-16">
        <div className="container-custom">
          <div className="max-w-6xl mx-auto">
            {/* Page header */}
            <div className="flex justify-between items-center mb-8">
              <div>
                <h2 className="text-2xl font-bold text-gray-900">Order History</h2>
                <p className="text-gray-600 mt-1">
                  Track and manage your orders
                </p>
              </div>

              <div className="flex space-x-4">
                <Link
                  href="/shop"
                  className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                >
                  Continue Shopping
                </Link>

                <button className="inline-flex items-center px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors">
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                  </svg>
                  Export Orders
                </button>
              </div>
            </div>

            {/* Order statistics */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
              <div className="bg-white p-6 rounded-lg border border-gray-200">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-lg">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Total Orders</p>
                    <p className="text-2xl font-bold text-gray-900">{orders.length}</p>
                  </div>
                </div>
              </div>

              <div className="bg-white p-6 rounded-lg border border-gray-200">
                <div className="flex items-center">
                  <div className="p-2 bg-green-100 rounded-lg">
                    <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Delivered</p>
                    <p className="text-2xl font-bold text-gray-900">
                      {orders.filter(o => o.orderStatus === 'delivered').length}
                    </p>
                  </div>
                </div>
              </div>

              <div className="bg-white p-6 rounded-lg border border-gray-200">
                <div className="flex items-center">
                  <div className="p-2 bg-yellow-100 rounded-lg">
                    <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">In Progress</p>
                    <p className="text-2xl font-bold text-gray-900">
                      {orders.filter(o => o.orderStatus && ['pending', 'confirmed', 'processing', 'shipped'].includes(o.orderStatus)).length}
                    </p>
                  </div>
                </div>
              </div>

              <div className="bg-white p-6 rounded-lg border border-gray-200">
                <div className="flex items-center">
                  <div className="p-2 bg-purple-100 rounded-lg">
                    <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Total Spent</p>
                    <p className="text-2xl font-bold text-gray-900">
                      ${orders.reduce((sum, order) => sum + order.totalAmount, 0).toFixed(2)}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Orders list */}
            <OrderList
              orders={orders}
              isLoading={isLoading}
              error={error}
              onStatusUpdate={handleStatusUpdate}
              onLoadMore={handleLoadMore}
              hasMore={false}
              showFilters={true}
            />
          </div>
        </div>
      </section>
    </MainLayout>
  );
}
