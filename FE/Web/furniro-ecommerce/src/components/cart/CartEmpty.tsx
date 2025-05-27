'use client';

import React from 'react';
import Link from 'next/link';
import { ShoppingCart, ArrowLeft, Heart, Star } from 'lucide-react';
import { Button } from '@/components/ui/Button';

interface CartEmptyProps {
  className?: string;
  showRecommendations?: boolean;
  showRecentlyViewed?: boolean;
}

export function CartEmpty({
  className = '',
  showRecommendations = true,
  showRecentlyViewed = false,
}: CartEmptyProps) {
  // Mock data for recommendations - in real app, this would come from API
  const recommendations = [
    {
      id: 1,
      name: 'Modern Sofa',
      price: 899,
      image: '/images/product-1.png',
      slug: 'modern-sofa',
      rating: 4.5,
    },
    {
      id: 2,
      name: 'Dining Table',
      price: 599,
      image: '/images/product-2.png',
      slug: 'dining-table',
      rating: 4.8,
    },
    {
      id: 3,
      name: 'Office Chair',
      price: 299,
      image: '/images/product-3.png',
      slug: 'office-chair',
      rating: 4.3,
    },
  ];

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  };

  return (
    <div className={`text-center ${className}`}>
      {/* Empty Cart Icon and Message */}
      <div className="max-w-md mx-auto">
        <div className="w-24 h-24 mx-auto mb-6 bg-gray-100 rounded-full flex items-center justify-center">
          <ShoppingCart className="w-12 h-12 text-gray-400" />
        </div>

        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          Your cart is empty
        </h2>

        <p className="text-gray-600 mb-8">
          Looks like you haven't added any items to your cart yet.
          Start shopping to fill it up!
        </p>

        {/* Action Buttons */}
        <div className="space-y-4 mb-12">
          <Link href="/shop">
            <Button size="lg" className="w-full sm:w-auto">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Continue Shopping
            </Button>
          </Link>

          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Link href="/categories">
              <Button variant="outline" size="sm">
                Browse Categories
              </Button>
            </Link>
            <Link href="/products/featured">
              <Button variant="outline" size="sm">
                <Star className="w-4 h-4 mr-2" />
                Featured Products
              </Button>
            </Link>
            <Link href="/wishlist">
              <Button variant="outline" size="sm">
                <Heart className="w-4 h-4 mr-2" />
                View Wishlist
              </Button>
            </Link>
          </div>
        </div>
      </div>

      {/* Product Recommendations */}
      {showRecommendations && (
        <div className="max-w-4xl mx-auto">
          <h3 className="text-xl font-semibold text-gray-900 mb-6">
            You might like these
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {recommendations.map((product) => (
              <div
                key={product.id}
                className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
              >
                <div className="aspect-square bg-gray-100 rounded-md mb-4 overflow-hidden">
                  <img
                    src={product.image}
                    alt={product.name}
                    className="w-full h-full object-cover"
                  />
                </div>

                <h4 className="font-medium text-gray-900 mb-2">
                  {product.name}
                </h4>

                <div className="flex items-center gap-2 mb-3">
                  <div className="flex items-center">
                    {[...Array(5)].map((_, i) => (
                      <Star
                        key={i}
                        className={`w-4 h-4 ${
                          i < Math.floor(product.rating)
                            ? 'text-yellow-400 fill-current'
                            : 'text-gray-300'
                        }`}
                      />
                    ))}
                  </div>
                  <span className="text-sm text-gray-600">
                    ({product.rating})
                  </span>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-lg font-bold text-blue-600">
                    {formatPrice(product.price)}
                  </span>
                  <Link href={`/product/${product.slug}`}>
                    <Button size="sm">
                      View Product
                    </Button>
                  </Link>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recently Viewed */}
      {showRecentlyViewed && (
        <div className="max-w-4xl mx-auto mt-12">
          <h3 className="text-xl font-semibold text-gray-900 mb-6">
            Recently viewed
          </h3>

          <div className="text-gray-500">
            <p>No recently viewed items</p>
          </div>
        </div>
      )}

      {/* Shopping Benefits */}
      <div className="max-w-2xl mx-auto mt-12 pt-8 border-t border-gray-200">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-6 text-center">
          <div>
            <div className="w-12 h-12 mx-auto mb-3 bg-blue-100 rounded-full flex items-center justify-center">
              <ShoppingCart className="w-6 h-6 text-blue-600" />
            </div>
            <h4 className="font-medium text-gray-900 mb-2">Free Shipping</h4>
            <p className="text-sm text-gray-600">
              On orders over $100
            </p>
          </div>

          <div>
            <div className="w-12 h-12 mx-auto mb-3 bg-green-100 rounded-full flex items-center justify-center">
              <Heart className="w-6 h-6 text-green-600" />
            </div>
            <h4 className="font-medium text-gray-900 mb-2">Easy Returns</h4>
            <p className="text-sm text-gray-600">
              30-day return policy
            </p>
          </div>

          <div>
            <div className="w-12 h-12 mx-auto mb-3 bg-purple-100 rounded-full flex items-center justify-center">
              <Star className="w-6 h-6 text-purple-600" />
            </div>
            <h4 className="font-medium text-gray-900 mb-2">Quality Guarantee</h4>
            <p className="text-sm text-gray-600">
              Premium furniture only
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default CartEmpty;
