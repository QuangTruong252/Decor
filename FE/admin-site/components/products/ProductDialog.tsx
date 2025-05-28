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
import { ProductForm, ProductFormValues } from "./ProductForm";
import { useCreateProduct, useUpdateProduct } from "@/hooks/useProducts";
import { getProductById, Product } from "@/services/products";
import { PackagePlus, Pencil } from "lucide-react";
import { useConfirmationDialog } from "@/components/ui/confirmation-dialog";

interface AddProductDialogProps {
  onSuccess?: () => void;
}

export function AddProductDialog({ onSuccess }: AddProductDialogProps) {
  const [open, setOpen] = useState(false);
  const createProductMutation = useCreateProduct();

  const handleSubmit = async (data: ProductFormValues & { images?: File[] }) => {
    try {
      await createProductMutation.mutateAsync(data);
      setOpen(false);
      if (onSuccess) onSuccess();
    } catch (error) {
      console.error("Error creating product:", error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button className="flex items-center gap-2">
          <PackagePlus className="h-4 w-4" />
          Add Product
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Add New Product</DialogTitle>
          <DialogDescription>
            Fill in the details to create a new product.
          </DialogDescription>
        </DialogHeader>
        <ProductForm
          onSubmit={handleSubmit}
          isSubmitting={createProductMutation.isPending}
        />
      </DialogContent>
    </Dialog>
  );
}

interface EditProductDialogProps {
  product: Product;
  onSuccess?: () => void;
}

export function EditProductDialog({ product, onSuccess }: EditProductDialogProps) {
  const [open, setOpen] = useState(false);
  const [productDetails, setProductDetails] = useState<Product | undefined>(undefined);
  const [isLoading, setIsLoading] = useState(false); // Add this state variable
  const updateProductMutation = useUpdateProduct();
  const handleEdit = async () => {
    setOpen(true);
    setIsLoading(true); // Set loading state to true
    const res = await getProductById(product.id);
    setProductDetails(res);
    setIsLoading(false); // Set loading state to false
  };
  const handleSubmit = async (data: ProductFormValues & { images?: File[] }) => {
    try {
      await updateProductMutation.mutateAsync({
        id: product.id,
        ...data,
      });
      setOpen(false);
      if (onSuccess) onSuccess();
    } catch (error) {
      console.error("Error updating product:", error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="icon" className="h-8 w-8 hover:cursor-pointer" onClick={() => handleEdit()}>
          <Pencil className="h-4 w-4" />
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Edit Product</DialogTitle>
          <DialogDescription>
            Update the product details.
          </DialogDescription>
        </DialogHeader>
        {isLoading ? (
          <div className="flex h-40 items-center justify-center">
            <div className="h-6 w-6 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
          </div>
        ) : (
          <ProductForm
            initialData={productDetails || product}
            onSubmit={handleSubmit}
            isSubmitting={updateProductMutation.isPending}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

interface DeleteProductDialogProps {
  productId: number;
  onDelete: (id: number) => Promise<void>;
  isDeleting: boolean | undefined;
}

export function DeleteProductButton({
  productId,
  onDelete,
  isDeleting,
}: DeleteProductDialogProps) {
  const { confirm } = useConfirmationDialog()

  const handleDelete = async () => {
    confirm({
      title: "Delete Product",
      message: "Are you sure you want to delete this product? This action cannot be undone.",
      confirmText: "Delete",
      cancelText: "Cancel",
      variant: "destructive",
      onConfirm: async () => {
        await onDelete(productId);
      },
    });
  };

  return (
    <Button
      variant="outline"
      size="icon"
      className="h-8 w-8 text-destructive hover:bg-destructive/10 hover:text-destructive hover:cursor-pointer"
      onClick={handleDelete}
      disabled={isDeleting}
      aria-label="Delete product"
      tabIndex={0}
      onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') handleDelete(); }}
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
