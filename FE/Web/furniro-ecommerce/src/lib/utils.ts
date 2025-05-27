import axios from 'axios';
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function getImageUrl(image: string | undefined | null) {
  if (!image) return '/images/product.jpg';
  return image.startsWith('http') ? image : `${process.env.NEXT_PUBLIC_API_IMAGE_URL}${image}`
}
