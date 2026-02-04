'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ChevronRight, ShieldCheck } from 'lucide-react';
import { toast } from 'sonner';
import { CheckoutSteps, ShippingForm, PaymentMethods, OrderReview } from '@/components/checkout';
import type { ShippingFormData } from '@/components/checkout';
import { CartSummary } from '@/components/cart';
import { Skeleton } from '@/components/ui/skeleton';
import { useCart } from '@/features/cart/hooks/useCart';
import { useCreateOrder } from '@/features/orders/hooks/useOrders';
import { useCheckoutStore } from '@/stores/checkoutStore';
import { useAuthStore } from '@/stores/authStore';
import type { PaymentMethod } from '@/types';

type CheckoutStep = 'shipping' | 'payment' | 'review';

export default function CheckoutPage() {
  const router = useRouter();
  const { data: cart, isLoading: cartLoading } = useCart();
  const createOrder = useCreateOrder();
  const { user } = useAuthStore();
  const {
    shippingAddress,
    paymentMethod,
    setShippingAddress,
    setPaymentMethod,
    reset: clearCheckout,
  } = useCheckoutStore();

  const [currentStep, setCurrentStep] = useState<CheckoutStep>('shipping');

  // Redirect if cart is empty
  useEffect(() => {
    if (!cartLoading && (!cart || cart.items.length === 0)) {
      router.push('/cart');
    }
  }, [cart, cartLoading, router]);

  const handleShippingSubmit = (data: ShippingFormData) => {
    // Transform ShippingFormData to OrderAddress
    setShippingAddress({
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      phone: data.phone,
      addressLine1: data.addressLine1,
      addressLine2: data.addressLine2,
      city: data.city,
      stateProvince: data.stateProvince,
      postalCode: data.postalCode,
      country: data.country,
    });
    setCurrentStep('payment');
  };

  const handlePaymentSubmit = (method: PaymentMethod) => {
    setPaymentMethod(method);
    setCurrentStep('review');
  };

  const handlePlaceOrder = async () => {
    if (!shippingAddress || !paymentMethod || !cart) return;

    try {
      const result = await createOrder.mutateAsync({
        shippingAddress: {
          firstName: shippingAddress.firstName,
          lastName: shippingAddress.lastName,
          phone: shippingAddress.phone,
          addressLine1: shippingAddress.addressLine1,
          addressLine2: shippingAddress.addressLine2,
          city: shippingAddress.city,
          stateProvince: shippingAddress.stateProvince,
          postalCode: shippingAddress.postalCode,
          country: shippingAddress.country,
        },
        billingAddress: {
          firstName: shippingAddress.firstName,
          lastName: shippingAddress.lastName,
          phone: shippingAddress.phone,
          addressLine1: shippingAddress.addressLine1,
          addressLine2: shippingAddress.addressLine2,
          city: shippingAddress.city,
          stateProvince: shippingAddress.stateProvince,
          postalCode: shippingAddress.postalCode,
          country: shippingAddress.country,
        },
        paymentMethod,
        notes: '',
      });

      clearCheckout();
      toast.success('Order placed successfully!');
      router.push(`/account/orders/${result.id}?success=true`);
    } catch {
      toast.error('Failed to place order. Please try again.');
    }
  };

  if (cartLoading) {
    return (
      <div className="container-custom py-8">
        <Skeleton className="mb-8 h-8 w-48" />
        <div className="flex flex-col gap-8 lg:flex-row">
          <div className="flex-1">
            <Skeleton className="h-96 rounded-lg" />
          </div>
          <div className="w-full lg:w-80">
            <Skeleton className="h-64 rounded-lg" />
          </div>
        </div>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return null; // Will redirect
  }

  return (
    <div className="container-custom py-8">
      {/* Breadcrumbs */}
      <nav className="mb-6 flex items-center gap-2 text-sm text-slate-600">
        <Link href="/" className="hover:text-slate-900">
          Home
        </Link>
        <ChevronRight className="h-4 w-4" />
        <Link href="/cart" className="hover:text-slate-900">
          Cart
        </Link>
        <ChevronRight className="h-4 w-4" />
        <span className="text-slate-900">Checkout</span>
      </nav>

      <h1 className="mb-8 text-3xl font-bold">Checkout</h1>

      {/* Checkout Steps */}
      <div className="mb-8">
        <CheckoutSteps currentStep={currentStep} />
      </div>

      <div className="flex flex-col gap-8 lg:flex-row">
        {/* Main Content */}
        <div className="flex-1">
          <div className="rounded-lg border p-6">
            {currentStep === 'shipping' && (
              <>
                <h2 className="mb-6 text-xl font-semibold">Shipping Information</h2>
                <ShippingForm onSubmit={handleShippingSubmit} />
              </>
            )}

            {currentStep === 'payment' && (
              <>
                <h2 className="mb-6 text-xl font-semibold">Payment Method</h2>
                <PaymentMethods
                  onSubmit={handlePaymentSubmit}
                  onBack={() => setCurrentStep('shipping')}
                />
              </>
            )}

            {currentStep === 'review' && (
              <>
                <h2 className="mb-6 text-xl font-semibold">Review Your Order</h2>
                <OrderReview
                  onSubmit={handlePlaceOrder}
                  onEditShipping={() => setCurrentStep('shipping')}
                  onEditPayment={() => setCurrentStep('payment')}
                  isLoading={createOrder.isPending}
                />
              </>
            )}
          </div>

          {/* Security badge */}
          <div className="mt-4 flex items-center justify-center gap-2 text-sm text-slate-500">
            <ShieldCheck className="h-4 w-4" />
            <span>Secure checkout - Your data is protected</span>
          </div>
        </div>

        {/* Order Summary Sidebar */}
        <div className="w-full lg:w-80">
          <div className="sticky top-24">
            <CartSummary cart={cart} showCheckoutButton={false} />
          </div>
        </div>
      </div>
    </div>
  );
}
