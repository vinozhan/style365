'use client';

import { useQuery } from '@tanstack/react-query';
import { categoryService } from '../services/categoryService';

export function useCategories(activeOnly = true) {
  return useQuery({
    queryKey: ['categories', { activeOnly }],
    queryFn: () => categoryService.getCategories(activeOnly),
    staleTime: 30 * 60 * 1000, // 30 minutes - categories don't change often
  });
}

export function useCategory(id: string) {
  return useQuery({
    queryKey: ['category', id],
    queryFn: () => categoryService.getCategoryById(id),
    enabled: !!id,
    staleTime: 30 * 60 * 1000,
  });
}

export function useCategoryBySlug(slug: string) {
  return useQuery({
    queryKey: ['category', 'slug', slug],
    queryFn: () => categoryService.getCategoryBySlug(slug),
    enabled: !!slug,
    staleTime: 30 * 60 * 1000,
  });
}

export function useCategoryTree() {
  return useQuery({
    queryKey: ['categories', 'tree'],
    queryFn: () => categoryService.getCategoryTree(),
    staleTime: 30 * 60 * 1000,
  });
}
