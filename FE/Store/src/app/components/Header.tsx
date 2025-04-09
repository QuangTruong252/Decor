'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Search, ShoppingBag, Phone, HelpCircle, User, Menu } from 'lucide-react';

const Header = () => {
  return (
    <header className="bg-white z-50 border-b border-gray-200">
      {/* Top Bar */}
      <div className="container mx-auto px-4 py-1 hidden md:flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Link href="tel:1900.63.3579" className="flex items-center text-xs text-gray-600 hover:text-primary">
            <Phone size={14} className="mr-1" />
            <span>1900.63.3579</span>
          </Link>
          <Link href="/support" className="flex items-center text-xs text-gray-600 hover:text-primary">
            <HelpCircle size={14} className="mr-1" />
            <span>Hỗ trợ</span>
          </Link>
        </div>
        <div className="flex items-center space-x-4 text-xs">
          <Link href="/order-tracking" className="text-gray-600 hover:text-primary">
            Đơn hàng của bạn
          </Link>
          <Link href="/news" className="text-gray-600 hover:text-primary">
            Tin tức
          </Link>
        </div>
      </div>

      {/* Main Header */}
      <div className="container mx-auto px-4 py-2">
        <div className="flex items-center justify-between">
          {/* Mobile Menu Button */}
          <div className="md:hidden">
            <button className="text-gray-700 p-1" aria-label="Menu">
              <Menu size={24} />
            </button>
          </div>
          
          {/* Logo */}
          <Link href="/" className="flex-shrink-0 flex items-center">
            <Image 
              src="/assets/logo-pro.svg" 
              alt="ThinkPro Logo" 
              width={44} 
              height={44} 
              className="h-11 w-auto"
              priority
            />
            <span className="ml-2 md:ml-3 hidden md:inline-block text-lg font-bold">Sản phẩm</span>
          </Link>

          {/* Search input */}
          <div className="relative flex-grow mx-4 max-w-xl">
            <div className="relative w-full flex items-center">
              <input
                type="text"
                placeholder="Xin chào, bạn đang tìm gì?"
                className="w-full h-10 px-4 py-2 pl-10 pr-10 rounded-full border border-gray-200 focus:border-primary focus:outline-none text-sm"
              />
              <div className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400">
                <Search size={18} />
              </div>
              {/* User avatar in search box */}
              <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                <div className="w-6 h-6 rounded-full bg-blue-100 flex items-center justify-center">
                  <Image 
                    src="/assets/user-avatar.png" 
                    alt="User" 
                    width={20} 
                    height={20} 
                    className="rounded-full"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Auth and cart buttons */}
          <div className="flex items-center space-x-4">
            <Link 
              href="/account" 
              className="flex items-center text-sm font-medium text-gray-700 hover:text-primary"
            >
              <User size={20} className="mr-1" />
              <span className="hidden md:inline">Đăng nhập</span>
            </Link>
            
            <Link 
              href="/cart" 
              className="text-gray-700 hover:text-primary relative"
              aria-label="Giỏ hàng"
            >
              <div className="relative">
                <ShoppingBag size={22} />
                <span className="absolute -top-1 -right-1 flex items-center justify-center w-4 h-4 rounded-full bg-primary text-white text-xs font-bold">
                  0
                </span>
              </div>
              <span className="hidden md:inline-block ml-1">0₫</span>
            </Link>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;
