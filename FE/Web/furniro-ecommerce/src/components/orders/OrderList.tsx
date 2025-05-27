'use client';

import React, { useState } from 'react';
import type { OrderDTO, OrderStatus } from '@/api/types';
import OrderCard from './OrderCard';
import OrderEmpty from './OrderEmpty';

interface OrderListProps {
  orders: OrderDTO[];
  isLoading?: boolean;
  error?: string | null;
  onStatusUpdate?: (orderId: number, status: string) => void;
  onLoadMore?: () => void;
  hasMore?: boolean;
  showFilters?: boolean;
}

export default function OrderList({
  orders,
  isLoading = false,
  error = null,
  onStatusUpdate,
  onLoadMore,
  hasMore = false,
  showFilters = true
}: OrderListProps) {
  const [statusFilter, setStatusFilter] = useState<OrderStatus | 'all'>('all');
  const [sortBy, setSortBy] = useState<'date' | 'amount'>('date');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

  const statusOptions = [
    { value: 'all', label: 'All Orders', count: orders.length },
    { value: 'pending', label: 'Pending', count: orders.filter(o => o.orderStatus === 'pending').length },
    { value: 'confirmed', label: 'Confirmed', count: orders.filter(o => o.orderStatus === 'confirmed').length },
    { value: 'processing', label: 'Processing', count: orders.filter(o => o.orderStatus === 'processing').length },
    { value: 'shipped', label: 'Shipped', count: orders.filter(o => o.orderStatus === 'shipped').length },
    { value: 'delivered', label: 'Delivered', count: orders.filter(o => o.orderStatus === 'delivered').length },
    { value: 'cancelled', label: 'Cancelled', count: orders.filter(o => o.orderStatus === 'cancelled').length },
  ];

  // Filter and sort orders
  const filteredAndSortedOrders = React.useMemo(() => {
    let filtered = orders;

    // Apply status filter
    if (statusFilter !== 'all') {
      filtered = filtered.filter(order => order.orderStatus === statusFilter);
    }

    // Apply sorting
    filtered.sort((a, b) => {
      let comparison = 0;
      
      if (sortBy === 'date') {
        comparison = new Date(a.orderDate).getTime() - new Date(b.orderDate).getTime();
      } else if (sortBy === 'amount') {
        comparison = a.totalAmount - b.totalAmount;
      }

      return sortOrder === 'desc' ? -comparison : comparison;
    });

    return filtered;
  }, [orders, statusFilter, sortBy, sortOrder]);

  if (error) {
    return (
      <div className="text-center py-12">
        <div className="text-red-600 mb-4">
          <svg className="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">Failed to load orders</h3>
        <p className="text-gray-600 mb-4">{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="inline-flex items-center px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark"
        >
          Try Again
        </button>
      </div>
    );
  }

  if (!isLoading && orders.length === 0) {
    return <OrderEmpty />;
  }

  return (
    <div>
      {/* Filters */}
      {showFilters && (
        <div className="mb-6 space-y-4">
          {/* Status filter tabs */}
          <div className="border-b border-gray-200">
            <nav className="-mb-px flex space-x-8 overflow-x-auto">
              {statusOptions.map((option) => (
                <button
                  key={option.value}
                  onClick={() => setStatusFilter(option.value as OrderStatus | 'all')}
                  className={`whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm ${
                    statusFilter === option.value
                      ? 'border-primary text-primary'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  {option.label}
                  {option.count > 0 && (
                    <span className={`ml-2 py-0.5 px-2 rounded-full text-xs ${
                      statusFilter === option.value
                        ? 'bg-primary text-white'
                        : 'bg-gray-100 text-gray-600'
                    }`}>
                      {option.count}
                    </span>
                  )}
                </button>
              ))}
            </nav>
          </div>

          {/* Sort controls */}
          <div className="flex justify-between items-center">
            <p className="text-sm text-gray-600">
              {filteredAndSortedOrders.length} order{filteredAndSortedOrders.length !== 1 ? 's' : ''}
            </p>
            
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2">
                <label htmlFor="sortBy" className="text-sm text-gray-600">Sort by:</label>
                <select
                  id="sortBy"
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value as 'date' | 'amount')}
                  className="text-sm border border-gray-300 rounded px-2 py-1 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                >
                  <option value="date">Order Date</option>
                  <option value="amount">Order Amount</option>
                </select>
              </div>
              
              <button
                onClick={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')}
                className="text-sm text-gray-600 hover:text-gray-900 flex items-center space-x-1"
              >
                <span>{sortOrder === 'asc' ? 'Ascending' : 'Descending'}</span>
                <svg className={`w-4 h-4 transform ${sortOrder === 'asc' ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Orders list */}
      <div className="space-y-6">
        {isLoading && orders.length === 0 ? (
          // Loading skeleton
          <div className="space-y-6">
            {[...Array(3)].map((_, index) => (
              <div key={index} className="bg-white border border-gray-200 rounded-lg p-6 animate-pulse">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <div className="h-6 bg-gray-200 rounded w-32 mb-2"></div>
                    <div className="h-4 bg-gray-200 rounded w-24"></div>
                  </div>
                  <div className="text-right">
                    <div className="h-6 bg-gray-200 rounded w-20 mb-2"></div>
                    <div className="h-4 bg-gray-200 rounded w-16"></div>
                  </div>
                </div>
                <div className="flex space-x-3 mb-4">
                  {[...Array(3)].map((_, i) => (
                    <div key={i} className="flex items-center space-x-2 bg-gray-50 rounded-lg p-2">
                      <div className="w-10 h-10 bg-gray-200 rounded"></div>
                      <div>
                        <div className="h-4 bg-gray-200 rounded w-20 mb-1"></div>
                        <div className="h-3 bg-gray-200 rounded w-12"></div>
                      </div>
                    </div>
                  ))}
                </div>
                <div className="grid grid-cols-2 gap-4 mb-4">
                  <div className="h-4 bg-gray-200 rounded"></div>
                  <div className="h-4 bg-gray-200 rounded"></div>
                </div>
                <div className="h-4 bg-gray-200 rounded w-3/4"></div>
              </div>
            ))}
          </div>
        ) : (
          filteredAndSortedOrders.map((order) => (
            <OrderCard
              key={order.id}
              order={order}
              onStatusUpdate={onStatusUpdate}
            />
          ))
        )}

        {/* Load more button */}
        {hasMore && !isLoading && (
          <div className="text-center pt-6">
            <button
              onClick={onLoadMore}
              className="inline-flex items-center px-6 py-3 border border-gray-300 rounded-lg text-gray-700 bg-white hover:bg-gray-50 transition-colors"
            >
              Load More Orders
            </button>
          </div>
        )}

        {/* Loading more indicator */}
        {isLoading && orders.length > 0 && (
          <div className="text-center py-6">
            <div className="inline-flex items-center space-x-2">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-primary"></div>
              <span className="text-gray-600">Loading more orders...</span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
