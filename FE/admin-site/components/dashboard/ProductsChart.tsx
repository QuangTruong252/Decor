"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { PopularProduct } from "@/services/dashboard";
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
} from "recharts";

interface ProductsChartProps {
  data: PopularProduct[];
  title?: string;
  className?: string;
}

export function ProductsChart({
  data,
  title = "Popular Products",
  className,
}: ProductsChartProps) {
  // Colors for the bars
  const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#8884D8"];

  // Format data for the chart
  const chartData = data.map((product, index) => ({
    name: product.name.length > 20 ? product.name.substring(0, 20) + "..." : product.name,
    sales: product.totalSold,
    revenue: product.totalRevenue,
    color: COLORS[index % COLORS.length],
  }));

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="text-base font-medium">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px]">
          <ResponsiveContainer width="100%" height="100%">
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
              <XAxis type="number" />
              <YAxis
                dataKey="name"
                type="category"
                tick={{ fontSize: 12 }}
                width={150}
              />
              <Tooltip
                formatter={(value, name) => {
                  if (name === "revenue") return [`$${value}`, "Revenue"];
                  return [value, "Units Sold"];
                }}
              />
              <Legend />
              <Bar dataKey="sales" name="Units Sold" barSize={20}>
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}
