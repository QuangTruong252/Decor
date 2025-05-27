'use client';

import React from 'react';
import CategoryCard from './CategoryCard';
import { CategoryDTO } from '@/api/types';

interface CategoryGridProps {
  categories: CategoryDTO[];
  isLoading?: boolean;
  error?: string | null;
  emptyMessage?: string;
  className?: string;
  variant?: 'default' | 'compact' | 'featured';
  columns?: 2 | 3 | 4 | 6;
  showProductCount?: boolean;
}

const CategoryGrid: React.FC<CategoryGridProps> = ({
  categories,
  isLoading = false,
  error = null,
  emptyMessage = "No categories found",
  className = "",
  variant = 'default',
  columns = 3,
  showProductCount = false
}) => {
  const getGridClasses = () => {
    const baseClasses = "grid gap-6";
    
    switch (columns) {
      case 2:
        return `${baseClasses} grid-cols-1 sm:grid-cols-2`;
      case 3:
        return `${baseClasses} grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`;
      case 4:
        return `${baseClasses} grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4`;
      case 6:
        return `${baseClasses} grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6`;
      default:
        return `${baseClasses} grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`;
    }
  };

  // Loading skeleton
  if (isLoading) {
    return (
      <div className={`${getGridClasses()} ${className}`}>
        {Array.from({ length: columns * 2 }).map((_, index) => (
          <div key={index} className="animate-pulse">
            <div className={`bg-gray-200 rounded-lg mb-4 ${
              variant === 'compact' ? 'aspect-square' : 
              variant === 'featured' ? 'aspect-[4/3]' : 'aspect-[3/2]'
            }`}></div>
            <div className="p-4">
              <div className="h-5 bg-gray-200 rounded mb-2"></div>
              {variant !== 'compact' && (
                <div className="h-4 bg-gray-200 rounded w-3/4"></div>
              )}
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
        <h3 className="text-lg font-medium text-gray-900 mb-2">Error loading categories</h3>
        <p className="text-gray-500 text-center">{error}</p>
      </div>
    );
  }

  // Empty state
  if (!categories || categories.length === 0) {
    return (
      <div className={`flex flex-col items-center justify-center py-12 ${className}`}>
        <div className="text-gray-400 mb-4">
          <svg className="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">No categories found</h3>
        <p className="text-gray-500 text-center">{emptyMessage}</p>
      </div>
    );
  }

  // Categories grid
  return (
    <div className={`${getGridClasses()} ${className}`}>
      {categories.map((category) => (
        <CategoryCard
          key={category.id}
          category={category}
          variant={variant}
          showProductCount={showProductCount}
        />
      ))}
    </div>
  );
};

export default CategoryGrid;
