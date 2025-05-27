import React from 'react';
import { ShoppingCart } from 'lucide-react';

interface ProductImagePlaceholderProps {
  className?: string;
  size?: 'sm' | 'md' | 'lg';
}

export function ProductImagePlaceholder({ 
  className = '', 
  size = 'md' 
}: ProductImagePlaceholderProps) {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-6 h-6',
    lg: 'w-8 h-8'
  };

  return (
    <div className={`w-full h-full bg-gray-200 flex items-center justify-center ${className}`}>
      <ShoppingCart className={`${sizeClasses[size]} text-gray-400`} />
    </div>
  );
}

export default ProductImagePlaceholder;
