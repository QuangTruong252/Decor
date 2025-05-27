'use client';

import React from 'react';
import Link from 'next/link';
import { CategoryDTO } from '@/api/types';

interface BreadcrumbItem {
  id: number;
  name: string;
  slug: string;
  href?: string;
}

interface CategoryBreadcrumbProps {
  categories: CategoryDTO[];
  currentCategory?: CategoryDTO;
  homeLabel?: string;
  separator?: React.ReactNode;
  className?: string;
  maxItems?: number;
}

const CategoryBreadcrumb: React.FC<CategoryBreadcrumbProps> = ({
  categories,
  currentCategory,
  homeLabel = "Home",
  separator,
  className = "",
  maxItems = 5
}) => {
  // Build breadcrumb items
  const buildBreadcrumbItems = (): BreadcrumbItem[] => {
    const items: BreadcrumbItem[] = [
      { id: 0, name: homeLabel, slug: '', href: '/' }
    ];

    // Add category hierarchy
    categories.forEach((category) => {
      items.push({
        id: category.id,
        name: category.name,
        slug: category.slug,
        href: `/category/${category.slug}`
      });
    });

    // Add current category if it's different from the last in categories
    if (currentCategory && (!categories.length || categories[categories.length - 1].id !== currentCategory.id)) {
      items.push({
        id: currentCategory.id,
        name: currentCategory.name,
        slug: currentCategory.slug,
        href: `/category/${currentCategory.slug}`
      });
    }

    return items;
  };

  const breadcrumbItems = buildBreadcrumbItems();

  // Truncate items if they exceed maxItems
  const displayItems = breadcrumbItems.length > maxItems 
    ? [
        breadcrumbItems[0], // Home
        { id: -1, name: '...', slug: '', href: undefined }, // Ellipsis
        ...breadcrumbItems.slice(-maxItems + 2) // Last few items
      ]
    : breadcrumbItems;

  const defaultSeparator = (
    <svg className="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
    </svg>
  );

  if (breadcrumbItems.length <= 1) {
    return null; // Don't show breadcrumb if only home
  }

  return (
    <nav className={`flex items-center space-x-2 text-sm ${className}`} aria-label="Breadcrumb">
      <ol className="flex items-center space-x-2">
        {displayItems.map((item, index) => {
          const isLast = index === displayItems.length - 1;
          const isEllipsis = item.name === '...';

          return (
            <li key={item.id} className="flex items-center">
              {index > 0 && (
                <span className="mx-2 flex-shrink-0">
                  {separator || defaultSeparator}
                </span>
              )}
              
              {isEllipsis ? (
                <span className="text-gray-500 px-2">...</span>
              ) : isLast ? (
                <span className="text-gray-900 font-medium" aria-current="page">
                  {item.name}
                </span>
              ) : (
                <Link
                  href={item.href || '/'}
                  className="text-gray-500 hover:text-gray-700 transition-colors"
                >
                  {item.name}
                </Link>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
};

// Structured data for SEO
interface BreadcrumbStructuredDataProps {
  categories: CategoryDTO[];
  currentCategory?: CategoryDTO;
}

export const BreadcrumbStructuredData: React.FC<BreadcrumbStructuredDataProps> = ({
  categories,
  currentCategory
}) => {
  const buildStructuredData = () => {
    const items = [
      {
        "@type": "ListItem",
        "position": 1,
        "name": "Home",
        "item": typeof window !== 'undefined' ? window.location.origin : ''
      }
    ];

    let position = 2;
    
    categories.forEach((category) => {
      items.push({
        "@type": "ListItem",
        "position": position++,
        "name": category.name,
        "item": `${typeof window !== 'undefined' ? window.location.origin : ''}/category/${category.slug}`
      });
    });

    if (currentCategory && (!categories.length || categories[categories.length - 1].id !== currentCategory.id)) {
      items.push({
        "@type": "ListItem",
        "position": position,
        "name": currentCategory.name,
        "item": `${typeof window !== 'undefined' ? window.location.origin : ''}/category/${currentCategory.slug}`
      });
    }

    return {
      "@context": "https://schema.org",
      "@type": "BreadcrumbList",
      "itemListElement": items
    };
  };

  return (
    <script
      type="application/ld+json"
      dangerouslySetInnerHTML={{
        __html: JSON.stringify(buildStructuredData())
      }}
    />
  );
};

export default CategoryBreadcrumb;
