/**
 * Create Folder Dialog Component
 */

"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { FolderPlus, AlertCircle } from "lucide-react";

interface CreateFolderDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreateFolder: (folderName: string) => void;
  currentPath: string;
}

export const CreateFolderDialog = ({
  open,
  onOpenChange,
  onCreateFolder,
  currentPath,
}: CreateFolderDialogProps) => {
  const [folderName, setFolderName] = useState("");
  const [error, setError] = useState("");

  const validateFolderName = (name: string): string | null => {
    if (!name.trim()) {
      return "Folder name is required";
    }
    
    if (name.length > 255) {
      return "Folder name is too long (max 255 characters)";
    }
    
    // Check for invalid characters
    const invalidChars = /[<>:"/\\|?*]/;
    if (invalidChars.test(name)) {
      return "Folder name contains invalid characters";
    }
    
    // Check for reserved names
    const reservedNames = ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"];
    if (reservedNames.includes(name.toUpperCase())) {
      return "This folder name is reserved";
    }
    
    return null;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const trimmedName = folderName.trim();
    const validationError = validateFolderName(trimmedName);
    
    if (validationError) {
      setError(validationError);
      return;
    }
    
    onCreateFolder(trimmedName);
    handleClose();
  };

  const handleClose = () => {
    setFolderName("");
    setError("");
    onOpenChange(false);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setFolderName(value);
    
    // Clear error when user starts typing
    if (error) {
      setError("");
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <FolderPlus className="h-5 w-5" />
            Create New Folder
          </DialogTitle>
          <DialogDescription>
            Create a new folder in {currentPath || "root folder"}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="folderName">Folder Name</Label>
            <Input
              id="folderName"
              value={folderName}
              onChange={handleInputChange}
              placeholder="Enter folder name"
              autoFocus
              className={error ? "border-destructive" : ""}
            />
          </div>

          {error && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={!folderName.trim()}>
              Create Folder
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};
