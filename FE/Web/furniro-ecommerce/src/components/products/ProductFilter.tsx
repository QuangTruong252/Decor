'use client';

import React, { useState, useEffect } from 'react';
import { CategoryService } from '@/api/services';
import { CategoryDTO, ProductSearchParams } from '@/api/types';

interface ProductFilterProps {
  filters: ProductSearchParams;
  onFiltersChange: (filters: ProductSearchParams) => void;
  className?: string;
  isCollapsible?: boolean;
}

interface PriceRange {
  min: number;
  max: number;
}

const ProductFilter: React.FC<ProductFilterProps> = ({
  filters,
  onFiltersChange,
  className = "",
  isCollapsible = false
}) => {
  const [categories, setCategories] = useState<CategoryDTO[]>([]);
  const [isExpanded, setIsExpanded] = useState(!isCollapsible);
  const [priceRange, setPriceRange] = useState<PriceRange>({
    min: filters.minPrice || 0,
    max: filters.maxPrice || 1000
  });

  useEffect(() => {
    fetchCategories();
  }, []);

  const fetchCategories = async () => {
    try {
      const categoriesData = await CategoryService.getCategories();
      setCategories(categoriesData.items || []);
    } catch (error) {
      console.error('Error fetching categories:', error);
    }
  };

  const handleCategoryChange = (categoryId: number) => {
    onFiltersChange({
      ...filters,
      categoryId: filters.categoryId === categoryId ? undefined : categoryId
    });
  };

  const handlePriceRangeChange = (type: 'min' | 'max', value: number) => {
    const newRange = { ...priceRange, [type]: value };
    setPriceRange(newRange);

    onFiltersChange({
      ...filters,
      minPrice: newRange.min,
      maxPrice: newRange.max
    });
  };

  const handleSortChange = (sortBy: string, sortOrder: 'asc' | 'desc') => {
    onFiltersChange({
      ...filters,
      sortBy: sortBy as any,
      sortOrder
    });
  };

  const handleToggleFilter = (filterType: keyof ProductSearchParams) => {
    onFiltersChange({
      ...filters,
      [filterType]: !filters[filterType]
    });
  };

  const clearFilters = () => {
    setPriceRange({ min: 0, max: 1000 });
    onFiltersChange({
      page: 1,
      limit: filters.limit
    });
  };

  const hasActiveFilters = !!(
    filters.categoryId ||
    filters.minPrice ||
    filters.maxPrice ||
    filters.inStock ||
    filters.featured ||
    filters.query
  );

  return (
    <div className={`bg-white rounded-lg border border-gray-200 ${className}`}>
      {/* Filter Header */}
      <div className="p-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-medium text-gray-900">Filters</h3>
          <div className="flex items-center space-x-2">
            {hasActiveFilters && (
              <button
                onClick={clearFilters}
                className="text-sm text-primary hover:text-primary-dark transition-colors"
              >
                Clear all
              </button>
            )}
            {isCollapsible && (
              <button
                onClick={() => setIsExpanded(!isExpanded)}
                className="p-1 text-gray-400 hover:text-gray-600 transition-colors"
              >
                <svg
                  className={`w-5 h-5 transform transition-transform ${isExpanded ? 'rotate-180' : ''}`}
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Filter Content */}
      {isExpanded && (
        <div className="p-4 space-y-6">
          {/* Sort Options */}
          <div>
            <h4 className="text-sm font-medium text-gray-900 mb-3">Sort by</h4>
            <select
              value={`${filters.sortBy || 'createdAt'}-${filters.sortOrder || 'desc'}`}
              onChange={(e) => {
                const [sortBy, sortOrder] = e.target.value.split('-');
                handleSortChange(sortBy, sortOrder as 'asc' | 'desc');
              }}
              className="w-full p-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-primary focus:border-transparent"
            >
              <option value="createdAt-desc">Newest first</option>
              <option value="createdAt-asc">Oldest first</option>
              <option value="name-asc">Name A-Z</option>
              <option value="name-desc">Name Z-A</option>
              <option value="price-asc">Price: Low to High</option>
              <option value="price-desc">Price: High to Low</option>
              <option value="averageRating-desc">Highest rated</option>
            </select>
          </div>

          {/* Categories */}
          <div>
            <h4 className="text-sm font-medium text-gray-900 mb-3">Categories</h4>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {categories.map((category) => (
                <label key={category.id} className="flex items-center">
                  <input
                    type="checkbox"
                    checked={filters.categoryId === category.id}
                    onChange={() => handleCategoryChange(category.id)}
                    className="rounded border-gray-300 text-primary focus:ring-primary"
                  />
                  <span className="ml-2 text-sm text-gray-700">{category.name}</span>
                </label>
              ))}
            </div>
          </div>

          {/* Price Range */}
          <div>
            <h4 className="text-sm font-medium text-gray-900 mb-3">Price Range</h4>
            <div className="space-y-4">
              {/* Price Input Fields */}
              <div className="grid grid-cols-5 gap-2 items-center">
                <div className="col-span-2">
                  <input
                    type="number"
                    placeholder="0"
                    value={priceRange.min || ''}
                    onChange={(e) => handlePriceRangeChange('min', Number(e.target.value) || 0)}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-2 focus:ring-primary focus:border-primary transition-colors"
                    min="0"
                    max="1000"
                  />
                </div>
                <div className="col-span-1 text-center">
                  <span className="text-gray-500 text-sm font-medium">to</span>
                </div>
                <div className="col-span-2">
                  <input
                    type="number"
                    placeholder="1000"
                    value={priceRange.max || ''}
                    onChange={(e) => handlePriceRangeChange('max', Number(e.target.value) || 1000)}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-2 focus:ring-primary focus:border-primary transition-colors"
                    min="0"
                    max="1000"
                  />
                </div>
              </div>

              {/* Dual Range Slider */}
              <div className="relative px-1 py-2">
                <div className="relative h-2">
                  {/* Track Background */}
                  <div className="absolute w-full h-2 bg-gray-200 rounded-full"></div>

                  {/* Active Range Track */}
                  <div
                    className="absolute h-2 bg-primary rounded-full transition-all duration-200"
                    style={{
                      left: `${(priceRange.min / 1000) * 100}%`,
                      width: `${((priceRange.max - priceRange.min) / 1000) * 100}%`
                    }}
                  ></div>

                  {/* Min Range Input */}
                  <input
                    type="range"
                    min="0"
                    max="1000"
                    step="10"
                    value={priceRange.min}
                    onChange={(e) => {
                      const value = Number(e.target.value);
                      if (value <= priceRange.max - 10) {
                        handlePriceRangeChange('min', value);
                      }
                    }}
                    className="absolute w-full h-2 bg-transparent appearance-none cursor-pointer slider-thumb z-10"
                    style={{ zIndex: priceRange.min > 1000 - 100 ? 5 : 1 }}
                  />

                  {/* Max Range Input */}
                  <input
                    type="range"
                    min="0"
                    max="1000"
                    step="10"
                    value={priceRange.max}
                    onChange={(e) => {
                      const value = Number(e.target.value);
                      if (value >= priceRange.min + 10) {
                        handlePriceRangeChange('max', value);
                      }
                    }}
                    className="absolute w-full h-2 bg-transparent appearance-none cursor-pointer slider-thumb z-10"
                    style={{ zIndex: priceRange.max < 100 ? 5 : 1 }}
                  />
                </div>
              </div>

              {/* Current Range Display */}
              <div className="flex justify-between items-center text-sm">
                <div className="text-gray-600">
                  <span className="font-medium text-primary">${priceRange.min}</span>
                  <span className="mx-2 text-gray-400">-</span>
                  <span className="font-medium text-primary">${priceRange.max}</span>
                </div>
                <button
                  onClick={() => {
                    setPriceRange({ min: 0, max: 1000 });
                    onFiltersChange({ ...filters, minPrice: undefined, maxPrice: undefined });
                  }}
                  className="text-xs text-gray-500 hover:text-primary transition-colors"
                >
                  Reset
                </button>
              </div>

              {/* Price Range Labels */}
              <div className="flex justify-between text-xs text-gray-400 px-1">
                <span>$0</span>
                <span>$1000+</span>
              </div>
            </div>
          </div>

          {/* Quick Filters */}
          <div>
            <h4 className="text-sm font-medium text-gray-900 mb-3">Quick Filters</h4>
            <div className="space-y-2">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={filters.inStock || false}
                  onChange={() => handleToggleFilter('inStock')}
                  className="rounded border-gray-300 text-primary focus:ring-primary"
                />
                <span className="ml-2 text-sm text-gray-700">In Stock Only</span>
              </label>

              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={filters.featured || false}
                  onChange={() => handleToggleFilter('featured')}
                  className="rounded border-gray-300 text-primary focus:ring-primary"
                />
                <span className="ml-2 text-sm text-gray-700">Featured Products</span>
              </label>
            </div>
          </div>

          {/* Active Filters */}
          {hasActiveFilters && (
            <div>
              <h4 className="text-sm font-medium text-gray-900 mb-3">Active Filters</h4>
              <div className="flex flex-wrap gap-2">
                {filters.categoryId && (
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-primary text-white">
                    Category
                    <button
                      onClick={() => handleCategoryChange(filters.categoryId!)}
                      className="ml-1 text-white hover:text-gray-200"
                    >
                      ×
                    </button>
                  </span>
                )}

                {(filters.minPrice || filters.maxPrice) && (
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-primary text-white">
                    Price: ${filters.minPrice || 0} - ${filters.maxPrice || 1000}
                    <button
                      onClick={() => {
                        setPriceRange({ min: 0, max: 1000 });
                        onFiltersChange({ ...filters, minPrice: undefined, maxPrice: undefined });
                      }}
                      className="ml-1 text-white hover:text-gray-200"
                    >
                      ×
                    </button>
                  </span>
                )}

                {filters.inStock && (
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-primary text-white">
                    In Stock
                    <button
                      onClick={() => handleToggleFilter('inStock')}
                      className="ml-1 text-white hover:text-gray-200"
                    >
                      ×
                    </button>
                  </span>
                )}

                {filters.featured && (
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-primary text-white">
                    Featured
                    <button
                      onClick={() => handleToggleFilter('featured')}
                      className="ml-1 text-white hover:text-gray-200"
                    >
                      ×
                    </button>
                  </span>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default ProductFilter;
