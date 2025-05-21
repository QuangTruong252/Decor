"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { OrderDTO } from "@/services/orders";
import { formatCurrency, getImageUrl } from "@/lib/utils";
import { format } from "date-fns";
import { Eye } from "lucide-react";
import { OrderStatusBadge } from "./OrderStatusBadge";

interface OrderDetailDialogProps {
  order: OrderDTO;
}

export function OrderDetailDialog({ order }: OrderDetailDialogProps) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="icon" className="h-8 w-8">
          <Eye className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Order #{order.id}</DialogTitle>
          <DialogDescription>
            Placed on {format(new Date(order.orderDate), "MMMM dd, yyyy")}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Order Status */}
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium">Status:</span>
            <OrderStatusBadge status={order.orderStatus || "Unknown"} />
          </div>

          {/* Customer Info */}
          <div className="rounded-md border p-4">
            <h3 className="mb-2 font-medium">Customer Information</h3>
            <div className="grid gap-1 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Name:</span>
                <span>{order.userFullName || `User #${order.userId}`}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Shipping Address:</span>
                <span>{order.shippingAddress || "Not provided"}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Payment Method:</span>
                <span>{order.paymentMethod || "Not specified"}</span>
              </div>
            </div>
          </div>

          {/* Order Items */}
          <div>
            <h3 className="mb-2 font-medium">Order Items</h3>
            <div className="rounded-md border">
              <div className="divide-y">
                {order.orderItems && order.orderItems.length > 0 ? (
                  order.orderItems.map((item) => (
                    <div key={item.id} className="flex items-center p-3">
                      <div className="h-12 w-12 overflow-hidden rounded-md bg-muted">
                        {item.productImageUrl ? (
                          <img
                            src={getImageUrl(item.productImageUrl)}
                            alt={item.productName || "Product"}
                            className="h-full w-full object-cover"
                          />
                        ) : (
                          <div className="flex h-full w-full items-center justify-center bg-muted text-muted-foreground text-xs">
                            No img
                          </div>
                        )}
                      </div>
                      <div className="ml-4 flex-1">
                        <p className="font-medium">{item.productName || `Product #${item.productId}`}</p>
                        <p className="text-xs text-muted-foreground">
                          {item.quantity} x {formatCurrency(item.unitPrice)}
                        </p>
                      </div>
                      <div className="text-right font-medium">
                        {formatCurrency(item.subtotal)}
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="p-4 text-center text-sm text-muted-foreground">
                    No items in this order.
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Order Summary */}
          <div className="rounded-md border p-4">
            <h3 className="mb-2 font-medium">Order Summary</h3>
            <div className="space-y-1 text-sm">
              <div className="flex justify-between font-medium">
                <span>Total:</span>
                <span>{formatCurrency(order.totalAmount)}</span>
              </div>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
