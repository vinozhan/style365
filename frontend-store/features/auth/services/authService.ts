import { apiClient } from '@/lib/api/client';
import type { LoginApiResponse, User, mapUserFromApi } from '@/types';

export interface RegisterInput {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginInput {
  email: string;
  password: string;
}

export interface ChangePasswordInput {
  currentPassword: string;
  newPassword: string;
}

export interface ResetPasswordInput {
  email: string;
  code: string;
  newPassword: string;
}

export const authService = {
  async register(data: RegisterInput): Promise<{ message: string; requiresEmailConfirmation: boolean }> {
    const response = await apiClient.post('/auth/register', data);
    return response.data;
  },

  async login(data: LoginInput): Promise<LoginApiResponse> {
    const response = await apiClient.post<LoginApiResponse>('/auth/login', data);
    return response.data;
  },

  async getProfile(): Promise<User> {
    const response = await apiClient.get('/auth/profile');
    return response.data;
  },

  async confirmEmail(email: string, code: string): Promise<{ message: string }> {
    const response = await apiClient.post('/auth/confirm-email', { email, code });
    return response.data;
  },

  async resendConfirmation(email: string): Promise<{ message: string }> {
    const response = await apiClient.post('/auth/resend-confirmation', { email });
    return response.data;
  },

  async forgotPassword(email: string): Promise<{ message: string }> {
    const response = await apiClient.post('/auth/forgot-password', { email });
    return response.data;
  },

  async resetPassword(data: ResetPasswordInput): Promise<{ message: string }> {
    const response = await apiClient.post('/auth/reset-password', data);
    return response.data;
  },

  async changePassword(data: ChangePasswordInput): Promise<{ message: string }> {
    const response = await apiClient.post('/auth/change-password', data);
    return response.data;
  },

  async refreshToken(refreshToken: string): Promise<{ accessToken: string; refreshToken?: string }> {
    const response = await apiClient.post('/auth/refresh-token', { refreshToken });
    return response.data;
  },

  async logout(): Promise<void> {
    await apiClient.post('/auth/logout');
  },
};
