import { useQuery } from '@tanstack/react-query';
import { customerService, type CustomerFilters } from '../services/customerService';

export function useCustomers(filters: CustomerFilters = {}) {
  return useQuery({
    queryKey: ['customers', filters],
    queryFn: () => customerService.getCustomers(filters),
    placeholderData: (previousData) => previousData,
  });
}

export function useCustomer(id: string) {
  return useQuery({
    queryKey: ['customer', id],
    queryFn: () => customerService.getCustomer(id),
    enabled: !!id,
  });
}

export function useCustomerOrders(customerId: string) {
  return useQuery({
    queryKey: ['customer', customerId, 'orders'],
    queryFn: () => customerService.getCustomerOrders(customerId),
    enabled: !!customerId,
  });
}
