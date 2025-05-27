'use client';

import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import { useCart } from '@/context/CartContext';
import { ProductService } from '@/api/services';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

interface ProductInteractiveClientProps {
  slug: string;
}

export default function ProductInteractiveClient({ slug }: ProductInteractiveClientProps) {
  const [product, setProduct] = useState<ProductDTO | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [selectedImage, setSelectedImage] = useState(0);
  const { addItem } = useCart();

  useEffect(() => {
    fetchProduct();
  }, [slug]);

  const fetchProduct = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // In a real app, you'd have a getProductBySlug method
      const productsResult = await ProductService.getProducts({ searchTerm: slug });
      const foundProduct = productsResult.items.find(p => p.slug === slug);

      if (foundProduct) {
        setProduct(foundProduct);
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



  const handleAddToCart = async () => {
    if (product) {
      try {
        await addItem(product.id, quantity);
        alert('Product added to cart!');
      } catch (error) {
        console.error('Failed to add item to cart:', error);
      }
    }
  };

  const handleQuantityChange = (newQuantity: number) => {
    if (newQuantity >= 1 && newQuantity <= (product?.stockQuantity || 1)) {
      setQuantity(newQuantity);
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 animate-pulse">
        <div>
          <div className="aspect-square bg-gray-200 rounded-lg mb-4"></div>
          <div className="grid grid-cols-4 gap-4">
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
    );
  }

  // Error state
  if (error || !product) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          {error || 'Product not found'}
        </h2>
        <p className="text-gray-600">Please try again or browse our other products.</p>
      </div>
    );
  }

  const discount = product.originalPrice
    ? Math.round(((product.originalPrice - product.price) / product.originalPrice) * 100)
    : 0;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
      {/* Product Images */}
      <div>
        <div className="relative aspect-square mb-4 bg-light rounded-lg overflow-hidden">
          <Image
            src={getImageUrl(product.images?.[selectedImage] || '/images/product-1.png')}
            alt={product.name || 'Product'}
            fill
            className="object-contain"
          />
          {discount > 0 && (
            <div className="absolute top-4 left-4 bg-primary text-white text-sm font-medium px-3 py-1 rounded">
              -{discount}%
            </div>
          )}
        </div>
        {product.images && product.images.length > 1 && (
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
                  src={getImageUrl(image)}
                  alt={`${product.name || 'Product'} - Image ${index + 1}`}
                  fill
                  className="object-contain"
                />
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Product Info */}
      <div>
        <h1 className="text-3xl font-bold text-dark mb-4">{product.name || 'Product'}</h1>
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

        {product.description && (
          <p className="text-text-secondary mb-8">{product.description}</p>
        )}

        {/* Stock Status */}
        <div className="flex items-center space-x-2 mb-6">
          <div className={`w-3 h-3 rounded-full ${
            product.stockQuantity > 10 ? 'bg-green-500' :
            product.stockQuantity > 0 ? 'bg-yellow-500' : 'bg-red-500'
          }`}></div>
          <span className="text-gray-600">
            {product.stockQuantity > 0
              ? `${product.stockQuantity} in stock`
              : 'Out of stock'
            }
          </span>
        </div>

        <div className="space-y-4 mb-8">
          <div className="flex items-center space-x-4">
            <label className="text-gray-700 font-medium">Quantity:</label>
            <div className="flex items-center border border-gray-300 rounded">
              <button
                onClick={() => handleQuantityChange(quantity - 1)}
                disabled={quantity <= 1}
                className="px-3 py-2 text-gray-600 hover:text-gray-800 disabled:opacity-50"
              >
                -
              </button>
              <span className="px-4 py-2 border-x border-gray-300">{quantity}</span>
              <button
                onClick={() => handleQuantityChange(quantity + 1)}
                disabled={quantity >= product.stockQuantity}
                className="px-3 py-2 text-gray-600 hover:text-gray-800 disabled:opacity-50"
              >
                +
              </button>
            </div>
          </div>

          <div className="flex space-x-4">
            <button
              onClick={handleAddToCart}
              disabled={product.stockQuantity === 0}
              className="flex-1 bg-primary text-white py-3 px-6 rounded-lg hover:bg-primary-dark transition-colors disabled:bg-gray-300 disabled:cursor-not-allowed"
            >
              Add to Cart
            </button>
            <button className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
            </button>
          </div>
        </div>

        <div className="border-t pt-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Product Details</h3>
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
            {product.isFeatured && (
              <div className="flex">
                <dt className="w-1/3 text-gray-600">Featured:</dt>
                <dd className="text-primary font-medium">Yes</dd>
              </div>
            )}
          </dl>
        </div>
      </div>
    </div>
  );
}
