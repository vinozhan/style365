import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { authService, type LoginCredentials } from '../services/authService';
import { useAuthStore } from '@/stores/authStore';

export function useLogin() {
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();

  return useMutation({
    mutationFn: (credentials: LoginCredentials) => authService.login(credentials),
    onSuccess: (data) => {
      const allowedRoles = ['Admin', 'SuperAdmin', 'ContentManager'];
      if (!allowedRoles.includes(data.user.role)) {
        toast.error('Access denied. Admin privileges required.');
        return;
      }
      setAuth(data.user, data.tokens.accessToken, data.tokens.refreshToken);
      toast.success('Welcome back!');
      navigate('/dashboard');
    },
    onError: (error: Error & { response?: { data?: { message?: string } } }) => {
      const message = error.response?.data?.message || 'Invalid email or password';
      toast.error(message);
    },
  });
}

export function useLogout() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { logout } = useAuthStore();

  return useMutation({
    mutationFn: () => authService.logout(),
    onSuccess: () => {
      logout();
      queryClient.clear();
      navigate('/login');
      toast.success('Logged out successfully');
    },
    onError: () => {
      // Even if API fails, log out locally
      logout();
      queryClient.clear();
      navigate('/login');
    },
  });
}

export function useProfile() {
  const { isAuthenticated, setUser, setLoading } = useAuthStore();

  return useQuery({
    queryKey: ['profile'],
    queryFn: async () => {
      try {
        const user = await authService.getProfile();
        setUser(user);
        return user;
      } finally {
        setLoading(false);
      }
    },
    enabled: isAuthenticated,
    retry: false,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
