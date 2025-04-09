'use client';

import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';

type ProductCardProps = {
  id: string;
  name: string;
  image: string;
  price: number;
  originalPrice: number;
  badge: string;
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
          <span className="absolute top-2 left-2 z-10 bg-accent text-white text-xs font-medium px-2 py-1 rounded">
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

const CountdownTimer = ({ endTime }: { endTime: Date }) => {
  const [timeLeft, setTimeLeft] = useState({
    hours: 0,
    minutes: 0,
    seconds: 0,
  });

  useEffect(() => {
    const calculateTimeLeft = () => {
      const difference = endTime.getTime() - new Date().getTime();
      
      if (difference <= 0) {
        return { hours: 0, minutes: 0, seconds: 0 };
      }
      
      return {
        hours: Math.floor(difference / (1000 * 60 * 60)),
        minutes: Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60)),
        seconds: Math.floor((difference % (1000 * 60)) / 1000),
      };
    };

    setTimeLeft(calculateTimeLeft());
    
    const timer = setInterval(() => {
      setTimeLeft(calculateTimeLeft());
    }, 1000);

    return () => clearInterval(timer);
  }, [endTime]);

  const padWithZero = (num: number) => {
    return num.toString().padStart(2, '0');
  };

  return (
    <div className="flex items-center space-x-2">
      <div className="flex items-center">
        <div className="bg-accent text-white px-2 py-1 rounded">
          {padWithZero(timeLeft.hours)}
        </div>
        <span className="mx-1 font-bold">:</span>
        <div className="bg-accent text-white px-2 py-1 rounded">
          {padWithZero(timeLeft.minutes)}
        </div>
        <span className="mx-1 font-bold">:</span>
        <div className="bg-accent text-white px-2 py-1 rounded">
          {padWithZero(timeLeft.seconds)}
        </div>
      </div>
    </div>
  );
};

const FlashSale = () => {
  // Set the flash sale end time to be 48 hours from now
  const endTime = new Date();
  endTime.setHours(endTime.getHours() + 48);

  const flashSaleProducts = [
    {
      id: 'flash-1',
      name: 'Laptop MSI Prestige 14 Evo A12M 256VN',
      image: '/assets/products/laptop-1.png',
      price: 15990000,
      originalPrice: 19990000,
      badge: 'Flashsale',
    },
    {
      id: 'flash-2',
      name: 'Màn hình Dell UltraSharp U2722DE 27 inch',
      image: '/assets/products/monitor-1.png',
      price: 9990000,
      originalPrice: 12990000,
      badge: 'Flashsale',
    },
    {
      id: 'flash-3',
      name: 'Bàn phím cơ Keychron K8 Pro',
      image: '/assets/products/keyboard-1.png',
      price: 2390000,
      originalPrice: 2990000,
      badge: 'Flashsale',
    },
    {
      id: 'flash-4',
      name: 'Chuột Logitech MX Master 3S',
      image: '/assets/products/mouse-1.png',
      price: 2190000,
      originalPrice: 2690000,
      badge: 'Flashsale',
    },
    {
      id: 'flash-5',
      name: 'Tai nghe không dây Sony WH-1000XM5',
      image: '/assets/products/headphone-1.png',
      price: 6590000,
      originalPrice: 8990000,
      badge: 'Flashsale',
    },
  ];

  return (
    <section className="mb-10">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-bold text-accent">Flashsale</h2>
        <CountdownTimer endTime={endTime} />
      </div>
      
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        {flashSaleProducts.map((product) => (
          <ProductCard key={product.id} {...product} />
        ))}
      </div>
    </section>
  );
};

export default FlashSale;
