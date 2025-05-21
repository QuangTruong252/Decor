"use client";

import { useState } from "react";
import { useGetCustomers, useDeleteCustomer } from "@/hooks/useCustomers";
import { CustomerTable } from "@/components/customers/CustomerTable";
import { AddCustomerDialog } from "@/components/customers/CustomerDialog";
import { Loader2 } from "lucide-react";

export default function CustomersPage() {
  const [selectedCustomer, setSelectedCustomer] = useState<number | null>(null);
  const { data: customers, isLoading, isError, refetch } = useGetCustomers();
  const deleteCustomerMutation = useDeleteCustomer();

  const handleDelete = async (id: number) => {
    setSelectedCustomer(id);
    try {
      await deleteCustomerMutation.mutateAsync(id);
    } catch (error) {
      console.error("Delete error:", error);
    } finally {
      setSelectedCustomer(null);
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
        <p>An error occurred while loading the customers</p>
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
        <h1 className="text-3xl font-bold">Customers</h1>
        <AddCustomerDialog onSuccess={() => refetch()} />
      </div>

      <CustomerTable
        customers={customers || []}
        onDelete={handleDelete}
        isDeleting={selectedCustomer}
      />
    </div>
  );
}