import { apiClient } from '@/lib/api/client';
import type { UserProfile, OrderAddress } from '@/types';

export interface UpdateProfileInput {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  bio?: string;
}

export const profileService = {
  async getProfile(): Promise<UserProfile> {
    const response = await apiClient.get<UserProfile>('/user-profiles/me');
    return response.data;
  },

  async updateProfile(data: UpdateProfileInput): Promise<UserProfile> {
    const response = await apiClient.put<UserProfile>('/user-profiles/me', data);
    return response.data;
  },

  async addAddress(address: OrderAddress): Promise<UserProfile> {
    const response = await apiClient.post<UserProfile>('/user-profiles/me/addresses', address);
    return response.data;
  },

  async updateAddress(index: number, address: OrderAddress): Promise<UserProfile> {
    const response = await apiClient.put<UserProfile>(`/user-profiles/me/addresses/${index}`, address);
    return response.data;
  },

  async deleteAddress(index: number): Promise<UserProfile> {
    const response = await apiClient.delete<UserProfile>(`/user-profiles/me/addresses/${index}`);
    return response.data;
  },
};
