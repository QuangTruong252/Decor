import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatCurrency (value: number) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value)
}

/**
 * Converts a relative image path to a full URL
 *
 * @param path - Relative image path or full URL
 * @returns Full image URL
 */
export function getImageUrl(path: string | null | undefined): string {
  const baseUrl = process.env.NEXT_PUBLIC_IMAGE_BASE_URL || '';

  // If path is null, undefined or empty string, return default image or empty string
  if (!path) {
    return ''; // Or return a default image URL
  }

  // If path is already a full URL (starts with http:// or https://), return as is
  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path;
  }
  // Check duplicate /uploads path
  path.replace('/uploads/uploads', '/uploads')

  // If path starts with /, remove it to avoid duplication with baseUrl
  const normalizedPath = path.startsWith('/') ? path.substring(1) : path;

  // Combine baseUrl and path
  return `${baseUrl}${normalizedPath}`;
}

export function isEmptyString(value: string | null | undefined): boolean {
  return value === null || value === undefined || value.trim() === '';
}


