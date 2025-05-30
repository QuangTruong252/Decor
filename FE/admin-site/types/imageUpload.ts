/**
 * Image Upload Types
 */

import { FileItem } from "./fileManager";

// Image upload result from different sources
export interface ImageUploadResult {
  source: 'device' | 'url' | 'system';
  files?: File[];
  urls?: string[];
  systemFiles?: FileItem[];
}

// Props for the main upload button component
export interface ImageUploadButtonProps {
  onImagesSelected: (result: ImageUploadResult) => void;
  multiple?: boolean;
  currentImages?: (File | string)[];
  onRemoveImage?: (index: number) => void;
  acceptedTypes?: string[];
  maxSize?: number;
  aspectRatio?: string;
  className?: string;
  disabled?: boolean;
  label?: string;
}

// Props for the upload dialog
export interface ImageUploadDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onImagesSelected: (result: ImageUploadResult) => void;
  multiple?: boolean;
  acceptedTypes?: string[];
  maxSize?: number;
  aspectRatio?: string;
}

// Tab content props
export interface UploadTabProps {
  onImagesSelected: (result: ImageUploadResult) => void;
  multiple?: boolean;
  acceptedTypes?: string[];
  maxSize?: number;
  aspectRatio?: string;
  filePath?: string;
}

// URL validation result
export interface UrlValidationResult {
  isValid: boolean;
  error?: string;
  imageData?: {
    url: string;
    width?: number;
    height?: number;
    size?: number;
  };
}

// Upload tab types
export type UploadTabType = 'device' | 'url' | 'system';

// Image preview item
export interface ImagePreviewItem {
  id: string;
  source: UploadTabType;
  preview: string;
  name: string;
  size?: number;
  file?: File;
  url?: string;
  systemFile?: FileItem;
}
