/**
 * Excel Import/Export API services
 */

import { API_URL, fetchWithAuth, fetchWithAuthFormData } from "@/lib/api-utils";
import { buildApiUrl, cleanFilters } from "@/lib/query-utils";
import type {
  CategoryExcelImportResultDTO,
  ProductExcelImportResultDTO,
  CustomerExcelImportResultDTO,
  ExcelValidationResultDTO,
  CategoryImportStatisticsDTO,
  ProductImportStatisticsDTO,
  CustomerImportStatisticsDTO,
  ImportOptions
} from '@/types/excel';
import type { CategoryFilters, ProductFilters, CustomerFilters } from '@/types/api';

// Category Excel Services
export const categoryExcelService = {
  // Export template
  async exportTemplate(includeExample: boolean = true): Promise<Blob> {
    try {
      const params = { includeExample };
      const url = buildApiUrl(`${API_URL}/api/Category/export-template`, params);
      const response = await fetchWithAuth(url);

      if (!response.ok) {
        throw new Error("Unable to download category template");
      }

      return response.blob();
    } catch (error) {
      console.error("Category export template error:", error);
      throw new Error("Unable to download category template. Please try again later.");
    }
  },

  // Export data with filters
  async exportData(filters: CategoryFilters = {}, format: string = 'xlsx'): Promise<Blob> {
    try {
      const cleanedFilters = cleanFilters(filters);
      const params = { ...cleanedFilters, format };
      const url = buildApiUrl(`${API_URL}/api/Category/export`, params);
      const response = await fetchWithAuth(url);

      if (!response.ok) {
        throw new Error("Unable to export category data");
      }

      return response.blob();
    } catch (error) {
      console.error("Category export data error:", error);
      throw new Error("Unable to export category data. Please try again later.");
    }
  },

  // Validate import file
  async validateImport(file: File): Promise<ExcelValidationResultDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetchWithAuthFormData(`${API_URL}/api/Category/validate-import`, formData);

      if (!response.ok) {
        throw new Error("Unable to validate category import file");
      }

      return response.json();
    } catch (error) {
      console.error("Category validate import error:", error);
      throw new Error("Unable to validate category import file. Please try again later.");
    }
  },

  // Import data
  async importData(file: File, options: ImportOptions = { validateOnly: false, showStatistics: false }): Promise<CategoryExcelImportResultDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const params = { validateOnly: options.validateOnly };
      const url = buildApiUrl(`${API_URL}/api/Category/import`, params);
      const response = await fetchWithAuthFormData(url, formData);

      if (!response.ok) {
        throw new Error("Unable to import category data");
      }

      return response.json();
    } catch (error) {
      console.error("Category import data error:", error);
      throw new Error("Unable to import category data. Please try again later.");
    }
  },

  // Get import statistics
  async getImportStatistics(file: File): Promise<CategoryImportStatisticsDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetchWithAuthFormData(`${API_URL}/api/Category/import-statistics`, formData);

      if (!response.ok) {
        throw new Error("Unable to get category import statistics");
      }

      return response.json();
    } catch (error) {
      console.error("Category import statistics error:", error);
      throw new Error("Unable to get category import statistics. Please try again later.");
    }
  }
};

// Product Excel Services
export const productExcelService = {
  // Export template
  async exportTemplate(includeExample: boolean = true): Promise<Blob> {
    try {
      const params = { includeExample };
      const url = buildApiUrl(`${API_URL}/api/Products/export-template`, params);
      const response = await fetchWithAuth(url);

      if (!response.ok) {
        throw new Error("Unable to download product template");
      }

      return response.blob();
    } catch (error) {
      console.error("Product export template error:", error);
      throw new Error("Unable to download product template. Please try again later.");
    }
  },

  // Export data with filters
  async exportData(filters: ProductFilters = {}, format: string = 'xlsx'): Promise<Blob> {
    try {
      const cleanedFilters = cleanFilters(filters);
      const params = { ...cleanedFilters, format };
      const url = buildApiUrl(`${API_URL}/api/Products/export`, params);
      const response = await fetchWithAuth(url);

      if (!response.ok) {
        throw new Error("Unable to export product data");
      }

      return response.blob();
    } catch (error) {
      console.error("Product export data error:", error);
      throw new Error("Unable to export product data. Please try again later.");
    }
  },

  // Validate import file
  async validateImport(file: File): Promise<ExcelValidationResultDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetchWithAuthFormData(`${API_URL}/api/Products/validate-import`, formData);

      if (!response.ok) {
        throw new Error("Unable to validate product import file");
      }

      return response.json();
    } catch (error) {
      console.error("Product validate import error:", error);
      throw new Error("Unable to validate product import file. Please try again later.");
    }
  },

  // Import data
  async importData(file: File, options: ImportOptions = { validateOnly: false, showStatistics: false }): Promise<ProductExcelImportResultDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const params = { validateOnly: options.validateOnly };
      const url = buildApiUrl(`${API_URL}/api/Products/import`, params);
      const response = await fetchWithAuthFormData(url, formData);

      if (!response.ok) {
        throw new Error("Unable to import product data");
      }

      return response.json();
    } catch (error) {
      console.error("Product import data error:", error);
      throw new Error("Unable to import product data. Please try again later.");
    }
  },

  // Get import statistics
  async getImportStatistics(file: File): Promise<ProductImportStatisticsDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetchWithAuthFormData(`${API_URL}/api/Products/import-statistics`, formData);

      if (!response.ok) {
        throw new Error("Unable to get product import statistics");
      }

      return response.json();
    } catch (error) {
      console.error("Product import statistics error:", error);
      throw new Error("Unable to get product import statistics. Please try again later.");
    }
  }
};

// Customer Excel Services
export const customerExcelService = {
  // Export template
  async exportTemplate(includeExample: boolean = true): Promise<Blob> {
    try {
      const params = { includeExample };
      const url = buildApiUrl(`${API_URL}/api/Customer/export-template`, params);
      const response = await fetchWithAuth(url);

      if (!response.ok) {
        throw new Error("Unable to download customer template");
      }

      return response.blob();
    } catch (error) {
      console.error("Customer export template error:", error);
      throw new Error("Unable to download customer template. Please try again later.");
    }
  },

  // Export data with filters
  async exportData(filters: CustomerFilters = {}, format: string = 'xlsx'): Promise<Blob> {
    try {
      const cleanedFilters = cleanFilters(filters);
      const params = { ...cleanedFilters, format };
      const url = buildApiUrl(`${API_URL}/api/Customer/export`, params);
      const response = await fetchWithAuth(url);

      if (!response.ok) {
        throw new Error("Unable to export customer data");
      }

      return response.blob();
    } catch (error) {
      console.error("Customer export data error:", error);
      throw new Error("Unable to export customer data. Please try again later.");
    }
  },

  // Validate import file
  async validateImport(file: File): Promise<ExcelValidationResultDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetchWithAuthFormData(`${API_URL}/api/Customer/validate-import`, formData);

      if (!response.ok) {
        throw new Error("Unable to validate customer import file");
      }

      return response.json();
    } catch (error) {
      console.error("Customer validate import error:", error);
      throw new Error("Unable to validate customer import file. Please try again later.");
    }
  },

  // Import data
  async importData(file: File, options: ImportOptions = { validateOnly: false, showStatistics: false }): Promise<CustomerExcelImportResultDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const params = { validateOnly: options.validateOnly };
      const url = buildApiUrl(`${API_URL}/api/Customer/import`, params);
      const response = await fetchWithAuthFormData(url, formData);

      if (!response.ok) {
        throw new Error("Unable to import customer data");
      }

      return response.json();
    } catch (error) {
      console.error("Customer import data error:", error);
      throw new Error("Unable to import customer data. Please try again later.");
    }
  },

  // Get import statistics
  async getImportStatistics(file: File): Promise<CustomerImportStatisticsDTO> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetchWithAuthFormData(`${API_URL}/api/Customer/import-statistics`, formData);

      if (!response.ok) {
        throw new Error("Unable to get customer import statistics");
      }

      return response.json();
    } catch (error) {
      console.error("Customer import statistics error:", error);
      throw new Error("Unable to get customer import statistics. Please try again later.");
    }
  }
};

// Utility functions
export const excelUtils = {
  // Download blob as file
  downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  },

  // Generate filename with timestamp
  generateFilename(prefix: string, extension: string = 'xlsx'): string {
    const timestamp = new Date().toISOString().slice(0, 19).replace(/[:-]/g, '');
    return `${prefix}_${timestamp}.${extension}`;
  },

  // Format file size
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  },

  // Validate file type
  isValidExcelFile(file: File): boolean {
    const validTypes = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
      'application/vnd.ms-excel', // .xls
      'text/csv' // .csv
    ];
    return validTypes.includes(file.type) || file.name.match(/\.(xlsx|xls|csv)$/i) !== null;
  },

  // Validate file size (max 10MB)
  isValidFileSize(file: File, maxSizeInMB: number = 10): boolean {
    const maxSizeInBytes = maxSizeInMB * 1024 * 1024;
    return file.size <= maxSizeInBytes;
  }
};
