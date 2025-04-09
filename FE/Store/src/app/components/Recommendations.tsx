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
  badge?: string;
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    minimumFractionDigits: 0,
  }).format(price);
};

const ProductCard = ({ id, name, image, price, originalPrice, badge }: ProductCardProps) => {
  const discount = Math.round(((originalPrice - price) / originalPrice) * 100);

  return (
    <Link 
      href={`/product/${id}`}
      className="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow group"
    >
      <div className="relative p-4">
        {/* Product badge */}
        {badge && (
          <span className="absolute top-2 left-2 z-10 bg-primary text-white text-xs font-medium px-2 py-1 rounded">
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
      </div>
    </Link>
  );
};

const Recommendations = () => {
  const recommendedProducts = [
    {
      id: 'rec-1',
      name: 'Laptop MSI Modern 14 C12M 445VN',
      image: '/assets/products/recommend-1.png',
      price: 12990000,
      originalPrice: 14990000,
      badge: 'Hot',
    },
    {
      id: 'rec-2',
      name: 'Màn hình LG UltraGear 27GR95QE-B 27" OLED',
      image: '/assets/products/recommend-2.png',
      price: 18990000,
      originalPrice: 21990000,
    },
    {
      id: 'rec-3',
      name: 'Chuột Logitech G Pro X Superlight 2',
      image: '/assets/products/recommend-3.png',
      price: 3290000,
      originalPrice: 3690000,
      badge: 'Mới',
    },
    {
      id: 'rec-4',
      name: 'Bàn phím AKKO 5108S Double White',
      image: '/assets/products/recommend-4.png',
      price: 1490000,
      originalPrice: 1690000,
    },
    {
      id: 'rec-5',
      name: 'Laptop ASUS Zenbook 14 OLED UX3405MA',
      image: '/assets/products/recommend-5.png',
      price: 26990000,
      originalPrice: 28990000,
      badge: 'Mới',
    },
    {
      id: 'rec-6',
      name: 'Tai nghe không dây Apple AirPods Pro 2',
      image: '/assets/products/recommend-6.png',
      price: 5990000,
      originalPrice: 6990000,
    },
    {
      id: 'rec-7',
      name: 'Mac mini M2 Pro',
      image: '/assets/products/recommend-7.png',
      price: 29990000,
      originalPrice: 32990000,
    },
    {
      id: 'rec-8',
      name: 'Ghế gaming E-Dra Hercules EGC207',
      image: '/assets/products/recommend-8.png',
      price: 3990000,
      originalPrice: 4590000,
    },
  ];

  return (
    <section className="mb-10">
      <h2 className="text-xl font-bold mb-4">Gợi ý cho bạn</h2>
      
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {recommendedProducts.map((product) => (
          <ProductCard key={product.id} {...product} />
        ))}
      </div>
    </section>
  );
};

export default Recommendations;
