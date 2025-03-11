<template>
  <div>
    <UPageHeader title="Dashboard" description="Overview of your store" />
    
    <!-- Stats Cards -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 mt-6">
      <UCard v-for="(stat, index) in stats" :key="index" class="bg-white dark:bg-gray-800">
        <div class="flex items-center">
          <div class="flex-shrink-0 rounded-md bg-primary-100 dark:bg-primary-900 p-3">
            <UIcon :name="stat.icon" class="h-6 w-6 text-primary-600 dark:text-primary-400" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400">{{ stat.name }}</h3>
            <div class="flex items-baseline">
              <p class="text-2xl font-semibold text-gray-900 dark:text-white">{{ stat.value }}</p>
              <p 
                :class="[
                  'ml-2 text-sm',
                  stat.change > 0 ? 'text-green-600' : 'text-red-600'
                ]"
              >
                {{ stat.change > 0 ? '+' : '' }}{{ stat.change }}%
              </p>
            </div>
          </div>
        </div>
      </UCard>
    </div>
    
    <!-- Charts -->
    <div class="mt-8 grid grid-cols-1 gap-4 lg:grid-cols-2">
      <!-- Sales Chart -->
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium">Sales Overview</h3>
            <USelect v-model="salesPeriod" :options="periodOptions" size="sm" />
          </div>
        </template>
        <div class="h-80">
          <UChart
            :data="salesChartData"
            :options="chartOptions"
            type="bar"
          />
        </div>
      </UCard>
      
      <!-- Products Chart -->
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium">Top Categories</h3>
          </div>
        </template>
        <div class="h-80">
          <UChart
            :data="categoryChartData"
            :options="chartOptions"
            type="pie"
          />
        </div>
      </UCard>
    </div>
    
    <!-- Recent Orders -->
    <UCard class="mt-8">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-medium">Recent Orders</h3>
          <UButton to="/orders" size="sm" variant="ghost">View all</UButton>
        </div>
      </template>
      <UTable :columns="orderColumns" :rows="recentOrders" />
    </UCard>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'

// Define page meta
definePageMeta({
  middleware: ['auth']
})

const api = useApi()
const isLoading = ref(true)
const statsData = ref({
  totalProducts: 0,
  totalUsers: 0,
  totalCategories: 0,
  totalOrders: 0
})

// Stats cards data
const stats = computed(() => [
  {
    name: 'Total Products',
    value: statsData.value.totalProducts,
    icon: 'i-heroicons-cube',
    change: 12.5
  },
  {
    name: 'Total Users',
    value: statsData.value.totalUsers,
    icon: 'i-heroicons-users',
    change: 8.2
  },
  {
    name: 'Total Categories',
    value: statsData.value.totalCategories,
    icon: 'i-heroicons-tag',
    change: 4.6
  },
  {
    name: 'Total Orders',
    value: statsData.value.totalOrders,
    icon: 'i-heroicons-shopping-cart',
    change: 15.3
  }
])

// Chart data
const salesPeriod = ref('week')
const periodOptions = ['day', 'week', 'month', 'year']

const salesChartData = ref({
  labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  datasets: [
    {
      label: 'Sales',
      data: [65, 59, 80, 81, 56, 55, 40],
      backgroundColor: '#3b82f6'
    }
  ]
})

const categoryChartData = ref({
  labels: ['Furniture', 'Decor', 'Lighting', 'Textiles', 'Kitchen'],
  datasets: [
    {
      data: [30, 25, 20, 15, 10],
      backgroundColor: ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6']
    }
  ]
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false
}

// Recent orders table
const orderColumns = [
  {
    key: 'id',
    label: 'Order ID'
  },
  {
    key: 'customer',
    label: 'Customer'
  },
  {
    key: 'date',
    label: 'Date'
  },
  {
    key: 'amount',
    label: 'Amount'
  },
  {
    key: 'status',
    label: 'Status'
  }
]

const recentOrders = ref([
  {
    id: '#ORD-001',
    customer: 'John Doe',
    date: '2023-11-05',
    amount: '$125.00',
    status: 'Completed'
  },
  {
    id: '#ORD-002',
    customer: 'Jane Smith',
    date: '2023-11-04',
    amount: '$250.00',
    status: 'Processing'
  },
  {
    id: '#ORD-003',
    customer: 'Robert Johnson',
    date: '2023-11-03',
    amount: '$340.00',
    status: 'Completed'
  },
  {
    id: '#ORD-004',
    customer: 'Emily Davis',
    date: '2023-11-02',
    amount: '$520.00',
    status: 'Shipped'
  },
  {
    id: '#ORD-005',
    customer: 'Michael Wilson',
    date: '2023-11-01',
    amount: '$175.00',
    status: 'Completed'
  }
])

// Fetch dashboard data
const fetchDashboardData = async () => {
  isLoading.value = true
  
  try {
    const { data, error } = await api.get('/api/admin/stats')
    
    if (error) {
      console.error('Error fetching dashboard stats:', error)
      return
    }
    
    if (data) {
      statsData.value = data
    }
  } catch (err) {
    console.error('Error fetching dashboard data:', err)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchDashboardData()
})
</script> 