import apiClient from '@/lib/api/client';
import type { Customer, Order, PaginatedResponse } from '@/types';

export interface CustomerFilters {
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export const customerService = {
  async getCustomers(filters: CustomerFilters = {}): Promise<PaginatedResponse<Customer>> {
    const params: Record<string, string | number | boolean | undefined> = {
      page: filters.page || 1,
      pageSize: filters.pageSize || 10,
    };

    if (filters.search) params.search = filters.search;
    if (filters.sortBy) params.sortBy = filters.sortBy;
    if (filters.sortOrder) params.ascending = filters.sortOrder === 'asc';

    const response = await apiClient.get<PaginatedResponse<Customer>>('/users', { params });
    return response.data;
  },

  async getCustomer(id: string): Promise<Customer> {
    const response = await apiClient.get<Customer>(`/users/${id}`);
    return response.data;
  },

  async getCustomerOrders(customerId: string): Promise<Order[]> {
    const response = await apiClient.get<PaginatedResponse<Order>>('/orders', {
      params: { userId: customerId, pageSize: 50 },
    });
    return response.data.items;
  },
};
