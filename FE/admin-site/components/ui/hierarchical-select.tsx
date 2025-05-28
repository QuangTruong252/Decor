"use client"

import * as React from "react"
import { ChevronDown, Folder, FolderOpen } from "lucide-react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { CategoryDTO } from "@/services/categories"

interface HierarchicalSelectProps {
  categories: CategoryDTO[]
  value?: number | string
  onValueChange: (value: number | string | undefined) => void
  placeholder?: string
  disabled?: boolean
  allowClear?: boolean
  showPath?: boolean
  leafOnly?: boolean // Only show categories without children
  className?: string
  onScroll?: (event: React.UIEvent<HTMLDivElement>) => void
}

interface CategoryOption {
  category: CategoryDTO
  level: number
  path: string[]
  hasChildren: boolean
}

/**
 * Flattens hierarchical categories into a flat list with level information
 */
function flattenCategoriesWithLevel(categories: CategoryDTO[], level = 0, parentPath: string[] = []): CategoryOption[] {
  const result: CategoryOption[] = []

  categories.forEach(category => {
    const currentPath = [...parentPath, category.name]
    const hasChildren = category.subcategories && category.subcategories.length > 0

    result.push({
      category,
      level,
      path: currentPath,
      hasChildren: hasChildren || false
    })

    if (hasChildren) {
      result.push(...flattenCategoriesWithLevel(category.subcategories!, level + 1, currentPath))
    }
  })

  return result
}

/**
 * Finds category by ID in hierarchical structure
 */
function findCategoryById(categories: CategoryDTO[], id: number | string): CategoryDTO | undefined {
  for (const category of categories) {
    if (category.id.toString() === id.toString()) {
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
function getCategoryPath(categories: CategoryDTO[], categoryId: number | string): string[] {
  const flatOptions = flattenCategoriesWithLevel(categories)
  const option = flatOptions.find(opt => opt.category.id.toString() === categoryId.toString())
  return option?.path || []
}

export function HierarchicalSelect({
  categories,
  value,
  onValueChange,
  placeholder = "Select category...",
  disabled = false,
  allowClear = true,
  showPath = true,
  leafOnly = false,
  className,
  onScroll
}: HierarchicalSelectProps) {
  const [open, setOpen] = React.useState(false)
  const [searchValue, setSearchValue] = React.useState("")
  const commandListRef = React.useRef<HTMLDivElement>(null)

  // Add direct scroll event listener as backup
  React.useEffect(() => {
    const element = commandListRef.current
    if (element && onScroll) {
      const handleScroll = (e: Event) => {
        console.log('Direct scroll listener triggered:', {
          target: e.target,
          scrollTop: (e.target as HTMLElement).scrollTop,
          context: 'Direct listener'
        });
        onScroll(e as unknown as React.UIEvent<HTMLDivElement>);
      };

      element.addEventListener('scroll', handleScroll);
      return () => element.removeEventListener('scroll', handleScroll);
    }
  }, [onScroll]);

  // Debug effect to log when popover opens
  React.useEffect(() => {
    console.log('HierarchicalSelect popover state changed:', { open, hasOnScroll: !!onScroll });
  }, [open, onScroll]);

  const flatOptions = React.useMemo(() => {
    const allOptions = flattenCategoriesWithLevel(categories)
    return leafOnly ? allOptions.filter(option => !option.hasChildren) : allOptions
  }, [categories, leafOnly])

  const selectedCategory = React.useMemo(() => {
    if (!value) return null
    return findCategoryById(categories, value)
  }, [categories, value])

  const selectedPath = React.useMemo(() => {
    if (!value) return []
    return getCategoryPath(categories, value)
  }, [categories, value])

  const filteredOptions = React.useMemo(() => {
    if (!searchValue) return flatOptions

    return flatOptions.filter(option =>
      option.category.name.toLowerCase().includes(searchValue.toLowerCase()) ||
      option.path.some(pathPart =>
        pathPart.toLowerCase().includes(searchValue.toLowerCase())
      )
    )
  }, [flatOptions, searchValue])

  const handleSelect = (categoryId: string) => {
    if (categoryId === "clear") {
      onValueChange(undefined)
    } else {
      onValueChange(parseInt(categoryId))
    }
    setOpen(false)
    setSearchValue("")
  }

  const getDisplayValue = () => {
    if (!selectedCategory) return placeholder

    if (showPath && selectedPath.length > 1) {
      return selectedPath.join(" > ")
    }

    return selectedCategory.name
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className={cn("w-full justify-between", className)}
          disabled={disabled}
        >
          <span className="truncate text-left">
            {getDisplayValue()}
          </span>
          <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[--radix-popover-trigger-width] max-w-[400px] p-0 max-h-[300px]" align="start">
        <Command>
          <CommandInput
            placeholder="Search categories..."
            value={searchValue}
            onValueChange={setSearchValue}
          />
          <CommandList
            ref={commandListRef}
            onScroll={(e) => {
              console.log('HierarchicalSelect scroll event:', {
                target: e.target,
                currentTarget: e.currentTarget,
                scrollTop: (e.target as HTMLElement).scrollTop,
                scrollHeight: (e.target as HTMLElement).scrollHeight,
                clientHeight: (e.target as HTMLElement).clientHeight,
                context: 'HierarchicalSelect'
              });
              onScroll?.(e);
            }}
            className="max-h-[250px] overflow-y-auto scrollbar-thin"
          >
            <CommandEmpty>No categories found.</CommandEmpty>
            <CommandGroup className="p-1">
              {allowClear && (
                <CommandItem
                  value="clear"
                  onSelect={() => handleSelect("clear")}
                  className="text-muted-foreground"
                >
                  <span className="ml-6">Clear selection</span>
                </CommandItem>
              )}
              {filteredOptions.map((option) => (
                <CommandItem
                  key={option.category.id}
                  value={option.category.id.toString()}
                  onSelect={handleSelect}
                  className={cn(
                    "flex items-center gap-2",
                    value?.toString() === option.category.id.toString() && "bg-accent font-bold"
                  )}
                >
                  <div
                    className="flex items-center gap-1"
                    style={{ marginLeft: `${option.level * 16}px` }}
                  >
                    {option.hasChildren ? (
                      <FolderOpen className="h-4 w-4 text-blue-500" />
                    ) : (
                      <Folder className="h-4 w-4 text-gray-500" />
                    )}
                    <span className={cn(
                      option.level === 0 ? "font-medium" : "font-normal",
                      option.level > 0 && "text-muted-foreground"
                    )}>
                      {option.category.name}
                    </span>
                    {showPath && option.level > 0 && (
                      <span className="text-xs text-muted-foreground ml-2">
                        ({option.path.slice(0, -1).join(" > ")})
                      </span>
                    )}
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
