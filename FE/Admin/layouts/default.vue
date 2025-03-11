<template>
  <div class="min-h-screen bg-gray-100 dark:bg-gray-900">
    <!-- Mobile sidebar overlay -->
    <div
      v-if="isSidebarOpen"
      class="fixed inset-0 z-40 bg-black bg-opacity-50 lg:hidden"
      @click="toggleSidebar"
    ></div>

    <!-- Sidebar -->
    <aside
      :class="[
        'fixed top-0 left-0 z-50 h-full w-64 transform bg-white dark:bg-gray-800 transition-transform duration-300 ease-in-out',
        isSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      ]"
    >
      <div class="flex h-16 items-center justify-center border-b border-gray-200 dark:border-gray-700">
        <h1 class="text-xl font-bold text-primary-600">Decor Admin</h1>
      </div>
      
      <!-- Sidebar menu -->
      <nav class="mt-5 px-2">
        <NuxtLink to="/dashboard" class="sidebar-link group">
          <UIcon name="i-heroicons-home" class="mr-3 h-5 w-5" />
          <span>Dashboard</span>
        </NuxtLink>
        
        <NuxtLink to="/banners" class="sidebar-link group">
          <UIcon name="i-heroicons-photo" class="mr-3 h-5 w-5" />
          <span>Banners</span>
        </NuxtLink>
        
        <NuxtLink to="/categories" class="sidebar-link group">
          <UIcon name="i-heroicons-tag" class="mr-3 h-5 w-5" />
          <span>Categories</span>
        </NuxtLink>
        
        <NuxtLink to="/products" class="sidebar-link group">
          <UIcon name="i-heroicons-cube" class="mr-3 h-5 w-5" />
          <span>Products</span>
        </NuxtLink>
        
        <NuxtLink to="/users" class="sidebar-link group">
          <UIcon name="i-heroicons-users" class="mr-3 h-5 w-5" />
          <span>Users</span>
        </NuxtLink>
      </nav>
    </aside>

    <!-- Main content -->
    <div class="lg:pl-64">
      <!-- Header -->
      <header class="sticky top-0 z-30 flex h-16 items-center bg-white dark:bg-gray-800 shadow">
        <div class="ml-4 lg:hidden">
          <UButton 
            color="gray" 
            variant="ghost" 
            icon="i-heroicons-bars-3"
            @click="toggleSidebar"
          />
        </div>
        
        <div class="ml-auto flex items-center px-4">
          <ColorScheme>
            <UButton
              color="gray"
              variant="ghost"
              :icon="$colorMode.preference === 'dark' ? 'i-heroicons-sun' : 'i-heroicons-moon'"
              @click="$colorMode.preference = $colorMode.preference === 'dark' ? 'light' : 'dark'"
            />
          </ColorScheme>
          
          <UDropdown :items="userMenuItems">
            <UButton color="white" variant="ghost" class="ml-2 flex items-center">
              <UAvatar :src="userAvatar" size="sm" class="mr-2" />
              <span class="font-medium">{{ userName }}</span>
              <UIcon name="i-heroicons-chevron-down" class="ml-1 h-4 w-4" />
            </UButton>
          </UDropdown>
        </div>
      </header>

      <!-- Page content -->
      <main class="p-4">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useAuthStore } from '~/stores/auth'

const auth = useAuthStore()
const isSidebarOpen = ref(false)

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

// User info
const userName = computed(() => auth.user?.name || 'Admin User')
const userAvatar = computed(() => auth.user?.avatar || 'https://ui.shadcn.com/avatars/01.png')

// User dropdown menu
const userMenuItems = computed(() => [
  [
    {
      label: 'Profile',
      icon: 'i-heroicons-user-circle',
      click: () => {}
    },
    {
      label: 'Settings',
      icon: 'i-heroicons-cog-6-tooth',
      click: () => {}
    }
  ],
  [
    {
      label: 'Logout',
      icon: 'i-heroicons-arrow-right-on-rectangle',
      click: () => auth.logout()
    }
  ]
])
</script>

<style scoped>
.sidebar-link {
  @apply flex items-center rounded-md px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700 my-1;
}

.router-link-active {
  @apply bg-primary-50 text-primary-600 dark:bg-primary-900/20 dark:text-primary-400;
}
</style> 