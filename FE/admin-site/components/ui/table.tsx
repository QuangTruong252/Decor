"use client"

import * as React from "react"

import { cn } from "@/lib/utils"
import { Skeleton } from "./skeleton"

function Table({ className, ...props }: React.ComponentProps<"table">) {
  return (
    <div
      data-slot="table-container"
      className="relative w-full overflow-x-auto rounded-md border"
    >
      <table
        data-slot="table"
        className={cn("w-full caption-bottom text-sm overflow-x-auto", className)}
        {...props}
      />
    </div>
  )
}

function TableHeader({ className, ...props }: React.ComponentProps<"thead">) {
  return (
    <thead
      data-slot="table-header"
      className={cn("[&_tr]:border-b bg-muted/50", className)}
      {...props}
    />
  )
}

function TableBody({ className, ...props }: React.ComponentProps<"tbody">) {
  return (
    <tbody
      data-slot="table-body"
      className={cn("[&_tr:last-child]:border-0", className)}
      {...props}
    />
  )
}

function TableFooter({ className, ...props }: React.ComponentProps<"tfoot">) {
  return (
    <tfoot
      data-slot="table-footer"
      className={cn(
        "bg-muted/50 border-t font-medium [&>tr]:last:border-b-0",
        className
      )}
      {...props}
    />
  )
}

function TableRow({ className, ...props }: React.ComponentProps<"tr">) {
  return (
    <tr
      data-slot="table-row"
      className={cn(
        "hover:bg-muted/50 data-[state=selected]:bg-muted border-b transition-colors",
        className
      )}
      {...props}
    />
  )
}

function TableHead({ className, ...props }: React.ComponentProps<"th">) {
  return (
    <th
      data-slot="table-head"
      className={cn(
        "text-foreground h-12 px-4 py-3 text-left align-middle font-medium whitespace-nowrap [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px]",
        className
      )}
      {...props}
    />
  )
}

function TableCell({ className, ...props }: React.ComponentProps<"td">) {
  return (
    <td
      data-slot="table-cell"
      className={cn(
        "px-4 py-3 align-middle [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px]",
        className
      )}
      {...props}
    />
  )
}

function TableCaption({
  className,
  ...props
}: React.ComponentProps<"caption">) {
  return (
    <caption
      data-slot="table-caption"
      className={cn("text-muted-foreground mt-4 text-sm", className)}
      {...props}
    />
  )
}

// Types for TableSkeleton configuration
export interface TableSkeletonColumn {
  type: 'text' | 'image' | 'badge' | 'actions' | 'checkbox' | 'currency'
  width?: string
  className?: string
}

export interface TableSkeletonProps {
  rows?: number
  columns: readonly TableSkeletonColumn[]
}

// Predefined column configurations for common table types
const tableSkeletonConfigs = {
  products: [
    { type: 'checkbox' as const },
    { type: 'image' as const },
    { type: 'text' as const, width: 'w-32' }, // Name
    { type: 'text' as const, width: 'w-20' }, // SKU
    { type: 'text' as const, width: 'w-24' }, // Category
    { type: 'currency' as const }, // Price
    { type: 'badge' as const }, // Stock
    { type: 'badge' as const }, // Status
    { type: 'badge' as const }, // Featured
    { type: 'actions' as const },
  ],
  categories: [
    { type: 'image' as const },
    { type: 'text' as const, width: 'w-32' }, // Name
    { type: 'text' as const, width: 'w-24' }, // Slug
    { type: 'text' as const, width: 'w-24' }, // Parent
    { type: 'text' as const, width: 'w-20' }, // Created
    { type: 'actions' as const },
  ],
  customers: [
    { type: 'text' as const, width: 'w-16' }, // ID
    { type: 'text' as const, width: 'w-32' }, // Name
    { type: 'text' as const, width: 'w-40' }, // Email
    { type: 'text' as const, width: 'w-24' }, // Location
    { type: 'text' as const, width: 'w-24' }, // Phone
    { type: 'text' as const, width: 'w-20' }, // Joined
    { type: 'actions' as const },
  ],
  orders: [
    { type: 'text' as const, width: 'w-20' }, // Order ID
    { type: 'text' as const, width: 'w-32' }, // Customer
    { type: 'text' as const, width: 'w-24' }, // Date
    { type: 'currency' as const }, // Total
    { type: 'badge' as const }, // Status
    { type: 'actions' as const },
  ],
} as const

// TableSkeleton component for loading states
function TableSkeleton({ rows = 5, columns }: TableSkeletonProps) {
  const renderSkeletonCell = (column: TableSkeletonColumn, index: number) => {
    const baseClassName = cn("px-4 py-3 align-middle", column.className)

    switch (column.type) {
      case 'checkbox':
        return (
          <TableCell key={index} className={baseClassName}>
            <Skeleton className="h-4 w-4 rounded" />
          </TableCell>
        )
      case 'image':
        return (
          <TableCell key={index} className={baseClassName}>
            <Skeleton className="h-10 w-10 rounded-md" />
          </TableCell>
        )
      case 'badge':
        return (
          <TableCell key={index} className={baseClassName}>
            <Skeleton className="h-6 w-16 rounded-full" />
          </TableCell>
        )
      case 'actions':
        return (
          <TableCell key={index} className={cn(baseClassName, "text-right")}>
            <div className="flex justify-end gap-2">
              <Skeleton className="h-8 w-8 rounded-md" />
              <Skeleton className="h-8 w-8 rounded-md" />
            </div>
          </TableCell>
        )
      case 'currency':
        return (
          <TableCell key={index} className={cn(baseClassName, "text-right")}>
            <Skeleton className="h-4 w-20 ml-auto" />
          </TableCell>
        )
      case 'text':
      default:
        const widthClass = column.width || 'w-24'
        return (
          <TableCell key={index} className={baseClassName}>
            <Skeleton className={cn("h-4", widthClass)} />
          </TableCell>
        )
    }
  }

  return (
    <>
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <TableRow key={rowIndex}>
          {columns.map((column, colIndex) => renderSkeletonCell(column, colIndex))}
        </TableRow>
      ))}
    </>
  )
}

export {
  Table,
  TableHeader,
  TableBody,
  TableFooter,
  TableHead,
  TableRow,
  TableCell,
  TableCaption,
  TableSkeleton,
  tableSkeletonConfigs,
}
