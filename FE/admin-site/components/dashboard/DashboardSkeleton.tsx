"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableHead,
  TableHeader,
  TableRow,
  TableSkeleton,
} from "@/components/ui/table";

/**
 * Skeleton loader for metric cards
 */
export function MetricCardSkeleton() {
  return (
    <Card className="overflow-hidden">
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div>
            <Skeleton className="h-4 w-24" /> {/* Title */}
            <Skeleton className="h-8 w-20 mt-2" /> {/* Value */}
            <div className="mt-1 flex items-center space-x-1">
              <Skeleton className="h-3 w-8" /> {/* Trend percentage */}
              <Skeleton className="h-3 w-20" /> {/* Trend label */}
            </div>
          </div>
          <div className="rounded-full bg-muted p-3">
            <Skeleton className="h-5 w-5" /> {/* Icon */}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton loader for bar/line chart components
 */
export function ChartSkeleton({
  height = "h-80",
  bars = 7
}: {
  height?: string;
  title?: string;
  bars?: number;
}) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base font-medium">
          <Skeleton className="h-5 w-32" />
        </CardTitle>
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className={`${height} space-y-3`}>
          <div className="flex items-end justify-between space-x-2">
            {Array.from({ length: bars }).map((_, i) => (
              <Skeleton
                key={i}
                className={`w-8 ${
                  i % 3 === 0 ? "h-20" : i % 2 === 0 ? "h-16" : "h-12"
                }`}
              />
            ))}
          </div>
          <div className="flex justify-between">
            {Array.from({ length: bars }).map((_, i) => (
              <Skeleton key={i} className="h-3 w-8" />
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton loader for pie chart components
 */
export function PieChartSkeleton({ legendItems = 4 }: { legendItems?: number }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base font-medium">
          <Skeleton className="h-5 w-32" />
        </CardTitle>
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className="flex items-center justify-center space-x-8">
          <Skeleton className="h-40 w-40 rounded-full" />
          <div className="space-y-3">
            {Array.from({ length: legendItems }).map((_, i) => (
              <div key={i} className="flex items-center space-x-2">
                <Skeleton className="h-3 w-3 rounded-full" />
                <Skeleton className="h-3 w-20" />
                <Skeleton className="h-3 w-12" />
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton loader for recent orders table
 */
export function RecentOrdersTableSkeleton() {
  // Define columns for recent orders table
  const recentOrdersColumns = [
    { type: 'text' as const, width: 'w-16' }, // Order ID
    { type: 'text' as const, width: 'w-32' }, // Customer
    { type: 'text' as const, width: 'w-24' }, // Date
    { type: 'badge' as const }, // Status
    { type: 'currency' as const }, // Amount
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base font-medium">
          <Skeleton className="h-5 w-32" />
        </CardTitle>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Order ID</TableHead>
              <TableHead>Customer</TableHead>
              <TableHead>Date</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Amount</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableSkeleton rows={5} columns={recentOrdersColumns} />
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton for Sales Chart (area chart with period buttons)
 */
export function SalesChartSkeleton() {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-base font-medium">
          <Skeleton className="h-5 w-32" />
        </CardTitle>
        <div className="flex items-center space-x-2">
          <Skeleton className="h-8 w-16" />
          <Skeleton className="h-8 w-20" />
          <Skeleton className="h-8 w-20" />
        </div>
      </CardHeader>
      <CardContent>
        <div className="h-[300px] space-y-3">
          {/* Area chart simulation */}
          <div className="relative h-64">
            <div className="absolute inset-0 flex items-end justify-between space-x-1">
              {Array.from({ length: 12 }).map((_, i) => (
                <div key={i} className="flex flex-col items-center space-y-1">
                  <Skeleton
                    className={`w-6 ${
                      i % 4 === 0 ? "h-32" : i % 3 === 0 ? "h-24" : i % 2 === 0 ? "h-20" : "h-16"
                    }`}
                  />
                </div>
              ))}
            </div>
          </div>
          {/* X-axis labels */}
          <div className="flex justify-between px-2">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-3 w-12" />
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton for Products Chart (bar chart)
 */
export function ProductsChartSkeleton() {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base font-medium">
          <Skeleton className="h-5 w-32" />
        </CardTitle>
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className="h-80 space-y-3">
          <div className="flex items-end justify-between space-x-2">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex flex-col items-center space-y-2">
                <Skeleton
                  className={`w-12 ${
                    i === 0 ? "h-32" : i === 1 ? "h-24" : i === 2 ? "h-20" : i === 3 ? "h-16" : "h-12"
                  }`}
                />
                <Skeleton className="h-3 w-16" />
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton for Order Status Chart (pie chart)
 */
export function OrderStatusChartSkeleton() {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base font-medium">
          <Skeleton className="h-5 w-32" />
        </CardTitle>
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className="flex items-center justify-center space-x-8">
          <Skeleton className="h-40 w-40 rounded-full" />
          <div className="space-y-3">
            {['Pending', 'Processing', 'Shipped', 'Delivered'].map((_, i) => (
              <div key={i} className="flex items-center space-x-2">
                <Skeleton className="h-3 w-3 rounded-full" />
                <Skeleton className="h-3 w-20" />
                <Skeleton className="h-3 w-8" />
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton for Category Sales Chart (horizontal bar chart)
 */
export function CategorySalesChartSkeleton() {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base font-medium">
          <Skeleton className="h-5 w-32" />
        </CardTitle>
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className="h-80 space-y-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="flex items-center space-x-3">
              <Skeleton className="h-4 w-16" />
              <div className="flex-1">
                <Skeleton
                  className={`h-6 ${
                    i === 0 ? "w-full" : i === 1 ? "w-4/5" : i === 2 ? "w-3/5" : i === 3 ? "w-2/5" : "w-1/5"
                  }`}
                />
              </div>
              <Skeleton className="h-4 w-12" />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Complete dashboard skeleton loader with responsive design
 */
export function DashboardSkeleton() {
  return (
    <div className="space-y-6 p-2">
      {/* Header */}
      <Skeleton className="h-9 w-48" /> {/* Dashboard title */}

      {/* Metric Cards - Responsive Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <MetricCardSkeleton key={i} />
        ))}
      </div>

      {/* Charts Row 1 - Sales Chart & Products Chart */}
      <div className="grid gap-4 md:grid-cols-2">
        <SalesChartSkeleton />
        <ProductsChartSkeleton />
      </div>

      {/* Charts Row 2 - Order Status Chart & Category Sales Chart */}
      <div className="grid gap-4 md:grid-cols-2">
        <OrderStatusChartSkeleton />
        <CategorySalesChartSkeleton />
      </div>

      {/* Recent Orders Table */}
      <RecentOrdersTableSkeleton />
    </div>
  );
}
