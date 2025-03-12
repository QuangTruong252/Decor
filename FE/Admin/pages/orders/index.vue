<template>
  <div>
    <div class="mb-6">
      <h1 class="text-2xl font-bold">Quản lý đơn hàng</h1>
      <p class="text-gray-600">Xem và cập nhật trạng thái các đơn hàng</p>
    </div>
    
    <!-- Filters -->
    <UCard class="mb-6">
      <div class="flex flex-col md:flex-row gap-4">
        <!-- Status filter -->
        <USelectMenu
          v-model="statusFilter"
          :options="statusOptions"
          placeholder="Lọc theo trạng thái"
          class="w-full md:w-64"
          @update:model-value="loadOrders"
        />
        
        <!-- Date range picker -->
        <UFormGroup class="w-full md:w-auto">
          <UDatePicker 
            v-model="dateRange" 
            range 
            placeholder="Lọc theo ngày đặt hàng"
            @update:model-value="loadOrders"
          />
        </UFormGroup>
        
        <!-- Reset filters -->
        <UButton
          v-if="isFiltered"
          color="gray"
          variant="ghost"
          icon="i-heroicons-x-mark"
          @click="resetFilters"
        >
          Xóa bộ lọc
        </UButton>
      </div>
    </UCard>
    
    <!-- Orders table -->
    <UCard>
      <UTable 
        :columns="columns"
        :rows="orders"
        :loading="loading"
        :empty-state="{ icon: 'i-heroicons-shopping-cart', label: 'Không có đơn hàng nào' }"
      >
        <!-- Order ID column with link -->
        <template #id-data="{ row }">
          <UButton 
            variant="link" 
            color="primary" 
            @click="viewOrderDetails(row)"
          >
            #{{ row.id }}
          </UButton>
        </template>
        
        <!-- Order date column -->
        <template #orderDate-data="{ row }">
          {{ formatDate(row.orderDate) }}
        </template>
        
        <!-- Amount column -->
        <template #totalAmount-data="{ row }">
          {{ formatCurrency(row.totalAmount) }}
        </template>
        
        <!-- Status column -->
        <template #status-data="{ row }">
          <UBadge
            :color="getOrderStatusColor(row.status)"
            variant="subtle"
            size="sm"
          >
            {{ row.status }}
          </UBadge>
        </template>
        
        <!-- Payment method column -->
        <template #paymentMethod-data="{ row }">
          <div class="flex items-center">
            <UIcon
              :name="getPaymentIcon(row.paymentMethod)"
              class="mr-2"
            />
            {{ row.paymentMethod }}
          </div>
        </template>
        
        <!-- Actions column -->
        <template #actions-data="{ row }">
          <div class="flex items-center gap-2">
            <UButton
              icon="i-heroicons-eye"
              color="gray"
              variant="ghost"
              size="sm"
              @click="viewOrderDetails(row)"
            />
            <UDropdown 
              :items="getStatusActions(row)"
              :popper="{ placement: 'bottom-end' }"
            >
              <UButton
                color="primary"
                variant="ghost"
                size="sm"
                trailing-icon="i-heroicons-chevron-down"
              >
                Cập nhật
              </UButton>
            </UDropdown>
          </div>
        </template>
      </UTable>
      
      <!-- Pagination -->
      <div class="mt-6 flex justify-center">
        <UPagination
          v-model="currentPage"
          :page-count="pageCount"
          :total="totalItems"
          :ui="{ wrapper: 'flex items-center gap-1' }"
          :page-size="pageSize"
          @update:model-value="handlePageChange"
        />
      </div>
    </UCard>
    
    <!-- Order details modal -->
    <UModal v-model="showDetailsModal" size="2xl">
      <UCard v-if="selectedOrder">
        <template #header>
          <div class="flex justify-between items-center">
            <h3 class="text-lg font-semibold">
              Chi tiết đơn hàng #{{ selectedOrder.id }}
            </h3>
            <UButton 
              icon="i-heroicons-x-mark" 
              color="gray" 
              variant="ghost" 
              @click="showDetailsModal = false" 
            />
          </div>
        </template>
        
        <div class="space-y-6">
          <!-- Order info -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <h4 class="text-sm font-medium text-gray-500 mb-1">Thông tin đơn hàng</h4>
              <table class="w-full text-sm">
                <tbody>
                  <tr>
                    <td class="py-1 text-gray-500">Mã đơn hàng:</td>
                    <td class="py-1 font-medium">#{{ selectedOrder.id }}</td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Ngày đặt:</td>
                    <td class="py-1">{{ formatDate(selectedOrder.orderDate) }}</td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Trạng thái:</td>
                    <td class="py-1">
                      <UBadge
                        :color="getOrderStatusColor(selectedOrder.status)"
                        variant="subtle"
                        size="sm"
                      >
                        {{ selectedOrder.status }}
                      </UBadge>
                    </td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Phương thức thanh toán:</td>
                    <td class="py-1">{{ selectedOrder.paymentMethod }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            
            <div>
              <h4 class="text-sm font-medium text-gray-500 mb-1">Thông tin khách hàng</h4>
              <table class="w-full text-sm">
                <tbody>
                  <tr>
                    <td class="py-1 text-gray-500">Tên khách hàng:</td>
                    <td class="py-1 font-medium">{{ selectedOrder.customerName }}</td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Email:</td>
                    <td class="py-1">{{ selectedOrder.customerEmail }}</td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Số điện thoại:</td>
                    <td class="py-1">{{ selectedOrder.customerPhone }}</td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Địa chỉ:</td>
                    <td class="py-1">{{ selectedOrder.shippingAddress }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          
          <!-- Order items -->
          <div>
            <h4 class="text-sm font-medium text-gray-500 mb-2">Danh sách sản phẩm</h4>
            <div class="border rounded-lg overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Sản phẩm
                    </th>
                    <th scope="col" class="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Số lượng
                    </th>
                    <th scope="col" class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Đơn giá
                    </th>
                    <th scope="col" class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Thành tiền
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  <tr v-for="item in selectedOrder.orderItems" :key="item.id">
                    <td class="px-4 py-4 whitespace-nowrap">
                      <div class="flex items-center">
                        <div class="h-10 w-10 flex-shrink-0">
                          <img 
                            v-if="item.productImage" 
                            :src="item.productImage" 
                            :alt="item.productName"
                            class="h-10 w-10 object-cover rounded"
                          />
                          <div v-else class="h-10 w-10 rounded bg-gray-100 flex items-center justify-center">
                            <UIcon name="i-heroicons-photo" class="text-gray-400" />
                          </div>
                        </div>
                        <div class="ml-4">
                          <div class="text-sm font-medium text-gray-900">
                            {{ item.productName }}
                          </div>
                          <div v-if="item.variant" class="text-xs text-gray-500">
                            {{ item.variant }}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td class="px-4 py-4 whitespace-nowrap text-sm text-gray-500 text-center">
                      {{ item.quantity }}
                    </td>
                    <td class="px-4 py-4 whitespace-nowrap text-sm text-gray-500 text-right">
                      {{ formatCurrency(item.unitPrice) }}
                    </td>
                    <td class="px-4 py-4 whitespace-nowrap text-sm font-medium text-gray-900 text-right">
                      {{ formatCurrency(item.unitPrice * item.quantity) }}
                    </td>
                  </tr>
                </tbody>
                <tfoot class="bg-gray-50">
                  <tr>
                    <td colspan="3" class="px-4 py-3 text-right text-sm font-medium text-gray-500">
                      Tổng tiền hàng:
                    </td>
                    <td class="px-4 py-3 text-right text-sm font-medium text-gray-900">
                      {{ formatCurrency(selectedOrder.subtotal) }}
                    </td>
                  </tr>
                  <tr>
                    <td colspan="3" class="px-4 py-3 text-right text-sm font-medium text-gray-500">
                      Phí vận chuyển:
                    </td>
                    <td class="px-4 py-3 text-right text-sm font-medium text-gray-900">
                      {{ formatCurrency(selectedOrder.shippingFee) }}
                    </td>
                  </tr>
                  <tr v-if="selectedOrder.discount">
                    <td colspan="3" class="px-4 py-3 text-right text-sm font-medium text-gray-500">
                      Giảm giá:
                    </td>
                    <td class="px-4 py-3 text-right text-sm font-medium text-red-600">
                      -{{ formatCurrency(selectedOrder.discount) }}
                    </td>
                  </tr>
                  <tr>
                    <td colspan="3" class="px-4 py-3 text-right text-sm font-bold text-gray-900">
                      Tổng cộng:
                    </td>
                    <td class="px-4 py-3 text-right text-sm font-bold text-gray-900">
                      {{ formatCurrency(selectedOrder.totalAmount) }}
                    </td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
          
          <!-- Update order status -->
          <div class="border rounded-lg p-4 bg-gray-50">
            <h4 class="text-sm font-medium text-gray-700 mb-3">Cập nhật trạng thái đơn hàng</h4>
            <div class="flex items-center space-x-4">
              <USelectMenu
                v-model="newStatus"
                :options="allStatusOptions"
                class="w-64"
                placeholder="Chọn trạng thái mới"
              />
              <UButton
                color="primary"
                :loading="updating"
                :disabled="!newStatus || newStatus === selectedOrder.status || updating"
                @click="updateOrderStatus"
              >
                Cập nhật
              </UButton>
            </div>
          </div>
        </div>
      </UCard>
    </UModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'


// Meta
definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

// State
const orders = ref([])
const loading = ref(true)
const updating = ref(false)
const selectedOrder = ref(null)
const showDetailsModal = ref(false)
const newStatus = ref(null)

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)
const pageCount = ref(1)
const totalItems = ref(0)

// Filters
const statusFilter = ref(null)
const dateRange = ref(null)

// Constants
const statusOptions = [
  { label: 'Tất cả trạng thái', value: null },
  { label: 'Chờ xác nhận', value: 'Pending' },
  { label: 'Đã xác nhận', value: 'Confirmed' },
  { label: 'Đang xử lý', value: 'Processing' },
  { label: 'Đang giao hàng', value: 'Shipping' },
  { label: 'Hoàn thành', value: 'Completed' },
  { label: 'Đã hủy', value: 'Cancelled' }
]

const allStatusOptions = statusOptions.filter(option => option.value !== null)

// Table columns
const columns = [
  { key: 'id', label: 'Mã đơn', sortable: true },
  { key: 'orderDate', label: 'Ngày đặt', sortable: true },
  { key: 'customerName', label: 'Khách hàng', sortable: true },
  { key: 'totalAmount', label: 'Tổng tiền', sortable: true },
  { key: 'status', label: 'Trạng thái', sortable: true },
  { key: 'paymentMethod', label: 'Thanh toán' },
  { key: 'actions', label: 'Thao tác' }
]

// Composables
const toast = useToast()

// Computed
const isFiltered = computed(() => {
  return statusFilter.value !== null || dateRange.value !== null
})

// Methods
// Format date
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

// Format currency
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

// Get color for order status
const getOrderStatusColor = (status) => {
  switch (status) {
    case 'Pending': return 'yellow'
    case 'Confirmed': return 'blue'
    case 'Processing': return 'indigo'
    case 'Shipping': return 'purple'
    case 'Completed': return 'green'
    case 'Cancelled': return 'red'
    default: return 'gray'
  }
}

// Get payment icon
const getPaymentIcon = (method) => {
  switch (method) {
    case 'Credit Card': return 'i-heroicons-credit-card'
    case 'PayPal': return 'i-heroicons-currency-dollar'
    case 'Bank Transfer': return 'i-heroicons-building-library'
    case 'Cash on Delivery': return 'i-heroicons-banknotes'
    default: return 'i-heroicons-currency-dollar'
  }
}

// Get status update actions
const getStatusActions = (order) => {
  // Return available status transitions based on current status
  const actions = []
  
  switch (order.status) {
    case 'Pending':
      actions.push(
        { label: 'Xác nhận đơn hàng', click: () => updateStatus(order, 'Confirmed') },
        { label: 'Hủy đơn hàng', click: () => updateStatus(order, 'Cancelled') }
      )
      break
    case 'Confirmed':
      actions.push(
        { label: 'Chuyển sang đang xử lý', click: () => updateStatus(order, 'Processing') },
        { label: 'Hủy đơn hàng', click: () => updateStatus(order, 'Cancelled') }
      )
      break
    case 'Processing':
      actions.push(
        { label: 'Chuyển sang đang giao hàng', click: () => updateStatus(order, 'Shipping') },
        { label: 'Hủy đơn hàng', click: () => updateStatus(order, 'Cancelled') }
      )
      break
    case 'Shipping':
      actions.push(
        { label: 'Hoàn thành đơn hàng', click: () => updateStatus(order, 'Completed') }
      )
      break
    case 'Completed':
      // No further actions
      break
    case 'Cancelled':
      // No further actions
      break
  }
  
  return actions
}

// Load orders
const loadOrders = async () => {
  try {
    loading.value = true
    
    // Prepare query parameters
    const params = {
      page: currentPage.value,
      pageSize: pageSize.value,
      status: statusFilter.value
    }
    
    // Add date range if selected
    if (dateRange.value && dateRange.value.length === 2) {
      params.startDate = dateRange.value[0]
      params.endDate = dateRange.value[1]
    }
    
    // Call API
    const response = await $fetch('/api/orders', {
      method: 'GET',
      params
    })
    
    orders.value = response.items || []
    totalItems.value = response.totalItems || 0
    pageCount.value = response.pageCount || 1
    
  } catch (error) {
    toast.error('Không thể tải danh sách đơn hàng')
    console.error(error)
  } finally {
    loading.value = false
  }
}

// Reset filters
const resetFilters = () => {
  statusFilter.value = null
  dateRange.value = null
  loadOrders()
}

// Handle page change
const handlePageChange = (page) => {
  currentPage.value = page
  loadOrders()
}

// View order details
const viewOrderDetails = async (order) => {
  try {
    loading.value = true
    
    // In a real app, you would fetch the full order details here
    // For now, we'll use the existing order data
    selectedOrder.value = order
    newStatus.value = order.status
    showDetailsModal.value = true
    
  } catch (error) {
    toast.error('Không thể tải chi tiết đơn hàng')
  } finally {
    loading.value = false
  }
}

// Update order status (quick update)
const updateStatus = async (order, newStatus) => {
  try {
    updating.value = true
    
    // Call API to update order status
    await $fetch(`/api/orders/${order.id}/status`, {
      method: 'PATCH',
      body: { status: newStatus }
    })
    
    // Update local order status
    order.status = newStatus
    
    toast.success(`Đã cập nhật trạng thái đơn hàng thành ${newStatus}`)
    
  } catch (error) {
    toast.error('Không thể cập nhật trạng thái đơn hàng')
  } finally {
    updating.value = false
  }
}

// Update order status from modal
const updateOrderStatus = async () => {
  if (!selectedOrder.value || !newStatus.value) return
  
  try {
    updating.value = true
    
    // Call API to update order status
    await $fetch(`/api/orders/${selectedOrder.value.id}/status`, {
      method: 'PATCH',
      body: { status: newStatus.value }
    })
    
    // Update local order status
    selectedOrder.value.status = newStatus.value
    
    // Also update in the orders list
    const orderInList = orders.value.find(o => o.id === selectedOrder.value.id)
    if (orderInList) {
      orderInList.status = newStatus.value
    }
    
    toast.success(`Đã cập nhật trạng thái đơn hàng thành ${newStatus.value}`)
    
  } catch (error) {
    toast.error('Không thể cập nhật trạng thái đơn hàng')
  } finally {
    updating.value = false
  }
}

// Load initial data
onMounted(() => {
  loadOrders()
})
</script> 