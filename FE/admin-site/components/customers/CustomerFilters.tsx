"use client"

import * as React from "react"
import { FilterSection, FilterGroup } from "@/components/shared/FilterPanel"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { DateRangePicker } from "@/components/ui/date-picker"
import { CustomerFilters } from "@/types/api"
import { formatDateForApi, getSelectValue, parseSelectValue } from "@/lib/query-utils"

interface CustomerFiltersProps {
  filters: CustomerFilters
  onFiltersChange: (filters: CustomerFilters) => void
  onApply: () => void
  onClearAll: () => void
}

export function CustomerFiltersComponent({
  filters,
  onFiltersChange,
  onApply,
  onClearAll,
}: CustomerFiltersProps) {
  const handleFilterChange = (key: keyof CustomerFilters, value: unknown) => {
    onFiltersChange({
      ...filters,
      [key]: value,
    })
  }

  const handleDateRangeChange = (range: { from?: Date; to?: Date }) => {
    onFiltersChange({
      ...filters,
      registeredAfter: range.from ? formatDateForApi(range.from) : undefined,
      registeredBefore: range.to ? formatDateForApi(range.to) : undefined,
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
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                placeholder="Enter email..."
                value={filters.email || ""}
                onChange={(e) => handleFilterChange("email", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="city">City</Label>
              <Input
                id="city"
                placeholder="Enter city..."
                value={filters.city || ""}
                onChange={(e) => handleFilterChange("city", e.target.value)}
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Location Filters */}
        <FilterSection title="Location">
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="state">State</Label>
              <Input
                id="state"
                placeholder="Enter state..."
                value={filters.state || ""}
                onChange={(e) => handleFilterChange("state", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="country">Country</Label>
              <Input
                id="country"
                placeholder="Enter country..."
                value={filters.country || ""}
                onChange={(e) => handleFilterChange("country", e.target.value)}
              />
            </div>
          </FilterGroup>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="postalCode">Postal Code</Label>
              <Input
                id="postalCode"
                placeholder="Enter postal code..."
                value={filters.postalCode || ""}
                onChange={(e) => handleFilterChange("postalCode", e.target.value)}
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Date Range Filter */}
        <FilterSection title="Registration Date">
          <div className="space-y-2">
            <Label>Registration Date Range</Label>
            <DateRangePicker
              from={filters.registeredAfter ? new Date(filters.registeredAfter) : undefined}
              to={filters.registeredBefore ? new Date(filters.registeredBefore) : undefined}
              onDateRangeChange={handleDateRangeChange}
              placeholder="Select date range..."
            />
          </div>
        </FilterSection>

        {/* Additional Options */}
        <FilterSection title="Additional Options">
          <div className="space-y-3">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="hasOrders"
                checked={filters.hasOrders === true}
                onCheckedChange={(checked) =>
                  handleFilterChange("hasOrders", checked === true ? true : undefined)
                }
              />
              <Label htmlFor="hasOrders">Has Orders</Label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="includeOrderCount"
                checked={filters.includeOrderCount === true}
                onCheckedChange={(checked) =>
                  handleFilterChange("includeOrderCount", checked === true ? true : undefined)
                }
              />
              <Label htmlFor="includeOrderCount">Include Order Count</Label>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="includeTotalSpent"
                checked={filters.includeTotalSpent === true}
                onCheckedChange={(checked) =>
                  handleFilterChange("includeTotalSpent", checked === true ? true : undefined)
                }
              />
              <Label htmlFor="includeTotalSpent">Include Total Spent</Label>
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
      <div className="flex justify-between pt-6 border-t">
        <Button variant="outline" onClick={handleClearFilters}>
          Clear Filters
        </Button>
        <div className="flex gap-2">
          <Button variant="outline" onClick={onClearAll}>
            Clear All
          </Button>
          <Button onClick={onApply}>
            Apply Filters
          </Button>
        </div>
      </div>
    </div>
  )
}
