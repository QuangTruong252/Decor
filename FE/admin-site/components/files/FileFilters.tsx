/**
 * File Filters Component
 */

"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Search,
  Filter,
  X,
  ChevronDown,
  Calendar,
  HardDrive,
} from "lucide-react";
import {
  FileFilters as FileFiltersType,
  SORT_OPTIONS,
  FILE_TYPE_OPTIONS,
  EXTENSION_OPTIONS,
} from "@/types/fileManager";
import { cn } from "@/lib/utils";
import { useState } from "react";

interface FileFiltersProps {
  filters: FileFiltersType;
  onUpdateFilters: (filters: Partial<FileFiltersType>) => void;
  onResetFilters: () => void;
}

export const FileFilters = ({
  filters,
  onUpdateFilters,
  onResetFilters,
}: FileFiltersProps) => {
  const [showAdvanced, setShowAdvanced] = useState(false);

  const hasActiveFilters = 
    filters.search ||
    filters.fileType !== "all" ||
    filters.extension ||
    filters.dateRange.from ||
    filters.dateRange.to ||
    filters.sizeRange.min ||
    filters.sizeRange.max;

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return "0 B";
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  };

  const parseFileSize = (sizeStr: string): number => {
    if (!sizeStr) return 0;
    const match = sizeStr.match(/^(\d+(?:\.\d+)?)\s*(B|KB|MB|GB)$/i);
    if (!match) return 0;
    
    const value = parseFloat(match[1]);
    const unit = match[2].toUpperCase();
    const multipliers = { B: 1, KB: 1024, MB: 1024 * 1024, GB: 1024 * 1024 * 1024 };
    return value * (multipliers[unit as keyof typeof multipliers] || 1);
  };

  return (
    <div className="border-b bg-muted/30 p-4 space-y-4">
      {/* Search and Quick Filters */}
      <div className="flex items-center gap-4">
        {/* Search */}
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search files and folders..."
            value={filters.search}
            onChange={(e) => onUpdateFilters({ search: e.target.value })}
            className="pl-10"
          />
          {filters.search && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onUpdateFilters({ search: "" })}
              className="absolute right-1 top-1/2 transform -translate-y-1/2 h-6 w-6 p-0"
            >
              <X className="h-3 w-3" />
            </Button>
          )}
        </div>

        {/* File Type */}
        <div className="flex items-center gap-2">
          <Label className="text-sm font-medium">Type:</Label>
          <Select
            value={filters.fileType}
            onValueChange={(value) => onUpdateFilters({ fileType: value as any })}
          >
            <SelectTrigger className="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {FILE_TYPE_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Sort */}
        <div className="flex items-center gap-2">
          <Label className="text-sm font-medium">Sort:</Label>
          <Select
            value={filters.sortBy}
            onValueChange={(value) => onUpdateFilters({ sortBy: value as any })}
          >
            <SelectTrigger className="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {SORT_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select
            value={filters.sortOrder}
            onValueChange={(value) => onUpdateFilters({ sortOrder: value as any })}
          >
            <SelectTrigger className="w-20">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="asc">A-Z</SelectItem>
              <SelectItem value="desc">Z-A</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {/* Advanced Toggle */}
        <Collapsible open={showAdvanced} onOpenChange={setShowAdvanced}>
          <CollapsibleTrigger asChild>
            <Button variant="outline" size="sm">
              <Filter className="h-4 w-4 mr-2" />
              Advanced
              <ChevronDown className={cn(
                "h-4 w-4 ml-2 transition-transform",
                showAdvanced && "rotate-180"
              )} />
            </Button>
          </CollapsibleTrigger>
        </Collapsible>

        {/* Reset */}
        {hasActiveFilters && (
          <Button variant="outline" size="sm" onClick={onResetFilters}>
            <X className="h-4 w-4 mr-2" />
            Reset
          </Button>
        )}
      </div>

      {/* Advanced Filters */}
      <Collapsible open={showAdvanced} onOpenChange={setShowAdvanced}>
        <CollapsibleContent className="space-y-4">
          <Separator />
          
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Extension Filter */}
            <div className="space-y-2">
              <Label className="text-sm font-medium">Extension</Label>
              <Select
                value={filters.extension}
                onValueChange={(value) => onUpdateFilters({ extension: value })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="All extensions" />
                </SelectTrigger>
                <SelectContent>
                  {EXTENSION_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Size Range */}
            <div className="space-y-2">
              <Label className="text-sm font-medium flex items-center gap-2">
                <HardDrive className="h-4 w-4" />
                File Size
              </Label>
              <div className="flex items-center gap-2">
                <Input
                  placeholder="Min (e.g., 1MB)"
                  value={filters.sizeRange.min ? formatFileSize(filters.sizeRange.min) : ""}
                  onChange={(e) => {
                    const size = parseFileSize(e.target.value);
                    onUpdateFilters({
                      sizeRange: { ...filters.sizeRange, min: size || undefined }
                    });
                  }}
                  className="text-xs"
                />
                <span className="text-muted-foreground">to</span>
                <Input
                  placeholder="Max (e.g., 10MB)"
                  value={filters.sizeRange.max ? formatFileSize(filters.sizeRange.max) : ""}
                  onChange={(e) => {
                    const size = parseFileSize(e.target.value);
                    onUpdateFilters({
                      sizeRange: { ...filters.sizeRange, max: size || undefined }
                    });
                  }}
                  className="text-xs"
                />
              </div>
            </div>

            {/* Date Range */}
            <div className="space-y-2">
              <Label className="text-sm font-medium flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Modified Date
              </Label>
              <div className="flex items-center gap-2">
                <Input
                  type="date"
                  value={filters.dateRange.from || ""}
                  onChange={(e) => onUpdateFilters({
                    dateRange: { ...filters.dateRange, from: e.target.value || undefined }
                  })}
                  className="text-xs"
                />
                <span className="text-muted-foreground">to</span>
                <Input
                  type="date"
                  value={filters.dateRange.to || ""}
                  onChange={(e) => onUpdateFilters({
                    dateRange: { ...filters.dateRange, to: e.target.value || undefined }
                  })}
                  className="text-xs"
                />
              </div>
            </div>
          </div>
        </CollapsibleContent>
      </Collapsible>

      {/* Active Filters */}
      {hasActiveFilters && (
        <div className="flex items-center gap-2 flex-wrap">
          <span className="text-sm text-muted-foreground">Active filters:</span>
          
          {filters.search && (
            <Badge variant="secondary" className="gap-1">
              Search: {filters.search}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onUpdateFilters({ search: "" })}
                className="h-3 w-3 p-0 hover:bg-transparent"
              >
                <X className="h-2 w-2" />
              </Button>
            </Badge>
          )}
          
          {filters.fileType !== "all" && (
            <Badge variant="secondary" className="gap-1">
              Type: {FILE_TYPE_OPTIONS.find(o => o.value === filters.fileType)?.label}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onUpdateFilters({ fileType: "all" })}
                className="h-3 w-3 p-0 hover:bg-transparent"
              >
                <X className="h-2 w-2" />
              </Button>
            </Badge>
          )}
          
          {filters.extension && (
            <Badge variant="secondary" className="gap-1">
              Extension: {filters.extension}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onUpdateFilters({ extension: "" })}
                className="h-3 w-3 p-0 hover:bg-transparent"
              >
                <X className="h-2 w-2" />
              </Button>
            </Badge>
          )}
          
          {(filters.sizeRange.min || filters.sizeRange.max) && (
            <Badge variant="secondary" className="gap-1">
              Size: {filters.sizeRange.min ? formatFileSize(filters.sizeRange.min) : "0"} - {filters.sizeRange.max ? formatFileSize(filters.sizeRange.max) : "∞"}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onUpdateFilters({ sizeRange: {} })}
                className="h-3 w-3 p-0 hover:bg-transparent"
              >
                <X className="h-2 w-2" />
              </Button>
            </Badge>
          )}
          
          {(filters.dateRange.from || filters.dateRange.to) && (
            <Badge variant="secondary" className="gap-1">
              Date: {filters.dateRange.from || "∞"} - {filters.dateRange.to || "∞"}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onUpdateFilters({ dateRange: {} })}
                className="h-3 w-3 p-0 hover:bg-transparent"
              >
                <X className="h-2 w-2" />
              </Button>
            </Badge>
          )}
        </div>
      )}
    </div>
  );
};
