'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { Trash2, Plus, Minus, Heart } from 'lucide-react';
import { useCartItem } from '@/context/CartContext';
import { Button } from '@/components/ui/Button';
import type { CartItem as CartItemType } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface CartItemProps {
  item: CartItemType;
  className?: string;
  showRemoveButton?: boolean;
  showWishlistButton?: boolean;
  layout?: 'horizontal' | 'vertical';
}

export function CartItem({
  item,
  className = '',
  showRemoveButton = true,
  showWishlistButton = false,
  layout = 'horizontal',
}: CartItemProps) {
  const { updateQuantity, removeItem, isUpdating, error } = useCartItem(item.id);
  const [localQuantity, setLocalQuantity] = useState(item.quantity);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  };

  const handleQuantityChange = async (newQuantity: number) => {
    if (newQuantity < 1) return;

    setLocalQuantity(newQuantity);
    try {
      await updateQuantity(newQuantity);
    } catch (error) {
      // Revert local quantity on error
      setLocalQuantity(item.quantity);
    }
  };

  const handleRemove = async () => {
    try {
      await removeItem();
    } catch (error) {
      console.error('Failed to remove item:', error);
    }
  };

  const incrementQuantity = () => {
    handleQuantityChange(localQuantity + 1);
  };

  const decrementQuantity = () => {
    if (localQuantity > 1) {
      handleQuantityChange(localQuantity - 1);
    }
  };

  if (layout === 'vertical') {
    return (
      <div className={`bg-white border border-gray-200 rounded-lg p-4 ${className}`}>
        {/* Product Image */}
        <div className="relative w-full h-48 bg-gray-100 rounded-md overflow-hidden mb-4">
          {item.productImage ? (
            <Image
              src={getImageUrl(item.productImage)}
              alt={item.productName || 'Product'}
              fill
              className="object-cover"
            />
          ) : (
            <div className="w-full h-full bg-gray-200 flex items-center justify-center">
              <span className="text-gray-400">No Image</span>
            </div>
          )}
        </div>

        {/* Product Info */}
        <div className="space-y-3">
          <Link
            href={`/product/${item.productSlug || '#'}`}
            className="text-lg font-semibold text-gray-900 hover:text-blue-600 line-clamp-2"
          >
            {item.productName || 'Unknown Product'}
          </Link>

          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500">Unit Price:</span>
            <span className="font-medium">{formatPrice(item.unitPrice)}</span>
          </div>

          {/* Quantity Controls */}
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500">Quantity:</span>
            <div className="flex items-center border rounded-md">
              <Button
                variant="ghost"
                size="sm"
                onClick={decrementQuantity}
                disabled={isUpdating || localQuantity <= 1}
                className="h-8 w-8 p-0"
              >
                <Minus className="w-3 h-3" />
              </Button>
              <span className="px-3 py-1 text-sm font-medium min-w-[3rem] text-center">
                {isUpdating ? '...' : localQuantity}
              </span>
              <Button
                variant="ghost"
                size="sm"
                onClick={incrementQuantity}
                disabled={isUpdating}
                className="h-8 w-8 p-0"
              >
                <Plus className="w-3 h-3" />
              </Button>
            </div>
          </div>

          {/* Subtotal */}
          <div className="flex items-center justify-between pt-2 border-t">
            <span className="text-lg font-semibold">Subtotal:</span>
            <span className="text-lg font-bold text-blue-600">
              {formatPrice(item.subtotal)}
            </span>
          </div>

          {/* Action Buttons */}
          <div className="flex gap-2 pt-2">
            {showWishlistButton && (
              <Button variant="outline" size="sm" className="flex-1">
                <Heart className="w-4 h-4 mr-2" />
                Save
              </Button>
            )}
            {showRemoveButton && (
              <Button
                variant="outline"
                size="sm"
                onClick={handleRemove}
                disabled={isUpdating}
                className="flex-1 text-red-600 hover:text-red-700 hover:bg-red-50"
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Remove
              </Button>
            )}
          </div>
        </div>

        {error && (
          <div className="mt-2 text-sm text-red-600 bg-red-50 p-2 rounded">
            {error}
          </div>
        )}
      </div>
    );
  }

  // Horizontal layout (default)
  return (
    <div className={`bg-white border border-gray-200 rounded-lg p-4 ${className}`}>
      <div className="flex items-start gap-4">
        {/* Product Image */}
        <div className="relative w-24 h-24 bg-gray-100 rounded-md overflow-hidden flex-shrink-0">
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
            className="text-lg font-semibold text-gray-900 hover:text-blue-600 line-clamp-2"
          >
            {item.productName || 'Unknown Product'}
          </Link>

          <div className="mt-2 space-y-2">
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-500">
                Unit Price: {formatPrice(item.unitPrice)}
              </span>
              {!item.isAvailable && (
                <span className="text-sm text-red-600 font-medium">
                  Out of Stock
                </span>
              )}
            </div>

            {/* Quantity Controls */}
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-500">Quantity:</span>
              <div className="flex items-center border rounded-md">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={decrementQuantity}
                  disabled={isUpdating || localQuantity <= 1}
                  className="h-8 w-8 p-0"
                >
                  <Minus className="w-3 h-3" />
                </Button>
                <span className="px-3 py-1 text-sm font-medium min-w-[3rem] text-center">
                  {isUpdating ? '...' : localQuantity}
                </span>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={incrementQuantity}
                  disabled={isUpdating || !item.isAvailable}
                  className="h-8 w-8 p-0"
                >
                  <Plus className="w-3 h-3" />
                </Button>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex items-center gap-2">
              {showWishlistButton && (
                <Button variant="ghost" size="sm">
                  <Heart className="w-4 h-4 mr-2" />
                  Save for Later
                </Button>
              )}
              {showRemoveButton && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={handleRemove}
                  disabled={isUpdating}
                  className="text-red-600 hover:text-red-700 hover:bg-red-50"
                >
                  <Trash2 className="w-4 h-4 mr-2" />
                  Remove
                </Button>
              )}
            </div>
          </div>
        </div>

        {/* Price */}
        <div className="text-right flex-shrink-0">
          <div className="text-lg font-bold text-blue-600">
            {formatPrice(item.subtotal)}
          </div>
          {item.quantity > 1 && (
            <div className="text-sm text-gray-500">
              {item.quantity} × {formatPrice(item.unitPrice)}
            </div>
          )}
        </div>
      </div>

      {error && (
        <div className="mt-4 text-sm text-red-600 bg-red-50 p-2 rounded">
          {error}
        </div>
      )}
    </div>
  );
}

export default CartItem;
