"use client";

import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { SalesTrendItem } from "@/services/dashboard";
import {
  Area,
  AreaChart,
  CartesianGrid,
  Legend,
  Line,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { format, subDays } from "date-fns";

interface SalesChartProps {
  data: SalesTrendItem[];
  title?: string;
  className?: string;
}

export function SalesChart({ data, title = "Sales Trend", className }: SalesChartProps) {
  const [period, setPeriod] = useState<"7days" | "30days" | "90days">("30days");

  // Filter data based on selected period
  const filteredData = filterDataByPeriod(data, period);

  // Format dates for display
  const formattedData = filteredData.map(item => ({
    ...item,
    formattedDate: format(new Date(item.date), "MMM dd")
  }));

  return (
    <Card className={className}>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-base font-medium">{title}</CardTitle>
        <div className="flex items-center space-x-2">
          <Button
            variant={period === "7days" ? "default" : "outline"}
            size="sm"
            onClick={() => setPeriod("7days")}
            className="h-8 text-xs"
          >
            7 Days
          </Button>
          <Button
            variant={period === "30days" ? "default" : "outline"}
            size="sm"
            onClick={() => setPeriod("30days")}
            className="h-8 text-xs"
          >
            30 Days
          </Button>
          <Button
            variant={period === "90days" ? "default" : "outline"}
            size="sm"
            onClick={() => setPeriod("90days")}
            className="h-8 text-xs"
          >
            90 Days
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        <div className="h-[300px]">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart
              data={formattedData}
              margin={{
                top: 10,
                right: 30,
                left: 0,
                bottom: 0,
              }}
            >
              <defs>
                <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#8884d8" stopOpacity={0.8} />
                  <stop offset="95%" stopColor="#8884d8" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="colorOrders" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#82ca9d" stopOpacity={0.8} />
                  <stop offset="95%" stopColor="#82ca9d" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis
                dataKey="formattedDate"
                tick={{ fontSize: 12 }}
                tickLine={false}
                axisLine={false}
              />
              <YAxis
                yAxisId="left"
                tick={{ fontSize: 12 }}
                tickLine={false}
                axisLine={false}
                tickFormatter={(value) => `$${value}`}
              />
              <YAxis
                yAxisId="right"
                orientation="right"
                tick={{ fontSize: 12 }}
                tickLine={false}
                axisLine={false}
              />
              <Tooltip
                formatter={(value, name) => {
                  if (name === "revenue") return [`$${value}`, "Revenue"];
                  return [value, "Orders"];
                }}
              />
              <Legend />
              <Area
                yAxisId="left"
                type="monotone"
                dataKey="revenue"
                stroke="#8884d8"
                fillOpacity={1}
                fill="url(#colorRevenue)"
              />
              <Line
                yAxisId="right"
                type="monotone"
                dataKey="orderCount"
                stroke="#82ca9d"
                name="orders"
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}

// Helper function to filter data by period
function filterDataByPeriod(data: SalesTrendItem[], period: "7days" | "30days" | "90days") {
  const today = new Date();
  let daysToSubtract = 0;

  switch (period) {
    case "7days":
      daysToSubtract = 7;
      break;
    case "30days":
      daysToSubtract = 30;
      break;
    case "90days":
      daysToSubtract = 90;
      break;
  }

  const cutoffDate = subDays(today, daysToSubtract);

  return (data || []).filter(item => {
    const itemDate = new Date(item.date);
    return itemDate >= cutoffDate;
  });
}
