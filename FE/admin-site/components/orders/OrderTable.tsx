"use client";

import { OrderDTO } from "@/services/orders";
import { formatCurrency } from "@/lib/utils";
import { format } from "date-fns";
import { OrderStatusBadge } from "./OrderStatusBadge";
import { OrderDetailDialog } from "./OrderDetailDialog";
import { OrderStatusDialog } from "./OrderStatusDialog";
import { DeleteOrderButton } from "./DeleteOrderButton";

interface OrderTableProps {
  orders: OrderDTO[];
  onDelete: (id: number) => Promise<void>;
  isDeleting?: number | null;
}

export function OrderTable({ orders, onDelete, isDeleting }: OrderTableProps) {
  return (
    <div className="rounded-lg border bg-card">
      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="px-4 py-3 text-left font-medium">Order ID</th>
              <th className="px-4 py-3 text-left font-medium">Customer</th>
              <th className="px-4 py-3 text-left font-medium">Date</th>
              <th className="px-4 py-3 text-right font-medium">Total</th>
              <th className="px-4 py-3 text-center font-medium">Status</th>
              <th className="px-4 py-3 text-right font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id} className="border-b transition-colors hover:bg-muted/50">
                <td className="px-4 py-3 font-medium">#{order.id}</td>
                <td className="px-4 py-3">
                  {order.userFullName || `User #${order.userId}`}
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {format(new Date(order.orderDate), "MMM dd, yyyy")}
                </td>
                <td className="px-4 py-3 text-right font-medium">
                  {formatCurrency(order.totalAmount)}
                </td>
                <td className="px-4 py-3 text-center">
                  <OrderStatusBadge status={order.orderStatus || "Unknown"} />
                </td>
                <td className="px-4 py-3 text-right">
                  <div className="flex justify-end gap-2">
                    <OrderDetailDialog order={order} />
                    <OrderStatusDialog 
                      orderId={order.id} 
                      currentStatus={order.orderStatus || "Unknown"} 
                    />
                    <DeleteOrderButton
                      orderId={order.id}
                      onDelete={onDelete}
                      isDeleting={isDeleting === order.id}
                    />
                  </div>
                </td>
              </tr>
            ))}

            {orders.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-muted-foreground">
                  No orders found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
