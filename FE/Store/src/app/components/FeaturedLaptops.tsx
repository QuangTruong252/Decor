'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { ChevronRight } from 'lucide-react';

type FeaturedCardProps = {
  title: string;
  description: string;
  date: string;
  bgColor: string;
  laptops: {
    id: string;
    name: string;
    image: string;
  }[];
};

const FeaturedCard = ({ title, description, date, bgColor, laptops }: FeaturedCardProps) => {
  return (
    <div className={`rounded-lg overflow-hidden p-5 ${bgColor}`}>
      <div className="flex flex-col h-full">
        <div className="mb-3">
          <h3 className="text-lg font-bold">{title}</h3>
          <p className="text-sm text-gray-700">{description}</p>
          <span className="text-xs text-gray-500">{date}</span>
        </div>
        
        {/* Horizontal image carousel with scroll-snap */}
        <div className="overflow-x-auto pb-4 -mx-2 px-2 flex-grow">
          <div className="flex space-x-4 scroll-smooth snap-x snap-mandatory">
            {laptops.map((laptop) => (
              <div 
                key={laptop.id}
                className="snap-start flex-shrink-0 w-[160px] group"
              >
                <Link 
                  href={`/product/${laptop.id}`} 
                  className="block bg-white rounded-lg p-2 shadow-sm hover:shadow-md transition-shadow"
                >
                  <div className="h-24 relative mb-2">
                    <Image 
                      src={laptop.image} 
                      alt={laptop.name}
                      fill
                      className="object-contain"
                    />
                  </div>
                  <p className="text-xs font-medium text-center line-clamp-2 group-hover:text-primary transition-colors">
                    {laptop.name}
                  </p>
                </Link>
              </div>
            ))}
          </div>
        </div>
        
        <div className="mt-3 text-right">
          <Link 
            href="/laptop-deals" 
            className="inline-flex items-center text-sm font-medium hover:text-primary"
          >
            Xem tất cả
            <ChevronRight size={16} className="ml-1" />
          </Link>
        </div>
      </div>
    </div>
  );
};

const FeaturedLaptops = () => {
  const greenCardData = {
    title: "Laptop Gaming cao cấp",
    description: "Hiệu năng vượt trội, thiết kế độc đáo",
    date: "Cập nhật: 09/04/2025",
    bgColor: "bg-green-light",
    laptops: [
      { id: "laptop-1", name: "Laptop Gaming ASUS ROG Strix G15", image: "/assets/laptops/gaming-1.png" },
      { id: "laptop-2", name: "Laptop MSI Stealth 16 Studio A1VFG", image: "/assets/laptops/gaming-2.png" },
      { id: "laptop-3", name: "Laptop Gaming Lenovo Legion Pro 5", image: "/assets/laptops/gaming-3.png" },
      { id: "laptop-4", name: "Laptop Gaming Acer Predator Helios Neo 16", image: "/assets/laptops/gaming-4.png" },
    ]
  };
  
  const pinkCardData = {
    title: "Laptop văn phòng mỏng nhẹ",
    description: "Di động tối đa, hiệu suất ổn định",
    date: "Cập nhật: 08/04/2025",
    bgColor: "bg-pink-light",
    laptops: [
      { id: "laptop-5", name: "Laptop LG Gram Style 16", image: "/assets/laptops/thin-1.png" },
      { id: "laptop-6", name: "Laptop Dell XPS 13 Plus 9320", image: "/assets/laptops/thin-2.png" },
      { id: "laptop-7", name: "Laptop Apple MacBook Air M2", image: "/assets/laptops/thin-3.png" },
      { id: "laptop-8", name: "Laptop Microsoft Surface Laptop 5", image: "/assets/laptops/thin-4.png" },
    ]
  };

  return (
    <section className="mb-10">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <FeaturedCard {...greenCardData} />
        <FeaturedCard {...pinkCardData} />
      </div>
    </section>
  );
};

export default FeaturedLaptops;
