/**
 * System Files Upload Tab Component
 * Handles image selection from existing files in the system
 */

"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Skeleton } from "@/components/ui/skeleton";
import { 
  Search, 
  AlertCircle, 
  Image as ImageIcon, 
  Folder,
  Check
} from "lucide-react";
import { cn, getImageUrl } from "@/lib/utils";
import { UploadTabProps, ImageUploadResult, ImagePreviewItem } from "@/types/imageUpload";
import { FileItem } from "@/types/fileManager";
import { useFileManager } from "@/hooks/useFileManager";
import { FolderTree } from "@/components/files/FolderTree";
import Image from "next/image";

export const SystemFilesTab = ({
  onImagesSelected,
  multiple = true,
}: UploadTabProps) => {
  const [selectedImages, setSelectedImages] = useState<ImagePreviewItem[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  
  const {
    browseData,
    rootFolderStructure,
    currentPath,
    isBrowseLoading,
    isRootFoldersLoading,
    error,
    navigateToPath,
    updateFilters,
  } = useFileManager();

  // Filter for images only and apply search
  useEffect(() => {
    updateFilters({
      search: searchQuery,
      fileType: "image",
      extension: "",
      sortBy: "name",
      sortOrder: "asc",
      dateRange: {},
      sizeRange: {},
    });
  }, [searchQuery, updateFilters]);

  const handleFileClick = (file: FileItem) => {
    if (file.type === "folder") {
      navigateToPath(file.relativePath);
      return;
    }

    if (file.type === "image" || file.type === "file") {
      const imagePreview: ImagePreviewItem = {
        id: Math.random().toString(36).substr(2, 9),
        source: "system",
        preview: getImageUrl(file.relativePath) || file.fullUrl || '',
        name: file.name,
        size: file.size,
        systemFile: file,
      };

      if (multiple) {
        // Check if already selected
        const isSelected = selectedImages.some(img => img.systemFile?.relativePath === file.relativePath);
        if (isSelected) {
          setSelectedImages(prev => prev.filter(img => img.systemFile?.relativePath !== file.relativePath));
        } else {
          setSelectedImages(prev => [...prev, imagePreview]);
        }
      } else {
        setSelectedImages([imagePreview]);
      }
    }
  };

  const handleSelectImages = () => {
    if (selectedImages.length > 0) {
      const result: ImageUploadResult = {
        source: "system",
        systemFiles: selectedImages.map(img => img.systemFile!),
      };
      onImagesSelected(result);
    }
  };

  const handleClearAll = () => {
    setSelectedImages([]);
  };

  const isFileSelected = (file: FileItem): boolean => {
    return selectedImages.some(img => img.systemFile?.relativePath === file.relativePath);
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return "0 B";
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric' 
    });
  };

  if (error) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          Failed to load files: {error instanceof Error ? error.message : "Unknown error"}
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="flex h-full max-h-[500px] border rounded-2xl">
      {/* Sidebar - Folder Tree */}
      <div className="w-48 border-r bg-muted/30 flex flex-col">
        <div className="p-3 border-b">
          <h3 className="text-sm font-medium text-muted-foreground mb-3">FOLDERS</h3>
          {isRootFoldersLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-6 w-full" />
              ))}
            </div>
          ) : (
            <FolderTree
              folderStructure={rootFolderStructure}
              currentPath={currentPath}
              onNavigateToPath={navigateToPath}
            />
          )}
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Search Bar */}
        <div className="p-4 border-b">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search images..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        {/* Current Path */}
        <div className="px-4 py-2 bg-muted/20 border-b">
          <div className="text-sm text-muted-foreground">
            Current folder: <span className="font-medium">{currentPath || "Root"}</span>
          </div>
        </div>

        {/* Images Grid */}
        <div className="flex-1 overflow-auto p-4">
          {isBrowseLoading ? (
            <div className="grid grid-cols-2 gap-4">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="space-y-2">
                  <Skeleton className="aspect-square w-full rounded-lg" />
                  <Skeleton className="h-4 w-3/4" />
                  <Skeleton className="h-3 w-1/2" />
                </div>
              ))}
            </div>
          ) : browseData?.items.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-64 text-center">
              <ImageIcon className="h-16 w-16 mx-auto mb-4 text-muted-foreground opacity-50" />
              <p className="text-muted-foreground mb-2">No images found in this folder</p>
              {searchQuery && <p className="text-sm text-muted-foreground">Try adjusting your search</p>}
            </div>
          ) : (
            <div className="grid grid-cols-2 gap-4">
              {browseData?.items.map((file: any) => (
                <div
                  key={file.relativePath}
                  className={cn(
                    "group cursor-pointer rounded-lg border bg-card overflow-hidden transition-all hover:shadow-md",
                    file.type === "image" && isFileSelected(file) && "ring-2 ring-primary"
                  )}
                  onClick={() => handleFileClick(file)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      handleFileClick(file);
                    }
                  }}
                  tabIndex={0}
                  role="button"
                  aria-label={`${file.type === "folder" ? "Open folder" : "Select image"} ${file.name}`}
                >
                  {/* Image/Folder Preview */}
                  <div className="aspect-square relative bg-muted">
                    {file.type === "folder" ? (
                      <div className="flex items-center justify-center h-full">
                        <Folder className="h-12 w-12 text-muted-foreground" />
                      </div>
                    ) : file.relativePath ? (
                      <Image
                        src={getImageUrl(file.relativePath)}
                        width={200}
                        height={50}
                        alt={file.name}
                        className="w-full h-full object-cover"
                        loading="lazy"
                      />
                    ) : (
                      <div className="flex items-center justify-center h-full">
                        <ImageIcon className="h-12 w-12 text-muted-foreground" />
                      </div>
                    )}
                    
                    {/* Selection indicator */}
                    {file.type === "image" && isFileSelected(file) && (
                      <div className="absolute top-2 right-2 h-6 w-6 bg-primary rounded-full flex items-center justify-center">
                        <Check className="h-4 w-4 text-primary-foreground" />
                      </div>
                    )}

                    {/* Hover overlay for images */}
                    {file.type === "image" && (
                      <div className="absolute inset-0 bg-black/20 opacity-0 group-hover:opacity-100 transition-opacity" />
                    )}
                  </div>

                  {/* File Info */}
                  <div className="p-3 space-y-1">
                    <div className="font-medium text-sm truncate" title={file.name}>
                      {file.name}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {file.type === "folder" 
                        ? `${file.metadata?.width || 0} items`
                        : formatFileSize(file.size)
                      }
                    </div>
                    {file.metadata && file.type === "image" && (
                      <div className="text-xs text-muted-foreground">
                        {file.metadata.width} × {file.metadata.height}
                      </div>
                    )}
                    <div className="text-xs text-muted-foreground">
                      {formatDate(file.modifiedAt)}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Selected Images Preview */}
        {selectedImages.length > 0 && (
          <div className="border-t bg-muted/20 p-4 space-y-3">
            {/* Actions */}
            <div className="flex justify-between items-center">
              <Button
                variant="outline"
                onClick={handleClearAll}
                size="sm"
              >
                Clear All
              </Button>
              
              <Button
                onClick={handleSelectImages}
                disabled={selectedImages.length === 0}
                className="flex items-center gap-2"
              >
                <ImageIcon className="h-4 w-4" />
                Select {selectedImages.length} Image{selectedImages.length !== 1 ? "s" : ""}
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
