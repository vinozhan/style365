import { apiClient } from '@/lib/api/client';
import type { Order, PaginatedResponse, OrderAddress } from '@/types';

export interface CreateOrderInput {
  shippingAddress: OrderAddress;
  billingAddress: OrderAddress;
  customerEmail: string;
  customerPhone?: string;
  notes?: string;
  paymentMethod: string;
}

export const orderService = {
  async createOrder(data: CreateOrderInput): Promise<Order> {
    const response = await apiClient.post<Order>('/orders', data);
    return response.data;
  },

  async getMyOrders(page = 1, pageSize = 10): Promise<PaginatedResponse<Order>> {
    const response = await apiClient.get<PaginatedResponse<Order>>('/orders', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async getOrderById(id: string): Promise<Order> {
    const response = await apiClient.get<Order>(`/orders/${id}`);
    return response.data;
  },

  async trackOrder(orderNumber: string, email: string): Promise<Order> {
    const response = await apiClient.get<Order>(`/orders/track/${orderNumber}`, {
      params: { email },
    });
    return response.data;
  },

  async cancelOrder(id: string, reason: string): Promise<void> {
    await apiClient.post(`/orders/${id}/cancel`, { reason });
  },
};
