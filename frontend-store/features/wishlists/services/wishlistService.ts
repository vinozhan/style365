import { apiClient } from '@/lib/api/client';
import type { WishlistStatus } from '@/types';

export interface WishlistItemDto {
  id: string;
  productId: string;
  productName: string;
  productSlug: string;
  productPrice: number;
  productImage?: string;
  isInStock: boolean;
  addedAt: string;
}

export interface Wishlist {
  id: string;
  name: string;
  isDefault: boolean;
  isPublic: boolean;
  itemCount: number;
  createdAt: string;
  items: WishlistItemDto[];
}

export const wishlistService = {
  async getWishlists(): Promise<Wishlist[]> {
    const response = await apiClient.get<Wishlist[]>('/wishlists');
    return response.data;
  },

  async addToWishlist(productId: string): Promise<void> {
    await apiClient.post('/wishlists/items', { productId });
  },

  async removeFromWishlist(productId: string): Promise<void> {
    await apiClient.delete(`/wishlists/items/${productId}`);
  },

  async checkWishlistStatus(productId: string): Promise<WishlistStatus> {
    const response = await apiClient.get<WishlistStatus>(`/wishlists/items/${productId}/status`);
    return response.data;
  },
};
