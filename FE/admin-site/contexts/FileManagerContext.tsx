/**
 * File Manager Context
 */

"use client";

import React, { createContext, useContext, useState, useCallback, ReactNode } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { fileManagerService } from "@/services/fileManager";
import { useToast } from "@/hooks/use-toast";
import { fetchWithAuth } from "@/lib/api-utils";
import {
  BrowseParams,
  FileFilters,
  ViewMode,
  CreateFolderRequest,
  DeleteRequest,
  MoveRequest,
  UploadProgress,
} from "@/types/fileManager";

// Query keys
export const fileManagerKeys = {
  all: ["fileManager"] as const,
  browse: (params: BrowseParams) => [...fileManagerKeys.all, "browse", params] as const,
  folders: (rootPath: string) => [...fileManagerKeys.all, "folders", rootPath] as const,
  fileInfo: (filePath: string) => [...fileManagerKeys.all, "fileInfo", filePath] as const,
};

// Default filters
const defaultFilters: FileFilters = {
  search: "",
  fileType: "all",
  extension: "",
  sortBy: "name",
  sortOrder: "asc",
  dateRange: {},
  sizeRange: {},
};

// Context type definition
interface FileManagerContextType {
  // Data
  browseData: any;
  folderStructure: any;
  rootFolderStructure: any;
  
  // State
  currentPath: string;
  selectedItems: string[];
  viewMode: ViewMode;
  filters: FileFilters;
  uploadProgress: UploadProgress[];
  
  // Loading states
  isLoading: boolean;
  isBrowseLoading: boolean;
  isFoldersLoading: boolean;
  isRootFoldersLoading: boolean;
  isUploading: boolean;
  isCreatingFolder: boolean;
  isDeleting: boolean;
  isMoving: boolean;
  isCopying: boolean;
  
  // Error states
  error: any;
  
  // Actions
  navigateToPath: (path: string) => void;
  navigateUp: () => void;
  selectItem: (relativePath: string) => void;
  selectAll: () => void;
  clearSelection: () => void;
  setViewMode: (mode: ViewMode) => void;
  updateFilters: (newFilters: Partial<FileFilters>) => void;
  resetFilters: () => void;
  uploadFiles: (files: File[], folderPath?: string) => void;
  createFolder: (folderName: string, parentPath?: string) => void;
  deleteSelectedItems: (permanent?: boolean) => void;
  deleteItem: (relativePath: string, permanent?: boolean) => void;
  moveSelectedItems: (destinationPath: string, overwriteExisting?: boolean) => void;
  moveItem: (sourcePath: string, destinationPath: string, overwriteExisting?: boolean) => void;
  copySelectedItems: (destinationPath: string, overwriteExisting?: boolean) => void;
  copyItem: (sourcePath: string, destinationPath: string, overwriteExisting?: boolean) => void;
  downloadFile: (relativePath: string) => void;
  refetchBrowse: () => void;
  refetchFolders: () => void;
  refetchRootFolders: () => void;
  
  // Computed
  hasSelection: boolean;
  selectedCount: number;
  canNavigateUp: boolean;
}

// Create context
const FileManagerContext = createContext<FileManagerContextType | null>(null);

// Provider component
export const FileManagerProvider = ({ children }: { children: ReactNode }) => {
  const toast = useToast();
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const isAuthenticated = !!session;

  // State
  const [currentPath, setCurrentPath] = useState<string>("");
  const [selectedItems, setSelectedItems] = useState<string[]>([]);
  const [viewMode, setViewMode] = useState<ViewMode>("grid");
  const [filters, setFilters] = useState<FileFilters>(defaultFilters);
  const [uploadProgress, setUploadProgress] = useState<UploadProgress[]>([]);

  // Browse files query
  const browseParams: BrowseParams = {
    path: currentPath,
    page: 1,
    pageSize: 20,
    search: filters.search || undefined,
    fileType: filters.fileType !== "all" ? filters.fileType : undefined,
    extension: filters.extension || undefined,
    sortBy: filters.sortBy,
    sortOrder: filters.sortOrder,
    minSize: filters.sizeRange.min,
    maxSize: filters.sizeRange.max,
    fromDate: filters.dateRange.from,
    toDate: filters.dateRange.to,
  };

  const {
    data: browseData,
    isLoading: isBrowseLoading,
    error: browseError,
    refetch: refetchBrowse,
  } = useQuery({
    queryKey: fileManagerKeys.browse(browseParams),
    queryFn: () => fileManagerService.browse(browseParams),
    staleTime: 30000, // 30 seconds
    enabled: isAuthenticated, // Only fetch if authenticated
  });

  // Folder structure query
  const {
    data: folderStructure,
    isLoading: isFoldersLoading,
    refetch: refetchFolders,
  } = useQuery({
    queryKey: fileManagerKeys.folders(currentPath),
    queryFn: () => fileManagerService.getFolderStructure(currentPath),
    staleTime: 60000, // 1 minute
    enabled: isAuthenticated, // Only fetch if authenticated
  });

  // Root folder structure query
  const {
    data: rootFolderStructure,
    isLoading: isRootFoldersLoading,
    refetch: refetchRootFolders
  } = useQuery({
    queryKey: fileManagerKeys.folders(""),
    queryFn: () => fileManagerService.getFolderStructure(""),
    staleTime: 60000, // 1 minute
    enabled: isAuthenticated, // Only fetch if authenticated
  });

  // Upload mutation
  const uploadMutation = useMutation({
    mutationFn: async ({
      files,
      folderPath,
      createThumbnails = true,
      overwriteExisting = false,
    }: {
      files: File[];
      folderPath?: string;
      createThumbnails?: boolean;
      overwriteExisting?: boolean;
    }) => {
      // Initialize upload progress
      const initialProgress = files.map(file => ({
        fileName: file.name,
        progress: 0,
        status: "pending" as const,
      }));
      setUploadProgress(initialProgress);

      // Simulate progress updates (in real app, this would come from upload progress)
      let progressStep = 0;
      const progressInterval = setInterval(() => {
        progressStep += 1;
        setUploadProgress(prev =>
          prev.map(item => ({
            ...item,
            progress: Math.min(item.progress + 15 + (progressStep % 3) * 5, 90),
            status: item.progress < 90 ? "uploading" as const : item.status,
          }))
        );
      }, 500);

      try {
        const result = await fileManagerService.uploadFiles(
          files,
          folderPath || currentPath,
          createThumbnails,
          overwriteExisting
        );

        clearInterval(progressInterval);
        
        // Mark as completed
        setUploadProgress(prev => 
          prev.map(item => ({
            ...item,
            progress: 100,
            status: "completed" as const,
          }))
        );

        return result;
      } catch (error) {
        clearInterval(progressInterval);
        
        // Mark as error
        setUploadProgress(prev => 
          prev.map(item => ({
            ...item,
            status: "error" as const,
            error: error instanceof Error ? error.message : "Upload failed",
          }))
        );
        
        throw error;
      }
    },
    onSuccess: (data) => {
      toast.success({
        title: "Upload successful",
        description: `${data.successCount} files uploaded successfully`,
      });
      
      // Invalidate queries
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.browse(browseParams) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders(currentPath) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders("") }); // Refetch root folders
      
      // Clear upload progress after delay
      setTimeout(() => setUploadProgress([]), 3000);
    },
    onError: (error) => {
      toast.error({
        title: "Upload failed",
        description: error instanceof Error ? error.message : "Failed to upload files",
      });
    },
  });

  // Create folder mutation
  const createFolderMutation = useMutation({
    mutationFn: (request: CreateFolderRequest) => fileManagerService.createFolder(request),
    onSuccess: (data) => {
      toast.success({
        title: "Folder created",
        description: `Folder "${data.name}" created successfully`,
      });
      
      // Invalidate queries
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.browse(browseParams) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders(currentPath) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders("") }); // Refetch root folders
    },
    onError: (error) => {
      toast.error({
        title: "Failed to create folder",
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (request: DeleteRequest) => fileManagerService.deleteFiles(request),
    onSuccess: (data) => {
      toast.success({
        title: "Files deleted",
        description: `${data.successCount} items deleted successfully`,
      });
      
      // Clear selection
      setSelectedItems([]);
      
      // Invalidate queries
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.browse(browseParams) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders(currentPath) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders("") }); // Refetch root folders
    },
    onError: (error) => {
      toast.error({
        title: "Failed to delete files",
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });

  // Move mutation
  const moveMutation = useMutation({
    mutationFn: (request: MoveRequest) => fileManagerService.moveFiles(request),
    onSuccess: (data) => {
      toast.success({
        title: "Files moved",
        description: `${data.successCount} items moved successfully`,
      });
      
      // Clear selection
      setSelectedItems([]);
      
      // Invalidate queries
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.all });
    },
    onError: (error) => {
      toast.error({
        title: "Failed to move files",
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });

  // Copy mutation
  const copyMutation = useMutation({
    mutationFn: (request: MoveRequest) => fileManagerService.copyFiles(request),
    onSuccess: (data) => {
      toast.success({
        title: "Files copied",
        description: `${data.successCount} items copied successfully`,
      });
      
      // Clear selection
      setSelectedItems([]);
      
      // Invalidate queries
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.browse(browseParams) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders(currentPath) });
      queryClient.invalidateQueries({ queryKey: fileManagerKeys.folders("") }); // Refetch root folders
    },
    onError: (error) => {
      toast.error({
        title: "Failed to copy files",
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });

  // Actions
  const navigateToPath = useCallback((path: string) => {
    setCurrentPath(path);
    setSelectedItems([]);
  }, []);

  const navigateUp = useCallback(() => {
    if (currentPath) {
      const parentPath = currentPath.split("/").slice(0, -1).join("/");
      navigateToPath(parentPath);
    }
  }, [currentPath, navigateToPath]);

  const selectItem = useCallback((relativePath: string) => {
    setSelectedItems(prevSelected => {
      if (prevSelected.includes(relativePath)) {
        return prevSelected.filter(item => item !== relativePath);
      }
      return [...prevSelected, relativePath];
    });
  }, []);

  const selectAll = useCallback(() => {
    if (browseData?.items) {
      const allPaths = browseData.items.map(item => item.relativePath);
      setSelectedItems(allPaths);
    }
  }, [browseData?.items]);

  const clearSelection = useCallback(() => {
    setSelectedItems([]);
  }, []);

  const updateFilters = useCallback((newFilters: Partial<FileFilters>) => {
    setFilters(prev => ({ ...prev, ...newFilters }));
  }, []);

  const resetFilters = useCallback(() => {
    setFilters(defaultFilters);
  }, []);

  const uploadFiles = useCallback((files: File[], folderPath?: string) => {
    uploadMutation.mutate({ files, folderPath });
  }, [uploadMutation]);

  const createFolder = useCallback((folderName: string, parentPath?: string) => {
    createFolderMutation.mutate({
      parentPath: parentPath || currentPath,
      folderName,
    });
  }, [createFolderMutation, currentPath]);

  const deleteSelectedItems = useCallback((permanent = false) => {
    setSelectedItems(currentItems => {
      
      if (currentItems.length > 0) {
        deleteMutation.mutate({
          filePaths: currentItems,
          permanent,
        });
      }
      
      return currentItems; // Keep items until mutation succeeds
    });
  }, [deleteMutation]);

  const deleteItem = useCallback((relativePath: string, permanent = false) => {
    if (relativePath) {
      deleteMutation.mutate({
        filePaths: [relativePath],
        permanent,
      });
    }
  }, [deleteMutation]);

  const moveSelectedItems = useCallback((destinationPath: string, overwriteExisting = false) => {
    setSelectedItems(currentItems => {
      if (currentItems.length > 0) {
        moveMutation.mutate({
          sourcePaths: currentItems,
          destinationPath,
          overwriteExisting,
        });
      }
      return currentItems;
    });
  }, [moveMutation]);

  const moveItem = useCallback((sourcePath: string, destinationPath: string, overwriteExisting = false) => {
    if (sourcePath) {
      moveMutation.mutate({
        sourcePaths: [sourcePath],
        destinationPath,
        overwriteExisting,
      });
    }
  }, [moveMutation]);

  const copySelectedItems = useCallback((destinationPath: string, overwriteExisting = false) => {
    setSelectedItems(currentItems => {
      if (currentItems.length > 0) {
        copyMutation.mutate({
          sourcePaths: currentItems,
          destinationPath,
          overwriteExisting,
        });
      }
      return currentItems;
    });
  }, [copyMutation]);

  const copyItem = useCallback((sourcePath: string, destinationPath: string, overwriteExisting = false) => {
    if (sourcePath) {
      copyMutation.mutate({
        sourcePaths: [sourcePath],
        destinationPath,
        overwriteExisting,
      });
    }
  }, [copyMutation]);

  // Download file action
  const downloadFile = useCallback(async (relativePath: string) => {
    try {
      const fileName = relativePath.split('/').pop() || 'download';

      toast.success({
        title: "Download Started",
        description: `Downloading ${fileName}...`,
      });

      // Use fetchWithAuth to download the file with authentication
      const downloadUrl = fileManagerService.getDownloadUrl(relativePath);
      const response = await fetchWithAuth(downloadUrl);
      
      if (!response.ok) {
        throw new Error(`Download failed: ${response.status} ${response.statusText}`);
      }

      // Get the file blob
      const blob = await response.blob();
      
      // Create blob URL and trigger download
      const blobUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = blobUrl;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      // Clean up the blob URL
      window.URL.revokeObjectURL(blobUrl);

      toast.success({
        title: "Download Complete",
        description: `${fileName} downloaded successfully.`,
      });
    } catch (error) {
      toast.error({
        title: "Download Failed",
        description: error instanceof Error ? error.message : "Could not download file.",
      });
      console.error("Error downloading file:", error);
    }
  }, [toast]);

  // Context value
  const value: FileManagerContextType = {
    // Data
    browseData,
    folderStructure,
    rootFolderStructure,
    
    // State
    currentPath,
    selectedItems,
    viewMode,
    filters,
    uploadProgress,
    
    // Loading states
    isLoading: isBrowseLoading || isFoldersLoading || isRootFoldersLoading,
    isBrowseLoading,
    isFoldersLoading,
    isRootFoldersLoading,
    isUploading: uploadMutation.isPending,
    isCreatingFolder: createFolderMutation.isPending,
    isDeleting: deleteMutation.isPending,
    isMoving: moveMutation.isPending,
    isCopying: copyMutation.isPending,
    
    // Error states
    error: browseError,
    
    // Actions
    navigateToPath,
    navigateUp,
    selectItem,
    selectAll,
    clearSelection,
    setViewMode,
    updateFilters,
    resetFilters,
    uploadFiles,
    createFolder,
    deleteSelectedItems,
    deleteItem,
    moveSelectedItems,
    moveItem,
    copySelectedItems,
    copyItem,
    downloadFile,
    refetchBrowse,
    refetchFolders,
    refetchRootFolders,
    
    // Computed
    hasSelection: selectedItems.length > 0,
    selectedCount: selectedItems.length,
    canNavigateUp: !!currentPath,
  };

  return (
    <FileManagerContext.Provider value={value}>
      {children}
    </FileManagerContext.Provider>
  );
};

// Hook to use the context
export const useFileManagerContext = () => {
  const context = useContext(FileManagerContext);
  if (!context) {
    throw new Error('useFileManagerContext must be used within a FileManagerProvider');
  }
  return context;
};
