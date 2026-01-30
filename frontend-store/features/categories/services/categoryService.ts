import { apiClient } from '@/lib/api/client';
import type { Category } from '@/types';

export const categoryService = {
  async getCategories(activeOnly = true): Promise<Category[]> {
    const response = await apiClient.get<Category[]>('/categories', {
      params: { activeOnly },
    });
    return response.data;
  },

  async getCategoryById(id: string): Promise<Category> {
    const response = await apiClient.get<Category>(`/categories/${id}`);
    return response.data;
  },

  async getCategoryBySlug(slug: string): Promise<Category> {
    // Get all categories and find by slug
    const categories = await this.getCategories();
    const findBySlug = (cats: Category[]): Category | undefined => {
      for (const cat of cats) {
        if (cat.slug === slug) return cat;
        if (cat.subCategories?.length) {
          const found = findBySlug(cat.subCategories);
          if (found) return found;
        }
      }
      return undefined;
    };
    const category = findBySlug(categories);
    if (!category) throw new Error('Category not found');
    return category;
  },

  async getCategoryTree(): Promise<Category[]> {
    const response = await apiClient.get<Category[]>('/categories/tree');
    return response.data;
  },
};
