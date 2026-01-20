import apiClient from '@/lib/api/client';
import type { User, LoginResponse, LoginApiResponse, UserRoleNumber, UserRoleString } from '@/types';

export interface LoginCredentials {
  email: string;
  password: string;
}

// Map role number to string
const roleNumberToString: Record<UserRoleNumber, UserRoleString> = {
  0: 'Customer',
  1: 'Admin',
  2: 'ContentManager',
  3: 'SuperAdmin',
};

// Transform API response to frontend format
function transformLoginResponse(apiResponse: LoginApiResponse): LoginResponse {
  return {
    user: {
      id: apiResponse.user.id,
      email: typeof apiResponse.user.email === 'object' ? apiResponse.user.email.value : apiResponse.user.email,
      firstName: apiResponse.user.firstName,
      lastName: apiResponse.user.lastName,
      role: roleNumberToString[apiResponse.user.role] || 'Customer',
      createdAt: apiResponse.user.createdAt,
      updatedAt: apiResponse.user.updatedAt,
    },
    tokens: {
      accessToken: apiResponse.accessToken,
      refreshToken: apiResponse.refreshToken,
      expiresIn: apiResponse.expiresIn,
    },
  };
}

export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    const response = await apiClient.post<LoginApiResponse>('/auth/login', credentials);
    return transformLoginResponse(response.data);
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
