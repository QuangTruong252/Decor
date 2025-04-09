'use client';

import React from 'react';
import Image from 'next/image';
import Link from 'next/link';
import Slider from 'react-slick';
import 'slick-carousel/slick/slick.css';
import 'slick-carousel/slick/slick-theme.css';
import { ChevronLeft, ChevronRight } from 'lucide-react';

// Custom arrows for the slider
const PrevArrow = (props: any) => {
  const { onClick } = props;
  return (
    <button 
      className="absolute left-4 top-1/2 -translate-y-1/2 z-10 bg-white/30 hover:bg-white/60 rounded-full p-2 backdrop-blur-sm"
      onClick={onClick}
      aria-label="Previous slide"
    >
      <ChevronLeft size={20} className="text-white" />
    </button>
  );
};

const NextArrow = (props: any) => {
  const { onClick } = props;
  return (
    <button 
      className="absolute right-4 top-1/2 -translate-y-1/2 z-10 bg-white/30 hover:bg-white/60 rounded-full p-2 backdrop-blur-sm"
      onClick={onClick}
      aria-label="Next slide"
    >
      <ChevronRight size={20} className="text-white" />
    </button>
  );
};

const HeroBanner = () => {
  const settings = {
    dots: true,
    infinite: true,
    speed: 500,
    slidesToShow: 1,
    slidesToScroll: 1,
    autoplay: true,
    autoplaySpeed: 5000,
    prevArrow: <PrevArrow />,
    nextArrow: <NextArrow />,
    dotsClass: "slick-dots custom-dots",
    appendDots: (dots: React.ReactNode) => (
      <div style={{ position: 'absolute', bottom: '10px', width: '100%', textAlign: 'center' }}>
        <ul style={{ margin: '0' }}>{dots}</ul>
      </div>
    ),
    customPaging: () => (
      <div className="w-2 h-2 mx-1 bg-white/50 rounded-full hover:bg-white"></div>
    ),
  };

  const banners = [
    {
      id: 1,
      image: '/assets/banners/banner-2400-x-600.webp',
      title: 'Welcome to ThinkPro V4',
      subtitle: 'Hãy gửi phản hồi để chúng mình cải thiện tốt hơn',
      buttonText: 'Gửi góp ý',
      buttonLink: '/feedback',
    },
    {
      id: 2,
      image: '/assets/banners/banner-desktop-2400x600.webp',
      title: 'ProCare+ Bảo hành mở rộng',
      subtitle: 'Nâng cấp trải nghiệm sử dụng và bảo vệ thiết bị của bạn',
      buttonText: 'Tìm hiểu thêm',
      buttonLink: '/procare-plus',
    }
  ];

  return (
    <section className="relative w-[80%] mx-auto rounded-lg h-[300px] md:h-[400px] overflow-hidden mb-8">
      <Slider {...settings} className="h-full">
        {banners.map((banner) => (
          <div key={banner.id} className="relative w-full h-[300px] md:h-[400px]">
            <div className="relative w-full h-full">
              <Image
                src={banner.image}
                alt={banner.title}
                fill
                priority
                className="object-cover"
                placeholder="blur"
                blurDataURL="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="
              />
              
            </div>
          </div>
        ))}
      </Slider>
    </section>
  );
};

export default HeroBanner;
