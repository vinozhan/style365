'use client';

import { useParams, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import Image from 'next/image';
import { ArrowLeft, CheckCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { Skeleton } from '@/components/ui/skeleton';
import { OrderTimeline, AddressCard } from '@/components/account';
import { useOrderById } from '@/features/orders/hooks/useOrders';
import { formatCurrency, formatDate } from '@/lib/utils';

export default function OrderDetailPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const orderId = params.id as string;
  const isNewOrder = searchParams.get('success') === 'true';

  const { data: order, isLoading } = useOrderById(orderId);

  if (isLoading) {
    return (
      <div>
        <Skeleton className="mb-6 h-8 w-48" />
        <Skeleton className="mb-6 h-24 rounded-lg" />
        <Skeleton className="h-64 rounded-lg" />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="text-center py-12">
        <h1 className="text-xl font-semibold">Order not found</h1>
        <p className="mt-2 text-slate-500">The order you're looking for doesn't exist.</p>
        <Button className="mt-6" asChild>
          <Link href="/account/orders">Back to Orders</Link>
        </Button>
      </div>
    );
  }

  return (
    <div>
      {/* Success message for new orders */}
      {isNewOrder && (
        <div className="mb-6 flex items-center gap-3 rounded-lg bg-green-50 p-4">
          <CheckCircle className="h-5 w-5 text-green-600" />
          <div>
            <p className="font-medium text-green-800">Order placed successfully!</p>
            <p className="text-sm text-green-600">
              Thank you for your purchase. You will receive a confirmation email shortly.
            </p>
          </div>
        </div>
      )}

      {/* Header */}
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/account/orders">
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold">Order #{order.orderNumber}</h1>
          <p className="text-sm text-slate-500">Placed on {formatDate(order.orderDate)}</p>
        </div>
      </div>

      {/* Order timeline */}
      <div className="mb-8 rounded-lg border p-6">
        <h2 className="mb-4 font-semibold">Order Status</h2>
        <OrderTimeline
          status={order.status}
          orderDate={order.orderDate}
          shippedDate={order.shippedDate}
          deliveredDate={order.deliveredDate}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Order items */}
        <div className="lg:col-span-2">
          <div className="rounded-lg border">
            <div className="border-b p-4">
              <h2 className="font-semibold">Order Items ({order.items.length})</h2>
            </div>
            <div className="divide-y">
              {order.items.map((item) => (
                <div key={item.id} className="flex gap-4 p-4">
                  <div className="relative h-20 w-20 flex-shrink-0 overflow-hidden rounded-lg bg-slate-100">
                    {item.productImageUrl ? (
                      <Image
                        src={item.productImageUrl}
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
        </div>

        {/* Order summary */}
        <div className="space-y-6">
          {/* Summary */}
          <div className="rounded-lg border p-4">
            <h2 className="mb-4 font-semibold">Order Summary</h2>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-slate-600">Subtotal</span>
                <span>{formatCurrency(order.subtotal)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-600">Shipping</span>
                <span>{formatCurrency(order.shippingAmount)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-600">Tax</span>
                <span>{formatCurrency(order.taxAmount)}</span>
              </div>
              {order.discountAmount > 0 && (
                <div className="flex justify-between text-green-600">
                  <span>Discount</span>
                  <span>-{formatCurrency(order.discountAmount)}</span>
                </div>
              )}
              <Separator className="my-2" />
              <div className="flex justify-between font-semibold">
                <span>Total</span>
                <span>{formatCurrency(order.totalAmount)}</span>
              </div>
            </div>
          </div>

          {/* Payment info */}
          {order.payment && (
            <div className="rounded-lg border p-4">
              <h2 className="mb-4 font-semibold">Payment</h2>
              <p className="text-sm text-slate-600">{order.payment.paymentMethod}</p>
              <p className="text-sm text-slate-500">Status: {order.payment.status}</p>
            </div>
          )}

          {/* Shipping address */}
          <div className="rounded-lg border p-4">
            <h2 className="mb-4 font-semibold">Shipping Address</h2>
            <AddressCard address={order.shippingAddress} showActions={false} />
          </div>
        </div>
      </div>

      {/* Notes */}
      {order.notes && (
        <div className="mt-6 rounded-lg border p-4">
          <h2 className="mb-2 font-semibold">Order Notes</h2>
          <p className="text-sm text-slate-600">{order.notes}</p>
        </div>
      )}
    </div>
  );
}
