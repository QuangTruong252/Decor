import React from 'react';
import Head from 'next/head';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface ProductSEOProps {
  product: ProductDTO;
  baseUrl?: string;
}

const ProductSEO: React.FC<ProductSEOProps> = ({
  product,
  baseUrl = typeof window !== 'undefined' ? window.location.origin : ''
}) => {
  const productUrl = `${baseUrl}/product/${product.slug}`;
  const imageUrl = product.images?.[0] ?
    getImageUrl(product.images[0]) :
    `${baseUrl}/images/product-1.png`;

  // Generate structured data for product
  const structuredData = {
    "@context": "https://schema.org",
    "@type": "Product",
    "name": product.name,
    "description": product.description || `${product.name} - ${product.categoryName}`,
    "image": product.images?.map(img => getImageUrl(img)) || [imageUrl],
    "url": productUrl,
    "sku": product.sku,
    "brand": {
      "@type": "Brand",
      "name": "Furniro"
    },
    "category": product.categoryName,
    "offers": {
      "@type": "Offer",
      "price": product.price.toString(),
      "priceCurrency": "USD",
      "availability": product.stockQuantity > 0 ?
        "https://schema.org/InStock" :
        "https://schema.org/OutOfStock",
      "url": productUrl,
      "seller": {
        "@type": "Organization",
        "name": "Furniro"
      }
    },
    "aggregateRating": product.averageRating > 0 ? {
      "@type": "AggregateRating",
      "ratingValue": product.averageRating.toString(),
      "bestRating": "5",
      "worstRating": "1"
    } : undefined
  };

  // Remove undefined fields
  Object.keys(structuredData).forEach(key => {
    if (structuredData[key as keyof typeof structuredData] === undefined) {
      delete structuredData[key as keyof typeof structuredData];
    }
  });

  const title = `${product.name} | Furniro`;
  const description = product.description ||
    `Shop ${product.name} in ${product.categoryName} category. High quality furniture at affordable prices.`;

  return (
    <Head>
      {/* Basic Meta Tags */}
      <title>{title}</title>
      <meta name="description" content={description} />
      <meta name="keywords" content={`${product.name}, ${product.categoryName}, furniture, home decor, furniro`} />

      {/* Canonical URL */}
      <link rel="canonical" href={productUrl} />

      {/* Open Graph Tags */}
      <meta property="og:type" content="product" />
      <meta property="og:title" content={title} />
      <meta property="og:description" content={description} />
      <meta property="og:url" content={productUrl} />
      <meta property="og:image" content={imageUrl || ''} />
      <meta property="og:image:alt" content={product.name || 'Product'} />
      <meta property="og:site_name" content="Furniro" />

      {/* Product specific Open Graph */}
      <meta property="product:price:amount" content={product.price.toString()} />
      <meta property="product:price:currency" content="USD" />
      <meta property="product:availability" content={product.stockQuantity > 0 ? "in stock" : "out of stock"} />
      <meta property="product:category" content={product.categoryName || 'Uncategorized'} />

      {/* Twitter Card Tags */}
      <meta name="twitter:card" content="summary_large_image" />
      <meta name="twitter:title" content={title} />
      <meta name="twitter:description" content={description} />
      <meta name="twitter:image" content={imageUrl || ''} />
      <meta name="twitter:image:alt" content={product.name || 'Product'} />

      {/* Additional Meta Tags */}
      <meta name="robots" content="index, follow" />
      <meta name="author" content="Furniro" />

      {/* Structured Data */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify(structuredData)
        }}
      />
    </Head>
  );
};

// SEO component for category pages
interface CategorySEOProps {
  categoryName: string;
  categoryDescription?: string;
  categorySlug: string;
  productCount?: number;
  baseUrl?: string;
}

export const CategorySEO: React.FC<CategorySEOProps> = ({
  categoryName,
  categoryDescription,
  categorySlug,
  productCount,
  baseUrl = typeof window !== 'undefined' ? window.location.origin : ''
}) => {
  const categoryUrl = `${baseUrl}/category/${categorySlug}`;
  const title = `${categoryName} | Furniro`;
  const description = categoryDescription ||
    `Shop ${categoryName} furniture collection. ${productCount ? `${productCount} products available.` : ''} High quality furniture at affordable prices.`;

  const structuredData = {
    "@context": "https://schema.org",
    "@type": "CollectionPage",
    "name": categoryName,
    "description": description,
    "url": categoryUrl,
    "mainEntity": {
      "@type": "ItemList",
      "name": `${categoryName} Products`,
      "numberOfItems": productCount
    }
  };

  return (
    <Head>
      <title>{title}</title>
      <meta name="description" content={description} />
      <meta name="keywords" content={`${categoryName}, furniture, home decor, furniro`} />
      <link rel="canonical" href={categoryUrl} />

      <meta property="og:type" content="website" />
      <meta property="og:title" content={title} />
      <meta property="og:description" content={description} />
      <meta property="og:url" content={categoryUrl} />
      <meta property="og:site_name" content="Furniro" />

      <meta name="twitter:card" content="summary" />
      <meta name="twitter:title" content={title} />
      <meta name="twitter:description" content={description} />

      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify(structuredData)
        }}
      />
    </Head>
  );
};

// SEO component for search pages
interface SearchSEOProps {
  query: string;
  resultCount?: number;
  baseUrl?: string;
}

export const SearchSEO: React.FC<SearchSEOProps> = ({
  query,
  resultCount,
  baseUrl = typeof window !== 'undefined' ? window.location.origin : ''
}) => {
  const searchUrl = `${baseUrl}/search?q=${encodeURIComponent(query)}`;
  const title = `Search results for "${query}" | Furniro`;
  const description = `Found ${resultCount || 0} products matching "${query}". Shop furniture and home decor at Furniro.`;

  return (
    <Head>
      <title>{title}</title>
      <meta name="description" content={description} />
      <link rel="canonical" href={searchUrl} />

      <meta property="og:type" content="website" />
      <meta property="og:title" content={title} />
      <meta property="og:description" content={description} />
      <meta property="og:url" content={searchUrl} />
      <meta property="og:site_name" content="Furniro" />

      <meta name="twitter:card" content="summary" />
      <meta name="twitter:title" content={title} />
      <meta name="twitter:description" content={description} />

      <meta name="robots" content="noindex, follow" />
    </Head>
  );
};

export default ProductSEO;
