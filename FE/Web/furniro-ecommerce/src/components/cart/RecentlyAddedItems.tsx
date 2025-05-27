'use client';

import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Clock, X } from 'lucide-react';
import { useCart } from '@/context/CartContext';
import type { CartItem } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface RecentlyAddedItem extends CartItem {
  addedAt: number;
}

interface RecentlyAddedItemsProps {
  className?: string;
  maxItems?: number;
  showTimeStamp?: boolean;
}

export function RecentlyAddedItems({
  className = '',
  maxItems = 3,
  showTimeStamp = true,
}: RecentlyAddedItemsProps) {
  const { cart } = useCart();
  const [recentItems, setRecentItems] = useState<RecentlyAddedItem[]>([]);

  useEffect(() => {
    // Get recently added items from localStorage
    const getRecentlyAddedItems = () => {
      try {
        const stored = localStorage.getItem('recently-added-items');
        return stored ? JSON.parse(stored) : [];
      } catch {
        return [];
      }
    };

    const recent = getRecentlyAddedItems();

    // Filter items that are still in cart and sort by most recent
    const validRecentItems = recent
      .filter((recentItem: RecentlyAddedItem) =>
        cart?.items.some(cartItem => cartItem.id === recentItem.id)
      )
      .sort((a: RecentlyAddedItem, b: RecentlyAddedItem) => b.addedAt - a.addedAt)
      .slice(0, maxItems);

    setRecentItems(validRecentItems);
  }, [cart, maxItems]);

  const formatTimeAgo = (timestamp: number) => {
    const now = Date.now();
    const diff = now - timestamp;
    const minutes = Math.floor(diff / (1000 * 60));
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    return `${days}d ago`;
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  };

  const removeFromRecent = (itemId: number) => {
    const updated = recentItems.filter(item => item.id !== itemId);
    setRecentItems(updated);

    // Update localStorage
    try {
      localStorage.setItem('recently-added-items', JSON.stringify(updated));
    } catch (error) {
      console.error('Failed to update recently added items:', error);
    }
  };

  if (recentItems.length === 0) {
    return null;
  }

  return (
    <div className={`bg-white border border-gray-200 rounded-lg p-4 ${className}`}>
      <div className="flex items-center gap-2 mb-4">
        <Clock className="w-4 h-4 text-gray-500" />
        <h3 className="text-sm font-medium text-gray-900">Recently Added</h3>
      </div>

      <div className="space-y-3">
        {recentItems.map((item) => (
          <div key={item.id} className="flex items-center gap-3 group">
            {/* Product Image */}
            <div className="relative w-12 h-12 bg-gray-100 rounded-md overflow-hidden flex-shrink-0">
              {item.productImage ? (
                <Image
                  src={getImageUrl(item.productImage)}
                  alt={item.productName || 'Product'}
                  fill
                  className="object-cover"
                />
              ) : (
                <div className="w-full h-full bg-gray-200 flex items-center justify-center">
                  <span className="text-xs text-gray-400">No Image</span>
                </div>
              )}
            </div>

            {/* Product Info */}
            <div className="flex-1 min-w-0">
              <Link
                href={`/product/${item.productSlug || '#'}`}
                className="text-sm font-medium text-gray-900 hover:text-blue-600 line-clamp-1"
              >
                {item.productName || 'Unknown Product'}
              </Link>
              <div className="flex items-center gap-2 mt-1">
                <span className="text-sm text-gray-600">
                  {item.quantity} × {formatPrice(item.unitPrice)}
                </span>
                {showTimeStamp && (
                  <>
                    <span className="text-gray-300">•</span>
                    <span className="text-xs text-gray-500">
                      {formatTimeAgo(item.addedAt)}
                    </span>
                  </>
                )}
              </div>
            </div>

            {/* Remove Button */}
            <button
              onClick={() => removeFromRecent(item.id)}
              className="opacity-0 group-hover:opacity-100 p-1 text-gray-400 hover:text-red-600 transition-all"
            >
              <X className="w-3 h-3" />
            </button>
          </div>
        ))}
      </div>

      {recentItems.length > 0 && (
        <div className="mt-4 pt-3 border-t border-gray-200">
          <button
            onClick={() => {
              setRecentItems([]);
              localStorage.removeItem('recently-added-items');
            }}
            className="text-xs text-gray-500 hover:text-gray-700 transition-colors"
          >
            Clear recent items
          </button>
        </div>
      )}
    </div>
  );
}

// Helper function to add item to recently added (call this when adding to cart)
export const addToRecentlyAdded = (item: CartItem) => {
  try {
    const stored = localStorage.getItem('recently-added-items');
    const recent: RecentlyAddedItem[] = stored ? JSON.parse(stored) : [];

    // Remove existing item if present
    const filtered = recent.filter(recentItem => recentItem.id !== item.id);

    // Add new item at the beginning
    const updated = [{
      ...item,
      addedAt: Date.now(),
    }, ...filtered].slice(0, 10); // Keep only last 10 items

    localStorage.setItem('recently-added-items', JSON.stringify(updated));
  } catch (error) {
    console.error('Failed to add to recently added items:', error);
  }
};

export default RecentlyAddedItems;
