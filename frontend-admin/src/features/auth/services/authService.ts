import apiClient from '@/lib/api/client';
import type { User, LoginResponse } from '@/types';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  },

  async logout(): Promise<void> {
    await apiClient.post('/auth/logout');
  },

  async refreshToken(refreshToken: string): Promise<{ accessToken: string; refreshToken: string }> {
    const response = await apiClient.post<{ accessToken: string; refreshToken: string }>('/auth/refresh-token', {
      refreshToken,
    });
    return response.data;
  },

  async getProfile(): Promise<User> {
    const response = await apiClient.get<User>('/auth/profile');
    return response.data;
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await apiClient.post('/auth/change-password', { currentPassword, newPassword });
  },
};
