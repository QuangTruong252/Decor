/**
 * File List Component
 */

"use client";

import { useState } from "react";
import { Checkbox } from "@/components/ui/checkbox";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Folder,
  File,
  ImageIcon,
  FileText,
  Music,
  Video,
  Archive,
  MoreVertical,
  Eye,
} from "lucide-react";
import { FileItem } from "@/types/fileManager";
import { cn, getImageUrl } from "@/lib/utils";
import { format } from "date-fns";
import Image from "next/image";


interface FileListProps {
  items: FileItem[];
  selectedItems: string[];
  onSelectItem: (relativePath: string) => void;
  onDoubleClick: (relativePath: string, type: string) => void;
  onPreviewFile: (item: FileItem) => void;
}

interface FileRowProps {
  item: FileItem;
  isSelected: boolean;
  onSelect: () => void;
  onDoubleClick: () => void;
  onPreview: (item: FileItem) => void;
}

const getFileIcon = (item: FileItem) => {
  if (item.type === "folder") {
    return <Folder className="h-4 w-4 text-blue-500" />;
  }

  if (item.type === "image") {
    return <ImageIcon className="h-4 w-4 text-green-500" />;
  }

  const extension = item.extension?.toLowerCase() || "";
  
  if ([".pdf"].includes(extension)) {
    return <FileText className="h-4 w-4 text-red-500" />;
  }
  if ([".mp3", ".wav", ".flac", ".aac"].includes(extension)) {
    return <Music className="h-4 w-4 text-pink-500" />;
  }
  if ([".mp4", ".avi", ".mov", ".wmv"].includes(extension)) {
    return <Video className="h-4 w-4 text-purple-500" />;
  }
  if ([".zip", ".rar", ".7z"].includes(extension)) {
    return <Archive className="h-4 w-4 text-orange-500" />;
  }

  return <File className="h-4 w-4 text-gray-500" />;
};

const FileRow = ({
  item,
  isSelected,
  onSelect,
  onDoubleClick,
  onPreview,
}: FileRowProps) => {
  const [showContextMenu, setShowContextMenu] = useState(false);

  const handleContextMenu = (e: React.MouseEvent) => {
    e.preventDefault();
    setShowContextMenu(true);
  };

  const handlePreviewClick = () => {
    onPreview(item);
    setShowContextMenu(false);
  };

  return (
    <TableRow
      className={cn(
        "group cursor-pointer hover:bg-muted/50",
        isSelected && "bg-primary/5"
      )}
      onDoubleClick={onDoubleClick}
      onContextMenu={handleContextMenu}
    >
      {/* Selection */}
      <TableCell className="w-12">
        <Checkbox
          checked={isSelected}
          onCheckedChange={onSelect}
        />
      </TableCell>

      {/* Icon & Name */}
      <TableCell className="flex items-center gap-3">
        {item.type === "image" && item.relativePath ? (
          <Image
            src={getImageUrl(item.relativePath)}
            width={64}
            height={64}
            alt={item.name}
            className="h-8 w-8 object-cover rounded"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.style.display = "none";
              target.nextElementSibling?.classList.remove("hidden");
            }}
          />
        ) : null}
        <div className={item.type === "image" && item.relativePath ? "hidden" : ""}>
          {getFileIcon(item)}
        </div>
        <span className="font-medium truncate" title={item.name}>
          {item.name}
        </span>
      </TableCell>

      {/* Size */}
      <TableCell className="text-muted-foreground">
        {item.type === "folder" ? "—" : item.formattedSize}
      </TableCell>

      {/* Type */}
      <TableCell className="text-muted-foreground">
        {item.type === "folder" ? "Folder" : item.extension?.toUpperCase() || "File"}
      </TableCell>

      {/* Modified */}
      <TableCell className="text-muted-foreground">
        {format(new Date(item.modifiedAt), "MMM d, yyyy 'at' h:mm a")}
      </TableCell>

      {/* Actions */}
      <TableCell className="w-12">
        {item.type !== "folder" ? (
          <DropdownMenu open={showContextMenu} onOpenChange={setShowContextMenu}>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                className="h-8 w-8 p-0 opacity-0 group-hover:opacity-100"
              >
                <MoreVertical className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem className="cursor-pointer" onClick={handlePreviewClick}>
                <Eye className="h-4 w-4 mr-2" />
                Preview
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        ) : null}
      </TableCell>
    </TableRow>
  );
};

export const FileList = ({
  items,
  selectedItems,
  onSelectItem,
  onDoubleClick,
  onPreviewFile,
}: FileListProps) => {
  return (
    <div className="border rounded-lg">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-12"></TableHead>
            <TableHead>Name</TableHead>
            <TableHead className="w-24">Size</TableHead>
            <TableHead className="w-24">Type</TableHead>
            <TableHead className="w-48">Modified</TableHead>
            <TableHead className="w-12"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((item) => (
            <FileRow
              key={item.relativePath}
              item={item}
              isSelected={selectedItems.includes(item.relativePath)}
              onSelect={() => onSelectItem(item.relativePath)}
              onDoubleClick={() => onDoubleClick(item.relativePath, item.type)}
              onPreview={() => onPreviewFile(item)}
            />
          ))}
        </TableBody>
      </Table>
    </div>
  );
};
