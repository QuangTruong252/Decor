"use client"

import * as React from "react"
import { useGetAllCategories } from "@/hooks/useCategories"
import { FilterSection, FilterGroup } from "@/components/shared/FilterPanel"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { DateRangePicker } from "@/components/ui/date-picker"
import { CategoryFilters } from "@/types/api"
import { formatDateForApi, getSelectValue, parseSelectValue } from "@/lib/query-utils"

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
  const { data: categoriesData } = useGetAllCategories()
  const categories = categoriesData || []

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
          <FilterGroup>
            <div className="w-full">
              <Label className="mb-2">Parent Category</Label>
              <Select
                value={getSelectValue(filters.parentId, "all")}
                onValueChange={(value) =>
                  handleFilterChange("parentId", parseSelectValue(value, "all", "number"))
                }
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Any parent..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Any Parent</SelectItem>
                  <SelectItem value="none">No Parent (Root Categories)</SelectItem>
                  {categories.map((category) => (
                    <SelectItem key={category.id} value={category.id.toString()}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </FilterGroup>
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
