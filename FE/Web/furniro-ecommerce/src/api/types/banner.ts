import { BaseEntity } from './common';

// Banner related types
export interface BannerDTO extends BaseEntity {
  title: string | null;
  imageUrl: string | null;
  link: string | null;
  isActive: boolean;
  displayOrder: number;
}

export interface CreateBannerDTO {
  title?: string;
  imageUrl?: string;
  link?: string;
  isActive?: boolean;
  displayOrder?: number;
  imageFile?: File;
}

export interface UpdateBannerDTO {
  title?: string;
  imageUrl?: string;
  link?: string;
  isActive?: boolean;
  displayOrder?: number;
  imageFile?: File;
}

// Extended banner types for UI
export interface Banner extends BannerDTO {
  canEdit: boolean;
  canDelete: boolean;
  isLoading?: boolean;
}

export interface BannerFormData {
  title: string;
  link: string;
  isActive: boolean;
  displayOrder: number;
  imageFile?: File;
  imagePreview?: string;
}

// Banner management types
export interface BannerFilters {
  isActive?: boolean;
  sortBy?: 'displayOrder' | 'createdAt' | 'title';
  sortDirection?: 'asc' | 'desc';
}

export interface BannerState {
  banners: BannerDTO[];
  activeBanners: BannerDTO[];
  isLoading: boolean;
  error: string | null;
}
