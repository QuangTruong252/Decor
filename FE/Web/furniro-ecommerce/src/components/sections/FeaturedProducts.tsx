import React from 'react';
import ProductCard from '../products/ProductCard';
import Link from 'next/link';

// Sample data - in a real app, this would come from an API
const featuredProducts = [
  {
    id: 1,
    name: 'Syltherine',
    price: 2500.00,
    originalPrice: 3500.00,
    image: '/images/product-1.png',
    category: 'Stylish cafe chair',
    slug: 'syltherine',
  },
  {
    id: 2,
    name: 'Leviosa',
    price: 2500.00,
    image: '/images/product-2.png',
    category: 'Stylish cafe chair',
    slug: 'leviosa',
  },
  {
    id: 3,
    name: 'Lolito',
    price: 7000.00,
    originalPrice: 14000.00,
    image: '/images/product-3.png',
    category: 'Luxury big sofa',
    slug: 'lolito',
  },
  {
    id: 4,
    name: 'Respira',
    price: 500.00,
    image: '/images/product-4.png',
    category: 'Minimalist fan',
    slug: 'respira',
  },
];

const FeaturedProducts = () => {
  return (
    <section className="py-16 bg-white">
      <div className="container-custom">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-dark mb-2">Our Products</h2>
          <p className="text-text-secondary">Check out our latest products</p>
        </div>
        
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {featuredProducts.map((product) => (
            <ProductCard key={product.id} {...product} />
          ))}
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
