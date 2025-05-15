export const useImage = () => {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiImageResourceUrl
  
  const getImageUrl = (path: string) => {
    return `${baseURL}/${path}`
  }
  
  return {
    getImageUrl
  }
}