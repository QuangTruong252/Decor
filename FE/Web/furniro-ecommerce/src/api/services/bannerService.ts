import { api } from '../client';
import { BANNER } from '../endpoints';
import type {
  BannerDTO,
  CreateBannerDTO,
  UpdateBannerDTO,
} from '../types';

export class BannerService {
  /**
   * Get all banners
   */
  static async getBanners(): Promise<BannerDTO[]> {
    const response = await api.get<BannerDTO[]>(BANNER.BASE);
    return response.data;
  }

  /**
   * Get active banners only
   */
  static async getActiveBanners(): Promise<BannerDTO[]> {
    const response = await api.get<BannerDTO[]>(BANNER.ACTIVE);
    return response.data;
  }

  /**
   * Get banner by ID
   */
  static async getBannerById(id: number): Promise<BannerDTO> {
    const response = await api.get<BannerDTO>(BANNER.BY_ID(id));
    return response.data;
  }

  /**
   * Create new banner
   */
  static async createBanner(bannerData: CreateBannerDTO): Promise<BannerDTO> {
    const response = await api.post<BannerDTO>(BANNER.BASE, bannerData);
    return response.data;
  }

  /**
   * Update banner
   */
  static async updateBanner(id: number, bannerData: UpdateBannerDTO): Promise<BannerDTO> {
    const response = await api.put<BannerDTO>(BANNER.BY_ID(id), bannerData);
    return response.data;
  }

  /**
   * Delete banner
   */
  static async deleteBanner(id: number): Promise<void> {
    await api.delete(BANNER.BY_ID(id));
  }

  /**
   * Create banner with image upload
   */
  static async createBannerWithImage(bannerData: CreateBannerDTO): Promise<BannerDTO> {
    const formData = new FormData();
    
    if (bannerData.title) formData.append('title', bannerData.title);
    if (bannerData.link) formData.append('link', bannerData.link);
    if (bannerData.isActive !== undefined) formData.append('isActive', bannerData.isActive.toString());
    if (bannerData.displayOrder !== undefined) formData.append('displayOrder', bannerData.displayOrder.toString());
    if (bannerData.imageFile) formData.append('imageFile', bannerData.imageFile);

    const response = await api.post<BannerDTO>(BANNER.BASE, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  /**
   * Update banner with image upload
   */
  static async updateBannerWithImage(id: number, bannerData: UpdateBannerDTO): Promise<BannerDTO> {
    const formData = new FormData();
    
    if (bannerData.title) formData.append('title', bannerData.title);
    if (bannerData.link) formData.append('link', bannerData.link);
    if (bannerData.isActive !== undefined) formData.append('isActive', bannerData.isActive.toString());
    if (bannerData.displayOrder !== undefined) formData.append('displayOrder', bannerData.displayOrder.toString());
    if (bannerData.imageFile) formData.append('imageFile', bannerData.imageFile);

    const response = await api.put<BannerDTO>(BANNER.BY_ID(id), formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }
}

export default BannerService;
