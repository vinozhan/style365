'use client';

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { Cart, CartItem } from '@/types';

interface CartState {
  cart: Cart | null;
  isLoading: boolean;
  isOpen: boolean;

  setCart: (cart: Cart | null) => void;
  setLoading: (loading: boolean) => void;
  openCart: () => void;
  closeCart: () => void;
  toggleCart: () => void;

  // Optimistic updates
  addItemOptimistic: (item: CartItem) => void;
  updateItemOptimistic: (itemId: string, quantity: number) => void;
  removeItemOptimistic: (itemId: string) => void;
  clearCartOptimistic: () => void;

  // Computed values
  totalItems: () => number;
  totalAmount: () => number;
}

export const useCartStore = create<CartState>()(
  persist(
    (set, get) => ({
      cart: null,
      isLoading: false,
      isOpen: false,

      setCart: (cart) => set({ cart }),
      setLoading: (isLoading) => set({ isLoading }),
      openCart: () => set({ isOpen: true }),
      closeCart: () => set({ isOpen: false }),
      toggleCart: () => set((state) => ({ isOpen: !state.isOpen })),

      addItemOptimistic: (item) =>
        set((state) => {
          if (!state.cart) {
            return {
              cart: {
                id: '',
                items: [item],
                totalItems: item.quantity,
                subTotal: item.subTotal,
                totalAmount: item.subTotal,
                currency: item.currency,
                lastModified: new Date().toISOString(),
              },
            };
          }

          const existingIndex = state.cart.items.findIndex(
            (i) => i.productId === item.productId && i.variantId === item.variantId
          );

          let newItems: CartItem[];
          if (existingIndex >= 0) {
            newItems = state.cart.items.map((i, idx) =>
              idx === existingIndex
                ? { ...i, quantity: i.quantity + item.quantity, subTotal: i.subTotal + item.subTotal }
                : i
            );
          } else {
            newItems = [...state.cart.items, item];
          }

          const totalItems = newItems.reduce((sum, i) => sum + i.quantity, 0);
          const totalAmount = newItems.reduce((sum, i) => sum + i.subTotal, 0);

          return {
            cart: {
              ...state.cart,
              items: newItems,
              totalItems,
              subTotal: totalAmount,
              totalAmount,
              lastModified: new Date().toISOString(),
            },
          };
        }),

      updateItemOptimistic: (itemId, quantity) =>
        set((state) => {
          if (!state.cart) return state;

          const newItems = state.cart.items.map((item) =>
            item.id === itemId
              ? { ...item, quantity, subTotal: item.unitPrice * quantity }
              : item
          );

          const totalItems = newItems.reduce((sum, i) => sum + i.quantity, 0);
          const totalAmount = newItems.reduce((sum, i) => sum + i.subTotal, 0);

          return {
            cart: {
              ...state.cart,
              items: newItems,
              totalItems,
              subTotal: totalAmount,
              totalAmount,
              lastModified: new Date().toISOString(),
            },
          };
        }),

      removeItemOptimistic: (itemId) =>
        set((state) => {
          if (!state.cart) return state;

          const newItems = state.cart.items.filter((item) => item.id !== itemId);
          const totalItems = newItems.reduce((sum, i) => sum + i.quantity, 0);
          const totalAmount = newItems.reduce((sum, i) => sum + i.subTotal, 0);

          return {
            cart: {
              ...state.cart,
              items: newItems,
              totalItems,
              subTotal: totalAmount,
              totalAmount,
              lastModified: new Date().toISOString(),
            },
          };
        }),

      clearCartOptimistic: () =>
        set((state) => ({
          cart: state.cart
            ? {
                ...state.cart,
                items: [],
                totalItems: 0,
                subTotal: 0,
                totalAmount: 0,
                lastModified: new Date().toISOString(),
              }
            : null,
        })),

      totalItems: () => get().cart?.totalItems ?? 0,
      totalAmount: () => get().cart?.totalAmount ?? 0,
    }),
    {
      name: 'cart-storage',
      partialize: (state) => ({
        cart: state.cart,
      }),
    }
  )
);
