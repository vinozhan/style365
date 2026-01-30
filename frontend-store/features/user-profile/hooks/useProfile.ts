'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { profileService, type UpdateProfileInput } from '../services/profileService';
import { useAuthStore } from '@/stores/authStore';
import type { OrderAddress } from '@/types';

export function useProfile() {
  const { isAuthenticated } = useAuthStore();

  return useQuery({
    queryKey: ['profile'],
    queryFn: () => profileService.getProfile(),
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000,
  });
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateProfileInput) => profileService.updateProfile(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      toast.success('Profile updated successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update profile');
    },
  });
}

export function useAddAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (address: OrderAddress) => profileService.addAddress(address),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      toast.success('Address added successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to add address');
    },
  });
}

export function useUpdateAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ index, address }: { index: number; address: OrderAddress }) =>
      profileService.updateAddress(index, address),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      toast.success('Address updated successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update address');
    },
  });
}

export function useDeleteAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (index: number) => profileService.deleteAddress(index),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      toast.success('Address deleted successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to delete address');
    },
  });
}
