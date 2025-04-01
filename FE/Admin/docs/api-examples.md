# API Usage Examples

## Introduction

Tài liệu này cung cấp các ví dụ về cách sử dụng API trong dự án Nuxt 3 theo pattern mới. Chúng tôi đã centralize tất cả API clients từ file `api-services/api.ts` để đảm bảo tính nhất quán và type safety.

## Cấu trúc API Service

```typescript
// api-services/api-service.ts
import { 
  AuthApi, 
  BannerApi, 
  CategoryApi, 
  HealthCheckApi, 
  OrderApi, 
  ProductsApi, 
  ReviewApi 
} from './api';

export const apiService = {
  authApi: new AuthApi(),
  bannerApi: new BannerApi(),
  categoryApi: new CategoryApi(),
  healthCheckApi: new HealthCheckApi(),
  orderApi: new OrderApi(),
  productsApi: new ProductsApi(),
  reviewApi: new ReviewApi(),
};

export const handleApiError = (error: unknown, context?: string): void => {
  // ... error handling implementation
};
```

## Standard API Call Pattern

Sử dụng pattern chuẩn sau đây cho tất cả API calls:

```typescript
import { apiService, handleApiError, showSuccessToast } from '~/api-services/api-service'

// State
const products = ref([])
const isLoading = ref(false)

/**
 * Fetch all products from the API
 */
const fetchProducts = async () => {
  isLoading.value = true
  
  try {
    const res = await apiService.productsApi.apiProductsGet();
    if (res.status === 200) {
      products.value = res.data;
    } else {
      showErrorToast('Failed to load products');
    }
  } catch (err) {
    handleApiError(err, 'Failed to load products');
  } finally {
    isLoading.value = false
  }
}
```

## Ví dụ theo Tính năng

### Products Management

#### 1. Lấy danh sách sản phẩm

```typescript
const fetchProducts = async () => {
  isLoading.value = true
  
  try {
    const res = await apiService.productsApi.apiProductsGet();
    if (res.status === 200) {
      products.value = res.data;
    } else {
      showErrorToast('Failed to load products');
    }
  } catch (err) {
    handleApiError(err, 'Failed to load products');
  } finally {
    isLoading.value = false
  }
}
```

#### 2. Lấy thông tin sản phẩm theo ID

```typescript
const fetchProductDetails = async (productId) => {
  isLoading.value = true
  
  try {
    const res = await apiService.productsApi.apiProductsIdGet({
      id: productId
    });
    
    if (res.status === 200) {
      productDetails.value = res.data;
    } else {
      showErrorToast('Failed to load product details');
    }
  } catch (err) {
    handleApiError(err, 'Failed to load product details');
  } finally {
    isLoading.value = false
  }
}
```

#### 3. Xóa sản phẩm

```typescript
const deleteProduct = async (productId) => {
  isDeleting.value = true
  
  try {
    const res = await apiService.productsApi.apiProductsIdDelete({
      id: productId
    });
    
    if (res.status === 204 || res.status === 200) {
      showSuccessToast('Product deleted successfully');
      // Remove from UI or refresh list
    } else {
      showErrorToast('Failed to delete product');
    }
  } catch (err) {
    handleApiError(err, 'Failed to delete product');
  } finally {
    isDeleting.value = false
  }
}
```

### Categories Management

#### 1. Lấy danh sách danh mục

```typescript
const fetchCategories = async () => {
  isLoading.value = true
  
  try {
    const res = await apiService.categoryApi.apiCategoryGet();
    
    if (res.status === 200) {
      categories.value = res.data;
    } else {
      showErrorToast('Failed to load categories');
    }
  } catch (err) {
    handleApiError(err, 'Failed to load categories');
  } finally {
    isLoading.value = false
  }
}
```

#### 2. Lấy cấu trúc danh mục phân cấp

```typescript
const fetchCategoryHierarchy = async () => {
  isLoading.value = true
  
  try {
    const res = await apiService.categoryApi.apiCategoryHierarchicalGet();
    
    if (res.status === 200) {
      hierarchicalCategories.value = res.data;
    } else {
      showErrorToast('Failed to load category hierarchy');
    }
  } catch (err) {
    handleApiError(err, 'Failed to load category hierarchy');
  } finally {
    isLoading.value = false
  }
}
```

### Orders Management

#### 1. Lấy danh sách đơn hàng

```typescript
const fetchOrders = async () => {
  isLoading.value = true
  
  try {
    const res = await apiService.orderApi.apiOrderGet();
    
    if (res.status === 200) {
      orders.value = res.data;
    } else {
      showErrorToast('Failed to load orders');
    }
  } catch (err) {
    handleApiError(err, 'Failed to load orders');
  } finally {
    isLoading.value = false
  }
}
```

#### 2. Cập nhật trạng thái đơn hàng

```typescript
const updateOrderStatus = async (orderId, newStatus) => {
  isUpdating.value = true
  
  try {
    const res = await apiService.orderApi.apiOrderIdStatusPut({
      id: orderId,
      updateOrderStatusDTO: {
        orderStatus: newStatus
      }
    });
    
    if (res.status === 204 || res.status === 200) {
      showSuccessToast(`Order status updated to ${newStatus}`);
      // Update UI or refresh
    } else {
      showErrorToast('Failed to update order status');
    }
  } catch (err) {
    handleApiError(err, 'Failed to update order status');
  } finally {
    isUpdating.value = false
  }
}
```

### Banner Management

#### 1. Tạo banner mới (với upload file)

```typescript
const createBanner = async (bannerData, imageFile) => {
  isCreating.value = true
  
  try {
    const res = await apiService.bannerApi.apiBannerPost({
      title: bannerData.title,
      imageFile: imageFile,
      link: bannerData.link,
      isActive: bannerData.isActive,
      displayOrder: bannerData.displayOrder
    });
    
    if (res.status === 201 || res.status === 200) {
      showSuccessToast('Banner created successfully');
      // Update UI or redirect
    } else {
      showErrorToast('Failed to create banner');
    }
  } catch (err) {
    handleApiError(err, 'Failed to create banner');
  } finally {
    isCreating.value = false
  }
}
```

### Reviews Management

#### 1. Lấy đánh giá của sản phẩm

```typescript
const fetchProductReviews = async (productId) => {
  isLoading.value = true
  
  try {
    const res = await apiService.reviewApi.apiReviewProductProductIdGet({
      productId: productId
    });
    
    if (res.status === 200) {
      reviews.value = res.data;
    } else {
      showErrorToast('Failed to load product reviews');
    }
  } catch (err) {
    handleApiError(err, 'Failed to load product reviews');
  } finally {
    isLoading.value = false
  }
}
```

#### 2. Lấy điểm đánh giá trung bình

```typescript
const fetchAverageRating = async (productId) => {
  try {
    const res = await apiService.reviewApi.apiReviewProductProductIdRatingGet({
      productId: productId
    });
    
    if (res.status === 200) {
      averageRating.value = res.data;
    }
  } catch (err) {
    handleApiError(err, 'Failed to load average rating');
  }
}
```

## Advanced API Usage

### Sử dụng useApiAdvanced Composable

Ngoài cách gọi API trực tiếp, chúng ta còn có thể sử dụng composable `useApiAdvanced` để quản lý tốt hơn các trạng thái API như loading, cache và retry:

```typescript
import { useApiAdvanced } from '~/composables/useApi'
import { apiService } from '~/api-services/api-service'

const {
  data: products,
  isLoading,
  error,
  execute: fetchProducts
} = useApiAdvanced(
  () => apiService.productsApi.apiProductsGet(),
  { 
    retries: 2,
    cacheTime: 60000 // 1 minute cache
  }
);

// Sau đó gọi:
await fetchProducts();
```

### Xử lý Upload Files

Đối với các trường hợp upload file phức tạp, có thể cần sử dụng API compat:

```typescript
import { useApiCompat } from '~/composables/useApi'

const apiCompat = useApiCompat();

const uploadProductWithImages = async (productData, imageFiles) => {
  const formData = new FormData();
  
  // Add product data
  Object.keys(productData).forEach(key => {
    formData.append(key, productData[key]);
  });
  
  // Add multiple images
  if (imageFiles && imageFiles.length) {
    for (let i = 0; i < imageFiles.length; i++) {
      formData.append('images', imageFiles[i]);
    }
  }
  
  try {
    const result = await apiCompat.post('/api/products', formData);
    if (result.error) {
      throw new Error(result.error.message || 'Failed to upload product');
    }
    
    return result.data;
  } catch (err) {
    handleApiError(err, 'Failed to upload product');
    throw err;
  }
}
``` 