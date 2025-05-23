// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
  modules: [
    '@nuxt/ui',
    '@pinia/nuxt',
    '@nuxtjs/tailwindcss'
  ],
  runtimeConfig: {
    public: {
      apiBaseUrl: 'http://localhost:5000',
      apiImageResourceUrl: 'http://localhost:5000/resources'
    }
  }
})
