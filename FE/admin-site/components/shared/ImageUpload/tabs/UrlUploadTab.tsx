/**
 * URL Upload Tab Component
 * Handles image upload from URL with validation and preview
 */

"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Loader2, AlertCircle, Link as LinkIcon, Image as ImageIcon, Plus } from "lucide-react";

import { UploadTabProps, ImageUploadResult, ImagePreviewItem, UrlValidationResult } from "@/types/imageUpload";
import { ImagePreviewGrid } from "../ImagePreviewGrid";

export const UrlUploadTab = ({
  onImagesSelected,
  multiple = true,
}: UploadTabProps) => {
  const [urlInput, setUrlInput] = useState("");
  const [selectedImages, setSelectedImages] = useState<ImagePreviewItem[]>([]);
  const [isValidating, setIsValidating] = useState(false);
  const [validationError, setValidationError] = useState<string>("");

  const validateImageUrl = async (url: string): Promise<UrlValidationResult> => {
    try {
      // Basic URL validation
      const urlPattern = /^https?:\/\/.+\.(jpg|jpeg|png|gif|webp|bmp|svg)(\?.*)?$/i;
      if (!urlPattern.test(url)) {
        return {
          isValid: false,
          error: "Please enter a valid image URL (jpg, png, gif, webp, bmp, svg)"
        };
      }

      // Try to load the image to validate it exists and get dimensions
      return new Promise((resolve) => {
        const img = new Image();
        img.crossOrigin = "anonymous";
        
        img.onload = () => {
          resolve({
            isValid: true,
            imageData: {
              url,
              width: img.naturalWidth,
              height: img.naturalHeight,
            }
          });
        };

        img.onerror = () => {
          resolve({
            isValid: false,
            error: "Unable to load image from this URL. Please check the URL and try again."
          });
        };

        // Set timeout for validation
        setTimeout(() => {
          resolve({
            isValid: false,
            error: "Image validation timed out. Please try again."
          });
        }, 10000);

        img.src = url;
      });
    } catch {
      return {
        isValid: false,
        error: "Invalid URL format"
      };
    }
  };

  const handleAddUrl = async () => {
    if (!urlInput.trim()) {
      setValidationError("Please enter an image URL");
      return;
    }

    setIsValidating(true);
    setValidationError("");

    try {
      const validation = await validateImageUrl(urlInput.trim());
      
      if (!validation.isValid) {
        setValidationError(validation.error || "Invalid image URL");
        return;
      }

      // Check if URL already exists
      const urlExists = selectedImages.some(img => img.url === urlInput.trim());
      if (urlExists) {
        setValidationError("This URL has already been added");
        return;
      }

      const newImage: ImagePreviewItem = {
        id: Math.random().toString(36).substr(2, 9),
        source: "url",
        preview: urlInput.trim(),
        name: urlInput.trim().split('/').pop()?.split('?')[0] || "Image from URL",
        url: urlInput.trim(),
      };

      if (multiple) {
        setSelectedImages(prev => [...prev, newImage]);
      } else {
        setSelectedImages([newImage]);
      }

      setUrlInput("");
    } catch {
      setValidationError("Failed to validate image URL");
    } finally {
      setIsValidating(false);
    }
  };

  const handleRemoveImage = (id: string) => {
    setSelectedImages(prev => prev.filter(img => img.id !== id));
  };

  const handleSelectImages = () => {
    if (selectedImages.length > 0) {
      const result: ImageUploadResult = {
        source: "url",
        urls: selectedImages.map(img => img.url!),
      };
      onImagesSelected(result);
    }
  };

  const handleClearAll = () => {
    setSelectedImages([]);
    setValidationError("");
    setUrlInput("");
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !isValidating && urlInput.trim()) {
      handleAddUrl();
    }
  };

  return (
    <div className="space-y-4">
      {/* URL Input */}
      <div className="space-y-2">
        <Label htmlFor="image-url">Image URL</Label>
        <div className="flex gap-2">
          <div className="relative flex-1">
            <LinkIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              id="image-url"
              type="url"
              placeholder="https://example.com/image.jpg"
              value={urlInput}
              onChange={(e) => setUrlInput(e.target.value)}
              onKeyPress={handleKeyPress}
              className="pl-10"
              disabled={isValidating}
            />
          </div>
          <Button
            onClick={handleAddUrl}
            disabled={!urlInput.trim() || isValidating || (!multiple && selectedImages.length > 0)}
            className="flex items-center gap-2"
          >
            {isValidating ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <Plus className="h-4 w-4" />
            )}
            {isValidating ? "Validating..." : "Add"}
          </Button>
        </div>
        
        {/* Instructions */}
        <div className="text-sm text-muted-foreground">
          Enter a direct link to an image file (jpg, png, gif, webp, bmp, svg)
          {multiple && " - you can add multiple URLs"}
        </div>
      </div>

      {/* Validation Error */}
      {validationError && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{validationError}</AlertDescription>
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

      {/* Instructions when empty */}
      {selectedImages.length === 0 && (
        <div className="text-center text-sm text-muted-foreground space-y-1 py-8">
          <p>Add images by entering their URLs</p>
          <p>Make sure the URLs point directly to image files</p>
          {multiple && <p>You can add multiple images from different URLs</p>}
        </div>
      )}

      {/* Examples */}
      <div className="border-t pt-4">
        <div className="text-sm font-medium mb-2">Example URLs:</div>
        <div className="text-xs text-muted-foreground space-y-1">
          <div>• https://example.com/image.jpg</div>
          <div>• https://cdn.example.com/photos/image.png</div>
          <div>• https://images.example.com/gallery/photo.webp</div>
        </div>
      </div>
    </div>
  );
};
