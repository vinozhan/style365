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

export function useReviewStats(productId: string) {
  return useQuery({
    queryKey: ['reviews', 'stats', productId],
    queryFn: () => reviewService.getReviewStats(productId),
    enabled: !!productId,
    staleTime: 10 * 60 * 1000,
  });
}

export function useCreateReview(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateReviewInput) => reviewService.createReview(productId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reviews', productId] });
      queryClient.invalidateQueries({ queryKey: ['reviews', 'stats', productId] });
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
