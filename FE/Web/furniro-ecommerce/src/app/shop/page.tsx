'use client';

import React, { useState, useEffect } from 'react';
import MainLayout from '@/components/layout/MainLayout';
import {
  ProductGrid,
  ProductList,
  ProductFilter,
  Pagination,
  PaginationInfo,
  ItemsPerPage,
  ViewToggle,
  ViewMode
} from '@/components/products';
import { CategoryBreadcrumb } from '@/components/categories';
import { ProductService } from '@/api/services';
import { ProductDTO, ProductSearchParams } from '@/api/types';



export default function ShopPage() {
  const [products, setProducts] = useState<ProductDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<ViewMode>('grid');
  const [filters, setFilters] = useState<ProductSearchParams>({
    page: 1,
    limit: 12,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  });
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);

  // Fetch products
  useEffect(() => {
    fetchProducts();
  }, [filters]);

  const fetchProducts = async () => {
    try {
      setIsLoading(true);
      setError(null);

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

      const productsResult = await ProductService.getProducts(apiParams);
      setProducts(productsResult.items);
      // Get pagination info from the API response
      setTotalItems(productsResult.pagination.totalCount);
      setTotalPages(productsResult.pagination.totalPages);
    } catch (err) {
      setError('Failed to load products');
      console.error('Error fetching products:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleFiltersChange = (newFilters: ProductSearchParams) => {
    setFilters({ ...newFilters, page: 1 }); // Reset to first page when filters change
  };

  const handlePageChange = (page: number) => {
    setFilters({ ...filters, page });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleItemsPerPageChange = (limit: number) => {
    setFilters({ ...filters, limit, page: 1 });
  };

  return (
    <MainLayout>
      {/* Shop Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">Shop</h1>
          <div className="flex items-center justify-center mt-4">
            <CategoryBreadcrumb categories={[]} />
          </div>
        </div>
      </div>

      {/* Shop Content */}
      <div className="container-custom py-16">
        <div className="flex flex-col lg:flex-row gap-8">
          {/* Sidebar - Filters */}
          <div className="w-full lg:w-1/4">
            <ProductFilter
              filters={filters}
              onFiltersChange={handleFiltersChange}
              className="sticky top-4"
            />
          </div>

          {/* Products Area */}
          <div className="w-full lg:w-3/4">
            {/* Toolbar */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 p-4 bg-light rounded-lg">
              <div className="mb-4 md:mb-0">
                <PaginationInfo
                  currentPage={filters.page || 1}
                  totalPages={totalPages}
                  totalItems={totalItems}
                  itemsPerPage={filters.limit || 12}
                />
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

            {/* Products Display */}
            {viewMode === 'grid' ? (
              <ProductGrid
                products={products}
                isLoading={isLoading}
                error={error}
                emptyMessage="No products found. Try adjusting your filters."
              />
            ) : (
              <ProductList
                products={products}
                isLoading={isLoading}
                error={error}
                emptyMessage="No products found. Try adjusting your filters."
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
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
