"use client";

import { ProductsDataTable } from "@/components/products/ProductsDataTable";

export default function ProductsPage() {

  return (
    <div className="p-6 overflow-y-auto">
      <ProductsDataTable />
    </div>
  );
}