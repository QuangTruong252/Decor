'use client';

import React from 'react';
import ProductCard from '../products/ProductCard';
import SimpleCarousel from '../ui/SimpleCarousel';
import Link from 'next/link';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface FeaturedProductsProps {
  products?: ProductDTO[];
  isLoading?: boolean;
}

const fallbackProducts = [
  {
    id: 1,
    name: 'Syltherine',
    price: 2500.00,
    originalPrice: 3500.00,
    images: ['/images/product-1.png'],
    categoryName: 'Stylish cafe chair',
    slug: 'syltherine',
    averageRating: 4.5,
    stockQuantity: 10,
  },
  {
    id: 2,
    name: 'Leviosa',
    price: 2500.00,
    originalPrice: 3500.00,
    images: ['/images/product-2.png'],
    categoryName: 'Stylish cafe chair',
    slug: 'leviosa',
    averageRating: 4.5,
    stockQuantity: 10,
  },
  {
    id: 3,
    name: 'Lolito',
    price: 7000.00,
    originalPrice: 14000.00,
    images: ['/images/product-3.png'],
    categoryName: 'Luxury big sofa',
    slug: 'lolito',
    averageRating: 4.5,
    stockQuantity: 10,
  },
  {
    id: 4,
    name: 'Respira',
    price: 500.00,
    originalPrice: 700.00,
    images: ['/images/product-4.png'],
    categoryName: 'Minimalist fan',
    slug: 'respira',
    averageRating: 4.5,
    stockQuantity: 10,
  },
];


const FeaturedProducts: React.FC<FeaturedProductsProps> = ({ products = [], isLoading = false }) => {
  // Use API products if available, otherwise fallback to static data
  const displayProducts = products.length > 0 ? products : fallbackProducts;

  if (isLoading) {
    return (
      <section className="py-16 bg-white">
        <div className="container-custom">
          <div className="text-center mb-12">
            <div className="h-8 bg-gray-300 rounded w-48 mx-auto mb-2 animate-pulse"></div>
            <div className="h-4 bg-gray-300 rounded w-64 mx-auto animate-pulse"></div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {[1, 2, 3, 4].map((index) => (
              <div key={index} className="group relative">
                <div className="relative overflow-hidden bg-gray-300 aspect-square rounded-lg animate-pulse"></div>
                <div className="mt-4 text-center">
                  <div className="h-4 bg-gray-300 rounded w-24 mx-auto mb-2 animate-pulse"></div>
                  <div className="h-6 bg-gray-300 rounded w-32 mx-auto mb-2 animate-pulse"></div>
                  <div className="h-4 bg-gray-300 rounded w-20 mx-auto mb-3 animate-pulse"></div>
                  <div className="h-8 bg-gray-300 rounded w-full animate-pulse"></div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="py-16 bg-white">
      <div className="container-custom">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-dark mb-2">Our Products</h2>
          <p className="text-text-secondary">Check out our latest products</p>
        </div>

        {/* Carousel for products */}
        <div className="relative px-8">
          <SimpleCarousel
            itemsPerView={{ mobile: 1, tablet: 2, desktop: 4 }}
            gap={16}
            autoPlay={true}
            autoPlayInterval={4000}
            showControls={true}
            className="mb-8"
          >
            {displayProducts.map((product) => (
              <ProductCard
                key={product.id}
                id={product.id}
                name={product.name || 'Product'}
                price={product.price}
                originalPrice={product.originalPrice}
                image={product.images?.[0] ? getImageUrl(product.images[0]) : (product as any).image}
                category={product.categoryName || (product as any).category || 'Uncategorized'}
                slug={product.slug || 'product-slug'}
                averageRating={product.averageRating}
                stockQuantity={product.stockQuantity}
              />
            ))}
          </SimpleCarousel>
        </div>

        <div className="mt-12 text-center">
          <Link
            href="/shop"
            className="btn-outline inline-block"
          >
            Show More
          </Link>
        </div>
      </div>
    </section>
  );
};

export default FeaturedProducts;
