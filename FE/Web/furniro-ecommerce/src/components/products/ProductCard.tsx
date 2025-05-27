'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { AddToCartButton } from '@/components/cart/AddToCartButton';
import { getImageUrl } from '@/lib/utils';

interface ProductCardProps {
  id: number;
  name: string;
  price: number;
  originalPrice?: number;
  image: string;
  category: string;
  slug: string;
  averageRating?: number;
  stockQuantity?: number;
}

const ProductCard: React.FC<ProductCardProps> = ({
  id,
  name,
  price,
  originalPrice,
  image,
  category,
  slug,
  averageRating = 0,
  stockQuantity = 0,
}) => {
  const discount = originalPrice ? Math.round(((originalPrice - price) / originalPrice) * 100) : 0;

  return (
    <div className="group relative h-full flex flex-col">
      {/* Product Image with Overlay */}
      <div className="relative overflow-hidden bg-gray-100 aspect-square rounded-lg">
        <Image
          src={getImageUrl(image)}
          alt={name}
          fill
          onError={(e) => {
            console.log('Image error:', e);
          }}
          className="object-cover object-center transition-transform duration-300 group-hover:scale-105"
        />

        {/* Discount Badge */}
        {discount > 0 && (
          <div className="absolute top-4 right-4 bg-primary text-white text-xs font-medium px-2 py-1 rounded">
            -{discount}%
          </div>
        )}

        {/* Stock Status Badge */}
        {stockQuantity === 0 && (
          <div className="absolute top-4 left-4 bg-red-500 text-white text-xs font-medium px-2 py-1 rounded">
            Out of Stock
          </div>
        )}

        {/* Action Buttons Overlay */}
        <div className="absolute inset-0 bg-black bg-opacity-20 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center">
          <div className="flex space-x-2">
            <button
              className="bg-white p-2 rounded-full hover:bg-primary hover:text-white transition-colors"
              aria-label="Add to wishlist"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
            </button>
            <Link
              href={`/product/${slug}`}
              className="bg-white p-2 rounded-full hover:bg-primary hover:text-white transition-colors"
              aria-label="View product"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
            </Link>
          </div>
        </div>
      </div>

      {/* Product Info - Flex grow to fill available space */}
      <div className="mt-4 text-center flex-grow flex flex-col">
        <h3 className="text-text-secondary text-sm">{category}</h3>
        <Link href={`/product/${slug}`} className="mt-1 block">
          <h2 className="text-lg font-medium text-dark hover:text-primary transition-colors line-clamp-2">{name}</h2>
        </Link>

        {/* Rating Display */}
        {averageRating > 0 && (
          <div className="mt-2 flex justify-center items-center">
            <div className="flex items-center">
              {[...Array(5)].map((_, i) => (
                <svg
                  key={i}
                  className={`w-4 h-4 ${
                    i < Math.floor(averageRating) ? 'text-yellow-400' : 'text-gray-300'
                  }`}
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                </svg>
              ))}
              <span className="ml-1 text-sm text-gray-600">({averageRating.toFixed(1)})</span>
            </div>
          </div>
        )}

        <div className="mt-2 flex justify-center items-center">
          <span className="text-primary font-medium">${price.toFixed(2)}</span>
          {originalPrice && (
            <span className="ml-2 text-text-secondary line-through text-sm">${originalPrice.toFixed(2)}</span>
          )}
        </div>

        {/* Stock Quantity Display */}
        {stockQuantity > 0 && stockQuantity <= 10 && (
          <div className="mt-1 text-xs text-orange-600">
            Only {stockQuantity} left in stock
          </div>
        )}

        {/* Add to Cart Button - Always at bottom */}
        <div className="mt-auto pt-3">
          <AddToCartButton
            productId={id}
            productName={name}
            variant="outline"
            size="sm"
            className="w-full"
            maxQuantity={stockQuantity}
            disabled={stockQuantity === 0}
          />
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
