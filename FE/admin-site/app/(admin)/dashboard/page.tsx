"use client";

import { useGetBasicDashboardData } from "@/hooks/useDashboard";
import { MetricCard } from "@/components/dashboard/MetricCard";
import { SalesChart } from "@/components/dashboard/SalesChart";
import { ProductsChart } from "@/components/dashboard/ProductsChart";
import { OrderStatusChart } from "@/components/dashboard/OrderStatusChart";
import { CategorySalesChart } from "@/components/dashboard/CategorySalesChart";
import { RecentOrdersTable } from "@/components/dashboard/RecentOrdersTable";
import { DashboardSkeleton } from "@/components/dashboard/DashboardSkeleton";
import { Package, ShoppingCart, Users, DollarSign } from "lucide-react";

export default function DashboardPage() {
  const { data, isLoading, error } = useGetBasicDashboardData();

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  if (error) {
    return (
      <div className="space-y-6 p-2">
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <div className="rounded-lg border bg-destructive/10 p-6 text-destructive">
          <p>Error loading dashboard data. Please try again later.</p>
        </div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="space-y-6 p-2">
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <div className="rounded-lg border bg-muted/50 p-6 text-muted-foreground">
          <p>No dashboard data available.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-2">
      <h1 className="text-3xl font-bold">Dashboard</h1>

      {/* Metric Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <MetricCard
          title="Total Products"
          value={data.totalProducts}
          icon={<Package className="h-5 w-5" />}
        />
        <MetricCard
          title="Total Orders"
          value={data.totalOrders}
          icon={<ShoppingCart className="h-5 w-5" />}
          trend={{
            value: 12.5,
            label: "from last month",
            isPositive: true,
          }}
        />
        <MetricCard
          title="Total Customers"
          value={data.totalCustomers}
          icon={<Users className="h-5 w-5" />}
          trend={{
            value: 8.2,
            label: "from last month",
            isPositive: true,
          }}
        />
        <MetricCard
          title="Total Revenue"
          value={`$${data.totalRevenue.toFixed(2)}`}
          icon={<DollarSign className="h-5 w-5" />}
          trend={{
            value: 15.3,
            label: "from last month",
            isPositive: true,
          }}
        />
      </div>

      {/* Charts */}
      <div className="grid gap-4 md:grid-cols-2">
        <SalesChart data={data.recentSalesTrend || []} />
        <ProductsChart data={data.popularProducts} />
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <OrderStatusChart data={data.orderStatusDistribution} />
        <CategorySalesChart
          data={data.salesByCategory || [
            { categoryId: 1, categoryName: "Furniture", totalSales: 120, revenue: 24000 },
            { categoryId: 2, categoryName: "Decor", totalSales: 85, revenue: 12750 },
            { categoryId: 3, categoryName: "Lighting", totalSales: 65, revenue: 9750 },
            { categoryId: 4, categoryName: "Textiles", totalSales: 45, revenue: 6750 },
            { categoryId: 5, categoryName: "Kitchen", totalSales: 30, revenue: 4500 },
          ]}
        />
      </div>

      {/* Recent Orders */}
      <RecentOrdersTable orders={data.recentOrders || []} />
    </div>
  );
}