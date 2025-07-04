"use client"

import * as React from "react"
import { useState, useEffect, useRef } from "react"
import { useGetCategories, useDeleteCategory } from "@/hooks/useCategories"
import { useCategories } from "@/hooks/useCategoryStore"
import { CategoryFiltersComponent } from "./CategoryFilters"
import { CategoryDialog } from "./CategoryDialog"
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
  TableSkeleton,
  tableSkeletonConfigs,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { getImageUrl, cn } from "@/lib/utils"
import { CategoryFilters } from "@/types/api"
import { createDefaultPagination, hasActiveFilters } from "@/lib/query-utils"
import { Filter, ArrowUpDown, ArrowUp, ArrowDown, X, Plus, Pencil, Trash2 } from "lucide-react"
import { format } from "date-fns"
import { Category } from "@/services/categories"
import { useConfirmationDialog } from "../ui/confirmation-dialog"
import { ImportExportToolbar } from "@/components/excel/import-export-toolbar"
import { categoryExcelService } from "@/services/excel"

export function CategoriesDataTable() {
  // State for applied filters (what's actually filtering the table)
  const [appliedFilters, setAppliedFilters] = useState<CategoryFilters>({
    ...createDefaultPagination(25),
    searchTerm: "",
  })

  // State for pending filters (what user is editing in dialog)
  const [pendingFilters, setPendingFilters] = useState<CategoryFilters>({
    ...createDefaultPagination(25),
    searchTerm: "",
  })

  // State for UI
  const [filtersOpen, setFiltersOpen] = useState(false)
  const [tableHeight, setTableHeight] = useState<number>(400)
  const [addDialogOpen, setAddDialogOpen] = useState(false)
  const [editDialogOpen, setEditDialogOpen] = useState(false)
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [editingCategory, setEditingCategory] = useState<any>(null)

  // Refs for height calculation
  const containerRef = useRef<HTMLDivElement>(null)
  const headerRef = useRef<HTMLDivElement>(null)
  const toolbarRef = useRef<HTMLDivElement>(null)
  const paginationRef = useRef<HTMLDivElement>(null)
  const activeFiltersRef = useRef<HTMLDivElement>(null)

  // API calls
  const { data, isLoading, error, refetch } = useGetCategories(appliedFilters)
  const { data: allCategories } = useCategories()
  const deleteCategoryMutation = useDeleteCategory()

  const { confirm } = useConfirmationDialog()

  // Derived state
  const categories = data?.items || []
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
  const handlePendingFiltersChange = (newFilters: CategoryFilters) => {
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

    if (appliedFilters.parentId) {
      const parentCategory = allCategories.find(c => c.id === appliedFilters.parentId)
      labels.push({
        key: 'parentId',
        label: `Parent: ${parentCategory?.name || 'Unknown'}`,
        value: appliedFilters.parentId
      })
    }

    if (appliedFilters.isRootCategory) {
      labels.push({
        key: 'isRootCategory',
        label: 'Root Categories Only',
        value: true
      })
    }

    if (appliedFilters.includeSubcategories) {
      labels.push({
        key: 'includeSubcategories',
        label: 'Include Subcategories',
        value: true
      })
    }

    if (appliedFilters.includeProductCount) {
      labels.push({
        key: 'includeProductCount',
        label: 'Include Product Count',
        value: true
      })
    }

    if (appliedFilters.createdAfter || appliedFilters.createdBefore) {
      const dateRange = []
      if (appliedFilters.createdAfter) {
        dateRange.push(new Date(appliedFilters.createdAfter).toLocaleDateString())
      }
      if (appliedFilters.createdBefore) {
        dateRange.push(new Date(appliedFilters.createdBefore).toLocaleDateString())
      }
      labels.push({
        key: 'dateRange',
        label: `Created: ${dateRange.join(' - ')}`,
        value: 'dateRange'
      })
    }

    return labels
  }

  // Remove individual filter
  const removeFilter = (filterKey: string) => {
    const newFilters = { ...appliedFilters }

    switch (filterKey) {
      case 'parentId':
        delete newFilters.parentId
        break
      case 'isRootCategory':
        delete newFilters.isRootCategory
        break
      case 'includeSubcategories':
        delete newFilters.includeSubcategories
        break
      case 'includeProductCount':
        delete newFilters.includeProductCount
        break
      case 'dateRange':
        delete newFilters.createdAfter
        delete newFilters.createdBefore
        break
    }

    setAppliedFilters(newFilters)
  }

  // Action handlers
  const handleDelete = async (categoryId: number) => {
    confirm({
      title: "Delete Product",
      message: "Are you sure you want to delete this product? This action cannot be undone.",
      confirmText: "Delete",
      cancelText: "Cancel",
      variant: "destructive",
      onConfirm: async () => {
        try {
          await deleteCategoryMutation.mutateAsync(categoryId)
          refetch()
        } catch (error) {
          console.error("Delete failed:", error)
        }
      },
    });
  }

  const handleAddCategory = () => {
    setEditingCategory(null)
    setAddDialogOpen(true)
  }

  const handleEditCategory = (category: Category) => {
    setEditingCategory(category)
    setEditDialogOpen(true)
  }

  const handleDialogSuccess = () => {
    setAddDialogOpen(false)
    setEditDialogOpen(false)
    setEditingCategory(null)
    refetch()
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
        <h1 className="text-2xl font-bold tracking-tight">Categories</h1>
        <p className="text-muted-foreground">Organize your product catalog</p>
      </div>

      {/* Toolbar */}
      <div ref={toolbarRef} className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="flex flex-1 items-center space-x-2">
          <SearchInput
            value={appliedFilters.searchTerm || ""}
            onChange={handleSearchChange}
            placeholder="Search categories..."
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
                <DialogTitle>Filter Categories</DialogTitle>
                <DialogDescription>
                  Use the filters below to narrow down your category search.
                </DialogDescription>
              </DialogHeader>
              <CategoryFiltersComponent
                filters={pendingFilters}
                onFiltersChange={handlePendingFiltersChange}
                onApply={handleApplyFilters}
                onClearAll={handleClearAllFilters}
              />
            </DialogContent>
          </Dialog>
        </div>
        <div className="flex items-center space-x-2">
          <ImportExportToolbar
            onExportData={categoryExcelService.exportData}
            onExportTemplate={categoryExcelService.exportTemplate}
            onValidateImport={categoryExcelService.validateImport}
            onImportData={categoryExcelService.importData}
            onGetImportStatistics={categoryExcelService.getImportStatistics}
            currentFilters={appliedFilters}
            hasActiveFilters={hasActiveFilters(appliedFilters)}
            exportType="categories"
            onImportSuccess={() => refetch()}
          />
          <Button onClick={handleAddCategory} className="gap-2">
            <Plus className="w-4 h-4" />
            Add Category
          </Button>
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
            <div
              className="relative overflow-auto"
              style={{ height: `${tableHeight}px` }}
            >
              <Table>
                <TableHeader className="sticky top-0 z-10 bg-background">
                  <TableRow>
                    <TableHead>Image</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead>Slug</TableHead>
                    <TableHead>Parent</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableSkeleton
                    rows={Math.min(10, Math.max(5, Math.floor(tableHeight / 60)))}
                    columns={tableSkeletonConfigs.categories}
                  />
                </TableBody>
              </Table>
            </div>
          ) : categories.length === 0 ? (
            <div className="flex h-96 items-center justify-center">
              <div className="text-center">
                <p className="text-muted-foreground">No categories found</p>
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
                    <TableHead>Image</TableHead>
                    <SortableHeader column="name">Name</SortableHeader>
                    <TableHead>Slug</TableHead>
                    <TableHead>Parent</TableHead>
                    <SortableHeader column="createdAt">Created</SortableHeader>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {categories.map((category) => (
                    <TableRow key={category.id}>
                      <TableCell>
                        <div className="h-10 w-16 rounded-md bg-muted overflow-hidden">
                          {category.imageUrl ? (
                            <img
                              src={getImageUrl(category.imageUrl)}
                              alt={category.name}
                              className="h-full w-full object-cover"
                            />
                          ) : (
                            <div className="h-full w-full bg-muted flex items-center justify-center text-xs text-muted-foreground">
                              No image
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div>
                          <div className="font-medium">{category.name}</div>
                          {category.description && (
                            <div className="text-sm text-muted-foreground line-clamp-1">
                              {category.description}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="font-mono text-sm">{category.slug}</TableCell>
                      <TableCell>
                        {category.parentId && allCategories ?
                          allCategories.find(parent => parent.id === category.parentId)?.name || "-"
                          : "-"}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {format(new Date(category.createdAt), "MMM dd, yyyy")}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            size="sm"
                            variant="outline"
                            className="gap-2"
                            onClick={() => handleEditCategory(category)}
                          >
                            <Pencil className="w-4 h-4" />
                          </Button>
                          <Button
                            size="sm"
                            variant="destructive"
                            className="gap-2"
                            onClick={() => handleDelete(category.id)}
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
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

      {/* Add Category Dialog */}
      <CategoryDialog
        open={addDialogOpen}
        onOpenChange={(open) => {
          setAddDialogOpen(open)
          if (!open) handleDialogSuccess()
        }}
        initialData={null}
      />

      {/* Edit Category Dialog */}
      <CategoryDialog
        open={editDialogOpen}
        onOpenChange={(open) => {
          setEditDialogOpen(open)
          if (!open) handleDialogSuccess()
        }}
        initialData={editingCategory}
      />
    </div>
  )
}
