import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  orderService,
  type OrderFilters,
  type UpdateOrderStatusInput,
  type UpdateTrackingInput,
} from '../services/orderService';

export function useOrders(filters: OrderFilters = {}) {
  return useQuery({
    queryKey: ['orders', filters],
    queryFn: () => orderService.getOrders(filters),
    placeholderData: (previousData) => previousData,
  });
}

export function useOrder(id: string) {
  return useQuery({
    queryKey: ['order', id],
    queryFn: () => orderService.getOrder(id),
    enabled: !!id,
  });
}

export function useUpdateOrderStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateOrderStatusInput }) =>
      orderService.updateOrderStatus(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['order', id] });
      toast.success('Order status updated successfully');
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to update order status';
      toast.error(message);
    },
  });
}

export function useUpdateTracking() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTrackingInput }) =>
      orderService.updateTracking(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['order', id] });
      toast.success('Tracking information updated successfully');
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to update tracking';
      toast.error(message);
    },
  });
}
