'use client';

import React from 'react';
import ProductCard from './ProductCard';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface ProductGridProps {
  products: ProductDTO[];
  isLoading?: boolean;
  error?: string | null;
  emptyMessage?: string;
  className?: string;
}

const ProductGrid: React.FC<ProductGridProps> = ({
  products,
  isLoading = false,
  error = null,
  emptyMessage = "No products found",
  className = ""
}) => {
  // Loading skeleton
  if (isLoading) {
    return (
      <div className={`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 ${className}`}>
        {Array.from({ length: 8 }).map((_, index) => (
          <div key={index} className="animate-pulse">
            <div className="bg-gray-200 aspect-square rounded-lg mb-4"></div>
            <div className="h-4 bg-gray-200 rounded mb-2"></div>
            <div className="h-6 bg-gray-200 rounded mb-2"></div>
            <div className="h-4 bg-gray-200 rounded w-1/2"></div>
          </div>
        ))}
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className={`flex flex-col items-center justify-center py-12 ${className}`}>
        <div className="text-red-500 mb-4">
          <svg className="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">Error loading products</h3>
        <p className="text-gray-500 text-center">{error}</p>
      </div>
    );
  }

  // Empty state
  if (!products || products.length === 0) {
    return (
      <div className={`flex flex-col items-center justify-center py-12 ${className}`}>
        <div className="text-gray-400 mb-4">
          <svg className="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">No products found</h3>
        <p className="text-gray-500 text-center">{emptyMessage}</p>
      </div>
    );
  }

  // Products grid
  return (
    <div className={`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 ${className}`}>
      {products.map((product) => (
        <ProductCard
          key={product.id}
          id={product.id}
          name={product.name || 'Product'}
          price={product.price}
          originalPrice={product.originalPrice}
          image={getImageUrl(product.images?.[0] || '/images/product-1.png')}
          category={product.categoryName || 'Uncategorized'}
          slug={product.slug || 'product-slug'}
          averageRating={product.averageRating}
          stockQuantity={product.stockQuantity}
        />
      ))}
    </div>
  );
};

export default ProductGrid;
