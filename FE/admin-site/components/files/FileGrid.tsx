/**
 * File Grid Component
 */

"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Folder,
  File,
  Image,
  FileText,
  Music,
  Video,
  Archive,
  MoreVertical,
  Download,
  Copy,
  Trash2,
  Star,
  Eye,
} from "lucide-react";
import { FileItem } from "@/types/fileManager";
import { cn } from "@/lib/utils";
import { format } from "date-fns";

interface FileGridProps {
  items: FileItem[];
  selectedItems: string[];
  onSelectItem: (relativePath: string) => void;
  onDoubleClick: (relativePath: string, type: string) => void;
}

interface FileCardProps {
  item: FileItem;
  isSelected: boolean;
  onSelect: () => void;
  onDoubleClick: () => void;
}

const getFileIcon = (item: FileItem) => {
  if (item.type === "folder") {
    return <Folder className="h-8 w-8 text-blue-500" />;
  }

  if (item.type === "image") {
    return <Image className="h-8 w-8 text-green-500" />;
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

const FileCard = ({ item, isSelected, onSelect, onDoubleClick }: FileCardProps) => {
  const [showContextMenu, setShowContextMenu] = useState(false);

  const handleContextMenu = (e: React.MouseEvent) => {
    e.preventDefault();
    setShowContextMenu(true);
  };

  return (
    <Card
      className={cn(
        "group relative cursor-pointer transition-all hover:shadow-md",
        isSelected && "ring-2 ring-primary bg-primary/5"
      )}
      onDoubleClick={onDoubleClick}
      onContextMenu={handleContextMenu}
    >
      {/* Selection Checkbox */}
      <div className="absolute top-2 left-2 z-10 opacity-0 group-hover:opacity-100 transition-opacity">
        <Checkbox
          checked={isSelected}
          onCheckedChange={onSelect}
          className="bg-background border-2"
        />
      </div>

      {/* Context Menu */}
      <div className="absolute top-2 right-2 z-10 opacity-0 group-hover:opacity-100 transition-opacity">
        <DropdownMenu open={showContextMenu} onOpenChange={setShowContextMenu}>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="sm" className="h-6 w-6 p-0 bg-background/80">
              <MoreVertical className="h-3 w-3" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem>
              <Eye className="h-4 w-4 mr-2" />
              Preview
            </DropdownMenuItem>
            <DropdownMenuItem>
              <Download className="h-4 w-4 mr-2" />
              Download
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem>
              <Copy className="h-4 w-4 mr-2" />
              Copy
            </DropdownMenuItem>
            <DropdownMenuItem>
              <Star className="h-4 w-4 mr-2" />
              Add to Starred
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem className="text-destructive">
              <Trash2 className="h-4 w-4 mr-2" />
              Delete
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="p-4">
        {/* Thumbnail/Icon */}
        <div className="flex items-center justify-center h-20 mb-3">
          {item.type === "image" && item.thumbnailUrl ? (
            <img
              src={item.thumbnailUrl}
              alt={item.name}
              className="max-h-full max-w-full object-cover rounded"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.style.display = "none";
                target.nextElementSibling?.classList.remove("hidden");
              }}
            />
          ) : null}
          <div className={item.type === "image" && item.thumbnailUrl ? "hidden" : ""}>
            {getFileIcon(item)}
          </div>
        </div>

        {/* File Info */}
        <div className="space-y-1">
          <div className="font-medium text-sm truncate" title={item.name}>
            {item.name}
          </div>
          <div className="text-xs text-muted-foreground">
            {item.formattedSize}
          </div>
          <div className="text-xs text-muted-foreground">
            {format(new Date(item.modifiedAt), "MMM d, yyyy")}
          </div>
        </div>
      </div>
    </Card>
  );
};

export const FileGrid = ({ items, selectedItems, onSelectItem, onDoubleClick }: FileGridProps) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6 gap-4">
      {items.map((item) => (
        <FileCard
          key={item.relativePath}
          item={item}
          isSelected={selectedItems.includes(item.relativePath)}
          onSelect={() => onSelectItem(item.relativePath)}
          onDoubleClick={() => onDoubleClick(item.relativePath, item.type)}
        />
      ))}
    </div>
  );
};
