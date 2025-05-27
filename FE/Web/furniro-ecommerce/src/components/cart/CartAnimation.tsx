'use client';

import React, { useEffect, useState } from 'react';
import { ShoppingCart } from 'lucide-react';

interface CartAnimationProps {
  isVisible: boolean;
  onComplete?: () => void;
  productName?: string;
  quantity?: number;
}

export function CartAnimation({
  isVisible,
  onComplete,
  productName = 'Item',
  quantity = 1,
}: CartAnimationProps) {
  const [animationStage, setAnimationStage] = useState<'hidden' | 'slideIn' | 'visible' | 'slideOut'>('hidden');

  useEffect(() => {
    if (isVisible) {
      setAnimationStage('slideIn');
      
      // Show for 2 seconds then slide out
      const timer = setTimeout(() => {
        setAnimationStage('slideOut');
        
        // Complete animation after slide out
        setTimeout(() => {
          setAnimationStage('hidden');
          onComplete?.();
        }, 300);
      }, 2000);

      return () => clearTimeout(timer);
    } else {
      setAnimationStage('hidden');
    }
  }, [isVisible, onComplete]);

  if (animationStage === 'hidden') {
    return null;
  }

  const getAnimationClasses = () => {
    switch (animationStage) {
      case 'slideIn':
        return 'animate-slide-in-right';
      case 'visible':
        return '';
      case 'slideOut':
        return 'animate-slide-out-right';
      default:
        return 'opacity-0';
    }
  };

  return (
    <div className={`fixed top-20 right-4 z-50 ${getAnimationClasses()}`}>
      <div className="bg-white border border-gray-200 rounded-lg shadow-lg p-4 max-w-sm">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
            <ShoppingCart className="w-5 h-5 text-green-600" />
          </div>
          <div className="flex-1">
            <p className="text-sm font-medium text-gray-900">
              Added to cart!
            </p>
            <p className="text-xs text-gray-600">
              {productName} {quantity > 1 && `(${quantity})`}
            </p>
          </div>
          <div className="w-6 h-6 bg-green-500 rounded-full flex items-center justify-center">
            <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          </div>
        </div>
      </div>
    </div>
  );
}

// Add custom animations to your global CSS
export const cartAnimationStyles = `
  @keyframes slide-in-right {
    from {
      transform: translateX(100%);
      opacity: 0;
    }
    to {
      transform: translateX(0);
      opacity: 1;
    }
  }

  @keyframes slide-out-right {
    from {
      transform: translateX(0);
      opacity: 1;
    }
    to {
      transform: translateX(100%);
      opacity: 0;
    }
  }

  .animate-slide-in-right {
    animation: slide-in-right 0.3s ease-out forwards;
  }

  .animate-slide-out-right {
    animation: slide-out-right 0.3s ease-in forwards;
  }
`;

export default CartAnimation;
