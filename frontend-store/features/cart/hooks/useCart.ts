'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { cartService, type AddToCartInput } from '../services/cartService';
import { useCartStore } from '@/stores/cartStore';
import { useAuthStore } from '@/stores/authStore';
import { clearSessionId } from '@/lib/api/client';

export function useCart() {
  const { setCart, setLoading } = useCartStore();

  return useQuery({
    queryKey: ['cart'],
    queryFn: async () => {
      setLoading(true);
      try {
        const cart = await cartService.getCart();
        setCart(cart);
        return cart;
      } finally {
        setLoading(false);
      }
    },
    staleTime: 60 * 1000, // 1 minute
  });
}

export function useCartSummary() {
  return useQuery({
    queryKey: ['cart', 'summary'],
    queryFn: () => cartService.getCartSummary(),
    staleTime: 60 * 1000,
  });
}

export function useAddToCart() {
  const queryClient = useQueryClient();
  const { addItemOptimistic, openCart } = useCartStore();

  return useMutation({
    mutationFn: (data: AddToCartInput) => cartService.addToCart(data),
    onSuccess: (cart) => {
      queryClient.setQueryData(['cart'], cart);
      queryClient.invalidateQueries({ queryKey: ['cart', 'summary'] });
      openCart();
      toast.success('Added to cart');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to add to cart');
    },
  });
}

export function useUpdateCartItem() {
  const queryClient = useQueryClient();
  const { updateItemOptimistic } = useCartStore();

  return useMutation({
    mutationFn: ({ itemId, quantity }: { itemId: string; quantity: number }) =>
      cartService.updateCartItem(itemId, quantity),
    onMutate: async ({ itemId, quantity }) => {
      // Optimistic update
      updateItemOptimistic(itemId, quantity);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['cart', 'summary'] });
    },
    onError: (error: Error) => {
      // Refetch to revert optimistic update
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      toast.error(error.message || 'Failed to update cart');
    },
  });
}

export function useRemoveCartItem() {
  const queryClient = useQueryClient();
  const { removeItemOptimistic } = useCartStore();

  return useMutation({
    mutationFn: (itemId: string) => cartService.removeCartItem(itemId),
    onMutate: async (itemId) => {
      removeItemOptimistic(itemId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['cart', 'summary'] });
      toast.success('Item removed from cart');
    },
    onError: (error: Error) => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      toast.error(error.message || 'Failed to remove item');
    },
  });
}

export function useClearCart() {
  const queryClient = useQueryClient();
  const { clearCartOptimistic } = useCartStore();

  return useMutation({
    mutationFn: () => cartService.clearCart(),
    onMutate: async () => {
      clearCartOptimistic();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['cart', 'summary'] });
      toast.success('Cart cleared');
    },
    onError: (error: Error) => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      toast.error(error.message || 'Failed to clear cart');
    },
  });
}

export function useMergeCart() {
  const queryClient = useQueryClient();
  const { setCart } = useCartStore();

  return useMutation({
    mutationFn: (sessionId: string) => cartService.mergeGuestCart(sessionId),
    onSuccess: (cart) => {
      setCart(cart);
      clearSessionId();
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['cart', 'summary'] });
    },
    onError: (error: Error) => {
      console.error('Failed to merge cart:', error);
    },
  });
}
