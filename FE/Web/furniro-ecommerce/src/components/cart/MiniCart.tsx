'use client';

import React, { useState, useRef, useEffect } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { ShoppingCart, X, Plus, Minus } from 'lucide-react';
import { useCart } from '@/context/CartContext';
import { Button } from '@/components/ui/Button';
import { RecentlyAddedItems } from './RecentlyAddedItems';
import { getImageUrl } from '@/lib/utils';
import { ProductImagePlaceholder } from '@/components/ui/ProductImagePlaceholder';

interface MiniCartProps {
  className?: string;
}

export function MiniCart({ className = '' }: MiniCartProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { cart, itemCount, subtotal, total, updateItem, removeItem, isLoading } = useCart();

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  };

  const handleUpdateQuantity = async (itemId: number, newQuantity: number) => {
    try {
      await updateItem(itemId, newQuantity);
    } catch (error) {
      console.error('Failed to update quantity:', error);
    }
  };

  const handleRemoveItem = async (itemId: number) => {
    try {
      await removeItem(itemId);
    } catch (error) {
      console.error('Failed to remove item:', error);
    }
  };

  return (
    <div className={`relative ${className}`} ref={dropdownRef}>
      {/* Cart Icon Trigger */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-gray-700 hover:text-gray-900 transition-colors"
      >
        <ShoppingCart className="w-6 h-6" />
        {itemCount > 0 && (
          <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center font-medium">
            {itemCount > 99 ? '99+' : itemCount}
          </span>
        )}
      </button>

      {/* Dropdown */}
      {isOpen && (
        <div className="absolute right-0 top-full mt-2 w-96 bg-white border border-gray-200 rounded-lg shadow-lg z-50">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b border-gray-200">
            <h3 className="text-lg font-semibold">Shopping Cart</h3>
            <button
              onClick={() => setIsOpen(false)}
              className="p-1 text-gray-400 hover:text-gray-600"
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          {/* Cart Items */}
          <div className="max-h-96 overflow-y-auto">
            {!cart || cart.items.length === 0 ? (
              <div className="p-8 text-center">
                <ShoppingCart className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 mb-4">Your cart is empty</p>
                <Link href="/shop">
                  <Button
                    onClick={() => setIsOpen(false)}
                    className="w-full"
                  >
                    Continue Shopping
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="p-4 space-y-4">
                {cart.items.map((item) => (
                  <div key={item.id} className="flex items-center gap-3">
                    {/* Product Image */}
                    <div className="relative w-16 h-16 bg-gray-100 rounded-md overflow-hidden flex-shrink-0">
                      {item.productImage ? (
                        <>
                          <Image
                            src={getImageUrl(item.productImage)}
                            alt={item.productName || 'Product'}
                            fill
                            className="object-cover"
                            onError={(e) => {
                              // Fallback to placeholder on error
                              const target = e.target as HTMLImageElement;
                              target.style.display = 'none';
                              const placeholder = target.parentElement?.querySelector('.image-placeholder');
                              if (placeholder) {
                                placeholder.classList.remove('hidden');
                              }
                            }}
                          />
                          <div className="image-placeholder hidden">
                            <ProductImagePlaceholder size="md" />
                          </div>
                        </>
                      ) : (
                        <ProductImagePlaceholder size="md" />
                      )}
                    </div>

                    {/* Product Info */}
                    <div className="flex-1 min-w-0">
                      <Link
                        href={`/product/${item.productSlug || '#'}`}
                        onClick={() => setIsOpen(false)}
                        className="text-sm font-medium text-gray-900 hover:text-blue-600 line-clamp-2"
                      >
                        {item.productName || 'Unknown Product'}
                      </Link>
                      <p className="text-sm text-gray-500 mt-1">
                        {formatPrice(item.unitPrice)}
                      </p>

                      {/* Quantity Controls */}
                      <div className="flex items-center gap-2 mt-2">
                        <div className="flex items-center border rounded">
                          <button
                            onClick={() => handleUpdateQuantity(item.id, item.quantity - 1)}
                            disabled={isLoading || item.quantity <= 1}
                            className="p-1 hover:bg-gray-100 disabled:opacity-50"
                          >
                            <Minus className="w-3 h-3" />
                          </button>
                          <span className="px-2 py-1 text-sm font-medium min-w-[2rem] text-center">
                            {item.quantity}
                          </span>
                          <button
                            onClick={() => handleUpdateQuantity(item.id, item.quantity + 1)}
                            disabled={isLoading}
                            className="p-1 hover:bg-gray-100 disabled:opacity-50"
                          >
                            <Plus className="w-3 h-3" />
                          </button>
                        </div>
                        <span className="text-sm font-medium">
                          {formatPrice(item.subtotal)}
                        </span>
                      </div>
                    </div>

                    {/* Remove Button */}
                    <button
                      onClick={() => handleRemoveItem(item.id)}
                      disabled={isLoading}
                      className="p-1 text-gray-400 hover:text-red-600 disabled:opacity-50"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Footer */}
          {cart && cart.items && cart.items.length > 0 && (
            <div className="border-t border-gray-200 p-4">
              {/* Subtotal and Total */}
              <div className="space-y-2 mb-4">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Subtotal:</span>
                  <span className="font-medium">
                    {formatPrice(subtotal)}
                  </span>
                </div>
                {cart.tax && cart.tax > 0 && (
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-600">Tax:</span>
                    <span className="font-medium">
                      {formatPrice(cart.tax)}
                    </span>
                  </div>
                )}
                {cart.shipping !== undefined && (
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-600">Shipping:</span>
                    <span className="font-medium">
                      {cart.shipping === 0 ? 'Free' : formatPrice(cart.shipping)}
                    </span>
                  </div>
                )}
                <div className="border-t pt-2">
                  <div className="flex items-center justify-between">
                    <span className="text-lg font-semibold">Total:</span>
                    <span className="text-lg font-bold text-primary">
                      {formatPrice(total)}
                    </span>
                  </div>
                </div>
              </div>

              {/* Recently Added Items */}
              <div className="px-4 pb-4">
                <RecentlyAddedItems
                  maxItems={2}
                  showTimeStamp={false}
                  className="border-0 p-0 bg-transparent"
                />
              </div>

              {/* Action Buttons */}
              <div className="space-y-2">
                <Button
                  variant="outline"
                  className="w-full"
                  onClick={() => {
                    setIsOpen(false);
                    // Use window.location for reliable navigation
                    window.location.href = '/cart';
                  }}
                >
                  View Cart ({itemCount} {itemCount === 1 ? 'item' : 'items'})
                </Button>
                <Button
                  className="w-full bg-primary hover:bg-primary/90"
                  onClick={() => {
                    setIsOpen(false);
                    window.location.href = '/checkout';
                  }}
                  disabled={isLoading}
                >
                  {isLoading ? (
                    <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                  ) : null}
                  Checkout
                </Button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default MiniCart;
