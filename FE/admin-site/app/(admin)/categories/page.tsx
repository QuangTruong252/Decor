"use client";

import { CategoriesDataTable } from "@/components/categories/CategoriesDataTable";

export default function CategoriesPage() {
  return (
    <div className="p-6 overflow-y-auto">
      <CategoriesDataTable/>
    </div>
  );
}