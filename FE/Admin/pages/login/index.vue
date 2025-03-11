<template>
  <div class="flex min-h-screen items-center justify-center bg-gray-100 dark:bg-gray-900 p-4">
    <UCard class="w-full max-w-md">
      <template #header>
        <div class="text-center">
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Decor Admin</h1>
          <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Sign in to your account</p>
        </div>
      </template>
      
      <UForm :state="formState" class="space-y-4" @submit="handleLogin">
        <UFormGroup label="Email" name="email">
          <UInput
            v-model="formState.email"
            type="email"
            placeholder="admin@example.com"
            autocomplete="email"
            :ui="{ base: 'w-full' }"
            required
          />
        </UFormGroup>
        
        <UFormGroup label="Password" name="password">
          <UInput
            v-model="formState.password"
            type="password"
            placeholder="••••••••"
            autocomplete="current-password"
            :ui="{ base: 'w-full' }"
            required
          />
        </UFormGroup>
        
        <div class="flex items-center justify-between">
          <UCheckbox v-model="formState.remember" name="remember" label="Remember me" />
          <UButton variant="link" to="#" size="sm">Forgot password?</UButton>
        </div>
        
        <UButton
          type="submit"
          color="primary"
          block
          :loading="isLoading"
          :disabled="isLoading"
        >
          Sign in
        </UButton>
        
        <UAlert
          v-if="error"
          color="red"
          variant="soft"
          icon="i-heroicons-exclamation-triangle"
          title="Login Failed"
          :description="error"
          class="mt-4"
        />
      </UForm>
      
      <template #footer>
        <div class="flex justify-between items-center">
          <ColorScheme>
            <UButton
              color="gray"
              variant="ghost"
              :icon="$colorMode.preference === 'dark' ? 'i-heroicons-sun' : 'i-heroicons-moon'"
              @click="$colorMode.preference = $colorMode.preference === 'dark' ? 'light' : 'dark'"
            />
          </ColorScheme>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            &copy; {{ new Date().getFullYear() }} Decor Store
          </p>
        </div>
      </template>
    </UCard>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { useRoute, useRouter } from 'vue-router'

// No layout for login page
definePageMeta({
  layout: false,
  middleware: ['auth']
})

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const isLoading = ref(false)
const error = ref('')

// Form state
const formState = reactive({
  email: '',
  password: '',
  remember: false
})

// Handle login form submission
const handleLogin = async () => {
  isLoading.value = true
  error.value = ''
  
  try {
    const result = await auth.login(formState.email, formState.password)
    
    if (result.success) {
      // Redirect to dashboard or intended page
      const redirectPath = route.query.redirect || '/dashboard'
      router.push(redirectPath)
    } else {
      error.value = result.message || 'Invalid email or password'
    }
  } catch (err) {
    error.value = 'An error occurred during login'
    console.error(err)
  } finally {
    isLoading.value = false
  }
}

// Check for error message in query params (e.g., from auth middleware)
onMounted(() => {
  if (route.query.error) {
    error.value = route.query.message || 'Authentication error'
  }
})
</script> 