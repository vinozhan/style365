import apiClient from '@/lib/api/client';
import type { Product, PaginatedResponse, Category } from '@/types';

export interface ProductFilters {
  search?: string;
  categoryId?: string;
  isActive?: boolean;
  stockStatus?: 'all' | 'inStock' | 'lowStock' | 'outOfStock';
  page?: number;
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

export interface UploadedImage {
  imageId: string;
  url: string;
  thumbnailSmallUrl?: string;
  thumbnailMediumUrl?: string;
  thumbnailLargeUrl?: string;
  webPUrl?: string;
  width: number;
  height: number;
  fileSize: number;
  originalFileName?: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface ImageUploadResult {
  productId: string;
  uploadedImages: UploadedImage[];
  failedUploads: { fileName: string; error: string }[];
  totalUploaded: number;
  totalFailed: number;
}

export interface BulkImportResult {
  totalRows: number;
  successCount: number;
  skippedCount: number;
  errorCount: number;
  validateOnly: boolean;
  importedProducts: {
    rowNumber: number;
    productId?: string;
    name: string;
    sku: string;
    variantsCreated: number;
  }[];
  errors: {
    rowNumber: number;
    sku: string;
    name: string;
    errors: string[];
  }[];
  skippedRows: {
    rowNumber: number;
    sku: string;
    reason: string;
  }[];
}

export const productService = {
  async getProducts(filters: ProductFilters = {}): Promise<PaginatedResponse<Product>> {
    const params: Record<string, string | number | boolean | undefined> = {
      page: filters.page || 1,
      pageSize: filters.pageSize || 10,
    };

    if (filters.search) params.searchTerm = filters.search;
    if (filters.categoryId) params.categoryId = filters.categoryId;
    if (filters.isActive !== undefined) params.isActive = filters.isActive;
    if (filters.sortBy) params.sortBy = filters.sortBy;
    if (filters.sortOrder) params.ascending = filters.sortOrder === 'asc';

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
    // Backend returns { categories: [...] } not { items: [...] }
    const response = await apiClient.get<{ categories: Category[] }>('/categories', {
      params: { pageSize: 100, activeOnly: false, includeSubCategories: true },
    });
    return response.data.categories;
  },

  // Image Upload APIs
  async uploadImages(
    productId: string,
    files: File[],
    altText?: string,
    onProgress?: (progress: number) => void
  ): Promise<ImageUploadResult> {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));
    if (altText) formData.append('altText', altText);

    const response = await apiClient.post<ImageUploadResult>(
      `/products/${productId}/images`,
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

  async deleteImage(productId: string, imageId: string): Promise<void> {
    await apiClient.delete(`/products/${productId}/images/${imageId}`);
  },

  async setPrimaryImage(productId: string, imageId: string): Promise<void> {
    await apiClient.put(`/products/${productId}/images/${imageId}/set-primary`);
  },

  // CSV Bulk Import APIs
  async importFromCsv(
    file: File,
    validateOnly = false,
    skipDuplicates = true
  ): Promise<BulkImportResult> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<BulkImportResult>(
      `/products/import/csv?validateOnly=${validateOnly}&skipDuplicates=${skipDuplicates}`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return response.data;
  },

  getImportTemplateUrl(): string {
    return `${apiClient.defaults.baseURL}/products/import/template`;
  },
};
