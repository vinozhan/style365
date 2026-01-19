import apiClient from '@/lib/api/client';
import type { Product, PaginatedResponse, Category } from '@/types';

export interface ProductFilters {
  search?: string;
  categoryId?: string;
  isActive?: boolean;
  stockStatus?: 'all' | 'inStock' | 'lowStock' | 'outOfStock';
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface CreateProductInput {
  name: string;
  description?: string;
  shortDescription?: string;
  sku: string;
  price: number;
  compareAtPrice?: number;
  costPrice?: number;
  stockQuantity: number;
  lowStockThreshold: number;
  isActive: boolean;
  isFeatured: boolean;
  categoryId?: string;
  tags: string[];
  metaTitle?: string;
  metaDescription?: string;
}

export interface UpdateProductInput extends CreateProductInput {
  id: string;
}

export const productService = {
  async getProducts(filters: ProductFilters = {}): Promise<PaginatedResponse<Product>> {
    const params: Record<string, string | number | boolean | undefined> = {
      pageNumber: filters.pageNumber || 1,
      pageSize: filters.pageSize || 10,
    };

    if (filters.search) params.search = filters.search;
    if (filters.categoryId) params.categoryId = filters.categoryId;
    if (filters.isActive !== undefined) params.isActive = filters.isActive;
    if (filters.sortBy) params.sortBy = filters.sortBy;
    if (filters.sortOrder) params.sortOrder = filters.sortOrder;

    // Handle stock status filter
    if (filters.stockStatus === 'outOfStock') {
      params.maxStock = 0;
    } else if (filters.stockStatus === 'lowStock') {
      params.minStock = 1;
      params.maxStock = 10;
    } else if (filters.stockStatus === 'inStock') {
      params.minStock = 11;
    }

    const response = await apiClient.get<PaginatedResponse<Product>>('/products', { params });
    return response.data;
  },

  async getProduct(id: string): Promise<Product> {
    const response = await apiClient.get<Product>(`/products/${id}`);
    return response.data;
  },

  async createProduct(data: CreateProductInput): Promise<Product> {
    const response = await apiClient.post<Product>('/products', data);
    return response.data;
  },

  async updateProduct(id: string, data: UpdateProductInput): Promise<Product> {
    const response = await apiClient.put<Product>(`/products/${id}`, data);
    return response.data;
  },

  async deleteProduct(id: string): Promise<void> {
    await apiClient.delete(`/products/${id}`);
  },

  async getCategories(): Promise<Category[]> {
    const response = await apiClient.get<PaginatedResponse<Category>>('/categories', {
      params: { pageSize: 100 },
    });
    return response.data.items;
  },
};
