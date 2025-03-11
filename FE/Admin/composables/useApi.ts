import { useAuthStore } from '~/stores/auth'

export const useApi = () => {
  const config = useRuntimeConfig()
  const auth = useAuthStore()
  
  const baseURL = config.public.apiBaseUrl
  
  const fetchWithAuth = async (endpoint: string, options: any = {}) => {
    const headers = {
      ...options.headers || {},
    }
    
    if (auth.token) {
      headers.Authorization = `Bearer ${auth.token}`
    }
    
    try {
      console.log(baseURL)
      const { data, error } = await useFetch(endpoint, {
        ...options,
        baseURL,
        headers
      })
      
      if (error.value) {
        // Handle 401 Unauthorized
        if (error.value.statusCode === 401) {
          auth.logout()
          return { data: null, error: error.value }
        }
        
        return { data: null, error: error.value }
      }
      
      return { data: data.value, error: null }
    } catch (err) {
      return { data: null, error: err }
    }
  }
  
  return {
    get: (endpoint: string, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'GET' }),
      
    post: (endpoint: string, data: any, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'POST', body: data }),
      
    put: (endpoint: string, data: any, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'PUT', body: data }),
      
    delete: (endpoint: string, options = {}) => 
      fetchWithAuth(endpoint, { ...options, method: 'DELETE' })
  }
} 