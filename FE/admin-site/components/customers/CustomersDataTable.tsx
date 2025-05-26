"use client"

import * as React from "react"
import { useState, useEffect, useRef } from "react"
import { useGetCustomers, useDeleteCustomer } from "@/hooks/useCustomers"
import { CustomerFiltersComponent } from "./CustomerFilters"
import { CustomerDetailDialog } from "./CustomerDetailDialog"
import { EditCustomerDialog } from "./CustomerDialog"
import { DeleteCustomerButton } from "./CustomerDialog"
import { AddCustomerDialog } from "./CustomerDialog"
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
import { cn } from "@/lib/utils"
import { CustomerFilters } from "@/types/api"
import { createDefaultPagination, hasActiveFilters } from "@/lib/query-utils"
import { Filter, ArrowUpDown, ArrowUp, ArrowDown, X } from "lucide-react"
import { format } from "date-fns"

export function CustomersDataTable() {
  // State for applied filters (what's actually filtering the table)
  const [appliedFilters, setAppliedFilters] = useState<CustomerFilters>({
    ...createDefaultPagination(25),
    searchTerm: "",
  })

  // State for pending filters (what user is editing in dialog)
  const [pendingFilters, setPendingFilters] = useState<CustomerFilters>({
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
  const { data, isLoading, error, refetch } = useGetCustomers(appliedFilters)
  const deleteCustomerMutation = useDeleteCustomer()

  // Derived state
  const customers = data?.items || []
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
  const handlePendingFiltersChange = (newFilters: CustomerFilters) => {
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

    if (appliedFilters.email) {
      labels.push({
        key: 'email',
        label: `Email: ${appliedFilters.email}`,
        value: appliedFilters.email
      })
    }

    if (appliedFilters.city) {
      labels.push({
        key: 'city',
        label: `City: ${appliedFilters.city}`,
        value: appliedFilters.city
      })
    }

    if (appliedFilters.state) {
      labels.push({
        key: 'state',
        label: `State: ${appliedFilters.state}`,
        value: appliedFilters.state
      })
    }

    if (appliedFilters.country) {
      labels.push({
        key: 'country',
        label: `Country: ${appliedFilters.country}`,
        value: appliedFilters.country
      })
    }

    if (appliedFilters.postalCode) {
      labels.push({
        key: 'postalCode',
        label: `Postal Code: ${appliedFilters.postalCode}`,
        value: appliedFilters.postalCode
      })
    }

    if (appliedFilters.hasOrders) {
      labels.push({
        key: 'hasOrders',
        label: 'Has Orders',
        value: true
      })
    }

    if (appliedFilters.registeredAfter || appliedFilters.registeredBefore) {
      const dateRange = []
      if (appliedFilters.registeredAfter) {
        dateRange.push(new Date(appliedFilters.registeredAfter).toLocaleDateString())
      }
      if (appliedFilters.registeredBefore) {
        dateRange.push(new Date(appliedFilters.registeredBefore).toLocaleDateString())
      }
      labels.push({
        key: 'dateRange',
        label: `Registered: ${dateRange.join(' - ')}`,
        value: 'dateRange'
      })
    }

    return labels
  }

  // Remove individual filter
  const removeFilter = (filterKey: string) => {
    const newFilters = { ...appliedFilters }

    switch (filterKey) {
      case 'email':
        delete newFilters.email
        break
      case 'city':
        delete newFilters.city
        break
      case 'state':
        delete newFilters.state
        break
      case 'country':
        delete newFilters.country
        break
      case 'postalCode':
        delete newFilters.postalCode
        break
      case 'hasOrders':
        delete newFilters.hasOrders
        break
      case 'dateRange':
        delete newFilters.registeredAfter
        delete newFilters.registeredBefore
        break
    }

    setAppliedFilters(newFilters)
  }

  // Action handlers
  const handleDelete = async (customerId: number) => {
    try {
      await deleteCustomerMutation.mutateAsync(customerId)
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
        <h1 className="text-2xl font-bold tracking-tight">Customers</h1>
        <p className="text-muted-foreground">Manage your customer database</p>
      </div>

      {/* Toolbar */}
      <div ref={toolbarRef} className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="flex flex-1 items-center space-x-2">
          <SearchInput
            value={appliedFilters.searchTerm || ""}
            onChange={handleSearchChange}
            placeholder="Search customers..."
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
                <DialogTitle>Filter Customers</DialogTitle>
                <DialogDescription>
                  Use the filters below to narrow down your customer search.
                </DialogDescription>
              </DialogHeader>
              <CustomerFiltersComponent
                filters={pendingFilters}
                onFiltersChange={handlePendingFiltersChange}
                onApply={handleApplyFilters}
                onClearAll={handleClearAllFilters}
              />
            </DialogContent>
          </Dialog>
        </div>
        <div className="flex items-center space-x-2">
          <AddCustomerDialog onSuccess={() => refetch()} />
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
          ) : customers.length === 0 ? (
            <div className="flex h-96 items-center justify-center">
              <div className="text-center">
                <p className="text-muted-foreground">No customers found</p>
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
                    <SortableHeader column="id">ID</SortableHeader>
                    <SortableHeader column="firstName">Name</SortableHeader>
                    <SortableHeader column="email">Email</SortableHeader>
                    <TableHead>Location</TableHead>
                    <TableHead>Phone</TableHead>
                    <SortableHeader column="createdAt">Joined</SortableHeader>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {customers.map((customer) => (
                    <TableRow key={customer.id}>
                      <TableCell className="font-medium">#{customer.id}</TableCell>
                      <TableCell>
                        <div>
                          <div className="font-medium">
                            {`${customer.firstName || ""} ${customer.lastName || ""}`.trim() || "N/A"}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {customer.email || "N/A"}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {customer.city && customer.state ? `${customer.city}, ${customer.state}` :
                         customer.city || customer.state || "N/A"}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {customer.phone || "N/A"}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {customer.createdAt ? format(new Date(customer.createdAt), "MMM dd, yyyy") : "N/A"}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <CustomerDetailDialog customerId={customer.id} />
                          <EditCustomerDialog
                            customer={customer}
                            onSuccess={() => refetch()}
                          />
                          <DeleteCustomerButton
                            customerId={customer.id}
                            onDelete={handleDelete}
                            isDeleting={deleteCustomerMutation.isPending}
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
