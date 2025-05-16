"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Loader2 } from "lucide-react";
import { Product } from "@/services/products";
import { useGetCategories } from "@/hooks/useCategories";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

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
  const [images, setImages] = useState<File[]>([]);
  const { data: categories, isLoading: isCategoriesLoading } = useGetCategories();

  // Initialize the form with react-hook-form
  const form = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema),
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
      setImages(fileArray);
    }
  };

  // Handle form submission
  const handleSubmit = async (data: ProductFormValues) => {
    try {
      // Add images to the data
      const formData = { ...data, images };
      await onSubmit(formData);
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
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
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
                  onBlur={(e) => {
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
              <Select
                disabled={isCategoriesLoading}
                onValueChange={(value) => field.onChange(parseInt(value))}
                value={field.value?.toString()}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a category" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {categories?.map((category) => (
                    <SelectItem key={category.id} value={category.id.toString()}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="space-y-2">
          <Label>Product Images</Label>
          <Input
            type="file"
            accept="image/*"
            multiple
            onChange={handleImageChange}
            className="cursor-pointer"
          />
          {images.length > 0 && (
            <p className="text-sm text-muted-foreground">
              {images.length} image(s) selected
            </p>
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
  );
}
