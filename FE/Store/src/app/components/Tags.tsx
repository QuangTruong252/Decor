'use client';

import React from 'react';
import Link from 'next/link';

type TagProps = {
  label: string;
  href: string;
};

const Tag = ({ label, href }: TagProps) => {
  return (
    <Link 
      href={href}
      className="px-3 py-1 rounded-full border border-gray-300 text-sm text-gray-700 hover:bg-primary hover:text-white hover:border-primary transition-colors"
    >
      {label}
    </Link>
  );
};

const Tags = () => {
  const tags = [
    { label: 'Laptop Gaming', href: '/tag/laptop-gaming' },
    { label: 'Laptop Văn Phòng', href: '/tag/laptop-van-phong' },
    { label: 'Laptop Đồ Họa', href: '/tag/laptop-do-hoa' },
    { label: 'MacBook', href: '/tag/macbook' },
    { label: 'PC Gaming', href: '/tag/pc-gaming' },
    { label: 'PC Đồ Họa', href: '/tag/pc-do-hoa' },
    { label: 'Màn Hình Gaming', href: '/tag/man-hinh-gaming' },
    { label: 'Màn Hình Đồ Họa', href: '/tag/man-hinh-do-hoa' },
    { label: 'Chuột Gaming', href: '/tag/chuot-gaming' },
    { label: 'Bàn Phím Cơ', href: '/tag/ban-phim-co' },
    { label: 'Tai Nghe Gaming', href: '/tag/tai-nghe-gaming' },
    { label: 'Ghế Gaming', href: '/tag/ghe-gaming' },
    { label: 'Bàn Gaming', href: '/tag/ban-gaming' },
    { label: 'Apple', href: '/tag/apple' },
    { label: 'Asus', href: '/tag/asus' },
    { label: 'MSI', href: '/tag/msi' },
    { label: 'Dell', href: '/tag/dell' },
    { label: 'Lenovo', href: '/tag/lenovo' },
  ];

  return (
    <section className="mb-10">
      <h2 className="text-xl font-bold mb-4">Tìm kiếm phổ biến</h2>
      
      <div className="flex flex-wrap gap-2">
        {tags.map((tag, index) => (
          <Tag key={index} label={tag.label} href={tag.href} />
        ))}
      </div>
    </section>
  );
};

export default Tags;
