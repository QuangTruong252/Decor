"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { OrderStatusCount, OrderStatusDistribution } from "@/services/dashboard";
import {
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
} from "recharts";

interface OrderStatusChartProps {
  data: OrderStatusCount[] | OrderStatusDistribution;
  title?: string;
  className?: string;
}

export function OrderStatusChart({
  data,
  title = "Order Status Distribution",
  className,
}: OrderStatusChartProps) {
  // Colors for the pie chart segments
  const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#8884D8", "#82ca9d"];

  // Format data for the chart
  console.log(data);

  // Convert data to chart format based on its type
  let chartData: { name: string; value: number; color: string }[] = [];

  if (Array.isArray(data)) {
    // Handle OrderStatusCount[] format
    chartData = data.map((item, index) => ({
      name: item.status,
      value: item.count,
      color: COLORS[index % COLORS.length],
    }));
  } else if (data) {
    // Handle OrderStatusDistribution format
    const statusMap = {
      pending: "Pending",
      processing: "Processing",
      shipped: "Shipped",
      delivered: "Delivered",
      cancelled: "Cancelled"
    };

    chartData = Object.entries(data)
      .filter(([key]) => key !== "total")
      .map(([key, value], index) => ({
        name: statusMap[key as keyof typeof statusMap] || key,
        value: value as number,
        color: COLORS[index % COLORS.length],
      }));
  }

  // Calculate total orders
  const totalOrders = chartData.reduce((sum, item) => sum + item.value, 0);

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="text-base font-medium">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px] flex items-center justify-center">
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={chartData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                  label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                >
                  {chartData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value, name) => {
                    const percentage = ((value as number) / totalOrders * 100).toFixed(1);
                    return [`${value} (${percentage}%)`, name];
                  }}
                />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <div className="text-center text-muted-foreground">No order data available</div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
