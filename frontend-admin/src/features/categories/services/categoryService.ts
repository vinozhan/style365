import apiClient from '@/lib/api/client';
import type { Category, PaginatedResponse } from '@/types';

export interface CategoryFilters {
  search?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateCategoryInput {
  name: string;
  slug?: string;
  description?: string;
  parentId?: string;
  imageUrl?: string;
  isActive: boolean;
  sortOrder: number;
}

export interface UpdateCategoryInput extends CreateCategoryInput {
  id: string;
}

export const categoryService = {
  async getCategories(filters: CategoryFilters = {}): Promise<PaginatedResponse<Category>> {
    const params: Record<string, string | number | boolean | undefined> = {
      pageNumber: filters.pageNumber || 1,
      pageSize: filters.pageSize || 100,
    };

    if (filters.search) params.search = filters.search;
    if (filters.isActive !== undefined) params.isActive = filters.isActive;

    const response = await apiClient.get<PaginatedResponse<Category>>('/categories', { params });
    return response.data;
  },

  async getCategory(id: string): Promise<Category> {
    const response = await apiClient.get<Category>(`/categories/${id}`);
    return response.data;
  },

  async createCategory(data: CreateCategoryInput): Promise<Category> {
    const response = await apiClient.post<Category>('/categories', data);
    return response.data;
  },

  async updateCategory(id: string, data: UpdateCategoryInput): Promise<Category> {
    const response = await apiClient.put<Category>(`/categories/${id}`, data);
    return response.data;
  },

  async deleteCategory(id: string): Promise<void> {
    await apiClient.delete(`/categories/${id}`);
  },
};

// Helper to build tree structure from flat list
export function buildCategoryTree(categories: Category[] | undefined | null): Category[] {
  if (!categories || categories.length === 0) {
    return [];
  }

  const map = new Map<string, Category>();
  const roots: Category[] = [];

  // Create a map and initialize children arrays
  categories.forEach((cat) => {
    map.set(cat.id, { ...cat, children: [] });
  });

  // Build tree
  categories.forEach((cat) => {
    const node = map.get(cat.id)!;
    if (cat.parentId && map.has(cat.parentId)) {
      const parent = map.get(cat.parentId)!;
      parent.children = parent.children || [];
      parent.children.push(node);
    } else {
      roots.push(node);
    }
  });

  return roots;
}
