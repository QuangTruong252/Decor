<template>
  <div>
    <UPageHeader title="Banner Management" description="Manage your website banners">
      <template #right>
        <UButton
          color="primary"
          icon="i-heroicons-plus"
          @click="openAddModal"
        >
          Add Banner
        </UButton>
      </template>
    </UPageHeader>
    
    <!-- Banners Table -->
    <UCard class="mt-6">
      <UTable
        :columns="columns"
        :rows="banners"
        :loading="isLoading"
        :empty-state="{ icon: 'i-heroicons-photo', label: 'No banners found' }"
      >
        <template #image-data="{ row }">
          <img
            :src="row.imageUrl"
            :alt="row.title"
            class="h-16 w-24 object-cover rounded"
          />
        </template>
        
        <template #isActive-data="{ row }">
          <UBadge :color="row.isActive ? 'green' : 'gray'" size="sm">
            {{ row.isActive ? 'Active' : 'Inactive' }}
          </UBadge>
        </template>
        
        <template #actions-data="{ row }">
          <div class="flex space-x-2">
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-pencil-square"
              size="xs"
              @click="editBanner(row)"
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
    </UCard>
    
    <!-- Add/Edit Banner Modal -->
    <UModal v-model="isModalOpen" :ui="{ width: 'max-w-2xl' }">
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium">{{ isEditing ? 'Edit Banner' : 'Add New Banner' }}</h3>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-x-mark"
              @click="isModalOpen = false"
            />
          </div>
        </template>
        
        <UForm :state="formState" class="space-y-4" @submit="saveBanner">
          <UFormGroup label="Title" name="title">
            <UInput
              v-model="formState.title"
              placeholder="Enter banner title"
              required
            />
          </UFormGroup>
          
          <UFormGroup label="Link URL" name="link">
            <UInput
              v-model="formState.link"
              placeholder="https://example.com/page"
              required
            />
          </UFormGroup>
          
          <UFormGroup label="Banner Image" name="image">
            <div class="space-y-2">
              <UFileInput
                v-model="formState.image"
                placeholder="Select an image"
                accept="image/*"
                :ui="{ base: 'w-full' }"
              />
              
              <div v-if="formState.imageUrl || imagePreview" class="mt-2">
                <img
                  :src="imagePreview || formState.imageUrl"
                  alt="Banner preview"
                  class="h-40 object-contain rounded border"
                />
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
        
        <p>Are you sure you want to delete this banner? This action cannot be undone.</p>
        
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
              @click="deleteBanner"
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
    label: 'Image'
  },
  {
    key: 'title',
    label: 'Title'
  },
  {
    key: 'link',
    label: 'Link'
  },
  {
    key: 'isActive',
    label: 'Status'
  },
  {
    key: 'actions',
    label: 'Actions',
    sortable: false
  }
]

// State
const banners = ref([])
const isLoading = ref(true)
const isModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const isSaving = ref(false)
const isDeleting = ref(false)
const selectedBanner = ref(null)
const imagePreview = ref(null)

// Form state
const formState = reactive({
  id: null,
  title: '',
  link: '',
  image: null,
  imageUrl: '',
  isActive: true
})

// Computed
const isEditing = computed(() => !!formState.id)

// Methods
const fetchBanners = async () => {
  isLoading.value = true
  
  try {
    const { data, error } = await api.get('/api/banners')
    
    if (error) {
      toast.add({
        title: 'Error',
        description: 'Failed to load banners',
        color: 'red'
      })
      return
    }
    
    banners.value = data || []
  } catch (err) {
    console.error('Error fetching banners:', err)
    toast.add({
      title: 'Error',
      description: 'Failed to load banners',
      color: 'red'
    })
  } finally {
    isLoading.value = false
  }
}

const openAddModal = () => {
  // Reset form
  Object.assign(formState, {
    id: null,
    title: '',
    link: '',
    image: null,
    imageUrl: '',
    isActive: true
  })
  
  imagePreview.value = null
  isModalOpen.value = true
}

const editBanner = (banner) => {
  // Populate form with banner data
  Object.assign(formState, {
    id: banner.id,
    title: banner.title,
    link: banner.link,
    image: null,
    imageUrl: banner.imageUrl,
    isActive: banner.isActive
  })
  
  imagePreview.value = null
  isModalOpen.value = true
}

const saveBanner = async () => {
  isSaving.value = true
  
  try {
    // Create FormData for file upload
    const formData = new FormData()
    formData.append('title', formState.title)
    formData.append('link', formState.link)
    formData.append('isActive', formState.isActive)
    
    if (formState.image) {
      formData.append('image', formState.image)
    }
    
    let result
    
    if (isEditing.value) {
      // Update existing banner
      formData.append('id', formState.id)
      result = await api.put(`/api/banners/${formState.id}`, formData)
    } else {
      // Create new banner
      result = await api.post('/api/banners', formData)
    }
    
    if (result.error) {
      throw new Error(result.error.message || 'Failed to save banner')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: isEditing.value ? 'Banner updated successfully' : 'Banner created successfully',
      color: 'green'
    })
    
    isModalOpen.value = false
    fetchBanners()
  } catch (err) {
    console.error('Error saving banner:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to save banner',
      color: 'red'
    })
  } finally {
    isSaving.value = false
  }
}

const confirmDelete = (banner) => {
  selectedBanner.value = banner
  isDeleteModalOpen.value = true
}

const deleteBanner = async () => {
  if (!selectedBanner.value) return
  
  isDeleting.value = true
  
  try {
    const { error } = await api.delete(`/api/banners/${selectedBanner.value.id}`)
    
    if (error) {
      throw new Error(error.message || 'Failed to delete banner')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: 'Banner deleted successfully',
      color: 'green'
    })
    
    isDeleteModalOpen.value = false
    fetchBanners()
  } catch (err) {
    console.error('Error deleting banner:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to delete banner',
      color: 'red'
    })
  } finally {
    isDeleting.value = false
  }
}

const toggleStatus = async (banner) => {
  try {
    const { error } = await api.put(`/api/banners/${banner.id}/toggle-status`, {
      isActive: !banner.isActive
    })
    
    if (error) {
      throw new Error(error.message || 'Failed to update banner status')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: `Banner ${banner.isActive ? 'deactivated' : 'activated'} successfully`,
      color: 'green'
    })
    
    fetchBanners()
  } catch (err) {
    console.error('Error toggling banner status:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to update banner status',
      color: 'red'
    })
  }
}

// Watch for image changes to create preview
watch(() => formState.image, (newImage) => {
  if (newImage) {
    const reader = new FileReader()
    reader.onload = (e) => {
      imagePreview.value = e.target.result
    }
    reader.readAsDataURL(newImage)
  } else {
    imagePreview.value = null
  }
})

// Fetch banners on component mount
onMounted(() => {
  fetchBanners()
})
</script> 