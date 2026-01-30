'use client';

import Link from 'next/link';
import { ShoppingBag, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import type { Cart } from '@/types';
import { formatCurrency } from '@/lib/utils';

interface CartSummaryProps {
  cart: Cart;
  showCheckoutButton?: boolean;
  isCheckingOut?: boolean;
  onCheckout?: () => void;
}

export function CartSummary({
  cart,
  showCheckoutButton = true,
  isCheckingOut = false,
  onCheckout,
}: CartSummaryProps) {
  const subtotal = cart.totalAmount;
  const shipping = 0; // Calculate based on shipping method
  const tax = 0; // Calculate based on region
  const total = subtotal + shipping + tax;

  return (
    <div className="rounded-lg border bg-slate-50 p-6">
      <h2 className="text-lg font-semibold">Order Summary</h2>

      <div className="mt-4 space-y-3">
        <div className="flex justify-between text-sm">
          <span className="text-slate-600">Subtotal ({cart.totalItems} items)</span>
          <span>{formatCurrency(subtotal)}</span>
        </div>

        <div className="flex justify-between text-sm">
          <span className="text-slate-600">Shipping</span>
          <span>{shipping === 0 ? 'Calculated at checkout' : formatCurrency(shipping)}</span>
        </div>

        <div className="flex justify-between text-sm">
          <span className="text-slate-600">Tax</span>
          <span>{tax === 0 ? 'Calculated at checkout' : formatCurrency(tax)}</span>
        </div>

        <Separator />

        <div className="flex justify-between font-semibold">
          <span>Total</span>
          <span>{formatCurrency(total)}</span>
        </div>
      </div>

      {showCheckoutButton && (
        <div className="mt-6 space-y-3">
          {onCheckout ? (
            <Button
              className="w-full"
              size="lg"
              onClick={onCheckout}
              disabled={isCheckingOut || cart.items.length === 0}
            >
              {isCheckingOut ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <ShoppingBag className="mr-2 h-4 w-4" />
              )}
              Proceed to Checkout
            </Button>
          ) : (
            <Button
              className="w-full"
              size="lg"
              asChild
              disabled={cart.items.length === 0}
            >
              <Link href="/checkout">
                <ShoppingBag className="mr-2 h-4 w-4" />
                Proceed to Checkout
              </Link>
            </Button>
          )}

          <Button variant="outline" className="w-full" asChild>
            <Link href="/products">Continue Shopping</Link>
          </Button>
        </div>
      )}

      {/* Promo code section */}
      <div className="mt-6">
        <p className="text-sm text-slate-600">
          Have a promo code? You can apply it at checkout.
        </p>
      </div>
    </div>
  );
}
