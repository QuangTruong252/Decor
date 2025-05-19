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

      <div className="rounded-lg border bg-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="px-4 py-3 text-left font-medium">Image</th>
                <th className="px-4 py-3 text-left font-medium">Name</th>
                <th className="px-4 py-3 text-left font-medium">SKU</th>
                <th className="px-4 py-3 text-right font-medium">Price</th>
                <th className="px-4 py-3 text-right font-medium">Stock</th>
                <th className="px-4 py-3 text-center font-medium">Status</th>
                <th className="px-4 py-3 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {products && products.map((product) => (
                <tr key={product.id} className="border-b transition-colors hover:bg-muted/50">
                  <td className="px-4 py-3">
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
                  </td>
                  <td className="px-4 py-3">
                    <div>
                      <p className="font-medium">{product.name}</p>
                      <p className="text-xs text-muted-foreground">{product.categoryName}</p>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">{product.sku}</td>
                  <td className="px-4 py-3 text-right">
                    <div>
                      <p className="font-medium">{formatCurrency(product.price)}</p>
                      {product.originalPrice > 0 && product.originalPrice !== product.price && (
                        <p className="text-xs text-muted-foreground line-through">
                          {formatCurrency(product.originalPrice)}
                        </p>
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <span className={`${product.stockQuantity <= 10 ? "text-destructive" : ""}`}>
                      {product.stockQuantity}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-center">
                    {product.isActive ? (
                      <Check className="mx-auto h-5 w-5 text-green-500" />
                    ) : (
                      <X className="mx-auto h-5 w-5 text-destructive" />
                    )}
                  </td>
                  <td className="px-4 py-3 text-right">
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
                  </td>
                </tr>
              ))}

              {products && products.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-8 text-center text-muted-foreground">
                    No products found. Add your first product!
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}