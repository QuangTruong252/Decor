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
import { useHierarchicalCategories } from "@/hooks/useCategoryStore";
import { HierarchicalSelect } from "@/components/ui/hierarchical-select";
import { ImageUploadButton, ImageUploadResult } from "@/components/shared/ImageUpload";
import { imageService } from "@/services/images";

const categorySchema = z.object({
  name: z.string().min(2, "Name is required").max(100),
  slug: z.string().min(1, "Slug is required").max(100),
  description: z.string().max(255).optional(),
  parentId: z.number().nullable().optional(),
  image: z.any().optional(),
});

type CategoryFormProps = {
  initialData?: Partial<Category>;
  onSubmit: (data: CreateCategoryPayload) => void;
  loading?: boolean;
  submitLabel?: string;
};

export const CategoryForm = ({ initialData, onSubmit, loading, submitLabel = "Save" }: CategoryFormProps) => {
  const [currentImage, setCurrentImage] = useState<File | string | null>(null);
  const [existingImageId, setExistingImageId] = useState<number | null>(null);

  const form = useForm<z.infer<typeof categorySchema>>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: initialData?.name || "",
      slug: initialData?.slug || "",
      description: initialData?.description || "",
      parentId: initialData?.parentId || null,
      image: undefined,
    },
  });

  const { data: categoriesData } = useHierarchicalCategories();
  const categories = categoriesData || [];
  useEffect(() => {
    if (initialData) {
      if (initialData.imageUrl) {
        setCurrentImage(initialData.imageUrl);
        // If updating existing category with image, we should preserve the existing image ID
        // For now, we'll handle this when we have the actual image ID from the backend
        setExistingImageId(null);
      } else {
        setCurrentImage(null);
        setExistingImageId(null);
      }

      form.reset({
        name: initialData.name || "",
        slug: initialData.slug || "",
        description: initialData.description || "",
        parentId: initialData.parentId || null,
        image: undefined,
      });
    }
  }, [initialData, form]);

  const handleImagesSelected = (result: ImageUploadResult) => {
    switch (result.source) {
      case "device":
        if (result.files && result.files.length > 0) {
          setCurrentImage(result.files[0]);
          form.setValue("image", result.files[0]);
        }
        break;
      case "url":
        if (result.urls && result.urls.length > 0) {
          setCurrentImage(result.urls[0]);
          form.setValue("image", result.urls[0]);
        }
        break;
      case "system":
        if (result.systemFiles && result.systemFiles.length > 0) {
          setCurrentImage(result.systemFiles[0].relativePath);
          form.setValue("image", result.systemFiles[0].relativePath);
        }
        break;
    }
  };
  const handleRemoveImage = () => {
    setCurrentImage(null);
    setExistingImageId(null);
    form.setValue("image", undefined);
  };const handleSubmit = async (values: z.infer<typeof categorySchema>) => {
    try {
      let imageId: number | undefined = existingImageId || undefined;
      
      // If we have a new File to upload, upload it first
      if (currentImage instanceof File) {
        const uploadResult = await imageService.uploadImages([currentImage], "categories");
        if (uploadResult.success && uploadResult.imageIds && uploadResult.imageIds.length > 0) {
          imageId = uploadResult.imageIds[0];
        } else {
          throw new Error(uploadResult.error || "Failed to upload image");
        }
      }

      const payload: CreateCategoryPayload = {
        name: values.name,
        slug: values.slug,
        description: values.description,
        parentId: values.parentId || null,
        imageId,
      };
      
      onSubmit(payload);
    } catch (error) {
      throw error;
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
              <HierarchicalSelect
                categories={categories.filter(c => !initialData?.id || c.id !== initialData.id)}
                value={field.value || undefined}
                onValueChange={(value) => field.onChange(value)}
                placeholder="None (Root Category)"
                allowClear={true}
                showPath={true}
                className="w-full"
              />
              <FormMessage />
            </FormItem>
          )}
        />        <FormField
          control={form.control}
          name="image"
          render={() => (
            <FormItem>
              <FormLabel>Category Image</FormLabel>
              <ImageUploadButton
                onImagesSelected={handleImagesSelected}
                currentImages={currentImage ? [currentImage] : []}
                onRemoveImage={handleRemoveImage}
                multiple={false}
                label="Upload Category Image"
                acceptedTypes={["image/*"]}
                maxSize={5 * 1024 * 1024}
                aspectRatio="16:9"
              />
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