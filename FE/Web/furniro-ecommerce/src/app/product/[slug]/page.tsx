'use client';

import React, { useState } from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Image from 'next/image';
import Link from 'next/link';
import ProductCard from '@/components/products/ProductCard';
import { useCart } from '@/context/CartContext';

// Sample product data - in a real app, this would come from an API
const product = {
  id: 1,
  name: 'Asgaard Sofa',
  price: 2500.00,
  originalPrice: 3500.00,
  description: 'Setting the bar as one of the loudest speakers in its class, the Kilburn is a compact, stout-hearted hero with a well-balanced audio which boasts a clear midrange and extended highs for a sound.',
  category: 'Sofa',
  sku: 'SS001',
  dimensions: {
    width: '112cm',
    height: '80cm',
    depth: '112cm',
  },
  images: [
    '/images/product-1.png',
    '/images/product-2.png',
    '/images/product-3.png',
    '/images/product-4.png',
  ],
  colors: ['#816DFA', '#000000', '#CDBA7B'],
  inStock: true,
};

// Sample related products
const relatedProducts = [
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
];

export default function ProductPage({ params }: { params: { slug: string } }) {
  const [quantity, setQuantity] = useState(1);
  const [selectedImage, setSelectedImage] = useState(0);
  const [selectedColor, setSelectedColor] = useState(product.colors[0]);
  const { addToCart } = useCart();

  const increaseQuantity = () => {
    setQuantity(quantity + 1);
  };

  const decreaseQuantity = () => {
    if (quantity > 1) {
      setQuantity(quantity - 1);
    }
  };

  const handleAddToCart = () => {
    // In a real app, we would add the selected color and other options
    addToCart({
      id: product.id,
      name: product.name,
      price: product.price,
      image: product.images[0],
      slug: params.slug
    });

    // Optional: Show a confirmation message
    alert('Product added to cart!');
  };

  return (
    <MainLayout>
      {/* Breadcrumb */}
      <div className="bg-secondary py-8">
        <div className="container-custom">
          <div className="flex items-center">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <Link href="/shop" className="text-dark hover:text-primary transition-colors">
              Shop
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">{product.name}</span>
          </div>
        </div>
      </div>

      {/* Product Details */}
      <section className="py-16">
        <div className="container-custom">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
            {/* Product Images */}
            <div>
              <div className="relative aspect-square mb-4 bg-light rounded-lg overflow-hidden">
                <Image
                  src={product.images[selectedImage]}
                  alt={product.name}
                  fill
                  className="object-contain"
                />
              </div>
              <div className="grid grid-cols-4 gap-4">
                {product.images.map((image, index) => (
                  <button
                    key={index}
                    className={`relative aspect-square bg-light rounded-lg overflow-hidden ${
                      selectedImage === index ? 'ring-2 ring-primary' : ''
                    }`}
                    onClick={() => setSelectedImage(index)}
                  >
                    <Image
                      src={image}
                      alt={`${product.name} - Image ${index + 1}`}
                      fill
                      className="object-contain"
                    />
                  </button>
                ))}
              </div>
            </div>

            {/* Product Info */}
            <div>
              <h1 className="text-3xl font-bold text-dark mb-4">{product.name}</h1>
              <p className="text-xl text-primary mb-4">${product.price.toFixed(2)}</p>

              <div className="flex items-center mb-6">
                <div className="flex">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <svg
                      key={star}
                      xmlns="http://www.w3.org/2000/svg"
                      className={`h-5 w-5 ${star <= 4 ? 'text-yellow-400' : 'text-gray-300'}`}
                      viewBox="0 0 20 20"
                      fill="currentColor"
                    >
                      <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                    </svg>
                  ))}
                </div>
                <span className="ml-2 text-text-secondary">5 Customer Reviews</span>
              </div>

              <p className="text-text-secondary mb-8">{product.description}</p>

              <div className="mb-8">
                <h3 className="text-lg font-medium mb-2">Color</h3>
                <div className="flex space-x-3">
                  {product.colors.map((color) => (
                    <button
                      key={color}
                      className={`w-8 h-8 rounded-full ${
                        selectedColor === color ? 'ring-2 ring-offset-2 ring-primary' : ''
                      }`}
                      style={{ backgroundColor: color }}
                      onClick={() => setSelectedColor(color)}
                      aria-label={`Select color ${color}`}
                    />
                  ))}
                </div>
              </div>

              <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4 mb-8">
                <div className="flex border border-border-color rounded-lg">
                  <button
                    className="px-4 py-2 text-dark hover:text-primary transition-colors"
                    onClick={decreaseQuantity}
                    aria-label="Decrease quantity"
                  >
                    -
                  </button>
                  <span className="px-4 py-2 border-x border-border-color">{quantity}</span>
                  <button
                    className="px-4 py-2 text-dark hover:text-primary transition-colors"
                    onClick={increaseQuantity}
                    aria-label="Increase quantity"
                  >
                    +
                  </button>
                </div>

                <button
                  className="btn-primary"
                  onClick={handleAddToCart}
                >
                  Add To Cart
                </button>

                <button className="p-2 text-dark hover:text-primary transition-colors" aria-label="Add to wishlist">
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                  </svg>
                </button>
              </div>

              <div className="border-t border-border-color pt-6 space-y-3">
                <p className="flex">
                  <span className="font-medium w-24">SKU</span>
                  <span className="text-text-secondary">{product.sku}</span>
                </p>
                <p className="flex">
                  <span className="font-medium w-24">Category</span>
                  <span className="text-text-secondary">{product.category}</span>
                </p>
                <p className="flex">
                  <span className="font-medium w-24">Dimensions</span>
                  <span className="text-text-secondary">
                    {product.dimensions.width} x {product.dimensions.height} x {product.dimensions.depth}
                  </span>
                </p>
                <p className="flex">
                  <span className="font-medium w-24">Share</span>
                  <span className="flex space-x-3">
                    <a href="#" className="text-dark hover:text-primary transition-colors">
                      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z"></path>
                      </svg>
                    </a>
                    <a href="#" className="text-dark hover:text-primary transition-colors">
                      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M23 3a10.9 10.9 0 0 1-3.14 1.53 4.48 4.48 0 0 0-7.86 3v1A10.66 10.66 0 0 1 3 4s-4 9 5 13a11.64 11.64 0 0 1-7 2c9 5 20 0 20-11.5a4.5 4.5 0 0 0-.08-.83A7.72 7.72 0 0 0 23 3z"></path>
                      </svg>
                    </a>
                    <a href="#" className="text-dark hover:text-primary transition-colors">
                      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <rect x="2" y="2" width="20" height="20" rx="5" ry="5"></rect>
                        <path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z"></path>
                        <line x1="17.5" y1="6.5" x2="17.51" y2="6.5"></line>
                      </svg>
                    </a>
                  </span>
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Product Description Tabs */}
      <section className="py-16 bg-light">
        <div className="container-custom">
          <div className="flex border-b border-border-color mb-8">
            <button className="px-6 py-3 font-medium text-primary border-b-2 border-primary">
              Description
            </button>
            <button className="px-6 py-3 font-medium text-text-secondary hover:text-dark transition-colors">
              Additional Information
            </button>
            <button className="px-6 py-3 font-medium text-text-secondary hover:text-dark transition-colors">
              Reviews (5)
            </button>
          </div>

          <div className="max-w-3xl">
            <p className="text-text-secondary mb-4">
              Setting the bar as one of the loudest speakers in its class, the Kilburn is a compact, stout-hearted hero with a well-balanced audio which boasts a clear midrange and extended highs for a sound that is both articulate and pronounced. The analogue knobs allow you to fine tune the controls to your personal preferences while the guitar-influenced leather strap enables easy and stylish travel.
            </p>
            <p className="text-text-secondary">
              The Kilburn features a classic design that hearkens back to the golden days of rock'n'roll, and its analogue knobs allow you to fine tune the controls to your personal preferences. The guitar-influenced leather strap enables easy and stylish travel, while the Kilburn's compact size makes it the ideal companion to take with you. Its 20-hour battery life means you can listen to your music all day long without having to worry about recharging.
            </p>
          </div>
        </div>
      </section>

      {/* Related Products */}
      <section className="py-16">
        <div className="container-custom">
          <h2 className="text-3xl font-bold text-dark mb-8 text-center">Related Products</h2>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {relatedProducts.map((product) => (
              <ProductCard key={product.id} {...product} />
            ))}
          </div>
        </div>
      </section>
    </MainLayout>
  );
}
