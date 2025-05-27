'use client';

import React from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { CategoryDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface CategoriesProps {
  categories?: CategoryDTO[];
  isLoading?: boolean;
}

// Fallback data for when API is not available
const fallbackCategories = [
  {
    id: 1,
    name: 'Dining',
    imageUrl: '/images/dining-category.png',
    slug: 'dining',
  },
  {
    id: 2,
    name: 'Living',
    imageUrl: '/images/living-category.png',
    slug: 'living',
  },
  {
    id: 3,
    name: 'Bedroom',
    imageUrl: '/images/bedroom-category.png',
    slug: 'bedroom',
  },
];

const Categories: React.FC<CategoriesProps> = ({ categories = [], isLoading = false }) => {
  // Use API categories if available, otherwise fallback to static data
  const displayCategories = categories.length > 0 ? categories.slice(0, 3) : fallbackCategories;

  if (isLoading) {
    return (
      <section className="py-16 bg-light">
        <div className="container-custom">
          <div className="text-center mb-12">
            <div className="h-8 bg-gray-300 rounded w-64 mx-auto mb-2 animate-pulse"></div>
            <div className="h-4 bg-gray-300 rounded w-96 mx-auto animate-pulse"></div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {[1, 2, 3].map((index) => (
              <div key={index} className="group">
                <div className="relative overflow-hidden rounded-lg aspect-square bg-gray-300 animate-pulse"></div>
                <div className="mt-4 h-6 bg-gray-300 rounded w-24 mx-auto animate-pulse"></div>
              </div>
            ))}
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="py-16 bg-light">
      <div className="container-custom">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-dark mb-2">Browse The Range</h2>
          <p className="text-text-secondary">Find furniture for every room in your home</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {displayCategories.map((category) => (
            <Link
              key={category.id}
              href={`/category/${category.slug}`}
              className="group"
            >
              <div className="relative overflow-hidden rounded-lg aspect-square">
                <Image
                  src={category.imageUrl ? getImageUrl(category.imageUrl) : (category as any).image}
                  alt={category.name || 'Category'}
                  fill
                  className="object-cover object-center transition-transform duration-300 group-hover:scale-105"
                />
              </div>
              <h3 className="mt-4 text-xl font-medium text-center text-dark group-hover:text-primary transition-colors">
                {category.name}
              </h3>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
};

export default Categories;
