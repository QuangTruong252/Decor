/**
 * File Manager Main View Component
 */

"use client";

import { useState } from "react";
import { useFileManager } from "@/hooks/useFileManager";
import { Skeleton } from "@/components/ui/skeleton";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertCircle } from "lucide-react";
import { cn } from "@/lib/utils";
import { BulkActionsBar } from "./BulkActionsBar";
import { CreateFolderDialog } from "./CreateFolderDialog";
import { FileFilters } from "./FileFilters";
import { FileGrid } from "./FileGrid";
import { FilePreviewDialog } from "./FilePreviewDialog";
import { FileToolbar } from "./FileToolbar";
import { FileUploadDialog } from "./FileUploadDialog";
import { FolderTree } from "./FolderTree";
import { FileList } from "./FileList";

import type { FileItem } from "@/types/fileManager"; // Ensure FileItem is imported if not already
export const FileManagerView = () => {
  const {
    browseData,
    rootFolderStructure,
    currentPath,
    selectedItems,
    viewMode,
    filters,
    uploadProgress,
    isBrowseLoading,
    isRootFoldersLoading,
    error,
    navigateToPath,
    navigateUp,
    selectItem,
    clearSelection,
    setViewMode,
    updateFilters,
    resetFilters,
    uploadFiles,
    createFolder,
    deleteSelectedItems,
    moveSelectedItems,
    copySelectedItems,
    hasSelection,
    selectedCount,
    canNavigateUp,
  } = useFileManager();
  // Dialog states
  const [showUploadDialog, setShowUploadDialog] = useState(false);
  const [showCreateFolderDialog, setShowCreateFolderDialog] = useState(false);
  const [showPreviewDialog, setShowPreviewDialog] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [previewFile, setPreviewFile] = useState<string | null>(null);

  // Event Handlers
  const handleFileDoubleClick = (relativePath: string, type: string) => {
    if (type === "folder") {
      navigateToPath(relativePath);
    } else {
      // For non-folder types, trigger preview
      const itemToPreview = browseData?.items.find(item => item.relativePath === relativePath);
      if (itemToPreview) {
        handlePreviewFile(itemToPreview);
      }
    }
  };
  const handlePreviewFile = (item: FileItem) => {
    setPreviewFile(item.relativePath);
    setShowPreviewDialog(true);
  };

  const handleUpload = (files: File[]) => {
    uploadFiles(files);
    setShowUploadDialog(false);
  };

  const handleCreateFolder = (folderName: string) => {
    createFolder(folderName);
    setShowCreateFolderDialog(false);
  };

  if (error) {
    return (
      <div className="flex h-full items-center justify-center p-6">
        <Alert variant="destructive" className="max-w-md">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Failed to load file manager: {error instanceof Error ? error.message : "Unknown error"}
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col bg-background">
      {/* Toolbar */}
      <FileToolbar
        currentPath={currentPath}
        canNavigateUp={canNavigateUp}
        viewMode={viewMode}
        hasSelection={hasSelection}
        selectedCount={selectedCount}
        showFilters={showFilters}
        onNavigateUp={navigateUp}
        onNavigateToPath={navigateToPath}
        onSetViewMode={setViewMode}
        onUpload={() => setShowUploadDialog(true)}
        onCreateFolder={() => setShowCreateFolderDialog(true)}
        onToggleFilters={() => setShowFilters(!showFilters)}
        onClearSelection={clearSelection}
      />

      {/* Filters Panel */}
      {showFilters && (
        <FileFilters
          filters={filters}
          onUpdateFilters={updateFilters}
          onResetFilters={resetFilters}
        />
      )}

      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar - Folder Tree */}
        <div className="w-64 border-r bg-muted/30">
          <div className="p-4">
            <h3 className="text-md font-medium text-muted-foreground mb-3">FOLDERS</h3>
            {isRootFoldersLoading ? (
              <div className="space-y-2">
                {Array.from({ length: 5 }).map((_, i) => (
                  <Skeleton key={i} className="h-6 w-full" />
                ))}
              </div>
            ) : (
              <FolderTree
                folderStructure={rootFolderStructure}
                currentPath={currentPath}
                onNavigateToPath={navigateToPath}
              />
            )}
          </div>
        </div>

        {/* Main Content */}
        <div className="flex-1 flex flex-col overflow-hidden relative">
          {/* Content Area */}
          <div className="flex-1 overflow-auto p-6">
            {isBrowseLoading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 xl:grid-cols-6 gap-4">
                {Array.from({ length: 12 }).map((_, i) => (
                  <div key={i} className="space-y-2">
                    <Skeleton className="h-32 w-full rounded-lg" />
                    <Skeleton className="h-4 w-3/4" />
                    <Skeleton className="h-3 w-1/2" />
                  </div>
                ))}
              </div>
            ) : browseData?.items.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-64 text-center">
                <div className="text-muted-foreground mb-2">No files or folders found</div>
                <div className="text-sm text-muted-foreground">
                  {filters.search ? "Try adjusting your search or filters" : "This folder is empty"}
                </div>
              </div>
            ) : viewMode === "grid" ? (
              <FileGrid
                items={browseData?.items || []}
                selectedItems={selectedItems}
                onSelectItem={selectItem}
                onDoubleClick={handleFileDoubleClick}
                onPreviewFile={handlePreviewFile}
              />
            ) : (
              <FileList
                items={browseData?.items || []}
                selectedItems={selectedItems}
                onSelectItem={selectItem}
                onDoubleClick={handleFileDoubleClick}
                onPreviewFile={handlePreviewFile}
              />
            )}
          </div>

          {/* Upload Progress */}
          {uploadProgress.length > 0 && (
            <div className="border-t bg-muted/30 p-4">
              <div className="space-y-2">
                <div className="text-sm font-medium">Uploading files...</div>
                {uploadProgress.map((progress, index) => (
                  <div key={index} className="flex items-center gap-3">
                    <div className="flex-1">
                      <div className="flex justify-between text-xs mb-1">
                        <span className="truncate">{progress.fileName}</span>
                        <span>{progress.progress}%</span>
                      </div>
                      <div className="w-full bg-muted rounded-full h-1.5">
                        <div
                          className={cn(
                            "h-1.5 rounded-full transition-all",
                            progress.status === "completed" ? "bg-green-500" :
                            progress.status === "error" ? "bg-red-500" :
                            "bg-blue-500"
                          )}
                          style={{ width: `${progress.progress}%` }}
                        />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Bulk Actions Bar */}
          {hasSelection && (
            <BulkActionsBar
              selectedCount={selectedCount}
              onMove={(destinationPath) => moveSelectedItems(destinationPath)}
              onCopy={(destinationPath) => copySelectedItems(destinationPath)}
              onDelete={() => deleteSelectedItems()}
              onClearSelection={clearSelection}
            />
          )}
        </div>
      </div>

      {/* Dialogs */}
      <FileUploadDialog
        open={showUploadDialog}
        onOpenChange={setShowUploadDialog}
        onUpload={handleUpload}
        currentPath={currentPath}
      />

      <CreateFolderDialog
        open={showCreateFolderDialog}
        onOpenChange={setShowCreateFolderDialog}
        onCreateFolder={handleCreateFolder}
        currentPath={currentPath}
      />

      {previewFile && (
        <FilePreviewDialog
          open={showPreviewDialog}
          onOpenChange={setShowPreviewDialog}
          filePath={previewFile}
          onClose={() => {
            setShowPreviewDialog(false);
            setPreviewFile(null);
          }}
        />
      )}
    </div>
  );
};
