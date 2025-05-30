# Image Upload System

A comprehensive, reusable image upload system with multiple upload methods and consistent UI/UX.

## Features

- **3 Upload Methods:**
  - Upload from Device (drag & drop + file picker)
  - From URL (with validation and preview)
  - From System Files (browse existing files in the system)

- **Reusable Components:**
  - Single or multiple image selection
  - Configurable file types and size limits
  - Consistent UI/UX across the application
  - Accessibility features built-in

- **Advanced Features:**
  - Image preview with remove functionality
  - URL validation for external images
  - File system integration
  - Progress indicators
  - Error handling

## Components

### ImageUploadButton

The main reusable component that provides a complete image upload interface.

```tsx
import { ImageUploadButton } from "@/components/shared/ImageUpload";

<ImageUploadButton
  onImagesSelected={handleImagesSelected}
  currentImages={images}
  onRemoveImage={handleRemoveImage}
  multiple={true}
  label="Upload Images"
  acceptedTypes={["image/*"]}
  maxSize={10 * 1024 * 1024} // 10MB
/>
```

### ImageUploadDialog

The dialog component with tabs for different upload methods.

```tsx
import { ImageUploadDialog } from "@/components/shared/ImageUpload";

<ImageUploadDialog
  open={showDialog}
  onOpenChange={setShowDialog}
  onImagesSelected={handleImagesSelected}
  multiple={true}
  acceptedTypes={["image/*"]}
  maxSize={10 * 1024 * 1024}
/>
```

## Usage Examples

### Product Images (Multiple)

```tsx
import { ImageUploadButton, ImageUploadResult } from "@/components/shared/ImageUpload";

const ProductForm = () => {
  const [images, setImages] = useState<(File | string)[]>([]);

  const handleImagesSelected = (result: ImageUploadResult) => {
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
    setImages(prev => prev.filter((_, i) => i !== index));
  };

  return (
    <ImageUploadButton
      onImagesSelected={handleImagesSelected}
      currentImages={images}
      onRemoveImage={handleRemoveImage}
      multiple={true}
      label="Upload Product Images"
      acceptedTypes={["image/*"]}
      maxSize={10 * 1024 * 1024}
    />
  );
};
```

### Category Thumbnail (Single)

```tsx
import { ImageUploadButton, ImageUploadResult } from "@/components/shared/ImageUpload";

const CategoryForm = () => {
  const [thumbnail, setThumbnail] = useState<string | File | null>(null);

  const handleImageSelected = (result: ImageUploadResult) => {
    switch (result.source) {
      case "device":
        if (result.files && result.files.length > 0) {
          setThumbnail(result.files[0]);
        }
        break;
      case "url":
        if (result.urls && result.urls.length > 0) {
          setThumbnail(result.urls[0]);
        }
        break;
      case "system":
        if (result.systemFiles && result.systemFiles.length > 0) {
          setThumbnail(result.systemFiles[0].relativePath);
        }
        break;
    }
  };

  const handleRemoveImage = () => {
    setThumbnail(null);
  };

  return (
    <ImageUploadButton
      onImagesSelected={handleImageSelected}
      currentImages={thumbnail ? [thumbnail] : []}
      onRemoveImage={handleRemoveImage}
      multiple={false}
      label="Upload Category Thumbnail"
      acceptedTypes={["image/*"]}
      maxSize={5 * 1024 * 1024} // 5MB
      aspectRatio="1:1"
    />
  );
};
```

## Props

### ImageUploadButtonProps

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `onImagesSelected` | `(result: ImageUploadResult) => void` | - | Callback when images are selected |
| `multiple` | `boolean` | `true` | Allow multiple image selection |
| `currentImages` | `(File \| string)[]` | `[]` | Currently selected images |
| `onRemoveImage` | `(index: number) => void` | - | Callback to remove an image |
| `acceptedTypes` | `string[]` | `["image/*"]` | Accepted file types |
| `maxSize` | `number` | `10MB` | Maximum file size in bytes |
| `aspectRatio` | `string` | - | Preferred aspect ratio (e.g., "1:1", "16:9") |
| `className` | `string` | - | Additional CSS classes |
| `disabled` | `boolean` | `false` | Disable the upload button |
| `label` | `string` | `"Upload Images"` | Button label text |

### ImageUploadResult

```tsx
interface ImageUploadResult {
  source: 'device' | 'url' | 'system';
  files?: File[];
  urls?: string[];
  systemFiles?: FileItem[];
}
```

## Integration with Forms

The component integrates seamlessly with form libraries like react-hook-form:

```tsx
// In ProductForm.tsx
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
```

## Accessibility

- Full keyboard navigation support
- ARIA labels and descriptions
- Screen reader friendly
- Focus management
- High contrast support

## File System Integration

The system files tab integrates with the existing file manager to allow users to select from previously uploaded images, providing a consistent experience across the application.

## Customization

The components use Tailwind CSS classes and can be easily customized by:

1. Modifying the default props
2. Passing custom className props
3. Updating the component styles
4. Extending the types for additional functionality

## Error Handling

- File size validation
- File type validation
- URL validation for external images
- Network error handling
- User-friendly error messages
