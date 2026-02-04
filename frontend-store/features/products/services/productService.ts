import { apiClient } from '@/lib/api/client';
import type { Product, PaginatedResponse, ProductFilters, PriceRange } from '@/types';

export const productService = {
  async getProducts(filters: ProductFilters = {}): Promise<PaginatedResponse<Product>> {
    const params = new URLSearchParams();

    if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);
    if (filters.categoryId) params.append('categoryId', filters.categoryId);
    if (filters.minPrice !== undefined) params.append('minPrice', filters.minPrice.toString());
    if (filters.maxPrice !== undefined) params.append('maxPrice', filters.maxPrice.toString());
    if (filters.brand) params.append('brand', filters.brand);
    if (filters.featuredOnly) params.append('featuredOnly', 'true');
    if (filters.tags?.length) filters.tags.forEach((tag) => params.append('tags', tag));
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

    const response = await apiClient.get<PaginatedResponse<Product>>('/products', { params });
    return response.data;
  },

  async getProductById(id: string): Promise<Product> {
    const response = await apiClient.get<Product>(`/products/${id}`);
    return response.data;
  },

  async getProductBySlug(slug: string): Promise<Product> {
    // Assuming the API supports slug-based lookup or we need to search
    const response = await apiClient.get<Product>(`/products/${slug}`);
    return response.data;
  },

  async getProductsByCategory(
    categoryId: string,
    page = 1,
    pageSize = 20
  ): Promise<PaginatedResponse<Product>> {
    const response = await apiClient.get<PaginatedResponse<Product>>(
      `/products/category/${categoryId}`,
      { params: { page, pageSize } }
    );
    return response.data;
  },

  async getFeaturedProducts(limit = 8): Promise<Product[]> {
    const response = await apiClient.get<PaginatedResponse<Product>>('/products/featured', {
      params: { limit },
    });
    return response.data.items;
  },

  async searchProducts(
    searchTerm: string,
    page = 1,
    pageSize = 20
  ): Promise<PaginatedResponse<Product>> {
    const response = await apiClient.get<PaginatedResponse<Product>>('/products/search', {
      params: { searchTerm, page, pageSize },
    });
    return response.data;
  },

  async getSearchSuggestions(query: string, limit = 10): Promise<string[]> {
    const response = await apiClient.get<string[]>('/products/suggestions', {
      params: { query, limit },
    });
    return response.data;
  },

  async getBrands(): Promise<string[]> {
    const response = await apiClient.get<string[]>('/products/brands');
    return response.data;
  },

  async getPriceRange(categoryId?: string): Promise<PriceRange> {
    const response = await apiClient.get<PriceRange>('/products/price-range', {
      params: categoryId ? { categoryId } : {},
    });
    return response.data;
  },
};
