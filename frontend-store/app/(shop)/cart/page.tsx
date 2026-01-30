'use client';

import Link from 'next/link';
import { ChevronRight, Loader2 } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import { CartItem, CartSummary, CartEmpty } from '@/components/cart';
import { useCart, useClearCart } from '@/features/cart/hooks/useCart';

export default function CartPage() {
  const { data: cart, isLoading } = useCart();
  const clearCart = useClearCart();

  if (isLoading) {
    return (
      <div className="container-custom py-8">
        <Skeleton className="mb-8 h-8 w-48" />
        <div className="flex flex-col gap-8 lg:flex-row">
          <div className="flex-1">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="flex gap-4 border-b py-4">
                <Skeleton className="h-24 w-24 rounded-lg" />
                <div className="flex-1">
                  <Skeleton className="h-5 w-48" />
                  <Skeleton className="mt-2 h-4 w-24" />
                  <Skeleton className="mt-4 h-8 w-32" />
                </div>
              </div>
            ))}
          </div>
          <div className="w-full lg:w-80">
            <Skeleton className="h-64 rounded-lg" />
          </div>
        </div>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="container-custom py-8">
        <nav className="mb-6 flex items-center gap-2 text-sm text-slate-600">
          <Link href="/" className="hover:text-slate-900">
            Home
          </Link>
          <ChevronRight className="h-4 w-4" />
          <span className="text-slate-900">Shopping Cart</span>
        </nav>
        <CartEmpty />
      </div>
    );
  }

  return (
    <div className="container-custom py-8">
      {/* Breadcrumbs */}
      <nav className="mb-6 flex items-center gap-2 text-sm text-slate-600">
        <Link href="/" className="hover:text-slate-900">
          Home
        </Link>
        <ChevronRight className="h-4 w-4" />
        <span className="text-slate-900">Shopping Cart</span>
      </nav>

      <div className="mb-8 flex items-center justify-between">
        <h1 className="text-3xl font-bold">Shopping Cart</h1>
        <button
          onClick={() => clearCart.mutate()}
          disabled={clearCart.isPending}
          className="text-sm text-slate-500 hover:text-slate-900 disabled:opacity-50"
        >
          {clearCart.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            'Clear Cart'
          )}
        </button>
      </div>

      <div className="flex flex-col gap-8 lg:flex-row">
        {/* Cart Items */}
        <div className="flex-1">
          <div className="rounded-lg border">
            <div className="p-4">
              {cart.items.map((item) => (
                <CartItem key={item.id} item={item} />
              ))}
            </div>
          </div>
        </div>

        {/* Order Summary */}
        <div className="w-full lg:w-80">
          <div className="sticky top-24">
            <CartSummary cart={cart} />
          </div>
        </div>
      </div>
    </div>
  );
}
