/**
 * Folder Tree Component
 */

"use client";

import { useEffect, useState } from "react";
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
  const isActive = currentPath.includes(folder.relativePath);
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

  useEffect(() => {
    if (isActive) {
      setIsOpen(true);
    }
  }, [isActive]);


  return (
    <div className="select-none my-2">
      <div
        className={cn(
          "flex items-center gap-1 rounded-md px-2 py-1 text-sm cursor-pointer hover:bg-accent",
          isActive && "bg-accent text-accent-foreground font-medium",
          level > 0 && "ml-2"
        )}
        onClick={handleClick}
        style={{ paddingLeft: `${level*level*3}px` }}
      >
        <Button
          variant="ghost"
          size="sm"
          onClick={hasChildren ? handleToggle : undefined}
          className="h-4 w-5 p-0 hover:bg-transparent"
        >
          {hasChildren ? (isOpen ? (
            <ChevronDown className="h-3 w-3" />
          ) : (
            <ChevronRight className="h-3 w-3" />
          )) : null}
        </Button>

        {isActive ? (
          <FolderOpen className="h-4 w-4 text-blue-500 flex-shrink-0" />
        ) : (
          <Folder className="h-4 w-4 text-blue-500 flex-shrink-0" />
        )}
        
        <span className="truncate flex-1 capitalize">{folder.name}</span>
        
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
    <div className="space-y-2">
      <FolderNode
        folder={folderStructure}
        currentPath={currentPath}
        onNavigateToPath={onNavigateToPath}
      />
    </div>
  );
};
