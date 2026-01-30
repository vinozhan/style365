import apiClient from '@/lib/api/client';
import type { Category, PaginatedResponse } from '@/types';

// Backend response format for categories
interface GetCategoriesApiResponse {
  categories: ApiCategoryDto[];
}

interface ApiCategoryDto {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  isActive: boolean;
  sortOrder: number;
  parentCategoryId?: string;
  subCategories?: ApiCategoryDto[];
  productCount: number;
}

export interface CategoryFilters {
  search?: string;
  isActive?: boolean;
  page?: number;
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

export interface CategoryImageUploadResult {
  categoryId: string;
  imageUrl: string;
  thumbnailUrl?: string;
  fileSize: number;
}

// Transform backend DTO to frontend Category type
function mapApiCategoryToCategory(dto: ApiCategoryDto): Category {
  return {
    id: dto.id,
    name: dto.name,
    slug: dto.slug,
    description: dto.description,
    imageUrl: dto.imageUrl,
    isActive: dto.isActive,
    sortOrder: dto.sortOrder,
    parentId: dto.parentCategoryId,
    productsCount: dto.productCount,
    children: dto.subCategories?.map(mapApiCategoryToCategory),
    createdAt: '',
    updatedAt: '',
  };
}

export const categoryService = {
  async getCategories(filters: CategoryFilters = {}): Promise<PaginatedResponse<Category>> {
    const params: Record<string, string | number | boolean | undefined> = {
      activeOnly: filters.isActive ?? false, // Show all categories by default
      includeSubCategories: true,
    };

    const response = await apiClient.get<GetCategoriesApiResponse>('/categories', { params });

    // Transform to PaginatedResponse format expected by frontend
    const items = response.data.categories.map(mapApiCategoryToCategory);

    return {
      items,
      totalItems: items.length,
      page: 1,
      pageSize: items.length,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    };
  },

  async getCategory(id: string): Promise<Category> {
    const response = await apiClient.get<ApiCategoryDto>(`/categories/${id}`);
    return mapApiCategoryToCategory(response.data);
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

  async uploadCategoryImage(
    categoryId: string,
    file: File,
    onProgress?: (progress: number) => void
  ): Promise<CategoryImageUploadResult> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<CategoryImageUploadResult>(
      `/categories/${categoryId}/image`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
        onUploadProgress: (progressEvent) => {
          if (onProgress && progressEvent.total) {
            const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            onProgress(progress);
          }
        },
      }
    );
    return response.data;
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
