"use client";
import { useEffect } from "react";
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
import { cn } from "@/lib/utils";

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
  const form = useForm<z.infer<typeof categorySchema>>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: initialData?.name || "",
      slug: initialData?.slug || "",
      description: initialData?.description || "",
      parentId: initialData?.parentId ? String(initialData.parentId) : "",
      image: undefined,
    },
  });

  const { data: categories } = useGetCategories();

  useEffect(() => {
    if (initialData) {
      form.reset({
        name: initialData.name || "",
        slug: initialData.slug || "",
        description: initialData.description || "",
        parentId: initialData.parentId ? String(initialData.parentId) : "",
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
      parentId: values.parentId ? Number(values.parentId) : undefined,
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
              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger>
                  <SelectValue placeholder="None" />
                </SelectTrigger>
                <SelectContent>
                  {/* Remove the empty string value to avoid Select.Item with value="" */}
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
              <Input type="file" accept="image/*" onChange={e => field.onChange(e.target.files)} />
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