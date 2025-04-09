'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { ArrowRight } from 'lucide-react';

type FeaturedNewsCardProps = {
  id: string;
  title: string;
  image: string;
  date: Date;
};

const FeaturedNewsCard = ({ id, title, image, date }: FeaturedNewsCardProps) => {
  const formattedDate = format(date, 'dd/MM/yyyy', { locale: vi });
  
  return (
    <Link href={`/news/${id}`} className="block h-full">
      <div className="relative h-full overflow-hidden rounded-lg group">
        <div className="relative h-full min-h-[300px]">
          <Image 
            src={image} 
            alt={title}
            fill
            className="object-cover group-hover:scale-105 transition-transform duration-300"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black/80 to-transparent"></div>
        </div>
        
        <div className="absolute bottom-0 left-0 p-4 md:p-6 w-full">
          <p className="text-xs text-gray-300 mb-2">{formattedDate}</p>
          <h3 className="text-xl md:text-2xl font-bold text-white group-hover:text-primary-100 transition-colors">
            {title}
          </h3>
        </div>
      </div>
    </Link>
  );
};

type NewsListItemProps = {
  id: string;
  title: string;
  thumbnail: string;
  date: Date;
};

const NewsListItem = ({ id, title, thumbnail, date }: NewsListItemProps) => {
  const formattedDate = format(date, 'dd/MM/yyyy', { locale: vi });
  
  return (
    <Link href={`/news/${id}`} className="flex items-start space-x-3 py-3 group">
      <div className="relative w-24 h-24 rounded-md overflow-hidden flex-shrink-0">
        <Image 
          src={thumbnail} 
          alt={title}
          fill
          className="object-cover"
        />
      </div>
      
      <div className="flex-grow">
        <h4 className="text-sm font-medium line-clamp-2 group-hover:text-primary transition-colors">
          {title}
        </h4>
        <p className="text-xs text-gray-500 mt-1">{formattedDate}</p>
      </div>
    </Link>
  );
};

const TechNews = () => {
  const featuredNews: FeaturedNewsCardProps = {
    id: 'news-1',
    title: 'Tối ưu hóa hiệu suất của dòng laptop mỏng nhẹ dành cho công việc',
    image: '/assets/news/featured-news.jpg',
    date: new Date('2025-04-05'),
  };
  
  const newsList: NewsListItemProps[] = [
    {
      id: 'news-2',
      title: 'Tại sao nên chọn laptop dòng cao cấp cho các nhà sáng tạo nội dung?',
      thumbnail: '/assets/news/news-thumb-1.jpg',
      date: new Date('2025-04-08'),
    },
    {
      id: 'news-3',
      title: 'So sánh hiệu năng chip M3 Pro và M3 Max trên MacBook Pro mới nhất',
      thumbnail: '/assets/news/news-thumb-2.jpg',
      date: new Date('2025-04-07'),
    },
    {
      id: 'news-4',
      title: 'Top 5 màn hình gaming tốt nhất dành cho game thủ năm 2025',
      thumbnail: '/assets/news/news-thumb-3.jpg',
      date: new Date('2025-04-06'),
    },
    {
      id: 'news-5',
      title: 'Hướng dẫn lựa chọn bàn phím cơ phù hợp với nhu cầu làm việc',
      thumbnail: '/assets/news/news-thumb-4.jpg',
      date: new Date('2025-04-04'),
    },
    {
      id: 'news-6',
      title: 'Những mẫu laptop gaming dưới 20 triệu đáng mua nhất hiện nay',
      thumbnail: '/assets/news/news-thumb-5.jpg',
      date: new Date('2025-04-03'),
    },
  ];

  return (
    <section className="mb-10">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-bold">Tin tức công nghệ</h2>
        <Link 
          href="/news" 
          className="flex items-center text-sm font-medium text-primary hover:text-primary-600"
        >
          Xem tất cả
          <ArrowRight size={16} className="ml-1" />
        </Link>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-1">
          <FeaturedNewsCard {...featuredNews} />
        </div>
        
        <div className="md:col-span-2">
          <div className="flex flex-col divide-y divide-gray-200">
            {newsList.map((news) => (
              <NewsListItem key={news.id} {...news} />
            ))}
          </div>
        </div>
      </div>
    </section>
  );
};

export default TechNews;
