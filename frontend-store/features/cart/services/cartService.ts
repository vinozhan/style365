import { apiClient, getOrCreateSessionId } from '@/lib/api/client';
import type { Cart, CartSummary } from '@/types';

export interface AddToCartInput {
  productId: string;
  quantity: number;
  variantId?: string;
}

export interface UpdateCartItemInput {
  quantity: number;
}

export const cartService = {
  async getCart(): Promise<Cart> {
    const sessionId = getOrCreateSessionId();
    const response = await apiClient.get<Cart>('/cart', {
      params: { sessionId },
    });
    return response.data;
  },

  async getCartSummary(): Promise<CartSummary> {
    const sessionId = getOrCreateSessionId();
    const response = await apiClient.get<CartSummary>('/cart/summary', {
      params: { sessionId },
    });
    return response.data;
  },

  async addToCart(data: AddToCartInput): Promise<Cart> {
    getOrCreateSessionId(); // Ensure session exists
    const response = await apiClient.post<Cart>('/cart/items', data);
    return response.data;
  },

  async updateCartItem(itemId: string, quantity: number): Promise<void> {
    await apiClient.put(`/cart/items/${itemId}`, { quantity });
  },

  async removeCartItem(itemId: string): Promise<void> {
    await apiClient.delete(`/cart/items/${itemId}`);
  },

  async clearCart(): Promise<void> {
    const sessionId = getOrCreateSessionId();
    await apiClient.delete('/cart', { params: { sessionId } });
  },

  async mergeGuestCart(sessionId: string): Promise<Cart> {
    const response = await apiClient.post<Cart>('/cart/merge', { sessionId });
    return response.data;
  },
};
