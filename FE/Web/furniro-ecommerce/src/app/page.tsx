import MainLayout from '@/components/layout/MainLayout';
import HeroBanner from '@/components/sections/HeroBanner';
import Categories from '@/components/sections/Categories';
import FeaturedProducts from '@/components/sections/FeaturedProducts';
import Features from '@/components/sections/Features';

export default function Home() {
  return (
    <MainLayout>
      <HeroBanner />
      <Categories />
      <FeaturedProducts />
      <Features />
    </MainLayout>
  );
}
