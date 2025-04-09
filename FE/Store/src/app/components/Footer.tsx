'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Facebook, Twitter, Instagram, Youtube, Linkedin, Mail, Phone, MapPin } from 'lucide-react';

const Footer = () => {
  const currentYear = new Date().getFullYear();
  
  const linkColumns = [
    {
      title: 'Về ThinkPro',
      links: [
        { label: 'Giới thiệu', href: '/about-us' },
        { label: 'Tuyển dụng', href: '/careers' },
        { label: 'Tin tức', href: '/news' },
        { label: 'Liên hệ', href: '/contact' },
      ],
    },
    {
      title: 'Chính sách',
      links: [
        { label: 'Chính sách bảo hành', href: '/warranty-policy' },
        { label: 'Chính sách thanh toán', href: '/payment-policy' },
        { label: 'Chính sách vận chuyển', href: '/shipping-policy' },
        { label: 'Chính sách đổi trả', href: '/return-policy' },
        { label: 'Chính sách bảo mật', href: '/privacy-policy' },
      ],
    },
    {
      title: 'Hỗ trợ khách hàng',
      links: [
        { label: 'Trung tâm trợ giúp', href: '/help-center' },
        { label: 'Hướng dẫn mua hàng', href: '/buying-guide' },
        { label: 'Đăng ký bảo hành', href: '/warranty-registration' },
        { label: 'Tra cứu đơn hàng', href: '/order-tracking' },
        { label: 'Trade-in thu cũ đổi mới', href: '/trade-in' },
      ],
    },
  ];

  return (
    <footer className="bg-gray-100 pt-12 pb-6">
      <div className="container mx-auto px-4 lg:px-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-8 mb-10">
          {/* Company Info */}
          <div className="lg:col-span-2">
            <Link href="/" className="inline-block mb-4">
              <Image 
                src="/assets/logo.png" 
                alt="ThinkPro Logo" 
                width={140} 
                height={40} 
                className="h-10 w-auto"
              />
            </Link>
            
            <p className="text-sm text-gray-600 mb-4">
              ThinkPro - Hệ thống laptop, PC và phụ kiện chính hãng. Trải nghiệm mua sắm với dịch vụ chuyên nghiệp, tận tâm và bảo hành tận nơi.
            </p>
            
            <div className="flex space-x-4 mb-6">
              <Link href="https://facebook.com/thinkpro" aria-label="ThinkPro Facebook" className="text-gray-600 hover:text-primary transition-colors">
                <Facebook size={20} />
              </Link>
              <Link href="https://twitter.com/thinkpro" aria-label="ThinkPro Twitter" className="text-gray-600 hover:text-primary transition-colors">
                <Twitter size={20} />
              </Link>
              <Link href="https://instagram.com/thinkpro" aria-label="ThinkPro Instagram" className="text-gray-600 hover:text-primary transition-colors">
                <Instagram size={20} />
              </Link>
              <Link href="https://youtube.com/thinkpro" aria-label="ThinkPro Youtube" className="text-gray-600 hover:text-primary transition-colors">
                <Youtube size={20} />
              </Link>
              <Link href="https://linkedin.com/company/thinkpro" aria-label="ThinkPro LinkedIn" className="text-gray-600 hover:text-primary transition-colors">
                <Linkedin size={20} />
              </Link>
            </div>
            
            <div className="space-y-2">
              <div className="flex items-start space-x-2">
                <Phone size={18} className="text-gray-600 mt-0.5 flex-shrink-0" />
                <p className="text-sm text-gray-600">Hotline: 1900 63 3579</p>
              </div>
              <div className="flex items-start space-x-2">
                <Mail size={18} className="text-gray-600 mt-0.5 flex-shrink-0" />
                <p className="text-sm text-gray-600">Email: support@thinkpro.vn</p>
              </div>
              <div className="flex items-start space-x-2">
                <MapPin size={18} className="text-gray-600 mt-0.5 flex-shrink-0" />
                <p className="text-sm text-gray-600">
                  Trụ sở: Tầng 2, 28 Thành Thái, Dịch Vọng, Cầu Giấy, Hà Nội
                </p>
              </div>
            </div>
          </div>
          
          {/* Link Columns */}
          {linkColumns.map((column, index) => (
            <div key={index}>
              <h4 className="font-medium text-gray-900 mb-4">{column.title}</h4>
              <ul className="space-y-2">
                {column.links.map((link, linkIndex) => (
                  <li key={linkIndex}>
                    <Link 
                      href={link.href} 
                      className="text-sm text-gray-600 hover:text-primary transition-colors"
                    >
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
        
        {/* Payment options */}
        <div className="border-t border-gray-200 pt-6 pb-4">
          <h4 className="font-medium text-gray-900 mb-4">Phương thức thanh toán</h4>
          <div className="flex flex-wrap gap-3">
            {['visa', 'mastercard', 'jcb', 'cash', 'momo', 'zalopay', 'vnpay'].map((method) => (
              <div key={method} className="w-12 h-8 bg-white rounded border border-gray-200 flex items-center justify-center">
                <Image 
                  src={`/assets/payment/${method}.png`} 
                  alt={`Payment method ${method}`}
                  width={28}
                  height={18}
                  className="object-contain"
                />
              </div>
            ))}
          </div>
        </div>
        
        {/* Copyright */}
        <div className="border-t border-gray-200 pt-6 text-center">
          <p className="text-sm text-gray-600">
            © {currentYear} ThinkPro. Tất cả các quyền được bảo lưu.
          </p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
