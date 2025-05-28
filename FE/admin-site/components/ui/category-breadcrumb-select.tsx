"use client"

import * as React from "react"
import { ChevronDown, ChevronRight, Home, X } from "lucide-react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { CategoryDTO } from "@/services/categories"

interface CategoryBreadcrumbSelectProps {
  categories: CategoryDTO[]
  value?: number
  onValueChange: (value: number | undefined) => void
  placeholder?: string
  disabled?: boolean
  allowClear?: boolean
  className?: string
}

interface BreadcrumbLevel {
  categories: CategoryDTO[]
  selectedCategory?: CategoryDTO
}

/**
 * Finds category by ID in hierarchical structure
 */
function findCategoryById(categories: CategoryDTO[], id: number): CategoryDTO | undefined {
  for (const category of categories) {
    if (category.id === id) {
      return category
    }
    if (category.subcategories) {
      const found = findCategoryById(category.subcategories, id)
      if (found) return found
    }
  }
  return undefined
}

/**
 * Gets the full path for a category
 */
function getCategoryPath(categories: CategoryDTO[], categoryId: number): CategoryDTO[] {
  const path: CategoryDTO[] = []

  function findPath(cats: CategoryDTO[], targetId: number): boolean {
    for (const cat of cats) {
      path.push(cat)

      if (cat.id === targetId) {
        return true
      }

      if (cat.subcategories && findPath(cat.subcategories, targetId)) {
        return true
      }

      path.pop()
    }
    return false
  }

  findPath(categories, categoryId)
  return path
}

/**
 * Gets categories at a specific level in the path
 */
function getCategoriesAtLevel(categories: CategoryDTO[], path: CategoryDTO[], level: number): CategoryDTO[] {
  if (level === 0) {
    return categories
  }

  if (level > path.length) {
    return []
  }

  const parentCategory = path[level - 1]
  return parentCategory.subcategories || []
}

export function CategoryBreadcrumbSelect({
  categories,
  value,
  onValueChange,
  placeholder = "Select category...",
  disabled = false,
  allowClear = true,
  className
}: CategoryBreadcrumbSelectProps) {
  const selectedPath = React.useMemo(() => {
    if (!value) return []
    return getCategoryPath(categories, value)
  }, [categories, value])

  const breadcrumbLevels = React.useMemo(() => {
    const levels: BreadcrumbLevel[] = []

    // Root level
    levels.push({
      categories,
      selectedCategory: selectedPath[0]
    })

    // Subsequent levels
    for (let i = 0; i < selectedPath.length; i++) {
      const currentCategory = selectedPath[i]
      if (currentCategory.subcategories && currentCategory.subcategories.length > 0) {
        levels.push({
          categories: currentCategory.subcategories,
          selectedCategory: selectedPath[i + 1]
        })
      }
    }

    return levels
  }, [categories, selectedPath])

  const handleCategorySelect = (category: CategoryDTO) => {
    onValueChange(category.id)
  }

  const handleClear = () => {
    onValueChange(undefined)
  }

  const handleLevelSelect = (levelIndex: number, category: CategoryDTO) => {
    // If selecting a category at an earlier level, clear subsequent selections
    onValueChange(category.id)
  }

  return (
    <div className={cn("flex items-center gap-1 p-2 border rounded-md bg-background", className)}>
      {/* Home/Root indicator */}
      <div className="flex items-center gap-1">
        <Home className="h-4 w-4 text-muted-foreground" />

        {selectedPath.length === 0 && (
          <span className="text-muted-foreground text-sm">{placeholder}</span>
        )}
      </div>

      {/* Breadcrumb levels */}
      {breadcrumbLevels.map((level, levelIndex) => (
        <React.Fragment key={levelIndex}>
          {/* Separator */}
          {levelIndex > 0 && (
            <ChevronRight className="h-4 w-4 text-muted-foreground" />
          )}

          {/* Level dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                className={cn(
                  "h-auto p-1 font-normal",
                  level.selectedCategory ? "text-foreground" : "text-muted-foreground"
                )}
                disabled={disabled}
              >
                <span className="max-w-[120px] truncate">
                  {level.selectedCategory?.name || "Choose..."}
                </span>
                <ChevronDown className="ml-1 h-3 w-3" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="start" className="w-56 max-h-[300px] overflow-y-auto scrollbar-thin">
              {level.categories.map((category) => (
                <DropdownMenuItem
                  key={category.id}
                  onClick={() => handleLevelSelect(levelIndex, category)}
                  className={cn(
                    "flex items-center justify-between",
                    level.selectedCategory?.id === category.id && "bg-accent"
                  )}
                >
                  <span className="truncate">{category.name}</span>
                  {category.subcategories && category.subcategories.length > 0 && (
                    <ChevronRight className="h-3 w-3 text-muted-foreground" />
                  )}
                </DropdownMenuItem>
              ))}

              {/* Clear option for current level */}
              {levelIndex > 0 && level.selectedCategory && (
                <>
                  <div className="border-t my-1" />
                  <DropdownMenuItem
                    onClick={() => {
                      // Select the parent category (previous level)
                      const parentCategory = selectedPath[levelIndex - 1]
                      if (parentCategory) {
                        onValueChange(parentCategory.id)
                      } else {
                        onValueChange(undefined)
                      }
                    }}
                    className="text-muted-foreground"
                  >
                    Clear this level
                  </DropdownMenuItem>
                </>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        </React.Fragment>
      ))}

      {/* Clear all button */}
      {allowClear && selectedPath.length > 0 && (
        <>
          <div className="flex-1" />
          <Button
            variant="ghost"
            size="sm"
            onClick={handleClear}
            className="h-auto p-1 text-muted-foreground hover:text-foreground"
            disabled={disabled}
          >
            <X className="h-3 w-3" />
          </Button>
        </>
      )}
    </div>
  )
}

/**
 * Display-only breadcrumb component for showing category path
 */
interface CategoryBreadcrumbDisplayProps {
  categories: CategoryDTO[]
  categoryId?: number
  separator?: React.ReactNode
  className?: string
}

export function CategoryBreadcrumbDisplay({
  categories,
  categoryId,
  separator = <ChevronRight className="h-3 w-3 text-muted-foreground" />,
  className
}: CategoryBreadcrumbDisplayProps) {
  const path = React.useMemo(() => {
    if (!categoryId) return []
    return getCategoryPath(categories, categoryId)
  }, [categories, categoryId])

  if (path.length === 0) {
    return null
  }

  return (
    <div className={cn("flex items-center gap-1 text-sm", className)}>
      <Home className="h-3 w-3 text-muted-foreground" />
      {path.map((category, index) => (
        <React.Fragment key={category.id}>
          {separator}
          <span
            className={cn(
              index === path.length - 1
                ? "font-medium text-foreground"
                : "text-muted-foreground"
            )}
          >
            {category.name}
          </span>
        </React.Fragment>
      ))}
    </div>
  )
}
