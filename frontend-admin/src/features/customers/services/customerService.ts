import apiClient from '@/lib/api/client';
import type { Customer, Order, PaginatedResponse } from '@/types';

export interface CustomerFilters {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

interface CustomerApiResponse {
  items: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber?: string;
    isActive: boolean;
    isEmailVerified: boolean;
    ordersCount: number;
    totalSpent: number;
    lastOrderDate?: string;
    lastLoginAt?: string;
    createdAt: string;
  }[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export const customerService = {
  async getCustomers(filters: CustomerFilters = {}): Promise<PaginatedResponse<Customer>> {
    const params: Record<string, string | number | boolean | undefined> = {
      page: filters.pageNumber || 1,
      pageSize: filters.pageSize || 10,
    };

    if (filters.search) params.search = filters.search;
    if (filters.sortBy) params.sortBy = filters.sortBy;
    if (filters.sortOrder) params.ascending = filters.sortOrder === 'asc';

    const response = await apiClient.get<CustomerApiResponse>('/users', { params });
    const data = response.data;

    return {
      items: data.items.map((item) => ({
        id: item.id,
        firstName: item.firstName,
        lastName: item.lastName,
        email: item.email,
        phoneNumber: item.phoneNumber,
        isActive: item.isActive,
        isEmailVerified: item.isEmailVerified,
        ordersCount: item.ordersCount,
        totalSpent: item.totalSpent,
        lastOrderDate: item.lastOrderDate,
        lastLoginAt: item.lastLoginAt,
        createdAt: item.createdAt,
      })),
      pageNumber: data.page,
      pageSize: data.pageSize,
      totalCount: data.totalItems,
      totalPages: data.totalPages,
      hasNextPage: data.page < data.totalPages,
      hasPreviousPage: data.page > 1,
    };
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
