import React from 'react';
import Link from 'next/link';
import Image from 'next/image';

const HeroBanner = () => {
  return (
    <section className="relative h-[600px] md:h-[700px] overflow-hidden">
      {/* Background Image */}
      <div className="absolute inset-0">
        <Image
          src="/images/hero-banner.png"
          alt="Modern furniture for your home"
          fill
          priority
          className="object-cover object-center"
        />
        <div className="absolute inset-0 bg-black bg-opacity-30"></div>
      </div>
      
      {/* Content */}
      <div className="container-custom relative h-full flex items-center">
        <div className="bg-secondary bg-opacity-90 p-8 md:p-12 max-w-md">
          <p className="text-primary font-medium mb-2">New Arrival</p>
          <h1 className="text-4xl md:text-5xl font-bold text-dark mb-4">
            Discover Our <br /> New Collection
          </h1>
          <p className="text-dark mb-8">
            Find the perfect furniture for your home with our latest collection of modern and stylish pieces.
          </p>
          <Link 
            href="/shop" 
            className="bg-primary text-white py-3 px-8 inline-block hover:bg-opacity-90 transition-all"
          >
            SHOP NOW
          </Link>
        </div>
      </div>
    </section>
  );
};

export default HeroBanner;
