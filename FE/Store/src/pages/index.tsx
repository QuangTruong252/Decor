import FeaturedLaptops from '@/app/components/FeaturedLaptops';
import FlashSale from '@/app/components/FlashSale';
import HeroBanner from '@/app/components/HeroBanner';
import Preorder from '@/app/components/Preorder';
import React from 'react';

// Import all section components
import WhyThinkPro from '@/app/components/WhyThinkPro';
import Recommendations from '@/app/components/Recommendations';
import TechNews from '@/app/components/TechNews';
import Tags from '@/app/components/Tags';
import Categories from '@/app/components/Categories';
import Footer from '@/app/components/Footer';
import Header from '@/app/components/Header';

export default function Home() {
  return (
    <main className="min-h-screen">
      <Header />
      <HeroBanner />
      <div className="container mx-auto px-4 lg:px-6">
        <Categories />
        <FeaturedLaptops />
        <FlashSale />
        <Preorder />
        <WhyThinkPro />
        <Recommendations />
        <TechNews />
        <Tags />
      </div>
      <Footer />
    </main>
  );
}
