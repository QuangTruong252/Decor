"use client";
import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Category } from "@/services/categories";
import { CategoryForm } from "./CategoryForm";
import { useCreateCategory, useUpdateCategory, useDeleteCategory } from "@/hooks/useCategories";

type CategoryDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialData?: Category | null;
};

export const CategoryDialog = ({ open, onOpenChange, initialData }: CategoryDialogProps) => {
  const isEdit = !!initialData;
  const [loading, setLoading] = useState(false);
  const createCategory = useCreateCategory();
  const updateCategory = useUpdateCategory();

  const handleSubmit = async (data: any) => {
    setLoading(true);
    try {
      if (isEdit && initialData) {
        await updateCategory.mutateAsync({ ...data, id: initialData.id });
      } else {
        await createCategory.mutateAsync(data);
      }
      onOpenChange(false);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit Category" : "Add Category"}</DialogTitle>
        </DialogHeader>
        {loading ? (
          <div className="flex h-40 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
          </div>
        ) : (
          <CategoryForm
            initialData={initialData || undefined}
            onSubmit={handleSubmit}
            loading={loading}
            submitLabel={isEdit ? "Update" : "Create"}
          />
        )}
      </DialogContent>
    </Dialog>
  );
};