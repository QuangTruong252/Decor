"use client";

import { useState } from "react";
import { useGetProducts, useDeleteProduct } from "@/hooks/useProducts";
import { formatCurrency, getImageUrl } from "@/lib/utils";
import { Check, X, Loader2 } from "lucide-react";
import {
  AddProductDialog,
  EditProductDialog,
  DeleteProductButton
} from "@/components/products/ProductDialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

export default function ProductsPage() {
  const [selectedProduct, setSelectedProduct] = useState<number | null>(null);
  const { data: products, isLoading, isError, refetch } = useGetProducts();
  const deleteProductMutation = useDeleteProduct();

  const handleDelete = async (id: number) => {
    setSelectedProduct(id);
    try {
      await deleteProductMutation.mutateAsync(id);
    } catch (error) {
      console.error("Delete error:", error);
    } finally {
      setSelectedProduct(null);
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
        <p>An error occurred while loading the product list</p>
        <button
          onClick={() => window.location.reload()}
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
        <h1 className="text-3xl font-bold">Products</h1>
        <AddProductDialog onSuccess={() => refetch()} />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Image</TableHead>
            <TableHead>Name</TableHead>
            <TableHead>SKU</TableHead>
            <TableHead className="text-right">Price</TableHead>
            <TableHead className="text-right">Stock</TableHead>
            <TableHead className="text-center">Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {products && products.length > 0 ? (
            products.map((product) => (
              <TableRow key={product.id}>
                <TableCell>
                  <div className="h-10 w-10 overflow-hidden rounded-md bg-muted">
                    {product.images && product.images.length > 0 ? (
                      <img
                        src={getImageUrl(product.images[0])}
                        alt={product.name}
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <div className="flex h-full w-full items-center justify-center bg-muted text-muted-foreground">
                        No img
                      </div>
                    )}
                  </div>
                </TableCell>
                <TableCell>
                  <div>
                    <p className="font-medium">{product.name}</p>
                    <p className="text-xs text-muted-foreground">{product.categoryName}</p>
                  </div>
                </TableCell>
                <TableCell className="text-muted-foreground">{product.sku}</TableCell>
                <TableCell className="text-right">
                  <div>
                    <p className="font-medium">{formatCurrency(product.price)}</p>
                    {product.originalPrice > 0 && product.originalPrice !== product.price && (
                      <p className="text-xs text-muted-foreground line-through">
                        {formatCurrency(product.originalPrice)}
                      </p>
                    )}
                  </div>
                </TableCell>
                <TableCell className="text-right">
                  <span className={`${product.stockQuantity <= 10 ? "text-destructive" : ""}`}>
                    {product.stockQuantity}
                  </span>
                </TableCell>
                <TableCell className="text-center">
                  {product.isActive ? (
                    <Check className="mx-auto h-5 w-5 text-green-500" />
                  ) : (
                    <X className="mx-auto h-5 w-5 text-destructive" />
                  )}
                </TableCell>
                <TableCell className="text-right">
                  <div className="flex justify-end gap-2">
                    <EditProductDialog
                      product={product}
                      onSuccess={() => refetch()}
                    />
                    <DeleteProductButton
                      productId={product.id}
                      onDelete={handleDelete}
                      isDeleting={selectedProduct !== null && selectedProduct === product.id}
                    />
                  </div>
                </TableCell>
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell colSpan={7} className="h-24 text-center text-muted-foreground">
                No products found. Add your first product!
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </div>
  );
}