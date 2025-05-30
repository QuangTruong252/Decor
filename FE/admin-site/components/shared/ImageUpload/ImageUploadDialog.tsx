/**
 * Image Upload Dialog Component
 * Main dialog with tabs for different upload methods
 */

"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Upload, Link, HardDrive } from "lucide-react";
import { ImageUploadDialogProps, ImageUploadResult, UploadTabType } from "@/types/imageUpload";
import { DeviceUploadTab } from "./tabs/DeviceUploadTab";
import { UrlUploadTab } from "./tabs/UrlUploadTab";
import { SystemFilesTab } from "./tabs/SystemFilesTab";

export const ImageUploadDialog = ({
  open,
  onOpenChange,
  onImagesSelected,
  multiple = true,
  acceptedTypes = ["image/*"],
  maxSize = 10 * 1024 * 1024, // 10MB
  aspectRatio,
}: ImageUploadDialogProps) => {
  const [activeTab, setActiveTab] = useState<UploadTabType>("device");

  const handleImagesSelected = (result: ImageUploadResult) => {
    onImagesSelected(result);
    onOpenChange(false);
  };

  const handleClose = () => {
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-7xl sm:max-w-2xl max-h-[90vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Upload className="h-5 w-5" />
            Upload Images
          </DialogTitle>
          <DialogDescription>
            Choose how you want to add images. You can upload from your device, 
            enter image URLs, or select from existing files in the system.
            {multiple ? " Multiple images are allowed." : " Single image selection only."}
          </DialogDescription>
        </DialogHeader>

        <div className="flex-1 overflow-hidden">
          <Tabs 
            value={activeTab} 
            onValueChange={(value: string) => setActiveTab(value as UploadTabType)}
            className="h-full flex flex-col"
          >
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="device" className="flex items-center gap-2">
                <Upload className="h-4 w-4" />
                <span className="hidden sm:inline">Upload from Device</span>
                <span className="sm:hidden">Device</span>
              </TabsTrigger>
              <TabsTrigger value="url" className="flex items-center gap-2">
                <Link className="h-4 w-4" />
                <span className="hidden sm:inline">From URL</span>
                <span className="sm:hidden">URL</span>
              </TabsTrigger>
              <TabsTrigger value="system" className="flex items-center gap-2">
                <HardDrive className="h-4 w-4" />
                <span className="hidden sm:inline">System Files</span>
                <span className="sm:hidden">System</span>
              </TabsTrigger>
            </TabsList>

            <div className="flex-1 overflow-auto mt-4">
              <TabsContent value="device" className="mt-0 h-full">
                <DeviceUploadTab
                  onImagesSelected={handleImagesSelected}
                  multiple={multiple}
                  acceptedTypes={acceptedTypes}
                  maxSize={maxSize}
                  aspectRatio={aspectRatio}
                />
              </TabsContent>

              <TabsContent value="url" className="mt-0 h-full">
                <UrlUploadTab
                  onImagesSelected={handleImagesSelected}
                  multiple={multiple}
                  acceptedTypes={acceptedTypes}
                  maxSize={maxSize}
                  aspectRatio={aspectRatio}
                />
              </TabsContent>

              <TabsContent value="system" className="mt-0 h-full">
                <SystemFilesTab
                  onImagesSelected={handleImagesSelected}
                  multiple={multiple}
                  acceptedTypes={acceptedTypes}
                  maxSize={maxSize}
                  aspectRatio={aspectRatio}
                />
              </TabsContent>
            </div>
          </Tabs>
        </div>
      </DialogContent>
    </Dialog>
  );
};
