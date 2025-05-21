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
import { OrderForm } from "./OrderForm";
import { useCreateOrder, useUpdateOrder, useGetOrderById } from "@/hooks/useOrders";
import { OrderDTO } from "@/services/orders";
import { Plus, Pencil } from "lucide-react";

interface AddOrderDialogProps {
  onSuccess?: () => void;
}

export function AddOrderDialog({ onSuccess }: AddOrderDialogProps) {
  const [open, setOpen] = useState(false);
  const createOrderMutation = useCreateOrder();

  const handleSubmit = async (data: any) => {
    try {
      await createOrderMutation.mutateAsync(data);
      setOpen(false);
      if (onSuccess) onSuccess();
    } catch (error) {
      console.error("Error creating order:", error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button className="flex items-center gap-2">
          <Plus className="h-4 w-4" />
          Add Order
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Add New Order</DialogTitle>
          <DialogDescription>
            Fill in the details to create a new order.
          </DialogDescription>
        </DialogHeader>
        <OrderForm
          onSubmit={handleSubmit}
          isSubmitting={createOrderMutation.isPending}
        />
      </DialogContent>
    </Dialog>
  );
}

interface EditOrderDialogProps {
  order: OrderDTO;
  onSuccess?: () => void;
}

export function EditOrderDialog({ order, onSuccess }: EditOrderDialogProps) {
  const [open, setOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const { data: orderDetails, refetch } = useGetOrderById(order.id);
  const updateOrderMutation = useUpdateOrder();

  const handleEdit = async () => {
    setOpen(true);
    setIsLoading(true);
    await refetch();
    setIsLoading(false);
  };

  const handleSubmit = async (data: any) => {
    try {
      await updateOrderMutation.mutateAsync({
        id: order.id,
        order: data,
      });
      setOpen(false);
      if (onSuccess) onSuccess();
    } catch (error) {
      console.error("Error updating order:", error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="icon" className="h-8 w-8" onClick={() => handleEdit()}>
          <Pencil className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Edit Order</DialogTitle>
          <DialogDescription>
            Update the order details.
          </DialogDescription>
        </DialogHeader>
        {isLoading ? (
          <div className="flex h-40 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
          </div>
        ) : (
          <OrderForm
            initialData={orderDetails || order}
            onSubmit={handleSubmit}
            isSubmitting={updateOrderMutation.isPending}
            isEdit={true}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
