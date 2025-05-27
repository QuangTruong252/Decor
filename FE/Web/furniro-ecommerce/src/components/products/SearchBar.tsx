'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { ProductService } from '@/api/services';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';
import Image from 'next/image';
import Link from 'next/link';

interface SearchBarProps {
  placeholder?: string;
  className?: string;
  showSuggestions?: boolean;
  onSearch?: (query: string) => void;
}

const SearchBar: React.FC<SearchBarProps> = ({
  placeholder = "Search products...",
  className = "",
  showSuggestions = true,
  onSearch
}) => {
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState<ProductDTO[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const router = useRouter();
  const searchRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Debounce search
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (query.trim() && showSuggestions) {
        fetchSuggestions(query.trim());
      } else {
        setSuggestions([]);
        setShowDropdown(false);
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [query, showSuggestions]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setShowDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const fetchSuggestions = async (searchQuery: string) => {
    try {
      setIsLoading(true);
      const results = await ProductService.searchProducts(searchQuery, { pageSize: 5 });
      setSuggestions(results.items);
      setShowDropdown(true);
    } catch (error) {
      console.error('Error fetching search suggestions:', error);
      setSuggestions([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim()) {
      if (onSearch) {
        onSearch(query.trim());
      } else {
        router.push(`/search?q=${encodeURIComponent(query.trim())}`);
      }
      setShowDropdown(false);
      inputRef.current?.blur();
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setQuery(e.target.value);
  };

  const handleSuggestionClick = (product: ProductDTO) => {
    setQuery('');
    setShowDropdown(false);
    router.push(`/product/${product.name || '#'}`);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      setShowDropdown(false);
      inputRef.current?.blur();
    }
  };

  return (
    <div ref={searchRef} className={`relative ${className}`}>
      <form onSubmit={handleSubmit} className="relative">
        <div className="relative">
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={handleInputChange}
            onKeyDown={handleKeyDown}
            onFocus={() => query.trim() && suggestions.length > 0 && setShowDropdown(true)}
            placeholder={placeholder}
            className="w-full pl-4 pr-12 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all"
          />
          <button
            type="submit"
            className="absolute right-2 top-1/2 transform -translate-y-1/2 p-2 text-gray-400 hover:text-primary transition-colors"
            aria-label="Search"
          >
            {isLoading ? (
              <div className="animate-spin w-5 h-5 border-2 border-gray-300 border-t-primary rounded-full"></div>
            ) : (
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            )}
          </button>
        </div>
      </form>

      {/* Search Suggestions Dropdown */}
      {showDropdown && showSuggestions && (
        <div className="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-50 max-h-96 overflow-y-auto">
          {suggestions.length > 0 ? (
            <>
              {suggestions.map((product) => (
                <button
                  key={product.id}
                  onClick={() => handleSuggestionClick(product)}
                  className="w-full flex items-center p-3 hover:bg-gray-50 transition-colors text-left"
                >
                  <div className="relative w-12 h-12 flex-shrink-0 mr-3">
                    <Image
                      src={getImageUrl(product.images?.[0] || '/images/product-1.png')}
                      alt={product.name || 'Product'}
                      fill
                      className="object-cover rounded"
                    />
                  </div>
                  <div className="flex-1 min-w-0">
                    <h4 className="text-sm font-medium text-gray-900 truncate">
                      {product.name || 'Product'}
                    </h4>
                    <p className="text-xs text-gray-500 truncate">
                      {product.categoryName || 'Uncategorized'}
                    </p>
                    <p className="text-sm font-medium text-primary">
                      ${product.price.toFixed(2)}
                    </p>
                  </div>
                </button>
              ))}

              {/* View All Results Link */}
              <div className="border-t border-gray-100">
                <Link
                  href={`/search?q=${encodeURIComponent(query)}`}
                  className="block w-full p-3 text-center text-sm text-primary hover:bg-gray-50 transition-colors"
                  onClick={() => setShowDropdown(false)}
                >
                  View all results for "{query}"
                </Link>
              </div>
            </>
          ) : (
            <div className="p-4 text-center text-gray-500">
              <svg className="w-8 h-8 mx-auto mb-2 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              <p className="text-sm">No products found</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default SearchBar;
