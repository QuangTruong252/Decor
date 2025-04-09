'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';

type CategoryProps = {
  icon: string;
  label: string;
  href: string;
};

const CategoryItem = ({ icon, label, href }: CategoryProps) => {
  return (
    <Link 
      href={href}
      className="flex flex-col items-center justify-center min-w-[80px] md:min-w-[100px] p-2"
    >
      <div className="w-16 h-16 mb-2 rounded-md overflow-hidden">
        <Image 
          src={icon} 
          alt={label} 
          width={80} 
          height={80} 
          className="object-cover w-full h-full" 
        />
      </div>
      <span className="text-xs font-medium text-center text-gray-700">{label}</span>
    </Link>
  );
};

const Categories = () => {
  const categories = [
    { icon: '/assets/categories/laptop-1-1.webp', label: 'Laptop', href: '/category/laptop' },
    { icon: '/assets/categories/danh-muc-icon-may-choi-game-game-console-thinkpro.vn.webp', label: 'Máy chơi game/ Game Console', href: '/category/gaming' },
    { icon: '/assets/categories/danh-muc-icon-kinh-thuc-te-ao-vrar-thinkpro.vn.webp', label: 'Kính Thực Tế Ảo VR/AR', href: '/category/vr' },
    { icon: '/assets/categories/danh-muc-icon-ban-phim-thinkpro.vn.webp', label: 'Bàn phím', href: '/category/keyboard' },
    { icon: '/assets/categories/danh-muc-icon-ghe-cong-thai-hoc-thinkpro.vn.webp', label: 'Ghế công thái học', href: '/category/chair' },
    { icon: '/assets/categories/danh-muc-icon-ban-nang-ha-thinkpro.vn.webp', label: 'Bàn nâng hạ', href: '/category/desk' },
    { icon: '/assets/categories/danh-muc-icon-hoc-tu-thinkpro.vn.webp', label: 'Hộc tủ', href: '/category/cabinet' },
    { icon: '/assets/categories/danh-muc-icon-am-thanh-thinkpro.vn.webp', label: 'Âm thanh', href: '/category/audio' },
    { icon: '/assets/categories/chuot.webp', label: 'Chuột', href: '/category/mouse' },
    { icon: '/assets/categories/danh-muc-icon-balo-tui-thinkpro.vn.webp', label: 'Balo, Túi', href: '/category/bag' },
    { icon: '/assets/categories/phan-mem-2.webp', label: 'Phần mềm', href: '/category/software' },
    { icon: '/assets/categories/danh-muc-icon-arm-man-hinh-thinkpro.vn.webp', label: 'Arm màn hình', href: '/category/arm' },
    { icon: '/assets/categories/danh-muc-icon-phu-kien-setup-thinkpro.vn.webp', label: 'Phụ kiện Setup', href: '/category/phone-stand' },
    { icon: '/assets/categories/32d4sam3200-512x512-png.webp', label: 'RAM', href: '/category/ram' },
    { icon: '/assets/categories/samsung-990-512x512png.webp', label: 'Ổ cứng', href: '/category/ssd' },
    { icon: '/assets/categories/danh-muc-icon-sac-thinkpro.vn.webp', label: 'Sạc', href: '/category/charger' },
  ];

  return (
    <section className="mb-10">
      <h2 className="text-xl font-bold mb-4 ml-4">Danh mục nổi bật</h2>
      <div className="overflow-x-auto pb-4 -mx-4 px-4 scrollbar-hide">
        <div className="flex space-x-3 min-w-max">
          {categories.map((category, index) => (
            <CategoryItem 
              key={index}
              icon={category.icon} 
              label={category.label} 
              href={category.href} 
            />
          ))}
        </div>
      </div>
    </section>
  );
};

export default Categories;
