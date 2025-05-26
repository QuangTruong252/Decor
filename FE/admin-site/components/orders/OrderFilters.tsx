"use client"

import * as React from "react"
import { FilterSection, FilterGroup } from "@/components/shared/FilterPanel"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { DateRangePicker } from "@/components/ui/date-picker"
import { OrderFilters, ORDER_STATUS_OPTIONS } from "@/types/api"
import { formatDateForApi, getSelectValue, parseSelectValue } from "@/lib/query-utils"

interface OrderFiltersProps {
  filters: OrderFilters
  onFiltersChange: (filters: OrderFilters) => void
  onApply: () => void
  onClearAll: () => void
}

export function OrderFiltersComponent({
  filters,
  onFiltersChange,
  onApply,
  onClearAll,
}: OrderFiltersProps) {
  const handleFilterChange = (key: keyof OrderFilters, value: unknown) => {
    onFiltersChange({
      ...filters,
      [key]: value,
    })
  }

  const handleDateRangeChange = (range: { from?: Date; to?: Date }) => {
    onFiltersChange({
      ...filters,
      orderDateFrom: range.from ? formatDateForApi(range.from) : undefined,
      orderDateTo: range.to ? formatDateForApi(range.to) : undefined,
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
              <Label htmlFor="userId">User ID</Label>
              <Input
                id="userId"
                type="number"
                placeholder="Enter user ID..."
                value={filters.userId || ""}
                onChange={(e) =>
                  handleFilterChange(
                    "userId",
                    e.target.value ? parseInt(e.target.value) : undefined
                  )
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="customerId">Customer ID</Label>
              <Input
                id="customerId"
                type="number"
                placeholder="Enter customer ID..."
                value={filters.customerId || ""}
                onChange={(e) =>
                  handleFilterChange(
                    "customerId",
                    e.target.value ? parseInt(e.target.value) : undefined
                  )
                }
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Status and Payment */}
        <FilterSection>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="orderStatus">Order Status</Label>
              <Select
                value={getSelectValue(filters.orderStatus, "all")}
                onValueChange={(value) =>
                  handleFilterChange(
                    "orderStatus",
                    parseSelectValue(value, "all", "string")
                  )
                }
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Any status..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Any Status</SelectItem>
                  {ORDER_STATUS_OPTIONS.map((status) => (
                    <SelectItem key={status.value} value={status.value}>
                      {status.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="paymentMethod">Payment Method</Label>
              <Input
                id="paymentMethod"
                placeholder="Enter payment method..."
                value={filters.paymentMethod || ""}
                onChange={(e) =>
                  handleFilterChange("paymentMethod", e.target.value)
                }
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Amount Range */}
        <FilterSection>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="minAmount">Minimum Amount</Label>
              <Input
                id="minAmount"
                type="number"
                step="0.01"
                placeholder="0.00"
                value={filters.minAmount || ""}
                onChange={(e) =>
                  handleFilterChange(
                    "minAmount",
                    e.target.value ? parseFloat(e.target.value) : undefined
                  )
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="maxAmount">Maximum Amount</Label>
              <Input
                id="maxAmount"
                type="number"
                step="0.01"
                placeholder="0.00"
                value={filters.maxAmount || ""}
                onChange={(e) =>
                  handleFilterChange(
                    "maxAmount",
                    e.target.value ? parseFloat(e.target.value) : undefined
                  )
                }
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Date Range Filter */}
        <FilterSection>
          <div className="space-y-2">
            <Label>Order Date Range</Label>
            <DateRangePicker
              from={
                filters.orderDateFrom
                  ? new Date(filters.orderDateFrom)
                  : undefined
              }
              to={
                filters.orderDateTo ? new Date(filters.orderDateTo) : undefined
              }
              onDateRangeChange={handleDateRangeChange}
              placeholder="Select date range..."
            />
          </div>
        </FilterSection>

        {/* Shipping Location */}
        <FilterSection>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="shippingCity">Shipping City</Label>
              <Input
                id="shippingCity"
                placeholder="Enter shipping city..."
                value={filters.shippingCity || ""}
                onChange={(e) =>
                  handleFilterChange("shippingCity", e.target.value)
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="shippingState">Shipping State</Label>
              <Input
                id="shippingState"
                placeholder="Enter shipping state..."
                value={filters.shippingState || ""}
                onChange={(e) =>
                  handleFilterChange("shippingState", e.target.value)
                }
              />
            </div>
          </FilterGroup>
          <FilterGroup>
            <div className="space-y-2">
              <Label htmlFor="shippingCountry">Shipping Country</Label>
              <Input
                id="shippingCountry"
                placeholder="Enter shipping country..."
                value={filters.shippingCountry || ""}
                onChange={(e) =>
                  handleFilterChange("shippingCountry", e.target.value)
                }
              />
            </div>
          </FilterGroup>
        </FilterSection>

        {/* Additional Options */}
        <FilterSection>
          <div className="space-y-3">
            <Label>Additional Options</Label>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="includeDeleted"
                checked={filters.includeDeleted === true}
                onCheckedChange={(checked) =>
                  handleFilterChange(
                    "includeDeleted",
                    checked === true ? true : undefined
                  )
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
        <Button onClick={onApply}>Apply Filters</Button>
      </div>
    </div>
  );
}
