import { apiClient } from '@/lib/api/client';
import type { Review, ReviewStats, PaginatedResponse } from '@/types';

export interface CreateReviewInput {
  rating: number;
  title: string;
  comment: string;
}

export interface GetReviewsParams {
  page?: number;
  pageSize?: number;
  sortBy?: 'rating' | 'created' | 'helpful';
  ascending?: boolean;
}

export const reviewService = {
  async getProductReviews(
    productId: string,
    params: GetReviewsParams = {}
  ): Promise<PaginatedResponse<Review>> {
    const response = await apiClient.get<PaginatedResponse<Review>>(
      `/products/${productId}/reviews`,
      {
        params: {
          page: params.page ?? 1,
          pageSize: params.pageSize ?? 10,
          sortBy: params.sortBy ?? 'created',
          ascending: params.ascending ?? false,
        },
      }
    );
    return response.data;
  },

  async getReviewStats(productId: string): Promise<ReviewStats> {
    const response = await apiClient.get<ReviewStats>(`/products/${productId}/reviews/stats`);
    return response.data;
  },

  async createReview(productId: string, data: CreateReviewInput): Promise<Review> {
    const response = await apiClient.post<Review>(`/products/${productId}/reviews`, data);
    return response.data;
  },

  async getUserReviews(): Promise<Review[]> {
    const response = await apiClient.get<Review[]>('/users/reviews');
    return response.data;
  },
};
