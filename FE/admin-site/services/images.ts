/**
 * Unified Image Upload Service
 * Handles image uploads from multiple sources with consistent API
 */

import { ImageUploadResult } from "@/types/imageUpload";
import { fetchWithAuthFormData, fetchWithAuth } from "@/lib/api-utils";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5001";

export interface ImageProcessResult {
  success: boolean;
  imageIds?: number[];
  error?: string;
}

export interface ImageUploadOptions {
  folderPath?: string;
  createThumbnails?: boolean;
  overwriteExisting?: boolean;
}

class ImageService {
  /**
   * Process images from ImageUploadResult and return backend image IDs
   */
  async processImageUpload(
    result: ImageUploadResult,
    options: ImageUploadOptions = {}
  ): Promise<ImageProcessResult> {
    const { folderPath = "images" } = options;

    try {
      switch (result.source) {
        case "device":
          return await this.handleDeviceUpload(result.files || [], folderPath);

        case "url":
          return await this.handleUrlUpload(result.urls || [], folderPath);

        case "system":
          return await this.handleSystemFileSelection(result.systemFiles || []);

        default:
          return { success: false, error: "Invalid upload source" };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : "Unknown error occurred"
      };
    }
  }

  /**
   * Handle device file uploads using the backend Image API
   */
  private async handleDeviceUpload(
    files: File[],
    folderPath: string
  ): Promise<ImageProcessResult> {
    if (files.length === 0) {
      return { success: false, error: "No files provided" };
    }

    try {
      const formData = new FormData();
      files.forEach(file => {
        formData.append("Files", file);
      });
      formData.append("folderName", folderPath);

      const response = await fetchWithAuthFormData(`${API_URL}/api/Image/upload`, formData);

      if (!response.ok) {
        throw new Error("Failed to upload images");
      }

      const result = await response.json();
      const imageIds = result.images?.map((img: any) => img.id) || [];

      return {
        success: imageIds.length > 0,
        imageIds: imageIds.length > 0 ? imageIds : undefined,
        error: imageIds.length === 0 ? "No images were uploaded successfully" : undefined
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : "Upload failed"
      };
    }
  }

  /**
   * Handle URL image uploads
   */
  private async handleUrlUpload(urls: string[], folderPath: string): Promise<ImageProcessResult> {
    if (urls.length === 0) {
      return { success: false, error: "No URLs provided" };
    }

    try {
      const imageIds: number[] = [];

      for (const url of urls) {
        const imageId = await this.uploadFromUrl(url, folderPath);
        if (imageId) {
          imageIds.push(imageId);
        }
      }

      return {
        success: imageIds.length > 0,
        imageIds: imageIds.length > 0 ? imageIds : undefined,
        error: imageIds.length === 0 ? "Failed to upload any images from URLs" : undefined
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : "URL upload failed"
      };
    }
  }

  /**
   * Get image IDs by file paths using the new API endpoint
   */
  async getImageIdsByFilePaths(filePaths: string[]): Promise<number[]> {
    try {
      if (filePaths.length === 0) {
        return [];
      }

      const queryParams = filePaths.map(path => `filePaths=${encodeURIComponent(path)}`).join('&');
      const response = await fetchWithAuth(`${API_URL}/api/Image/get-by-filepaths?${queryParams}`);
      
      if (!response.ok) {
        throw new Error("Failed to get images by file paths");
      }
      
      const result = await response.json(); // ImageUploadResponseDTO
      return result.images?.map((img: any) => img.id) || [];
    } catch (error) {
      console.error("Error getting image IDs by paths:", error);
      return [];
    }
  }

  /**
   * Handle system file selection - get IDs by file paths from the backend
   */
  private async handleSystemFileSelection(systemFiles: any[]): Promise<ImageProcessResult> {
    if (systemFiles.length === 0) {
      return { success: false, error: "No system files selected" };
    }

    try {
      const filePaths = systemFiles.map(file => file.relativePath || file.path);
      const imageIds = await this.getImageIdsByFilePaths(filePaths);
      
      return {
        success: imageIds.length > 0,
        imageIds: imageIds.length > 0 ? imageIds : undefined,
        error: imageIds.length === 0 ? "No valid system files found" : undefined
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : "Failed to process system files"
      };
    }
  }

  /**
   * Upload image from URL by downloading it first
   */
  private async uploadFromUrl(url: string, folderPath: string): Promise<number | null> {
    try {
      const response = await fetch(url);
      if (!response.ok) {
        throw new Error(`Failed to fetch image from URL: ${response.statusText}`);
      }

      const blob = await response.blob();
      const filename = this.extractFilenameFromUrl(url);
      const file = new File([blob], filename, { type: blob.type });

      const uploadResult = await this.handleDeviceUpload([file], folderPath);
      
      if (uploadResult.success && uploadResult.imageIds && uploadResult.imageIds.length > 0) {
        return uploadResult.imageIds[0];
      }

      return null;
    } catch (error) {
      throw new Error(`Failed to upload from URL ${url}: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  }

  /**
   * Extract filename from URL
   */
  private extractFilenameFromUrl(url: string): string {
    try {
      const urlObj = new URL(url);
      const pathname = urlObj.pathname;
      const filename = pathname.split('/').pop() || 'image';
      
      if (!filename.includes('.')) {
        return `${filename}.jpg`;
      }
      
      return filename;
    } catch {
      return `image_${Date.now()}.jpg`;
    }
  }

  /**
   * Upload images directly and return their IDs
   * This is a simplified method for direct file uploads
   */
  async uploadImages(files: File[], folderName: string = "images"): Promise<ImageProcessResult> {
    return this.handleDeviceUpload(files, folderName);
  }

  /**
   * Validate image upload result
   */
  validateUploadResult(result: ImageUploadResult): { isValid: boolean; error?: string } {
    if (!result.source) {
      return { isValid: false, error: "Upload source is required" };
    }

    switch (result.source) {
      case "device":
        if (!result.files || result.files.length === 0) {
          return { isValid: false, error: "No files selected for device upload" };
        }
        break;

      case "url":
        if (!result.urls || result.urls.length === 0) {
          return { isValid: false, error: "No URLs provided for URL upload" };
        }
        break;

      case "system":
        if (!result.systemFiles || result.systemFiles.length === 0) {
          return { isValid: false, error: "No system files selected" };
        }
        break;

      default:
        return { isValid: false, error: "Invalid upload source" };
    }

    return { isValid: true };
  }
}

export const imageService = new ImageService();
