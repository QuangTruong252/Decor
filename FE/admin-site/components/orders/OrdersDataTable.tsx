"use client"

import * as React from "react"
import { useState, useEffect, useRef } from "react"
import { useGetOrders, useDeleteOrder } from "@/hooks/useOrders"
import { OrderFiltersComponent } from "./OrderFilters"
import { OrderDetailDialog } from "./OrderDetailDialog"
import { OrderStatusDialog } from "./OrderStatusDialog"
import { AddOrderDialog } from "./OrderDialog"
import { DeleteOrderButton } from "./DeleteOrderButton"
import { OrderStatusBadge } from "./OrderStatusBadge"
import { SearchInput } from "@/components/shared/SearchInput"
import { Pagination } from "@/components/ui/pagination"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { formatCurrency, cn } from "@/lib/utils"
import { OrderFilters } from "@/types/api"
import { createDefaultPagination, hasActiveFilters } from "@/lib/query-utils"
import { Filter, ArrowUpDown, ArrowUp, ArrowDown, X } from "lucide-react"
import { format } from "date-fns"

export function OrdersDataTable() {
  // State for applied filters (what's actually filtering the table)
  const [appliedFilters, setAppliedFilters] = useState<OrderFilters>({
    ...createDefaultPagination(25),
    searchTerm: "",
  })

  // State for pending filters (what user is editing in dialog)
  const [pendingFilters, setPendingFilters] = useState<OrderFilters>({
    ...createDefaultPagination(25),
    searchTerm: "",
  })

  // State for UI
  const [filtersOpen, setFiltersOpen] = useState(false)
  const [tableHeight, setTableHeight] = useState<number>(400)

  // Refs for height calculation
  const containerRef = useRef<HTMLDivElement>(null)
  const headerRef = useRef<HTMLDivElement>(null)
  const toolbarRef = useRef<HTMLDivElement>(null)
  const paginationRef = useRef<HTMLDivElement>(null)
  const activeFiltersRef = useRef<HTMLDivElement>(null)

  // API calls
  const { data, isLoading, error, refetch } = useGetOrders(appliedFilters)
  const deleteOrderMutation = useDeleteOrder()

  // Derived state
  const orders = data?.items || []
  const pagination = data?.pagination || {
    currentPage: 1,
    pageSize: 25,
    totalCount: 0,
    totalPages: 1,
    hasNext: false,
    hasPrevious: false,
  }

  const calculateTableHeight = () => {
    if (!containerRef.current) return;

    const containerHeight = window.innerHeight;
    const headerHeight = headerRef.current?.offsetHeight || 0;
    const toolbarHeight = toolbarRef.current?.offsetHeight || 0;
    const paginationHeight = paginationRef.current?.offsetHeight || 0;
    const activeFiltersHeight = activeFiltersRef.current?.offsetHeight || 0;

    // Account for padding, margins, and other spacing (approximately 120px)
    const otherSpacing = 260;
    const minTableHeight = 200; // Minimum height for at least 2 rows

    const calculatedHeight =
      containerHeight -
      headerHeight -
      toolbarHeight -
      paginationHeight -
      activeFiltersHeight -
      otherSpacing;
    const finalHeight = Math.max(minTableHeight, calculatedHeight);

    setTableHeight(finalHeight);
  };

  // Filter handlers for pending filters (dialog editing)
  const handlePendingFiltersChange = (newFilters: OrderFilters) => {
    setPendingFilters(newFilters)
  }

  // Apply filters from dialog to actual table
  const handleApplyFilters = () => {
    setAppliedFilters(pendingFilters)
    setFiltersOpen(false)
  }

  // Clear all filters
  const handleClearAllFilters = () => {
    const clearedFilters = {
      pageNumber: appliedFilters.pageNumber,
      pageSize: appliedFilters.pageSize,
      sortBy: appliedFilters.sortBy,
      sortDirection: appliedFilters.sortDirection,
      isDescending: appliedFilters.isDescending,
    }
    setPendingFilters(clearedFilters)
    setAppliedFilters(clearedFilters)
    setFiltersOpen(false)
  }

  // Handle dialog open - initialize pending filters with applied filters
  const handleFiltersDialogOpen = (open: boolean) => {
    if (open) {
      setPendingFilters(appliedFilters)
    }
    setFiltersOpen(open)
  }

  const handleSearchChange = (searchTerm: string) => {
    setAppliedFilters(prev => ({
      ...prev,
      searchTerm,
      pageNumber: 1, // Reset to first page on search
    }))
  }

  const handlePageChange = (pageNumber: number) => {
    setAppliedFilters(prev => ({ ...prev, pageNumber }))
  }

  const handlePageSizeChange = (pageSize: number) => {
    setAppliedFilters(prev => ({
      ...prev,
      pageSize,
      pageNumber: 1 // Reset to first page when changing page size
    }))
  }

  // Column header sorting
  const handleColumnSort = (column: string) => {
    const currentDirection = appliedFilters.sortBy === column ? appliedFilters.sortDirection : "desc"
    const newDirection = currentDirection === "asc" ? "desc" : "asc"

    setAppliedFilters(prev => ({
      ...prev,
      sortBy: column,
      sortDirection: newDirection,
      isDescending: newDirection === "desc",
    }))
  }

  // Get sort icon for column headers
  const getSortIcon = (column: string) => {
    if (appliedFilters.sortBy !== column) {
      return <ArrowUpDown className="ml-2 h-4 w-4" />
    }
    return appliedFilters.sortDirection === "asc" ?
      <ArrowUp className="ml-2 h-4 w-4" /> :
      <ArrowDown className="ml-2 h-4 w-4" />
  }

  // Get active filter labels for display
  const getActiveFilterLabels = () => {
    const labels: Array<{ key: string; label: string; value: string | number | boolean }> = []

    if (appliedFilters.userId) {
      labels.push({
        key: 'userId',
        label: `User ID: ${appliedFilters.userId}`,
        value: appliedFilters.userId
      })
    }

    if (appliedFilters.customerId) {
      labels.push({
        key: 'customerId',
        label: `Customer ID: ${appliedFilters.customerId}`,
        value: appliedFilters.customerId
      })
    }

    if (appliedFilters.orderStatus) {
      labels.push({
        key: 'orderStatus',
        label: `Status: ${appliedFilters.orderStatus}`,
        value: appliedFilters.orderStatus
      })
    }

    if (appliedFilters.paymentMethod) {
      labels.push({
        key: 'paymentMethod',
        label: `Payment: ${appliedFilters.paymentMethod}`,
        value: appliedFilters.paymentMethod
      })
    }

    if (appliedFilters.minAmount || appliedFilters.maxAmount) {
      const amountRange = []
      if (appliedFilters.minAmount) amountRange.push(`$${appliedFilters.minAmount}`)
      if (appliedFilters.maxAmount) amountRange.push(`$${appliedFilters.maxAmount}`)
      labels.push({
        key: 'amount',
        label: `Amount: ${amountRange.join(' - ')}`,
        value: 'amount'
      })
    }

    if (appliedFilters.orderDateFrom || appliedFilters.orderDateTo) {
      const dateRange = []
      if (appliedFilters.orderDateFrom) {
        dateRange.push(new Date(appliedFilters.orderDateFrom).toLocaleDateString())
      }
      if (appliedFilters.orderDateTo) {
        dateRange.push(new Date(appliedFilters.orderDateTo).toLocaleDateString())
      }
      labels.push({
        key: 'dateRange',
        label: `Date: ${dateRange.join(' - ')}`,
        value: 'dateRange'
      })
    }

    if (appliedFilters.shippingCity) {
      labels.push({
        key: 'shippingCity',
        label: `Shipping City: ${appliedFilters.shippingCity}`,
        value: appliedFilters.shippingCity
      })
    }

    if (appliedFilters.shippingState) {
      labels.push({
        key: 'shippingState',
        label: `Shipping State: ${appliedFilters.shippingState}`,
        value: appliedFilters.shippingState
      })
    }

    if (appliedFilters.shippingCountry) {
      labels.push({
        key: 'shippingCountry',
        label: `Shipping Country: ${appliedFilters.shippingCountry}`,
        value: appliedFilters.shippingCountry
      })
    }

    return labels
  }

  // Remove individual filter
  const removeFilter = (filterKey: string) => {
    const newFilters = { ...appliedFilters }

    switch (filterKey) {
      case 'userId':
        delete newFilters.userId
        break
      case 'customerId':
        delete newFilters.customerId
        break
      case 'orderStatus':
        delete newFilters.orderStatus
        break
      case 'paymentMethod':
        delete newFilters.paymentMethod
        break
      case 'amount':
        delete newFilters.minAmount
        delete newFilters.maxAmount
        break
      case 'dateRange':
        delete newFilters.orderDateFrom
        delete newFilters.orderDateTo
        break
      case 'shippingCity':
        delete newFilters.shippingCity
        break
      case 'shippingState':
        delete newFilters.shippingState
        break
      case 'shippingCountry':
        delete newFilters.shippingCountry
        break
    }

    setAppliedFilters(newFilters)
  }

  // Action handlers
  const handleDelete = async (orderId: number) => {
    try {
      await deleteOrderMutation.mutateAsync(orderId)
      refetch()
    } catch (error) {
      console.error("Delete failed:", error)
    }
  }

  // Sortable column header component
  const SortableHeader = ({ column, children, className }: {
    column: string;
    children: React.ReactNode;
    className?: string
  }) => (
    <TableHead
      className={cn("cursor-pointer select-none hover:bg-muted/50", className)}
      onClick={() => handleColumnSort(column)}
    >
      <div className="flex items-center">
        {children}
        {getSortIcon(column)}
      </div>
    </TableHead>
  )

  // Calculate dynamic table height
  useEffect(() => {
    calculateTableHeight()
    window.addEventListener('resize', calculateTableHeight)

    return () => window.removeEventListener('resize', calculateTableHeight)
  }, [])

  return (
    <div ref={containerRef} className="space-y-4">
      {/* Header */}
      <div ref={headerRef}>
        <h1 className="text-2xl font-bold tracking-tight">Orders</h1>
        <p className="text-muted-foreground">Manage customer orders and fulfillment</p>
      </div>

      {/* Toolbar */}
      <div ref={toolbarRef} className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="flex flex-1 items-center space-x-2">
          <SearchInput
            value={appliedFilters.searchTerm || ""}
            onChange={handleSearchChange}
            placeholder="Search orders..."
            className="max-w-sm"
          />
          <Dialog open={filtersOpen} onOpenChange={handleFiltersDialogOpen}>
            <DialogTrigger asChild>
              <Button variant="outline" className="gap-2">
                <Filter className="h-4 w-4" />
                Filters
                {hasActiveFilters(appliedFilters) && (
                  <Badge variant="secondary" className="ml-1 h-5 px-1 text-xs">
                    {getActiveFilterLabels().length}
                  </Badge>
                )}
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Filter Orders</DialogTitle>
                <DialogDescription>
                  Use the filters below to narrow down your order search.
                </DialogDescription>
              </DialogHeader>
              <OrderFiltersComponent
                filters={pendingFilters}
                onFiltersChange={handlePendingFiltersChange}
                onApply={handleApplyFilters}
                onClearAll={handleClearAllFilters}
              />
            </DialogContent>
          </Dialog>
        </div>
        <div className="flex items-center space-x-2">
          <AddOrderDialog onSuccess={() => refetch()} />
        </div>
      </div>

      {/* Active Filters Display */}
      {(() => {
        const activeFilters = getActiveFilterLabels()
        return activeFilters.length > 0 ? (
          <div ref={activeFiltersRef} className="flex flex-wrap gap-2 p-4 bg-muted/30 rounded-lg border">
            <span className="text-sm font-medium text-muted-foreground">Active filters:</span>
            {activeFilters.map((filter) => (
              <Badge
                key={filter.key}
                variant="secondary"
                className="gap-1 pr-1"
              >
                {filter.label}
                <button
                  onClick={() => removeFilter(filter.key)}
                  className="ml-1 hover:bg-destructive/20 rounded-full p-0.5"
                  aria-label={`Remove ${filter.label} filter`}
                >
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            ))}
            <Button
              variant="ghost"
              size="sm"
              onClick={handleClearAllFilters}
              className="h-6 px-2 text-xs"
            >
              Clear all
            </Button>
          </div>
        ) : null
      })()}

      {/* Data Table */}
      <div>
        <div className="p-0">
          {error ? (
            <div className="flex h-96 items-center justify-center">
              <div className="text-center">
                <p className="text-destructive">Error loading data</p>
                <p className="text-sm text-muted-foreground">{error.message}</p>
              </div>
            </div>
          ) : isLoading ? (
            <div className="flex h-96 items-center justify-center">
              <div className="text-center">
                <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-2" />
                <p className="text-sm text-muted-foreground">Loading...</p>
              </div>
            </div>
          ) : orders.length === 0 ? (
            <div className="flex h-96 items-center justify-center">
              <div className="text-center">
                <p className="text-muted-foreground">No orders found</p>
                <p className="text-sm text-muted-foreground">Try adjusting your search or filters</p>
              </div>
            </div>
          ) : (
            <div
              className="relative overflow-auto"
              style={{ height: `${tableHeight}px` }}
            >
              <Table>
                <TableHeader className="sticky top-0 z-10 bg-background">
                  <TableRow>
                    <SortableHeader column="id">Order ID</SortableHeader>
                    <TableHead>Customer</TableHead>
                    <SortableHeader column="orderDate">Date</SortableHeader>
                    <SortableHeader column="totalAmount" className="text-right">Total</SortableHeader>
                    <SortableHeader column="orderStatus" className="text-center">Status</SortableHeader>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {orders.map((order) => (
                    <TableRow key={order.id}>
                      <TableCell className="font-medium">#{order.id}</TableCell>
                      <TableCell>
                        {order.customerFullName || `User #${order.userId}`}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {format(new Date(order.orderDate), "MMM dd, yyyy")}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        {formatCurrency(order.totalAmount)}
                      </TableCell>
                      <TableCell className="text-center">
                        <OrderStatusBadge status={order.orderStatus || "Unknown"} />
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <OrderDetailDialog order={order} />
                          <OrderStatusDialog
                            orderId={order.id}
                            currentStatus={order.orderStatus || "Unknown"}
                          />
                          <DeleteOrderButton
                            orderId={order.id}
                            onDelete={handleDelete}
                            isDeleting={deleteOrderMutation.isPending}
                          />
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </div>
      </div>

      {/* Pagination */}
      {pagination.totalCount > 0 && (
        <div ref={paginationRef}>
          <Pagination
            currentPage={pagination.currentPage}
            totalPages={pagination.totalPages}
            pageSize={pagination.pageSize}
            totalCount={pagination.totalCount}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
          />
        </div>
      )}
    </div>
  )
}
