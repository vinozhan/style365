'use client';

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { authService, type LoginInput, type RegisterInput } from '../services/authService';
import { useAuthStore } from '@/stores/authStore';
import { useMergeCart } from '@/features/cart/hooks/useCart';
import { mapUserFromApi } from '@/types';

export function useLogin() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { setAuth } = useAuthStore();
  const { mutate: mergeCart } = useMergeCart();

  return useMutation({
    mutationFn: (data: LoginInput) => authService.login(data),
    onSuccess: (response) => {
      const user = mapUserFromApi(response.user);
      setAuth(user, response.accessToken, response.refreshToken);

      // Set cookie for middleware
      document.cookie = `accessToken=${response.accessToken}; path=/; max-age=${response.expiresIn}`;

      // Try to merge guest cart
      const sessionId = localStorage.getItem('sessionId');
      if (sessionId) {
        mergeCart(sessionId);
      }

      queryClient.invalidateQueries({ queryKey: ['cart'] });
      toast.success('Welcome back!');
      router.push('/');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Login failed');
    },
  });
}

export function useRegister() {
  const router = useRouter();

  return useMutation({
    mutationFn: (data: RegisterInput) => authService.register(data),
    onSuccess: (response) => {
      if (response.requiresEmailConfirmation) {
        toast.success('Registration successful! Please check your email to confirm your account.');
        router.push('/confirm-email');
      } else {
        toast.success('Registration successful!');
        router.push('/login');
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Registration failed');
    },
  });
}

export function useLogout() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { logout } = useAuthStore();

  return useMutation({
    mutationFn: () => authService.logout(),
    onSuccess: () => {
      logout();
      document.cookie = 'accessToken=; path=/; max-age=0';
      queryClient.clear();
      toast.success('Logged out successfully');
      router.push('/');
    },
    onError: () => {
      // Still logout on error
      logout();
      document.cookie = 'accessToken=; path=/; max-age=0';
      queryClient.clear();
      router.push('/');
    },
  });
}

export function useForgotPassword() {
  return useMutation({
    mutationFn: (email: string) => authService.forgotPassword(email),
    onSuccess: () => {
      toast.success('Password reset instructions sent to your email');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to send reset instructions');
    },
  });
}

export function useResetPassword() {
  const router = useRouter();

  return useMutation({
    mutationFn: (data: { email: string; token: string; newPassword: string }) =>
      authService.resetPassword({ email: data.email, code: data.token, newPassword: data.newPassword }),
    onSuccess: () => {
      toast.success('Password reset successful! Please login with your new password.');
      router.push('/login');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to reset password');
    },
  });
}

export function useConfirmEmail() {
  const router = useRouter();

  return useMutation({
    mutationFn: (data: { userId: string; token: string }) =>
      authService.confirmEmail(data.userId, data.token),
    onSuccess: () => {
      toast.success('Email confirmed! You can now login.');
      router.push('/login');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to confirm email');
    },
  });
}

export function useResendConfirmation() {
  return useMutation({
    mutationFn: (email: string) => authService.resendConfirmation(email),
    onSuccess: () => {
      toast.success('Confirmation email sent!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to send confirmation email');
    },
  });
}
