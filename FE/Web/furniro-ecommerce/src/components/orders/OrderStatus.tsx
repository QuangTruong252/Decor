'use client';

import React from 'react';
import type { OrderStatus as OrderStatusType, OrderDTO } from '@/api/types';
import { OrderStatusManagementService } from '@/services/orderStatusManagement';

interface OrderStatusProps {
  order: OrderDTO;
  showProgress?: boolean;
  showEstimatedDelivery?: boolean;
  size?: 'sm' | 'md' | 'lg';
  onStatusChange?: (newStatus: OrderStatusType) => void;
  userRole?: 'customer' | 'admin' | 'system';
}

export default function OrderStatus({
  order,
  showProgress = false,
  showEstimatedDelivery = false,
  size = 'md',
  onStatusChange,
  userRole = 'customer'
}: OrderStatusProps) {
  const statusInfo = OrderStatusManagementService.getStatusInfo(order.orderStatus as any);
  const progress = OrderStatusManagementService.getOrderProgress(order.orderStatus as any);
  const estimatedDelivery = OrderStatusManagementService.getEstimatedDeliveryDate(order);
  const allowedTransitions = OrderStatusManagementService.getAllowedTransitions(order, userRole);

  const handleStatusChange = (newStatus: OrderStatusType) => {
    const validation = OrderStatusManagementService.validateTransition(order, newStatus, userRole);

    if (!validation.isValid) {
      alert(validation.reason);
      return;
    }

    onStatusChange?.(newStatus);
  };

  const getSizeClasses = () => {
    switch (size) {
      case 'sm':
        return {
          badge: 'px-2 py-1 text-xs',
          icon: 'text-sm',
          text: 'text-sm'
        };
      case 'lg':
        return {
          badge: 'px-4 py-2 text-base',
          icon: 'text-xl',
          text: 'text-lg'
        };
      default:
        return {
          badge: 'px-3 py-1 text-sm',
          icon: 'text-base',
          text: 'text-base'
        };
    }
  };

  const sizeClasses = getSizeClasses();

  return (
    <div className="space-y-3">
      {/* Status badge */}
      <div className="flex items-center space-x-2">
        <span className={`${OrderStatusManagementService.getStatusBadgeClass(order.orderStatus as any)} ${sizeClasses.badge}`}>
          <span className={`mr-1 ${sizeClasses.icon}`}>{statusInfo.icon}</span>
          {statusInfo.label}
        </span>

        {/* Status actions for admin */}
        {userRole === 'admin' && allowedTransitions.length > 0 && (
          <div className="relative group">
            <button className="text-gray-400 hover:text-gray-600">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z" />
              </svg>
            </button>

            <div className="absolute right-0 top-6 w-48 bg-white border border-gray-200 rounded-lg shadow-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-10">
              <div className="py-1">
                {allowedTransitions.map((transition) => (
                  <button
                    key={transition.to}
                    onClick={() => handleStatusChange(transition.to)}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    {transition.description}
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Status description */}
      <p className={`text-gray-600 ${sizeClasses.text}`}>
        {statusInfo.description}
      </p>

      {/* Progress bar */}
      {showProgress && !statusInfo.isFinal && (
        <div className="space-y-2">
          <div className="flex justify-between text-sm text-gray-600">
            <span>Order Progress</span>
            <span>{progress}%</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className={`h-2 rounded-full transition-all duration-300 ${
                progress === 100 ? 'bg-green-500' : 'bg-blue-500'
              }`}
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>
      )}

      {/* Estimated delivery */}
      {showEstimatedDelivery && estimatedDelivery && !statusInfo.isFinal && (
        <div className="flex items-center space-x-2 text-sm text-gray-600">
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span>
            Estimated delivery: {estimatedDelivery.toLocaleDateString('en-US', {
              year: 'numeric',
              month: 'long',
              day: 'numeric'
            })}
          </span>
        </div>
      )}

      {/* Status-specific information */}
      {order.orderStatus === 'shipped' && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
          <div className="flex items-center space-x-2">
            <svg className="w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
            </svg>
            <div>
              <p className="text-sm font-medium text-blue-800">Your order is on its way!</p>
              <p className="text-xs text-blue-600">Track your package for real-time updates</p>
            </div>
          </div>
        </div>
      )}

      {order.orderStatus === 'delivered' && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-3">
          <div className="flex items-center space-x-2">
            <svg className="w-5 h-5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <p className="text-sm font-medium text-green-800">Order delivered successfully!</p>
              <p className="text-xs text-green-600">We hope you enjoy your purchase</p>
            </div>
          </div>
        </div>
      )}

      {order.orderStatus === 'cancelled' && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-3">
          <div className="flex items-center space-x-2">
            <svg className="w-5 h-5 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
            <div>
              <p className="text-sm font-medium text-red-800">Order cancelled</p>
              <p className="text-xs text-red-600">If you paid online, your refund will be processed within 3-5 business days</p>
            </div>
          </div>
        </div>
      )}

      {/* Customer actions */}
      {userRole === 'customer' && (
        <div className="flex space-x-3">
          {OrderStatusManagementService.canCustomerCancel(order) && (
            <button
              onClick={() => handleStatusChange('cancelled')}
              className="text-sm text-red-600 hover:text-red-700 font-medium"
            >
              Cancel Order
            </button>
          )}

          {OrderStatusManagementService.canReturn(order) && (
            <button
              onClick={() => handleStatusChange('refunded')}
              className="text-sm text-gray-600 hover:text-gray-700 font-medium"
            >
              Request Return
            </button>
          )}

          {order.orderStatus === 'delivered' && (
            <button className="text-sm text-blue-600 hover:text-blue-700 font-medium">
              Leave Review
            </button>
          )}
        </div>
      )}
    </div>
  );
}
