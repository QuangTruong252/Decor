'use client';

import React, { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
import MainLayout from '@/components/layout/MainLayout';
import {
  ProductDetail,
  ProductReviews,
  RelatedProducts
} from '@/components/products';
import { CategoryBreadcrumb } from '@/components/categories';
import { ProductSEO } from '@/components/seo';
import { ProductService, CategoryService } from '@/api/services';
import { ProductDTO, CategoryDTO } from '@/api/types';
import useProductStore from '@/store/productStore';
import Link from 'next/link';

export default function ProductPage() {
  const params = useParams();
  const slug = params.slug as string;

  const [product, setProduct] = useState<ProductDTO | null>(null);
  const [categories, setCategories] = useState<CategoryDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'description' | 'additional' | 'reviews'>('description');
  const { addToRecentlyViewed } = useProductStore();

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        setIsLoading(true);
        setError(null);

        // Get product by slug
        const productsResult = await ProductService.getProducts({ searchTerm: slug });
        const foundProduct = productsResult.items.find(p => p.slug === slug);

        if (foundProduct) {
          setProduct(foundProduct);
          addToRecentlyViewed(foundProduct);

          // Get category breadcrumbs
          if (foundProduct.categoryId) {
            const categoryBreadcrumbs = await CategoryService.getCategoryBreadcrumbs(foundProduct.categoryId);
            setCategories(categoryBreadcrumbs);
          }
        } else {
          setError('Product not found');
        }
      } catch (err) {
        setError('Failed to load product');
        console.error('Error fetching product:', err);
      } finally {
        setIsLoading(false);
      }
    };

    if (slug) {
      fetchProduct();
    }
  }, [slug, addToRecentlyViewed]);



  if (isLoading) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <div className="animate-pulse">
            <div className="h-8 bg-gray-200 rounded w-1/3 mb-8"></div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
              <div className="space-y-4">
                <div className="aspect-square bg-gray-200 rounded-lg"></div>
                <div className="grid grid-cols-4 gap-2">
                  {[...Array(4)].map((_, i) => (
                    <div key={i} className="aspect-square bg-gray-200 rounded"></div>
                  ))}
                </div>
              </div>
              <div className="space-y-6">
                <div className="h-8 bg-gray-200 rounded w-3/4"></div>
                <div className="h-6 bg-gray-200 rounded w-1/2"></div>
                <div className="h-20 bg-gray-200 rounded"></div>
                <div className="h-12 bg-gray-200 rounded w-1/3"></div>
              </div>
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (error || !product) {
    return (
      <MainLayout>
        <div className="container-custom py-16">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              {error || 'Product not found'}
            </h1>
            <Link href="/shop" className="text-primary hover:underline">
              Back to Shop
            </Link>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      {/* SEO */}
      <ProductSEO product={product} />

      {/* Breadcrumb */}
      <div className="bg-gray-50 py-4">
        <div className="container-custom">
          <CategoryBreadcrumb
            categories={categories}
            currentCategory={categories[categories.length - 1]}
          />
        </div>
      </div>

      {/* Product Details */}
      <section className="py-16">
        <div className="container-custom">
          <ProductDetail product={product} />
        </div>
      </section>

      {/* Product Description Tabs */}
      <section className="py-16 bg-light">
        <div className="container-custom">
          <div className="flex border-b border-border-color mb-8">
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'description'
                  ? 'text-primary border-b-2 border-primary'
                  : 'text-text-secondary hover:text-dark'
              }`}
              onClick={() => setActiveTab('description')}
            >
              Description
            </button>
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'additional'
                  ? 'text-primary border-b-2 border-primary'
                  : 'text-text-secondary hover:text-dark'
              }`}
              onClick={() => setActiveTab('additional')}
            >
              Additional Information
            </button>
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'reviews'
                  ? 'text-primary border-b-2 border-primary'
                  : 'text-text-secondary hover:text-dark'
              }`}
              onClick={() => setActiveTab('reviews')}
            >
              Reviews
            </button>
          </div>

          <div className="max-w-4xl">
            {activeTab === 'description' && (
              <div>
                {product.description ? (
                  <p className="text-text-secondary leading-relaxed">{product.description}</p>
                ) : (
                  <p className="text-text-secondary">No description available for this product.</p>
                )}
              </div>
            )}

            {activeTab === 'additional' && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div>
                  <h4 className="font-medium text-gray-900 mb-4">Product Information</h4>
                  <dl className="space-y-2">
                    <div className="flex">
                      <dt className="w-1/3 text-gray-600">SKU:</dt>
                      <dd className="text-gray-900">{product.sku}</dd>
                    </div>
                    <div className="flex">
                      <dt className="w-1/3 text-gray-600">Category:</dt>
                      <dd className="text-gray-900">{product.categoryName}</dd>
                    </div>
                    <div className="flex">
                      <dt className="w-1/3 text-gray-600">Stock:</dt>
                      <dd className="text-gray-900">{product.stockQuantity} units</dd>
                    </div>
                    <div className="flex">
                      <dt className="w-1/3 text-gray-600">Status:</dt>
                      <dd className={`${product.isActive ? 'text-green-600' : 'text-red-600'}`}>
                        {product.isActive ? 'Active' : 'Inactive'}
                      </dd>
                    </div>
                  </dl>
                </div>
                <div>
                  <h4 className="font-medium text-gray-900 mb-4">Pricing</h4>
                  <dl className="space-y-2">
                    <div className="flex">
                      <dt className="w-1/3 text-gray-600">Current Price:</dt>
                      <dd className="text-gray-900 font-medium">${product.price.toFixed(2)}</dd>
                    </div>
                    {product.originalPrice && (
                      <div className="flex">
                        <dt className="w-1/3 text-gray-600">Original Price:</dt>
                        <dd className="text-gray-500 line-through">${product.originalPrice.toFixed(2)}</dd>
                      </div>
                    )}
                    <div className="flex">
                      <dt className="w-1/3 text-gray-600">Rating:</dt>
                      <dd className="text-gray-900">{product.averageRating.toFixed(1)}/5.0</dd>
                    </div>
                  </dl>
                </div>
              </div>
            )}

            {activeTab === 'reviews' && (
              <ProductReviews productId={product.id} />
            )}
          </div>
        </div>
      </section>

      {/* Related Products */}
      <section className="py-16">
        <div className="container-custom">
          <RelatedProducts currentProduct={product} />
        </div>
      </section>
    </MainLayout>
  );
}
