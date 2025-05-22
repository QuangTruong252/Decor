import React from 'react';
import MainLayout from '@/components/layout/MainLayout';
import ProductCard from '@/components/products/ProductCard';
import Link from 'next/link';

// Sample data - in a real app, this would come from an API
const products = [
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
  {
    id: 5,
    name: 'Syltherine',
    price: 2500.00,
    originalPrice: 3500.00,
    image: '/images/product-1.png',
    category: 'Stylish cafe chair',
    slug: 'syltherine-2',
  },
  {
    id: 6,
    name: 'Leviosa',
    price: 2500.00,
    image: '/images/product-2.png',
    category: 'Stylish cafe chair',
    slug: 'leviosa-2',
  },
  {
    id: 7,
    name: 'Lolito',
    price: 7000.00,
    originalPrice: 14000.00,
    image: '/images/product-3.png',
    category: 'Luxury big sofa',
    slug: 'lolito-2',
  },
  {
    id: 8,
    name: 'Respira',
    price: 500.00,
    image: '/images/product-4.png',
    category: 'Minimalist fan',
    slug: 'respira-2',
  },
];

// Sample categories
const categories = [
  { id: 1, name: 'Dining', count: 15 },
  { id: 2, name: 'Living', count: 22 },
  { id: 3, name: 'Bedroom', count: 18 },
  { id: 4, name: 'Office', count: 12 },
  { id: 5, name: 'Kitchen', count: 10 },
];

export default function ShopPage() {
  return (
    <MainLayout>
      {/* Shop Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">Shop</h1>
          <div className="flex items-center justify-center mt-4">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">Shop</span>
          </div>
        </div>
      </div>

      {/* Shop Content */}
      <div className="container-custom py-16">
        <div className="flex flex-col lg:flex-row gap-8">
          {/* Sidebar */}
          <div className="w-full lg:w-1/4">
            <div className="border border-border-color p-6 rounded-lg mb-8">
              <h3 className="text-xl font-medium mb-4">Categories</h3>
              <ul className="space-y-3">
                {categories.map((category) => (
                  <li key={category.id}>
                    <Link 
                      href={`/category/${category.name.toLowerCase()}`}
                      className="flex justify-between items-center text-dark hover:text-primary transition-colors"
                    >
                      <span>{category.name}</span>
                      <span className="text-text-secondary">({category.count})</span>
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            <div className="border border-border-color p-6 rounded-lg">
              <h3 className="text-xl font-medium mb-4">Price Range</h3>
              <div className="space-y-4">
                <div className="flex items-center">
                  <input 
                    type="range" 
                    min="0" 
                    max="10000" 
                    className="w-full accent-primary" 
                    defaultValue="5000"
                  />
                </div>
                <div className="flex justify-between">
                  <span>$0</span>
                  <span>$10,000</span>
                </div>
                <button className="w-full bg-primary text-white py-2 rounded hover:bg-opacity-90 transition-all">
                  Filter
                </button>
              </div>
            </div>
          </div>

          {/* Products Grid */}
          <div className="w-full lg:w-3/4">
            {/* Filters and Sorting */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 p-4 bg-light rounded-lg">
              <div className="mb-4 md:mb-0">
                <p className="text-text-secondary">
                  Showing <span className="text-dark font-medium">1-{products.length}</span> of <span className="text-dark font-medium">36</span> results
                </p>
              </div>
              <div className="flex items-center space-x-4">
                <label className="flex items-center space-x-2">
                  <span className="text-dark">Show:</span>
                  <select className="border border-border-color rounded p-2 focus:outline-none focus:border-primary">
                    <option>12</option>
                    <option>24</option>
                    <option>36</option>
                  </select>
                </label>
                <label className="flex items-center space-x-2">
                  <span className="text-dark">Sort by:</span>
                  <select className="border border-border-color rounded p-2 focus:outline-none focus:border-primary">
                    <option>Default</option>
                    <option>Price: Low to High</option>
                    <option>Price: High to Low</option>
                    <option>Newest</option>
                  </select>
                </label>
              </div>
            </div>

            {/* Products */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              {products.map((product) => (
                <ProductCard key={product.id} {...product} />
              ))}
            </div>

            {/* Pagination */}
            <div className="flex justify-center mt-12">
              <div className="flex space-x-2">
                <button className="w-10 h-10 flex items-center justify-center border border-border-color rounded hover:bg-primary hover:text-white transition-colors">
                  1
                </button>
                <button className="w-10 h-10 flex items-center justify-center border border-border-color rounded hover:bg-primary hover:text-white transition-colors">
                  2
                </button>
                <button className="w-10 h-10 flex items-center justify-center border border-border-color rounded hover:bg-primary hover:text-white transition-colors">
                  3
                </button>
                <button className="w-10 h-10 flex items-center justify-center border border-border-color rounded hover:bg-primary hover:text-white transition-colors">
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
