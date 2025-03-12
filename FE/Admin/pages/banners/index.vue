<template>
  <div>
    <div class="mb-6">
      <h1 class="text-2xl font-bold">Quản lý Banners</h1>
      <p class="text-gray-600">Thêm và quản lý banners hiển thị trên trang chủ</p>
    </div>

    <!-- Banners Grid -->
    <UCard class="relative overflow-hidden">
      <div class="mb-4 flex justify-between items-center">
        <UInput
          v-model="searchTerm"
          placeholder="Tìm kiếm theo tiêu đề"
          icon="i-heroicons-magnifying-glass"
          @update:model-value="loadBanners"
          class="w-full md:w-64"
        />
        <UButton color="primary" icon="i-heroicons-plus" @click="showBannerModal = true">
          Thêm banner mới
        </UButton>
      </div>
      
      <!-- Loading skeleton -->
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        <USkeleton v-for="i in 4" :key="i" class="h-60" />
      </div>
      
      <!-- Banner cards -->
      <div v-else-if="banners.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        <UCard
          v-for="banner in banners"
          :key="banner.id"
          class="group overflow-hidden"
          :class="{ 'border-gray-200': !banner.isActive, 'border-primary': banner.isActive }"
        >
          <div class="relative">
            <!-- Banner image -->
            <div class="aspect-video overflow-hidden bg-gray-100 rounded-md">
              <img
                :src="banner.imageUrl"
                :alt="banner.title"
                class="w-full h-full object-cover"
                :class="{ 'opacity-60': !banner.isActive }"
              />
            </div>
            
            <!-- Overlay with actions -->
            <div class="absolute top-2 right-2 flex gap-1">
              <UButton
                icon="i-heroicons-pencil-square"
                color="white"
                variant="solid"
                size="xs"
                @click="editBanner(banner)"
              />
              <UButton
                icon="i-heroicons-trash"
                color="white"
                variant="solid"
                size="xs"
                @click="confirmDelete(banner)"
              />
            </div>
            
            <!-- Active/Inactive badge -->
            <UBadge
              :color="banner.isActive ? 'green' : 'gray'"
              size="sm"
              variant="subtle"
              class="absolute top-2 left-2"
            >
              {{ banner.isActive ? 'Đang hiển thị' : 'Đã ẩn' }}
            </UBadge>
          </div>
          
          <div class="mt-3">
            <div class="flex justify-between items-center">
              <h3 class="font-medium line-clamp-1">{{ banner.title }}</h3>
              <UToggle
                v-model="banner.isActive"
                @change="toggleBannerStatus(banner)"
                color="primary"
                :disabled="savingId === banner.id"
              />
            </div>
            <p v-if="banner.link" class="text-xs text-gray-500 truncate mt-1">
              Link: {{ banner.link }}
            </p>
            <p class="text-xs text-gray-500 mt-1">
              Thứ tự: {{ banner.displayOrder || 0 }}
            </p>
          </div>
        </UCard>
      </div>
      
      <!-- Empty state -->
      <div v-else class="text-center py-12">
        <UIcon name="i-heroicons-photo" class="text-gray-400 text-6xl mx-auto mb-4" />
        <h3 class="text-lg font-medium text-gray-900">Không có banner nào</h3>
        <p class="text-gray-500 mb-6">Chưa có banner nào được tạo. Hãy bắt đầu thêm banner mới.</p>
        <UButton color="primary" icon="i-heroicons-plus" @click="showBannerModal = true">
          Thêm banner mới
        </UButton>
      </div>
      
      <!-- Pagination -->
      <div v-if="banners.length > 0" class="mt-6 flex justify-center">
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
    
    <!-- Banner Modal (Add/Edit) -->
    <UModal v-model="showBannerModal">
      <UCard>
        <template #header>
          <div class="flex justify-between items-center">
            <h3 class="text-lg font-semibold">{{ isEditing ? 'Chỉnh sửa banner' : 'Thêm banner mới' }}</h3>
            <UButton icon="i-heroicons-x-mark" color="gray" variant="ghost" @click="showBannerModal = false" />
          </div>
        </template>
        
        <div class="space-y-4">
          <UForm :state="bannerForm" class="space-y-4" @submit="saveBanner">
            <!-- Title -->
            <UFormGroup label="Tiêu đề" name="title">
              <UInput v-model="bannerForm.title" placeholder="Nhập tiêu đề banner" />
            </UFormGroup>
            
            <!-- Link -->
            <UFormGroup label="Link" name="link">
              <UInput v-model="bannerForm.link" placeholder="Nhập URL khi click vào banner" />
            </UFormGroup>
            
            <!-- Display order -->
            <UFormGroup label="Thứ tự hiển thị" name="displayOrder">
              <UInput
                v-model="bannerForm.displayOrder"
                type="number"
                placeholder="Nhập thứ tự hiển thị"
                min="0"
                step="1"
              />
            </UFormGroup>
            
            <!-- Active status -->
            <UFormGroup label="Trạng thái" name="isActive">
              <div class="flex items-center">
                <UToggle v-model="bannerForm.isActive" color="primary" />
                <span class="ml-2 text-sm text-gray-500">
                  {{ bannerForm.isActive ? 'Hiển thị trên trang chủ' : 'Ẩn banner' }}
                </span>
              </div>
            </UFormGroup>
            
            <!-- Image -->
            <UFormGroup label="Hình ảnh" name="imageFile">
              <div class="space-y-2">
                <!-- Current image (for editing) -->
                <div v-if="isEditing && bannerForm.imageUrl" class="mb-4">
                  <img
                    :src="bannerForm.imageUrl"
                    alt="Banner image"
                    class="h-40 object-contain rounded border"
                  />
                </div>
                
                <!-- File uploader -->
                <UFormGroup>
                  <UUpload
                    v-model="bannerForm.imageFile"
                    :max-size="5242880"
                    accept="image/*"
                    :placeholder="isEditing ? 'Chọn ảnh mới (nếu muốn thay đổi)' : 'Chọn ảnh banner'"
                  />
                </UFormGroup>
              </div>
            </UFormGroup>
            
            <div class="flex justify-end space-x-2 pt-4">
              <UButton
                type="button"
                color="gray"
                variant="soft"
                @click="showBannerModal = false"
              >
                Hủy
              </UButton>
              <UButton
                type="submit"
                color="primary"
                :loading="saving"
              >
                {{ isEditing ? 'Cập nhật' : 'Thêm mới' }}
              </UButton>
            </div>
          </UForm>
        </div>
      </UCard>
    </UModal>
    
    <!-- Delete confirmation modal -->
    <UModal v-model="showDeleteModal">
      <div class="p-4">
        <div class="flex items-center gap-4 mb-4">
          <UIcon name="i-heroicons-exclamation-triangle" class="text-red-500 text-xl" />
          <h3 class="text-lg font-medium">Xác nhận xóa banner</h3>
        </div>
        
        <div v-if="selectedBanner" class="mb-4">
          <p>Bạn có chắc chắn muốn xóa banner <strong>{{ selectedBanner.title }}</strong>?</p>
        </div>
        
        <div class="flex justify-end gap-2">
          <UButton color="gray" variant="soft" @click="showDeleteModal = false">
            Hủy
          </UButton>
          <UButton color="red" :loading="saving" @click="deleteBanner">
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
  middleware: ['auth']
})

// State
const banners = ref([])
const loading = ref(true)
const saving = ref(false)
const savingId = ref(null)
const selectedBanner = ref(null)

// Pagination
const currentPage = ref(1)
const pageSize = ref(12) // 3x4 grid for desktop
const pageCount = ref(1)
const totalItems = ref(0)
const searchTerm = ref('')

// Modals visibility
const showBannerModal = ref(false)
const showDeleteModal = ref(false)
const isEditing = ref(false)

// Banner form
const bannerForm = reactive({
  id: 0,
  title: '',
  link: '',
  displayOrder: 0,
  isActive: true,
  imageUrl: '',
  imageFile: null
})

// Composables
const toast = useToast()
const api = useApi()
// Reset form
const resetForm = () => {
  Object.assign(bannerForm, {
    id: 0,
    title: '',
    link: '',
    displayOrder: 0,
    isActive: true,
    imageUrl: '',
    imageFile: null
  })
}

// Load banners
const loadBanners = async () => {
  try {
    loading.value = true
    
    const params = {
      page: currentPage.value,
      pageSize: pageSize.value,
      searchTerm: searchTerm.value
    }
    
    // Call API
    const response = await api.get('/api/Banner', {
      method: 'GET',
      // params
    })
    
    banners.value = response.items || []
    totalItems.value = response.totalItems || 0
    pageCount.value = response.pageCount || 1
    
  } catch (error) {
    toast.add({
      title: 'Error',
      description: error.message || 'Failed to load banners',
      color: 'red'
    })
    console.error(error)
  } finally {
    loading.value = false
  }
}

// Handle pagination
const handlePageChange = (page) => {
  currentPage.value = page
  loadBanners()
}

// Edit banner
const editBanner = (banner) => {
  isEditing.value = true
  selectedBanner.value = banner
  
  Object.assign(bannerForm, {
    id: banner.id,
    title: banner.title,
    link: banner.link,
    displayOrder: banner.displayOrder || 0,
    isActive: banner.isActive,
    imageUrl: banner.imageUrl,
    imageFile: null
  })
  
  showBannerModal.value = true
}

// Confirm delete banner
const confirmDelete = (banner) => {
  selectedBanner.value = banner
  showDeleteModal.value = true
}

// Toggle banner active status
const toggleBannerStatus = async (banner) => {
  try {
    savingId.value = banner.id
    
    // Call API
    await api.patch(`/api/Banner/${banner.id}/status`, {
      method: 'PATCH',
      body: { isActive: banner.isActive }
    })
    
    toast.success(`Banner ${banner.isActive ? 'đã được hiển thị' : 'đã bị ẩn'}`)
  } catch (error) {
    // Revert toggle on error
    banner.isActive = !banner.isActive
    toast.add({
      title: 'Error',
      description: error.message || 'Failed to update banner status',
      color: 'red'
    })
    console.error(error)
  } finally {
    savingId.value = null
  }
}

// Save banner (add or update)
const saveBanner = async () => {
  try {
    saving.value = true
    
    // Validate form
    if (!bannerForm.title) {
      toast.add({
        title: 'Error',
        description: 'Please enter a banner title',
        color: 'red'
      })
      saving.value = false
      return
    }
    
    // Require image file for new banners
    if (!isEditing.value && !bannerForm.imageFile) {
      toast.add({
        title: 'Error',
        description: 'Please select a banner image',
        color: 'red'
      })
      saving.value = false
      return
    }
    
    // Create form data for file upload
    const formData = new FormData()
    formData.append('title', bannerForm.title)
    formData.append('link', bannerForm.link)
    formData.append('displayOrder', bannerForm.displayOrder.toString())
    formData.append('isActive', bannerForm.isActive.toString())
    
    if (bannerForm.imageFile) {
      formData.append('image', bannerForm.imageFile)
    }
    
    if (isEditing.value) {
      // Update banner
      await api.put(`/api/Banner/${bannerForm.id}`, {
        method: 'PUT',
        body: formData
      })
      
      toast.success('Cập nhật banner thành công')
    } else {
      // Create new banner
      await api.post('/api/Banner', {
        method: 'POST',
        body: formData
      })
      
      toast.success('Thêm banner mới thành công')
    }
    
    // Reload banners
    await loadBanners()
    
    // Close modal and reset form
    showBannerModal.value = false
    resetForm()
    isEditing.value = false
    
  } catch (error) {
    toast.add({
      title: 'Error',
      description: error.message || 'Failed to save banner',
      color: 'red'
    })
    console.error(error)
  } finally {
    saving.value = false
  }
}

// Delete banner
const deleteBanner = async () => {
  if (!selectedBanner.value) return
  
  try {
    saving.value = true
    
    // Call API
    await api.delete(`/api/Banner/${selectedBanner.value.id}`, {
      method: 'DELETE'
    })
    
    toast.success('Xóa banner thành công')
    
    // Reload banners
    await loadBanners()
    
    // Close modal
    showDeleteModal.value = false
    selectedBanner.value = null
    
  } catch (error) {
    toast.add({
      title: 'Error',
      description: error.message || 'Failed to delete banner',
      color: 'red'
    })
    console.error(error)
  } finally {
    saving.value = false
  }
}

// Load initial data
onMounted(() => {
  loadBanners()
})
</script> 