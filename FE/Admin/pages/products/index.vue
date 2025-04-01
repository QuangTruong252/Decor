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
            :src="row.imageUrl || '/images/placeholder.png'"
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
              <UFileInput
                v-model="formState.images"
                placeholder="Select images"
                accept="image/*"
                multiple
                :ui="{ base: 'w-full' }"
              />
              
              <div v-if="formState.imageUrls.length || imagePreview.length" class="mt-2 grid grid-cols-4 gap-2">
                <div v-for="(url, index) in formState.imageUrls" :key="`existing-${index}`" class="relative">
                  <img
                    :src="url"
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
                </div>
                
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
import type { CategoryDTO, ProductDTO, ProductImageDTO } from '~/api-services'
import { apiService, handleApiError, showSuccessToast, showErrorToast } from '~/api-services/api-service'
import { useApiCompat } from '~/composables/useApi'

// Define page meta
definePageMeta({
  middleware: ['auth']
})

const apiCompat = useApiCompat()

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
  imageUrls: string[];
  imageUrl: string;
  isActive: boolean;
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
  imageUrls: [],
  imageUrl: '',
  isActive: true
})

// Computed properties
const isEditing = computed(() => !!formState.id)

const categoryOptions = computed(() => [
  { id: undefined, name: 'All Categories' },
  ...categories.value
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
    imageUrls: [],
    imageUrl: '',
    isActive: true
  })
  imagePreview.value = []
  isModalOpen.value = true
}

const editProduct = (product: ProductDTO): void => {
  Object.assign(formState, {
    id: product.id,
    name: product.name || '',
    description: product.description || '',
    price: product.price || 0,
    stockQuantity: product.stockQuantity || 0,
    categoryId: product.categoryId || undefined,
    images: null,
    imageUrls: product.images?.map(img => img.imageUrl || '') || [],
    imageUrl: product.imageUrl || '',
    isActive: product.isActive ?? true
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
    
    // Thêm categoryId nếu có
    if (formState.categoryId !== undefined) {
      formData.append('categoryId', formState.categoryId.toString())
    }
    
    // Thêm trạng thái
    formData.append('isActive', formState.isActive.toString())

    // Thêm các hình ảnh đã tồn tại
    formState.imageUrls.forEach((url: string, index: number) => {
      formData.append(`existingImages[${index}]`, url)
    })

    // Thêm các file hình ảnh mới
    if (formState.images) {
      for (let i = 0; i < formState.images.length; i++) {
        const file = formState.images.item(i)
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
  formState.imageUrls.splice(index, 1)
}

const removeNewImage = (index: number): void => {
  if (!formState.images) return;
  
  // Tạo DataTransfer object mới
  const dt = new DataTransfer()
  
  // Thêm các file còn lại vào DataTransfer, bỏ qua file ở vị trí index
  for (let i = 0; i < formState.images.length; i++) {
    const file = formState.images.item(i)
    if (i !== index && file) {
      dt.items.add(file)
    }
  }
  
  // Cập nhật formState.images với FileList mới
  formState.images = dt.files
  
  // Cập nhật preview
  imagePreview.value.splice(index, 1)
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
  imagePreview.value = []
  
  if (newImages) {
    for (let i = 0; i < newImages.length; i++) {
      const file = newImages.item(i)
      if (file) {
        const reader = new FileReader()
        reader.onload = (e: ProgressEvent<FileReader>) => {
          if (e.target?.result) {
            imagePreview.value.push(e.target.result as string)
          }
        }
        reader.readAsDataURL(file)
      }
    }
  }
}, { deep: true })

// Fetch data on component mount
onMounted(() => {
  fetchCategories()
  fetchProducts()
})
</script> 