import apiClient from '@/lib/api/client';
import type { DashboardStats } from '@/types';

interface DashboardStatsResponse {
  totalOrders: number;
  totalRevenue: number;
  totalProducts: number;
  lowStockProducts: number;
  totalCustomers: number;
  pendingOrders: number;
  processingOrders: number;
  completedOrders: number;
  recentOrders: {
    id: string;
    orderNumber: string;
    customerName: string;
    customerEmail: string;
    totalAmount: number;
    status: string;
    createdAt: string;
  }[];
  topProducts: {
    id: string;
    name: string;
    imageUrl?: string;
    totalSold: number;
    revenue: number;
  }[];
}

export const dashboardService = {
  async getStats(): Promise<DashboardStats> {
    try {
      const response = await apiClient.get<DashboardStatsResponse>('/dashboard/stats');
      const data = response.data;

      return {
        totalOrders: data.totalOrders,
        totalRevenue: data.totalRevenue,
        totalProducts: data.totalProducts,
        totalCustomers: data.totalCustomers,
        lowStockCount: data.lowStockProducts,
        pendingOrdersCount: data.pendingOrders,
        recentOrders: data.recentOrders.map((order) => ({
          id: order.id,
          orderNumber: order.orderNumber,
          customerName: order.customerName,
          customerEmail: order.customerEmail,
          totalAmount: order.totalAmount,
          status: order.status,
          createdAt: order.createdAt,
        })),
        topProducts: data.topProducts,
        salesByMonth: [],
      };
    } catch {
      return {
        totalOrders: 0,
        totalRevenue: 0,
        totalProducts: 0,
        totalCustomers: 0,
        lowStockCount: 0,
        pendingOrdersCount: 0,
        recentOrders: [],
        topProducts: [],
        salesByMonth: [],
      };
    }
  },
};
