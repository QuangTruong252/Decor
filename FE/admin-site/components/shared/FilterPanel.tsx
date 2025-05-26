"use client"

import * as React from "react"
import { Filter } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible"
import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"
import { hasActiveFilters } from "@/lib/query-utils"

interface FilterPanelProps {
  title?: string
  children: React.ReactNode
  filters: Record<string, unknown>
  onClearFilters: () => void
  isOpen?: boolean
  onOpenChange?: (open: boolean) => void
  className?: string
}

export function FilterPanel({
  title = "Filters",
  children,
  filters,
  onClearFilters,
  isOpen = false,
  onOpenChange,
  className,
}: FilterPanelProps) {
  const [internalOpen, setInternalOpen] = React.useState(isOpen)
  const activeFiltersCount = React.useMemo(() => {
    return Object.values(filters).filter(value => 
      value !== undefined && 
      value !== null && 
      value !== '' &&
      !(Array.isArray(value) && value.length === 0)
    ).length
  }, [filters])

  const isControlled = onOpenChange !== undefined
  const open = isControlled ? isOpen : internalOpen
  const setOpen = isControlled ? onOpenChange : setInternalOpen

  return (
    <Card className={cn("w-full", className)}>
      <Collapsible open={open} onOpenChange={setOpen}>
        <CollapsibleTrigger asChild>
          <CardHeader className="cursor-pointer hover:bg-muted/50 transition-colors">
            <CardTitle className="flex items-center justify-between text-base">
              <div className="flex items-center gap-2">
                <Filter className="h-4 w-4" />
                {title}
                {activeFiltersCount > 0 && (
                  <Badge variant="secondary" className="h-5 text-xs">
                    {activeFiltersCount}
                  </Badge>
                )}
              </div>
              <div className="flex items-center gap-2">
                {hasActiveFilters(filters) && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={(e) => {
                      e.stopPropagation()
                      onClearFilters()
                    }}
                    className="h-6 px-2 text-xs"
                  >
                    Clear
                  </Button>
                )}
              </div>
            </CardTitle>
          </CardHeader>
        </CollapsibleTrigger>
        <CollapsibleContent>
          <CardContent className="pt-0">
            {children}
          </CardContent>
        </CollapsibleContent>
      </Collapsible>
    </Card>
  )
}

interface FilterSectionProps {
  title?: string | null
  children: React.ReactNode
  className?: string
}

export function FilterSection({ title = null, children, className }: FilterSectionProps) {
  return (
    <div className={cn("space-y-2", className)}>
      {title && <h4 className="text-sm font-medium text-muted-foreground">{title}</h4>}
      {children}
    </div>
  )
}

interface FilterGroupProps {
  children: React.ReactNode
  className?: string
}

export function FilterGroup({ children, className }: FilterGroupProps) {
  return (
    <div className={cn("grid gap-4 md:grid-cols-2", className)}>
      {children}
    </div>
  )
}
