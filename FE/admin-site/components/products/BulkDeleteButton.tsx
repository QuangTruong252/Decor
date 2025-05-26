"use client";

import { Button } from "@/components/ui/button";
import { useConfirmationDialog } from "@/components/ui/confirmation-dialog";
import { useBulkDeleteProducts } from "@/hooks/useProducts";
import { Trash2 } from "lucide-react";

interface BulkDeleteButtonProps {
  selectedIds: number[];
  onSuccess?: () => void;
}

export function BulkDeleteButton({ selectedIds, onSuccess }: BulkDeleteButtonProps) {
  const { confirm } = useConfirmationDialog();
  const bulkDeleteMutation = useBulkDeleteProducts();

  const handleBulkDelete = async () => {
    if (selectedIds.length === 0) return;

    confirm({
      title: "Delete Products",
      message: `Are you sure you want to delete ${selectedIds.length} product${selectedIds.length > 1 ? 's' : ''}? This action cannot be undone.`,
      confirmText: "Delete",
      cancelText: "Cancel",
      variant: "destructive",
      onConfirm: async () => {
        try {
          await bulkDeleteMutation.mutateAsync(selectedIds);
          onSuccess?.();
        } catch (error) {
          console.error("Bulk delete error:", error);
        }
      },
    });
  };

  if (selectedIds.length === 0) {
    return null;
  }

  return (
    <Button
      variant="destructive"
      size="sm"
      onClick={handleBulkDelete}
      disabled={bulkDeleteMutation.isPending}
      className="gap-2"
    >
      {bulkDeleteMutation.isPending ? (
        <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"></div>
      ) : (
        <Trash2 className="h-4 w-4" />
      )}
      Delete {selectedIds.length} item{selectedIds.length > 1 ? 's' : ''}
    </Button>
  );
}
