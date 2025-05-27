'use client';

import React, { useState, useEffect, Suspense } from 'react';
import { useSearchParams } from 'next/navigation';
import MainLayout from '@/components/layout/MainLayout';
import {
  ProductGrid,
  ProductList,
  ProductFilter,
  SearchBar,
  Pagination,
  PaginationInfo,
  ItemsPerPage,
  ViewToggle,
  ViewMode
} from '@/components/products';
import { ProductService } from '@/api/services';
import { ProductDTO, ProductSearchParams } from '@/api/types';

function SearchPageContent() {
  const searchParams = useSearchParams();
  const query = searchParams.get('q') || '';

  const [products, setProducts] = useState<ProductDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<ViewMode>('grid');
  const [filters, setFilters] = useState<ProductSearchParams>({
    page: 1,
    limit: 12,
    sortBy: 'createdAt',
    sortOrder: 'desc',
    query: query
  });
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);

  useEffect(() => {
    if (query) {
      setFilters(prev => ({ ...prev, query, page: 1 }));
    }
  }, [query]);

  useEffect(() => {
    fetchProducts();
  }, [filters]);

  const fetchProducts = async () => {
    try {
      setIsLoading(true);
      setError(null);

      if (filters.query) {
        // Map frontend parameters to API parameters
        const apiParams = {
          ...filters,
          pageNumber: filters.page,
          pageSize: filters.limit,
          searchTerm: filters.query,
          sortDirection: filters.sortOrder,
          // Remove frontend-specific parameters
          page: undefined,
          limit: undefined,
          query: undefined,
          sortOrder: undefined
        };

        const productsResult = await ProductService.searchProducts(filters.query, apiParams);
        setProducts(productsResult.items);
        // Get pagination info from the API response
        setTotalItems(productsResult.pagination.totalCount);
        setTotalPages(productsResult.pagination.totalPages);
      } else {
        setProducts([]);
        setTotalItems(0);
        setTotalPages(0);
      }
    } catch (err) {
      setError('Failed to search products');
      console.error('Error searching products:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleFiltersChange = (newFilters: ProductSearchParams) => {
    setFilters({ ...newFilters, query: filters.query, page: 1 });
  };

  const handlePageChange = (page: number) => {
    setFilters({ ...filters, page });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleItemsPerPageChange = (limit: number) => {
    setFilters({ ...filters, limit, page: 1 });
  };

  const handleSearch = (newQuery: string) => {
    setFilters({ ...filters, query: newQuery, page: 1 });
    // Update URL
    const url = new URL(window.location.href);
    url.searchParams.set('q', newQuery);
    window.history.pushState({}, '', url.toString());
  };

  return (
    <MainLayout>
      {/* Search Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold text-dark mb-4">Search Results</h1>
            {query && (
              <p className="text-lg text-gray-600">
                Showing results for: <span className="font-medium">"{query}"</span>
              </p>
            )}
          </div>

          {/* Search Bar */}
          <div className="max-w-2xl mx-auto">
            <SearchBar
              onSearch={handleSearch}
              placeholder="Search products..."
              className="w-full"
            />
          </div>
        </div>
      </div>

      {/* Search Content */}
      <div className="container-custom py-16">
        {!query ? (
          // No search query
          <div className="text-center py-12">
            <div className="text-gray-400 mb-4">
              <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-gray-900 mb-4">Start your search</h2>
            <p className="text-gray-600 mb-8">Enter a keyword to find products you're looking for.</p>
          </div>
        ) : (
          <div className="flex flex-col lg:flex-row gap-8">
            {/* Sidebar - Filters */}
            <div className="w-full lg:w-1/4">
              <ProductFilter
                filters={filters}
                onFiltersChange={handleFiltersChange}
                className="sticky top-4"
              />
            </div>

            {/* Results Area */}
            <div className="w-full lg:w-3/4">
              {/* Toolbar */}
              <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 p-4 bg-light rounded-lg">
                <div className="mb-4 md:mb-0">
                  {isLoading ? (
                    <div className="h-5 bg-gray-200 rounded w-48 animate-pulse"></div>
                  ) : (
                    <PaginationInfo
                      currentPage={filters.page || 1}
                      totalPages={totalPages}
                      totalItems={totalItems}
                      itemsPerPage={filters.limit || 12}
                    />
                  )}
                </div>

                <div className="flex items-center space-x-4">
                  <ItemsPerPage
                    itemsPerPage={filters.limit || 12}
                    onItemsPerPageChange={handleItemsPerPageChange}
                  />

                  <ViewToggle
                    currentView={viewMode}
                    onViewChange={setViewMode}
                  />
                </div>
              </div>

              {/* Search Results */}
              {isLoading ? (
                // Loading state
                <div className="space-y-4">
                  {viewMode === 'grid' ? (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                      {[...Array(6)].map((_, i) => (
                        <div key={i} className="animate-pulse">
                          <div className="bg-gray-200 aspect-square rounded-lg mb-4"></div>
                          <div className="h-4 bg-gray-200 rounded mb-2"></div>
                          <div className="h-6 bg-gray-200 rounded mb-2"></div>
                          <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="space-y-4">
                      {[...Array(5)].map((_, i) => (
                        <div key={i} className="flex bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden animate-pulse">
                          <div className="w-48 h-48 bg-gray-200"></div>
                          <div className="flex-1 p-6">
                            <div className="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
                            <div className="h-6 bg-gray-200 rounded w-1/2 mb-4"></div>
                            <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                            <div className="h-4 bg-gray-200 rounded w-1/2 mb-4"></div>
                            <div className="flex justify-between">
                              <div className="h-8 bg-gray-200 rounded w-24"></div>
                              <div className="h-8 bg-gray-200 rounded w-32"></div>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              ) : error ? (
                // Error state
                <div className="text-center py-12">
                  <div className="text-red-500 mb-4">
                    <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <h3 className="text-lg font-medium text-gray-900 mb-2">Search Error</h3>
                  <p className="text-gray-500 mb-4">{error}</p>
                  <button
                    onClick={() => fetchProducts()}
                    className="text-primary hover:underline"
                  >
                    Try again
                  </button>
                </div>
              ) : products.length === 0 ? (
                // No results
                <div className="text-center py-12">
                  <div className="text-gray-400 mb-4">
                    <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                    </svg>
                  </div>
                  <h3 className="text-lg font-medium text-gray-900 mb-2">No products found</h3>
                  <p className="text-gray-500 mb-4">
                    We couldn't find any products matching "{query}". Try different keywords or browse our categories.
                  </p>
                  <div className="flex flex-col sm:flex-row gap-4 justify-center">
                    <button
                      onClick={() => handleSearch('')}
                      className="text-primary hover:underline"
                    >
                      Clear search
                    </button>
                    <a href="/shop" className="text-primary hover:underline">
                      Browse all products
                    </a>
                  </div>
                </div>
              ) : (
                // Results
                <>
                  {viewMode === 'grid' ? (
                    <ProductGrid
                      products={products}
                      isLoading={false}
                      error={null}
                      emptyMessage={`No products found for "${query}".`}
                    />
                  ) : (
                    <ProductList
                      products={products}
                      isLoading={false}
                      error={null}
                      emptyMessage={`No products found for "${query}".`}
                    />
                  )}

                  {/* Pagination */}
                  {totalPages > 1 && (
                    <div className="mt-12 flex flex-col sm:flex-row justify-between items-center gap-4">
                      <PaginationInfo
                        currentPage={filters.page || 1}
                        totalPages={totalPages}
                        totalItems={totalItems}
                        itemsPerPage={filters.limit || 12}
                        className="order-2 sm:order-1"
                      />

                      <Pagination
                        currentPage={filters.page || 1}
                        totalPages={totalPages}
                        onPageChange={handlePageChange}
                        className="order-1 sm:order-2"
                      />
                    </div>
                  )}
                </>
              )}
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}

export default function SearchPage() {
  return (
    <Suspense fallback={
      <MainLayout>
        <div className="bg-secondary py-16">
          <div className="container-custom">
            <div className="text-center mb-8">
              <h1 className="text-4xl font-bold text-dark mb-4">Search Results</h1>
              <div className="h-6 bg-gray-200 rounded w-64 mx-auto animate-pulse"></div>
            </div>
            <div className="max-w-2xl mx-auto">
              <div className="h-12 bg-gray-200 rounded animate-pulse"></div>
            </div>
          </div>
        </div>
        <div className="container-custom py-16">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
            <p className="mt-4 text-gray-600">Loading search results...</p>
          </div>
        </div>
      </MainLayout>
    }>
      <SearchPageContent />
    </Suspense>
  );
}
