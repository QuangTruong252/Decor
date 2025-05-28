"use client"

import * as React from "react"
import { useState, useEffect, useRef } from "react"
import { useGetProducts, useDeleteProduct } from "@/hooks/useProducts"
import { useCategories } from "@/hooks/useCategoryStore"
import { ProductFiltersComponent } from "./ProductFilters"
import { AddProductDialog, EditProductDialog, DeleteProductButton } from "./ProductDialog"
import { BulkDeleteButton } from "./BulkDeleteButton"
import { SearchInput } from "@/components/shared/SearchInput"
import { Pagination } from "@/components/ui/pagination"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
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
import { formatCurrency, getImageUrl, cn } from "@/lib/utils"
import { ProductFilters } from "@/types/api"
import { createDefaultPagination, hasActiveFilters } from "@/lib/query-utils"
import { Check, X, Filter, ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react"
import { Category } from "@/services/categories"
import { ImportExportToolbar } from "@/components/excel/import-export-toolbar"
import { productExcelService } from "@/services/excel"

export function ProductsDataTable() {
  // State for applied filters (what's actually filtering the table)
  const [appliedFilters, setAppliedFilters] = useState<ProductFilters>({
    ...createDefaultPagination(25),
    searchTerm: "",
  })

  // State for pending filters (what user is editing in dialog)
  const [pendingFilters, setPendingFilters] = useState<ProductFilters>({
    ...createDefaultPagination(25),
    searchTerm: "",
  })

  // State for UI
  const [selectedProducts, setSelectedProducts] = useState<number[]>([])
  const [filtersOpen, setFiltersOpen] = useState(false)
  const [tableHeight, setTableHeight] = useState<number>(400)

  // Refs for height calculation
  const containerRef = useRef<HTMLDivElement>(null)
  const headerRef = useRef<HTMLDivElement>(null)
  const toolbarRef = useRef<HTMLDivElement>(null)
  const paginationRef = useRef<HTMLDivElement>(null)
  const activeFiltersRef = useRef<HTMLDivElement>(null)

  // API calls
  const { data, isLoading, error, refetch } = useGetProducts(appliedFilters)
  const { data: categoriesData } = useCategories()
  const deleteProductMutation = useDeleteProduct()

  // Derived state
  const products = data?.items || []
  const pagination = data?.pagination || {
    currentPage: 1,
    pageSize: 25,
    totalCount: 0,
    totalPages: 1,
    hasNext: false,
    hasPrevious: false,
  }

  // Selection handlers
  const isAllSelected = selectedProducts.length === products.length && products.length > 0
  const isIndeterminate = selectedProducts.length > 0 && selectedProducts.length < products.length
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
  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedProducts(products.map(p => p.id))
    } else {
      setSelectedProducts([])
    }
  }

  const handleSelectProduct = (productId: number, checked: boolean) => {
    if (checked) {
      setSelectedProducts(prev => [...prev, productId])
    } else {
      setSelectedProducts(prev => prev.filter(id => id !== productId))
    }
  }

  // Filter handlers for pending filters (dialog editing)
  const handlePendingFiltersChange = (newFilters: ProductFilters) => {
    setPendingFilters(newFilters)
  }

  // Apply filters from dialog to actual table
  const handleApplyFilters = () => {
    setAppliedFilters(pendingFilters)
    setSelectedProducts([]) // Clear selection when filters change
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
    setSelectedProducts([])
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

    if (appliedFilters.categoryId) {
      const categories = categoriesData || []
      const category = categories.find((c: Category) => c.id === appliedFilters.categoryId)
      labels.push({
        key: 'categoryId',
        label: `Category: ${category?.name || 'Unknown'}`,
        value: appliedFilters.categoryId
      })
    }

    if (appliedFilters.minPrice || appliedFilters.maxPrice) {
      const priceRange = []
      if (appliedFilters.minPrice) priceRange.push(`$${appliedFilters.minPrice}`)
      if (appliedFilters.maxPrice) priceRange.push(`$${appliedFilters.maxPrice}`)
      labels.push({
        key: 'price',
        label: `Price: ${priceRange.join(' - ')}`,
        value: 'price'
      })
    }

    if (appliedFilters.stockQuantityMin || appliedFilters.stockQuantityMax) {
      const stockRange = []
      if (appliedFilters.stockQuantityMin) stockRange.push(appliedFilters.stockQuantityMin.toString())
      if (appliedFilters.stockQuantityMax) stockRange.push(appliedFilters.stockQuantityMax.toString())
      labels.push({
        key: 'stock',
        label: `Stock: ${stockRange.join(' - ')}`,
        value: 'stock'
      })
    }

    if (appliedFilters.sku) {
      labels.push({
        key: 'sku',
        label: `SKU: ${appliedFilters.sku}`,
        value: appliedFilters.sku
      })
    }

    if (appliedFilters.minRating) {
      labels.push({
        key: 'minRating',
        label: `Rating: ${appliedFilters.minRating}+ stars`,
        value: appliedFilters.minRating
      })
    }

    if (appliedFilters.isFeatured) {
      labels.push({
        key: 'isFeatured',
        label: 'Featured only',
        value: true
      })
    }

    if (appliedFilters.isActive) {
      labels.push({
        key: 'isActive',
        label: 'Active only',
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
      case 'categoryId':
        delete newFilters.categoryId
        break
      case 'price':
        delete newFilters.minPrice
        delete newFilters.maxPrice
        break
      case 'stock':
        delete newFilters.stockQuantityMin
        delete newFilters.stockQuantityMax
        break
      case 'sku':
        delete newFilters.sku
        break
      case 'minRating':
        delete newFilters.minRating
        break
      case 'isFeatured':
        delete newFilters.isFeatured
        break
      case 'isActive':
        delete newFilters.isActive
        break
      case 'dateRange':
        delete newFilters.createdAfter
        delete newFilters.createdBefore
        break
    }

    setAppliedFilters(newFilters)
    setSelectedProducts([])
  }

  // Action handlers
  const handleDelete = async (productId: number) => {
    try {
      await deleteProductMutation.mutateAsync(productId)
      setSelectedProducts(prev => prev.filter(id => id !== productId))
      refetch()
    } catch (error) {
      console.error("Delete failed:", error)
    }
  }

  const handleBulkDelete = () => {
    setSelectedProducts([])
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
        <h1 className="text-2xl font-bold tracking-tight">Products</h1>
        <p className="text-muted-foreground">Manage your product inventory</p>
      </div>

      {/* Toolbar */}
      <div ref={toolbarRef} className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="flex flex-1 items-center space-x-2">
          <SearchInput
            value={appliedFilters.searchTerm || ""}
            onChange={handleSearchChange}
            placeholder="Search products..."
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
                <DialogTitle>Filter Products</DialogTitle>
                <DialogDescription>
                  Use the filters below to narrow down your product search.
                </DialogDescription>
              </DialogHeader>
              <ProductFiltersComponent
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
            onExportData={productExcelService.exportData}
            onExportTemplate={productExcelService.exportTemplate}
            onValidateImport={productExcelService.validateImport}
            onImportData={productExcelService.importData}
            onGetImportStatistics={productExcelService.getImportStatistics}
            currentFilters={appliedFilters}
            hasActiveFilters={hasActiveFilters(appliedFilters)}
            exportType="products"
            onImportSuccess={() => refetch()}
          />
          {selectedProducts.length > 0 && (
            <BulkDeleteButton
              selectedIds={selectedProducts}
              onSuccess={handleBulkDelete}
            />
          )}
          <AddProductDialog onSuccess={() => refetch()} />
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
                    <TableHead className="w-12">
                      <div className="h-4 w-4" />
                    </TableHead>
                    <TableHead>Image</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead>SKU</TableHead>
                    <TableHead>Category</TableHead>
                    <TableHead className="text-right">Price</TableHead>
                    <TableHead className="text-right">Stock</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="text-center">Featured</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableSkeleton
                    rows={Math.min(10, Math.max(5, Math.floor(tableHeight / 60)))}
                    columns={tableSkeletonConfigs.products}
                  />
                </TableBody>
              </Table>
            </div>
          ) : products.length === 0 ? (
            <div className="flex h-96 items-center justify-center">
              <div className="text-center">
                <p className="text-muted-foreground">No products found</p>
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
                    <TableHead className="w-12">
                      <Checkbox
                        checked={isAllSelected}
                        ref={(el) => {
                          if (el) (el as HTMLInputElement).indeterminate = isIndeterminate
                        }}
                        onCheckedChange={handleSelectAll}
                        aria-label="Select all products"
                      />
                    </TableHead>
                    <TableHead>Image</TableHead>
                    <SortableHeader column="name">Name</SortableHeader>
                    <SortableHeader column="sku">SKU</SortableHeader>
                    <TableHead>Category</TableHead>
                    <SortableHeader column="price" className="text-right">Price</SortableHeader>
                    <SortableHeader column="stockQuantity" className="text-right">Stock</SortableHeader>
                    <SortableHeader column="isActive" className="text-center">Status</SortableHeader>
                    <SortableHeader column="isFeatured" className="text-center">Featured</SortableHeader>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {products.map((product) => (
                    <TableRow key={product.id}>
                      <TableCell>
                        <Checkbox
                          checked={selectedProducts.includes(product.id)}
                          onCheckedChange={(checked) =>
                            handleSelectProduct(product.id, checked as boolean)
                          }
                          aria-label={`Select product ${product.name}`}
                        />
                      </TableCell>
                      <TableCell>
                        <div className="h-10 w-10 rounded-md bg-muted overflow-hidden">
                          {product.images && product.images.length > 0 ? (
                            <img
                              src={getImageUrl(product.images[0])}
                              alt={product.name}
                              className="h-full w-full object-cover"
                            />
                          ) : (
                            <div className="h-full w-full bg-muted" />
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div>
                          <div className="font-medium">{product.name}</div>
                          {product.description && (
                            <div className="text-sm text-muted-foreground line-clamp-1">
                              {product.description}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="font-mono text-sm">{product.sku}</TableCell>
                      <TableCell>{product.categoryName}</TableCell>
                      <TableCell className="text-right">
                        <div>
                          <div className="font-medium">{formatCurrency(product.price)}</div>
                          {product.originalPrice && product.originalPrice > product.price && (
                            <div className="text-sm text-muted-foreground line-through">
                              {formatCurrency(product.originalPrice)}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        <Badge variant={product.stockQuantity <= 10 ? "destructive" : "secondary"}>
                          {product.stockQuantity}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-center">
                        {product.isActive ? (
                          <Check className="mx-auto h-4 w-4 text-green-500" />
                        ) : (
                          <X className="mx-auto h-4 w-4 text-destructive" />
                        )}
                      </TableCell>
                      <TableCell className="text-center">
                        {product.isFeatured ? (
                          <Badge variant="default">Featured</Badge>
                        ) : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <EditProductDialog
                            product={product}
                            onSuccess={() => refetch()}
                          />
                          <DeleteProductButton
                            productId={product.id}
                            onDelete={handleDelete}
                            isDeleting={deleteProductMutation.isPending}
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
