'use client';

import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { productService } from '../services/productService';
import type { ProductFilters } from '@/types';

export function useProducts(filters: ProductFilters = {}) {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: () => productService.getProducts(filters),
    placeholderData: keepPreviousData,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useProduct(id: string) {
  return useQuery({
    queryKey: ['product', id],
    queryFn: () => productService.getProductById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

export function useFeaturedProducts(limit = 8) {
  return useQuery({
    queryKey: ['products', 'featured', limit],
    queryFn: () => productService.getFeaturedProducts(limit),
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

export function useProductsByCategory(categoryId: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['products', 'category', categoryId, page, pageSize],
    queryFn: () => productService.getProductsByCategory(categoryId, page, pageSize),
    enabled: !!categoryId,
    placeholderData: keepPreviousData,
    staleTime: 5 * 60 * 1000,
  });
}

export function useProductSearch(searchTerm: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['products', 'search', searchTerm, page, pageSize],
    queryFn: () => productService.searchProducts(searchTerm, page, pageSize),
    enabled: searchTerm.length >= 2,
    placeholderData: keepPreviousData,
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function useSearchSuggestions(query: string, limit = 10) {
  return useQuery({
    queryKey: ['products', 'suggestions', query],
    queryFn: () => productService.getSearchSuggestions(query, limit),
    enabled: query.length >= 2,
    staleTime: 30 * 1000,
  });
}

export function useBrands() {
  return useQuery({
    queryKey: ['products', 'brands'],
    queryFn: () => productService.getBrands(),
    staleTime: 30 * 60 * 1000, // 30 minutes
  });
}

export function usePriceRange(categoryId?: string) {
  return useQuery({
    queryKey: ['products', 'price-range', categoryId],
    queryFn: () => productService.getPriceRange(categoryId),
    staleTime: 30 * 60 * 1000,
  });
}
