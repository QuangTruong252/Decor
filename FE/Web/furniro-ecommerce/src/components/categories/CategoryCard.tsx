'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { CategoryDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface CategoryCardProps {
  category: CategoryDTO;
  showProductCount?: boolean;
  className?: string;
  variant?: 'default' | 'compact' | 'featured';
}

const CategoryCard: React.FC<CategoryCardProps> = ({
  category,
  showProductCount = false,
  className = "",
  variant = 'default'
}) => {
  const getCardClasses = () => {
    const baseClasses = "group relative overflow-hidden rounded-lg transition-all duration-300";

    switch (variant) {
      case 'compact':
        return `${baseClasses} bg-white border border-gray-200 hover:shadow-md`;
      case 'featured':
        return `${baseClasses} bg-gradient-to-br from-primary/10 to-primary/5 border-2 border-primary/20 hover:border-primary/40 hover:shadow-lg`;
      default:
        return `${baseClasses} bg-white shadow-sm hover:shadow-md`;
    }
  };

  const getImageClasses = () => {
    switch (variant) {
      case 'compact':
        return "aspect-square";
      case 'featured':
        return "aspect-[4/3]";
      default:
        return "aspect-[3/2]";
    }
  };

  return (
    <Link href={`/category/${category.slug}`} className={`block ${className}`}>
      <div className={getCardClasses()}>
        {/* Category Image */}
        <div className={`relative ${getImageClasses()} overflow-hidden`}>
          {category.imageUrl ? (
            <Image
              src={getImageUrl(category.imageUrl)}
              alt={category.name}
              fill
              className="object-cover transition-transform duration-300 group-hover:scale-105"
            />
          ) : (
            <div className="w-full h-full bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center">
              <svg className="w-12 h-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
            </div>
          )}

          {/* Overlay */}
          <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-20 transition-all duration-300" />

          {/* Category Badge */}
          {variant === 'featured' && (
            <div className="absolute top-3 left-3 bg-primary text-white text-xs font-medium px-2 py-1 rounded-full">
              Featured
            </div>
          )}
        </div>

        {/* Category Info */}
        <div className={`p-4 ${variant === 'compact' ? 'p-3' : ''}`}>
          <div className="flex items-center justify-between">
            <div className="flex-1 min-w-0">
              <h3 className={`font-medium text-gray-900 group-hover:text-primary transition-colors ${
                variant === 'compact' ? 'text-sm' : 'text-lg'
              }`}>
                {category.name}
              </h3>

              {category.description && variant !== 'compact' && (
                <p className="text-sm text-gray-600 mt-1 line-clamp-2">
                  {category.description}
                </p>
              )}

              {/* Parent Category */}
              {category.parentName && (
                <p className="text-xs text-gray-500 mt-1">
                  in {category.parentName}
                </p>
              )}
            </div>

            {/* Arrow Icon */}
            <div className="ml-3 flex-shrink-0">
              <svg
                className="w-5 h-5 text-gray-400 group-hover:text-primary transition-colors"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </div>
          </div>

          {/* Subcategories Count */}
          {category.subcategories && category.subcategories.length > 0 && (
            <div className="mt-2 flex items-center text-xs text-gray-500">
              <svg className="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
              {category.subcategories.length} subcategories
            </div>
          )}

          {/* Product Count (if provided) */}
          {showProductCount && (
            <div className="mt-2 text-xs text-gray-500">
              View products →
            </div>
          )}
        </div>
      </div>
    </Link>
  );
};

export default CategoryCard;
