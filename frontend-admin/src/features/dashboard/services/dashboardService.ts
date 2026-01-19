import apiClient from '@/lib/api/client';
import type { DashboardStats, Order, PaginatedResponse } from '@/types';

export const dashboardService = {
  async getStats(): Promise<DashboardStats> {
    // Since we don't have a dedicated dashboard endpoint yet,
    // we'll aggregate data from multiple endpoints
    try {
      const [ordersResponse, productsResponse] = await Promise.all([
        apiClient.get<PaginatedResponse<Order>>('/orders', { params: { pageSize: 5 } }),
        apiClient.get<{ totalCount: number; lowStockCount: number }>('/products/stats'),
      ]);

      // Calculate stats from responses
      const recentOrders = ordersResponse.data.items || [];
      const totalOrders = ordersResponse.data.totalCount || 0;
      const totalProducts = productsResponse.data?.totalCount || 0;
      const lowStockCount = productsResponse.data?.lowStockCount || 0;

      // Calculate revenue from recent orders (simplified)
      const totalRevenue = recentOrders.reduce((sum, order) => sum + order.totalAmount, 0);

      return {
        totalOrders,
        totalRevenue,
        totalProducts,
        totalCustomers: 0, // Will need backend support
        lowStockCount,
        pendingOrdersCount: recentOrders.filter((o) => o.status === 'Pending').length,
        recentOrders,
        salesByMonth: [], // Will need backend support
      };
    } catch {
      // Return default values if API calls fail
      return {
        totalOrders: 0,
        totalRevenue: 0,
        totalProducts: 0,
        totalCustomers: 0,
        lowStockCount: 0,
        pendingOrdersCount: 0,
        recentOrders: [],
        salesByMonth: [],
      };
    }
  },

  async getRecentOrders(limit = 5): Promise<Order[]> {
    const response = await apiClient.get<PaginatedResponse<Order>>('/orders', {
      params: { pageSize: limit, sortBy: 'createdAt', sortOrder: 'desc' },
    });
    return response.data.items;
  },
};
