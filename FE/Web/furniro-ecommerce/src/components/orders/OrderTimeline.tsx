'use client';

import React from 'react';
import type { OrderDTO } from '@/api/types';
import { OrderStatusManagementService } from '@/services/orderStatusManagement';

interface OrderTimelineProps {
  order: OrderDTO;
  showEstimatedSteps?: boolean;
  compact?: boolean;
}

export default function OrderTimeline({
  order,
  showEstimatedSteps = true,
  compact = false
}: OrderTimelineProps) {
  const timeline = OrderStatusManagementService.generateTimeline(order);
  const allStatuses = ['pending', 'confirmed', 'processing', 'shipped', 'delivered'];
  const currentStatusIndex = allStatuses.indexOf(order.orderStatus || 'pending');

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getStepStatus = (stepIndex: number) => {
    const status = order.orderStatus || 'pending';
    if (status === 'cancelled' || status === 'refunded') {
      return stepIndex === 0 ? 'completed' : 'cancelled';
    }

    if (stepIndex < currentStatusIndex) return 'completed';
    if (stepIndex === currentStatusIndex) return 'current';
    return 'upcoming';
  };

  const getStepIcon = (status: string, stepStatus: string) => {
    const statusInfo = OrderStatusManagementService.getStatusInfo(status as any);

    if (stepStatus === 'completed') {
      return (
        <div className="w-8 h-8 bg-green-500 rounded-full flex items-center justify-center">
          <svg className="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
          </svg>
        </div>
      );
    }

    if (stepStatus === 'current') {
      return (
        <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center ring-4 ring-blue-100">
          <span className="text-white text-sm">{statusInfo.icon}</span>
        </div>
      );
    }

    if (stepStatus === 'cancelled') {
      return (
        <div className="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center">
          <svg className="w-4 h-4 text-gray-500" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
          </svg>
        </div>
      );
    }

    return (
      <div className="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center">
        <span className="text-gray-400 text-sm">{statusInfo.icon}</span>
      </div>
    );
  };

  const getConnectorClass = (stepStatus: string) => {
    if (stepStatus === 'completed') return 'bg-green-500';
    if (stepStatus === 'cancelled') return 'bg-gray-300';
    return 'bg-gray-200';
  };

  if (compact) {
    return (
      <div className="space-y-3">
        {timeline.map((event, index) => (
          <div key={index} className="flex items-center space-x-3">
            <div className="w-6 h-6 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0">
              <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-900">{event.description}</p>
              <p className="text-xs text-gray-500">{formatDate(event.timestamp)}</p>
            </div>
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h3 className="text-lg font-semibold text-gray-900">Order Timeline</h3>

      <div className="relative">
        {/* Timeline steps */}
        {allStatuses.map((status, index) => {
          const stepStatus = getStepStatus(index);
          const statusInfo = OrderStatusManagementService.getStatusInfo(status as any);
          const timelineEvent = timeline.find(event => event.status === status);

          return (
            <div key={status} className="relative">
              {/* Connector line */}
              {index < allStatuses.length - 1 && (
                <div className="absolute left-4 top-8 w-0.5 h-16 -ml-px">
                  <div className={`h-full ${getConnectorClass(stepStatus)}`}></div>
                </div>
              )}

              {/* Step content */}
              <div className="relative flex items-start space-x-4">
                {/* Step icon */}
                {getStepIcon(status, stepStatus)}

                {/* Step details */}
                <div className="flex-1 min-w-0 pb-8">
                  <div className="flex items-center justify-between">
                    <div>
                      <h4 className={`text-sm font-medium ${
                        stepStatus === 'current' ? 'text-blue-600' :
                        stepStatus === 'completed' ? 'text-green-600' :
                        stepStatus === 'cancelled' ? 'text-gray-500' :
                        'text-gray-400'
                      }`}>
                        {statusInfo.label}
                      </h4>
                      <p className="text-sm text-gray-600 mt-1">
                        {statusInfo.description}
                      </p>
                    </div>

                    {/* Timestamp */}
                    {timelineEvent && (
                      <div className="text-right">
                        <p className="text-xs text-gray-500">
                          {formatDate(timelineEvent.timestamp)}
                        </p>
                        {timelineEvent.updatedBy && (
                          <p className="text-xs text-gray-400">
                            by {timelineEvent.updatedBy}
                          </p>
                        )}
                      </div>
                    )}

                    {/* Estimated time for upcoming steps */}
                    {showEstimatedSteps && stepStatus === 'upcoming' && statusInfo.estimatedDuration && (
                      <div className="text-right">
                        <p className="text-xs text-gray-400">
                          Est. {statusInfo.estimatedDuration}h
                        </p>
                      </div>
                    )}
                  </div>

                  {/* Additional notes */}
                  {timelineEvent?.note && (
                    <div className="mt-2 p-2 bg-gray-50 rounded text-xs text-gray-600">
                      {timelineEvent.note}
                    </div>
                  )}

                  {/* Current step additional info */}
                  {stepStatus === 'current' && (
                    <div className="mt-3">
                      {status === 'processing' && (
                        <div className="flex items-center space-x-2 text-sm text-blue-600">
                          <svg className="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                          </svg>
                          <span>Your order is being prepared...</span>
                        </div>
                      )}

                      {status === 'shipped' && (
                        <div className="flex items-center space-x-2 text-sm text-blue-600">
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                          </svg>
                          <span>Package is in transit</span>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Cancelled/Refunded status */}
      {(order.orderStatus === 'cancelled' || order.orderStatus === 'refunded') && (
        <div className="mt-6 p-4 bg-gray-50 border border-gray-200 rounded-lg">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-gray-400 rounded-full flex items-center justify-center">
              <svg className="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
              </svg>
            </div>
            <div>
              <h4 className="text-sm font-medium text-gray-900">
                Order {OrderStatusManagementService.formatStatus(order.orderStatus || 'unknown')}
              </h4>
              <p className="text-sm text-gray-600">
                {formatDate(order.updatedAt)}
              </p>
              {order.orderStatus === 'refunded' && (
                <p className="text-xs text-gray-500 mt-1">
                  Refund will be processed within 3-5 business days
                </p>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Estimated delivery */}
      {showEstimatedSteps && !OrderStatusManagementService.getStatusInfo((order.orderStatus || 'pending') as any).isFinal && (
        <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
          <div className="flex items-center space-x-3">
            <svg className="w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <h4 className="text-sm font-medium text-blue-800">Estimated Delivery</h4>
              <p className="text-sm text-blue-600">
                {OrderStatusManagementService.getEstimatedDeliveryDate(order)?.toLocaleDateString('en-US', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric'
                }) || 'Calculating...'}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
