"use client";

import { useState, useEffect } from "react";
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
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { ImageUploadButton, ImageUploadResult } from "@/components/shared/ImageUpload";
import { imageService } from "@/services/images";

const productSchema = z.object({
  name: z.string().min(3, "Name must be at least 3 characters").max(255, "Name must be less than 255 characters"),
  slug: z.string().max(255, "Slug must be less than 255 characters"),
  description: z.string().optional(),
  price: z.coerce.number().min(0.01, "Price must be at least 0.01"),
  originalPrice: z.coerce.number().min(0, "Original price must be a positive number").optional(),
  stockQuantity: z.coerce.number().min(0, "Stock quantity must be a non-negative number"),
  sku: z.string().max(50, "SKU must be less than 50 characters"),
  categoryId: z.coerce.number().min(1, "Category is required"),
  isFeatured: z.boolean().default(false),
  isActive: z.boolean().default(true),
  images: z.array(z.instanceof(File)).optional().default([]),
  imageIds: z.array(z.number()).optional().default([]),
});

export type ProductFormValues = z.infer<typeof productSchema>;

interface ProductFormProps {
  initialData?: Product & { imageDetails?: { id: number; fileName: string; filePath: string }[] };
  onSubmit: (data: ProductFormValues) => Promise<void>;
  isSubmitting: boolean;
}

export function ProductForm({
  initialData,
  onSubmit,
  isSubmitting,
}: ProductFormProps) {
  const [images, setImages] = useState<(File | string)[]>(
    initialData?.images || []
  );
  const [existingImageIds, setExistingImageIds] = useState<number[]>([]);
  const [pendingUploadResults, setPendingUploadResults] = useState<ImageUploadResult[]>([]);

  const { data: categoriesData, isLoading: isCategoriesLoading } = useHierarchicalCategories();
  const categories = categoriesData || [];

  useEffect(() => {
    if (initialData) {
      const existingIds: number[] = [];
      const imageUrls: string[] = [];
      
      // Extract image IDs from imageDetails if available
      if (initialData.imageDetails && initialData.imageDetails.length > 0) {
        initialData.imageDetails.forEach((detail) => {
          existingIds.push(detail.id);
          imageUrls.push(detail.filePath);
        });
      } else if (initialData.images && initialData.images.length > 0) {
        // Fallback to image URLs if no imageDetails
        initialData.images.forEach((img) => {
          if (typeof img === 'string') {
            imageUrls.push(img);
          }
        });
      }
      
      setImages(imageUrls);
      setExistingImageIds(existingIds);
    }
  }, [initialData]);

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
          categoryId: 1,
          isFeatured: false,
          isActive: true,
        },
  });

  const handleImagesSelected = async (result: ImageUploadResult) => {
    const validation = imageService.validateUploadResult(result);
    if (!validation.isValid) {
      return;
    }

    // Store the upload result for processing during submit
    setPendingUploadResults(prev => [...prev, result]);

    // Update the images display array
    switch (result.source) {
      case "device":
        if (result.files) {
          setImages(prev => [...prev, ...result.files!]);
        }
        break;
      case "url":
        if (result.urls) {
          setImages(prev => [...prev, ...result.urls!]);
        }
        break;
      case "system":
        if (result.systemFiles) {
          const urls = result.systemFiles.map(file => file.relativePath);
          setImages(prev => [...prev, ...urls]);
        }
        break;
    }
  };

  const handleRemoveImage = (index: number) => {
    const imageToRemove = images[index];
    
    // If removing an existing image (string URL), also remove its ID from existingImageIds
    if (typeof imageToRemove === 'string' && initialData?.imageDetails) {
      const imageDetail = initialData.imageDetails.find(detail => detail.filePath === imageToRemove);
      if (imageDetail) {
        setExistingImageIds(prev => prev.filter(id => id !== imageDetail.id));
      }
    }
    
    // Remove from pending upload results if it exists
    setPendingUploadResults(prev => {
      return prev.filter(result => {
        if (result.source === "device" && result.files) {
          return !result.files.some(file => file === imageToRemove);
        }
        if (result.source === "url" && result.urls) {
          return !result.urls.some(url => url === imageToRemove);
        }
        if (result.source === "system" && result.systemFiles) {
          return !result.systemFiles.some(file => file.relativePath === imageToRemove);
        }
        return true;
      });
    });
    
    setImages(prev => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (data: ProductFormValues) => {
    try {
      const allImageIds: number[] = [...existingImageIds];
      
      // Process all pending upload results
      for (const result of pendingUploadResults) {
        const processResult = await imageService.processImageUpload(result, { folderPath: "products" });
        
        if (processResult.success && processResult.imageIds) {
          allImageIds.push(...processResult.imageIds);
        } else {
          throw new Error(processResult.error || `Failed to process ${result.source} images`);
        }
      }

      await onSubmit({
        ...data,
        images: [],
        imageIds: allImageIds,
      });
    } catch (error) {
      throw error;
    }
  };

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

  const generateSKU = () => {
    const name = form.getValues("name");
    // get first letter of each word, convert to uppercase, and join with hyphens
    if (!name) return;
    // remove any non-alphanumeric characters and extra spaces
    const cleanedName = name.replace(/[^a-zA-Z0-9\s]/g, "").replace(/\s+/g, " ");
    const sku = cleanedName
      .split(" ")
      .map(word => word.charAt(0).toUpperCase())
      .join("");
    form.setValue("sku", sku);
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
            <div className="flex items-end">
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
              <Button
                type="button"
                variant="outline"
                onClick={generateSKU}
                className="shrink-0 ml-2"
              >
                Generate
              </Button>
            </div>
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
                />
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="space-y-2">
            <Label>Product Images</Label>
            <ImageUploadButton
              onImagesSelected={handleImagesSelected}
              currentImages={images}
              onRemoveImage={handleRemoveImage}
              multiple={true}
              label="Upload Product Images"
              acceptedTypes={["image/*"]}
              maxSize={10 * 1024 * 1024}
            />
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
