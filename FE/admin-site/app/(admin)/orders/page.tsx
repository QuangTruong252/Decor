"use client";

import { useState } from "react";
import { useGetOrders, useDeleteOrder } from "@/hooks/useOrders";
import { OrderTable } from "@/components/orders/OrderTable";
import { AddOrderDialog } from "@/components/orders/OrderDialog";
import { Loader2 } from "lucide-react";

export default function OrdersPage() {
  const [selectedOrder, setSelectedOrder] = useState<number | null>(null);
  const { data: orders, isLoading, isError, refetch } = useGetOrders();
  const deleteOrderMutation = useDeleteOrder();

  const handleDelete = async (id: number) => {
    setSelectedOrder(id);
    try {
      await deleteOrderMutation.mutateAsync(id);
    } catch (error) {
      console.error("Delete error:", error);
    } finally {
      setSelectedOrder(null);
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-96 items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex h-96 flex-col items-center justify-center text-destructive">
        <p>An error occurred while loading the orders</p>
        <button
          onClick={() => refetch()}
          className="mt-4 rounded-md bg-primary px-4 py-2 text-primary-foreground"
        >
          Try again
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-2">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Orders</h1>
        <AddOrderDialog onSuccess={() => refetch()} />
      </div>

      <OrderTable
        orders={orders || []}
        onDelete={handleDelete}
        isDeleting={selectedOrder}
      />
    </div>
  );
}