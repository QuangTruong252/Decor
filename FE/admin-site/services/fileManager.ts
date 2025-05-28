/**
 * File Manager API Service
 */

import { API_URL, fetchWithAuth, fetchWithAuthFormData } from "@/lib/api-utils";
import {
  BrowseResponse,
  BrowseParams,
  FolderStructure,
  FileItem,
  UploadResponse,
  CreateFolderRequest,
  DeleteRequest,
  DeleteResponse,
  MoveRequest,
  MoveResponse,
  ImageMetadata,
} from "@/types/fileManager";

const BASE_URL = `${API_URL}/api/filemanager`;

export const fileManagerService = {
  /**
   * Health check
   */
  async healthCheck(): Promise<{ status: string; uploadsPath: string }> {
    const response = await fetchWithAuth(`${BASE_URL}/health`);
    if (!response.ok) {
      throw new Error("Failed to check health");
    }
    return response.json();
  },

  /**
   * Browse files and folders
   */
  async browse(params: BrowseParams = {}): Promise<BrowseResponse> {
    const searchParams = new URLSearchParams();
    
    if (params.path) searchParams.append("path", params.path);
    if (params.page) searchParams.append("page", params.page.toString());
    if (params.pageSize) searchParams.append("pageSize", params.pageSize.toString());
    if (params.search) searchParams.append("search", params.search);
    if (params.fileType && params.fileType !== "all") searchParams.append("fileType", params.fileType);
    if (params.extension) searchParams.append("extension", params.extension);
    if (params.sortBy) searchParams.append("sortBy", params.sortBy);
    if (params.sortOrder) searchParams.append("sortOrder", params.sortOrder);
    if (params.minSize) searchParams.append("minSize", params.minSize.toString());
    if (params.maxSize) searchParams.append("maxSize", params.maxSize.toString());
    if (params.fromDate) searchParams.append("fromDate", params.fromDate);
    if (params.toDate) searchParams.append("toDate", params.toDate);

    const url = `${BASE_URL}/browse${searchParams.toString() ? `?${searchParams.toString()}` : ""}`;
    const response = await fetchWithAuth(url);
    
    if (!response.ok) {
      throw new Error("Failed to browse files");
    }
    
    return response.json();
  },

  /**
   * Get folder structure
   */
  async getFolderStructure(rootPath: string = ""): Promise<FolderStructure> {
    const searchParams = new URLSearchParams();
    if (rootPath) searchParams.append("rootPath", rootPath);

    const url = `${BASE_URL}/folders${searchParams.toString() ? `?${searchParams.toString()}` : ""}`;
    const response = await fetchWithAuth(url);
    
    if (!response.ok) {
      throw new Error("Failed to get folder structure");
    }
    
    return response.json();
  },

  /**
   * Get file info
   */
  async getFileInfo(filePath: string): Promise<FileItem> {
    const searchParams = new URLSearchParams();
    searchParams.append("filePath", filePath);

    const response = await fetchWithAuth(`${BASE_URL}/info?${searchParams.toString()}`);
    
    if (!response.ok) {
      throw new Error("Failed to get file info");
    }
    
    return response.json();
  },

  /**
   * Upload files
   */
  async uploadFiles(
    files: File[],
    folderPath: string = "",
    createThumbnails: boolean = true,
    overwriteExisting: boolean = false
  ): Promise<UploadResponse> {
    const formData = new FormData();
    
    files.forEach(file => {
      formData.append("files", file);
    });
    
    formData.append("FolderPath", folderPath);
    formData.append("CreateThumbnails", createThumbnails.toString());
    formData.append("OverwriteExisting", overwriteExisting.toString());

    const response = await fetchWithAuthFormData(`${BASE_URL}/upload`, formData, "POST");
    
    if (!response.ok) {
      throw new Error("Failed to upload files");
    }
    
    return response.json();
  },

  /**
   * Create folder
   */
  async createFolder(request: CreateFolderRequest): Promise<FileItem> {
    const response = await fetchWithAuth(`${BASE_URL}/create-folder`, {
      method: "POST",
      body: JSON.stringify(request),
    });
    
    if (!response.ok) {
      throw new Error("Failed to create folder");
    }
    
    return response.json();
  },

  /**
   * Delete files/folders
   */
  async deleteFiles(request: DeleteRequest): Promise<DeleteResponse> {
    const response = await fetchWithAuth(`${BASE_URL}/delete`, {
      method: "DELETE",
      body: JSON.stringify(request),
    });
    
    if (!response.ok) {
      throw new Error("Failed to delete files");
    }
    
    return response.json();
  },

  /**
   * Move files
   */
  async moveFiles(request: MoveRequest): Promise<MoveResponse> {
    const response = await fetchWithAuth(`${BASE_URL}/move`, {
      method: "POST",
      body: JSON.stringify(request),
    });
    
    if (!response.ok) {
      throw new Error("Failed to move files");
    }
    
    return response.json();
  },

  /**
   * Copy files
   */
  async copyFiles(request: MoveRequest): Promise<MoveResponse> {
    const response = await fetchWithAuth(`${BASE_URL}/copy`, {
      method: "POST",
      body: JSON.stringify(request),
    });
    
    if (!response.ok) {
      throw new Error("Failed to copy files");
    }
    
    return response.json();
  },

  /**
   * Generate thumbnail
   */
  async generateThumbnail(imagePath: string): Promise<{ thumbnailUrl: string }> {
    const response = await fetchWithAuth(`${BASE_URL}/generate-thumbnail`, {
      method: "POST",
      body: JSON.stringify(imagePath),
    });
    
    if (!response.ok) {
      throw new Error("Failed to generate thumbnail");
    }
    
    return response.json();
  },

  /**
   * Get image metadata
   */
  async getImageMetadata(imagePath: string): Promise<ImageMetadata> {
    const searchParams = new URLSearchParams();
    searchParams.append("imagePath", imagePath);

    const response = await fetchWithAuth(`${BASE_URL}/metadata?${searchParams.toString()}`);
    
    if (!response.ok) {
      throw new Error("Failed to get image metadata");
    }
    
    return response.json();
  },

  /**
   * Cleanup orphaned files
   */
  async cleanupOrphanedFiles(): Promise<{ cleanedCount: number; message: string }> {
    const response = await fetchWithAuth(`${BASE_URL}/cleanup-orphaned`, {
      method: "POST",
    });
    
    if (!response.ok) {
      throw new Error("Failed to cleanup orphaned files");
    }
    
    return response.json();
  },

  /**
   * Sync database
   */
  async syncDatabase(): Promise<{ syncedCount: number; message: string }> {
    const response = await fetchWithAuth(`${BASE_URL}/sync-database`, {
      method: "POST",
    });
    
    if (!response.ok) {
      throw new Error("Failed to sync database");
    }
    
    return response.json();
  },

  /**
   * Get missing files
   */
  async getMissingFiles(): Promise<{ missingFiles: string[]; count: number }> {
    const response = await fetchWithAuth(`${BASE_URL}/missing-files`);
    
    if (!response.ok) {
      throw new Error("Failed to get missing files");
    }
    
    return response.json();
  },
};

/**
 * Utility functions
 */
export const fileManagerUtils = {
  /**
   * Get file icon based on extension
   */
  getFileIcon(extension: string): string {
    const ext = extension.toLowerCase();
    
    if ([".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"].includes(ext)) {
      return "image";
    }
    if ([".pdf"].includes(ext)) {
      return "file-text";
    }
    if ([".doc", ".docx"].includes(ext)) {
      return "file-text";
    }
    if ([".xls", ".xlsx"].includes(ext)) {
      return "file-spreadsheet";
    }
    if ([".ppt", ".pptx"].includes(ext)) {
      return "presentation";
    }
    if ([".mp4", ".avi", ".mov", ".wmv"].includes(ext)) {
      return "video";
    }
    if ([".mp3", ".wav", ".flac", ".aac"].includes(ext)) {
      return "music";
    }
    if ([".zip", ".rar", ".7z"].includes(ext)) {
      return "archive";
    }
    
    return "file";
  },

  /**
   * Format file size
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return "0 B";
    
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB", "TB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  },

  /**
   * Get breadcrumb from path
   */
  getBreadcrumb(path: string): Array<{ name: string; path: string }> {
    if (!path) return [{ name: "My Drive", path: "" }];
    
    const parts = path.split("/").filter(Boolean);
    const breadcrumb = [{ name: "My Drive", path: "" }];
    
    let currentPath = "";
    parts.forEach(part => {
      currentPath += (currentPath ? "/" : "") + part;
      breadcrumb.push({ name: part, path: currentPath });
    });
    
    return breadcrumb;
  },

  /**
   * Check if file is image
   */
  isImage(extension: string): boolean {
    const imageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];
    return imageExtensions.includes(extension.toLowerCase());
  },

  /**
   * Get file type color
   */
  getFileTypeColor(type: string, extension: string): string {
    if (type === "folder") return "text-blue-500";
    if (type === "image") return "text-green-500";
    
    const ext = extension.toLowerCase();
    if ([".pdf"].includes(ext)) return "text-red-500";
    if ([".doc", ".docx"].includes(ext)) return "text-blue-600";
    if ([".xls", ".xlsx"].includes(ext)) return "text-green-600";
    if ([".ppt", ".pptx"].includes(ext)) return "text-orange-500";
    if ([".mp4", ".avi", ".mov"].includes(ext)) return "text-purple-500";
    if ([".mp3", ".wav", ".flac"].includes(ext)) return "text-pink-500";
    
    return "text-gray-500";
  },
};
