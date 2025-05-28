/**
 * File Manager Types
 */

// File/Folder item from API
export interface FileItem {
  name: string;
  path: string;
  relativePath: string;
  type: "file" | "folder" | "image";
  size: number;
  formattedSize: string;
  createdAt: string;
  modifiedAt: string;
  extension?: string;
  fullUrl?: string;
  thumbnailUrl?: string;
  metadata?: ImageMetadata;
}

// Image metadata
export interface ImageMetadata {
  width: number;
  height: number;
  format: string;
  aspectRatio: number;
  colorSpace: string;
}

// Browse response from API
export interface BrowseResponse {
  currentPath: string;
  parentPath: string;
  items: FileItem[];
  totalItems: number;
  totalFiles: number;
  totalFolders: number;
  totalSize: number;
  formattedTotalSize: string;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Folder structure
export interface FolderStructure {
  name: string;
  path: string;
  relativePath: string;
  fileCount: number;
  folderCount: number;
  totalItems: number;
  totalSize: number;
  formattedSize: string;
  subfolders: FolderStructure[];
  hasChildren: boolean;
}

// Browse parameters
export interface BrowseParams {
  path?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  fileType?: "all" | "image" | "folder";
  extension?: string;
  sortBy?: "name" | "size" | "date" | "type";
  sortOrder?: "asc" | "desc";
  minSize?: number;
  maxSize?: number;
  fromDate?: string;
  toDate?: string;
}

// Upload response
export interface UploadResponse {
  uploadedFiles: FileItem[];
  errors: string[];
  successCount: number;
  errorCount: number;
  totalSize: number;
  formattedTotalSize: string;
}

// Create folder request
export interface CreateFolderRequest {
  parentPath: string;
  folderName: string;
}

// Delete request
export interface DeleteRequest {
  filePaths: string[];
  permanent?: boolean;
}

// Delete response
export interface DeleteResponse {
  deletedFiles: string[];
  errors: string[];
  successCount: number;
  errorCount: number;
}

// Move/Copy request
export interface MoveRequest {
  sourcePaths: string[];
  destinationPath: string;
  overwriteExisting?: boolean;
}

// Move/Copy response
export interface MoveResponse {
  processedFiles: string[];
  errors: string[];
  successCount: number;
  errorCount: number;
  operation: "Move" | "Copy";
}

// File manager state
export interface FileManagerState {
  currentPath: string;
  selectedItems: string[];
  viewMode: "grid" | "list";
  isLoading: boolean;
  error: string | null;
}

// Filter state
export interface FileFilters {
  search: string;
  fileType: "all" | "image" | "folder";
  extension: string;
  sortBy: "name" | "size" | "date" | "type";
  sortOrder: "asc" | "desc";
  dateRange: {
    from?: string;
    to?: string;
  };
  sizeRange: {
    min?: number;
    max?: number;
  };
}

// View modes
export type ViewMode = "grid" | "list";

// Sort options
export const SORT_OPTIONS = [
  { value: "name", label: "Name" },
  { value: "size", label: "Size" },
  { value: "date", label: "Date Modified" },
  { value: "type", label: "Type" },
] as const;

// File type options
export const FILE_TYPE_OPTIONS = [
  { value: "all", label: "All Files" },
  { value: "image", label: "Images" },
  { value: "folder", label: "Folders" },
] as const;

// File extension options
export const EXTENSION_OPTIONS = [
  { value: "all", label: "All Extensions" },
  { value: ".jpg", label: "JPEG" },
  { value: ".png", label: "PNG" },
  { value: ".gif", label: "GIF" },
  { value: ".webp", label: "WebP" },
  { value: ".pdf", label: "PDF" },
  { value: ".doc", label: "Word" },
  { value: ".xlsx", label: "Excel" },
] as const;

// Context menu actions
export interface ContextMenuAction {
  id: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  action: (items: string[]) => void;
  disabled?: boolean;
  destructive?: boolean;
}

// Upload progress
export interface UploadProgress {
  fileName: string;
  progress: number;
  status: "pending" | "uploading" | "completed" | "error";
  error?: string;
}
