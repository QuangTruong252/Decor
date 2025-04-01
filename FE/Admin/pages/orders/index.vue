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
            :color="getOrderStatusColor(row.orderStatus)"
            variant="subtle"
            size="sm"
          >
            {{ row.orderStatus }}
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
                        :color="getOrderStatusColor(selectedOrder.orderStatus)"
                        variant="subtle"
                        size="sm"
                      >
                        {{ selectedOrder.orderStatus }}
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
                    <td class="py-1 font-medium">{{ selectedOrder.userFullName }}</td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Email:</td>
                    <td class="py-1">{{ selectedOrder.userEmail || 'N/A' }}</td>
                  </tr>
                  <tr>
                    <td class="py-1 text-gray-500">Số điện thoại:</td>
                    <td class="py-1">{{ selectedOrder.userPhone || 'N/A' }}</td>
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
                            v-if="item.productImageUrl" 
                            :src="item.productImageUrl" 
                            :alt="item.productName || ''"
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
                          <div v-if="item.variantName" class="text-xs text-gray-500">
                            {{ item.variantName }}
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
                      {{ formatCurrency(item.unitPrice && item.quantity ? item.unitPrice * item.quantity : 0) }}
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
                :disabled="!newStatus || newStatus === selectedOrder.orderStatus || updating"
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

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import type { OrderDTO } from '~/api-services'
import { apiService, handleApiError, showSuccessToast, showErrorToast } from '~/api-services/api-service'
import { useApiCompat } from '~/composables/useApi'
import type { DropdownItem } from '#ui/types'

// Define page meta
definePageMeta({
  middleware: ['auth']
})

const apiCompat = useApiCompat()

// Define interfaces
interface DateRange {
  start?: Date;
  end?: Date;
}

interface OrderStatusUpdate {
  orderId: number;
  status: string;
  notes?: string;
}

// Extend OrderDTO with additional fields needed for UI
interface ExtendedOrderDTO extends OrderDTO {
  userEmail?: string | null;
  userPhone?: string | null;
  subtotal?: number;
  shippingFee?: number;
  discount?: number;
  orderItems?: Array<{
    id?: number;
    productName?: string | null;
    productImageUrl?: string | null;
    quantity?: number;
    unitPrice?: number;
    variantName?: string | null;
  }>;
}

// Table columns
const columns = [
  {
    key: 'id',
    label: 'Mã đơn hàng',
    sortable: true
  },
  {
    key: 'userFullName',
    label: 'Khách hàng',
    sortable: true
  },
  {
    key: 'orderDate',
    label: 'Ngày đặt',
    sortable: true
  },
  {
    key: 'totalAmount',
    label: 'Tổng tiền',
    sortable: true
  },
  {
    key: 'status',
    label: 'Trạng thái',
    sortable: true
  },
  {
    key: 'paymentMethod',
    label: 'Thanh toán',
    sortable: true
  },
  {
    key: 'actions',
    label: 'Thao tác',
    sortable: false
  }
]

// Status options
const statusOptions = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'Pending', label: 'Chờ xử lý' },
  { value: 'Processing', label: 'Đang xử lý' },
  { value: 'Shipped', label: 'Đã giao hàng' },
  { value: 'Delivered', label: 'Đã nhận hàng' },
  { value: 'Cancelled', label: 'Đã hủy' }
]

// Tạo danh sách không bao gồm tùy chọn trống cho dropdown thay đổi trạng thái
const allStatusOptions = statusOptions.filter(option => option.value !== '')

// State
const orders = ref<ExtendedOrderDTO[]>([])
const selectedOrder = ref<ExtendedOrderDTO | null>(null)
const loading = ref(true)
const updating = ref(false)
const isDetailsModalOpen = ref(false)
const isUpdateStatusModalOpen = ref(false)
const showDetailsModal = ref(false) // Để tương thích với template
const statusFilter = ref('')
const dateRange = ref<DateRange>({})
const currentPage = ref(1)
const perPage = ref(10)
const pageSize = ref(10) // Để tương thích với template
const totalOrders = ref(0)
const totalItems = ref(0) // Để tương thích với template
const pageCount = ref(1)
const newStatus = ref('')

// Status update form
const statusUpdateForm = reactive<OrderStatusUpdate>({
  orderId: 0,
  status: '',
  notes: ''
})

// Computed
const isFiltered = computed(() => {
  return statusFilter.value !== '' || !!dateRange.value.start || !!dateRange.value.end
})

// Methods
const loadOrders = async (): Promise<void> => {
  loading.value = true
  
  try {
    // Build query parameters
    const params: Record<string, any> = {
      page: currentPage.value,
      pageSize: perPage.value
    }
    
    if (statusFilter.value) {
      params.status = statusFilter.value
    }
    
    if (dateRange.value.start) {
      params.startDate = dateRange.value.start.toISOString().split('T')[0]
    }
    
    if (dateRange.value.end) {
      params.endDate = dateRange.value.end.toISOString().split('T')[0]
    }
    
    const res = await apiService.orderApi.apiOrderGet(params)
    
    if (res.status === 200) {
      orders.value = res.data
      totalOrders.value = res.data.length // Adjust if API provides total count
      pageCount.value = res.data.length // Adjust if API provides total count
    } else {
      showErrorToast('Failed to load orders')
    }
  } catch (err) {
    handleApiError(err, 'Failed to load orders')
  } finally {
    loading.value = false
  }
}

const viewOrderDetails = (order: ExtendedOrderDTO): void => {
  selectedOrder.value = order
  isDetailsModalOpen.value = true
  showDetailsModal.value = true
}

const openUpdateStatusModal = (order: ExtendedOrderDTO): void => {
  selectedOrder.value = order
  
  // Initialize the form with current order data
  statusUpdateForm.orderId = order.id || 0
  statusUpdateForm.status = order.orderStatus || ''
  statusUpdateForm.notes = ''
  
  isUpdateStatusModalOpen.value = true
}

const updateOrderStatus = async (): Promise<void> => {
  if (!selectedOrder.value || !statusUpdateForm.orderId) return
  
  try {
    const res = await apiService.orderApi.apiOrderIdStatusPut({
      id: statusUpdateForm.orderId,
      updateOrderStatusDTO: {
        orderStatus: statusUpdateForm.status,
        notes: statusUpdateForm.notes
      }
    })
    
    if (res.status === 200 || res.status === 204) {
      showSuccessToast('Order status updated successfully')
      isUpdateStatusModalOpen.value = false
      
      // Refresh orders list
      loadOrders()
    } else {
      showErrorToast('Failed to update order status')
    }
  } catch (err) {
    handleApiError(err, 'Failed to update order status')
  }
}

const resetFilters = (): void => {
  statusFilter.value = ''
  dateRange.value = {}
  loadOrders()
}

const changePage = (page: number): void => {
  currentPage.value = page
  loadOrders()
}

// Helper functions
const formatDate = (dateString: string | undefined): string => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

const formatCurrency = (amount: number | undefined): string => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount || 0)
}

type BadgeColor = 'gray' | 'red' | 'yellow' | 'green' | 'blue' | 'indigo' | 'purple' | 'pink'

const getOrderStatusColor = (status: string | null | undefined): BadgeColor => {
  if (!status) return 'gray'
  
  const statusMap: Record<string, BadgeColor> = {
    'Pending': 'yellow',
    'Processing': 'blue',
    'Shipped': 'indigo',
    'Delivered': 'green',
    'Cancelled': 'red'
  }
  
  return statusMap[status] || 'gray'
}

const getPaymentIcon = (method: string | undefined): string => {
  if (!method) return 'i-heroicons-credit-card'
  
  const iconMap: Record<string, string> = {
    'Credit Card': 'i-heroicons-credit-card',
    'PayPal': 'i-mdi-paypal',
    'Bank Transfer': 'i-heroicons-building-library',
    'Cash': 'i-heroicons-banknotes',
    'Momo': 'i-heroicons-device-phone-mobile'
  }
  
  return iconMap[method] || 'i-heroicons-credit-card'
}

// Get status update actions for dropdown menu
const getStatusActions = (order: ExtendedOrderDTO): DropdownItem[][] => {
  // Return available status transitions based on current status
  const actions: DropdownItem[] = []
  
  switch (order.orderStatus) {
    case 'Pending':
      actions.push(
        { label: 'Chuyển sang đang xử lý', click: () => openUpdateStatusModal(order) },
        { label: 'Hủy đơn hàng', click: () => openUpdateStatusModal(order) }
      )
      break
    case 'Processing':
      actions.push(
        { label: 'Chuyển sang đã giao hàng', click: () => openUpdateStatusModal(order) },
        { label: 'Hủy đơn hàng', click: () => openUpdateStatusModal(order) }
      )
      break
    case 'Shipped':
      actions.push(
        { label: 'Chuyển sang đã nhận hàng', click: () => openUpdateStatusModal(order) }
      )
      break
    case 'Delivered':
      // No further actions
      break
    case 'Cancelled':
      // No further actions
      break
    default:
      actions.push(
        { label: 'Cập nhật trạng thái', click: () => openUpdateStatusModal(order) }
      )
  }
  
  return [actions]
}

// Compatibility with template
const handlePageChange = (page: number): void => {
  changePage(page)
}

// Initialize
onMounted(() => {
  loadOrders()
})
</script> 