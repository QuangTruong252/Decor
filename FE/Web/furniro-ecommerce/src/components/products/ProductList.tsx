'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { useCart } from '@/context/CartContext';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface ProductListProps {
  products: ProductDTO[];
  isLoading?: boolean;
  error?: string | null;
  emptyMessage?: string;
  className?: string;
}

const ProductListItem: React.FC<{ product: ProductDTO }> = ({ product }) => {
  const { addItem } = useCart();
  const discount = product.originalPrice
    ? Math.round(((product.originalPrice - product.price) / product.originalPrice) * 100)
    : 0;

  const handleAddToCart = async () => {
    try {
      await addItem(product.id, 1);
    } catch (error) {
      console.error('Failed to add item to cart:', error);
    }
  };

  return (
    <div className="flex bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
      {/* Product Image */}
      <div className="relative w-48 h-48 flex-shrink-0">
        <Image
          src={getImageUrl(product.images?.[0] || '/images/product-1.png')}
          alt={product.name || 'Product'}
          fill
          className="object-cover"
        />
        {discount > 0 && (
          <div className="absolute top-2 left-2 bg-primary text-white text-xs font-medium px-2 py-1 rounded">
            -{discount}%
          </div>
        )}
      </div>

      {/* Product Info */}
      <div className="flex-1 p-6 flex flex-col justify-between">
        <div>
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-gray-500 mb-1">{product.categoryName || 'Uncategorized'}</p>
              <Link href={`/product/${product.slug || '#'}`}>
                <h3 className="text-lg font-medium text-gray-900 hover:text-primary transition-colors">
                  {product.name || 'Product'}
                </h3>
              </Link>
            </div>
            <div className="text-right">
              <div className="flex items-center space-x-2">
                <span className="text-lg font-medium text-primary">${product.price.toFixed(2)}</span>
                {product.originalPrice && (
                  <span className="text-sm text-gray-500 line-through">
                    ${product.originalPrice.toFixed(2)}
                  </span>
                )}
              </div>
            </div>
          </div>

          {product.description && (
            <p className="text-gray-600 mt-2 line-clamp-2">{product.description}</p>
          )}

          <div className="flex items-center mt-3 space-x-4">
            {/* Rating */}
            <div className="flex items-center">
              <div className="flex items-center">
                {[...Array(5)].map((_, i) => (
                  <svg
                    key={i}
                    className={`w-4 h-4 ${
                      i < Math.floor(product.averageRating)
                        ? 'text-yellow-400'
                        : 'text-gray-300'
                    }`}
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                  </svg>
                ))}
              </div>
              <span className="text-sm text-gray-500 ml-1">({product.averageRating.toFixed(1)})</span>
            </div>

            {/* Stock Status */}
            <div className="flex items-center">
              <div className={`w-2 h-2 rounded-full mr-2 ${
                product.stockQuantity > 10 ? 'bg-green-500' :
                product.stockQuantity > 0 ? 'bg-yellow-500' : 'bg-red-500'
              }`}></div>
              <span className="text-sm text-gray-500">
                {product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Out of stock'}
              </span>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center justify-between mt-4">
          <div className="flex space-x-2">
            <button
              onClick={handleAddToCart}
              disabled={product.stockQuantity === 0}
              className="bg-primary text-white px-4 py-2 rounded-md hover:bg-primary-dark transition-colors disabled:bg-gray-300 disabled:cursor-not-allowed"
            >
              Add to Cart
            </button>
            <Link
              href={`/product/${product.slug || '#'}`}
              className="border border-gray-300 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-50 transition-colors"
            >
              View Details
            </Link>
          </div>

          <div className="flex space-x-2">
            <button
              className="p-2 text-gray-400 hover:text-red-500 transition-colors"
              aria-label="Add to wishlist"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
            </button>
            <button
              className="p-2 text-gray-400 hover:text-blue-500 transition-colors"
              aria-label="Compare"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

const ProductList: React.FC<ProductListProps> = ({
  products,
  isLoading = false,
  error = null,
  emptyMessage = "No products found",
  className = ""
}) => {
  // Loading skeleton
  if (isLoading) {
    return (
      <div className={`space-y-4 ${className}`}>
        {Array.from({ length: 5 }).map((_, index) => (
          <div key={index} className="flex bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden animate-pulse">
            <div className="w-48 h-48 bg-gray-200"></div>
            <div className="flex-1 p-6">
              <div className="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
              <div className="h-6 bg-gray-200 rounded w-1/2 mb-4"></div>
              <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div className="h-4 bg-gray-200 rounded w-1/2 mb-4"></div>
              <div className="flex justify-between">
                <div className="h-8 bg-gray-200 rounded w-24"></div>
                <div className="h-8 bg-gray-200 rounded w-32"></div>
              </div>
            </div>
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

  // Products list
  return (
    <div className={`space-y-4 ${className}`}>
      {products.map((product) => (
        <ProductListItem key={product.id} product={product} />
      ))}
    </div>
  );
};

export default ProductList;
