'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { useCart } from '@/context/CartContext';

interface ProductCardProps {
  id: number;
  name: string;
  price: number;
  originalPrice?: number;
  image: string;
  category: string;
  slug: string;
}

const ProductCard: React.FC<ProductCardProps> = ({
  id,
  name,
  price,
  originalPrice,
  image,
  category,
  slug,
}) => {
  const { addToCart } = useCart();
  const discount = originalPrice ? Math.round(((originalPrice - price) / originalPrice) * 100) : 0;

  const handleAddToCart = () => {
    addToCart({
      id,
      name,
      price,
      image,
      slug
    });
  };

  return (
    <div className="group relative">
      {/* Product Image with Overlay */}
      <div className="relative overflow-hidden bg-gray-100 aspect-square rounded-lg">
        <Image
          src={image}
          alt={name}
          fill
          className="object-cover object-center transition-transform duration-300 group-hover:scale-105"
        />

        {/* Discount Badge */}
        {discount > 0 && (
          <div className="absolute top-4 right-4 bg-primary text-white text-xs font-medium px-2 py-1 rounded">
            -{discount}%
          </div>
        )}

        {/* Action Buttons Overlay */}
        <div className="absolute inset-0 bg-black bg-opacity-20 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center">
          <div className="flex space-x-2">
            <button
              className="bg-white p-2 rounded-full hover:bg-primary hover:text-white transition-colors"
              aria-label="Add to cart"
              onClick={handleAddToCart}
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
              </svg>
            </button>
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

      {/* Product Info */}
      <div className="mt-4 text-center">
        <h3 className="text-text-secondary text-sm">{category}</h3>
        <Link href={`/product/${slug}`} className="mt-1 block">
          <h2 className="text-lg font-medium text-dark hover:text-primary transition-colors">{name}</h2>
        </Link>
        <div className="mt-2 flex justify-center items-center">
          <span className="text-primary font-medium">${price.toFixed(2)}</span>
          {originalPrice && (
            <span className="ml-2 text-text-secondary line-through text-sm">${originalPrice.toFixed(2)}</span>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
