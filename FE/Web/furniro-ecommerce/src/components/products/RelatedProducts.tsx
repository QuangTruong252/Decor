'use client';

import React, { useState, useEffect } from 'react';
import ProductCard from './ProductCard';
import { ProductService } from '@/api/services';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface RelatedProductsProps {
  currentProduct: ProductDTO;
  maxItems?: number;
  title?: string;
  className?: string;
}

const RelatedProducts: React.FC<RelatedProductsProps> = ({
  currentProduct,
  maxItems = 4,
  title = "Related Products",
  className = ""
}) => {
  const [relatedProducts, setRelatedProducts] = useState<ProductDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchRelatedProducts();
  }, [currentProduct.id]);

  const fetchRelatedProducts = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Strategy 1: Get products from the same category
      let products: ProductDTO[] = [];

      if (currentProduct.categoryId) {
        try {
          const categoryResult = await ProductService.getProductsByCategory(currentProduct.categoryId);
          products = categoryResult?.items || [];
        } catch (categoryError) {
          console.warn('Failed to fetch products by category:', categoryError);
          products = [];
        }
      }

      // Filter out the current product (ensure products is an array)
      products = (products || []).filter(p => p.id !== currentProduct.id);

      // If we don't have enough products from the same category, get featured products
      if (products.length < maxItems) {
        try {
          const featuredResult = await ProductService.getFeaturedProducts();
          // Handle both array response and paginated response
          const featuredProducts = Array.isArray(featuredResult)
            ? featuredResult
            : ((featuredResult as any)?.items || []);

          const additionalProducts = (featuredProducts || [])
            .filter((p: ProductDTO) => p.id !== currentProduct.id && !products.find(existing => existing.id === p.id))
            .slice(0, maxItems - products.length);

          products = [...products, ...additionalProducts];
        } catch (featuredError) {
          console.warn('Failed to fetch featured products:', featuredError);
        }
      }

      // Limit to maxItems and ensure we have valid products
      const validProducts = (products || []).filter(p => p && p.id);
      setRelatedProducts(validProducts.slice(0, maxItems));
    } catch (err) {
      setError('Failed to load related products');
      console.error('Error fetching related products:', err);
      setRelatedProducts([]); // Set empty array on error
    } finally {
      setIsLoading(false);
    }
  };

  if (error) {
    return null; // Don't show the section if there's an error
  }

  if (isLoading) {
    return (
      <div className={`${className}`}>
        <h2 className="text-2xl font-bold text-gray-900 mb-8">{title}</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {Array.from({ length: maxItems }).map((_, index) => (
            <div key={index} className="animate-pulse">
              <div className="bg-gray-200 aspect-square rounded-lg mb-4"></div>
              <div className="h-4 bg-gray-200 rounded mb-2"></div>
              <div className="h-6 bg-gray-200 rounded mb-2"></div>
              <div className="h-4 bg-gray-200 rounded w-1/2"></div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (relatedProducts.length === 0) {
    return null; // Don't show the section if no related products
  }

  return (
    <div className={`${className}`}>
      <div className="flex items-center justify-between mb-8">
        <h2 className="text-2xl font-bold text-gray-900">{title}</h2>
        {relatedProducts.length >= maxItems && (
          <a
            href={`/category/${currentProduct.categoryId ? 'category-slug' : 'all'}`}
            className="text-primary hover:text-primary-dark transition-colors font-medium"
          >
            View All →
          </a>
        )}
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {relatedProducts.map((product) => (
          <ProductCard
            key={product.id}
            id={product.id}
            name={product.name || 'Product'}
            price={product.price}
            originalPrice={product.originalPrice}
            image={getImageUrl(product.images?.[0] || '/images/product-1.png')}
            category={product.categoryName || 'Uncategorized'}
            slug={product.slug || 'product-slug'}
            averageRating={product.averageRating}
            stockQuantity={product.stockQuantity}
          />
        ))}
      </div>
    </div>
  );
};

// Alternative component for showing products you might also like
interface YouMightAlsoLikeProps {
  currentProduct: ProductDTO;
  maxItems?: number;
  className?: string;
}

export const YouMightAlsoLike: React.FC<YouMightAlsoLikeProps> = ({
  currentProduct,
  maxItems = 4,
  className = ""
}) => {
  return (
    <RelatedProducts
      currentProduct={currentProduct}
      maxItems={maxItems}
      title="You might also like"
      className={className}
    />
  );
};

// Component for showing recently viewed products
interface RecentlyViewedProps {
  currentProduct: ProductDTO;
  maxItems?: number;
  className?: string;
}

export const RecentlyViewed: React.FC<RecentlyViewedProps> = ({
  currentProduct,
  maxItems = 4,
  className = ""
}) => {
  const [recentProducts, setRecentProducts] = useState<ProductDTO[]>([]);

  useEffect(() => {
    // Get recently viewed products from localStorage
    const getRecentlyViewed = () => {
      try {
        const recent = localStorage.getItem('recentlyViewed');
        if (recent) {
          const productIds = JSON.parse(recent) as number[];
          // Filter out current product and limit
          return productIds
            .filter(id => id !== currentProduct.id)
            .slice(0, maxItems);
        }
      } catch (error) {
        console.error('Error getting recently viewed products:', error);
      }
      return [];
    };

    const recentIds = getRecentlyViewed();

    // Fetch product details for recent IDs
    const fetchRecentProducts = async () => {
      try {
        const products = await Promise.all(
          recentIds.map(id => ProductService.getProductById(id))
        );
        setRecentProducts(products.filter(Boolean));
      } catch (error) {
        console.error('Error fetching recent products:', error);
      }
    };

    if (recentIds.length > 0) {
      fetchRecentProducts();
    }
  }, [currentProduct.id, maxItems]);

  if (recentProducts.length === 0) {
    return null;
  }

  return (
    <div className={`${className}`}>
      <h2 className="text-2xl font-bold text-gray-900 mb-8">Recently Viewed</h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {recentProducts.map((product) => (
          <ProductCard
            key={product.id}
            id={product.id}
            name={product.name || 'Product'}
            price={product.price}
            originalPrice={product.originalPrice}
            image={getImageUrl(product.images?.[0] || '/images/product-1.png')}
            category={product.categoryName || 'Uncategorized'}
            slug={product.slug || '#'}
            averageRating={product.averageRating}
            stockQuantity={product.stockQuantity}
          />
        ))}
      </div>
    </div>
  );
};

export default RelatedProducts;
