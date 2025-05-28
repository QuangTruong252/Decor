/**
 * File Manager Toolbar Component
 */

"use client";

import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  ChevronLeft,
  Upload,
  Plus,
  Grid3X3,
  List,
  Filter,
  FolderPlus,
  Home,
  ChevronRight,
} from "lucide-react";
import { fileManagerUtils } from "@/services/fileManager";
import { ViewMode } from "@/types/fileManager";

interface FileToolbarProps {
  currentPath: string;
  canNavigateUp: boolean;
  viewMode: ViewMode;
  hasSelection: boolean;
  selectedCount: number;
  showFilters: boolean;
  onNavigateUp: () => void;
  onNavigateToPath: (path: string) => void;
  onSetViewMode: (mode: ViewMode) => void;
  onUpload: () => void;
  onCreateFolder: () => void;
  onToggleFilters: () => void;
  onClearSelection: () => void;
}

export const FileToolbar = ({
  currentPath,
  canNavigateUp,
  viewMode,
  hasSelection,
  selectedCount,
  showFilters,
  onNavigateUp,
  onNavigateToPath,
  onSetViewMode,
  onUpload,
  onCreateFolder,
  onToggleFilters,
  onClearSelection,
}: FileToolbarProps) => {
  const breadcrumb = fileManagerUtils.getBreadcrumb(currentPath);

  return (
    <div className="border-b bg-background">
      <div className="flex items-center justify-between p-4">
        {/* Left Section - Navigation */}
        <div className="flex items-center gap-2">
          {/* Back Button */}
          <Button
            variant="ghost"
            size="sm"
            onClick={onNavigateUp}
            disabled={!canNavigateUp}
            className="h-8 w-8 p-0"
          >
            <ChevronLeft className="h-4 w-4" />
          </Button>

          {/* Breadcrumb */}
          <div className="flex items-center gap-1 text-sm">
            {breadcrumb.map((item, index) => (
              <div key={item.path} className="flex items-center gap-1">
                {index === 0 ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => onNavigateToPath(item.path)}
                    className="h-6 px-2 text-sm font-medium"
                  >
                    <Home className="h-3 w-3 mr-1" />
                    {item.name}
                  </Button>
                ) : (
                  <>
                    <ChevronRight className="h-3 w-3 text-muted-foreground" />
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onNavigateToPath(item.path)}
                      className="h-6 px-2 text-sm capitalize"
                    >
                      {item.name}
                    </Button>
                  </>
                )}
              </div>
            ))}
          </div>

          {/* Selection Info */}
          {hasSelection && (
            <>
              <Separator orientation="vertical" className="h-4" />
              <Badge variant="secondary" className="text-xs">
                {selectedCount} selected
              </Badge>
              <Button
                variant="ghost"
                size="sm"
                onClick={onClearSelection}
                className="h-6 px-2 text-xs"
              >
                Clear
              </Button>
            </>
          )}
        </div>

        {/* Right Section - Actions */}
        <div className="flex items-center gap-2">
          {/* Upload Button */}
          <Button onClick={onUpload} size="sm" className="h-8">
            <Upload className="h-4 w-4 mr-2" />
            Upload
          </Button>

          {/* Create New Dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" size="sm" className="h-8">
                <Plus className="h-4 w-4 mr-2" />
                Create New
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={onCreateFolder}>
                <FolderPlus className="h-4 w-4 mr-2" />
                New Folder
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          <Separator orientation="vertical" className="h-4" />

          {/* View Mode Toggle */}
          <div className="flex items-center border rounded-md">
            <Button
              variant={viewMode === "grid" ? "default" : "ghost"}
              size="sm"
              onClick={() => onSetViewMode("grid")}
              className="h-8 w-8 p-0 rounded-r-none"
            >
              <Grid3X3 className="h-4 w-4" />
            </Button>
            <Button
              variant={viewMode === "list" ? "default" : "ghost"}
              size="sm"
              onClick={() => onSetViewMode("list")}
              className="h-8 w-8 p-0 rounded-l-none"
            >
              <List className="h-4 w-4" />
            </Button>
          </div>

          {/* Filter Toggle */}
          <Button
            variant={showFilters ? "default" : "ghost"}
            size="sm"
            onClick={onToggleFilters}
            className="h-8 w-8 p-0"
          >
            <Filter className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  );
};
