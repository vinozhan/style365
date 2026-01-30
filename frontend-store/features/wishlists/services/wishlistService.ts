import { apiClient } from '@/lib/api/client';
import type { WishlistStatus, Product } from '@/types';

export interface WishlistItem {
  id: string;
  productId: string;
  product: Product;
  addedAt: string;
}

export interface Wishlist {
  id: string;
  name: string;
  items: WishlistItem[];
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
