import apiClient from '@/lib/api/client';
import type { Order, PaginatedResponse, OrderStatus } from '@/types';

export interface OrderFilters {
  search?: string;
  status?: OrderStatus;
  dateFrom?: string;
  dateTo?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface UpdateOrderStatusInput {
  status: OrderStatus;
  notes?: string;
}

export interface UpdateTrackingInput {
  trackingNumber: string;
  shippingCarrier?: string;
}

export const orderService = {
  async getOrders(filters: OrderFilters = {}): Promise<PaginatedResponse<Order>> {
    const params: Record<string, string | number | undefined> = {
      pageNumber: filters.pageNumber || 1,
      pageSize: filters.pageSize || 10,
    };

    if (filters.search) params.search = filters.search;
    if (filters.status) params.status = filters.status;
    if (filters.dateFrom) params.dateFrom = filters.dateFrom;
    if (filters.dateTo) params.dateTo = filters.dateTo;
    if (filters.sortBy) params.sortBy = filters.sortBy;
    if (filters.sortOrder) params.sortOrder = filters.sortOrder;

    const response = await apiClient.get<PaginatedResponse<Order>>('/orders', { params });
    return response.data;
  },

  async getOrder(id: string): Promise<Order> {
    const response = await apiClient.get<Order>(`/orders/${id}`);
    return response.data;
  },

  async updateOrderStatus(id: string, data: UpdateOrderStatusInput): Promise<Order> {
    const response = await apiClient.put<Order>(`/orders/${id}/status`, data);
    return response.data;
  },

  async updateTracking(id: string, data: UpdateTrackingInput): Promise<Order> {
    const response = await apiClient.put<Order>(`/orders/${id}/tracking`, data);
    return response.data;
  },
};
