<template>
  <div>
    <UPageHeader title="Product Management" description="Manage your store products">
      <template #right>
        <div class="flex items-center space-x-2">
          <USelect
            v-model="selectedCategory"
            :options="categoryOptions"
            placeholder="Filter by category"
            option-attribute="name"
            value-attribute="id"
            size="sm"
            class="w-48"
          />
          <UButton
            color="primary"
            icon="i-heroicons-plus"
            @click="openAddModal"
          >
            Add Product
          </UButton>
        </div>
      </template>
    </UPageHeader>
    
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
                v-model="formState.price"
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
              />
            </UFormGroup>
            
            <UFormGroup label="Stock" name="stock">
              <UInput
                v-model="formState.stock"
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
              rows="3"
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

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useApi } from '~/composables/useApi'
import { useToast } from '@nuxt/ui'

// Define page meta
definePageMeta({
  middleware: ['auth']
})

const api = useApi()
const toast = useToast()

// Table columns
const columns = [
  {
    key: 'image',
    label: 'Image',
    sortable: false
  },
  {
    key: 'name',
    label: 'Name',
    sortable: true
  },
  {
    key: 'price',
    label: 'Price',
    sortable: true
  },
  {
    key: 'category',
    label: 'Category',
    sortable: true
  },
  {
    key: 'createdAt',
    label: 'Created At',
    sortable: true
  },
  {
    key: 'actions',
    label: 'Actions',
    sortable: false
  }
]

// State
const products = ref([])
const categories = ref([])
const isLoading = ref(true)
const isModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const isSaving = ref(false)
const isDeleting = ref(false)
const selectedProduct = ref(null)
const selectedCategory = ref(null)
const imagePreview = ref([])
const sortBy = ref({ column: 'createdAt', direction: 'desc' })
const currentPage = ref(1)
const perPage = ref(10)

// Form state
const formState = reactive({
  id: null,
  name: '',
  description: '',
  price: 0,
  stock: 0,
  categoryId: null,
  images: [],
  imageUrls: [],
  isActive: true
})

// Computed
const isEditing = computed(() => !!formState.id)

const categoryOptions = computed(() => {
  return [{ id: null, name: 'All Categories' }, ...categories.value]
})

const filteredProducts = computed(() => {
  let filtered = products.value
  
  // Filter by category if selected
  if (selectedCategory.value) {
    filtered = filtered.filter(product => product.categoryId === selectedCategory.value)
  }
  
  return filtered
})

const pageCount = computed(() => {
  return Math.ceil(filteredProducts.value.length / perPage.value)
})

const paginationInfo = computed(() => {
  const total = filteredProducts.value.length
  const from = total === 0 ? 0 : (currentPage.value - 1) * perPage.value + 1
  const to = Math.min(from + perPage.value - 1, total)
  
  return { from, to, total }
})

const paginatedProducts = computed(() => {
  const start = (currentPage.value - 1) * perPage.value
  const end = start + perPage.value
  
  return filteredProducts.value.slice(start, end)
})

// Methods
const fetchProducts = async () => {
  isLoading.value = true
  
  try {
    const { data, error } = await api.get('/api/products')
    
    if (error) {
      toast.add({
        title: 'Error',
        description: 'Failed to load products',
        color: 'red'
      })
      return
    }
    
    products.value = data || []
  } catch (err) {
    console.error('Error fetching products:', err)
    toast.add({
      title: 'Error',
      description: 'Failed to load products',
      color: 'red'
    })
  } finally {
    isLoading.value = false
  }
}

const fetchCategories = async () => {
  try {
    const { data, error } = await api.get('/api/categories')
    
    if (error) {
      toast.add({
        title: 'Error',
        description: 'Failed to load categories',
        color: 'red'
      })
      return
    }
    
    categories.value = data || []
  } catch (err) {
    console.error('Error fetching categories:', err)
    toast.add({
      title: 'Error',
      description: 'Failed to load categories',
      color: 'red'
    })
  }
}

const openAddModal = () => {
  // Reset form
  Object.assign(formState, {
    id: null,
    name: '',
    description: '',
    price: 0,
    stock: 0,
    categoryId: null,
    images: [],
    imageUrls: [],
    isActive: true
  })
  
  imagePreview.value = []
  isModalOpen.value = true
}

const editProduct = (product) => {
  // Populate form with product data
  Object.assign(formState, {
    id: product.id,
    name: product.name,
    description: product.description || '',
    price: product.price,
    stock: product.stock || 0,
    categoryId: product.categoryId,
    images: [],
    imageUrls: product.images || [],
    isActive: product.isActive
  })
  
  imagePreview.value = []
  isModalOpen.value = true
}

const saveProduct = async () => {
  isSaving.value = true
  
  try {
    // Create FormData for file upload
    const formData = new FormData()
    formData.append('name', formState.name)
    formData.append('description', formState.description)
    formData.append('price', formState.price)
    formData.append('stock', formState.stock)
    formData.append('categoryId', formState.categoryId)
    formData.append('isActive', formState.isActive)
    
    // Add existing image URLs
    formState.imageUrls.forEach((url, index) => {
      formData.append(`existingImages[${index}]`, url)
    })
    
    // Add new images
    if (formState.images && formState.images.length) {
      for (let i = 0; i < formState.images.length; i++) {
        formData.append('images', formState.images[i])
      }
    }
    
    let result
    
    if (isEditing.value) {
      // Update existing product
      formData.append('id', formState.id)
      result = await api.put(`/api/products/${formState.id}`, formData)
    } else {
      // Create new product
      result = await api.post('/api/products', formData)
    }
    
    if (result.error) {
      throw new Error(result.error.message || 'Failed to save product')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: isEditing.value ? 'Product updated successfully' : 'Product created successfully',
      color: 'green'
    })
    
    isModalOpen.value = false
    fetchProducts()
  } catch (err) {
    console.error('Error saving product:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to save product',
      color: 'red'
    })
  } finally {
    isSaving.value = false
  }
}

const confirmDelete = (product) => {
  selectedProduct.value = product
  isDeleteModalOpen.value = true
}

const deleteProduct = async () => {
  if (!selectedProduct.value) return
  
  isDeleting.value = true
  
  try {
    const { error } = await api.delete(`/api/products/${selectedProduct.value.id}`)
    
    if (error) {
      throw new Error(error.message || 'Failed to delete product')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: 'Product deleted successfully',
      color: 'green'
    })
    
    isDeleteModalOpen.value = false
    fetchProducts()
  } catch (err) {
    console.error('Error deleting product:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to delete product',
      color: 'red'
    })
  } finally {
    isDeleting.value = false
  }
}

const toggleStatus = async (product) => {
  try {
    const { error } = await api.put(`/api/products/${product.id}/toggle-status`, {
      isActive: !product.isActive
    })
    
    if (error) {
      throw new Error(error.message || 'Failed to update product status')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: `Product ${product.isActive ? 'deactivated' : 'activated'} successfully`,
      color: 'green'
    })
    
    fetchProducts()
  } catch (err) {
    console.error('Error toggling product status:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to update product status',
      color: 'red'
    })
  }
}

const removeExistingImage = (index) => {
  formState.imageUrls.splice(index, 1)
}

const removeNewImage = (index) => {
  // Create a new FileList without the removed image
  const dt = new DataTransfer()
  const files = formState.images
  
  for (let i = 0; i < files.length; i++) {
    if (i !== index) {
      dt.items.add(files[i])
    }
  }
  
  formState.images = dt.files
  imagePreview.value.splice(index, 1)
}

const sortTable = (column, direction) => {
  sortBy.value = { column, direction }
}

const changePage = (page) => {
  currentPage.value = page
}

// Helper functions
const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const formatPrice = (price) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(price)
}

const getCategoryName = (categoryId) => {
  const category = categories.value.find(c => c.id === categoryId)
  return category ? category.name : 'Uncategorized'
}

// Watch for image changes to create preview
watch(() => formState.images, (newImages) => {
  if (newImages && newImages.length) {
    imagePreview.value = []
    
    for (let i = 0; i < newImages.length; i++) {
      const reader = new FileReader()
      reader.onload = (e) => {
        imagePreview.value.push(e.target.result)
      }
      reader.readAsDataURL(newImages[i])
    }
  }
}, { deep: true })

// Fetch data on component mount
onMounted(() => {
  fetchCategories()
  fetchProducts()
})
</script> 