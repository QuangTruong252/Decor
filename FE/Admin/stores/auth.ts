import { defineStore } from 'pinia'

interface User {
  id: string
  email: string
  name: string
  role: string
}

interface AuthState {
  token: string | null
  user: User | null
  isAuthenticated: boolean
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => {
    if (process.client) {
      const storedToken = localStorage.getItem('token')
      const storedUser = localStorage.getItem('user')
      
      return {
        token: storedToken,
        user: storedUser ? JSON.parse(storedUser) : null,
        isAuthenticated: !!storedToken
      }
    }
    
    return {
      token: null,
      user: null,
      isAuthenticated: false
    }
  },
  
  getters: {
    isAdmin: (state) => state.user?.role === 'Admin',
    getToken: (state) => state.token,
    getUser: (state) => state.user
  },
  
  actions: {
    
    setToken(token: string) {
      this.token = token
      this.isAuthenticated = true
      if (process.client) {
        localStorage.setItem('token', token)
      }
    },
    
    setUser(user: User) {
      this.user = user
      if (process.client) {
        localStorage.setItem('user', JSON.stringify(user))
      }
    },
    
    async login(email: string, password: string) {
      try {
        const baseURL = useRuntimeConfig().public.apiBaseUrl
        const { data, error } = await useFetch(`${baseURL}/api/auth/login`, {
          method: 'POST',
          body: { email, password }
        })
        
        if (error.value) {
          throw new Error(error.value?.data?.message || 'Login failed')
        }
        
        if (data.value) {
          const { token, user } = data.value as { token: string; user: User }
          this.setToken(token)
          this.setUser(user)
          return { success: true }
        }
        
        return { success: false, message: 'Invalid response from server' }
      } catch (err: any) {
        return { success: false, message: err.message || 'Login failed' }
      }
    },
    
    logout() {
      this.token = null
      this.user = null
      this.isAuthenticated = false
      
      if (process.client) {
        localStorage.removeItem('token')
        localStorage.removeItem('user')
      }
      
      return navigateTo('/login')
    },
    
    async checkAuth() {
      if (!this.token) return false
      const baseURL = useRuntimeConfig().public.apiBaseUrl
      try {
        const { data, error } = await useFetch(`${baseURL}/api/auth/me`, {
          headers: {
            Authorization: `Bearer ${this.token}`
          }
        })
        
        if (error.value || !data.value) {
          this.logout()
          return false
        }
        
        this.setUser(data.value as User)
        return true
      } catch (err) {
        this.logout()
        return false
      }
    }
  }
}) 