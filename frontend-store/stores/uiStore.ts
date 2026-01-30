'use client';

import { create } from 'zustand';
import type { Product } from '@/types';

interface UIState {
  // Mobile menu
  isMobileMenuOpen: boolean;
  openMobileMenu: () => void;
  closeMobileMenu: () => void;
  toggleMobileMenu: () => void;

  // Search
  isSearchOpen: boolean;
  searchQuery: string;
  openSearch: () => void;
  closeSearch: () => void;
  setSearchQuery: (query: string) => void;

  // Quick view modal
  quickViewProduct: Product | null;
  openQuickView: (product: Product) => void;
  closeQuickView: () => void;

  // Auth modal (for login prompt)
  isAuthModalOpen: boolean;
  authModalRedirect: string | null;
  openAuthModal: (redirect?: string) => void;
  closeAuthModal: () => void;

  // Toast/notifications handled by sonner, but we can add custom state if needed
}

export const useUIStore = create<UIState>((set) => ({
  // Mobile menu
  isMobileMenuOpen: false,
  openMobileMenu: () => set({ isMobileMenuOpen: true }),
  closeMobileMenu: () => set({ isMobileMenuOpen: false }),
  toggleMobileMenu: () => set((state) => ({ isMobileMenuOpen: !state.isMobileMenuOpen })),

  // Search
  isSearchOpen: false,
  searchQuery: '',
  openSearch: () => set({ isSearchOpen: true }),
  closeSearch: () => set({ isSearchOpen: false, searchQuery: '' }),
  setSearchQuery: (query) => set({ searchQuery: query }),

  // Quick view
  quickViewProduct: null,
  openQuickView: (product) => set({ quickViewProduct: product }),
  closeQuickView: () => set({ quickViewProduct: null }),

  // Auth modal
  isAuthModalOpen: false,
  authModalRedirect: null,
  openAuthModal: (redirect) => set({ isAuthModalOpen: true, authModalRedirect: redirect ?? null }),
  closeAuthModal: () => set({ isAuthModalOpen: false, authModalRedirect: null }),
}));
