'use client';

import React, { useEffect } from 'react';
import { useParams, useSearchParams } from 'next/navigation';
import MainLayout from '@/components/layout/MainLayout';
import Link from 'next/link';
import { useOrderStore } from '@/store/orderStore';
import { OrderService } from '@/api/services';
import { OrderConfirmation } from '@/components/checkout';
import { OrderStatus, OrderTimeline } from '@/components/orders';
import { OrderStatusManagementService } from '@/services/orderStatusManagement';

export default function OrderDetailPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const orderId = parseInt(params.id as string);
  const isSuccess = searchParams.get('success') === 'true';

  const {
    currentOrder,
    isLoading,
    error,
    fetchOrderById,
    updateOrderStatus
  } = useOrderStore();

  useEffect(() => {
    if (orderId) {
      fetchOrderById(orderId);
    }
  }, [orderId, fetchOrderById]);

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

  const handleCancelOrder = async () => {
    if (currentOrder && window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await updateOrderStatus(currentOrder.id, 'cancelled');
      } catch (error) {
        console.error('Failed to cancel order:', error);
        alert('Failed to cancel order. Please try again.');
      }
    }
  };

  const handleContinueShopping = () => {
    window.location.href = '/shop';
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <div className="max-w-4xl mx-auto">
            <div className="animate-pulse">
              <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
              <div className="bg-white border border-gray-200 rounded-lg p-6">
                <div className="h-6 bg-gray-200 rounded w-1/3 mb-4"></div>
                <div className="space-y-3">
                  <div className="h-4 bg-gray-200 rounded"></div>
                  <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                  <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (error) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <div className="max-w-4xl mx-auto text-center">
            <div className="text-red-600 mb-4">
              <svg className="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Order Not Found</h1>
            <p className="text-gray-600 mb-6">{error}</p>
            <Link
              href="/orders"
              className="inline-flex items-center px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark"
            >
              Back to Orders
            </Link>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (!currentOrder) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <div className="max-w-4xl mx-auto text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Order Not Found</h1>
            <p className="text-gray-600 mb-6">The order you're looking for doesn't exist.</p>
            <Link
              href="/orders"
              className="inline-flex items-center px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark"
            >
              Back to Orders
            </Link>
          </div>
        </div>
      </MainLayout>
    );
  }

  // Show order confirmation for successful orders
  if (isSuccess) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <OrderConfirmation
            order={currentOrder}
            onContinueShopping={handleContinueShopping}
          />
        </div>
      </MainLayout>
    );
  }

  const canCancel = OrderStatusManagementService.canCustomerCancel(currentOrder);
  const canReturn = OrderStatusManagementService.canReturn(currentOrder);

  return (
    <MainLayout>
      {/* Order Detail Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">Order Details</h1>
          <div className="flex items-center justify-center mt-4">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <Link href="/orders" className="text-dark hover:text-primary transition-colors">
              Orders
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">Order #{currentOrder.id}</span>
          </div>
        </div>
      </div>

      {/* Order Detail Content */}
      <section className="py-16">
        <div className="container-custom">
          <div className="max-w-4xl mx-auto">
            {/* Order header */}
            <div className="bg-white border border-gray-200 rounded-lg p-6 mb-6">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h2 className="text-2xl font-bold text-gray-900">Order #{currentOrder.id}</h2>
                  <p className="text-gray-600">Placed on {formatDate(currentOrder.orderDate)}</p>
                </div>
                <div className="text-right">
                  <OrderStatus
                    order={currentOrder}
                    showProgress={true}
                    showEstimatedDelivery={true}
                    size="lg"
                    onStatusChange={(newStatus) => updateOrderStatus(currentOrder.id, newStatus)}
                    userRole="customer"
                  />
                </div>
              </div>

              {/* Order actions */}
              <div className="flex space-x-4">
                {canCancel && (
                  <button
                    onClick={handleCancelOrder}
                    className="px-4 py-2 border border-red-300 text-red-700 rounded-lg hover:bg-red-50"
                  >
                    Cancel Order
                  </button>
                )}

                {canReturn && (
                  <button className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50">
                    Return Items
                  </button>
                )}

                <button className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50">
                  Reorder
                </button>

                <button className="px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark">
                  Track Order
                </button>
              </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              {/* Order items */}
              <div className="lg:col-span-2">
                <div className="bg-white border border-gray-200 rounded-lg p-6">
                  <h3 className="text-lg font-semibold mb-4">Order Items</h3>

                  <div className="space-y-4">
                    {currentOrder.orderItems?.map((item) => (
                      <div key={item.id} className="flex items-center space-x-4 p-4 border border-gray-100 rounded-lg">
                        <img
                          src={item.productImageUrl || '/placeholder-product.jpg'}
                          alt={item.productName || 'Product'}
                          className="w-16 h-16 object-cover rounded"
                        />
                        <div className="flex-1">
                          <h4 className="font-medium text-gray-900">{item.productName || 'Unknown Product'}</h4>
                          <p className="text-sm text-gray-600">Quantity: {item.quantity}</p>
                          <p className="text-sm text-gray-600">Unit Price: {formatPrice(item.unitPrice)}</p>
                        </div>
                        <div className="text-right">
                          <p className="font-medium">{formatPrice(item.subtotal)}</p>
                        </div>
                      </div>
                    )) || <p className="text-gray-500">No items found</p>}
                  </div>

                  {/* Order total */}
                  <div className="mt-6 pt-6 border-t border-gray-200">
                    <div className="flex justify-between text-lg font-semibold">
                      <span>Total</span>
                      <span className="text-primary">{formatPrice(currentOrder.totalAmount)}</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Order information */}
              <div className="space-y-6">
                {/* Shipping information */}
                <div className="bg-white border border-gray-200 rounded-lg p-6">
                  <h3 className="text-lg font-semibold mb-4">Shipping Information</h3>
                  <div className="space-y-3 text-sm">
                    <div>
                      <span className="text-gray-600">Customer:</span>
                      <p className="font-medium">{currentOrder.userFullName || 'Unknown Customer'}</p>
                    </div>
                    <div>
                      <span className="text-gray-600">Address:</span>
                      <p className="font-medium">{currentOrder.shippingAddress || 'No address provided'}</p>
                    </div>
                    {currentOrder.shippingDetails?.trackingNumber && (
                      <div>
                        <span className="text-gray-600">Tracking Number:</span>
                        <p className="font-medium">{currentOrder.shippingDetails.trackingNumber}</p>
                      </div>
                    )}
                  </div>
                </div>

                {/* Payment information */}
                <div className="bg-white border border-gray-200 rounded-lg p-6">
                  <h3 className="text-lg font-semibold mb-4">Payment Information</h3>
                  <div className="space-y-3 text-sm">
                    <div>
                      <span className="text-gray-600">Payment Method:</span>
                      <p className="font-medium capitalize">{currentOrder.paymentMethod?.replace('_', ' ') || 'Not specified'}</p>
                    </div>
                    <div>
                      <span className="text-gray-600">Payment Status:</span>
                      <p className="font-medium capitalize">{currentOrder.paymentDetails?.status || 'Pending'}</p>
                    </div>
                    {currentOrder.paymentDetails?.transactionId && (
                      <div>
                        <span className="text-gray-600">Transaction ID:</span>
                        <p className="font-medium">{currentOrder.paymentDetails.transactionId}</p>
                      </div>
                    )}
                  </div>
                </div>

                {/* Order timeline */}
                <div className="bg-white border border-gray-200 rounded-lg p-6">
                  <OrderTimeline
                    order={currentOrder}
                    showEstimatedSteps={true}
                    compact={false}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </MainLayout>
  );
}
