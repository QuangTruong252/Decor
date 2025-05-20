"use client";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { useGetCategories } from "@/hooks/useCategories";
import { Category } from "@/services/categories";
import { CategoryDialog } from "@/components/categories/CategoryDialog";
import { Plus, Pencil, Loader2 } from "lucide-react";
import { getImageUrl } from "@/lib/utils";

export default function CategoriesPage() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editCategory, setEditCategory] = useState<Category | null>(null);
  const { data: categories, isLoading, isError, refetch } = useGetCategories();

  const handleAdd = () => {
    setEditCategory(null);
    setDialogOpen(true);
  };

  const handleEdit = (category: Category) => {
    setEditCategory(category);
    setDialogOpen(true);
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
        <p>An error occurred while loading the category list</p>
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
        <h1 className="text-3xl font-bold">Categories</h1>
        <Button onClick={handleAdd} aria-label="Add Category" className="gap-2"><Plus className="w-4 h-4" />Add Category</Button>
      </div>
      <div className="rounded-lg border bg-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="px-4 py-3 text-left font-medium">Image</th>
                <th className="px-4 py-3 text-left font-medium">Name</th>
                <th className="px-4 py-3 text-left font-medium">Slug</th>
                <th className="px-4 py-3 text-left font-medium">Parent</th>
                <th className="px-4 py-3 text-left font-medium">Created</th>
                <th className="px-4 py-3 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {categories && categories.length > 0 ? (
                categories.map((cat) => (
                  <tr key={cat.id} className="border-b transition-colors hover:bg-muted/50">
                    <td className="px-4 py-3">
                      {cat.imageUrl ? (
                        <img src={getImageUrl(cat.imageUrl)} alt={cat.name} className="h-12 w-24 object-cover rounded" />
                      ) : (
                        <span className="text-muted-foreground">No image</span>
                      )}
                    </td>
                    <td className="px-4 py-3">{cat.name}</td>
                    <td className="px-4 py-3">{cat.slug}</td>
                    <td className="px-4 py-3">{cat.parentCategory?.name || "-"}</td>
                    <td className="px-4 py-3">{new Date(cat.createdAt).toLocaleDateString()}</td>
                    <td className="px-4 py-3 text-right">
                      <Button size="sm" variant="outline" onClick={() => handleEdit(cat)} aria-label={`Edit ${cat.name}`} className="gap-2"><Pencil className="w-4 h-4" />Edit</Button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-muted-foreground">
                    No categories found. Add your first category!
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
      <CategoryDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        initialData={editCategory}
      />
    </div>
  );
}