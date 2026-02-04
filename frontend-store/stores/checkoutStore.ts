'use client';

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { OrderAddress, PaymentMethod } from '@/types';

export type CheckoutStep = 'shipping' | 'payment' | 'review';

interface CheckoutState {
  currentStep: CheckoutStep;
  shippingAddress: OrderAddress | null;
  billingAddress: OrderAddress | null;
  useSameBillingAddress: boolean;
  shippingMethod: string | null;
  shippingCost: number;
  paymentMethod: PaymentMethod | null;
  notes: string;

  // Actions
  setStep: (step: CheckoutStep) => void;
  nextStep: () => void;
  prevStep: () => void;
  setShippingAddress: (address: OrderAddress | null) => void;
  setBillingAddress: (address: OrderAddress | null) => void;
  setUseSameBillingAddress: (same: boolean) => void;
  setShippingMethod: (method: string, cost: number) => void;
  setPaymentMethod: (method: PaymentMethod) => void;
  setNotes: (notes: string) => void;
  reset: () => void;

  // Computed
  canProceedToPayment: () => boolean;
  canProceedToReview: () => boolean;
  getBillingAddress: () => OrderAddress | null;
}

const STEP_ORDER: CheckoutStep[] = ['shipping', 'payment', 'review'];

const initialState = {
  currentStep: 'shipping' as CheckoutStep,
  shippingAddress: null,
  billingAddress: null,
  useSameBillingAddress: true,
  shippingMethod: null,
  shippingCost: 0,
  paymentMethod: null,
  notes: '',
};

export const useCheckoutStore = create<CheckoutState>()(
  persist(
    (set, get) => ({
      ...initialState,

      setStep: (step) => set({ currentStep: step }),

      nextStep: () => {
        const currentIndex = STEP_ORDER.indexOf(get().currentStep);
        if (currentIndex < STEP_ORDER.length - 1) {
          set({ currentStep: STEP_ORDER[currentIndex + 1] });
        }
      },

      prevStep: () => {
        const currentIndex = STEP_ORDER.indexOf(get().currentStep);
        if (currentIndex > 0) {
          set({ currentStep: STEP_ORDER[currentIndex - 1] });
        }
      },

      setShippingAddress: (address) => set({ shippingAddress: address }),

      setBillingAddress: (address) => set({ billingAddress: address }),

      setUseSameBillingAddress: (same) => set({ useSameBillingAddress: same }),

      setShippingMethod: (method, cost) =>
        set({ shippingMethod: method, shippingCost: cost }),

      setPaymentMethod: (method) => set({ paymentMethod: method }),

      setNotes: (notes) => set({ notes }),

      reset: () => set(initialState),

      canProceedToPayment: () => {
        const state = get();
        return (
          state.shippingAddress !== null &&
          state.shippingMethod !== null
        );
      },

      canProceedToReview: () => {
        const state = get();
        return (
          state.shippingAddress !== null &&
          state.shippingMethod !== null &&
          state.paymentMethod !== null
        );
      },

      getBillingAddress: () => {
        const state = get();
        return state.useSameBillingAddress
          ? state.shippingAddress
          : state.billingAddress;
      },
    }),
    {
      name: 'checkout-storage',
      partialize: (state) => ({
        shippingAddress: state.shippingAddress,
        billingAddress: state.billingAddress,
        useSameBillingAddress: state.useSameBillingAddress,
        shippingMethod: state.shippingMethod,
        shippingCost: state.shippingCost,
        paymentMethod: state.paymentMethod,
        notes: state.notes,
      }),
    }
  )
);
