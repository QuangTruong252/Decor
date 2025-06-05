/**
 * File Manager Hook - Backwards Compatible Wrapper
 * Now uses FileManagerContext for shared state
 */

import { useFileManagerContext } from "@/contexts/FileManagerContext";

/**
 * Backwards compatible hook that wraps the context
 * This ensures existing components continue to work without changes
 */
export const useFileManager = () => {
  return useFileManagerContext();
};

// Re-export the query keys for backwards compatibility
export { fileManagerKeys } from "@/contexts/FileManagerContext";
