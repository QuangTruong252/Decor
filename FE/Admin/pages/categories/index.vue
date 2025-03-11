<template>
  <div>
    <UPageHeader title="Category Management" description="Manage product categories">
      <template #right>
        <UButton
          color="primary"
          icon="i-heroicons-plus"
          @click="openAddModal"
        >
          Add Category
        </UButton>
      </template>
    </UPageHeader>
    
    <!-- Categories Table -->
    <UCard class="mt-6">
      <UTable
        :columns="columns"
        :rows="categories"
        :loading="isLoading"
        :empty-state="{ icon: 'i-heroicons-tag', label: 'No categories found' }"
        :sort="{ column: 'createdAt', direction: 'desc' }"
        @sort="sortTable"
      >
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
              @click="editCategory(row)"
            />
            <UButton
              color="red"
              variant="ghost"
              icon="i-heroicons-trash"
              size="xs"
              @click="confirmDelete(row)"
            />
          </div>
        </template>
      </UTable>
    </UCard>
    
    <!-- Add/Edit Category Modal -->
    <UModal v-model="isModalOpen">
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium">{{ isEditing ? 'Edit Category' : 'Add New Category' }}</h3>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-x-mark"
              @click="isModalOpen = false"
            />
          </div>
        </template>
        
        <UForm :state="formState" class="space-y-4" @submit="saveCategory">
          <UFormGroup label="Name" name="name" required>
            <UInput
              v-model="formState.name"
              placeholder="Enter category name"
              required
            />
          </UFormGroup>
          
          <UFormGroup label="Slug" name="slug">
            <UInput
              v-model="formState.slug"
              placeholder="category-slug"
              :disabled="isAutoSlug"
              :trailing="isAutoSlug ? 'Auto-generated' : ''"
            />
            <template #hint>
              <div class="flex items-center mt-1">
                <UCheckbox
                  v-model="isAutoSlug"
                  label="Auto-generate from name"
                  name="autoSlug"
                />
              </div>
            </template>
          </UFormGroup>
          
          <UFormGroup label="Description" name="description">
            <UTextarea
              v-model="formState.description"
              placeholder="Enter category description"
              rows="3"
            />
          </UFormGroup>
          
          <UFormGroup label="Parent Category" name="parentId">
            <USelect
              v-model="formState.parentId"
              :options="parentCategoryOptions"
              placeholder="Select parent category (optional)"
              option-attribute="name"
              value-attribute="id"
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
        
        <p>Are you sure you want to delete the category "{{ selectedCategory?.name }}"?</p>
        <p class="mt-2 text-sm text-red-600">This will also delete all products in this category.</p>
        
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
              @click="deleteCategory"
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
    key: 'name',
    label: 'Name',
    sortable: true
  },
  {
    key: 'slug',
    label: 'Slug',
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
const categories = ref([])
const isLoading = ref(true)
const isModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const isSaving = ref(false)
const isDeleting = ref(false)
const selectedCategory = ref(null)
const isAutoSlug = ref(true)
const sortBy = ref({ column: 'createdAt', direction: 'desc' })

// Form state
const formState = reactive({
  id: null,
  name: '',
  slug: '',
  description: '',
  parentId: null
})

// Computed
const isEditing = computed(() => !!formState.id)

const parentCategoryOptions = computed(() => {
  // Filter out the current category and its children to prevent circular references
  const filteredCategories = categories.value.filter(cat => 
    !isEditing.value || cat.id !== formState.id
  )
  
  return [{ id: null, name: 'None (Top Level)' }, ...filteredCategories]
})

// Methods
const fetchCategories = async () => {
  isLoading.value = true
  
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
  } finally {
    isLoading.value = false
  }
}

const openAddModal = () => {
  // Reset form
  Object.assign(formState, {
    id: null,
    name: '',
    slug: '',
    description: '',
    parentId: null
  })
  
  isAutoSlug.value = true
  isModalOpen.value = true
}

const editCategory = (category) => {
  // Populate form with category data
  Object.assign(formState, {
    id: category.id,
    name: category.name,
    slug: category.slug,
    description: category.description || '',
    parentId: category.parentId
  })
  
  isAutoSlug.value = false
  isModalOpen.value = true
}

const saveCategory = async () => {
  isSaving.value = true
  
  try {
    // Generate slug if auto-slug is enabled
    if (isAutoSlug.value) {
      formState.slug = generateSlug(formState.name)
    }
    
    let result
    
    if (isEditing.value) {
      // Update existing category
      result = await api.put(`/api/categories/${formState.id}`, formState)
    } else {
      // Create new category
      result = await api.post('/api/categories', formState)
    }
    
    if (result.error) {
      throw new Error(result.error.message || 'Failed to save category')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: isEditing.value ? 'Category updated successfully' : 'Category created successfully',
      color: 'green'
    })
    
    isModalOpen.value = false
    fetchCategories()
  } catch (err) {
    console.error('Error saving category:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to save category',
      color: 'red'
    })
  } finally {
    isSaving.value = false
  }
}

const confirmDelete = (category) => {
  selectedCategory.value = category
  isDeleteModalOpen.value = true
}

const deleteCategory = async () => {
  if (!selectedCategory.value) return
  
  isDeleting.value = true
  
  try {
    const { error } = await api.delete(`/api/categories/${selectedCategory.value.id}`)
    
    if (error) {
      throw new Error(error.message || 'Failed to delete category')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: 'Category deleted successfully',
      color: 'green'
    })
    
    isDeleteModalOpen.value = false
    fetchCategories()
  } catch (err) {
    console.error('Error deleting category:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to delete category',
      color: 'red'
    })
  } finally {
    isDeleting.value = false
  }
}

const sortTable = (column, direction) => {
  sortBy.value = { column, direction }
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

const generateSlug = (name) => {
  return name
    .toLowerCase()
    .replace(/[^\w\s-]/g, '') // Remove special characters
    .replace(/\s+/g, '-') // Replace spaces with hyphens
    .replace(/-+/g, '-') // Replace multiple hyphens with single hyphen
}

// Watch for name changes to update slug if auto-slug is enabled
watch(() => formState.name, (newName) => {
  if (isAutoSlug.value && newName) {
    formState.slug = generateSlug(newName)
  }
})

// Fetch categories on component mount
onMounted(() => {
  fetchCategories()
})
</script> 