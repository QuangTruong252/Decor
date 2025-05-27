import { api } from '../client';
import { DASHBOARD } from '../endpoints';
import type {
  DashboardSummaryDTO,
  SalesTrendDTO,
  PopularProductDTO,
  CategorySalesDTO,
  OrderStatusDistributionDTO,
  SalesTrendParams,
  PopularProductsParams,
} from '../types';

export class DashboardService {
  /**
   * Get dashboard summary with all key metrics
   */
  static async getSummary(): Promise<DashboardSummaryDTO> {
    const response = await api.get<DashboardSummaryDTO>(DASHBOARD.SUMMARY);
    return response.data;
  }

  /**
   * Get sales trend data
   */
  static async getSalesTrend(params?: SalesTrendParams): Promise<SalesTrendDTO> {
    const response = await api.get<SalesTrendDTO>(DASHBOARD.SALES_TREND, { params });
    return response.data;
  }

  /**
   * Get popular products
   */
  static async getPopularProducts(params?: PopularProductsParams): Promise<PopularProductDTO[]> {
    const response = await api.get<PopularProductDTO[]>(DASHBOARD.POPULAR_PRODUCTS, { params });
    return response.data;
  }

  /**
   * Get sales by category
   */
  static async getSalesByCategory(): Promise<CategorySalesDTO[]> {
    const response = await api.get<CategorySalesDTO[]>(DASHBOARD.SALES_BY_CATEGORY);
    return response.data;
  }

  /**
   * Get order status distribution
   */
  static async getOrderStatusDistribution(): Promise<OrderStatusDistributionDTO> {
    const response = await api.get<OrderStatusDistributionDTO>(DASHBOARD.ORDER_STATUS_DISTRIBUTION);
    return response.data;
  }

  /**
   * Get daily sales trend (convenience method)
   */
  static async getDailySalesTrend(startDate?: string, endDate?: string): Promise<SalesTrendDTO> {
    return this.getSalesTrend({
      period: 'daily',
      startDate,
      endDate,
    });
  }

  /**
   * Get weekly sales trend (convenience method)
   */
  static async getWeeklySalesTrend(startDate?: string, endDate?: string): Promise<SalesTrendDTO> {
    return this.getSalesTrend({
      period: 'weekly',
      startDate,
      endDate,
    });
  }

  /**
   * Get monthly sales trend (convenience method)
   */
  static async getMonthlySalesTrend(startDate?: string, endDate?: string): Promise<SalesTrendDTO> {
    return this.getSalesTrend({
      period: 'monthly',
      startDate,
      endDate,
    });
  }

  /**
   * Get top N popular products (convenience method)
   */
  static async getTopProducts(limit: number = 5): Promise<PopularProductDTO[]> {
    return this.getPopularProducts({ limit });
  }
}

export default DashboardService;
