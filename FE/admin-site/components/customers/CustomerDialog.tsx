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
import { CustomerForm } from "./CustomerForm";
import { useCreateCustomer, useUpdateCustomer, useGetCustomerById } from "@/hooks/useCustomers";
import { CustomerDTO } from "@/services/customers";
import { Plus, Pencil } from "lucide-react";
import { useConfirmationDialog } from "@/components/ui/confirmation-dialog";

interface AddCustomerDialogProps {
  onSuccess?: () => void;
}

export function AddCustomerDialog({ onSuccess }: AddCustomerDialogProps) {
  const [open, setOpen] = useState(false);
  const createCustomerMutation = useCreateCustomer();

  const handleSubmit = async (data: any) => {
    try {
      await createCustomerMutation.mutateAsync(data);
      setOpen(false);
      if (onSuccess) onSuccess();
    } catch (error) {
      console.error("Error creating customer:", error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button className="flex items-center gap-2">
          <Plus className="h-4 w-4" />
          Add Customer
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Add New Customer</DialogTitle>
          <DialogDescription>
            Fill in the details to create a new customer.
          </DialogDescription>
        </DialogHeader>
        <CustomerForm
          onSubmit={handleSubmit}
          isSubmitting={createCustomerMutation.isPending}
        />
      </DialogContent>
    </Dialog>
  );
}

interface EditCustomerDialogProps {
  customer: CustomerDTO;
  onSuccess?: () => void;
}

export function EditCustomerDialog({ customer, onSuccess }: EditCustomerDialogProps) {
  const [open, setOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const { data: customerDetails, refetch } = useGetCustomerById(customer.id);
  const updateCustomerMutation = useUpdateCustomer();

  const handleEdit = async () => {
    setOpen(true);
    setIsLoading(true);
    await refetch();
    setIsLoading(false);
  };

  const handleSubmit = async (data: any) => {
    try {
      await updateCustomerMutation.mutateAsync({
        id: customer.id,
        customer: data,
      });
      setOpen(false);
      if (onSuccess) onSuccess();
    } catch (error) {
      console.error("Error updating customer:", error);
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
          <DialogTitle>Edit Customer</DialogTitle>
          <DialogDescription>
            Update the customer details.
          </DialogDescription>
        </DialogHeader>
        {isLoading ? (
          <div className="flex h-40 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
          </div>
        ) : (
          <CustomerForm
            initialData={customerDetails || customer}
            onSubmit={handleSubmit}
            isSubmitting={updateCustomerMutation.isPending}
            isEdit={true}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

interface DeleteCustomerButtonProps {
  customerId: number;
  onDelete: (id: number) => Promise<void>;
  isDeleting?: boolean;
}

export function DeleteCustomerButton({
  customerId,
  onDelete,
  isDeleting,
}: DeleteCustomerButtonProps) {
  const { confirm } = useConfirmationDialog();

  const handleDelete = async () => {
    confirm({
      title: "Delete Customer",
      message: "Are you sure you want to delete this customer? This action cannot be undone.",
      confirmText: "Delete",
      cancelText: "Cancel",
      variant: "destructive",
      onConfirm: async () => {
        await onDelete(customerId);
      },
    });
  };

  return (
    <Button
      variant="outline"
      size="icon"
      className="h-8 w-8 text-destructive hover:bg-destructive/10 hover:text-destructive"
      onClick={handleDelete}
      disabled={isDeleting}
      aria-label="Delete customer"
    >
      {isDeleting ? (
        <div className="h-4 w-4 animate-spin rounded-full border-2 border-destructive border-t-transparent"></div>
      ) : (
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          className="lucide lucide-trash-2"
        >
          <path d="M3 6h18" />
          <path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6" />
          <path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2" />
          <line x1="10" x2="10" y1="11" y2="17" />
          <line x1="14" x2="14" y1="11" y2="17" />
        </svg>
      )}
    </Button>
  );
}
