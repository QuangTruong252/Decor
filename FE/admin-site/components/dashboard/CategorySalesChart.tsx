"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CategorySales } from "@/services/dashboard";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
  PieChart,
  Pie,
} from "recharts";

interface CategorySalesChartProps {
  data: CategorySales[];
  title?: string;
  className?: string;
  type?: "pie" | "bar";
}

export function CategorySalesChart({
  data,
  title = "Sales by Category",
  className,
  type = "pie",
}: CategorySalesChartProps) {
  // Colors for the chart
  const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#8884D8", "#82ca9d", "#ffc658"];

  // Format data for the chart
  const chartData = data.map((category, index) => ({
    name: category.categoryName,
    value: category.totalRevenue,
    sales: category.totalSales,
    percentage: category.percentage,
    color: COLORS[index % COLORS.length],
  }));

  // Calculate total revenue
  const totalRevenue = chartData.reduce((sum, item) => sum + item.value, 0);

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="text-base font-medium">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px]">
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              {type === "pie" ? (
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
                      const percentage = ((value as number) / totalRevenue * 100).toFixed(1);
                      return [`$${value} (${percentage}%)`, name];
                    }}
                  />
                  <Legend />
                </PieChart>
              ) : (
                <BarChart
                  data={chartData}
                  layout="vertical"
                  margin={{
                    top: 5,
                    right: 30,
                    left: 20,
                    bottom: 5,
                  }}
                >
                  <CartesianGrid strokeDasharray="3 3" horizontal={true} vertical={false} />
                  <XAxis type="number" tickFormatter={(value) => `$${value}`} />
                  <YAxis
                    dataKey="name"
                    type="category"
                    tick={{ fontSize: 12 }}
                    width={120}
                  />
                  <Tooltip
                    formatter={(value, name) => {
                      if (name === "value") return [`$${value}`, "Revenue"];
                      return [value, "Units Sold"];
                    }}
                  />
                  <Legend />
                  <Bar dataKey="value" name="Revenue" fill="#8884d8" barSize={20}>
                    {chartData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Bar>
                </BarChart>
              )}
            </ResponsiveContainer>
          ) : (
            <div className="flex h-full items-center justify-center text-muted-foreground">
              No category sales data available
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
