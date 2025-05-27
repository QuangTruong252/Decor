'use client';

import React, { useState, useEffect } from 'react';
import MainLayout from '@/components/layout/MainLayout';
import HeroBanner from '@/components/sections/HeroBanner';
import Categories from '@/components/sections/Categories';
import FeaturedProducts from '@/components/sections/FeaturedProducts';
import Features from '@/components/sections/Features';
import { ProductService, BannerService, CategoryService } from '@/api/services';
import { ProductDTO, BannerDTO, CategoryDTO } from '@/api/types';
import toast from 'react-hot-toast';

export default function Home() {
  const [featuredProducts, setFeaturedProducts] = useState<ProductDTO[]>([]);
  const [banners, setBanners] = useState<BannerDTO[]>([]);
  const [categories, setCategories] = useState<CategoryDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetchHomeData();
  }, []);

  const fetchHomeData = async () => {
    try {
      setIsLoading(true);

      // Fetch all data in parallel
      const [productsResponse, bannersResponse, categoriesResponse] = await Promise.allSettled([
        ProductService.getFeaturedProducts(),
        BannerService.getActiveBanners(),
        CategoryService.getCategories({ pageSize: 6 })
      ]);

      // Handle featured products
      if (productsResponse.status === 'fulfilled') {
        setFeaturedProducts(productsResponse.value || []);
      } else {
        console.error('Failed to fetch featured products:', productsResponse.reason);
      }

      // Handle banners
      if (bannersResponse.status === 'fulfilled') {
        setBanners(bannersResponse.value || []);
      } else {
        console.error('Failed to fetch banners:', bannersResponse.reason);
      }

      // Handle categories
      if (categoriesResponse.status === 'fulfilled') {
        setCategories(categoriesResponse.value.items || []);
      } else {
        console.error('Failed to fetch categories:', categoriesResponse.reason);
      }

    } catch (err) {
      const errorMessage = 'Failed to load home page data';
      toast.error(errorMessage);
      console.error('Error fetching home data:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <MainLayout>
      <HeroBanner banners={banners} isLoading={isLoading} />
      <Categories categories={categories} isLoading={isLoading} />
      <FeaturedProducts products={featuredProducts} isLoading={isLoading} />
      <Features />
    </MainLayout>
  );
}
