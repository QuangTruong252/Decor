"use client"

import * as React from "react"
import { useHierarchicalCategories } from "@/hooks/useCategoryStore"
import { FilterSection, FilterGroup } from "@/components/shared/FilterPanel"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { HierarchicalSelect } from "@/components/ui/hierarchical-select"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { DateRangePicker } from "@/components/ui/date-picker"
import { ProductFilters } from "@/types/api"
import { formatDateForApi, getSelectValue, parseSelectValue } from "@/lib/query-utils"

interface ProductFiltersProps {
  filters: ProductFilters
  onFiltersChange: (filters: ProductFilters) => void
  onApply?: () => void
  onClearAll?: () => void
  isOpen?: boolean
  onOpenChange?: (open: boolean) => void
}

export function ProductFiltersComponent({
  filters,
  onFiltersChange,
  onApply,
  onClearAll,
}: ProductFiltersProps) {
  const { data: categories } = useHierarchicalCategories()

  const handleFilterChange = (key: keyof ProductFilters, value: unknown) => {
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
            <div className="space-y-2">
              <Label htmlFor="sku">SKU</Label>
              <Input
                id="sku"
                placeholder="Enter SKU..."
                value={filters.sku || ""}
                onChange={(e) => handleFilterChange("sku", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="category">Category</Label>
              <HierarchicalSelect
                categories={categories || []}
                value={filters.categoryId}
                onValueChange={(value) => handleFilterChange("categoryId", value)}
                placeholder="Select category..."
                allowClear={true}
                showPath={true}
                className="w-full"
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Price Range */}
        <FilterSection>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="minPrice">Min Price</Label>
              <Input
                id="minPrice"
                type="number"
                placeholder="0.00"
                value={filters.minPrice || ""}
                onChange={(e) =>
                  handleFilterChange("minPrice", e.target.value ? parseFloat(e.target.value) : undefined)
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="maxPrice">Max Price</Label>
              <Input
                id="maxPrice"
                type="number"
                placeholder="999.99"
                value={filters.maxPrice || ""}
                onChange={(e) =>
                  handleFilterChange("maxPrice", e.target.value ? parseFloat(e.target.value) : undefined)
                }
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Stock Range */}
        <FilterSection>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="stockMin">Min Stock</Label>
              <Input
                id="stockMin"
                type="number"
                placeholder="0"
                value={filters.stockQuantityMin || ""}
                onChange={(e) =>
                  handleFilterChange("stockQuantityMin", e.target.value ? parseInt(e.target.value) : undefined)
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="stockMax">Max Stock</Label>
              <Input
                id="stockMax"
                type="number"
                placeholder="1000"
                value={filters.stockQuantityMax || ""}
                onChange={(e) =>
                  handleFilterChange("stockQuantityMax", e.target.value ? parseInt(e.target.value) : undefined)
                }
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Rating and Status */}
        <FilterSection>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="minRating">Min Rating</Label>
              <Select
                value={getSelectValue(filters.minRating, "any")}
                onValueChange={(value) =>
                  handleFilterChange("minRating", parseSelectValue(value, "any", "float"))
                }
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Any rating..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="any">Any Rating</SelectItem>
                  <SelectItem value="1">1+ Stars</SelectItem>
                  <SelectItem value="2">2+ Stars</SelectItem>
                  <SelectItem value="3">3+ Stars</SelectItem>
                  <SelectItem value="4">4+ Stars</SelectItem>
                  <SelectItem value="5">5 Stars</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-3 flex flex-col justify-end">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="featured"
                  checked={filters.isFeatured === true}
                  onCheckedChange={(checked) =>
                    handleFilterChange("isFeatured", checked === true ? true : undefined)
                  }
                />
                <Label htmlFor="featured">Featured products only</Label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="active"
                  checked={filters.isActive === true}
                  onCheckedChange={(checked) =>
                    handleFilterChange("isActive", checked === true ? true : undefined)
                  }
                />
                <Label htmlFor="active">Active products only</Label>
              </div>
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Date Range */}
        <FilterSection title="Created Date">
          <DateRangePicker
            from={filters.createdAfter ? new Date(filters.createdAfter) : undefined}
            to={filters.createdBefore ? new Date(filters.createdBefore) : undefined}
            onDateRangeChange={handleDateRangeChange}
            placeholder="Select date range..."
          />
        </FilterSection>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-between pt-4">
        <Button
          variant="outline"
          onClick={onClearAll || handleClearFilters}
        >
          Clear Filters
        </Button>
        {onApply && (
          <Button onClick={onApply}>
            Apply Filters
          </Button>
        )}
      </div>
    </div>
  )
}
