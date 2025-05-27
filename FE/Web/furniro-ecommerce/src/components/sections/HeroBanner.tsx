'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { BannerDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface HeroBannerProps {
  banners?: BannerDTO[];
  isLoading?: boolean;
}

const HeroBanner: React.FC<HeroBannerProps> = ({ banners = [], isLoading = false }) => {
  const [currentBannerIndex, setCurrentBannerIndex] = useState(0);

  // Auto-rotate banners every 5 seconds
  useEffect(() => {
    if (banners.length > 1) {
      const interval = setInterval(() => {
        setCurrentBannerIndex((prev) => (prev + 1) % banners.length);
      }, 5000);
      return () => clearInterval(interval);
    }
  }, [banners.length]);

  const handlePrevBanner = () => {
    setCurrentBannerIndex((prev) => (prev - 1 + banners.length) % banners.length);
  };

  const handleNextBanner = () => {
    setCurrentBannerIndex((prev) => (prev + 1) % banners.length);
  };

  // Use API banner data if available, otherwise fallback to default
  const currentBanner = banners.length > 0 ? banners[currentBannerIndex] : null;
  const backgroundImage = currentBanner?.imageUrl
    ? getImageUrl(currentBanner.imageUrl)
    : '/images/hero-banner.png';

  if (isLoading) {
    return (
      <section className="relative h-[600px] md:h-[700px] overflow-hidden">
        <div className="absolute inset-0 bg-gray-200 animate-pulse"></div>
        <div className="container-custom relative h-full flex items-center">
          <div className="bg-gray-300 animate-pulse p-8 md:p-12 max-w-md rounded">
            <div className="h-4 bg-gray-400 rounded mb-2"></div>
            <div className="h-8 bg-gray-400 rounded mb-4"></div>
            <div className="h-4 bg-gray-400 rounded mb-8"></div>
            <div className="h-10 bg-gray-400 rounded w-32"></div>
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="relative h-[600px] md:h-[700px] overflow-hidden">
      {/* Background Image */}
      <div className="absolute inset-0">
        <Image
          src={backgroundImage}
          alt={currentBanner?.title || "Modern furniture for your home"}
          fill
          priority
          className="object-cover object-center"
        />
        <div className="absolute inset-0 bg-black bg-opacity-30"></div>
      </div>

      {/* Navigation Controls for Multiple Banners */}
      {banners.length > 1 && (
        <>
          <button
            onClick={handlePrevBanner}
            className="absolute left-4 top-1/2 transform -translate-y-1/2 bg-white bg-opacity-20 hover:bg-opacity-30 text-white p-2 rounded-full transition-all z-10"
            aria-label="Previous banner"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button
            onClick={handleNextBanner}
            className="absolute right-4 top-1/2 transform -translate-y-1/2 bg-white bg-opacity-20 hover:bg-opacity-30 text-white p-2 rounded-full transition-all z-10"
            aria-label="Next banner"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </>
      )}

      {/* Banner Indicators */}
      {banners.length > 1 && (
        <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2 flex space-x-2 z-10">
          {banners.map((_, index) => (
            <button
              key={index}
              onClick={() => setCurrentBannerIndex(index)}
              className={`w-3 h-3 rounded-full transition-all ${
                index === currentBannerIndex ? 'bg-white' : 'bg-white bg-opacity-50'
              }`}
              aria-label={`Go to banner ${index + 1}`}
            />
          ))}
        </div>
      )}

      {/* Content */}
      <div className="container-custom relative h-full flex items-center">
        <div className="bg-secondary bg-opacity-90 p-8 md:p-12 max-w-md">
          <p className="text-primary font-medium mb-2">
            {currentBanner ? 'Featured' : 'New Arrival'}
          </p>
          <h1 className="text-4xl md:text-5xl font-bold text-dark mb-4">
            {currentBanner?.title || (
              <>
                Discover Our <br /> New Collection
              </>
            )}
          </h1>
          <p className="text-dark mb-8">
            Find the perfect furniture for your home with our latest collection of modern and stylish pieces.
          </p>
          <Link
            href={currentBanner?.link || "/shop"}
            className="bg-primary text-white py-3 px-8 inline-block hover:bg-opacity-90 transition-all"
          >
            SHOP NOW
          </Link>
        </div>
      </div>
    </section>
  );
};

export default HeroBanner;
