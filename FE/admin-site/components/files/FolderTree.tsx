/**
 * Folder Tree Component
 */

"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";
import { ChevronRight, ChevronDown, Folder, FolderOpen } from "lucide-react";
import { FolderStructure } from "@/types/fileManager";
import { cn } from "@/lib/utils";

interface FolderTreeProps {
  folderStructure?: FolderStructure;
  currentPath: string;
  onNavigateToPath: (path: string) => void;
}

interface FolderNodeProps {
  folder: FolderStructure;
  currentPath: string;
  onNavigateToPath: (path: string) => void;
  level?: number;
}

const FolderNode = ({ folder, currentPath, onNavigateToPath, level = 0 }: FolderNodeProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const isActive = currentPath === folder.relativePath;
  const hasChildren = folder.hasChildren && folder.subfolders.length > 0;

  const handleClick = () => {
    onNavigateToPath(folder.relativePath);
  };

  const handleToggle = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (hasChildren) {
      setIsOpen(!isOpen);
    }
  };

  return (
    <div className="select-none">
      <div
        className={cn(
          "flex items-center gap-1 rounded-md px-2 py-1 text-sm cursor-pointer hover:bg-accent",
          isActive && "bg-accent text-accent-foreground font-medium",
          level > 0 && "ml-4"
        )}
        onClick={handleClick}
        style={{ paddingLeft: `${level * 16 + 8}px` }}
      >
        {hasChildren ? (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleToggle}
            className="h-4 w-4 p-0 hover:bg-transparent"
          >
            {isOpen ? (
              <ChevronDown className="h-3 w-3" />
            ) : (
              <ChevronRight className="h-3 w-3" />
            )}
          </Button>
        ) : (
          <div className="w-4" />
        )}
        
        {isActive ? (
          <FolderOpen className="h-4 w-4 text-blue-500 flex-shrink-0" />
        ) : (
          <Folder className="h-4 w-4 text-blue-500 flex-shrink-0" />
        )}
        
        <span className="truncate flex-1">{folder.name}</span>
        
        {folder.totalItems > 0 && (
          <span className="text-xs text-muted-foreground">
            {folder.totalItems}
          </span>
        )}
      </div>

      {hasChildren && (
        <Collapsible open={isOpen} onOpenChange={setIsOpen}>
          <CollapsibleContent className="space-y-0">
            {folder.subfolders.map((subfolder) => (
              <FolderNode
                key={subfolder.relativePath}
                folder={subfolder}
                currentPath={currentPath}
                onNavigateToPath={onNavigateToPath}
                level={level + 1}
              />
            ))}
          </CollapsibleContent>
        </Collapsible>
      )}
    </div>
  );
};

export const FolderTree = ({ folderStructure, currentPath, onNavigateToPath }: FolderTreeProps) => {
  if (!folderStructure) {
    return (
      <div className="text-sm text-muted-foreground">
        No folders found
      </div>
    );
  }

  return (
    <div className="space-y-1">
      <FolderNode
        folder={folderStructure}
        currentPath={currentPath}
        onNavigateToPath={onNavigateToPath}
      />
    </div>
  );
};
