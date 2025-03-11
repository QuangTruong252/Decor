<template>
  <div>
    <UPageHeader title="User Management" description="Manage user accounts">
      <template #right>
        <UInput
          v-model="searchQuery"
          placeholder="Search users..."
          icon="i-heroicons-magnifying-glass"
          class="w-64"
          @input="debounceSearch"
        />
      </template>
    </UPageHeader>
    
    <!-- Users Table -->
    <UCard class="mt-6">
      <UTable
        :columns="columns"
        :rows="filteredUsers"
        :loading="isLoading"
        :empty-state="{ icon: 'i-heroicons-users', label: 'No users found' }"
        :sort="{ column: 'createdAt', direction: 'desc' }"
        @sort="sortTable"
      >
        <template #role-data="{ row }">
          <UBadge :color="getRoleBadgeColor(row.role)" size="sm">
            {{ row.role }}
          </UBadge>
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
              @click="editUser(row)"
            />
            <UButton
              :color="row.isActive ? 'red' : 'green'"
              variant="ghost"
              :icon="row.isActive ? 'i-heroicons-lock-closed' : 'i-heroicons-lock-open'"
              size="xs"
              @click="toggleUserStatus(row)"
            />
          </div>
        </template>
      </UTable>
      
      <!-- Pagination -->
      <div class="mt-4 flex items-center justify-between">
        <div class="text-sm text-gray-600 dark:text-gray-400">
          Showing {{ paginationInfo.from }} to {{ paginationInfo.to }} of {{ paginationInfo.total }} users
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
    
    <!-- Edit User Modal -->
    <UModal v-model="isModalOpen">
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium">Edit User</h3>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-x-mark"
              @click="isModalOpen = false"
            />
          </div>
        </template>
        
        <UForm :state="formState" class="space-y-4" @submit="saveUser">
          <UFormGroup label="Name" name="name">
            <UInput
              v-model="formState.name"
              placeholder="User name"
              disabled
            />
          </UFormGroup>
          
          <UFormGroup label="Email" name="email">
            <UInput
              v-model="formState.email"
              placeholder="user@example.com"
              disabled
            />
          </UFormGroup>
          
          <UFormGroup label="Role" name="role">
            <USelect
              v-model="formState.role"
              :options="roleOptions"
              placeholder="Select role"
            />
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
              Update
            </UButton>
          </div>
        </UForm>
      </UCard>
    </UModal>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { useToast } from '@nuxt/ui'
import { useAuthStore } from '~/stores/auth'

// Define page meta
definePageMeta({
  middleware: ['auth']
})

const api = useApi()
const toast = useToast()
const auth = useAuthStore()

// Table columns
const columns = [
  {
    key: 'name',
    label: 'Name',
    sortable: true
  },
  {
    key: 'email',
    label: 'Email',
    sortable: true
  },
  {
    key: 'role',
    label: 'Role',
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
const users = ref([])
const isLoading = ref(true)
const isModalOpen = ref(false)
const isSaving = ref(false)
const searchQuery = ref('')
const sortBy = ref({ column: 'createdAt', direction: 'desc' })
const currentPage = ref(1)
const perPage = ref(10)

// Form state
const formState = reactive({
  id: '',
  name: '',
  email: '',
  role: '',
  isActive: true
})

// Role options
const roleOptions = ['User', 'Admin']

// Computed
const filteredUsers = computed(() => {
  if (!searchQuery.value) return users.value
  
  const query = searchQuery.value.toLowerCase()
  return users.value.filter(user => 
    user.name.toLowerCase().includes(query) || 
    user.email.toLowerCase().includes(query) ||
    user.role.toLowerCase().includes(query)
  )
})

const pageCount = computed(() => {
  return Math.ceil(filteredUsers.value.length / perPage.value)
})

const paginationInfo = computed(() => {
  const total = filteredUsers.value.length
  const from = total === 0 ? 0 : (currentPage.value - 1) * perPage.value + 1
  const to = Math.min(from + perPage.value - 1, total)
  
  return { from, to, total }
})

const paginatedUsers = computed(() => {
  const start = (currentPage.value - 1) * perPage.value
  const end = start + perPage.value
  
  return filteredUsers.value.slice(start, end)
})

// Methods
const fetchUsers = async () => {
  isLoading.value = true
  
  try {
    const { data, error } = await api.get('/api/users')
    
    if (error) {
      toast.add({
        title: 'Error',
        description: 'Failed to load users',
        color: 'red'
      })
      return
    }
    
    users.value = data || []
  } catch (err) {
    console.error('Error fetching users:', err)
    toast.add({
      title: 'Error',
      description: 'Failed to load users',
      color: 'red'
    })
  } finally {
    isLoading.value = false
  }
}

const editUser = (user) => {
  // Populate form with user data
  Object.assign(formState, {
    id: user.id,
    name: user.name,
    email: user.email,
    role: user.role,
    isActive: user.isActive
  })
  
  isModalOpen.value = true
}

const saveUser = async () => {
  isSaving.value = true
  
  try {
    const { error } = await api.put(`/api/users/${formState.id}`, {
      role: formState.role,
      isActive: formState.isActive
    })
    
    if (error) {
      throw new Error(error.message || 'Failed to update user')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: 'User updated successfully',
      color: 'green'
    })
    
    // If current user is updated, refresh auth
    if (formState.id === auth.user?.id) {
      await auth.checkAuth()
    }
    
    isModalOpen.value = false
    fetchUsers()
  } catch (err) {
    console.error('Error updating user:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to update user',
      color: 'red'
    })
  } finally {
    isSaving.value = false
  }
}

const toggleUserStatus = async (user) => {
  try {
    // Prevent deactivating yourself
    if (user.id === auth.user?.id) {
      toast.add({
        title: 'Warning',
        description: 'You cannot deactivate your own account',
        color: 'yellow'
      })
      return
    }
    
    const { error } = await api.put(`/api/users/${user.id}/toggle-status`, {
      isActive: !user.isActive
    })
    
    if (error) {
      throw new Error(error.message || 'Failed to update user status')
    }
    
    // Success
    toast.add({
      title: 'Success',
      description: `User ${user.isActive ? 'deactivated' : 'activated'} successfully`,
      color: 'green'
    })
    
    fetchUsers()
  } catch (err) {
    console.error('Error toggling user status:', err)
    toast.add({
      title: 'Error',
      description: err.message || 'Failed to update user status',
      color: 'red'
    })
  }
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

const getRoleBadgeColor = (role) => {
  switch (role) {
    case 'Admin':
      return 'red'
    case 'User':
      return 'blue'
    default:
      return 'gray'
  }
}

// Debounce search
let debounceTimeout
const debounceSearch = () => {
  clearTimeout(debounceTimeout)
  debounceTimeout = setTimeout(() => {
    currentPage.value = 1 // Reset to first page on search
  }, 300)
}

// Fetch users on component mount
onMounted(() => {
  fetchUsers()
})
</script> 