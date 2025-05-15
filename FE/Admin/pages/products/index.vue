<template>
  <div>
    <div class="flex items-center justify-end space-x-2">
      <USelect
        v-model="selectedCategory"
        :options="categoryOptions"
        placeholder="Filter by category"
        option-attribute="name"
        value-attribute="id"
        size="sm"
        class="w-48"
        :nullable="true"
      />
      <UButton
        color="primary"
        icon="i-heroicons-plus"
        @click="openAddModal"
      >
        Add Product
      </UButton>
    </div>

    <!-- Products Table -->
    <UCard class="mt-6">
      <UTable
        :columns="columns"
        :rows="filteredProducts"
        :loading="isLoading"
        :empty-state="{ icon: 'i-heroicons-cube', label: 'No products found' }"
        :sort="{ column: 'createdAt', direction: 'desc' }"
        @sort="sortTable"
      >
        <template #image-data="{ row }">
          <img
            :src="imageService.getImageUrl(row.images?.[0]) || '/images/placeholder.png'"
            :alt="row.name"
            class="h-12 w-12 object-cover rounded"
          />
        </template>

        <template #price-data="{ row }">
          {{ formatPrice(row.price) }}
        </template>

        <template #category-data="{ row }">
          {{ getCategoryName(row.categoryId) }}
        </template>

        <template #createdAt-data="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>

        <template #actions-data="{ row }">
          <div class="flex space-x-2">
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-pencil-square"
              size="xs"
              @click="editProduct(row)"
            />
            <UButton
              color="red"
              variant="ghost"
              icon="i-heroicons-trash"
              size="xs"
              @click="confirmDelete(row)"
            />
            <UButton
              :color="row.isActive ? 'yellow' : 'green'"
              variant="ghost"
              :icon="row.isActive ? 'i-heroicons-eye-slash' : 'i-heroicons-eye'"
              size="xs"
              @click="toggleStatus(row)"
            />
          </div>
        </template>
      </UTable>

      <!-- Pagination -->
      <div class="mt-4 flex items-center justify-between">
        <div class="text-sm text-gray-600 dark:text-gray-400">
          Showing {{ paginationInfo.from }} to {{ paginationInfo.to }} of {{ paginationInfo.total }} products
        </div>
        <UPagination
          v-model="currentPage"
          :page-count="pageCount"
          :total="paginationInfo.total"
          :ui="{ wrapper: 'flex items-center gap-1' }"
          @update:model-value="changePage"
        />
      </div>
    </UCard>

    <!-- Add/Edit Product Modal -->
    <UModal v-model="isModalOpen" :ui="{ width: 'max-w-3xl' }">
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium">{{ isEditing ? 'Edit Product' : 'Add New Product' }}</h3>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-x-mark"
              @click="isModalOpen = false"
            />
          </div>
        </template>

        <UForm :state="formState" class="space-y-4" @submit="saveProduct">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormGroup label="Name" name="name" required>
              <UInput
                v-model="formState.name"
                placeholder="Enter product name"
                required
              />
            </UFormGroup>

            <UFormGroup label="SKU" name="sku" required>
              <UInput
                v-model="formState.sku"
                placeholder="Enter product SKU"
                required
              />
            </UFormGroup>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormGroup label="Price" name="price" required>
              <UInput
                v-model.number="formState.price"
                type="number"
                min="0"
                step="0.01"
                placeholder="0.00"
                required
              />
            </UFormGroup>

            <UFormGroup label="Original Price" name="originalPrice">
              <UInput
                v-model.number="formState.originalPrice"
                type="number"
                min="0"
                step="0.01"
                placeholder="0.00"
              />
            </UFormGroup>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormGroup label="Category" name="categoryId" required>
              <USelect
                v-model="formState.categoryId"
                :options="categories"
                placeholder="Select category"
                option-attribute="name"
                value-attribute="id"
                required
                :nullable="true"
              />
            </UFormGroup>

            <UFormGroup label="Stock" name="stockQuantity">
              <UInput
                v-model.number="formState.stockQuantity"
                type="number"
                min="0"
                step="1"
                placeholder="0"
              />
            </UFormGroup>
          </div>

          <UFormGroup label="Description" name="description">
            <UTextarea
              v-model="formState.description"
              placeholder="Enter product description"
              :rows="3"
            />
          </UFormGroup>

          <UFormGroup label="Product Images" name="images">
            <div class="space-y-2">
              <div class="flex items-center space-x-2">
                <div class="relative flex-grow" ref="fileInputRef">
                  <input type="file" multiple @change="handleFileChange" class="hidden" />
                </div>
                <UButton
                  color="primary"
                  icon="i-heroicons-photo"
                  @click="triggerFileInput"
                  title="Upload Images"
                >
                  Choose Images
                </UButton>
              </div>

              <div class="mt-2">
                <p class="text-sm text-gray-500 mb-2">
                  {{ isEditing ? 'Current and new images will be displayed below. You can remove images or set a default image.' : 'New images will be displayed below.' }}
                </p>

                <!-- Existing images section -->
                <div v-if="formState.existingImages.length > 0" class="mb-4">
                  <h4 class="text-sm font-medium mb-2">Current Images</h4>
                  <div class="grid grid-cols-4 gap-2">
                    <div v-for="(imageUrl, index) in formState.existingImages" :key="`existing-${index}`" class="relative">
                      <img
                        :src="imageService.getImageUrl(imageUrl)"
                        alt="Product image"
                        class="h-24 w-full object-cover rounded border"
                      />
                      <UButton
                        color="red"
                        variant="solid"
                        icon="i-heroicons-x-mark"
                        size="xs"
                        class="absolute top-1 right-1"
                        @click="removeExistingImage(index)"
                      />
                      <UButton
                        v-if="formState.defaultImageIndex !== index"
                        color="green"
                        variant="solid"
                        icon="i-heroicons-star"
                        size="xs"
                        class="absolute top-1 left-1"
                        @click="setDefaultImage(index)"
                        title="Set as default image"
                      />
                      <div
                        v-if="formState.defaultImageIndex === index"
                        class="absolute top-1 left-1 bg-green-500 text-white text-xs px-1 rounded"
                        title="Default image"
                      >
                        Default
                      </div>
                    </div>
                  </div>
                </div>

                <!-- New images preview section -->
                <div v-if="imagePreview.length > 0" class="mb-2">
                  <h4 class="text-sm font-medium mb-2">New Images</h4>
                  <div class="grid grid-cols-4 gap-2">
                    <div v-for="(preview, index) in imagePreview" :key="`preview-${index}`" class="relative">
                      <img
                        :src="preview"
                        alt="Image preview"
                        class="h-24 w-full object-cover rounded border"
                      />
                      <UButton
                        color="red"
                        variant="solid"
                        icon="i-heroicons-x-mark"
                        size="xs"
                        class="absolute top-1 right-1"
                        @click="removeNewImage(index)"
                      />
                    </div>
                  </div>
                </div>

                <div v-if="!formState.existingImages.length && !imagePreview.length" class="text-center p-4 border border-dashed rounded">
                  <p class="text-gray-500">No images selected</p>
                </div>
              </div>
            </div>
          </UFormGroup>

          <UFormGroup name="isActive">
            <UCheckbox
              v-model="formState.isActive"
              label="Active"
              name="isActive"
            />
          </UFormGroup>

          <div class="flex justify-end space-x-2">
            <UButton
              color="gray"
              variant="outline"
              @click="isModalOpen = false"
            >
              Cancel
            </UButton>
            <UButton
              type="submit"
              color="primary"
              :loading="isSaving"
              :disabled="isSaving"
            >
              {{ isEditing ? 'Update' : 'Create' }}
            </UButton>
          </div>
        </UForm>
      </UCard>
    </UModal>

    <!-- Delete Confirmation Modal -->
    <UModal v-model="isDeleteModalOpen">
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium">Confirm Delete</h3>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-x-mark"
              @click="isDeleteModalOpen = false"
            />
          </div>
        </template>

        <p>Are you sure you want to delete the product "{{ selectedProduct?.name }}"?</p>
        <p class="mt-2 text-sm text-red-600">This action cannot be undone.</p>

        <template #footer>
          <div class="flex justify-end space-x-2">
            <UButton
              color="gray"
              variant="outline"
              @click="isDeleteModalOpen = false"
            >
              Cancel
            </UButton>
            <UButton
              color="red"
              :loading="isDeleting"
              :disabled="isDeleting"
              @click="deleteProduct"
            >
              Delete
            </UButton>
          </div>
        </template>
      </UCard>
    </UModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import type { CategoryDTO, ProductDTO } from '~/api-services'
import { apiService, handleApiError, showSuccessToast, showErrorToast } from '~/api-services/api-service'
import { useApi } from '~/composables/useApi'
import { useImage } from '~/composables/useImage'

// Define page meta
definePageMeta({
  middleware: ['auth']
})

const apiCompat = useApi()
const imageService = useImage()

// Table columns
const columns = [
  { key: 'image', label: 'Image', sortable: false },
  { key: 'name', label: 'Name', sortable: true },
  { key: 'price', label: 'Price', sortable: true },
  { key: 'category', label: 'Category', sortable: true },
  { key: 'createdAt', label: 'Created At', sortable: true },
  { key: 'actions', label: 'Actions', sortable: false }
]

// Define interface for form state
interface ProductForm {
  id?: number;
  name: string;
  description: string;
  price: number;
  stock?: number;
  stockQuantity: number;
  category?: number | null;
  categoryId?: number | undefined;
  images: FileList | null;
  existingImages: string[];
  defaultImageIndex?: number;
  isActive: boolean;
  slug?: string;
  sku?: string;
  originalPrice?: number;
  isFeatured?: boolean;
}

// State
const products = ref<ProductDTO[]>([])
const categories = ref<CategoryDTO[]>([])
const isLoading = ref(true)
const isModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const isSaving = ref(false)
const isDeleting = ref(false)
const selectedProduct = ref<ProductDTO | null>(null)
const selectedCategory = ref<number | undefined>(undefined)
const imagePreview = ref<string[]>([])
const sortBy = ref({ column: 'createdAt', direction: 'desc' })
const currentPage = ref(1)
const perPage = ref(10)

// Form state
const formState = reactive<ProductForm>({
  id: undefined,
  name: '',
  description: '',
  price: 0,
  stock: 0,
  stockQuantity: 0,
  category: null,
  categoryId: undefined,
  images: null,
  existingImages: [],
  defaultImageIndex: undefined,
  isActive: true,
  slug: '',
  sku: '',
  originalPrice: 0,
  isFeatured: false
})

// Computed properties
const isEditing = computed(() => !!formState.id)

const categoryOptions = computed(() => [
  { id: undefined, name: 'All Categories' },
  ...categories?.value
])

const filteredProducts = computed(() => {
  return selectedCategory.value
    ? products.value.filter(product => product.categoryId === selectedCategory.value)
    : products.value
})

const pageCount = computed(() => Math.ceil(filteredProducts.value.length / perPage.value))

const paginationInfo = computed(() => {
  const total = filteredProducts.value.length
  const from = total === 0 ? 0 : (currentPage.value - 1) * perPage.value + 1
  const to = Math.min(from + perPage.value - 1, total)
  return { from, to, total }
})

const paginatedProducts = computed(() => {
  const start = (currentPage.value - 1) * perPage.value
  return filteredProducts.value.slice(start, start + perPage.value)
})

// Methods
const fetchProducts = async (): Promise<void> => {
  isLoading.value = true
  try {
    const res = await apiService.productsApi.apiProductsGet()
    if (res.status === 200) {
      products.value = res.data
    } else {
      showErrorToast('Failed to load products')
    }
  } catch (err) {
    handleApiError(err, 'Failed to load products')
  } finally {
    isLoading.value = false
  }
}

const fetchCategories = async (): Promise<void> => {
  try {
    const res = await apiService.categoryApi.apiCategoryGet()
    if (res.status === 200) {
      categories.value = res.data || []
    } else {
      showErrorToast('Failed to load categories')
    }
  } catch (err) {
    handleApiError(err, 'Failed to load categories')
  }
}

const openAddModal = (): void => {
  Object.assign(formState, {
    id: undefined,
    name: '',
    description: '',
    price: 0,
    stock: 0,
    stockQuantity: 0,
    category: null,
    categoryId: undefined,
    images: null,
    existingImages: [],
    defaultImageIndex: undefined,
    isActive: true,
    slug: '',
    sku: '', // SKU sẽ được người dùng nhập, là trường bắt buộc
    originalPrice: 0,
    isFeatured: false
  })
  imagePreview.value = []
  isModalOpen.value = true
}

const editProduct = (product: ProductDTO): void => {
  // Lấy danh sách URL hình ảnh từ product.images
  const existingImages = product.images?.map(img => typeof img === 'string' ? img : img.imageUrl) || []

  // Tìm hình ảnh mặc định (nếu có)
  const defaultImageIndex = product.images?.findIndex(img =>
    typeof img === 'object' && img.isDefault === true
  )

  Object.assign(formState, {
    id: product.id,
    name: product.name || '',
    description: product.description || '',
    price: product.price || 0,
    stockQuantity: product.stockQuantity || 0,
    categoryId: product.categoryId || undefined,
    images: null,
    existingImages: existingImages.filter(Boolean) as string[],
    defaultImageIndex: defaultImageIndex !== -1 ? defaultImageIndex : undefined,
    isActive: product.isActive ?? true,
    slug: product.slug || '',
    sku: product.sku || '',
    originalPrice: product.originalPrice || 0,
    isFeatured: product.isFeatured || false
  })
  imagePreview.value = []
  isModalOpen.value = true
}

const saveProduct = async (): Promise<void> => {
  isSaving.value = true
  try {
    const formData = new FormData()

    // Thêm các trường cơ bản
    formData.append('name', formState.name || '')
    formData.append('description', formState.description || '')
    formData.append('price', (formState.price || 0).toString())
    formData.append('stockQuantity', (formState.stockQuantity || 0).toString())

    // Thêm các trường bổ sung
    if (formState.slug) {
      formData.append('slug', formState.slug)
    } else {
      // Tạo slug từ tên sản phẩm nếu không có
      formData.append('slug', formState.name.toLowerCase().replace(/\s+/g, '-'))
    }

    // SKU là trường bắt buộc
    formData.append('sku', formState.sku || '')

    if (formState.originalPrice !== undefined) {
      formData.append('originalPrice', formState.originalPrice.toString())
    }

    if (formState.isFeatured !== undefined) {
      formData.append('isFeatured', formState.isFeatured.toString())
    }

    // Thêm categoryId nếu có
    if (formState.categoryId !== undefined) {
      formData.append('categoryId', formState.categoryId.toString())
    }

    // Thêm trạng thái
    formData.append('isActive', formState.isActive.toString())

    // Thêm thông tin về hình ảnh đã tồn tại
    if (formState.existingImages.length > 0) {
      formState.existingImages.forEach((imageUrl, index) => {
        formData.append(`existingImages[${index}]`, imageUrl)

        // Đánh dấu hình ảnh mặc định
        if (formState.defaultImageIndex === index) {
          formData.append('defaultImageIndex', index.toString())
        }
      })
    }

    // Thêm các file hình ảnh mới
    if (formState.images) {
      for (let i = 0; i < formState.images.length; i++) {
        const file = formState.images[i]
        if (file) {
          formData.append('images', file)
        }
      }
    }

    let result
    if (isEditing.value && formState.id !== undefined) {
      formData.append('id', formState.id.toString())
      result = await apiCompat.put(`/api/products/${formState.id}`, formData)
    } else {
      result = await apiCompat.post('/api/products', formData)
    }

    if (result.error) {
      throw new Error(result.error.message || 'Failed to save product')
    }

    showSuccessToast(isEditing.value ? 'Product updated successfully' : 'Product created successfully')
    isModalOpen.value = false
    fetchProducts()
  } catch (err) {
    handleApiError(err, 'Failed to save product')
  } finally {
    isSaving.value = false
  }
}

const confirmDelete = (product: ProductDTO): void => {
  selectedProduct.value = product
  isDeleteModalOpen.value = true
}

const deleteProduct = async (): Promise<void> => {
  if (!selectedProduct.value?.id) return
  isDeleting.value = true
  try {
    const res = await apiService.productsApi.apiProductsIdDelete({ id: selectedProduct.value.id })
    if (res.status === 204 || res.status === 200) {
      showSuccessToast('Product deleted successfully')
      isDeleteModalOpen.value = false
      fetchProducts()
    } else {
      showErrorToast('Failed to delete product')
    }
  } catch (err) {
    handleApiError(err, 'Failed to delete product')
  } finally {
    isDeleting.value = false
  }
}

const toggleStatus = async (product: ProductDTO): Promise<void> => {
  try {
    const updatedProduct = {
      id: product.id,
      name: product.name || '',
      price: product.price || 0,
      isActive: !product.isActive
    }
    const res = await apiService.productsApi.apiProductsIdPut(updatedProduct)
    if (res.status === 204 || res.status === 200) {
      showSuccessToast(`Product ${product.isActive ? 'deactivated' : 'activated'} successfully`)
      fetchProducts()
    } else {
      showErrorToast('Failed to update product status')
    }
  } catch (err) {
    handleApiError(err, 'Failed to update product status')
  }
}

const removeExistingImage = (index: number): void => {
  // Nếu xóa hình ảnh mặc định, cần reset defaultImageIndex
  if (formState.defaultImageIndex === index) {
    formState.defaultImageIndex = undefined
  } else if (formState.defaultImageIndex !== undefined && formState.defaultImageIndex > index) {
    // Nếu xóa hình ảnh trước hình ảnh mặc định, cần giảm defaultImageIndex
    formState.defaultImageIndex--
  }

  formState.existingImages.splice(index, 1)
}

const setDefaultImage = (index: number): void => {
  formState.defaultImageIndex = index
}

const removeNewImage = (index: number): void => {
  if (!formState.images) return;

  const newArr = formState.images.filter((_, i) => i !== index);
  formState.images = newArr
  imagePreview.value = Array.from(newArr || []).map((file) => URL.createObjectURL(file));
  console.log(formState.images, imagePreview.value)
}

// Ref cho input file
const fileInputRef = ref<HTMLElement | null>(null)

// Hàm kích hoạt sự kiện click trên input file
const triggerFileInput = (): void => {
  // Sử dụng ref để truy cập input file
  if (fileInputRef.value) {
    // Tìm input trong component
    const inputElement = fileInputRef.value.querySelector('input[type="file"]')
    if (inputElement) {
      // Kích hoạt sự kiện click
      inputElement.click()
    }
  }
}

const handleFileChange = (event: Event): void => {
  const target = event.target as HTMLInputElement
  if (target.files) {
    const files = Array.from(target.files)
    const newImages = Array.from(formState.images || [])
    newImages.push(...files)
    formState.images = newImages
    imagePreview.value = Array.from(formState.images || []).map((file) => URL.createObjectURL(file))
  }
  target.value = ''
}

const sortTable = (column: string, direction: string): void => {
  sortBy.value = { column, direction }
}

const changePage = (page: number): void => {
  currentPage.value = page
}

// Helper functions
const formatDate = (dateString: string): string => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const isDefaultImage = (index: number): boolean => {
  return formState.defaultImageIndex === index
}

const formatPrice = (price: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(price || 0)
}

const getCategoryName = (id: number): string => {
  const category = categories.value.find(c => c.id === id)
  return category?.name || '-'
}

// Watch for image changes to create preview
watch(() => formState.images, (newImages) => {

}, { deep: true })

// Fetch data on component mount
onMounted(() => {
  fetchCategories()
  fetchProducts()
})
</script>