// Common types used across the application

export interface ApiResponse<T = any> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginationParams {
  page?: number;
  limit?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
    hasNext: boolean;
    hasPrev: boolean;
  };
}

export interface SearchParams {
  query?: string;
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  featured?: boolean;
}

export interface FilterParams extends SearchParams, PaginationParams {}

export interface BaseEntity {
  id: number;
  createdAt: string;
  updatedAt: string;
}

export interface ImageUpload {
  file: File;
  preview?: string;
}

export interface BulkDeleteRequest {
  ids: number[];
}

export interface BulkDeleteDTO {
  ids: number[];
}

export interface ImageDTO {
  id: number;
  fileName: string | null;
  filePath: string | null;
  altText: string | null;
  createdAt: string;
}

export interface StatusResponse {
  success: boolean;
  message: string;
}

// Error types
export interface ApiError {
  message: string;
  status: number;
  errors?: Record<string, string[]>;
}

export interface ValidationError {
  field: string;
  message: string;
}

// Form states
export interface FormState {
  isLoading: boolean;
  isSubmitting: boolean;
  errors: Record<string, string>;
  isDirty: boolean;
  isValid: boolean;
}

// Generic CRUD operations
export interface CrudOperations<T, CreateT = Partial<T>, UpdateT = Partial<T>> {
  getAll: (params?: FilterParams) => Promise<T[]>;
  getById: (id: number) => Promise<T>;
  create: (data: CreateT) => Promise<T>;
  update: (id: number, data: UpdateT) => Promise<T>;
  delete: (id: number) => Promise<void>;
  bulkDelete?: (ids: number[]) => Promise<void>;
}

// File upload types
export interface FileUploadResponse {
  url: string;
  filename: string;
  size: number;
  mimeType: string;
}

export interface MultipartFormData {
  [key: string]: string | File | File[];
}
