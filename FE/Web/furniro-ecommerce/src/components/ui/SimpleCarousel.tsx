'use client';

import React, { useState, useEffect, useRef, useCallback } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface SimpleCarouselProps {
  children: React.ReactNode[];
  itemsPerView?: {
    mobile: number;
    tablet: number;
    desktop: number;
  };
  gap?: number;
  autoPlay?: boolean;
  autoPlayInterval?: number;
  showControls?: boolean;
  className?: string;
}

const SimpleCarousel: React.FC<SimpleCarouselProps> = ({
  children,
  itemsPerView = { mobile: 1, tablet: 2, desktop: 4 },
  gap = 16,
  autoPlay = false,
  autoPlayInterval = 5000,
  showControls = true,
  className = ''
}) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [itemsToShow, setItemsToShow] = useState(itemsPerView.desktop);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const autoPlayRef = useRef<NodeJS.Timeout | null>(null);

  // Calculate max index based on items to show
  const maxIndex = Math.max(0, children.length - itemsToShow);

  // Handle responsive items per view
  useEffect(() => {
    const handleResize = () => {
      const width = window.innerWidth;
      if (width < 768) {
        setItemsToShow(itemsPerView.mobile);
      } else if (width < 1024) {
        setItemsToShow(itemsPerView.tablet);
      } else {
        setItemsToShow(itemsPerView.desktop);
      }
    };

    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [itemsPerView]);

  // Auto play functionality
  useEffect(() => {
    if (autoPlay && children.length > itemsToShow) {
      autoPlayRef.current = setInterval(() => {
        setCurrentIndex((prev) => (prev >= maxIndex ? 0 : prev + 1));
      }, autoPlayInterval);

      return () => {
        if (autoPlayRef.current) {
          clearInterval(autoPlayRef.current);
        }
      };
    }
  }, [autoPlay, autoPlayInterval, maxIndex, children.length, itemsToShow]);

  // Navigation functions
  const goToPrevious = useCallback(() => {
    if (isTransitioning) return;
    setIsTransitioning(true);
    setCurrentIndex((prev) => (prev <= 0 ? maxIndex : prev - 1));
    setTimeout(() => setIsTransitioning(false), 300);
  }, [isTransitioning, maxIndex]);

  const goToNext = useCallback(() => {
    if (isTransitioning) return;
    setIsTransitioning(true);
    setCurrentIndex((prev) => (prev >= maxIndex ? 0 : prev + 1));
    setTimeout(() => setIsTransitioning(false), 300);
  }, [isTransitioning, maxIndex]);

  // Pause auto play on hover
  const handleMouseEnter = () => {
    if (autoPlayRef.current) {
      clearInterval(autoPlayRef.current);
    }
  };

  const handleMouseLeave = () => {
    if (autoPlay && children.length > itemsToShow) {
      autoPlayRef.current = setInterval(() => {
        setCurrentIndex((prev) => (prev >= maxIndex ? 0 : prev + 1));
      }, autoPlayInterval);
    }
  };

  // Calculate the width of each item
  const itemWidth = `calc((100% - ${gap * (itemsToShow - 1)}px) / ${itemsToShow})`;
  
  // Calculate the translation distance
  const translateX = currentIndex * (100 / itemsToShow);

  return (
    <div 
      className={`relative ${className}`}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      {/* Carousel Container */}
      <div className="overflow-hidden">
        <div 
          className="flex transition-transform duration-300 ease-in-out"
          style={{
            transform: `translateX(-${translateX}%)`,
            gap: `${gap}px`
          }}
        >
          {children.map((child, index) => (
            <div
              key={index}
              className="flex-shrink-0"
              style={{ width: itemWidth }}
            >
              {child}
            </div>
          ))}
        </div>
      </div>

      {/* Navigation Controls */}
      {showControls && children.length > itemsToShow && (
        <>
          <button
            onClick={goToPrevious}
            disabled={isTransitioning}
            className="absolute left-2 top-1/2 transform -translate-y-1/2 bg-white shadow-lg rounded-full p-2 hover:bg-gray-50 transition-all disabled:opacity-50 z-10"
            aria-label="Previous items"
          >
            <ChevronLeft className="w-6 h-6 text-gray-600" />
          </button>
          <button
            onClick={goToNext}
            disabled={isTransitioning}
            className="absolute right-2 top-1/2 transform -translate-y-1/2 bg-white shadow-lg rounded-full p-2 hover:bg-gray-50 transition-all disabled:opacity-50 z-10"
            aria-label="Next items"
          >
            <ChevronRight className="w-6 h-6 text-gray-600" />
          </button>
        </>
      )}
    </div>
  );
};

export default SimpleCarousel;
