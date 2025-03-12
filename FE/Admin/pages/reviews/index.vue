<template>
  <div>
    <div class="mb-6">
      <h1 class="text-2xl font-bold">Quản lý đánh giá</h1>
      <p class="text-gray-600">Xem và quản lý các đánh giá sản phẩm từ khách hàng</p>
    </div>
    
    <!-- Filters -->
    <UCard class="mb-6">
      <div class="flex flex-col md:flex-row gap-4">
        <!-- Product filter -->
        <USelectMenu
          v-model="productFilter"
          :options="productOptions"
          placeholder="Lọc theo sản phẩm"
          searchable
          searchable-placeholder="Tìm kiếm sản phẩm"
          class="w-full md:w-64"
          option-attribute="label"
          @update:model-value="loadReviews"
        />
        
        <!-- Rating filter -->
        <USelectMenu
          v-model="ratingFilter"
          :options="ratingOptions"
          placeholder="Lọc theo đánh giá"
          class="w-full md:w-48"
          @update:model-value="loadReviews"
        />
        
        <!-- Reset filters -->
        <UButton
          v-if="isFiltered"
          color="gray"
          variant="ghost"
          icon="i-heroicons-x-mark"
          @click="resetFilters"
        >
          Xóa bộ lọc
        </UButton>
      </div>
    </UCard>
    
    <!-- Reviews table -->
    <UCard>
      <UTable 
        :columns="columns"
        :rows="reviews"
        :loading="loading"
        :empty-state="{ icon: 'i-heroicons-star', label: 'Không có đánh giá nào' }"
      >
        <!-- Product column -->
        <template #productName-data="{ row }">
          <div class="flex items-center">
            <div class="h-8 w-8 flex-shrink-0">
              <img 
                v-if="row.productImage" 
                :src="row.productImage" 
                :alt="row.productName"
                class="h-8 w-8 object-cover rounded"
              />
              <div v-else class="h-8 w-8 rounded bg-gray-100 flex items-center justify-center">
                <UIcon name="i-heroicons-photo" class="text-gray-400" />
              </div>
            </div>
            <span class="ml-2 line-clamp-1">{{ row.productName }}</span>
          </div>
        </template>
        
        <!-- Rating column -->
        <template #rating-data="{ row }">
          <div class="flex">
            <UIcon
              v-for="i in 5"
              :key="i"
              :name="i <= row.rating ? 'i-heroicons-star-solid' : 'i-heroicons-star'"
              class="w-5 h-5"
              :class="i <= row.rating ? 'text-yellow-400' : 'text-gray-300'"
            />
          </div>
        </template>
        
        <!-- Comment column (truncated) -->
        <template #comment-data="{ row }">
          <span
            class="line-clamp-2"
            :title="row.comment"
          >
            {{ row.comment || 'Không có nội dung' }}
          </span>
        </template>
        
        <!-- CreatedAt column -->
        <template #createdAt-data="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>
        
        <!-- Actions column -->
        <template #actions-data="{ row }">
          <div class="flex items-center gap-2">
            <UButton
              icon="i-heroicons-eye"
              color="gray"
              variant="ghost"
              size="sm"
              @click="viewReview(row)"
            />
            <UButton
              icon="i-heroicons-trash"
              color="red"
              variant="ghost"
              size="sm"
              @click="confirmDelete(row)"
            />
          </div>
        </template>
      </UTable>
      
      <!-- Pagination -->
      <div class="mt-6 flex justify-center">
        <UPagination
          v-model="currentPage"
          :page-count="pageCount"
          :total="totalItems"
          :ui="{ wrapper: 'flex items-center gap-1' }"
          :page-size="pageSize"
          @update:model-value="handlePageChange"
        />
      </div>
    </UCard>
    
    <!-- Review detail modal -->
    <UModal v-model="showReviewModal">
      <UCard v-if="selectedReview">
        <template #header>
          <div class="flex justify-between items-center">
            <h3 class="text-lg font-semibold">Chi tiết đánh giá</h3>
            <UButton icon="i-heroicons-x-mark" color="gray" variant="ghost" @click="showReviewModal = false" />
          </div>
        </template>
        
        <div class="space-y-6">
          <!-- Product info -->
          <div class="flex items-center gap-4">
            <div
              class="w-16 h-16 bg-gray-100 rounded flex items-center justify-center shrink-0"
            >
              <img
                v-if="selectedReview.productImage"
                :src="selectedReview.productImage"
                :alt="selectedReview.productName"
                class="w-full h-full object-cover rounded"
              />
              <UIcon
                v-else
                name="i-heroicons-photo"
                class="text-gray-400 text-2xl"
              />
            </div>
            <div>
              <h4 class="font-medium">{{ selectedReview.productName }}</h4>
              <div class="flex mt-1">
                <UIcon
                  v-for="i in 5"
                  :key="i"
                  :name="i <= selectedReview.rating ? 'i-heroicons-star-solid' : 'i-heroicons-star'"
                  class="w-5 h-5"
                  :class="i <= selectedReview.rating ? 'text-yellow-400' : 'text-gray-300'"
                />
              </div>
            </div>
          </div>
          
          <!-- Review content -->
          <div>
            <h4 class="font-medium text-sm text-gray-500 mb-1">Nội dung đánh giá</h4>
            <p class="whitespace-pre-line">{{ selectedReview.comment || 'Không có nội dung' }}</p>
          </div>
          
          <!-- Review info -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <h4 class="font-medium text-sm text-gray-500 mb-1">Người đánh giá</h4>
              <p>{{ selectedReview.userName }}</p>
            </div>
            <div>
              <h4 class="font-medium text-sm text-gray-500 mb-1">Ngày đánh giá</h4>
              <p>{{ formatDate(selectedReview.createdAt) }}</p>
            </div>
          </div>
        </div>
        
        <template #footer>
          <div class="flex justify-end gap-2">
            <UButton
              color="red"
              @click="confirmDelete(selectedReview)"
            >
              Xóa đánh giá
            </UButton>
            <UButton
              color="gray"
              variant="soft"
              @click="showReviewModal = false"
            >
              Đóng
            </UButton>
          </div>
        </template>
      </UCard>
    </UModal>
    
    <!-- Delete confirmation modal -->
    <UModal v-model="showDeleteModal">
      <div class="p-4">
        <div class="flex items-center gap-4 mb-4">
          <UIcon name="i-heroicons-exclamation-triangle" class="text-red-500 text-xl" />
          <h3 class="text-lg font-medium">Xác nhận xóa đánh giá</h3>
        </div>
        
        <p class="mb-4">Bạn có chắc chắn muốn xóa đánh giá này? Hành động này không thể hoàn tác.</p>
        
        <div class="flex justify-end gap-2">
          <UButton color="gray" variant="soft" @click="showDeleteModal = false">
            Hủy
          </UButton>
          <UButton color="red" :loading="deleting" @click="deleteReview">
            Xóa
          </UButton>
        </div>
      </div>
    </UModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'


// Meta
definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

// State
const reviews = ref([])
const products = ref([])
const loading = ref(true)
const deleting = ref(false)
const selectedReview = ref(null)

// Table configuration
const columns = [
  { key: 'productName', label: 'Sản phẩm', sortable: true },
  { key: 'userName', label: 'Người đánh giá' },
  { key: 'rating', label: 'Đánh giá', sortable: true },
  { key: 'comment', label: 'Nội dung' },
  { key: 'createdAt', label: 'Ngày tạo', sortable: true },
  { key: 'actions', label: 'Thao tác' }
]

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)
const pageCount = ref(1)
const totalItems = ref(0)

// Filters
const productFilter = ref(null)
const ratingFilter = ref(null)

// Modals
const showReviewModal = ref(false)
const showDeleteModal = ref(false)

// Composables
const toast = useToast()

// Computed properties
const productOptions = computed(() => {
  return [
    { label: 'Tất cả sản phẩm', value: null },
    ...products.value.map(p => ({ label: p.name, value: p.id }))
  ]
})

const ratingOptions = computed(() => {
  return [
    { label: 'Tất cả đánh giá', value: null },
    { label: '5 sao', value: 5 },
    { label: '4 sao', value: 4 },
    { label: '3 sao', value: 3 },
    { label: '2 sao', value: 2 },
    { label: '1 sao', value: 1 }
  ]
})

const isFiltered = computed(() => {
  return productFilter.value !== null || ratingFilter.value !== null
})

// Format date
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

// Load reviews
const loadReviews = async () => {
  try {
    loading.value = true
    
    // Prepare query parameters
    const params = {
      page: currentPage.value,
      pageSize: pageSize.value,
      productId: productFilter.value,
      rating: ratingFilter.value
    }
    
    // Call API
    const response = await $fetch('/api/reviews', {
      method: 'GET',
      params
    })
    
    reviews.value = response.items || []
    totalItems.value = response.totalItems || 0
    pageCount.value = response.pageCount || 1
    
  } catch (error) {
    toast.error('Không thể tải danh sách đánh giá')
    console.error(error)
  } finally {
    loading.value = false
  }
}

// Load products for filter
const loadProducts = async () => {
  try {
    // Call API to get all products
    const response = await $fetch('/api/products', {
      method: 'GET',
      params: { pageSize: 100 } // Get a large number of products for the filter
    })
    
    products.value = response.items || []
  } catch (error) {
    toast.error('Không thể tải danh sách sản phẩm')
    console.error(error)
  }
}

// Reset filters
const resetFilters = () => {
  productFilter.value = null
  ratingFilter.value = null
  loadReviews()
}

// Handle page change
const handlePageChange = (page) => {
  currentPage.value = page
  loadReviews()
}

// View review details
const viewReview = (review) => {
  selectedReview.value = review
  showReviewModal.value = true
}

// Confirm delete
const confirmDelete = (review) => {
  selectedReview.value = review
  showDeleteModal.value = true
  showReviewModal.value = false
}

// Delete review
const deleteReview = async () => {
  if (!selectedReview.value) return
  
  try {
    deleting.value = true
    
    // Call API to delete review
    await $fetch(`/api/reviews/${selectedReview.value.id}`, {
      method: 'DELETE'
    })
    
    // Success message
    toast.success('Đánh giá đã được xóa thành công')
    
    // Close modal
    showDeleteModal.value = false
    
    // Reload reviews
    await loadReviews()
    
  } catch (error) {
    toast.error('Không thể xóa đánh giá')
    console.error(error)
  } finally {
    deleting.value = false
    selectedReview.value = null
  }
}

// Load initial data
onMounted(async () => {
  await Promise.all([loadReviews(), loadProducts()])
})
</script> 