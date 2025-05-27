'use client';

import React, { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
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
import { CategoryBreadcrumb, CategoryGrid } from '@/components/categories';
import { ProductService, CategoryService } from '@/api/services';
import { ProductDTO, CategoryDTO, ProductSearchParams } from '@/api/types';

export default function CategoryPage() {
  const params = useParams();
  const slug = params.slug as string;

  const [category, setCategory] = useState<CategoryDTO | null>(null);
  const [products, setProducts] = useState<ProductDTO[]>([]);
  const [subcategories, setSubcategories] = useState<CategoryDTO[]>([]);
  const [breadcrumbCategories, setBreadcrumbCategories] = useState<CategoryDTO[]>([]);
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

  useEffect(() => {
    if (slug) {
      fetchCategoryData();
    }
  }, [slug]);

  useEffect(() => {
    if (category) {
      fetchProducts();
    }
  }, [category, filters]);

  const fetchCategoryData = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Get category by slug
      const categoriesResult = await CategoryService.getCategories();
      const foundCategory = categoriesResult.items.find(c => c.slug === slug);

      if (foundCategory) {
        setCategory(foundCategory);

        // Get breadcrumb categories
        const breadcrumbs = await CategoryService.getCategoryBreadcrumbs(foundCategory.id);
        setBreadcrumbCategories(breadcrumbs);

        // Get subcategories
        if (foundCategory.subcategories && foundCategory.subcategories.length > 0) {
          setSubcategories(foundCategory.subcategories);
        }

        // Update filters with category
        setFilters(prev => ({ ...prev, categoryId: foundCategory.id }));
      } else {
        setError('Category not found');
      }
    } catch (err) {
      setError('Failed to load category');
      console.error('Error fetching category:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchProducts = async () => {
    try {
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
      console.error('Error fetching products:', err);
    }
  };

  const handleFiltersChange = (newFilters: ProductSearchParams) => {
    setFilters({ ...newFilters, categoryId: category?.id, page: 1 });
  };

  const handlePageChange = (page: number) => {
    setFilters({ ...filters, page });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleItemsPerPageChange = (limit: number) => {
    setFilters({ ...filters, limit, page: 1 });
  };

  const handleSearch = (query: string) => {
    setFilters({ ...filters, query, page: 1 });
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <div className="animate-pulse">
            <div className="h-8 bg-gray-200 rounded w-1/3 mb-8"></div>
            <div className="h-20 bg-gray-200 rounded mb-8"></div>
            <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
              <div className="h-96 bg-gray-200 rounded"></div>
              <div className="lg:col-span-3">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                  {[...Array(6)].map((_, i) => (
                    <div key={i} className="h-64 bg-gray-200 rounded"></div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (error || !category) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              {error || 'Category not found'}
            </h1>
            <p className="text-gray-600 mb-8">
              The category you're looking for doesn't exist or has been moved.
            </p>
            <a href="/shop" className="text-primary hover:underline">
              Browse all products
            </a>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      {/* Category Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold text-dark mb-4">{category.name}</h1>
            {category.description && (
              <p className="text-lg text-gray-600 max-w-2xl mx-auto">
                {category.description}
              </p>
            )}
          </div>
          <CategoryBreadcrumb
            categories={breadcrumbCategories}
            currentCategory={category}
          />
        </div>
      </div>

      {/* Category Content */}
      <div className="container-custom py-16">
        {/* Subcategories */}
        {subcategories.length > 0 && (
          <div className="mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Subcategories</h2>
            <CategoryGrid
              categories={subcategories}
              variant="compact"
              columns={6}
              className="mb-8"
            />
          </div>
        )}

        {/* Search Bar */}
        <div className="mb-8">
          <SearchBar
            onSearch={handleSearch}
            placeholder={`Search in ${category.name}...`}
            className="max-w-md mx-auto"
          />
        </div>

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
                isLoading={false}
                error={null}
                emptyMessage={`No products found in ${category.name}. Try adjusting your filters.`}
              />
            ) : (
              <ProductList
                products={products}
                isLoading={false}
                error={null}
                emptyMessage={`No products found in ${category.name}. Try adjusting your filters.`}
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
