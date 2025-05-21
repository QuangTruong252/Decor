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
import { useGetCustomerById } from "@/hooks/useCustomers";
import { formatCurrency } from "@/lib/utils";
import { format } from "date-fns";
import { Eye, Loader2 } from "lucide-react";
import { OrderStatusBadge } from "../orders/OrderStatusBadge"; 

interface CustomerDetailDialogProps {
  customerId: number;
}

export function CustomerDetailDialog({ customerId }: CustomerDetailDialogProps) {
  const [open, setOpen] = useState(false);
  const { data: customer, isLoading, isError } = useGetCustomerById(customerId);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="icon" className="h-8 w-8">
          <Eye className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Customer Details</DialogTitle>
          <DialogDescription>
            View information about this customer.
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="flex h-40 items-center justify-center">
            <Loader2 className="h-6 w-6 animate-spin text-primary" />
          </div>
        ) : isError ? (
          <div className="py-6 text-center text-destructive">
            <p>Failed to load customer details.</p>
          </div>
        ) : customer ? (
          <div className="space-y-6">
            {/* Customer Info */}
            <div className="rounded-md border p-4">
              <h3 className="mb-2 font-medium">Basic Information</h3>
              <div className="grid gap-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Name:</span>
                  <span>{`${customer.firstName || ""} ${customer.lastName || ""}`.trim() || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Email:</span>
                  <span>{customer.email || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Phone:</span>
                  <span>{customer.phone || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Address:</span>
                  <span>{customer.address || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">City:</span>
                  <span>{customer.city || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">State/Province:</span>
                  <span>{customer.state || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Country:</span>
                  <span>{customer.country || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Postal Code:</span>
                  <span>{customer.postalCode || "N/A"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Joined:</span>
                  <span>
                    {customer.createdAt
                      ? format(new Date(customer.createdAt), "MMMM dd, yyyy")
                      : "N/A"}
                  </span>
                </div>
              </div>
            </div>

            {/* Recent Orders */}
            {customer.orders && customer.orders.length > 0 ? (
              <div>
                <h3 className="mb-2 font-medium">Recent Orders</h3>
                <div className="rounded-md border">
                  <div className="divide-y">
                    {customer.orders.map((order: any) => (
                      <div key={order.id} className="flex items-center justify-between p-3">
                        <div>
                          <p className="font-medium">Order #{order.id}</p>
                          <p className="text-xs text-muted-foreground">
                            {format(new Date(order.orderDate), "MMM dd, yyyy")}
                          </p>
                        </div>
                        <div className="flex items-center gap-3">
                          <OrderStatusBadge status={order.orderStatus} />
                          <span className="font-medium">{formatCurrency(order.totalAmount)}</span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            ) : (
              <div className="rounded-md border p-4 text-center text-sm text-muted-foreground">
                No orders found for this customer.
              </div>
            )}
          </div>
        ) : (
          <div className="py-6 text-center text-muted-foreground">
            <p>No customer data available.</p>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
