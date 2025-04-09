'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';

type ProductCardProps = {
  id: string;
  name: string;
  image: string;
  price: number;
  originalPrice: number;
  badge: string;
  releaseDate?: string;
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    minimumFractionDigits: 0,
  }).format(price);
};

const ProductCard = ({ id, name, image, price, originalPrice, badge, releaseDate }: ProductCardProps) => {
  const discount = Math.round(((originalPrice - price) / originalPrice) * 100);

  return (
    <Link 
      href={`/product/${id}`}
      className="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow group"
    >
      <div className="relative p-4">
        {/* Product badge */}
        {badge && (
          <span className="absolute top-2 left-2 z-10 bg-success text-white text-xs font-medium px-2 py-1 rounded">
            {badge}
          </span>
        )}
        
        {/* Discount percentage */}
        {discount > 0 && (
          <span className="absolute top-2 right-2 z-10 bg-accent text-white text-xs font-medium px-2 py-1 rounded">
            -{discount}%
          </span>
        )}

        {/* Product image */}
        <div className="relative h-40 mb-3">
          <Image 
            src={image} 
            alt={name}
            fill
            className="object-contain"
          />
        </div>

        {/* Product name */}
        <h3 className="text-sm font-medium line-clamp-2 mb-2 min-h-[40px] group-hover:text-primary transition-colors">
          {name}
        </h3>

        {/* Product price */}
        <div>
          <p className="text-accent font-bold">
            {formatPrice(price)}
          </p>
          {originalPrice > price && (
            <p className="text-gray-500 text-xs line-through">
              {formatPrice(originalPrice)}
            </p>
          )}
        </div>
        
        {/* Release date */}
        {releaseDate && (
          <p className="text-xs text-gray-500 mt-1">
            Dự kiến: {releaseDate}
          </p>
        )}
      </div>
    </Link>
  );
};

const Preorder = () => {
  const preorderProducts = [
    {
      id: 'preorder-1',
      name: 'MacBook Pro 14-inch M3 Pro',
      image: '/assets/products/preorder-1.png',
      price: 47990000,
      originalPrice: 49990000,
      badge: 'Preorder',
      releaseDate: '20/04/2025',
    },
    {
      id: 'preorder-2',
      name: 'Màn hình Samsung Odyssey OLED G8',
      image: '/assets/products/preorder-2.png',
      price: 22990000,
      originalPrice: 24990000,
      badge: 'Preorder',
      releaseDate: '15/04/2025',
    },
    {
      id: 'preorder-3',
      name: 'iPad Pro M2 12.9-inch WiFi',
      image: '/assets/products/preorder-3.png',
      price: 29990000,
      originalPrice: 31990000,
      badge: 'Preorder',
      releaseDate: '25/04/2025',
    },
    {
      id: 'preorder-4',
      name: 'Surface Laptop Studio 2',
      image: '/assets/products/preorder-4.png',
      price: 53990000,
      originalPrice: 56990000,
      badge: 'Preorder',
      releaseDate: '18/04/2025',
    },
  ];

  return (
    <section className="mb-10">
      <h2 className="text-xl font-bold text-success mb-4">Đặt trước giá hời</h2>
      
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {preorderProducts.map((product) => (
          <ProductCard key={product.id} {...product} />
        ))}
      </div>
    </section>
  );
};

export default Preorder;
