"use client";

import { useState } from "react";
import { Resolver, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Loader2 } from "lucide-react";
import { Product } from "@/services/products";
import { useHierarchicalCategories } from "@/hooks/useCategoryStore";
import { HierarchicalSelect } from "@/components/ui/hierarchical-select";
// Select components removed - using HierarchicalSelect instead
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { getImageUrl } from "@/lib/utils";

// Define the form schema with zod
const productSchema = z.object({
  name: z.string().min(1, "Name is required"),
  slug: z.string().min(1, "Slug is required"),
  description: z.string().optional(),
  price: z.coerce.number().min(0, "Price must be a positive number"),
  originalPrice: z.coerce.number().min(0, "Original price must be a positive number").optional(),
  stockQuantity: z.coerce.number().min(0, "Stock quantity must be a non-negative number"),
  sku: z.string().min(1, "SKU is required"),
  categoryId: z.coerce.number().min(1, "Category is required"),
  isFeatured: z.boolean().default(false),
  isActive: z.boolean().default(true),
  images: z.array(z.instanceof(File)).optional().default([]),
});

export type ProductFormValues = z.infer<typeof productSchema>;

interface ProductFormProps {
  initialData?: Product;
  onSubmit: (data: ProductFormValues) => Promise<void>;
  isSubmitting: boolean;
}

export function ProductForm({
  initialData,
  onSubmit,
  isSubmitting,
}: ProductFormProps) {
  // State for images (support both File and string for edit mode)
  const [images, setImages] = useState<(File | string)[]>(
    initialData && initialData.images ? initialData.images : []
  );
  const { data: categoriesData, isLoading: isCategoriesLoading } = useHierarchicalCategories();
  const categories = categoriesData || [];

  // Initialize the form with react-hook-form
  const form = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema) as Resolver<ProductFormValues>,
    defaultValues: initialData
      ? {
          name: initialData.name,
          slug: initialData.slug,
          description: initialData.description || "",
          price: initialData.price,
          originalPrice: initialData.originalPrice,
          stockQuantity: initialData.stockQuantity,
          sku: initialData.sku,
          categoryId: initialData.categoryId,
          isFeatured: initialData.isFeatured,
          isActive: initialData.isActive,
        }
      : {
          name: "",
          slug: "",
          description: "",
          price: 0,
          originalPrice: 0,
          stockQuantity: 0,
          sku: "",
          categoryId: 1, // Default category ID
          isFeatured: false,
          isActive: true,
        },
  });

  // Handle image upload
  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const fileArray = Array.from(e.target.files);
      setImages((prev) => [...prev, ...fileArray]);
    }
  };

  // Remove image (by index)
  const handleRemoveImage = (idx: number) => {
    setImages((prev) => prev.filter((_, i) => i !== idx));
  };

  // Handle form submission
  const handleSubmit = async (data: ProductFormValues) => {
    try {
      // Add images to the data
      await onSubmit({
        ...data,
        // Filter out string images and only keep File objects
        images: images.filter((img): img is File => img instanceof File)
      });
      // Don't reset the form here, as the dialog will be closed by the parent component
    } catch (error) {
      console.error("Form submission error:", error);
    }
  };

  // Generate slug from name
  const generateSlug = () => {
    const name = form.getValues("name");
    if (name) {
      const slug = name
        .toLowerCase()
        .replace(/[^\w\s-]/g, "")
        .replace(/\s+/g, "-");
      form.setValue("slug", slug);
    }
  };

  return (
    <>
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
        <div className="overflow-y-auto max-h-[calc(100vh-200px)] flex flex-col gap-3">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Product Name</FormLabel>
                <FormControl>
                  <Input
                    placeholder="Enter product name"
                    {...field}
                    onBlur={() => {
                      field.onBlur();
                      if (!form.getValues("slug")) {
                        generateSlug();
                      }
                    }}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="slug"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Slug</FormLabel>
                <div className="flex gap-2">
                  <FormControl>
                    <Input placeholder="product-slug" {...field} />
                  </FormControl>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={generateSlug}
                    className="shrink-0"
                  >
                    Generate
                  </Button>
                </div>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Textarea
                    placeholder="Enter product description"
                    className="min-h-[100px]"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <FormField
              control={form.control}
              name="price"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Price</FormLabel>
                  <FormControl>
                    <Input type="number" min="0" step="0.01" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="originalPrice"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Original Price (Optional)</FormLabel>
                  <FormControl>
                    <Input type="number" min="0" step="0.01" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <FormField
              control={form.control}
              name="stockQuantity"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Stock Quantity</FormLabel>
                  <FormControl>
                    <Input type="number" min="0" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="sku"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>SKU</FormLabel>
                  <FormControl>
                    <Input placeholder="SKU-12345" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <FormField
            control={form.control}
            name="categoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Category</FormLabel>
                <HierarchicalSelect
                  categories={categories || []}
                  value={field.value}
                  onValueChange={(value) => field.onChange(value)}
                  placeholder="Select a category"
                  disabled={isCategoriesLoading}
                  allowClear={false}
                  showPath={false}
                  leafOnly={false}
                  className="w-full"
                  onScroll={(e) => console.log('ProductForm scroll event:', e.target)}
                />
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="space-y-2">
            <Label>Product Images</Label>
            <div className="flex items-center gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => document.getElementById('product-image-upload')?.click()}
                className="flex items-center gap-2"
                aria-label="Upload images"
                tabIndex={0}
                onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') document.getElementById('product-image-upload')?.click(); }}
              >
                <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v2a2 2 0 002 2h12a2 2 0 002-2v-2M7 10l5-5m0 0l5 5m-5-5v12" /></svg>
                Upload
              </Button>
              <Input
                id="product-image-upload"
                type="file"
                accept="image/*"
                multiple
                onChange={handleImageChange}
                className="hidden"
                tabIndex={-1}
                aria-label="Select images to upload"
              />
            </div>
            {images.length > 0 && (
              <div className="flex flex-wrap gap-3 mt-2">
                {images.map((img, idx) => (
                  <div key={idx} className="relative w-24 h-24 group border rounded overflow-hidden">
                    {typeof img === "string" ? (
                      <img
                        src={getImageUrl(img)}
                        alt={`Product image ${idx + 1}`}
                        className="object-cover w-full h-full"
                      />
                    ) : (
                      <img
                        src={URL.createObjectURL(img)}
                        alt={`Product image ${idx + 1}`}
                        className="object-cover w-full h-full"
                      />
                    )}
                    <button
                      type="button"
                      aria-label="Remove image"
                      tabIndex={0}
                      onClick={() => handleRemoveImage(idx)}
                      onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') handleRemoveImage(idx); }}
                      className="absolute top-1 right-1 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center opacity-80 hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-red-400 z-10"
                    >
                      &times;
                    </button>
                  </div>
                ))}
              </div>
            )}
            {images.length > 0 && (
              <p className="text-sm text-muted-foreground">{images.length} image(s) selected</p>
            )}
          </div>

          <div className="flex flex-col gap-4 sm:flex-row">
            <FormField
              control={form.control}
              name="isFeatured"
              render={({ field }) => (
                <FormItem className="flex flex-row items-center gap-2 space-y-0">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <FormLabel className="text-sm font-normal">Featured Product</FormLabel>
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="isActive"
              render={({ field }) => (
                <FormItem className="flex flex-row items-center gap-2 space-y-0">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <FormLabel className="text-sm font-normal">Active</FormLabel>
                </FormItem>
              )}
            />
          </div>
        </div>

        <div className="flex justify-end">
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Saving...
              </>
            ) : (
              "Save Product"
            )}
          </Button>
        </div>
      </form>
    </Form>
    </>
  );
}
