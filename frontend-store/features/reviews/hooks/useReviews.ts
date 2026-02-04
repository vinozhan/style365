'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { reviewService, type CreateReviewInput, type GetReviewsParams } from '../services/reviewService';

export function useProductReviews(productId: string, params: GetReviewsParams = {}) {
  return useQuery({
    queryKey: ['reviews', productId, params],
    queryFn: () => reviewService.getProductReviews(productId, params),
    enabled: !!productId,
    staleTime: 5 * 60 * 1000,
  });
}

// Alias with page/pageSize params for convenience
export function useReviews(productId: string, page = 1, pageSize = 10) {
  return useProductReviews(productId, { page, pageSize });
}

export function useReviewStats(productId: string) {
  return useQuery({
    queryKey: ['reviews', 'stats', productId],
    queryFn: () => reviewService.getReviewStats(productId),
    enabled: !!productId,
    staleTime: 10 * 60 * 1000,
  });
}

export function useCreateReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateReviewInput & { productId: string }) =>
      reviewService.createReview(data.productId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['reviews', variables.productId] });
      queryClient.invalidateQueries({ queryKey: ['reviews', 'stats', variables.productId] });
      toast.success('Review submitted successfully!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to submit review');
    },
  });
}

export function useUserReviews() {
  return useQuery({
    queryKey: ['reviews', 'user'],
    queryFn: () => reviewService.getUserReviews(),
    staleTime: 5 * 60 * 1000,
  });
}
