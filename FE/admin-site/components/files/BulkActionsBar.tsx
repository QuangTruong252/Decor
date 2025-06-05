/**
 * Bulk Actions Bar Component
 */

"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
  Download,
  Copy,
  Move,
  Trash2,
  X,
} from "lucide-react";
import { useFileManagerContext } from "@/contexts/FileManagerContext";
import { CopyFileDialog } from "./CopyFileDialog";
import { MoveFileDialog } from "./MoveFileDialog";
import { useConfirmationDialog } from "../ui/confirmation-dialog";

interface BulkActionsBarProps {
  selectedCount: number;
  onClearSelection: () => void;
}

export const BulkActionsBar = ({
  selectedCount,
  onClearSelection,
}: BulkActionsBarProps) => {
  const [showMoveDialog, setShowMoveDialog] = useState(false);
  const [showCopyDialog, setShowCopyDialog] = useState(false);
  const [isDownloading, setIsDownloading] = useState(false);
  const { confirm } = useConfirmationDialog();
  const { deleteSelectedItems, selectedItems, downloadFile } = useFileManagerContext();
  const handleDelete = async () => {
    try {
      confirm({
        title: "Delete Items",
        message: `Are you sure you want to delete ${selectedCount} selected item${selectedCount !== 1 ? "s" : ""}? This action cannot be undone.`,
        confirmText: "Delete Items",
        variant: "destructive",
        onConfirm: () => {
          deleteSelectedItems();
        },
      });

    } catch (error) {
      console.error("Error with confirmation dialog:", error);
    }
  };

  const handleDownload = async () => {
    if (selectedItems.length === 0) return;

    setIsDownloading(true);
    
    try {
      if (selectedItems.length === 1) {
        // Single file download
        downloadFile(selectedItems[0]);
      } else {
        // Multiple files - download sequentially with small delays
        for (const item of selectedItems) {
          downloadFile(item);
          // Small delay between downloads to avoid overwhelming the browser
          if (selectedItems.indexOf(item) < selectedItems.length - 1) {
            await new Promise(resolve => setTimeout(resolve, 500));
          }
        }
      }
    } catch (error) {
      console.error("Error downloading files:", error);
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <>
      <div className="absolute bottom-[30px] left-1/2 translate-x-[-50%] translate-y-[-50%] rounded-lg bg-background border shadow-xl z-50">
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
            <Button 
              variant="outline" 
              size="sm" 
              onClick={handleDownload}
              disabled={isDownloading}
            >
              <Download className="h-4 w-4 mr-2" />
              {isDownloading ? "Downloading..." : "Download"}
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
              onClick={() => handleDelete()}
              className="text-destructive hover:text-destructive"
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete
            </Button>
          </div>
        </div>
      </div>

      {/* Move Dialog */}
      <MoveFileDialog
        open={showMoveDialog}
        onOpenChange={setShowMoveDialog}
      />

      {/* Copy Dialog */}
      <CopyFileDialog
        open={showCopyDialog}
        onOpenChange={setShowCopyDialog}
      />
    </>
  );
};
