'use client';

import React, { useState } from 'react';
import Image from 'next/image';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';
import { AddToCartButton } from '@/components/cart/AddToCartButton';
import { QuantitySelector } from '@/components/cart/QuantitySelector';
import { useProductCart } from '@/context/CartContext';

interface ProductDetailProps {
  product: ProductDTO;
  className?: string;
}

const ProductDetail: React.FC<ProductDetailProps> = ({ product, className = "" }) => {
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);
  const [quantity, setQuantity] = useState(1);
  const { isInCart, quantity: cartQuantity, addToCart, isUpdating } = useProductCart(product.id);

  const handleAddToCart = async () => {
    try {
      await addToCart(quantity);
    } catch (error) {
      console.error('Failed to add to cart:', error);
    }
  };

  const handleQuantityChange = (newQuantity: number) => {
    if (newQuantity >= 1 && newQuantity <= product.stockQuantity) {
      setQuantity(newQuantity);
    }
  };

  const discount = product.originalPrice
    ? Math.round(((product.originalPrice - product.price) / product.originalPrice) * 100)
    : 0;

  return (
    <div className={`grid grid-cols-1 lg:grid-cols-2 gap-12 ${className}`}>
      {/* Product Images */}
      <div className="space-y-4">
        {/* Main Image */}
        <div className="relative aspect-square overflow-hidden rounded-lg bg-gray-100">
          <Image
            src={getImageUrl(product.images?.[selectedImageIndex] || '/images/product-1.png')}
            alt={product.name || 'Product'}
            fill
            className="object-cover"
          />
          {discount > 0 && (
            <div className="absolute top-4 left-4 bg-primary text-white text-sm font-medium px-3 py-1 rounded">
              -{discount}%
            </div>
          )}
        </div>

        {/* Thumbnail Images */}
        {product.images && product.images.length > 1 && (
          <div className="grid grid-cols-4 gap-2">
            {product.images.map((image, index) => (
              <button
                key={index}
                onClick={() => setSelectedImageIndex(index)}
                className={`relative aspect-square overflow-hidden rounded border-2 transition-colors ${
                  selectedImageIndex === index ? 'border-primary' : 'border-gray-200'
                }`}
              >
                <Image
                  src={getImageUrl(image)}
                  alt={`${product.name || 'Product'} ${index + 1}`}
                  fill
                  className="object-cover"
                />
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Product Info */}
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">{product.name || 'Product'}</h1>
          <p className="text-gray-600">{product.categoryName || 'Uncategorized'}</p>
        </div>

        {/* Price */}
        <div className="flex items-center space-x-4">
          <span className="text-3xl font-bold text-primary">${product.price.toFixed(2)}</span>
          {product.originalPrice && (
            <span className="text-xl text-gray-500 line-through">
              ${product.originalPrice.toFixed(2)}
            </span>
          )}
        </div>

        {/* Rating */}
        <div className="flex items-center space-x-2">
          <div className="flex items-center">
            {[...Array(5)].map((_, i) => (
              <svg
                key={i}
                className={`w-5 h-5 ${
                  i < Math.floor(product.averageRating)
                    ? 'text-yellow-400'
                    : 'text-gray-300'
                }`}
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
              </svg>
            ))}
          </div>
          <span className="text-gray-600">({product.averageRating.toFixed(1)})</span>
        </div>

        {/* Description */}
        {product.description && (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">Description</h3>
            <p className="text-gray-600 leading-relaxed">{product.description}</p>
          </div>
        )}

        {/* Stock Status */}
        <div className="flex items-center space-x-2">
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
          {isInCart && (
            <span className="text-blue-600 font-medium">
              ({cartQuantity} in cart)
            </span>
          )}
        </div>

        {/* Quantity and Add to Cart */}
        <div className="space-y-4">
          <div className="flex items-center space-x-4">
            <label className="text-gray-700 font-medium">Quantity:</label>
            <QuantitySelector
              value={quantity}
              onChange={handleQuantityChange}
              min={1}
              max={product.stockQuantity}
              disabled={product.stockQuantity === 0 || isUpdating}
              size="md"
              variant="default"
              showInput={false}
            />
          </div>

          <div className="flex space-x-4">
            {!isInCart ? (
              <button
                onClick={handleAddToCart}
                disabled={product.stockQuantity === 0 || isUpdating}
                className="flex-1 bg-primary text-white py-3 px-6 rounded-lg hover:bg-primary-dark transition-colors disabled:bg-gray-300 disabled:cursor-not-allowed flex items-center justify-center"
              >
                {isUpdating ? (
                  <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                ) : null}
                Add to Cart {quantity > 1 && `(${quantity})`}
              </button>
            ) : (
              <AddToCartButton
                productId={product.id}
                productName={product.name || 'Product'}
                variant="default"
                size="lg"
                className="flex-1"
                maxQuantity={product.stockQuantity}
                disabled={product.stockQuantity === 0}
                showQuantity={false}
              />
            )}

            <button className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
            </button>
          </div>
        </div>

        {/* Product Details */}
        <div className="border-t pt-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Product Details</h3>
          <dl className="space-y-2">
            <div className="flex">
              <dt className="w-1/3 text-gray-600">SKU:</dt>
              <dd className="text-gray-900">{product.sku || 'N/A'}</dd>
            </div>
            <div className="flex">
              <dt className="w-1/3 text-gray-600">Category:</dt>
              <dd className="text-gray-900">{product.categoryName || 'Uncategorized'}</dd>
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
};

export default ProductDetail;
