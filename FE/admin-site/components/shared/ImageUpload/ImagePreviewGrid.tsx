/**
 * Image Preview Grid Component
 * Displays selected images in a grid layout with remove functionality
 */

"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { X, Eye } from "lucide-react";
import { cn } from "@/lib/utils";
import { ImagePreviewItem } from "@/types/imageUpload";

interface ImagePreviewGridProps {
  images: ImagePreviewItem[];
  onRemoveImage: (id: string) => void;
  className?: string;
  showSource?: boolean;
  maxHeight?: string;
}

export const ImagePreviewGrid = ({
  images,
  onRemoveImage,
  className,
  showSource = true,
  maxHeight = "300px",
}: ImagePreviewGridProps) => {
  const [previewImage, setPreviewImage] = useState<string | null>(null);

  const formatFileSize = (bytes?: number): string => {
    if (!bytes) return "";
    if (bytes === 0) return "0 B";
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  };

  const getSourceLabel = (source: string): string => {
    switch (source) {
      case "device":
        return "Device";
      case "url":
        return "URL";
      case "system":
        return "System";
      default:
        return source;
    }
  };

  const getSourceColor = (source: string): string => {
    switch (source) {
      case "device":
        return "bg-blue-100 text-blue-800";
      case "url":
        return "bg-green-100 text-green-800";
      case "system":
        return "bg-purple-100 text-purple-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  if (images.length === 0) {
    return null;
  }

  return (
    <>
      <div className={cn("space-y-3", className)}>
        <div className="flex items-center justify-between">
          <h4 className="font-medium text-sm">
            Selected Images ({images.length})
          </h4>
          {images.length > 0 && (
            <div className="text-xs text-muted-foreground">
              Total: {formatFileSize(images.reduce((sum, img) => sum + (img.size || 0), 0))}
            </div>
          )}
        </div>

        <div 
          className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3 overflow-auto"
          style={{ maxHeight }}
        >
          {images.map((image) => (
            <div
              key={image.id}
              className="relative group border rounded-lg overflow-hidden bg-muted/30"
            >
              {/* Image */}
              <div className="aspect-square relative">
                <img
                  src={image.preview}
                  width={200}
                  height={50}
                  alt={image.name}
                  className="w-full h-full object-cover"
                  loading="lazy"
                />
                
                {/* Overlay with actions */}
                <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-1">
                  <Button
                    variant="secondary"
                    size="sm"
                    className="h-8 w-8 p-0"
                    onClick={() => setPreviewImage(image.preview)}
                    aria-label="Preview image"
                    tabIndex={0}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        setPreviewImage(image.preview);
                      }
                    }}
                  >
                    <Eye className="h-4 w-4" />
                  </Button>
                  
                  <Button
                    variant="destructive"
                    size="sm"
                    className="h-8 w-8 p-0"
                    onClick={() => onRemoveImage(image.id)}
                    aria-label="Remove image"
                    tabIndex={0}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        onRemoveImage(image.id);
                      }
                    }}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>

                {/* Source badge */}
                {showSource && (
                  <Badge
                    variant="secondary"
                    className={cn(
                      "absolute top-2 left-2 text-xs px-1.5 py-0.5",
                      getSourceColor(image.source)
                    )}
                  >
                    {getSourceLabel(image.source)}
                  </Badge>
                )}
              </div>

              {/* Image info */}
              <div className="p-2 space-y-1">
                <div className="text-xs font-medium truncate" title={image.name}>
                  {image.name}
                </div>
                {image.size && (
                  <div className="text-xs text-muted-foreground">
                    {formatFileSize(image.size)}
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Preview Modal */}
      {previewImage && (
        <div
          className="fixed inset-0 z-50 bg-black/80 flex items-center justify-center p-4"
          onClick={() => setPreviewImage(null)}
          onKeyDown={(e) => {
            if (e.key === 'Escape') {
              setPreviewImage(null);
            }
          }}
          tabIndex={0}
          role="dialog"
          aria-label="Image preview"
        >
          <div className="relative max-w-4xl max-h-full">
            <img
              src={previewImage}
              alt="Preview"
              className="max-w-full max-h-full object-contain"
              onClick={(e) => e.stopPropagation()}
            />
            <Button
              variant="secondary"
              size="sm"
              className="absolute top-4 right-4"
              onClick={() => setPreviewImage(null)}
              aria-label="Close preview"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </>
  );
};
