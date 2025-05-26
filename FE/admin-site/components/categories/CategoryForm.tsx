"use client";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Category, CreateCategoryPayload } from "@/services/categories";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useGetCategories } from "@/hooks/useCategories";
import { cn, getImageUrl } from "@/lib/utils";

const categorySchema = z.object({
  name: z.string().min(2, "Name is required").max(100),
  slug: z.string().min(1, "Slug is required").max(100),
  description: z.string().max(255).optional(),
  parentId: z.string().optional(),
  image: z.any().optional(),
});

type CategoryFormProps = {
  initialData?: Partial<Category>;
  onSubmit: (data: CreateCategoryPayload) => void;
  loading?: boolean;
  submitLabel?: string;
};

export const CategoryForm = ({ initialData, onSubmit, loading, submitLabel = "Save" }: CategoryFormProps) => {
  const [imagePreview, setImagePreview] = useState<string | null>(null);

  const form = useForm<z.infer<typeof categorySchema>>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: initialData?.name || "",
      slug: initialData?.slug || "",
      description: initialData?.description || "",
      parentId: initialData?.parentId ? String(initialData.parentId) : "0",
      image: undefined,
    },
  });

  const { data: categories } = useGetCategories();

  useEffect(() => {
    if (initialData) {
      // Set image preview if available
      if (initialData.imageUrl) {
        setImagePreview(getImageUrl(initialData.imageUrl));
      } else {
        setImagePreview(null);
      }

      form.reset({
        name: initialData.name || "",
        slug: initialData.slug || "",
        description: initialData.description || "",
        parentId: initialData.parentId ? String(initialData.parentId) : "0",
        image: undefined,
      });
    }
    // eslint-disable-next-line
  }, [initialData]);

  const handleSubmit = (values: z.infer<typeof categorySchema>) => {
    const payload: CreateCategoryPayload = {
      name: values.name,
      slug: values.slug,
      description: values.description,
      parentId: values.parentId && values.parentId !== "0" ? Number(values.parentId) : null,
      image: values.image instanceof FileList ? values.image[0] : undefined,
    };
    onSubmit(payload);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <Input {...field} placeholder="Category name" autoFocus required />
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
              <Input {...field} placeholder="category-slug" required />
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
              <Textarea {...field} placeholder="Description (optional)" />
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="parentId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Parent Category</FormLabel>
              <Select
                onValueChange={field.onChange}
                value={field.value || "0"}
                defaultValue={field.value || "0"}
              >
                <SelectTrigger>
                  <SelectValue placeholder="None" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="0">None</SelectItem>
                  {categories?.filter(c => !initialData?.id || c.id !== initialData.id).map((cat) => (
                    <SelectItem key={cat.id} value={String(cat.id)}>{cat.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="image"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Image</FormLabel>
              <div className="space-y-2">
                {imagePreview && (
                  <div className="mb-2">
                    <p className="text-sm text-muted-foreground mb-1">Current image:</p>
                    <div className="relative w-48 h-24 group border rounded overflow-hidden">
                      <img
                        src={imagePreview}
                        alt="Current category image"
                        className="object-cover w-full h-full"
                      />
                    </div>
                  </div>
                )}
                <div className="flex items-center gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => document.getElementById('category-image-upload')?.click()}
                    className="flex items-center gap-2"
                    aria-label={imagePreview ? "Change image" : "Upload image"}
                    tabIndex={0}
                    onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') document.getElementById('category-image-upload')?.click(); }}
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v2a2 2 0 002 2h12a2 2 0 002-2v-2M7 10l5-5m0 0l5 5m-5-5v12" />
                    </svg>
                    {imagePreview ? "Change Image" : "Upload Image"}
                  </Button>
                  <Input
                    id="category-image-upload"
                    type="file"
                    accept="image/*"
                    onChange={e => {
                      field.onChange(e.target.files);

                      // Create preview for the newly selected image
                      if (e.target.files && e.target.files.length > 0) {
                        const file = e.target.files[0];
                        const reader = new FileReader();
                        reader.onloadend = () => {
                          setImagePreview(reader.result as string);
                        };
                        reader.readAsDataURL(file);
                      }
                    }}
                    className="hidden"
                    tabIndex={-1}
                    aria-label="Select image to upload"
                  />
                </div>
              </div>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" className="w-full" disabled={loading}>
          {submitLabel}
        </Button>
      </form>
    </Form>
  );
};