/**
 * Device Upload Tab Component
 * Handles file upload from user's device with drag & drop support
 */

"use client";

import { useCallback, useState } from "react";
import { useDropzone } from "react-dropzone";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Upload, AlertCircle, Image as ImageIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { UploadTabProps, ImageUploadResult, ImagePreviewItem } from "@/types/imageUpload";
import { ImagePreviewGrid } from "../ImagePreviewGrid";

export const DeviceUploadTab = ({
  onImagesSelected,
  multiple = true,
  acceptedTypes = ["image/*"],
  maxSize = 10 * 1024 * 1024, // 10MB
}: UploadTabProps) => {
  const [selectedImages, setSelectedImages] = useState<ImagePreviewItem[]>([]);
  const [dragError, setDragError] = useState<string>("");

  const onDrop = useCallback((acceptedFiles: File[], rejectedFiles: any[]) => {
    setDragError("");

    if (rejectedFiles.length > 0) {
      const rejection = rejectedFiles[0];
      if (rejection.errors.some((e: any) => e.code === "file-too-large")) {
        setDragError(`Some files are too large. Maximum file size is ${Math.round(maxSize / (1024 * 1024))}MB.`);
      } else if (rejection.errors.some((e: any) => e.code === "file-invalid-type")) {
        setDragError("Some files have invalid types. Please select valid image files.");
      } else {
        setDragError("Some files were rejected.");
      }
    }

    if (acceptedFiles.length > 0) {
      const newImages: ImagePreviewItem[] = acceptedFiles.map((file) => ({
        id: Math.random().toString(36).substr(2, 9),
        source: "device",
        preview: URL.createObjectURL(file),
        name: file.name,
        size: file.size,
        file,
      }));

      if (multiple) {
        setSelectedImages(prev => [...prev, ...newImages]);
      } else {
        // Clean up previous object URLs for single selection
        selectedImages.forEach(img => {
          if (img.preview.startsWith('blob:')) {
            URL.revokeObjectURL(img.preview);
          }
        });
        setSelectedImages(newImages);
      }
    }
  }, [multiple, maxSize, selectedImages]);

  const { getRootProps, getInputProps, isDragActive, isDragReject } = useDropzone({
    onDrop,
    accept: acceptedTypes.reduce((acc, type) => {
      if (type === "image/*") {
        acc["image/*"] = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg"];
      } else {
        acc[type] = [];
      }
      return acc;
    }, {} as Record<string, string[]>),
    maxSize,
    multiple,
  });

  const handleRemoveImage = (id: string) => {
    setSelectedImages(prev => {
      const updated = prev.filter(img => img.id !== id);
      // Revoke object URL to prevent memory leaks
      const imageToRemove = prev.find(img => img.id === id);
      if (imageToRemove?.preview.startsWith('blob:')) {
        URL.revokeObjectURL(imageToRemove.preview);
      }
      return updated;
    });
  };

  const handleSelectImages = () => {
    if (selectedImages.length > 0) {
      const result: ImageUploadResult = {
        source: "device",
        files: selectedImages.map(img => img.file!),
      };
      onImagesSelected(result);
    }
  };

  const handleClearAll = () => {
    // Clean up object URLs
    selectedImages.forEach(img => {
      if (img.preview.startsWith('blob:')) {
        URL.revokeObjectURL(img.preview);
      }
    });
    setSelectedImages([]);
    setDragError("");
  };

  return (
    <div className="space-y-4">
      {/* Drop Zone */}
      {(selectedImages.length === 0 || multiple) && (
        <div
          {...getRootProps()}
          className={cn(
            "border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors",
            "hover:border-primary/50 hover:bg-muted/50",
            isDragActive && !isDragReject && "border-primary bg-primary/5",
            isDragReject && "border-destructive bg-destructive/5",
            dragError && "border-destructive"
          )}
        >
          <input {...getInputProps()} />
          <div className="flex flex-col items-center gap-3">
            <Upload className={cn(
              "h-12 w-12",
              isDragActive && !isDragReject && "text-primary",
              isDragReject && "text-destructive",
              dragError && "text-destructive"
            )} />
            <div>
              {isDragActive ? (
                isDragReject ? (
                  <span className="text-destructive font-medium">Invalid file type</span>
                ) : (
                  <span className="text-primary font-medium">Drop the images here</span>
                )
              ) : (
                <>
                  <div className="font-medium text-lg mb-1">
                    {selectedImages.length === 0 
                      ? "Drag & drop images here, or click to select"
                      : "Add more images"
                    }
                  </div>
                  <div className="text-sm text-muted-foreground">
                    Supports JPG, PNG, GIF, WebP up to {Math.round(maxSize / (1024 * 1024))}MB each
                    {multiple && " (multiple files allowed)"}
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Error Alert */}
      {dragError && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{dragError}</AlertDescription>
        </Alert>
      )}

      {/* Selected Images Preview */}
      {selectedImages.length > 0 && (
        <div className="space-y-3">
          <ImagePreviewGrid
            images={selectedImages}
            onRemoveImage={handleRemoveImage}
            showSource={false}
            maxHeight="250px"
          />

          {/* Actions */}
          <div className="flex justify-between items-center pt-2">
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

      {/* Instructions */}
      {selectedImages.length === 0 && (
        <div className="text-center text-sm text-muted-foreground space-y-1">
          <p>Select images from your device to upload</p>
          <p>You can drag and drop multiple files or click to browse</p>
        </div>
      )}
    </div>
  );
};
