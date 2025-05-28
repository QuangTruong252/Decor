/**
 * Bulk Actions Bar Component
 */

"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Download,
  Copy,
  Move,
  Trash2,
  X,
  FolderOpen,
} from "lucide-react";

interface BulkActionsBarProps {
  selectedCount: number;
  onMove: (destinationPath: string) => void;
  onCopy: (destinationPath: string) => void;
  onDelete: () => void;
  onClearSelection: () => void;
}

export const BulkActionsBar = ({
  selectedCount,
  onMove,
  onCopy,
  onDelete,
  onClearSelection,
}: BulkActionsBarProps) => {
  const [showMoveDialog, setShowMoveDialog] = useState(false);
  const [showCopyDialog, setShowCopyDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [destinationPath, setDestinationPath] = useState("");

  const handleMove = () => {
    if (destinationPath) {
      onMove(destinationPath);
      setShowMoveDialog(false);
      setDestinationPath("");
    }
  };

  const handleCopy = () => {
    if (destinationPath) {
      onCopy(destinationPath);
      setShowCopyDialog(false);
      setDestinationPath("");
    }
  };

  const handleDelete = () => {
    onDelete();
    setShowDeleteDialog(false);
  };

  // Mock folder options - in real app, this would come from folder structure
  const folderOptions = [
    { value: "", label: "Root" },
    { value: "categories", label: "Categories" },
    { value: "products", label: "Products" },
    { value: "banners", label: "Banners" },
    { value: "temp", label: "Temp" },
  ];

  return (
    <>
      <div className="fixed bottom-0 left-0 right-0 bg-background border-t shadow-lg z-50">
        <div className="flex items-center justify-between p-4">
          {/* Selection Info */}
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium">
              {selectedCount} item{selectedCount !== 1 ? "s" : ""} selected
            </span>
            <Button
              variant="ghost"
              size="sm"
              onClick={onClearSelection}
              className="h-8 w-8 p-0"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-2">
            {/* Download */}
            <Button variant="outline" size="sm">
              <Download className="h-4 w-4 mr-2" />
              Download
            </Button>

            <Separator orientation="vertical" className="h-4" />

            {/* Move */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowMoveDialog(true)}
            >
              <Move className="h-4 w-4 mr-2" />
              Move to
            </Button>

            {/* Copy */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowCopyDialog(true)}
            >
              <Copy className="h-4 w-4 mr-2" />
              Copy
            </Button>

            <Separator orientation="vertical" className="h-4" />

            {/* Delete */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowDeleteDialog(true)}
              className="text-destructive hover:text-destructive"
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete
            </Button>
          </div>
        </div>
      </div>

      {/* Move Dialog */}
      <Dialog open={showMoveDialog} onOpenChange={setShowMoveDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Move Items</DialogTitle>
            <DialogDescription>
              Select the destination folder for {selectedCount} selected item{selectedCount !== 1 ? "s" : ""}.
            </DialogDescription>
          </DialogHeader>
          
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">Destination Folder</label>
              <Select value={destinationPath} onValueChange={setDestinationPath}>
                <SelectTrigger className="mt-1">
                  <SelectValue placeholder="Select a folder" />
                </SelectTrigger>
                <SelectContent>
                  {folderOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      <div className="flex items-center gap-2">
                        <FolderOpen className="h-4 w-4" />
                        {option.label}
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowMoveDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleMove} disabled={!destinationPath}>
              Move Items
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Copy Dialog */}
      <Dialog open={showCopyDialog} onOpenChange={setShowCopyDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Copy Items</DialogTitle>
            <DialogDescription>
              Select the destination folder to copy {selectedCount} selected item{selectedCount !== 1 ? "s" : ""}.
            </DialogDescription>
          </DialogHeader>
          
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">Destination Folder</label>
              <Select value={destinationPath} onValueChange={setDestinationPath}>
                <SelectTrigger className="mt-1">
                  <SelectValue placeholder="Select a folder" />
                </SelectTrigger>
                <SelectContent>
                  {folderOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      <div className="flex items-center gap-2">
                        <FolderOpen className="h-4 w-4" />
                        {option.label}
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCopyDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleCopy} disabled={!destinationPath}>
              Copy Items
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Items</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete {selectedCount} selected item{selectedCount !== 1 ? "s" : ""}? 
              This action cannot be undone.
            </DialogDescription>
          </DialogHeader>

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDeleteDialog(false)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Delete Items
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
};
