import { useAuthStore } from '~/stores/auth'

export default defineNuxtRouteMiddleware(async (to) => {
  // Skip middleware if on login page
  if (to.path === '/login') {
    return
  }

  const auth = useAuthStore()
  
  // If not authenticated, redirect to login
  if (!auth.isAuthenticated) {
    return navigateTo('/login')
  }
  
  // Check if token is valid by making an API request
  const isValid = await auth.checkAuth()
  if (!isValid) {
    return navigateTo('/login')
  }
  
  // Check if user has admin role
  if (!auth.isAdmin) {
    return navigateTo('/login')
  }
}) 