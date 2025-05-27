import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import { ProductDTO, CategoryDTO, ProductSearchParams } from '@/api/types';
import { ProductService, CategoryService } from '@/api/services';

interface ProductState {
  // Products
  products: ProductDTO[];
  featuredProducts: ProductDTO[];
  recentlyViewed: ProductDTO[];

  // Categories
  categories: CategoryDTO[];

  // Search & Filters
  searchQuery: string;
  filters: ProductSearchParams;

  // Pagination
  currentPage: number;
  totalPages: number;
  totalItems: number;

  // Loading states
  isLoading: boolean;
  isLoadingMore: boolean;

  // Error states
  error: string | null;

  // Cache
  cache: {
    products: { [key: string]: { data: ProductDTO[]; timestamp: number } };
    categories: { data: CategoryDTO[]; timestamp: number } | null;
  };
}

interface ProductActions {
  // Product actions
  fetchProducts: (params?: ProductSearchParams) => Promise<void>;
  fetchFeaturedProducts: () => Promise<void>;
  fetchProductById: (id: number) => Promise<ProductDTO | null>;
  fetchProductBySlug: (slug: string) => Promise<ProductDTO | null>;
  searchProducts: (query: string, params?: ProductSearchParams) => Promise<void>;

  // Category actions
  fetchCategories: () => Promise<void>;
  fetchCategoryById: (id: number) => Promise<CategoryDTO | null>;

  // Filter actions
  setFilters: (filters: Partial<ProductSearchParams>) => void;
  clearFilters: () => void;
  setSearchQuery: (query: string) => void;

  // Pagination actions
  setCurrentPage: (page: number) => void;
  loadMoreProducts: () => Promise<void>;

  // Recently viewed
  addToRecentlyViewed: (product: ProductDTO) => void;

  // Cache management
  clearCache: () => void;

  // Reset state
  reset: () => void;
}

type ProductStore = ProductState & ProductActions;

const CACHE_DURATION = 5 * 60 * 1000; // 5 minutes
const MAX_RECENTLY_VIEWED = 10;

const initialState: ProductState = {
  products: [],
  featuredProducts: [],
  recentlyViewed: [],
  categories: [],
  searchQuery: '',
  filters: {
    page: 1,
    limit: 12,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  },
  currentPage: 1,
  totalPages: 1,
  totalItems: 0,
  isLoading: false,
  isLoadingMore: false,
  error: null,
  cache: {
    products: {},
    categories: null
  }
};

const useProductStore = create<ProductStore>()(
  devtools(
    persist(
      (set, get) => ({
        ...initialState,

        // Product actions
        fetchProducts: async (params) => {
          const state = get();
          const searchParams = { ...state.filters, ...params };
          const cacheKey = JSON.stringify(searchParams);

          // Check cache first
          const cached = state.cache.products[cacheKey];
          if (cached && Date.now() - cached.timestamp < CACHE_DURATION) {
            set({
              products: cached.data,
              isLoading: false,
              error: null
            });
            return;
          }

          set({ isLoading: true, error: null });

          try {
            const productsResult = await ProductService.getProducts(searchParams);

            // Update cache
            set((state) => ({
              products: productsResult.items,
              currentPage: productsResult.pagination.currentPage,
              totalPages: productsResult.pagination.totalPages,
              totalItems: productsResult.pagination.totalCount,
              filters: searchParams,
              isLoading: false,
              cache: {
                ...state.cache,
                products: {
                  ...state.cache.products,
                  [cacheKey]: { data: productsResult.items, timestamp: Date.now() }
                }
              }
            }));
          } catch (error) {
            set({
              error: 'Failed to fetch products',
              isLoading: false
            });
          }
        },

        fetchFeaturedProducts: async () => {
          const state = get();

          // Check if we already have featured products
          if (state.featuredProducts.length > 0) return;

          try {
            const products = await ProductService.getFeaturedProducts();
            set({ featuredProducts: products });
          } catch (error) {
            console.error('Failed to fetch featured products:', error);
          }
        },

        fetchProductById: async (id) => {
          try {
            const product = await ProductService.getProductById(id);
            return product;
          } catch (error) {
            console.error('Failed to fetch product:', error);
            return null;
          }
        },

        fetchProductBySlug: async (slug) => {
          try {
            // For now, search by slug since we don't have a direct getBySlug method
            const productsResult = await ProductService.searchProducts(slug);
            return productsResult.items.find((p: ProductDTO) => p.slug === slug) || null;
          } catch (error) {
            console.error('Failed to fetch product by slug:', error);
            return null;
          }
        },

        searchProducts: async (query, params) => {
          const state = get();
          const searchParams = { ...state.filters, ...params, query };

          set({ isLoading: true, error: null, searchQuery: query });

          try {
            const productsResult = await ProductService.searchProducts(query, searchParams);
            set({
              products: productsResult.items,
              currentPage: productsResult.pagination.currentPage,
              totalPages: productsResult.pagination.totalPages,
              totalItems: productsResult.pagination.totalCount,
              filters: searchParams,
              isLoading: false
            });
          } catch (error) {
            set({
              error: 'Failed to search products',
              isLoading: false
            });
          }
        },

        // Category actions
        fetchCategories: async () => {
          const state = get();

          // Check cache first
          if (state.cache.categories && Date.now() - state.cache.categories.timestamp < CACHE_DURATION) {
            set({ categories: state.cache.categories.data });
            return;
          }

          try {
            const categoriesResult = await CategoryService.getCategories();
            set({
              categories: categoriesResult.items,
              cache: {
                ...state.cache,
                categories: { data: categoriesResult.items, timestamp: Date.now() }
              }
            });
          } catch (error) {
            console.error('Failed to fetch categories:', error);
          }
        },

        fetchCategoryById: async (id) => {
          try {
            const category = await CategoryService.getCategoryById(id);
            return category;
          } catch (error) {
            console.error('Failed to fetch category:', error);
            return null;
          }
        },

        // Filter actions
        setFilters: (newFilters) => {
          set((state) => ({
            filters: { ...state.filters, ...newFilters, page: 1 },
            currentPage: 1
          }));
        },

        clearFilters: () => {
          set({
            filters: {
              page: 1,
              limit: 12,
              sortBy: 'createdAt',
              sortOrder: 'desc'
            },
            searchQuery: '',
            currentPage: 1
          });
        },

        setSearchQuery: (query) => {
          set({ searchQuery: query });
        },

        // Pagination actions
        setCurrentPage: (page) => {
          set((state) => ({
            currentPage: page,
            filters: { ...state.filters, page }
          }));
        },

        loadMoreProducts: async () => {
          const state = get();
          if (state.isLoadingMore || state.currentPage >= state.totalPages) return;

          set({ isLoadingMore: true });

          try {
            const nextPage = state.currentPage + 1;
            const searchParams = { ...state.filters, page: nextPage };
            const newProductsResult = await ProductService.getProducts(searchParams);

            set((state) => ({
              products: [...state.products, ...newProductsResult.items],
              currentPage: nextPage,
              totalPages: newProductsResult.pagination.totalPages,
              totalItems: newProductsResult.pagination.totalCount,
              filters: { ...state.filters, page: nextPage },
              isLoadingMore: false
            }));
          } catch (error) {
            set({
              error: 'Failed to load more products',
              isLoadingMore: false
            });
          }
        },

        // Recently viewed
        addToRecentlyViewed: (product) => {
          set((state) => {
            const filtered = state.recentlyViewed.filter(p => p.id !== product.id);
            const updated = [product, ...filtered].slice(0, MAX_RECENTLY_VIEWED);

            // Also save to localStorage
            try {
              localStorage.setItem('recentlyViewed', JSON.stringify(updated.map(p => p.id)));
            } catch (error) {
              console.error('Failed to save recently viewed:', error);
            }

            return { recentlyViewed: updated };
          });
        },

        // Cache management
        clearCache: () => {
          set({
            cache: {
              products: {},
              categories: null
            }
          });
        },

        // Reset state
        reset: () => {
          set(initialState);
        }
      }),
      {
        name: 'product-store',
        partialize: (state) => ({
          recentlyViewed: state.recentlyViewed,
          featuredProducts: state.featuredProducts,
          categories: state.categories
        })
      }
    ),
    { name: 'product-store' }
  )
);

export default useProductStore;
