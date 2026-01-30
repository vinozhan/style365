'use client';

import Image from 'next/image';
import { CreditCard, Banknote, Building2, MapPin, Edit2, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { useCheckoutStore } from '@/stores/checkoutStore';
import { useCart } from '@/features/cart/hooks/useCart';
import { formatCurrency } from '@/lib/utils';
import type { PaymentMethod } from '@/types';

const paymentIcons: Record<PaymentMethod, React.ComponentType<{ className?: string }>> = {
  CreditCard: CreditCard,
  CashOnDelivery: Banknote,
  BankTransfer: Building2,
  PayHere: CreditCard,
};

const paymentNames: Record<PaymentMethod, string> = {
  CreditCard: 'Credit/Debit Card',
  CashOnDelivery: 'Cash on Delivery',
  BankTransfer: 'Bank Transfer',
  PayHere: 'PayHere',
};

interface OrderReviewProps {
  onSubmit: () => void;
  onEditShipping: () => void;
  onEditPayment: () => void;
  isLoading?: boolean;
}

export function OrderReview({
  onSubmit,
  onEditShipping,
  onEditPayment,
  isLoading,
}: OrderReviewProps) {
  const { shippingAddress, paymentMethod } = useCheckoutStore();
  const { data: cart } = useCart();

  if (!shippingAddress || !paymentMethod || !cart) {
    return null;
  }

  const PaymentIcon = paymentIcons[paymentMethod];
  const subtotal = cart.totalAmount;
  const shipping = 0; // Calculate based on shipping method
  const tax = 0; // Calculate based on region
  const total = subtotal + shipping + tax;

  return (
    <div className="space-y-6">
      {/* Shipping Address */}
      <div className="rounded-lg border p-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <MapPin className="h-5 w-5 text-slate-600" />
            <h3 className="font-medium">Shipping Address</h3>
          </div>
          <Button variant="ghost" size="sm" onClick={onEditShipping}>
            <Edit2 className="mr-1 h-4 w-4" />
            Edit
          </Button>
        </div>
        <div className="mt-3 text-sm text-slate-600">
          <p className="font-medium text-slate-900">
            {shippingAddress.firstName} {shippingAddress.lastName}
          </p>
          <p>{shippingAddress.addressLine1}</p>
          {shippingAddress.addressLine2 && <p>{shippingAddress.addressLine2}</p>}
          <p>
            {shippingAddress.city}, {shippingAddress.state} {shippingAddress.postalCode}
          </p>
          <p>{shippingAddress.country}</p>
          <p className="mt-2">{shippingAddress.phone}</p>
          <p>{shippingAddress.email}</p>
        </div>
      </div>

      {/* Payment Method */}
      <div className="rounded-lg border p-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <PaymentIcon className="h-5 w-5 text-slate-600" />
            <h3 className="font-medium">Payment Method</h3>
          </div>
          <Button variant="ghost" size="sm" onClick={onEditPayment}>
            <Edit2 className="mr-1 h-4 w-4" />
            Edit
          </Button>
        </div>
        <p className="mt-3 text-sm text-slate-600">{paymentNames[paymentMethod]}</p>
      </div>

      {/* Order Items */}
      <div className="rounded-lg border p-4">
        <h3 className="font-medium">Order Items ({cart.totalItems})</h3>
        <div className="mt-4 space-y-4">
          {cart.items.map((item) => (
            <div key={item.id} className="flex gap-4">
              <div className="relative h-16 w-16 flex-shrink-0 overflow-hidden rounded bg-slate-100">
                {item.imageUrl ? (
                  <Image
                    src={item.imageUrl}
                    alt={item.productName}
                    fill
                    className="object-cover"
                  />
                ) : (
                  <div className="flex h-full items-center justify-center text-xs text-slate-400">
                    No image
                  </div>
                )}
              </div>
              <div className="flex flex-1 justify-between">
                <div>
                  <p className="font-medium">{item.productName}</p>
                  {item.variantName && (
                    <p className="text-sm text-slate-500">{item.variantName}</p>
                  )}
                  <p className="text-sm text-slate-500">Qty: {item.quantity}</p>
                </div>
                <p className="font-medium">{formatCurrency(item.totalPrice)}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Order Summary */}
      <div className="rounded-lg border bg-slate-50 p-4">
        <h3 className="font-medium">Order Summary</h3>
        <div className="mt-4 space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-slate-600">Subtotal</span>
            <span>{formatCurrency(subtotal)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-slate-600">Shipping</span>
            <span>{shipping === 0 ? 'Free' : formatCurrency(shipping)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-slate-600">Tax</span>
            <span>{formatCurrency(tax)}</span>
          </div>
          <Separator className="my-2" />
          <div className="flex justify-between font-semibold">
            <span>Total</span>
            <span>{formatCurrency(total)}</span>
          </div>
        </div>
      </div>

      {/* Place Order Button */}
      <Button
        className="w-full"
        size="lg"
        onClick={onSubmit}
        disabled={isLoading}
      >
        {isLoading ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Placing Order...
          </>
        ) : (
          `Place Order - ${formatCurrency(total)}`
        )}
      </Button>

      <p className="text-center text-xs text-slate-500">
        By placing this order, you agree to our Terms of Service and Privacy Policy.
      </p>
    </div>
  );
}
