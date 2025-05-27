'use client';

import React, { useState, useEffect, useRef } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { useRouter } from 'next/navigation';
import { useCart } from '@/context/CartContext';
import { useAuth } from '@/context/AuthContext';
import { UserProfileCompact } from '@/components/auth/UserProfile';
import { MiniCart } from '@/components/cart/MiniCart';
import { CartIcon } from '@/components/cart/CartIcon';

import { ProductService } from '@/api/services';
import { ProductDTO } from '@/api/types';
import { getImageUrl } from '@/lib/utils';

const Header = () => {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchSuggestions, setSearchSuggestions] = useState<ProductDTO[]>([]);
  const [isSearchLoading, setIsSearchLoading] = useState(false);
  const [showSearchDropdown, setShowSearchDropdown] = useState(false);

  const { itemCount, total } = useCart();
  const { user, isAuthenticated, logout } = useAuth();
  const router = useRouter();

  const userMenuRef = useRef<HTMLDivElement>(null);
  const searchRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  const toggleSearch = () => {
    setIsSearchOpen(!isSearchOpen);
    if (!isSearchOpen) {
      setTimeout(() => searchInputRef.current?.focus(), 100);
    } else {
      setSearchQuery('');
      setShowSearchDropdown(false);
    }
  };

  // Debounced search for suggestions
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (searchQuery.trim()) {
        fetchSearchSuggestions(searchQuery.trim());
      } else {
        setSearchSuggestions([]);
        setShowSearchDropdown(false);
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchQuery]);

  // Close menus when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(event.target as Node)) {
        setIsUserMenuOpen(false);
      }
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setShowSearchDropdown(false);
        // Close mobile search if clicking outside
        if (window.innerWidth < 768) {
          setIsSearchOpen(false);
          setSearchQuery('');
        }
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const fetchSearchSuggestions = async (query: string) => {
    try {
      setIsSearchLoading(true);
      const results = await ProductService.searchProducts(query, { pageSize: 5 });
      setSearchSuggestions(results.items);
      setShowSearchDropdown(true);
    } catch (error) {
      console.error('Error fetching search suggestions:', error);
      setSearchSuggestions([]);
    } finally {
      setIsSearchLoading(false);
    }
  };

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
      setShowSearchDropdown(false);
      setIsSearchOpen(false);
      searchInputRef.current?.blur();
    }
  };

  const handleSuggestionClick = (product: ProductDTO) => {
    setSearchQuery('');
    setShowSearchDropdown(false);
    setIsSearchOpen(false);
    router.push(`/product/${product.slug || product.id}`);
  };

  const handleSearchKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      setShowSearchDropdown(false);
      setIsSearchOpen(false);
      setSearchQuery('');
      searchInputRef.current?.blur();
    }
  };

  return (
    <header className="bg-white shadow-sm relative">
      <div className="container-custom py-4">
        <div className="flex items-center justify-between">
          {/* Left Side: Logo + Navigation */}
          <div className="flex items-center space-x-8">
            {/* Logo */}
            <Link href="/" className="flex items-center flex-shrink-0">
              <Image
                src="/images/logo.png"
                alt="Furniro Logo"
                width={150}
                height={40}
                className="h-10 w-auto"
              />
            </Link>

            {/* Desktop Navigation */}
            <nav className="hidden md:flex items-center space-x-8">
              <Link href="/" className="text-dark hover:text-primary transition-colors">
                Home
              </Link>
              <Link href="/shop" className="text-dark hover:text-primary transition-colors">
                Shop
              </Link>
              <Link href="/about" className="text-dark hover:text-primary transition-colors">
                About
              </Link>
              <Link href="/contact" className="text-dark hover:text-primary transition-colors">
                Contact
              </Link>
            </nav>
          </div>

          {/* Desktop Search Bar (Center) */}
          <div className="hidden lg:flex flex-1 max-w-xl mx-8" ref={searchRef}>
            <form onSubmit={handleSearchSubmit} className="relative w-full">
              <div className="relative">
                <input
                  ref={searchInputRef}
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onKeyDown={handleSearchKeyDown}
                  onFocus={() => searchQuery.trim() && searchSuggestions.length > 0 && setShowSearchDropdown(true)}
                  placeholder="Search products..."
                  className="w-full pl-4 pr-12 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all"
                />
                <button
                  type="submit"
                  className="absolute right-2 top-1/2 transform -translate-y-1/2 p-2 text-gray-400 hover:text-primary transition-colors"
                  aria-label="Search"
                >
                  {isSearchLoading ? (
                    <div className="w-5 h-5 border-2 border-gray-300 border-t-primary rounded-full animate-spin"></div>
                  ) : (
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                  )}
                </button>
              </div>

              {/* Desktop Search Suggestions Dropdown */}
              {showSearchDropdown && (
                <div className="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-50 max-h-96 overflow-y-auto">
                  {searchSuggestions.length > 0 ? (
                    <>
                      {searchSuggestions.map((product) => (
                        <button
                          key={product.id}
                          onClick={() => handleSuggestionClick(product)}
                          className="w-full flex items-center p-3 hover:bg-gray-50 transition-colors text-left"
                        >
                          <div className="relative w-12 h-12 flex-shrink-0 mr-3">
                            <Image
                              src={getImageUrl(product.images?.[0] || '/images/product-1.png')}
                              alt={product.name || 'Product'}
                              fill
                              className="object-cover rounded"
                            />
                          </div>
                          <div className="flex-1 min-w-0">
                            <h4 className="text-sm font-medium text-gray-900 truncate">
                              {product.name || 'Product'}
                            </h4>
                            <p className="text-sm text-gray-500 truncate">
                              {product.categoryName || 'Uncategorized'}
                            </p>
                            <p className="text-sm font-medium text-primary">
                              ${product.price?.toFixed(2)}
                            </p>
                          </div>
                        </button>
                      ))}

                      {/* View All Results Link */}
                      <div className="border-t border-gray-100">
                        <Link
                          href={`/search?q=${encodeURIComponent(searchQuery)}`}
                          className="block w-full p-3 text-center text-sm text-primary hover:bg-gray-50 transition-colors"
                          onClick={() => setShowSearchDropdown(false)}
                        >
                          View all results for "{searchQuery}"
                        </Link>
                      </div>
                    </>
                  ) : (
                    <div className="p-4 text-center text-gray-500">
                      <svg className="w-8 h-8 mx-auto mb-2 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                      </svg>
                      <p className="text-sm">No products found</p>
                    </div>
                  )}
                </div>
              )}
            </form>
          </div>

          {/* Right Side: Icons in order - Search, Wishlist, Cart, User */}
          <div className="flex items-center space-x-4">
            {/* Tablet/Mobile Search Button */}
            <button
              onClick={toggleSearch}
              aria-label="Search"
              className="lg:hidden p-2 hover:text-primary transition-colors"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </button>

            {/* Wishlist Icon */}
            <button aria-label="Wishlist" className="hidden md:block p-2 hover:text-primary transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
            </button>

            {/* Cart Icon */}
            <div className="hidden md:block">
              <div className="hidden sm:block">
                <MiniCart />
              </div>
              <div className="sm:hidden">
                <CartIcon showBadge={true} variant="icon" size="default" />
              </div>
            </div>

            {/* User Authentication Section */}
            <div className="hidden md:block">
              {isAuthenticated ? (
                <div className="relative" ref={userMenuRef}>
                  {/* User Icon Button */}
                  <button
                    onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                    className="p-2 hover:text-primary transition-colors"
                    aria-label="User menu"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </button>

                  {/* User Dropdown Overlay */}
                  {isUserMenuOpen && (
                    <div className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-lg py-4 z-50 border">
                      {/* User Info Header */}
                      <div className="px-4 pb-3 border-b border-gray-100">
                        <div className="flex items-center space-x-3">
                          <div className="w-10 h-10 bg-primary rounded-full flex items-center justify-center">
                            <span className="text-white font-medium text-sm">
                              {user?.username?.charAt(0).toUpperCase() || 'U'}
                            </span>
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="text-sm font-medium text-gray-900 truncate">
                              {user?.username || 'User'}
                            </p>
                            <p className="text-xs text-gray-500 truncate">
                              {user?.email || 'user@example.com'}
                            </p>
                          </div>
                        </div>
                      </div>

                      {/* Navigation Menu */}
                      <div className="py-2">
                        <Link
                          href="/profile"
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors"
                          onClick={() => setIsUserMenuOpen(false)}
                        >
                          <svg className="w-4 h-4 mr-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                          </svg>
                          My Profile
                        </Link>
                        <Link
                          href="/orders"
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors"
                          onClick={() => setIsUserMenuOpen(false)}
                        >
                          <svg className="w-4 h-4 mr-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                          </svg>
                          My Orders
                        </Link>
                        <Link
                          href="/wishlist"
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors"
                          onClick={() => setIsUserMenuOpen(false)}
                        >
                          <svg className="w-4 h-4 mr-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                          </svg>
                          Wishlist
                        </Link>
                        <Link
                          href="/settings"
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors"
                          onClick={() => setIsUserMenuOpen(false)}
                        >
                          <svg className="w-4 h-4 mr-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                          </svg>
                          Settings
                        </Link>
                      </div>

                      {/* Logout Section */}
                      <div className="border-t border-gray-100 pt-2">
                        <button
                          onClick={() => {
                            logout();
                            setIsUserMenuOpen(false);
                          }}
                          className="flex items-center w-full px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition-colors"
                        >
                          <svg className="w-4 h-4 mr-3 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                          </svg>
                          Logout
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <div className="relative" ref={userMenuRef}>
                  {/* Guest User Icon */}
                  <button
                    onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                    className="p-2 hover:text-primary transition-colors"
                    aria-label="User menu"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </button>

                  {/* Guest User Dropdown */}
                  {isUserMenuOpen && (
                    <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg py-3 z-50 border">
                      <div className="px-4 pb-3 border-b border-gray-100">
                        <p className="text-sm text-gray-600">Welcome to Furniro</p>
                        <p className="text-xs text-gray-500">Please sign in to your account</p>
                      </div>
                      <div className="py-2">
                        <Link
                          href="/login"
                          className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors"
                          onClick={() => setIsUserMenuOpen(false)}
                        >
                          <svg className="w-4 h-4 mr-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                          </svg>
                          Login
                        </Link>
                        <Link
                          href="/register"
                          className="flex items-center px-4 py-2 text-sm text-primary hover:bg-primary/5 transition-colors"
                          onClick={() => setIsUserMenuOpen(false)}
                        >
                          <svg className="w-4 h-4 mr-3 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                          </svg>
                          Sign Up
                        </Link>
                      </div>
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Mobile Menu Button */}
          <button
            className="md:hidden p-2 text-dark"
            onClick={toggleMobileMenu}
            aria-label="Toggle mobile menu"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>

        {/* Mobile/Tablet Search Overlay */}
        {isSearchOpen && (
          <div className="lg:hidden absolute top-full left-0 right-0 bg-white border-t border-gray-200 shadow-lg z-50">
            <div className="container-custom py-4" ref={searchRef}>
              <form onSubmit={handleSearchSubmit} className="relative">
                <div className="relative">
                  <input
                    ref={searchInputRef}
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    onKeyDown={handleSearchKeyDown}
                    onFocus={() => searchQuery.trim() && searchSuggestions.length > 0 && setShowSearchDropdown(true)}
                    placeholder="Search products..."
                    className="w-full pl-4 pr-12 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all text-base"
                  />
                  <button
                    type="submit"
                    className="absolute right-2 top-1/2 transform -translate-y-1/2 p-2 text-gray-400 hover:text-primary transition-colors"
                    aria-label="Search"
                  >
                    {isSearchLoading ? (
                      <div className="w-5 h-5 border-2 border-gray-300 border-t-primary rounded-full animate-spin"></div>
                    ) : (
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                      </svg>
                    )}
                  </button>
                  <button
                    type="button"
                    onClick={toggleSearch}
                    className="absolute right-10 top-1/2 transform -translate-y-1/2 p-2 text-gray-400 hover:text-gray-600 transition-colors"
                    aria-label="Close search"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>

                {/* Mobile Search Suggestions Dropdown */}
                {showSearchDropdown && (
                  <div className="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-50 max-h-80 overflow-y-auto">
                    {searchSuggestions.length > 0 ? (
                      <>
                        {searchSuggestions.map((product) => (
                          <button
                            key={product.id}
                            onClick={() => handleSuggestionClick(product)}
                            className="w-full flex items-center p-4 hover:bg-gray-50 transition-colors text-left"
                          >
                            <div className="relative w-12 h-12 flex-shrink-0 mr-3">
                              <Image
                                src={getImageUrl(product.images?.[0] || '/images/product-1.png')}
                                alt={product.name || 'Product'}
                                fill
                                className="object-cover rounded"
                              />
                            </div>
                            <div className="flex-1 min-w-0">
                              <h4 className="text-sm font-medium text-gray-900 truncate">
                                {product.name || 'Product'}
                              </h4>
                              <p className="text-sm text-gray-500 truncate">
                                {product.categoryName || 'Uncategorized'}
                              </p>
                              <p className="text-sm font-medium text-primary">
                                ${product.price?.toFixed(2)}
                              </p>
                            </div>
                          </button>
                        ))}

                        {/* View All Results Link */}
                        <div className="border-t border-gray-100">
                          <Link
                            href={`/search?q=${encodeURIComponent(searchQuery)}`}
                            className="block w-full p-4 text-center text-sm text-primary hover:bg-gray-50 transition-colors"
                            onClick={() => {
                              setShowSearchDropdown(false);
                              setIsSearchOpen(false);
                            }}
                          >
                            View all results for "{searchQuery}"
                          </Link>
                        </div>
                      </>
                    ) : (
                      <div className="p-6 text-center text-gray-500">
                        <svg className="w-8 h-8 mx-auto mb-2 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <p className="text-sm">No products found</p>
                      </div>
                    )}
                  </div>
                )}
              </form>
            </div>
          </div>
        )}

        {/* Mobile Menu */}
        {isMobileMenuOpen && (
          <div className="md:hidden mt-4 pb-4">
            <nav className="flex flex-col space-y-4">
              <Link href="/" className="text-dark hover:text-primary transition-colors">
                Home
              </Link>
              <Link href="/shop" className="text-dark hover:text-primary transition-colors">
                Shop
              </Link>
              <Link href="/about" className="text-dark hover:text-primary transition-colors">
                About
              </Link>
              <Link href="/contact" className="text-dark hover:text-primary transition-colors">
                Contact
              </Link>
            </nav>
            {/* Mobile Authentication */}
            <div className="mt-4 pt-4 border-t border-gray-200">
              {isAuthenticated ? (
                <div className="space-y-2">
                  <div className="flex items-center space-x-3 px-2 py-2">
                    <UserProfileCompact />
                  </div>
                  <Link
                    href="/profile"
                    className="block px-2 py-2 text-dark hover:text-primary transition-colors"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    My Profile
                  </Link>
                  <Link
                    href="/orders"
                    className="block px-2 py-2 text-dark hover:text-primary transition-colors"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    My Orders
                  </Link>
                  <button
                    onClick={() => {
                      logout();
                      setIsMobileMenuOpen(false);
                    }}
                    className="block w-full text-left px-2 py-2 text-red-600 hover:text-red-700 transition-colors"
                  >
                    Logout
                  </button>
                </div>
              ) : (
                <div className="space-y-2">
                  <Link
                    href="/login"
                    className="block px-2 py-2 text-dark hover:text-primary transition-colors"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    Login
                  </Link>
                  <Link
                    href="/register"
                    className="block px-2 py-2 bg-primary text-white rounded hover:bg-primary/90 transition-colors text-center"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    Sign Up
                  </Link>
                </div>
              )}
            </div>

            {/* Mobile Icons - Same order: Search, Wishlist, Cart */}
            <div className="flex items-center space-x-4 mt-4">
              <button
                onClick={toggleSearch}
                aria-label="Search"
                className="p-2 hover:text-primary transition-colors"
              >
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </button>
              <button aria-label="Wishlist" className="p-2 hover:text-primary transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
              </button>
              <CartIcon showBadge={true} variant="icon" size="default" />
            </div>

            {/* Cart Summary for Mobile */}
            <div className="sm:hidden ml-4">
              <div className="text-xs text-gray-600">
                {itemCount > 0 && (
                  <span className="font-medium">
                    ${total.toFixed(2)}
                  </span>
                )}
              </div>
            </div>
          </div>
        )}


      </div>
    </header>
  );
};

export default Header;
