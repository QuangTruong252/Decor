/**
 * File Upload Dialog Component
 */

"use client";

import { useState, useCallback } from "react";
import { useDropzone } from "react-dropzone";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Upload,
  File,
  X,
  AlertCircle,
  Image,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface FileUploadDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onUpload: (files: File[]) => void;
  currentPath: string;
}

interface UploadFile {
  file: File;
  id: string;
  preview?: string;
}

export const FileUploadDialog = ({
  open,
  onOpenChange,
  onUpload,
  currentPath,
}: FileUploadDialogProps) => {
  const [uploadFiles, setUploadFiles] = useState<UploadFile[]>([]);
  const [createThumbnails, setCreateThumbnails] = useState(true);
  const [overwriteExisting, setOverwriteExisting] = useState(false);
  const [dragError, setDragError] = useState<string>("");

  const onDrop = useCallback((acceptedFiles: File[], rejectedFiles: any[]) => {
    setDragError("");

    if (rejectedFiles.length > 0) {
      const rejection = rejectedFiles[0];
      if (rejection.errors.some((e: any) => e.code === "file-too-large")) {
        setDragError("Some files are too large. Maximum file size is 10MB.");
      } else if (rejection.errors.some((e: any) => e.code === "file-invalid-type")) {
        setDragError("Some files have invalid types. Please select valid files.");
      } else {
        setDragError("Some files were rejected.");
      }
    }

    if (acceptedFiles.length > 0) {
      const newFiles = acceptedFiles.map((file) => ({
        file,
        id: Math.random().toString(36).substr(2, 9),
        preview: file.type.startsWith("image/") ? URL.createObjectURL(file) : undefined,
      }));
      
      setUploadFiles(prev => [...prev, ...newFiles]);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive, isDragReject } = useDropzone({
    onDrop,
    accept: {
      "image/*": [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"],
      "application/pdf": [".pdf"],
      "application/msword": [".doc"],
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
      "application/vnd.ms-excel": [".xls"],
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": [".xlsx"],
      "text/plain": [".txt"],
      "text/csv": [".csv"],
    },
    maxSize: 10 * 1024 * 1024, // 10MB
    multiple: true,
  });

  const removeFile = (id: string) => {
    setUploadFiles(prev => {
      const updated = prev.filter(f => f.id !== id);
      // Revoke object URL to prevent memory leaks
      const fileToRemove = prev.find(f => f.id === id);
      if (fileToRemove?.preview) {
        URL.revokeObjectURL(fileToRemove.preview);
      }
      return updated;
    });
  };

  const handleUpload = () => {
    if (uploadFiles.length > 0) {
      const files = uploadFiles.map(f => f.file);
      onUpload(files);
      
      // Clean up object URLs
      uploadFiles.forEach(f => {
        if (f.preview) {
          URL.revokeObjectURL(f.preview);
        }
      });
      
      setUploadFiles([]);
    }
  };

  const handleClose = () => {
    // Clean up object URLs
    uploadFiles.forEach(f => {
      if (f.preview) {
        URL.revokeObjectURL(f.preview);
      }
    });
    
    setUploadFiles([]);
    setDragError("");
    onOpenChange(false);
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return "0 B";
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  };

  const totalSize = uploadFiles.reduce((sum, f) => sum + f.file.size, 0);

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>Upload Files</DialogTitle>
          <DialogDescription>
            Upload files to {currentPath || "root folder"}
          </DialogDescription>
        </DialogHeader>

        <div className="flex-1 overflow-auto space-y-4">
          {/* Drop Zone */}
          {uploadFiles.length === 0 && (
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
                      <span className="text-primary font-medium">Drop the files here</span>
                    )
                  ) : (
                    <>
                      <div className="font-medium text-lg mb-1">
                        Drag & drop files here, or click to select
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Supports images, documents, and other files up to 10MB each
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

          {/* File List */}
          {uploadFiles.length > 0 && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <h4 className="font-medium">Selected Files ({uploadFiles.length})</h4>
                <div className="text-sm text-muted-foreground">
                  Total: {formatFileSize(totalSize)}
                </div>
              </div>

              <div className="space-y-2 max-h-60 overflow-auto">
                {uploadFiles.map((uploadFile) => (
                  <div
                    key={uploadFile.id}
                    className="flex items-center gap-3 p-3 border rounded-lg"
                  >
                    {/* Preview/Icon */}
                    <div className="flex-shrink-0">
                      {uploadFile.preview ? (
                        <img
                          src={uploadFile.preview}
                          alt={uploadFile.file.name}
                          className="h-10 w-10 object-cover rounded"
                        />
                      ) : (
                        <div className="h-10 w-10 bg-muted rounded flex items-center justify-center">
                          {uploadFile.file.type.startsWith("image/") ? (
                            <Image className="h-5 w-5" />
                          ) : (
                            <File className="h-5 w-5" />
                          )}
                        </div>
                      )}
                    </div>

                    {/* File Info */}
                    <div className="flex-1 min-w-0">
                      <div className="font-medium text-sm truncate">
                        {uploadFile.file.name}
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {formatFileSize(uploadFile.file.size)}
                      </div>
                    </div>

                    {/* Remove Button */}
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => removeFile(uploadFile.id)}
                      className="h-8 w-8 p-0"
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>

              {/* Add More Button */}
              <div
                {...getRootProps()}
                className="border-2 border-dashed rounded-lg p-4 text-center cursor-pointer hover:border-primary/50 hover:bg-muted/50"
              >
                <input {...getInputProps()} />
                <div className="text-sm text-muted-foreground">
                  Click or drag to add more files
                </div>
              </div>
            </div>
          )}

          {/* Options */}
          {uploadFiles.length > 0 && (
            <div className="space-y-3 pt-4 border-t">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="thumbnails"
                  checked={createThumbnails}
                  onCheckedChange={setCreateThumbnails}
                />
                <Label htmlFor="thumbnails" className="text-sm">
                  Generate thumbnails for images
                </Label>
              </div>
              
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="overwrite"
                  checked={overwriteExisting}
                  onCheckedChange={setOverwriteExisting}
                />
                <Label htmlFor="overwrite" className="text-sm">
                  Overwrite existing files
                </Label>
              </div>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            Cancel
          </Button>
          <Button
            onClick={handleUpload}
            disabled={uploadFiles.length === 0}
          >
            Upload {uploadFiles.length > 0 && `${uploadFiles.length} file${uploadFiles.length !== 1 ? "s" : ""}`}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
