'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Link from 'next/link';
import Image from 'next/image';
import { Search, Loader2, Package, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { OrderTimeline } from '@/components/account';
import { useTrackOrder } from '@/features/orders/hooks/useOrders';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { Order } from '@/types';

const trackSchema = z.object({
  orderNumber: z.string().min(1, 'Order number is required'),
  email: z.string().email('Valid email required'),
});

type TrackFormData = z.infer<typeof trackSchema>;

export default function TrackOrderPage() {
  const [order, setOrder] = useState<Order | null>(null);
  const trackOrder = useTrackOrder();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TrackFormData>({
    resolver: zodResolver(trackSchema),
  });

  const onSubmit = async (data: TrackFormData) => {
    try {
      const result = await trackOrder.mutateAsync(data.orderNumber);
      setOrder(result);
    } catch {
      // Error handled by mutation
    }
  };

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Header */}
      <header className="border-b bg-white">
        <div className="container-custom flex h-16 items-center">
          <Link href="/" className="text-2xl font-bold">
            Style365
          </Link>
        </div>
      </header>

      <main className="container-custom py-12">
        <div className="mx-auto max-w-2xl">
          {order ? (
            // Order details view
            <div>
              <Button
                variant="ghost"
                className="mb-6"
                onClick={() => setOrder(null)}
              >
                <ArrowLeft className="mr-2 h-4 w-4" />
                Track Another Order
              </Button>

              <div className="rounded-lg border bg-white p-6">
                <div className="mb-6">
                  <h1 className="text-2xl font-bold">Order #{order.orderNumber}</h1>
                  <p className="text-slate-500">Placed on {formatDate(order.createdAt)}</p>
                </div>

                {/* Timeline */}
                <div className="mb-8">
                  <h2 className="mb-4 font-semibold">Order Status</h2>
                  <OrderTimeline
                    status={order.status}
                    createdAt={order.createdAt}
                    shippedAt={order.shippedAt}
                    deliveredAt={order.deliveredAt}
                  />
                </div>

                <Separator className="my-6" />

                {/* Order items */}
                <div>
                  <h2 className="mb-4 font-semibold">Order Items</h2>
                  <div className="space-y-4">
                    {order.items.map((item) => (
                      <div key={item.id} className="flex gap-4">
                        <div className="relative h-16 w-16 flex-shrink-0 overflow-hidden rounded bg-slate-100">
                          {item.productImage ? (
                            <Image
                              src={item.productImage}
                              alt={item.productName}
                              fill
                              className="object-cover"
                            />
                          ) : (
                            <div className="flex h-full items-center justify-center text-xs text-slate-400">
                              No img
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
                          <p className="font-medium">{formatCurrency(item.lineTotal)}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                <Separator className="my-6" />

                {/* Order summary */}
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
                  <Separator className="my-2" />
                  <div className="flex justify-between font-semibold">
                    <span>Total</span>
                    <span>{formatCurrency(order.totalAmount)}</span>
                  </div>
                </div>

                <Separator className="my-6" />

                {/* Shipping address */}
                <div>
                  <h2 className="mb-2 font-semibold">Shipping Address</h2>
                  <p className="text-sm text-slate-600">
                    {order.shippingAddress.firstName} {order.shippingAddress.lastName}
                    <br />
                    {order.shippingAddress.addressLine1}
                    <br />
                    {order.shippingAddress.addressLine2 && (
                      <>
                        {order.shippingAddress.addressLine2}
                        <br />
                      </>
                    )}
                    {order.shippingAddress.city}, {order.shippingAddress.stateProvince}{' '}
                    {order.shippingAddress.postalCode}
                    <br />
                    {order.shippingAddress.country}
                  </p>
                </div>
              </div>
            </div>
          ) : (
            // Search form
            <div className="rounded-lg border bg-white p-8">
              <div className="mb-6 text-center">
                <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-slate-100">
                  <Package className="h-8 w-8 text-slate-600" />
                </div>
                <h1 className="text-2xl font-bold">Track Your Order</h1>
                <p className="mt-2 text-slate-600">
                  Enter your order number and email to track your order status.
                </p>
              </div>

              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div>
                  <Label htmlFor="orderNumber">Order Number</Label>
                  <Input
                    id="orderNumber"
                    placeholder="e.g., ORD-123456"
                    {...register('orderNumber')}
                    className={errors.orderNumber ? 'border-red-500' : ''}
                  />
                  {errors.orderNumber && (
                    <p className="mt-1 text-xs text-red-500">{errors.orderNumber.message}</p>
                  )}
                </div>

                <div>
                  <Label htmlFor="email">Email Address</Label>
                  <Input
                    id="email"
                    type="email"
                    placeholder="Enter the email used for the order"
                    {...register('email')}
                    className={errors.email ? 'border-red-500' : ''}
                  />
                  {errors.email && (
                    <p className="mt-1 text-xs text-red-500">{errors.email.message}</p>
                  )}
                </div>

                <Button
                  type="submit"
                  className="w-full"
                  disabled={trackOrder.isPending}
                >
                  {trackOrder.isPending ? (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  ) : (
                    <Search className="mr-2 h-4 w-4" />
                  )}
                  Track Order
                </Button>

                {trackOrder.isError && (
                  <p className="text-center text-sm text-red-500">
                    Order not found. Please check your order number and email.
                  </p>
                )}
              </form>

              <div className="mt-6 text-center">
                <p className="text-sm text-slate-500">
                  Need help?{' '}
                  <Link href="/contact" className="text-slate-900 underline">
                    Contact Support
                  </Link>
                </p>
              </div>
            </div>
          )}
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t bg-white py-6">
        <div className="container-custom text-center text-sm text-slate-500">
          &copy; {new Date().getFullYear()} Style365. All rights reserved.
        </div>
      </footer>
    </div>
  );
}
