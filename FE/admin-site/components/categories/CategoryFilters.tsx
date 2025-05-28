"use client"

import * as React from "react"
import { useHierarchicalCategories } from "@/hooks/useCategoryStore"
import { FilterSection } from "@/components/shared/FilterPanel"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { HierarchicalSelect } from "@/components/ui/hierarchical-select"
import { Checkbox } from "@/components/ui/checkbox"
import { DateRangePicker } from "@/components/ui/date-picker"
import { CategoryFilters } from "@/types/api"
import { formatDateForApi } from "@/lib/query-utils"

interface CategoryFiltersProps {
  filters: CategoryFilters
  onFiltersChange: (filters: CategoryFilters) => void
  onApply: () => void
  onClearAll: () => void
}

export function CategoryFiltersComponent({
  filters,
  onFiltersChange,
  onApply,
  onClearAll,
}: CategoryFiltersProps) {
  const { data: categories } = useHierarchicalCategories()

  const handleFilterChange = (key: keyof CategoryFilters, value: unknown) => {
    onFiltersChange({
      ...filters,
      [key]: value,
    })
  }

  const handleDateRangeChange = (range: { from?: Date; to?: Date }) => {
    onFiltersChange({
      ...filters,
      createdAfter: range.from ? formatDateForApi(range.from) : undefined,
      createdBefore: range.to ? formatDateForApi(range.to) : undefined,
    })
  }

  const handleClearFilters = () => {
    onFiltersChange({
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
      sortBy: filters.sortBy,
      sortDirection: filters.sortDirection,
      isDescending: filters.isDescending,
    })
  }

  return (
    <div>
      <div className="space-y-6">
        {/* Basic Filters */}
        <FilterSection>
          <div className="w-full">
            <Label className="mb-2">Parent Category</Label>
            <HierarchicalSelect
              categories={categories || []}
              value={filters.parentId}
              onValueChange={(value) => handleFilterChange("parentId", value)}
              placeholder="Any parent..."
              allowClear={true}
              showPath={false}
              className="w-full"
            />
          </div>
        </FilterSection>

        {/* Date Range Filter */}
        <FilterSection>
          <div className="space-y-2">
            <Label>Creation Date Range</Label>
            <DateRangePicker
              from={filters.createdAfter ? new Date(filters.createdAfter) : undefined}
              to={filters.createdBefore ? new Date(filters.createdBefore) : undefined}
              onDateRangeChange={handleDateRangeChange}
              placeholder="Select date range..."
            />
          </div>
        </FilterSection>

        {/* Additional Options */}
        <FilterSection>
          <div className="space-y-3">
            <Label>Additional Options</Label>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="isRootCategory"
                checked={filters.isRootCategory === true}
                onCheckedChange={(checked) =>
                  handleFilterChange("isRootCategory", checked === true ? true : undefined)
                }
              />
              <Label htmlFor="isRootCategory">Root Categories Only</Label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="includeSubcategories"
                checked={filters.includeSubcategories === true}
                onCheckedChange={(checked) =>
                  handleFilterChange("includeSubcategories", checked === true ? true : undefined)
                }
              />
              <Label htmlFor="includeSubcategories">Include Subcategories</Label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="includeProductCount"
                checked={filters.includeProductCount === true}
                onCheckedChange={(checked) =>
                  handleFilterChange("includeProductCount", checked === true ? true : undefined)
                }
              />
              <Label htmlFor="includeProductCount">Include Product Count</Label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="includeDeleted"
                checked={filters.includeDeleted === true}
                onCheckedChange={(checked) =>
                  handleFilterChange("includeDeleted", checked === true ? true : undefined)
                }
              />
              <Label htmlFor="includeDeleted">Include Deleted</Label>
            </div>
          </div>
        </FilterSection>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-end gap-2 pt-6">
        <Button variant="outline" onClick={onClearAll || handleClearFilters}>
          Clear Filters
        </Button>
        <Button onClick={onApply}>
          Apply Filters
        </Button>
      </div>
    </div>
  )
}
