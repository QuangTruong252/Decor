'use client';

import React from 'react';
import Link from 'next/link';
import { ShoppingCart } from 'lucide-react';
import { useCart } from '@/context/CartContext';
import { Button } from '@/components/ui/Button';

interface CartIconProps {
  className?: string;
  showBadge?: boolean;
  showTotal?: boolean;
  variant?: 'icon' | 'button';
  size?: 'sm' | 'default' | 'lg';
}

export function CartIcon({
  className = '',
  showBadge = true,
  showTotal = false,
  variant = 'icon',
  size = 'default',
}: CartIconProps) {
  const { itemCount, total, isLoading } = useCart();

  const iconSizes = {
    sm: 'w-4 h-4',
    default: 'w-5 h-5',
    lg: 'w-6 h-6',
  };

  const badgeSizes = {
    sm: 'w-4 h-4 text-xs',
    default: 'w-5 h-5 text-xs',
    lg: 'w-6 h-6 text-sm',
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  };

  if (variant === 'button') {
    return (
      <Link href="/cart">
        <Button
          variant="outline"
          size={size}
          className={`relative ${className}`}
        >
          <ShoppingCart className={iconSizes[size]} />
          {showBadge && itemCount > 0 && (
            <span className={`absolute -top-2 -right-2 ${badgeSizes[size]} bg-red-500 text-white rounded-full flex items-center justify-center font-medium`}>
              {itemCount > 99 ? '99+' : itemCount}
            </span>
          )}
          {showTotal && (
            <span className="ml-2 font-medium">
              {formatPrice(total)}
            </span>
          )}
        </Button>
      </Link>
    );
  }

  return (
    <Link href="/cart" className={`relative inline-block ${className}`}>
      <div className="relative">
        <ShoppingCart className={`${iconSizes[size]} text-gray-700 hover:text-gray-900 transition-colors`} />

        {showBadge && itemCount > 0 && (
          <span className={`absolute -top-2 -right-2 ${badgeSizes[size]} bg-red-500 text-white rounded-full flex items-center justify-center font-medium min-w-[20px] min-h-[20px]`}>
            {isLoading ? (
              <div className="w-2 h-2 border border-white border-t-transparent rounded-full animate-spin" />
            ) : (
              <span className="text-xs font-bold">
                {itemCount > 99 ? '99+' : itemCount}
              </span>
            )}
          </span>
        )}

        {showTotal && total > 0 && (
          <div className="absolute top-full left-1/2 transform -translate-x-1/2 mt-1 px-2 py-1 bg-gray-900 text-white text-xs rounded whitespace-nowrap">
            {formatPrice(total)}
          </div>
        )}
      </div>
    </Link>
  );
}

export default CartIcon;
