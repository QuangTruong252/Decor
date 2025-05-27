'use client';

import React from 'react';
import Link from 'next/link';
import type { OrderDTO } from '@/api/types';
import { OrderService } from '@/api/services';
import { OrderStatusManagementService } from '@/services/orderStatusManagement';

interface OrderCardProps {
  order: OrderDTO;
  onStatusUpdate?: (orderId: number, status: string) => void;
  showActions?: boolean;
}

export default function OrderCard({ order, onStatusUpdate, showActions = true }: OrderCardProps) {
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const getStatusBadgeClass = (status: string) => {
    return OrderStatusManagementService.getStatusBadgeClass(status as any);
  };

  const canCancel = OrderStatusManagementService.canCustomerCancel(order);
  const canReturn = OrderStatusManagementService.canReturn(order);

  const handleCancelOrder = async () => {
    if (window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await OrderService.cancelOrder(order.id);
        onStatusUpdate?.(order.id, 'cancelled');
      } catch (error) {
        console.error('Failed to cancel order:', error);
        alert('Failed to cancel order. Please try again.');
      }
    }
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow">
      {/* Order header */}
      <div className="flex justify-between items-start mb-4">
        <div>
          <div className="flex items-center space-x-3">
            <h3 className="text-lg font-semibold text-gray-900">
              Order #{order.id}
            </h3>
            <span className={getStatusBadgeClass(order.orderStatus || 'unknown')}>
              {OrderStatusManagementService.formatStatus((order.orderStatus || 'unknown') as any)}
            </span>
          </div>
          <p className="text-sm text-gray-600 mt-1">
            Placed on {formatDate(order.orderDate)}
          </p>
        </div>
        <div className="text-right">
          <p className="text-lg font-semibold text-gray-900">
            {formatPrice(order.totalAmount)}
          </p>
          <p className="text-sm text-gray-600">
            {order.orderItems?.length || 0} item{(order.orderItems?.length || 0) !== 1 ? 's' : ''}
          </p>
        </div>
      </div>

      {/* Order items preview */}
      <div className="mb-4">
        <div className="flex items-center space-x-3 overflow-x-auto pb-2">
          {order.orderItems?.slice(0, 3).map((item) => (
            <div key={item.id} className="flex-shrink-0 flex items-center space-x-2 bg-gray-50 rounded-lg p-2">
              <img
                src={item.productImageUrl || '/placeholder-product.jpg'}
                alt={item.productName || 'Product'}
                className="w-10 h-10 object-cover rounded"
              />
              <div className="min-w-0">
                <p className="text-sm font-medium text-gray-900 truncate max-w-32">
                  {item.productName || 'Unknown Product'}
                </p>
                <p className="text-xs text-gray-600">
                  Qty: {item.quantity}
                </p>
              </div>
            </div>
          )) || <p className="text-gray-500">No items</p>}
          {(order.orderItems?.length || 0) > 3 && (
            <div className="flex-shrink-0 flex items-center justify-center w-16 h-14 bg-gray-100 rounded-lg">
              <span className="text-xs text-gray-600">
                +{(order.orderItems?.length || 0) - 3} more
              </span>
            </div>
          )}
        </div>
      </div>

      {/* Order details */}
      <div className="grid grid-cols-2 gap-4 mb-4 text-sm">
        <div>
          <span className="text-gray-600">Customer:</span>
          <p className="font-medium">{order.userFullName || 'Unknown Customer'}</p>
        </div>
        <div>
          <span className="text-gray-600">Payment:</span>
          <p className="font-medium capitalize">
            {order.paymentMethod?.replace('_', ' ') || 'Not specified'}
          </p>
        </div>
      </div>

      {/* Shipping address */}
      <div className="mb-4">
        <span className="text-sm text-gray-600">Shipping to:</span>
        <p className="text-sm font-medium text-gray-900 mt-1">
          {order.shippingAddress || 'No address provided'}
        </p>
      </div>

      {/* Actions */}
      {showActions && (
        <div className="flex justify-between items-center pt-4 border-t border-gray-200">
          <div className="flex space-x-3">
            <Link
              href={`/orders/${order.id}`}
              className="text-sm text-primary hover:text-primary-dark font-medium"
            >
              View Details
            </Link>

            {canCancel && (
              <button
                onClick={handleCancelOrder}
                className="text-sm text-red-600 hover:text-red-700 font-medium"
              >
                Cancel Order
              </button>
            )}

            {canReturn && (
              <button className="text-sm text-gray-600 hover:text-gray-700 font-medium">
                Return Items
              </button>
            )}
          </div>

          <div className="flex space-x-2">
            <button className="text-sm text-gray-600 hover:text-gray-700 font-medium">
              Reorder
            </button>

            {order.orderStatus === 'delivered' && (
              <button className="text-sm text-primary hover:text-primary-dark font-medium">
                Leave Review
              </button>
            )}
          </div>
        </div>
      )}

      {/* Delivery estimate */}
      {(order.orderStatus === 'processing' || order.orderStatus === 'shipped') && (
        <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
          <div className="flex items-center">
            <svg className="w-4 h-4 text-blue-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
              <path d="M8 16.5a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0zM15 16.5a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0z" />
              <path d="M3 4a1 1 0 00-1 1v10a1 1 0 001 1h1.05a2.5 2.5 0 014.9 0H10a1 1 0 001-1V5a1 1 0 00-1-1H3zM14 7a1 1 0 00-1 1v6.05A2.5 2.5 0 0115.95 16H17a1 1 0 001-1V8a1 1 0 00-1-1h-3z" />
            </svg>
            <span className="text-sm text-blue-800">
              {order.orderStatus === 'processing'
                ? 'Your order is being prepared for shipment'
                : 'Your order is on its way'
              }
            </span>
          </div>
        </div>
      )}
    </div>
  );
}
