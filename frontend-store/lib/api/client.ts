import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type { ApiError } from '@/types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001';

// Helper to get session ID for guest carts
function getSessionId(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('sessionId');
}

// Helper to generate a new session ID
export function generateSessionId(): string {
  const sessionId = `guest_${Date.now()}_${Math.random().toString(36).substring(2, 15)}`;
  if (typeof window !== 'undefined') {
    localStorage.setItem('sessionId', sessionId);
  }
  return sessionId;
}

// Helper to get or create session ID
export function getOrCreateSessionId(): string {
  const existing = getSessionId();
  if (existing) return existing;
  return generateSessionId();
}

// Helper to clear session ID (after cart merge)
export function clearSessionId(): void {
  if (typeof window !== 'undefined') {
    localStorage.removeItem('sessionId');
  }
}

// Helper to clear all auth state
function clearAuthState(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('auth-storage');
}

// Create axios instance
export const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Request interceptor to attach JWT token and session ID
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    if (typeof window !== 'undefined') {
      // Attach JWT token if available
      const token = localStorage.getItem('accessToken');
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }

      // Attach session ID for cart operations (for guest users)
      const sessionId = getSessionId();
      if (sessionId && config.headers) {
        config.headers['X-Session-Id'] = sessionId;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor to handle token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiError>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // If 401 and we haven't retried yet, try to refresh token
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      if (typeof window !== 'undefined') {
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
          try {
            const response = await axios.post(`${API_BASE_URL}/api/auth/refresh-token`, {
              refreshToken,
            });

            const { accessToken, refreshToken: newRefreshToken } = response.data;
            localStorage.setItem('accessToken', accessToken);
            if (newRefreshToken) {
              localStorage.setItem('refreshToken', newRefreshToken);
            }

            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${accessToken}`;
            }

            return apiClient(originalRequest);
          } catch {
            // Refresh failed, clear auth state
            clearAuthState();
            // Don't redirect on storefront - let the app handle it
            return Promise.reject(error);
          }
        }
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
