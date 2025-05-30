/**
 * Image Upload Button Component
 * Reusable button component that triggers the image upload dialog
 */

"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Upload } from "lucide-react";
import { cn, getImageUrl } from "@/lib/utils";
import { ImageUploadButtonProps, ImageUploadResult } from "@/types/imageUpload";
import { ImageUploadDialog } from "./ImageUploadDialog";

export const ImageUploadButton = ({
  onImagesSelected,
  multiple = true,
  currentImages = [],
  onRemoveImage,
  acceptedTypes = ["image/*"],
  maxSize = 10 * 1024 * 1024, // 10MB
  aspectRatio,
  className,
  disabled = false,
  label = "Upload Images",
}: ImageUploadButtonProps) => {
  const [showDialog, setShowDialog] = useState(false);

  const handleImagesSelected = (result: ImageUploadResult) => {
    onImagesSelected(result);
    setShowDialog(false);
  };

  const handleRemoveCurrentImage = (index: number) => {
    if (onRemoveImage) {
      onRemoveImage(index);
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return "0 B";
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  };

  return (
    <div className={cn("space-y-3", className)}>
      {/* Upload Button */}
      <Button
        type="button"
        variant="outline"
        onClick={() => setShowDialog(true)}
        disabled={disabled}
        className="flex items-center gap-2"
        aria-label={label}
        tabIndex={0}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            setShowDialog(true);
          }
        }}
      >
        <Upload className="h-4 w-4" />
        {label}
      </Button>

      {/* Current Images Display */}
      {currentImages.length > 0 && (
        <div className="space-y-2">
          <div className="text-sm font-medium">
            Current Images ({currentImages.length})
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
            {currentImages.map((image, index) => (
              <div
                key={index}
                className="relative group border rounded-lg overflow-hidden bg-muted/30"
              >
                {/* Image */}
                <div className="aspect-square relative">
                  <img
                    src={typeof image === "string" ? getImageUrl(image) : URL.createObjectURL(image)}
                    alt={`Image ${index + 1}`}
                    className="w-full h-full object-cover"
                    loading="lazy"
                  />
                  
                  {/* Remove button overlay */}
                  {onRemoveImage && (
                    <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                      <Button
                        variant="destructive"
                        size="sm"
                        className="h-8 w-8 p-0"
                        onClick={() => handleRemoveCurrentImage(index)}
                        aria-label="Remove image"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' || e.key === ' ') {
                            handleRemoveCurrentImage(index);
                          }
                        }}
                      >
                        ×
                      </Button>
                    </div>
                  )}
                </div>

                {/* Image info */}
                <div className="p-2">
                  <div className="text-xs text-muted-foreground truncate">
                    {typeof image === "string" 
                      ? image.split('/').pop() || "Image"
                      : image.name
                    }
                  </div>
                  {typeof image !== "string" && (
                    <div className="text-xs text-muted-foreground">
                      {formatFileSize(image.size)}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Upload Dialog */}
      <ImageUploadDialog
        open={showDialog}
        onOpenChange={setShowDialog}
        onImagesSelected={handleImagesSelected}
        multiple={multiple}
        acceptedTypes={acceptedTypes}
        maxSize={maxSize}
        aspectRatio={aspectRatio}
      />
    </div>
  );
};
