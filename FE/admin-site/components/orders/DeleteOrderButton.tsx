"use client";

import { Button } from "@/components/ui/button";
import { useConfirmationDialog } from "@/components/ui/confirmation-dialog";
import { Trash2 } from "lucide-react";

interface DeleteOrderButtonProps {
  orderId: number;
  onDelete: (id: number) => Promise<void>;
  isDeleting?: boolean;
}

export function DeleteOrderButton({
  orderId,
  onDelete,
  isDeleting,
}: DeleteOrderButtonProps) {
  const { confirm } = useConfirmationDialog();

  const handleDelete = async () => {
    confirm({
      title: "Delete Order",
      message: "Are you sure you want to delete this order? This action cannot be undone.",
      confirmText: "Delete",
      cancelText: "Cancel",
      variant: "destructive",
      onConfirm: async () => {
        await onDelete(orderId);
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
      aria-label="Delete order"
    >
      {isDeleting ? (
        <div className="h-4 w-4 animate-spin rounded-full border-2 border-destructive border-t-transparent"></div>
      ) : (
        <Trash2 className="h-4 w-4" />
      )}
    </Button>
  );
}
