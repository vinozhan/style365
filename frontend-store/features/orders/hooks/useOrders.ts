'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { orderService, type CreateOrderInput } from '../services/orderService';
import { useCheckoutStore } from '@/stores/checkoutStore';
import { useCartStore } from '@/stores/cartStore';

export function useMyOrders(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: ['orders', page, pageSize],
    queryFn: () => orderService.getMyOrders(page, pageSize),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

// Alias for convenience
export const useOrders = useMyOrders;

export function useOrder(id: string) {
  return useQuery({
    queryKey: ['order', id],
    queryFn: () => orderService.getOrderById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

// Alias for convenience
export const useOrderById = useOrder;

export function useTrackOrder() {
  return useMutation({
    mutationFn: (orderNumber: string) => orderService.trackOrder(orderNumber, ''),
    onError: (error: Error) => {
      toast.error(error.message || 'Order not found');
    },
  });
}

export function useCreateOrder() {
  const queryClient = useQueryClient();
  const { reset: resetCheckout } = useCheckoutStore();
  const { clearCartOptimistic } = useCartStore();

  return useMutation({
    mutationFn: (data: CreateOrderInput) => orderService.createOrder(data),
    onSuccess: (order) => {
      // Clear cart and checkout state
      clearCartOptimistic();
      resetCheckout();
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      toast.success(`Order ${order.orderNumber} placed successfully!`);
      return order;
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to place order');
    },
  });
}

export function useCancelOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      orderService.cancelOrder(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      toast.success('Order cancelled successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to cancel order');
    },
  });
}
