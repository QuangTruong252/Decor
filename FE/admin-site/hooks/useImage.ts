"use client";

import { getImageUrl } from "@/lib/utils";
import { useMemo } from "react";

/**
 * Hook for handling image URLs in React components
 *
 * @returns Utility functions for working with image URLs
 */
export function useImage() {
  // Use useMemo to avoid recreating functions on each render
  const utils = useMemo(() => {
    return {
      /**
       * Converts a relative image path to a full URL
       *
       * @param path - Relative image path or full URL
       * @returns Full image URL
       */
      getUrl: (path: string | null | undefined): string => {
        return getImageUrl(path);
      },

      /**
       * Creates a src object for img tag or Next.js Image component
       *
       * @param path - Relative image path or full URL
       * @returns Object with src property containing the full URL
       */
      getSrc: (path: string | null | undefined) => {
        return { src: getImageUrl(path) };
      },

      /**
       * Checks if a path is a full URL
       *
       * @param path - Path to check
       * @returns true if it's a full URL, false otherwise
       */
      isFullUrl: (path: string | null | undefined): boolean => {
        if (!path) return false;
        return path.startsWith('http://') || path.startsWith('https://');
      }
    };
  }, []);

  return utils;
}
