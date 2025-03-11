<script setup>
// Redirect to dashboard
definePageMeta({
  middleware: ['auth']
})

// Redirect to dashboard
onMounted(() => {
  navigateTo('/dashboard')
})
</script>

<template>
  <div class="flex min-h-screen items-center justify-center">
    <UCard>
      <div class="text-center">
        <h1 class="text-xl font-bold">Redirecting to Dashboard...</h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">Please wait...</p>
        <UButton to="/dashboard" color="primary" class="mt-4">Go to Dashboard</UButton>
      </div>
    </UCard>
  </div>
</template>
