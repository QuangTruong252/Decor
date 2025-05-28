/**
 * Types for Excel Import/Export functionality
 */

// Excel Import/Export types
export interface CategoryExcelDTO {
  id?: number;
  name: string;
  slug: string;
  description?: string;
  parentId?: number;
  parentName?: string;
  imageUrl?: string;
  sortOrder: number;
  createdAt?: string;
  productCount: number;
  subcategoryCount: number;
  level: number;
  categoryPath?: string;
  isActive: boolean;
  totalRevenue: number;
  averageProductPrice: number;
  validationErrors?: string[];
  rowNumber: number;
  hasErrors: boolean;
}

export interface ProductExcelDTO {
  id?: number;
  name: string;
  slug: string;
  description?: string;
  price: number;
  originalPrice: number;
  stockQuantity: number;
  sku: string;
  categoryId: number;
  categoryName?: string;
  isFeatured: boolean;
  isActive: boolean;
  averageRating: number;
  createdAt?: string;
  updatedAt?: string;
  imageUrls?: string;
  reviewCount: number;
  totalSales: number;
  revenue: number;
  lastSaleDate?: string;
  validationErrors?: string[];
  rowNumber: number;
  hasErrors: boolean;
}

export interface CustomerExcelDTO {
  id?: number;
  firstName: string;
  lastName: string;
  email: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  phone?: string;
  dateOfBirth?: string;
  createdAt?: string;
  updatedAt?: string;
  orderCount: number;
  totalSpent: number;
  averageOrderValue: number;
  lastOrderDate?: string;
  lifetimeValue: number;
  customerStatus?: string;
  customerSegment?: string;
  validationErrors?: string[];
  rowNumber: number;
  hasErrors: boolean;
}

export interface ExcelImportResultDTO<T> {
  data?: T[];
  errors?: ExcelValidationErrorDTO[];
  totalRows: number;
  successfulRows: number;
  errorRows: number;
  isSuccess: boolean;
  summary?: string;
  processingTime: string;
  metadata?: Record<string, any>;
}

export interface ExcelValidationErrorDTO {
  rowNumber: number;
  columnName?: string;
  invalidValue?: string;
  errorMessage?: string;
  errorCode?: string;
  severity: ExcelErrorSeverity;
  propertyName?: string;
  suggestedFix?: string;
  context?: Record<string, any>;
}

export interface ExcelValidationResultDTO {
  isValid: boolean;
  errors?: string[];
  warnings?: string[];
  fileInfo: ExcelFileInfoDTO;
  detectedColumns?: string[];
  missingColumns?: string[];
  extraColumns?: string[];
}

export interface ExcelFileInfoDTO {
  fileSizeBytes: number;
  worksheetCount: number;
  rowCount: number;
  columnCount: number;
  worksheetNames?: string[];
  fileFormat?: string;
}

export interface CategoryImportStatisticsDTO {
  totalRows: number;
  newCategories: number;
  updatedCategories: number;
  errorRows: number;
  rootCategories: number;
  subcategories: number;
  maxHierarchyDepth: number;
  parentCategories?: string[];
  duplicateNames?: string[];
  circularReferences?: string[];
  estimatedProcessingTime: string;
  fileSizeBytes: number;
}

export interface ProductImportStatisticsDTO {
  totalRows: number;
  newProducts: number;
  updatedProducts: number;
  errorRows: number;
  categories?: string[];
  duplicateSkus?: string[];
  estimatedProcessingTime: string;
  fileSizeBytes: number;
}

export interface CustomerImportStatisticsDTO {
  totalRows: number;
  newCustomers: number;
  updatedCustomers: number;
  errorRows: number;
  countries?: string[];
  states?: string[];
  cities?: string[];
  duplicateEmails?: string[];
  invalidEmails?: string[];
  invalidPhones?: string[];
  geographicDistribution?: Record<string, number>;
  estimatedProcessingTime: string;
  fileSizeBytes: number;
}

export enum ExcelErrorSeverity {
  Warning = 1,
  Error = 2,
  Critical = 3
}

// Type aliases for convenience
export type CategoryExcelImportResultDTO = ExcelImportResultDTO<CategoryExcelDTO>;
export type ProductExcelImportResultDTO = ExcelImportResultDTO<ProductExcelDTO>;
export type CustomerExcelImportResultDTO = ExcelImportResultDTO<CustomerExcelDTO>;

// Export options
export interface ExportOptions {
  format: 'xlsx' | 'csv';
  includeFilters: boolean;
  includeExample?: boolean;
}

// Import options
export interface ImportOptions {
  validateOnly: boolean;
  showStatistics: boolean;
}

// File upload state
export interface FileUploadState {
  file: File | null;
  isUploading: boolean;
  progress: number;
  error?: string;
}

// Import process state
export interface ImportProcessState {
  step: 'upload' | 'validate' | 'preview' | 'import' | 'complete';
  isProcessing: boolean;
  validationResult?: ExcelValidationResultDTO;
  importResult?: ExcelImportResultDTO<any>;
  statistics?: CategoryImportStatisticsDTO | ProductImportStatisticsDTO | CustomerImportStatisticsDTO;
  error?: string;
}

// Export process state
export interface ExportProcessState {
  isExporting: boolean;
  progress: number;
  error?: string;
}
