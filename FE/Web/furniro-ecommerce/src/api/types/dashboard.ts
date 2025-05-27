// Dashboard and Analytics related types

export interface DashboardSummaryDTO {
  totalProducts: number;
  totalOrders: number;
  totalCustomers: number;
  totalRevenue: number;
  recentOrders: RecentOrderDTO[] | null;
  popularProducts: PopularProductDTO[] | null;
  salesByCategory: CategorySalesDTO[] | null;
  orderStatusDistribution: OrderStatusDistributionDTO;
  recentSalesTrend: SalesTrendPointDTO[] | null;
}

export interface RecentOrderDTO {
  orderId: number;
  orderDate: string;
  customerName: string | null;
  totalAmount: number;
  orderStatus: string | null;
}

export interface PopularProductDTO {
  productId: number;
  name: string | null;
  imageUrl: string | null;
  price: number;
  totalSold: number;
  totalRevenue: number;
}

export interface CategorySalesDTO {
  categoryId: number;
  categoryName: string | null;
  totalSales: number;
  totalRevenue: number;
  percentage: number;
}

export interface OrderStatusDistributionDTO {
  pending: number;
  processing: number;
  shipped: number;
  delivered: number;
  cancelled: number;
  total: number;
}

export interface SalesTrendDTO {
  period: string | null;
  startDate: string;
  endDate: string;
  data: SalesTrendPointDTO[] | null;
}

export interface SalesTrendPointDTO {
  date: string;
  revenue: number;
  orderCount: number;
}

// Dashboard query parameters
export interface SalesTrendParams {
  period?: string;
  startDate?: string;
  endDate?: string;
}

export interface PopularProductsParams {
  limit?: number;
}

// Dashboard analytics types for UI
export interface DashboardMetrics {
  totalProducts: number;
  totalOrders: number;
  totalCustomers: number;
  totalRevenue: number;
  growthRates: {
    products: number;
    orders: number;
    customers: number;
    revenue: number;
  };
}

export interface ChartDataPoint {
  label: string;
  value: number;
  color?: string;
}

export interface SalesChartData {
  labels: string[];
  datasets: {
    label: string;
    data: number[];
    backgroundColor?: string;
    borderColor?: string;
  }[];
}
