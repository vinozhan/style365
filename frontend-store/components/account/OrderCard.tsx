import Link from 'next/link';
import Image from 'next/image';
import { ChevronRight } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import type { Order } from '@/types';
import { formatCurrency, formatDate } from '@/lib/utils';
import { cn } from '@/lib/utils';

interface OrderCardProps {
  order: Order;
}

const statusColors: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Processing: 'bg-blue-100 text-blue-800',
  Shipped: 'bg-purple-100 text-purple-800',
  Delivered: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
  Refunded: 'bg-slate-100 text-slate-800',
};

export function OrderCard({ order }: OrderCardProps) {
  const displayItems = order.items.slice(0, 3);
  const remainingCount = order.items.length - 3;

  return (
    <div className="rounded-lg border p-4">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-3">
            <p className="font-medium">Order #{order.orderNumber}</p>
            <Badge className={cn('rounded-full', statusColors[order.status] || 'bg-slate-100')}>
              {order.status}
            </Badge>
          </div>
          <p className="mt-1 text-sm text-slate-500">
            Placed on {formatDate(order.orderDate)}
          </p>
        </div>
        <p className="text-lg font-semibold">{formatCurrency(order.totalAmount)}</p>
      </div>

      {/* Order items preview */}
      <div className="mt-4 flex items-center gap-2">
        {displayItems.map((item) => (
          <div
            key={item.id}
            className="relative h-16 w-16 overflow-hidden rounded-lg bg-slate-100"
          >
            {item.productImageUrl ? (
              <Image
                src={item.productImageUrl}
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
        ))}
        {remainingCount > 0 && (
          <div className="flex h-16 w-16 items-center justify-center rounded-lg bg-slate-100 text-sm text-slate-600">
            +{remainingCount}
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="mt-4 flex items-center justify-between border-t pt-4">
        <p className="text-sm text-slate-500">
          {order.items.length} {order.items.length === 1 ? 'item' : 'items'}
        </p>
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/account/orders/${order.id}`}>
            View Details
            <ChevronRight className="ml-1 h-4 w-4" />
          </Link>
        </Button>
      </div>
    </div>
  );
}
