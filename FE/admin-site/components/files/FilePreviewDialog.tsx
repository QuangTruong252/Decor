/**
 * File Preview Dialog Component
 */

"use client";

import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Download,
  Copy,
  Trash2,
  File,
  ImageIcon,
  FileText,
  Music,
  Video,
  Archive,
  AlertCircle,
  Calendar,
  Info,
} from "lucide-react";
import { fileManagerService } from "@/services/fileManager";
import { FileItem } from "@/types/fileManager";
import { format } from "date-fns";
import { getImageUrl } from "@/lib/utils";
import Image from "next/image"
interface FilePreviewDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  filePath: string;
  onClose: () => void;
}

const getFileIcon = (item: FileItem) => {
  if (item.type === "folder") {
    return <File className="h-8 w-8 text-blue-500" />;
  }

  if (item.type === "image") {
    return <ImageIcon className="h-8 w-8 text-green-500" />;
  }

  const extension = item.extension?.toLowerCase() || "";
  
  if ([".pdf"].includes(extension)) {
    return <FileText className="h-8 w-8 text-red-500" />;
  }
  if ([".mp3", ".wav", ".flac", ".aac"].includes(extension)) {
    return <Music className="h-8 w-8 text-pink-500" />;
  }
  if ([".mp4", ".avi", ".mov", ".wmv"].includes(extension)) {
    return <Video className="h-8 w-8 text-purple-500" />;
  }
  if ([".zip", ".rar", ".7z"].includes(extension)) {
    return <Archive className="h-8 w-8 text-orange-500" />;
  }

  return <File className="h-8 w-8 text-gray-500" />;
};

export const FilePreviewDialog = ({
  open,
  onOpenChange,
  filePath,
  onClose,
}: FilePreviewDialogProps) => {
  const [fileInfo, setFileInfo] = useState<FileItem | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open && filePath) {
      loadFileInfo();
    }
  }, [open, filePath]);

  const loadFileInfo = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      const info = await fileManagerService.getFileInfo(filePath);
      setFileInfo(info);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load file info");
    } finally {
      setIsLoading(false);
    }
  };

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const handleClose = () => {
    setFileInfo(null);
    setError(null);
    onClose();
  };

  const canPreviewImage = fileInfo?.type === "image" && fileInfo.fullUrl;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-3">
            {fileInfo && getFileIcon(fileInfo)}
            <span className="truncate">{fileInfo?.name || "Loading..."}</span>
          </DialogTitle>
          <DialogDescription>
            File preview and information
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="flex-1 space-y-4">
            <Skeleton className="h-64 w-full" />
            <div className="space-y-2">
              <Skeleton className="h-4 w-1/3" />
              <Skeleton className="h-4 w-1/2" />
              <Skeleton className="h-4 w-1/4" />
            </div>
          </div>
        ) : error ? (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : fileInfo ? (
          <div className="flex-1 overflow-auto space-y-6">
            {/* Preview Section */}
            {canPreviewImage ? (
              <div className="flex justify-center bg-muted/30 rounded-lg p-4">
                <Image
                  src={getImageUrl(fileInfo.relativePath)}
                  width={500}
                  height={200}
                  alt={fileInfo.name}
                  className="max-h-96 max-w-full object-contain rounded"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.style.display = "none";
                  }}
                />
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center bg-muted/30 rounded-lg p-12">
                {getFileIcon(fileInfo)}
                <div className="mt-4 text-center">
                  <div className="font-medium">{fileInfo.name}</div>
                  <div className="text-sm text-muted-foreground mt-1">
                    Preview not available for this file type
                  </div>
                </div>
              </div>
            )}

            {/* File Information */}
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold">File Information</h3>
                <div className="flex items-center gap-2">
                  <Button variant="outline" size="sm">
                    <Download className="h-4 w-4" />
                  </Button>
                  <Button variant="outline" size="sm">
                    <Copy className="h-4 w-4" />
                  </Button>
                  <Button variant="outline" size="sm" className="text-destructive">
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Basic Info */}
                <div className="space-y-3">
                  <h4 className="font-medium flex items-center gap-2">
                    <Info className="h-4 w-4" />
                    Basic Information
                  </h4>
                  
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Name:</span>
                      <span className="font-medium">{fileInfo.name}</span>
                    </div>
                    
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Type:</span>
                      <Badge variant="secondary">
                        {fileInfo.type === "folder" ? "Folder" : fileInfo.extension?.toUpperCase() || "File"}
                      </Badge>
                    </div>
                    
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Size:</span>
                      <span>{fileInfo.formattedSize}</span>
                    </div>
                    
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Path:</span>
                      <span className="truncate max-w-48" title={fileInfo.relativePath}>
                        {fileInfo.relativePath}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Dates */}
                <div className="space-y-3">
                  <h4 className="font-medium flex items-center gap-2">
                    <Calendar className="h-4 w-4" />
                    Dates
                  </h4>
                  
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Created:</span>
                      <span>{format(new Date(fileInfo.createdAt), "MMM d, yyyy 'at' h:mm a")}</span>
                    </div>
                    
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Modified:</span>
                      <span>{format(new Date(fileInfo.modifiedAt), "MMM d, yyyy 'at' h:mm a")}</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Image Metadata */}
              {fileInfo.metadata && (
                <>
                  <Separator />
                  <div className="space-y-3">
                    <h4 className="font-medium flex items-center gap-2">
                      <ImageIcon className="h-4 w-4" />
                      Image Properties
                    </h4>
                    
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground block">Dimensions</span>
                        <span className="font-medium">
                          {fileInfo.metadata.width} × {fileInfo.metadata.height}
                        </span>
                      </div>
                      
                      <div>
                        <span className="text-muted-foreground block">Format</span>
                        <span className="font-medium">{fileInfo.metadata.format}</span>
                      </div>
                      
                      <div>
                        <span className="text-muted-foreground block">Aspect Ratio</span>
                        <span className="font-medium">{fileInfo.metadata.aspectRatio.toFixed(2)}</span>
                      </div>
                      
                      <div>
                        <span className="text-muted-foreground block">Color Space</span>
                        <span className="font-medium">{fileInfo.metadata.colorSpace}</span>
                      </div>
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>
        ) : null}
      </DialogContent>
    </Dialog>
  );
};
