import { create } from 'zustand';
import { persist, createJSONStorage, devtools } from 'zustand/middleware';
import { OrderService } from '@/api/services';
import type {
  OrderDTO,
  CreateOrderDTO,
  OrderFormData,
  OrderStatus,
  PaymentMethod,
  OrderFilters,
  OrderSummary,
  Order
} from '@/api/types';

// Order state interface
interface OrderState {
  // Current orders
  orders: OrderDTO[];
  currentOrder: Order | null;

  // Loading states
  isLoading: boolean;
  isCreating: boolean;
  isUpdating: boolean;

  // Error handling
  error: string | null;

  // Pagination
  currentPage: number;
  totalPages: number;
  totalItems: number;

  // Filters and search
  filters: OrderFilters;
  searchQuery: string;

  // Cache
  cache: {
    orders: Record<string, { data: OrderDTO[]; timestamp: number }>;
    orderDetails: Record<number, { data: Order; timestamp: number }>;
  };

  // Last updated timestamp
  lastUpdated: number | null;
}

// Order actions interface
interface OrderActions {
  // Order management
  createOrder: (orderData: OrderFormData) => Promise<OrderDTO>;
  fetchOrders: (params?: OrderFilters) => Promise<void>;
  fetchOrderById: (id: number) => Promise<void>;
  fetchUserOrders: (userId?: number) => Promise<void>;
  updateOrderStatus: (id: number, status: OrderStatus) => Promise<void>;
  cancelOrder: (id: number) => Promise<void>;

  // Search and filter
  setSearchQuery: (query: string) => void;
  setFilters: (filters: Partial<OrderFilters>) => void;
  clearFilters: () => void;
  searchOrders: (query: string) => Promise<void>;

  // Pagination
  setPage: (page: number) => void;
  loadMore: () => Promise<void>;

  // Selectors
  getOrderById: (id: number) => OrderDTO | null;
  getOrdersByStatus: (status: OrderStatus) => OrderDTO[];
  getRecentOrders: (limit?: number) => OrderDTO[];

  // Utilities
  setLoading: (loading: boolean) => void;
  setCreating: (creating: boolean) => void;
  setUpdating: (updating: boolean) => void;
  setError: (error: string | null) => void;
  clearError: () => void;

  // Cache management
  clearCache: () => void;

  // Reset state
  reset: () => void;
}

type OrderStore = OrderState & OrderActions;

const CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

const initialState: OrderState = {
  orders: [],
  currentOrder: null,
  isLoading: false,
  isCreating: false,
  isUpdating: false,
  error: null,
  currentPage: 1,
  totalPages: 1,
  totalItems: 0,
  filters: {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'orderDate',
    sortDirection: 'desc'
  },
  searchQuery: '',
  cache: {
    orders: {},
    orderDetails: {}
  },
  lastUpdated: null
};

// Helper function to check if cache is valid
const isCacheValid = (timestamp: number): boolean => {
  return Date.now() - timestamp < CACHE_DURATION;
};

// Helper function to generate cache key
const generateCacheKey = (params: OrderFilters): string => {
  return JSON.stringify(params);
};

export const useOrderStore = create<OrderStore>()(
  devtools(
    persist(
      (set, get) => ({
        ...initialState,

        // Create new order
        createOrder: async (orderData: OrderFormData) => {
          const { setCreating, setError } = get();

          try {
            setCreating(true);
            setError(null);

            // Transform form data to CreateOrderDTO
            const createOrderData: CreateOrderDTO = {
              userId: 1, // This should come from auth context
              paymentMethod: orderData.paymentMethod,
              shippingAddress: JSON.stringify(orderData.shippingAddress),
              orderItems: [] // This should come from cart
            };

            const newOrder = await OrderService.createOrder(createOrderData);

            // Add to orders list
            set(state => ({
              orders: [newOrder, ...state.orders],
              totalItems: state.totalItems + 1,
              isCreating: false,
              lastUpdated: Date.now()
            }));

            // Clear cache
            get().clearCache();

            return newOrder;
          } catch (error) {
            console.error('Failed to create order:', error);
            setError('Failed to create order');
            setCreating(false);
            throw error;
          }
        },

        // Fetch orders with caching
        fetchOrders: async (params?: OrderFilters) => {
          const { setLoading, setError, cache } = get();
          const searchParams = { ...get().filters, ...params };
          const cacheKey = generateCacheKey(searchParams);

          // Check cache first
          const cachedData = cache.orders[cacheKey];
          if (cachedData && isCacheValid(cachedData.timestamp)) {
            set({
              orders: cachedData.data,
              isLoading: false
            });
            return;
          }

          try {
            setLoading(true);
            setError(null);

            const ordersResult = await OrderService.getOrders(searchParams);

            set(state => ({
              orders: ordersResult.items,
              currentPage: ordersResult.pagination.currentPage,
              totalPages: ordersResult.pagination.totalPages,
              totalItems: ordersResult.pagination.totalCount,
              isLoading: false,
              lastUpdated: Date.now(),
              cache: {
                ...state.cache,
                orders: {
                  ...state.cache.orders,
                  [cacheKey]: {
                    data: ordersResult.items,
                    timestamp: Date.now()
                  }
                }
              }
            }));
          } catch (error) {
            console.error('Failed to fetch orders:', error);
            setError('Failed to load orders');
            setLoading(false);
          }
        },

        // Fetch order by ID with caching
        fetchOrderById: async (id: number) => {
          const { setLoading, setError, cache } = get();

          // Check cache first
          const cachedData = cache.orderDetails[id];
          if (cachedData && isCacheValid(cachedData.timestamp)) {
            set({
              currentOrder: cachedData.data,
              isLoading: false
            });
            return;
          }

          try {
            setLoading(true);
            setError(null);

            const orderDTO = await OrderService.getOrderById(id);

            // Transform to Order type with additional properties
            const order: Order = {
              ...orderDTO,
              statusHistory: [],
              paymentDetails: {
                method: orderDTO.paymentMethod as PaymentMethod,
                status: 'completed',
                amount: orderDTO.totalAmount,
                currency: 'USD'
              },
              shippingDetails: {
                address: orderDTO.shippingAddress || '',
                city: orderDTO.shippingCity || '',
                state: orderDTO.shippingState || '',
                postalCode: orderDTO.shippingPostalCode || '',
                country: orderDTO.shippingCountry || ''
              },
              canCancel: OrderService.canCancelOrder(orderDTO),
              canReturn: OrderService.canReturnOrder(orderDTO)
            };

            set(state => ({
              currentOrder: order,
              isLoading: false,
              lastUpdated: Date.now(),
              cache: {
                ...state.cache,
                orderDetails: {
                  ...state.cache.orderDetails,
                  [id]: {
                    data: order,
                    timestamp: Date.now()
                  }
                }
              }
            }));
          } catch (error) {
            console.error('Failed to fetch order:', error);
            setError('Failed to load order details');
            setLoading(false);
          }
        },

        // Fetch user orders
        fetchUserOrders: async (userId?: number) => {
          const { setLoading, setError } = get();

          try {
            setLoading(true);
            setError(null);

            const orders = userId
              ? await OrderService.getOrdersByUserId(userId)
              : await OrderService.getCurrentUserOrders();

            set({
              orders,
              isLoading: false,
              lastUpdated: Date.now()
            });
          } catch (error) {
            console.error('Failed to fetch user orders:', error);
            setError('Failed to load orders');
            setLoading(false);
          }
        },

        // Update order status
        updateOrderStatus: async (id: number, status: OrderStatus) => {
          const { setUpdating, setError } = get();

          try {
            setUpdating(true);
            setError(null);

            await OrderService.updateOrderStatus(id, { orderStatus: status });

            // Update local state
            set(state => ({
              orders: state.orders.map(order =>
                order.id === id ? { ...order, orderStatus: status } : order
              ),
              currentOrder: state.currentOrder?.id === id
                ? { ...state.currentOrder, orderStatus: status }
                : state.currentOrder,
              isUpdating: false,
              lastUpdated: Date.now()
            }));

            // Clear cache
            get().clearCache();
          } catch (error) {
            console.error('Failed to update order status:', error);
            setError('Failed to update order status');
            setUpdating(false);
            throw error;
          }
        },

        // Cancel order
        cancelOrder: async (id: number) => {
          await get().updateOrderStatus(id, 'cancelled');
        },

        // Search and filter methods
        setSearchQuery: (query: string) => {
          set({ searchQuery: query });
        },

        setFilters: (filters: Partial<OrderFilters>) => {
          set(state => ({
            filters: { ...state.filters, ...filters }
          }));
        },

        clearFilters: () => {
          set({
            filters: initialState.filters,
            searchQuery: ''
          });
        },

        searchOrders: async (query: string) => {
          const { setLoading, setError } = get();

          try {
            setLoading(true);
            setError(null);

            const orders = await OrderService.searchOrders(query);

            set({
              orders,
              searchQuery: query,
              isLoading: false,
              lastUpdated: Date.now()
            });
          } catch (error) {
            console.error('Failed to search orders:', error);
            setError('Failed to search orders');
            setLoading(false);
          }
        },

        // Pagination methods
        setPage: (page: number) => {
          set(state => ({
            currentPage: page,
            filters: { ...state.filters, pageNumber: page }
          }));
        },

        loadMore: async () => {
          const { currentPage, filters } = get();
          const nextPage = currentPage + 1;

          await get().fetchOrders({ ...filters, pageNumber: nextPage });
          get().setPage(nextPage);
        },

        // Selectors
        getOrderById: (id: number) => {
          const { orders } = get();
          return orders.find(order => order.id === id) || null;
        },

        getOrdersByStatus: (status: OrderStatus) => {
          const { orders } = get();
          return orders.filter(order => order.orderStatus === status);
        },

        getRecentOrders: (limit: number = 5) => {
          const { orders } = get();
          return orders
            .sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())
            .slice(0, limit);
        },

        // Utility methods
        setLoading: (loading: boolean) => set({ isLoading: loading }),
        setCreating: (creating: boolean) => set({ isCreating: creating }),
        setUpdating: (updating: boolean) => set({ isUpdating: updating }),
        setError: (error: string | null) => set({ error }),
        clearError: () => set({ error: null }),

        // Cache management
        clearCache: () => {
          set({
            cache: {
              orders: {},
              orderDetails: {}
            }
          });
        },

        // Reset state
        reset: () => {
          set(initialState);
        }
      }),
      {
        name: 'order-storage',
        storage: createJSONStorage(() => localStorage),
        partialize: (state) => ({
          orders: state.orders,
          currentOrder: state.currentOrder,
          lastUpdated: state.lastUpdated,
          cache: state.cache
        }),
      }
    ),
    { name: 'OrderStore' }
  )
);

export default useOrderStore;
