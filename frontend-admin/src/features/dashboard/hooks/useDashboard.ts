import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';

export function useDashboardStats() {
  return useQuery({
    queryKey: ['dashboard', 'stats'],
    queryFn: () => dashboardService.getStats(),
    staleTime: 60 * 1000, // 1 minute
    refetchInterval: 5 * 60 * 1000, // Refresh every 5 minutes
  });
}

export function useRecentOrders(limit = 5) {
  return useQuery({
    queryKey: ['dashboard', 'recentOrders', limit],
    queryFn: () => dashboardService.getRecentOrders(limit),
    staleTime: 60 * 1000,
  });
}
