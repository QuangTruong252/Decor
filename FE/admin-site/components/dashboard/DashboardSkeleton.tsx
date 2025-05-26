"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent, CardHeader } from "@/components/ui/card";

/**
 * Skeleton loader for metric cards
 */
export function MetricCardSkeleton() {
  return (
    <Card className="overflow-hidden">
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-8 w-16" />
            <Skeleton className="h-3 w-32" />
          </div>
          <Skeleton className="h-12 w-12 rounded-full" />
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton loader for chart components
 */
export function ChartSkeleton({ height = "h-80" }: { height?: string }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <Skeleton className="h-6 w-32" />
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className={`${height} space-y-3`}>
          <div className="flex items-end justify-between space-x-2">
            {Array.from({ length: 7 }).map((_, i) => (
              <Skeleton
                key={i}
                className={`w-8 ${
                  i % 3 === 0 ? "h-20" : i % 2 === 0 ? "h-16" : "h-12"
                }`}
              />
            ))}
          </div>
          <div className="flex justify-between">
            {Array.from({ length: 7 }).map((_, i) => (
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
export function PieChartSkeleton() {
  return (
    <Card>
      <CardHeader className="pb-2">
        <Skeleton className="h-6 w-32" />
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className="flex items-center justify-center space-x-8">
          <Skeleton className="h-40 w-40 rounded-full" />
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="flex items-center space-x-2">
                <Skeleton className="h-3 w-3 rounded-full" />
                <Skeleton className="h-3 w-20" />
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
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-32" />
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {/* Table header */}
          <div className="grid grid-cols-4 gap-4 pb-2 border-b">
            <Skeleton className="h-4 w-16" />
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-4 w-16" />
            <Skeleton className="h-4 w-12" />
          </div>

          {/* Table rows */}
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="grid grid-cols-4 gap-4 py-2">
              <Skeleton className="h-4 w-12" />
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-4 w-16" />
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
    <div className="space-y-6 p-4 md:p-6">
      {/* Header */}
      <div className="space-y-2">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-4 w-64" />
      </div>

      {/* Metric Cards - Responsive Grid */}
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <MetricCardSkeleton key={i} />
        ))}
      </div>

      {/* Charts - Responsive Grid */}
      <div className="grid gap-4 grid-cols-1 lg:grid-cols-2">
        <ChartSkeleton />
        <ChartSkeleton />
      </div>

      <div className="grid gap-4 grid-cols-1 lg:grid-cols-2">
        <PieChartSkeleton />
        <ChartSkeleton />
      </div>

      {/* Recent Orders Table */}
      <RecentOrdersTableSkeleton />
    </div>
  );
}
