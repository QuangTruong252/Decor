"use client";

import { useImage } from "@/hooks/useImage";
import { getImageUrl } from "@/lib/utils";
import Image from "next/image";

interface ImageExampleProps {
  imagePath?: string;
}

export function ImageExample({ imagePath = "products/sample.jpg" }: ImageExampleProps) {
  // Using the useImage hook in the component
  const image = useImage();

  // Using the utility function directly (not through the hook)
  const directImageUrl = getImageUrl(imagePath);

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-semibold">Image URL Processing Example</h2>

      <div className="grid gap-4 md:grid-cols-2">
        {/* Using the useImage hook */}
        <div className="rounded-lg border p-4">
          <h3 className="mb-2 font-medium">Using useImage hook</h3>
          <div className="aspect-video overflow-hidden rounded-md bg-muted">
            <img
              src={image.getUrl(imagePath)}
              alt="Image example"
              className="h-full w-full object-cover"
            />
          </div>
          <p className="mt-2 text-sm text-muted-foreground">
            URL: {image.getUrl(imagePath)}
          </p>
        </div>

        {/* Using the utility function directly */}
        <div className="rounded-lg border p-4">
          <h3 className="mb-2 font-medium">Using getImageUrl function directly</h3>
          <div className="aspect-video overflow-hidden rounded-md bg-muted">
            <img
              src={directImageUrl}
              alt="Image example"
              className="h-full w-full object-cover"
            />
          </div>
          <p className="mt-2 text-sm text-muted-foreground">
            URL: {directImageUrl}
          </p>
        </div>
      </div>

      {/* Example with Next.js Image component */}
      <div className="rounded-lg border p-4">
        <h3 className="mb-2 font-medium">Using with Next.js Image</h3>
        <div className="relative aspect-video overflow-hidden rounded-md bg-muted">
          <Image
            {...image.getSrc(imagePath)}
            alt="Example image with Next.js Image"
            fill
            className="object-cover"
          />
        </div>
        <p className="mt-2 text-sm text-muted-foreground">
          URL: {image.getUrl(imagePath)}
        </p>
      </div>
    </div>
  );
}
