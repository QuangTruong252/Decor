"use client"

import * as React from "react"
import { ChevronDown, ChevronRight, Folder, FolderOpen, Plus, Minus } from "lucide-react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { CategoryDTO } from "@/services/categories"

interface CategoryTreeProps {
  categories: CategoryDTO[]
  selectedIds?: number[]
  onSelectionChange?: (selectedIds: number[]) => void
  multiSelect?: boolean
  showCheckboxes?: boolean
  expandedByDefault?: boolean
  onCategoryClick?: (category: CategoryDTO) => void
  className?: string
}

interface CategoryTreeNodeProps {
  category: CategoryDTO
  level: number
  isSelected: boolean
  isExpanded: boolean
  onToggleExpand: (categoryId: number) => void
  onSelect: (categoryId: number, selected: boolean) => void
  onCategoryClick?: (category: CategoryDTO) => void
  showCheckboxes: boolean
  multiSelect: boolean
}

function CategoryTreeNode({
  category,
  level,
  isSelected,
  isExpanded,
  onToggleExpand,
  onSelect,
  onCategoryClick,
  showCheckboxes,
  multiSelect
}: CategoryTreeNodeProps) {
  const hasChildren = category.subcategories && category.subcategories.length > 0
  const indentWidth = level * 20

  const handleClick = () => {
    if (onCategoryClick) {
      onCategoryClick(category)
    } else if (!showCheckboxes) {
      onSelect(category.id, !isSelected)
    }
  }

  const handleCheckboxChange = (checked: boolean) => {
    onSelect(category.id, checked)
  }

  return (
    <div className="select-none">
      <div
        className={cn(
          "flex items-center gap-2 py-1 px-2 rounded-md hover:bg-accent/50 cursor-pointer",
          isSelected && !showCheckboxes && "bg-accent",
          "transition-colors"
        )}
        style={{ paddingLeft: `${indentWidth + 8}px` }}
      >
        {/* Expand/Collapse Button */}
        <Button
          variant="ghost"
          size="sm"
          className="h-4 w-4 p-0 hover:bg-transparent"
          onClick={(e) => {
            e.stopPropagation()
            if (hasChildren) {
              onToggleExpand(category.id)
            }
          }}
          disabled={!hasChildren}
        >
          {hasChildren ? (
            isExpanded ? (
              <ChevronDown className="h-3 w-3" />
            ) : (
              <ChevronRight className="h-3 w-3" />
            )
          ) : (
            <div className="h-3 w-3" />
          )}
        </Button>

        {/* Checkbox */}
        {showCheckboxes && (
          <Checkbox
            checked={isSelected}
            onCheckedChange={handleCheckboxChange}
            onClick={(e) => e.stopPropagation()}
          />
        )}

        {/* Folder Icon */}
        <div className="flex items-center gap-1">
          {hasChildren ? (
            isExpanded ? (
              <FolderOpen className="h-4 w-4 text-blue-500" />
            ) : (
              <Folder className="h-4 w-4 text-blue-600" />
            )
          ) : (
            <Folder className="h-4 w-4 text-gray-500" />
          )}
        </div>

        {/* Category Name */}
        <span
          className={cn(
            "flex-1 text-sm truncate",
            level === 0 ? "font-medium" : "font-normal",
            isSelected && !showCheckboxes && "text-accent-foreground"
          )}
          onClick={handleClick}
        >
          {category.name}
        </span>

        {/* Product Count Badge */}
        {category.productCount !== undefined && (
          <span className="text-xs text-muted-foreground bg-muted px-2 py-0.5 rounded-full">
            {category.productCount}
          </span>
        )}
      </div>

      {/* Children */}
      {hasChildren && isExpanded && (
        <div className="ml-2">
          {category.subcategories!.map((child) => (
            <CategoryTreeNode
              key={child.id}
              category={child}
              level={level + 1}
              isSelected={isSelected}
              isExpanded={isExpanded}
              onToggleExpand={onToggleExpand}
              onSelect={onSelect}
              onCategoryClick={onCategoryClick}
              showCheckboxes={showCheckboxes}
              multiSelect={multiSelect}
            />
          ))}
        </div>
      )}
    </div>
  )
}

export function CategoryTree({
  categories,
  selectedIds = [],
  onSelectionChange,
  multiSelect = false,
  showCheckboxes = false,
  expandedByDefault = false,
  onCategoryClick,
  className
}: CategoryTreeProps) {
  const [expandedIds, setExpandedIds] = React.useState<Set<number>>(
    new Set(expandedByDefault ? getAllCategoryIds(categories) : [])
  )
  const [internalSelectedIds, setInternalSelectedIds] = React.useState<number[]>(selectedIds)

  // Update internal state when prop changes
  React.useEffect(() => {
    setInternalSelectedIds(selectedIds)
  }, [selectedIds])

  const handleToggleExpand = (categoryId: number) => {
    setExpandedIds(prev => {
      const newSet = new Set(prev)
      if (newSet.has(categoryId)) {
        newSet.delete(categoryId)
      } else {
        newSet.add(categoryId)
      }
      return newSet
    })
  }

  const handleSelect = (categoryId: number, selected: boolean) => {
    let newSelectedIds: number[]

    if (multiSelect || showCheckboxes) {
      if (selected) {
        newSelectedIds = [...internalSelectedIds, categoryId]
      } else {
        newSelectedIds = internalSelectedIds.filter(id => id !== categoryId)
      }
    } else {
      newSelectedIds = selected ? [categoryId] : []
    }

    setInternalSelectedIds(newSelectedIds)
    onSelectionChange?.(newSelectedIds)
  }

  const handleExpandAll = () => {
    setExpandedIds(new Set(getAllCategoryIds(categories)))
  }

  const handleCollapseAll = () => {
    setExpandedIds(new Set())
  }

  return (
    <div className={cn("space-y-1", className)}>
      {/* Expand/Collapse All Controls */}
      <div className="flex items-center gap-2 mb-2 pb-2 border-b">
        <Button
          variant="outline"
          size="sm"
          onClick={handleExpandAll}
          className="h-7 px-2 text-xs"
        >
          <Plus className="h-3 w-3 mr-1" />
          Expand All
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={handleCollapseAll}
          className="h-7 px-2 text-xs"
        >
          <Minus className="h-3 w-3 mr-1" />
          Collapse All
        </Button>
        {showCheckboxes && internalSelectedIds.length > 0 && (
          <span className="text-xs text-muted-foreground ml-auto">
            {internalSelectedIds.length} selected
          </span>
        )}
      </div>

      {/* Tree Nodes */}
      <div className="space-y-0.5 max-h-[400px] overflow-y-auto pr-2 scrollbar-thin">
        {categories.map((category) => (
          <CategoryTreeNode
            key={category.id}
            category={category}
            level={0}
            isSelected={internalSelectedIds.includes(category.id)}
            isExpanded={expandedIds.has(category.id)}
            onToggleExpand={handleToggleExpand}
            onSelect={handleSelect}
            onCategoryClick={onCategoryClick}
            showCheckboxes={showCheckboxes}
            multiSelect={multiSelect}
          />
        ))}
      </div>
    </div>
  )
}

/**
 * Helper function to get all category IDs from hierarchical structure
 */
function getAllCategoryIds(categories: CategoryDTO[]): number[] {
  const ids: number[] = []

  function collectIds(cats: CategoryDTO[]) {
    cats.forEach(cat => {
      ids.push(cat.id)
      if (cat.subcategories) {
        collectIds(cat.subcategories)
      }
    })
  }

  collectIds(categories)
  return ids
}
