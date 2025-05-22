import React from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Link from 'next/link';
import Image from 'next/image';
import Features from '@/components/sections/Features';

export default function AboutPage() {
  return (
    <MainLayout>
      {/* About Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">About Us</h1>
          <div className="flex items-center justify-center mt-4">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">About</span>
          </div>
        </div>
      </div>

      {/* Our Story */}
      <section className="py-16">
        <div className="container-custom">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="text-3xl font-bold text-dark mb-6">Our Story</h2>
              <p className="text-text-secondary mb-6">
                Furniro is a global furniture retailer founded in 2010 with a vision to create affordable, well-designed furniture for people around the world. What started as a small workshop in a garage has grown into an international brand with presence in over 50 countries.
              </p>
              <p className="text-text-secondary mb-6">
                Our mission is to offer high-quality, stylish furniture that makes everyday life at home better. We believe that beautiful, functional furniture should be accessible to everyone, regardless of budget or space constraints.
              </p>
              <p className="text-text-secondary">
                Today, Furniro continues to innovate and expand, but our core values remain the same: quality, affordability, sustainability, and exceptional customer service.
              </p>
            </div>
            <div className="relative h-[400px] lg:h-[500px]">
              <Image
                src="/images/product-3.png"
                alt="Our workshop"
                fill
                className="object-cover rounded-lg"
              />
            </div>
          </div>
        </div>
      </section>

      {/* Our Values */}
      <section className="py-16 bg-light">
        <div className="container-custom">
          <h2 className="text-3xl font-bold text-dark text-center mb-12">Our Values</h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <div className="w-16 h-16 bg-primary bg-opacity-10 rounded-full flex items-center justify-center mb-4 mx-auto">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                </svg>
              </div>
              <h3 className="text-xl font-medium text-dark text-center mb-2">Quality</h3>
              <p className="text-text-secondary text-center">
                We use only the finest materials and craftsmanship to ensure our furniture stands the test of time.
              </p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <div className="w-16 h-16 bg-primary bg-opacity-10 rounded-full flex items-center justify-center mb-4 mx-auto">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 className="text-xl font-medium text-dark text-center mb-2">Affordability</h3>
              <p className="text-text-secondary text-center">
                We believe beautiful design should be accessible to everyone, regardless of budget.
              </p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <div className="w-16 h-16 bg-primary bg-opacity-10 rounded-full flex items-center justify-center mb-4 mx-auto">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 className="text-xl font-medium text-dark text-center mb-2">Sustainability</h3>
              <p className="text-text-secondary text-center">
                We're committed to sustainable practices and reducing our environmental footprint.
              </p>
            </div>
            
            <div className="bg-white p-6 rounded-lg shadow-sm">
              <div className="w-16 h-16 bg-primary bg-opacity-10 rounded-full flex items-center justify-center mb-4 mx-auto">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 5.636l-3.536 3.536m0 5.656l3.536 3.536M9.172 9.172L5.636 5.636m3.536 9.192l-3.536 3.536M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
              <h3 className="text-xl font-medium text-dark text-center mb-2">Service</h3>
              <p className="text-text-secondary text-center">
                We provide exceptional customer service from browsing to delivery and beyond.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Our Team */}
      <section className="py-16">
        <div className="container-custom">
          <h2 className="text-3xl font-bold text-dark text-center mb-12">Meet Our Team</h2>
          
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
            {[1, 2, 3, 4].map((member) => (
              <div key={member} className="text-center">
                <div className="relative h-80 mb-4 bg-light rounded-lg overflow-hidden">
                  <Image
                    src={`/images/product-${member}.png`}
                    alt={`Team Member ${member}`}
                    fill
                    className="object-cover"
                  />
                </div>
                <h3 className="text-xl font-medium text-dark">John Doe</h3>
                <p className="text-text-secondary">Furniture Designer</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <Features />

      {/* Call to Action */}
      <section className="py-16 bg-primary bg-opacity-10">
        <div className="container-custom text-center">
          <h2 className="text-3xl font-bold text-dark mb-6">Ready to Transform Your Space?</h2>
          <p className="text-text-secondary max-w-2xl mx-auto mb-8">
            Browse our collection of high-quality furniture and find the perfect pieces to make your house feel like home.
          </p>
          <Link href="/shop" className="btn-primary inline-block">
            Shop Now
          </Link>
        </div>
      </section>
    </MainLayout>
  );
}
