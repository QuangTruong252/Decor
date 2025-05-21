"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { OrderDTO } from "@/services/orders";
import { RecentOrder } from "@/services/dashboard";
import { format } from "date-fns";
import { Badge } from "@/components/ui/badge";
import Link from "next/link";

interface RecentOrdersTableProps {
  orders: OrderDTO[] | RecentOrder[];
  title?: string;
  className?: string;
}

export function RecentOrdersTable({
  orders,
  title = "Recent Orders",
  className,
}: RecentOrdersTableProps) {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="text-base font-medium">{title}</CardTitle>
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
            {orders.length > 0 ? (
              orders.map((order) => {
                // Determine if it's a RecentOrder or OrderDTO
                const isRecentOrder = 'orderId' in order;
                const orderId = isRecentOrder ? order.orderId : order.id;
                const customerName = isRecentOrder ? order.customerName : (order.userFullName || "Guest");
                const orderDate = order.orderDate;
                const orderStatus = isRecentOrder ? order.orderStatus : (order.orderStatus || "Unknown");
                const amount = order.totalAmount;

                return (
                  <TableRow key={orderId}>
                    <TableCell className="font-medium">
                      <Link href={`/orders/${orderId}`} className="text-primary hover:underline">
                        #{orderId}
                      </Link>
                    </TableCell>
                    <TableCell>{customerName}</TableCell>
                    <TableCell>{format(new Date(orderDate), "MMM dd, yyyy")}</TableCell>
                    <TableCell>
                      <OrderStatusBadge status={orderStatus} />
                    </TableCell>
                    <TableCell className="text-right">${amount.toFixed(2)}</TableCell>
                  </TableRow>
                );
              })
            ) : (
              <TableRow>
                <TableCell colSpan={5} className="h-24 text-center">
                  No recent orders
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

function OrderStatusBadge({ status }: { status: string }) {
  let variant:
    | "default"
    | "secondary"
    | "destructive"
    | "outline"
    | null
    | undefined = "default";

  switch (status.toLowerCase()) {
    case "pending":
      variant = "outline";
      break;
    case "processing":
      variant = "secondary";
      break;
    case "shipped":
    case "delivered":
      variant = "default";
      break;
    case "cancelled":
    case "refunded":
      variant = "destructive";
      break;
  }

  return <Badge variant={variant}>{status}</Badge>;
}
