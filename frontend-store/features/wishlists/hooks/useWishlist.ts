'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { wishlistService } from '../services/wishlistService';
import { useAuthStore } from '@/stores/authStore';

export function useWishlists() {
  const { isAuthenticated } = useAuthStore();

  return useQuery({
    queryKey: ['wishlists'],
    queryFn: () => wishlistService.getWishlists(),
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000,
  });
}

export function useWishlistStatus(productId: string) {
  const { isAuthenticated } = useAuthStore();

  return useQuery({
    queryKey: ['wishlist', 'status', productId],
    queryFn: () => wishlistService.checkWishlistStatus(productId),
    enabled: isAuthenticated && !!productId,
    staleTime: 60 * 1000,
  });
}

export function useAddToWishlist() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (productId: string) => wishlistService.addToWishlist(productId),
    onSuccess: (_, productId) => {
      queryClient.invalidateQueries({ queryKey: ['wishlists'] });
      queryClient.setQueryData(['wishlist', 'status', productId], {
        isInWishlist: true,
        wishlistIds: [],
      });
      toast.success('Added to wishlist');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to add to wishlist');
    },
  });
}

export function useRemoveFromWishlist() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (productId: string) => wishlistService.removeFromWishlist(productId),
    onSuccess: (_, productId) => {
      queryClient.invalidateQueries({ queryKey: ['wishlists'] });
      queryClient.setQueryData(['wishlist', 'status', productId], {
        isInWishlist: false,
        wishlistIds: [],
      });
      toast.success('Removed from wishlist');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to remove from wishlist');
    },
  });
}

export function useToggleWishlist() {
  const addMutation = useAddToWishlist();
  const removeMutation = useRemoveFromWishlist();

  return {
    toggle: (productId: string, isInWishlist: boolean) => {
      if (isInWishlist) {
        removeMutation.mutate(productId);
      } else {
        addMutation.mutate(productId);
      }
    },
    isLoading: addMutation.isPending || removeMutation.isPending,
  };
}
