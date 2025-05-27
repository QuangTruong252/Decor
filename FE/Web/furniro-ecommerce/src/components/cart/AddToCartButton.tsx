'use client';

import React, { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { useProductCart } from '@/context/CartContext';
import { ShoppingCart, Plus, Minus, Check } from 'lucide-react';
import { toast } from 'react-hot-toast';
import { addToRecentlyAdded } from './RecentlyAddedItems';

interface AddToCartButtonProps {
  productId: number;
  productName?: string;
  className?: string;
  variant?: 'default' | 'outline' | 'ghost';
  size?: 'sm' | 'default' | 'lg';
  showQuantity?: boolean;
  maxQuantity?: number;
  disabled?: boolean;
}

export function AddToCartButton({
  productId,
  productName = 'Product',
  className = '',
  variant = 'default',
  size = 'default',
  showQuantity = false,
  maxQuantity = 99,
  disabled = false,
}: AddToCartButtonProps) {
  const {
    isInCart,
    quantity,
    addToCart,
    updateQuantity,
    removeFromCart,
    isUpdating,
    error,
  } = useProductCart(productId);

  const [localQuantity, setLocalQuantity] = useState(1);
  const [showSuccess, setShowSuccess] = useState(false);

  const handleAddToCart = async () => {
    try {
      await addToCart(localQuantity);
      setShowSuccess(true);
      toast.success(
        `${productName} ${localQuantity > 1 ? `(${localQuantity})` : ''} added to cart!`,
        {
          duration: 3000,
          icon: '🛒',
        }
      );

      // Hide success state after 2 seconds
      setTimeout(() => setShowSuccess(false), 2000);
    } catch (error: any) {
      console.error('Failed to add to cart:', {
        error: error?.message || error,
        productId,
        productName,
        localQuantity,
        stack: error?.stack
      });

      const errorMessage = error?.message || 'Failed to add item to cart';
      toast.error(errorMessage);
    }
  };

  const handleUpdateQuantity = async (newQuantity: number) => {
    if (newQuantity < 1) {
      await handleRemoveFromCart();
      return;
    }

    if (newQuantity > maxQuantity) {
      toast.error(`Maximum quantity is ${maxQuantity}`);
      return;
    }

    try {
      await updateQuantity(newQuantity);
      toast.success('Cart updated!');
    } catch (error: any) {
      console.error('Failed to update quantity:', {
        error: error?.message || error,
        productId,
        newQuantity,
        stack: error?.stack
      });

      const errorMessage = error?.message || 'Failed to update cart';
      toast.error(errorMessage);
    }
  };

  const handleRemoveFromCart = async () => {
    try {
      await removeFromCart();
      toast.success(`${productName} removed from cart`);
    } catch (error: any) {
      console.error('Failed to remove from cart:', {
        error: error?.message || error,
        productId,
        productName,
        stack: error?.stack
      });

      const errorMessage = error?.message || 'Failed to remove item from cart';
      toast.error(errorMessage);
    }
  };

  const incrementLocalQuantity = () => {
    if (localQuantity < maxQuantity) {
      setLocalQuantity(prev => prev + 1);
    }
  };

  const decrementLocalQuantity = () => {
    if (localQuantity > 1) {
      setLocalQuantity(prev => prev - 1);
    }
  };

  const incrementCartQuantity = () => {
    handleUpdateQuantity(quantity + 1);
  };

  const decrementCartQuantity = () => {
    handleUpdateQuantity(quantity - 1);
  };

  // Show success state
  if (showSuccess) {
    return (
      <Button
        variant={variant}
        size={size}
        className={`${className} bg-green-600 hover:bg-green-700`}
        disabled
      >
        <Check className="w-4 h-4 mr-2" />
        Added!
      </Button>
    );
  }

  // If item is not in cart, show add to cart button
  if (!isInCart) {
    return (
      <div className={`flex items-center gap-2 ${className}`}>
        {showQuantity && (
          <div className="flex items-center border rounded-md">
            <Button
              variant="ghost"
              size="sm"
              onClick={decrementLocalQuantity}
              disabled={localQuantity <= 1}
              className="h-8 w-8 p-0"
            >
              <Minus className="w-3 h-3" />
            </Button>
            <span className="px-3 py-1 text-sm font-medium min-w-[2rem] text-center">
              {localQuantity}
            </span>
            <Button
              variant="ghost"
              size="sm"
              onClick={incrementLocalQuantity}
              disabled={localQuantity >= maxQuantity}
              className="h-8 w-8 p-0"
            >
              <Plus className="w-3 h-3" />
            </Button>
          </div>
        )}

        <Button
          variant={variant}
          size={size}
          onClick={handleAddToCart}
          disabled={disabled || isUpdating}
          className="flex-1"
        >
          {isUpdating ? (
            <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
          ) : (
            <ShoppingCart className="w-4 h-4 mr-2" />
          )}
          Add to Cart
          {showQuantity && localQuantity > 1 && (
            <span className="ml-1">({localQuantity})</span>
          )}
        </Button>
      </div>
    );
  }

  // If item is in cart, show quantity controls
  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <div className="flex items-center border rounded-md bg-gray-50">
        <Button
          variant="ghost"
          size="sm"
          onClick={decrementCartQuantity}
          disabled={isUpdating || quantity <= 1}
          className="h-8 w-8 p-0"
        >
          <Minus className="w-3 h-3" />
        </Button>
        <span className="px-3 py-1 text-sm font-medium min-w-[2rem] text-center">
          {isUpdating ? (
            <div className="w-3 h-3 border border-gray-400 border-t-transparent rounded-full animate-spin mx-auto" />
          ) : (
            quantity
          )}
        </span>
        <Button
          variant="ghost"
          size="sm"
          onClick={incrementCartQuantity}
          disabled={isUpdating || quantity >= maxQuantity}
          className="h-8 w-8 p-0"
        >
          <Plus className="w-3 h-3" />
        </Button>
      </div>

      <Button
        variant="outline"
        size="sm"
        onClick={handleRemoveFromCart}
        disabled={isUpdating}
        className="text-red-600 hover:text-red-700 hover:bg-red-50"
      >
        Remove
      </Button>
    </div>
  );
}

export default AddToCartButton;
