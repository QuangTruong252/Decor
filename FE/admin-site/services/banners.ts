"use client";

import { API_URL, fetchWithAuth, fetchWithAuthFormData } from "@/lib/api-utils";

export interface Banner {
  id: number;
  title: string;
  imageUrl: string;
  link?: string | null;
  isActive: boolean;
  displayOrder?: number;
  isDeleted?: boolean;
  createdAt: string;
}

export interface BannerDTO {
  id: number;
  title?: string | null;
  imageUrl?: string | null;
  link?: string | null;
  isActive: boolean;
  displayOrder?: number;
  createdAt: string;
}

export interface CreateBannerPayload {
  title: string;
  imageFile: File;
  link?: string;
  isActive?: boolean;
  displayOrder?: number;
}

export interface UpdateBannerPayload {
  id: number;
  title?: string;
  imageFile?: File;
  link?: string;
  isActive?: boolean;
  displayOrder?: number;
}

export async function getBanners(): Promise<BannerDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Banner`);
    if (!response.ok) throw new Error("Unable to fetch banners");
    return response.json();
  } catch (error) {
    console.error("Get banners error:", error);
    throw new Error("Unable to fetch banners. Please try again later.");
  }
}

export async function getActiveBanners(): Promise<BannerDTO[]> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Banner/active`);
    if (!response.ok) throw new Error("Unable to fetch active banners");
    return response.json();
  } catch (error) {
    console.error("Get active banners error:", error);
    throw new Error("Unable to fetch active banners. Please try again later.");
  }
}

export async function getBannerById(id: number): Promise<BannerDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Banner/${id}`);
    if (!response.ok) throw new Error("Unable to fetch banner details");
    return response.json();
  } catch (error) {
    console.error(`Get banner by id ${id} error:`, error);
    throw new Error("Unable to fetch banner details. Please try again later.");
  }
}

export async function createBanner(payload: CreateBannerPayload): Promise<Banner> {
  try {
    const formData = new FormData();
    formData.append("Title", payload.title);
    formData.append("ImageFile", payload.imageFile);
    if (payload.link) formData.append("Link", payload.link);
    if (payload.isActive !== undefined) formData.append("IsActive", String(payload.isActive));
    if (payload.displayOrder !== undefined) formData.append("DisplayOrder", String(payload.displayOrder));

    const response = await fetchWithAuthFormData(`${API_URL}/api/Banner`, formData);

    if (!response.ok) throw new Error("Unable to create banner");
    return response.json();
  } catch (error) {
    console.error("Create banner error:", error);
    throw new Error("Unable to create banner. Please try again later.");
  }
}

export async function updateBanner(id: number, payload: UpdateBannerPayload): Promise<void> {
  try {
    const formData = new FormData();
    if (payload.title) formData.append("Title", payload.title);
    if (payload.imageFile) formData.append("ImageFile", payload.imageFile);
    if (payload.link) formData.append("Link", payload.link);
    if (payload.isActive !== undefined) formData.append("IsActive", String(payload.isActive));
    if (payload.displayOrder !== undefined) formData.append("DisplayOrder", String(payload.displayOrder));

    const response = await fetchWithAuthFormData(`${API_URL}/api/Banner/${id}`, formData, "PUT");

    if (!response.ok) throw new Error("Unable to update banner");
  } catch (error) {
    console.error(`Update banner ${id} error:`, error);
    throw new Error("Unable to update banner. Please try again later.");
  }
}

export async function deleteBanner(id: number): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Banner/${id}`, {
      method: "DELETE",
    });

    if (!response.ok) throw new Error("Unable to delete banner");
  } catch (error) {
    console.error(`Delete banner ${id} error:`, error);
    throw new Error("Unable to delete banner. Please try again later.");
  }
}
